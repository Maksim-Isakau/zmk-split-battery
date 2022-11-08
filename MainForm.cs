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
using System.Collections.Generic;
using System.Linq;
using Windows.Storage.Streams;
using System.Diagnostics;
using static ZMKSplit.BatteryMonitor;

// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getlastinputinfo
// 
// TODO:
//  - statuses
//  - state machine? once device list enum is completed check if we're connecting/connected before setting "ready" etc
//  - correctly abort awaits, crash on exit after ReadBatteryLevels->UpdateTrayIcon

namespace ZMKSplit
{
    public partial class MainForm : Form
    {
        public static readonly String DEVICE_TYPE_ZMK = "ZMK Split Keyboard";
        public static readonly String DEVICE_TYPE_GENERIC = "Generic BLE device";

        public static readonly String RELOAD_BUTTON_STATE_RELOADING = "Looking for devices..";
        public static readonly String RELOAD_BUTTON_STATE_READY = "Reload Devices";
        
        public static readonly String STATUS_CONNECTING = "Connecting to '{0}'..";
        public static readonly String STATUS_CONNECTION_FAILED = "Could not connected to '{0}': {1}";
        public static readonly String STATUS_CONNECTED = "Connected to {0} '{1}'";
        public static readonly String STATUS_READY = "Ready";
        public static readonly String STATUS_READ_BATTERY_LEVEL_FAILED = "Could not read battery level: {0}";

        private BatteryMonitor _batteryMonitor;
        private string _deviceName;
        private string _deviceID;
        private string _deviceType;

        public MainForm()
        {
            _batteryMonitor = new BatteryMonitor();
            _deviceName = "";
            _deviceID = "";
            _deviceType = "";
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            UpdateTrayIcon(false, new List<(string, int)>());
            Microsoft.Win32.SystemEvents.UserPreferenceChanged += new Microsoft.Win32.UserPreferenceChangedEventHandler(PreferenceChangedHandler);

            ListBLEDevices();
        }

        private void ListBLEDevices()
        {
            reloadButton.Enabled = false;
            reloadButton.Text = RELOAD_BUTTON_STATE_RELOADING;
            BatteryMonitor.ListPairedDevices((string deviceName, string deviceID) =>
            {
                BeginInvoke(new Action(() =>
                {
                    devicesListView.BeginUpdate();
                    devicesListView.Items.Add(new ListViewItem { Text = deviceName, Tag = deviceID });
                    devicesListView.EndUpdate();
                }));
            }, () =>
            {
                BeginInvoke(new Action(() =>
                {
                    reloadButton.Enabled = true;
                    reloadButton.Text = RELOAD_BUTTON_STATE_READY;
                }));
            });
        }

        private async void ConnectToSelectedDevice()
        {
            if (devicesListView.SelectedItems.Count == 0)
                return;
            
            string deviceName = devicesListView.SelectedItems[0].Text;
            string deviceID = (string)devicesListView.SelectedItems[0].Tag;
            
            readBatteryLevelsTimer.Enabled = false;
            statusLabel.Text = String.Format(STATUS_CONNECTING, deviceName);
            connectButton.Enabled = false;

            var res = await _batteryMonitor.Connect(deviceName, deviceID);

            if (res.Status == BatteryMonitor.ConnectStatus.DeviceNotFound)
            {
                statusLabel.Text = String.Format(STATUS_CONNECTION_FAILED, deviceName, "Device not found");
            }
            else if (res.Status == BatteryMonitor.ConnectStatus.BatteryServiceNotFound)
            {
                statusLabel.Text = String.Format(STATUS_CONNECTION_FAILED, deviceName, "Battery service not found");
            }
            else if (res.Status == BatteryMonitor.ConnectStatus.BatteryLevelCharacteristicNotFound)
            {
                statusLabel.Text = String.Format(STATUS_CONNECTION_FAILED, deviceName, "Could not find battery level GATT characteristic. Is the device offline?");
            }
            else
            {
                Debug.Assert(res.Status == BatteryMonitor.ConnectStatus.Connected, "Unknown BatteryMonitor.ConnectStatus");
                if (res.Status == BatteryMonitor.ConnectStatus.Connected)
                {
                    _deviceName = deviceName;
                    _deviceID = deviceID;
                    _deviceType = res.Type == BatteryMonitor.DeviceType.ZMK ? DEVICE_TYPE_ZMK : DEVICE_TYPE_GENERIC;
                    statusLabel.Text = String.Format(STATUS_CONNECTED, _deviceType, _deviceName);
                    readBatteryLevelsTimer.Enabled = true;
                }
            }

            connectButton.Enabled = true;
        }

        private void PreferenceChangedHandler(object sender, Microsoft.Win32.UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General)
            {
                // Reload icon if Color Theme has been changed
                readBatteryLevelsTimer_Tick(sender, e);
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
            string iconName = "white-";
            if (IsWindowsThemeLight())
            {
                iconName = "black-";
            }
            if (pcnt == -1)
            {
                iconName += "dsc";
            }
            else
            {
                pcnt = ((int)Math.Round(pcnt / 10.0)) * 10;
                iconName += pcnt.ToString("d3");
            }
            object obj = ZMKSplit.Properties.Resources.ResourceManager.GetObject(iconName, ZMKSplit.Properties.Resources.Culture)!;
            return ((Icon)(obj));
        }
        
        public void UpdateTrayIcon(bool connected, List<(string, int)> batteryLevels)
        {
            if (!connected || batteryLevels.Count == 0)
            {
                notifyIcon.Icon = GetBatteryIcon(-1);
                notifyIcon.Text = "Not connected";
                return;
            }
            else
            {
                notifyIcon.Icon = GetBatteryIcon(batteryLevels.Min<(string, int), int>(x => x.Item2));
                notifyIcon.Text = _deviceName + "\n";
                for (int i = 0; i < batteryLevels.Count; i++)
                {
                    notifyIcon.Text += "\n" + batteryLevels[i].Item1 + ": " + batteryLevels[i].Item2 + "%";
                }
            }
        }

        private void reloadButton_MouseClick(object sender, MouseEventArgs e)
        {
            ListBLEDevices();
        }

        private void devicesListView_DoubleClick(object sender, EventArgs e)
        {
            if (connectButton.Enabled)
            {
                ConnectToSelectedDevice();
            }
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

        private async void readBatteryLevelsTimer_Tick(object sender, EventArgs e)
        {
            var res = await _batteryMonitor.ReadBatteryLevels();
            if (res.Status == BatteryMonitor.ReadStatus.Failure)
            {
                statusLabel.Text = String.Format(STATUS_READ_BATTERY_LEVEL_FAILED, res.ErrorMessage);
            }
            else if (res.Status == BatteryMonitor.ReadStatus.Success)
            {
                statusLabel.Text = String.Format(STATUS_CONNECTED, _deviceType, _deviceName);
            }
            else if (res.Status == BatteryMonitor.ReadStatus.NotConnected)
            {
                statusLabel.Text = String.Format(STATUS_READY);
            }
            else
            {
                Debug.Assert(res.Status == BatteryMonitor.ReadStatus.Success, "Unknown BatteryMonitor.ReadStatus");
            }
            UpdateTrayIcon(_batteryMonitor.IsConnected(), res.BatteryLevels);
        }
    }
}
