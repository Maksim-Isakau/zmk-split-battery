using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Security.Cryptography.Xml;
using System.Windows.Forms;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

//https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getlastinputinfo

namespace ZMKSplit
{
    public partial class MainForm : Form
    {
        public static readonly Guid BATTERY_UUID = Guid.Parse("{0000180F-0000-1000-8000-00805F9B34FB}");
        public static readonly Guid BATTERY_LEVEL_UUID = Guid.Parse("{00002A19-0000-1000-8000-00805F9B34FB}");
        public static readonly String STATUS_LOADING_DEVICE_LIST = "Loading BLE devices..";
        public static readonly String STATUS_READY = "Ready";

        private BluetoothLEDevice? bleDevice;
        private GattDeviceService? gattService;
        private GattCharacteristic? primaryGattCharacteristic;
        private GattCharacteristic? secondaryGattCharacteristic2;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            SetSVGIcon();
            Microsoft.Win32.SystemEvents.UserPreferenceChanged += new Microsoft.Win32.UserPreferenceChangedEventHandler(PreferenceChangedHandler);

            ListBLE2();
        }

        private async void ListBLE2()
        {
            reloadButton.Enabled = false;
            statusLabel.Text = STATUS_LOADING_DEVICE_LIST;

            string aqsFilter = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";
            string[] bleAdditionalProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.Bluetooth.Le.IsConnectable", };
            var devices = await DeviceInformation.FindAllAsync(aqsFilter, bleAdditionalProperties, DeviceInformationKind.AssociationEndpoint);

            devicesListView.BeginUpdate();
            devicesListView.Items.Clear();
            foreach (DeviceInformation di in devices)
            {
                if (!String.IsNullOrWhiteSpace(di.Name))
                {
                    ListViewItem listViewItem = new ListViewItem { Text = di.Name };
                    listViewItem.SubItems.Add(di.Pairing.IsPaired ? "Paired " : "Not paired");
                    listViewItem.Tag = di;
                    devicesListView.Items.Add(listViewItem);
                }
            }
            devicesListView.EndUpdate();

            statusLabel.Text = STATUS_READY;
            reloadButton.Enabled = true;
        }

        private void ListBLE()
        {
            string aqsFilter = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";
            string[] bleAdditionalProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.Bluetooth.Le.IsConnectable", };
            var task = DeviceInformation.FindAllAsync(aqsFilter, bleAdditionalProperties, DeviceInformationKind.AssociationEndpoint).AsTask();
            task.Wait();

            devicesListView.BeginUpdate();
            foreach (DeviceInformation di in task.Result)
            {
                ListViewItem listViewItem = new ListViewItem { Text = di.Name };
                listViewItem.SubItems.Add(di.Pairing.IsPaired ? "Paired " : "Not paired");
                listViewItem.SubItems.Add(di.Id);
                listViewItem.Tag = di;
            }
            devicesListView.EndUpdate();
            //var res = r.AsTask().Wait();
            /*DeviceWatcher watcher = DeviceInformation.CreateWatcher(aqsFilter, bleAdditionalProperties, DeviceInformationKind.AssociationEndpoint);
            //watcher.Added += (DeviceWatcher deviceWatcher, DeviceInformation devInfo) =>
            {
                if (!String.IsNullOrWhiteSpace(devInfo.Name))
                {
                    devicesListView.BeginUpdate();
                    
                    ListViewItem listViewItem = new ListViewItem{ Text = devInfo.Name };
                    listViewItem.SubItems.Add(devInfo.Pairing.IsPaired ? "Paired " : "Not paired");
                    listViewItem.SubItems.Add(devInfo.Id);
                    listViewItem.Tag = devInfo;

                    devicesListView.EndUpdate();
                }
            };
            watcher.Updated += (_, __) => { };
            watcher.EnumerationCompleted += (DeviceWatcher deviceWatcher, object arg) => { deviceWatcher.Stop(); };
            watcher.Stopped += (DeviceWatcher deviceWatcher, object arg) => { deviceWatcher.Start(); };
            watcher.Start();
            */
        }

        private void PreferenceChangedHandler(object sender, Microsoft.Win32.UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General)
            {
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

        private void refreshIntervalEditBox_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void reloadButton_MouseClick(object sender, MouseEventArgs e)
        {
            ListBLE2();
        }

        private void devicesListView_DoubleClick(object sender, EventArgs e)
        {
            if (devicesListView.SelectedItems.Count != 0)
            {
                notifyIcon.Icon = GetBattIcon(GetBattLevel());
                notifyIcon.Text = "Hello";
                DeviceInformation di = (DeviceInformation)devicesListView.SelectedItems[0].Tag;
            }
            
        }
    }
}
