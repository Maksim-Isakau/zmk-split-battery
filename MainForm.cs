using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage.Streams;

// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getlastinputinfo
// 
// TODO:
//  - not connected icon
//  - statuses
//  -
//

namespace ZMKSplit
{
    public partial class MainForm : Form
    {
        public static readonly Guid BATTERY_UUID = Guid.Parse("{0000180F-0000-1000-8000-00805F9B34FB}");
        public static readonly Guid BATTERY_LEVEL_UUID = Guid.Parse("{00002A19-0000-1000-8000-00805F9B34FB}");

        public static readonly String ZMK_CENTRAL_HALF_NAME = "Central";
        public static readonly String ZMK_PERIPHERAL_HALF_NAME = "Peripheral";

        public static readonly String DEVICE_TYPE_ZMK = "ZMK Split Keyboard";
        public static readonly String DEVICE_TYPE_GENERIC = "Generic BLE device";

        public static readonly String STATUS_LOADING_DEVICE_LIST = "Fetching BLE devices..";
        public static readonly String STATUS_READY = "Ready";

        private class BLEDevice
        {
            public string Name { get; set; }
            public BluetoothLEDevice Device { get; set; }
            public GattDeviceService GattService { get; set; }
            public IReadOnlyList<GattCharacteristic> GattCharacteristics { get; set; }
            public List<int> BatteryLevels { get; set; }
            public int MinBatteryLevel { get => BatteryLevels.Min(); }
            
            public BLEDevice(string name, BluetoothLEDevice dev, GattDeviceService gattSrv, IReadOnlyList<GattCharacteristic> gattChrs)
            {
                Name = name;
                Device = dev;
                GattService = gattSrv;
                GattCharacteristics = gattChrs;
                BatteryLevels = new List<int>();
            }
        };

        private BLEDevice? bleDevice;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            UpdateTrayIcon();
            Microsoft.Win32.SystemEvents.UserPreferenceChanged += new Microsoft.Win32.UserPreferenceChangedEventHandler(PreferenceChangedHandler);

            ListBLE();
        }

        private void ListBLE()
        {
            reloadButton.Enabled = false;
            statusLabel.Text = STATUS_LOADING_DEVICE_LIST;

            string aqsFilter = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";
            string[] bleAdditionalProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.Bluetooth.Le.IsConnectable", };
            
            DeviceWatcher watcher = DeviceInformation.CreateWatcher(aqsFilter, bleAdditionalProperties, DeviceInformationKind.AssociationEndpoint);
            watcher.Added += (DeviceWatcher deviceWatcher, DeviceInformation di) =>
            {
                if (!String.IsNullOrWhiteSpace(di.Name))
                {
                    BeginInvoke(new Action(() =>
                    {
                        devicesListView.BeginUpdate();

                        ListViewItem listViewItem = new ListViewItem { Text = di.Name };
                        listViewItem.SubItems.Add(di.Pairing.IsPaired ? "Paired " : "Not paired");
                        listViewItem.SubItems.Add(di.Id);
                        listViewItem.Tag = di;

                        devicesListView.Items.Add(listViewItem);

                        devicesListView.EndUpdate();
                    }));
                }
            };
            watcher.Updated += (_, __) => { };
            watcher.EnumerationCompleted += (DeviceWatcher deviceWatcher, object arg) =>
            {
                deviceWatcher.Stop();
            };
            watcher.Stopped += (DeviceWatcher deviceWatcher, object arg) =>
            {
                BeginInvoke(new Action(() =>
                {
                    statusLabel.Text = STATUS_READY;
                    reloadButton.Enabled = true;
                }));
            };
            watcher.Start();
        }

