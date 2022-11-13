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
using Microsoft.Toolkit.Uwp.Notifications;

namespace ZMKSplit
{
    public partial class MainForm : Form
    {
        public static readonly String RELOAD_BUTTON_STATE_RELOADING = "Looking for devices..";
        public static readonly String RELOAD_BUTTON_STATE_READY = "Reload Devices";

        public static readonly String CONNECT_BUTTON_CONNECT = "Connect";
        public static readonly String CONNECT_BUTTON_DISCONNECT = "Disconnect";

        public static readonly String STATUS_CONNECTING = "Connecting to '{0}'..";
        public static readonly String STATUS_CONNECTING_IN = "Connecting to '{0}' in {1} seconds..";
        public static readonly String STATUS_CONNECTION_FAILED = "Could not connected to '{0}': {1}";
        public static readonly String STATUS_CONNECTED = "Connected to {0}";
        public static readonly String STATUS_READY = "Ready";
        public static readonly String STATUS_READ_BATTERY_LEVEL_FAILED = "Could not read battery level: {0}";

        public static readonly int    BATTERY_LOW_LEVEL_THRESHOLD = 20;
        public static readonly string BATTERY_LOW_TIP_TITLE = "Low battery";
        public static readonly string BATTERY_LOW_TIP_MESSAGE = "{0} battery level is below " + BATTERY_LOW_LEVEL_THRESHOLD + "%";

        public static readonly int    RECONNECT_INTERVAL = 300;

        private BatteryMonitor _batteryMonitor;
        private string _deviceName = "";
        private string _deviceID = "";
        private int _lastMinLevel = -1;
        private int _reconnectCounter = RECONNECT_INTERVAL;

        public MainForm()
        {
            _batteryMonitor = new BatteryMonitor(OnBatteryLevelChanged);
            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            UpdateTrayIcon();
            
            Microsoft.Win32.SystemEvents.UserPreferenceChanged += new Microsoft.Win32.UserPreferenceChangedEventHandler(PreferenceChangedHandler);
            
            statusLabel.Text = STATUS_READY;
            connectButton.Text = CONNECT_BUTTON_CONNECT;
            reloadButton.Text = RELOAD_BUTTON_STATE_READY;

            ParseCommandLineArguments();

            if (_deviceID.Length != 0 && _deviceName.Length != 0)
            {
                // make sure a toast notification will pop up once we connect to the device
                _lastMinLevel = 100;
                
                BeginInvoke(new Action(() =>
                {
                    Hide();
                    _reconnectCounter = 1;
                    reconnectTimer.Start();
                }));
            }
            else
            {
                WindowState = FormWindowState.Normal;
                ShowInTaskbar = true;
                ListBLEDevices();
            }
        }

        private void ParseCommandLineArguments()
        {
            // parse -deviceid and -devicename
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-deviceid" && i + 1 < args.Length)
                {
                    _deviceID = args[++i].Trim("\"".ToCharArray());
                }
                else if (args[i] == "-devicename" && i + 1 < args.Length)
                {
                    _deviceName = args[++i].Trim("\"".ToCharArray());
                }
            }
        }

