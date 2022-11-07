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
using InTheHand.Bluetooth;

//https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getlastinputinfo

namespace ZMKSplit
{
    public partial class MainForm : Form
    {
        public static readonly Guid BATTERY_UUID = Guid.Parse("{0000180F-0000-1000-8000-00805F9B34FB}");
        public static readonly Guid BATTERY_LEVEL_UUID = Guid.Parse("{00002A19-0000-1000-8000-00805F9B34FB}");
        public static readonly String STATUS_LOADING_DEVICE_LIST = "Fetching BLE devices..";
        public static readonly String STATUS_READY = "Ready";

        private class BLEDevice
        {
            public string Name { get; set; }
            public BluetoothLEDevice Device { get; set; }
            public GattDeviceService GattService { get; set; }
            public List<GattCharacteristic> GattCharacteristics { get; set; }
        };

        private BLEDevice? _bleDevice;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            SetSVGIcon();
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

            DeviceInformation di = (DeviceInformation)devicesListView.SelectedItems[0].Tag;

            var dev = await BluetoothLEDevice.FromIdAsync(di.Id);
            if (dev == null)
            {
                //
                return;
            }
            var gattServices = await dev.GetGattServicesForUuidAsync(BATTERY_UUID, BluetoothCacheMode.Uncached).AsTask();
            if (gattServices == null)
            {
                //
                return;
            }

            List<GattCharacteristicsResult?> results = new List<GattCharacteristicsResult?>();
            for (int i = 0; i < gattServices.Services.Count; i++)
            {
                var gattService = gattServices.Services[i];
                var gattCharacteristics = await gattService.GetCharacteristicsForUuidAsync(BATTERY_LEVEL_UUID, BluetoothCacheMode.Uncached);
                results.Append(gattCharacteristics);
            }

            if (gattCharacteristics.Characteristics.Count != 0)
            {
                _bleDevice.device = dev;
                _bleDevice.gattService = gattService;
                _bleDevice.gattCharacteristics = gattCharacteristics;
                selectGattCharacteristic = gattCharacteristicsTask.Result.Characteristics[0];
                selectGattCharacteristic2 = gattCharacteristicsTask.Result.Characteristics[1];
                var descr1 = selectGattCharacteristic.UserDescription;
                var descr2 = selectGattCharacteristic2.UserDescription;
                var cnt = gattCharacteristicsTask.Result.Characteristics.Count;
                form.Notify("BLE Device ID:" + selectedBLEDev.DeviceId);
                form.StartUpdate();
                return;
            }

            notifyIcon.Icon = GetBattIcon(GetBattLevel());
            notifyIcon.Text = "Hello";
        }

        private void PreferenceChangedHandler(object sender, Microsoft.Win32.UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General)
            {
                // Reload icon if Color Theme has been changed
                SetSVGIcon();
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

        private int GetBattLevel()
        {
            return 46;
        }

        private Icon GetBattIcon(int pcnt)
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

        public void SetSVGIcon()
        {
            notifyIcon.Icon = GetBattIcon(GetBattLevel());
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