        private async void ConnectToSelectedDevice()
        {
            if (devicesListView.SelectedItems.Count == 0)
                return;

            bleDevice = null;

            DeviceInformation di = (DeviceInformation)devicesListView.SelectedItems[0].Tag;

            var dev = await BluetoothLEDevice.FromIdAsync(di.Id);
            if (dev == null)
            {
                // set error in status
                return;
            }
            var gattServices = await dev.GetGattServicesForUuidAsync(BATTERY_UUID, BluetoothCacheMode.Uncached).AsTask();
            if (gattServices == null)
            {
                // set error in status
                return;
            }

            List<GattCharacteristicsResult?> gattCharacteristicsResults = new List<GattCharacteristicsResult?>();
            for (int i = 0; i < gattServices.Services.Count; i++)
            {
                var gattService = gattServices.Services[i];
                var gattCharacteristics = await gattService.GetCharacteristicsForUuidAsync(BATTERY_LEVEL_UUID, BluetoothCacheMode.Uncached);
                gattCharacteristicsResults.Append(gattCharacteristics);
            }

            string deviceTypeString = DEVICE_TYPE_GENERIC;

            // first search for ZMK Split, it has two parts named "Central" and "Peripheral"
            var selectedIndex = gattCharacteristicsResults.FindIndex(c =>
            {
                return
                    c != null && c.Characteristics.Count == 2 &&
                    c.Characteristics[0].UserDescription == ZMK_CENTRAL_HALF_NAME &&
                    c.Characteristics[1].UserDescription == ZMK_PERIPHERAL_HALF_NAME;
            });

            // generic BLE device with two batteries?
            if (selectedIndex == -1)
            {
                selectedIndex = gattCharacteristicsResults.FindIndex(c => c != null && c.Characteristics.Count == 2);
            }
            else
            {
                deviceTypeString = DEVICE_TYPE_ZMK;
            }

            // generic BLE device with one battery?
            if (selectedIndex == -1)
            {
                selectedIndex = gattCharacteristicsResults.FindIndex(c => c != null && c.Characteristics.Count == 1);
            }

            if (selectedIndex != -1)
            {
                bleDevice = new BLEDevice(di.Name, dev, gattServices.Services[selectedIndex], gattCharacteristicsResults[selectedIndex]!.Characteristics);
                UpdateTrayIcon();
            }
            else
            {
                // sett error in status
            }
        }

        private async static Task<int> ReadValueFromGattCharacteristics(GattCharacteristic gc)
        {
            var res = await gc.ReadValueAsync(BluetoothCacheMode.Uncached);
            if (res == null)
                return -1;

            IBuffer buffer = res.Value;
            byte[] data = new byte[buffer.Length];
            DataReader.FromBuffer(buffer).ReadBytes(data);
            return data[0] == 255 ? -1 : data[0];
        }
        
        private int GetBatteryLevel()
        {
            return 46;
        }

        private void PreferenceChangedHandler(object sender, Microsoft.Win32.UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General)
            {
                // Reload icon if Color Theme has been changed
                UpdateTrayIcon();
            }
        }

        private bool IsWindowsThemeLight()
        {
            var keyOpt = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize");
            if (keyOpt is RegistryKey key)
            {
                var valOpt = key.GetValue("SystemUsesLightTheme");
                if (valOpt is int val)
                {
                    return val != 0;
                }
            }
            return true;
        }

        private Icon GetBatteryIcon(int pcnt)
        {
            pcnt = ((int)Math.Round(pcnt / 10.0)) * 10;
            string iconName = "white-";
            if (IsWindowsThemeLight())
            {
                iconName = "black-";
            }
            iconName += pcnt.ToString("d3");
            object obj = ZMKSplit.Properties.Resources.ResourceManager.GetObject(iconName, ZMKSplit.Properties.Resources.Culture)!;
            return ((Icon)(obj));
        }

        public void UpdateTrayIcon()
        {
            notifyIcon.Icon = GetBatteryIcon(GetBatteryLevel());
            notifyIcon.Text = "Hello";
        }

        private void reloadButton_MouseClick(object sender, MouseEventArgs e)
        {
            ListBLE();
        }

        private void devicesListView_DoubleClick(object sender, EventArgs e)
        {
            ConnectToSelectedDevice();
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            ConnectToSelectedDevice();
        }

        private void devicesListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (devicesListView.SelectedItems.Count != 0)
            {
                connectButton.Enabled = true;
            }
            else
            {
                connectButton.Enabled = false;
            }
        }
    }
}