        private void ListBLEDevices()
        {
            devicesListView.Items.Clear();
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

        public void OnBatteryLevelChanged()
        {
            BeginInvoke(new Action(() => UpdateTrayIcon()));
        }

        private async Task<bool> ConnectToDevice(string deviceName, string deviceID)
        {
            statusLabel.Text = String.Format(STATUS_CONNECTING, deviceName);

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
            else if (res.Status == BatteryMonitor.ConnectStatus.SubscribtionFailure)
            {
                statusLabel.Text = String.Format(STATUS_CONNECTION_FAILED, deviceName, "Could not subscribe to battery level notifications");
            }
            else
            {
                Debug.Assert(res.Status == BatteryMonitor.ConnectStatus.Connected, "Unknown BatteryMonitor.ConnectStatus");
                if (res.Status == BatteryMonitor.ConnectStatus.Connected)
                {
                    _deviceName = deviceName;
                    _deviceID = deviceID;
                    statusLabel.Text = String.Format(STATUS_CONNECTED, _deviceName);
                    UpdateTrayIcon();
                }
            }

            return res.Status == ConnectStatus.Connected;
        }

        private async Task<bool> ConnectToSelectedDevice()
        {
            if (devicesListView.SelectedItems.Count == 0)
                return false;
            
            string deviceName = devicesListView.SelectedItems[0].Text;
            string deviceID = (string)devicesListView.SelectedItems[0].Tag;

            reconnectTimer.Stop();

            return await ConnectToDevice(deviceName, deviceID);
        }

        private void DisconnectFromSelectedDevice()
        {
            _batteryMonitor.Disconnect();
            _deviceName = "";
            _deviceID = "";
            statusLabel.Text = STATUS_READY;
            UpdateTrayIcon();
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
        
        public void UpdateTrayIcon()
        {
            int minLevel = 100;
            if (!_batteryMonitor.IsConnected() || _batteryMonitor.Batteries.Count == 0)
            {
                minLevel = -1;
                notifyIcon.Text = "Not connected";
            }
            else if (_batteryMonitor.Batteries.Count == 1)
            {
                minLevel = _batteryMonitor.Batteries.First().Value.Level;
                notifyIcon.Text = String.Format("{0}: {1}%", _deviceName, minLevel);
            }
            else
            {
                notifyIcon.Text = _deviceName + "\n";
                foreach (var battery in _batteryMonitor.Batteries.Values)
                {
                    notifyIcon.Text += battery.Name + ": " + battery.Level + "%\n";
                    minLevel = Math.Min(minLevel, battery.Level);
                }
            }
            notifyIcon.Icon = GetBatteryIcon(minLevel);
            if (_lastMinLevel > BATTERY_LOW_LEVEL_THRESHOLD && minLevel != -1 && minLevel <= BATTERY_LOW_LEVEL_THRESHOLD)
            {
                new ToastContentBuilder()
                    .AddText(BATTERY_LOW_TIP_TITLE)
                    .AddText(String.Format(BATTERY_LOW_TIP_MESSAGE, _deviceName))
                    .Show();
            }
        }

        private void reloadButton_MouseClick(object sender, MouseEventArgs e)
        {
            ListBLEDevices();
        }

        private void devicesListView_DoubleClick(object sender, EventArgs e)
        {
            if (connectButton.Enabled && !_batteryMonitor.IsConnected())
            {
                connectButton_Click(sender, e);
            }
        }
        
        private async void connectButton_Click(object sender, EventArgs e)
        {
            connectButton.Enabled = false;
            if (_batteryMonitor.IsConnected())
            {
                DisconnectFromSelectedDevice();
                connectButton.Text = CONNECT_BUTTON_CONNECT;
            }
            else
            {
                var res = await ConnectToSelectedDevice();
                if (res)
                {
                    connectButton.Text = CONNECT_BUTTON_DISCONNECT;
                }
            }
            connectButton.Enabled = true;
        }

        private void exitContextMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void showContextMenuItem_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            ShowInTaskbar = true;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private async void reconnectTimer_Tick(object sender, EventArgs e)
        {
            if (--_reconnectCounter == 0)
            {
                reconnectTimer.Stop();
                
                Debug.Assert(_deviceID.Length > 0);
                Debug.Assert(_deviceName.Length > 0);
                
                bool res = await ConnectToDevice(_deviceName, _deviceID);
                if (!res)
                {
                    _reconnectCounter = RECONNECT_INTERVAL;
                    reconnectTimer.Start();
                }
                else
                {
                    connectButton.Text = CONNECT_BUTTON_DISCONNECT;
                }
            }
            else
            {
                statusLabel.Text = String.Format(STATUS_CONNECTING_IN, _deviceName, _reconnectCounter);
            }
        }
    }
}
