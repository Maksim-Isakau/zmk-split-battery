using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace ZMKSplit
{
    internal class BatteryMonitor
    {
        public static readonly Guid BATTERY_UUID = Guid.Parse("{0000180F-0000-1000-8000-00805F9B34FB}");
        public static readonly Guid BATTERY_LEVEL_UUID = Guid.Parse("{00002A19-0000-1000-8000-00805F9B34FB}");

        public static readonly String ZMK_CENTRAL_HALF_NAME = "Central";
        public static readonly String ZMK_PERIPHERAL_HALF_NAME = "Peripheral";

        public enum DeviceType
        {
            Unknown,
            Generic,
            ZMK,
        }

        public struct BatteryStatus
        {
            public List<int> BatteryLevels { get; set; }
            public int MinBatteryLevel { get => BatteryLevels.Min(); }
        };

        public BatteryStatus? GetBatteryStatus()
        {
            return _bleDevice?.BatteryStatus;
        }

        private class BLEDevice
        {
            public string Name { get; set; }
            public BluetoothLEDevice Device { get; set; }
            public GattDeviceService GattService { get; set; }
            public IReadOnlyList<GattCharacteristic> GattCharacteristics { get; set; }
            public BatteryStatus BatteryStatus { get; set; }

            public BLEDevice(string name, BluetoothLEDevice dev, GattDeviceService gattSrv, IReadOnlyList<GattCharacteristic> gattChrs)
            {
                Name = name;
                Device = dev;
                GattService = gattSrv;
                GattCharacteristics = gattChrs;
                BatteryStatus = new BatteryStatus();
            }
        };

        private BLEDevice? _bleDevice;

        public BatteryMonitor()
        {
        }

        public delegate void ListDevicesCallback(string deviceName, string deviceID);
        public delegate void ListDevicesCompletionCallback();
        public static void ListPairedDevices(ListDevicesCallback cb, ListDevicesCompletionCallback completionCB)
        {
            string aqsFilter = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";
            string[] bleAdditionalProperties =
            {
                "System.Devices.Aep.DeviceAddress",
                "System.Devices.Aep.Bluetooth.Le.IsConnectable",
            };

            DeviceWatcher watcher = DeviceInformation.CreateWatcher(aqsFilter, bleAdditionalProperties, DeviceInformationKind.AssociationEndpoint);
            watcher.Added += (DeviceWatcher deviceWatcher, DeviceInformation di) =>
            {
                if (di.Pairing.IsPaired && !String.IsNullOrWhiteSpace(di.Name))
                {
                    cb(di.Name, di.Id);
                }
            };
            watcher.Updated += (_, __) => { };
            watcher.EnumerationCompleted += (DeviceWatcher deviceWatcher, object arg) =>
            {
                deviceWatcher.Stop();
            };
            watcher.Stopped += (DeviceWatcher deviceWatcher, object arg) =>
            {
                completionCB();
            };
            watcher.Start();
        }

        public enum ConnectStatus
        {
            Connected,
            DeviceNotFound,
            BatteryServiceNotFound,
            BatteryLevelCharacteristicNotFound
        }

        public struct ConnectResult
        {
            public ConnectStatus Status { get; set; }
            public DeviceType Type { get; set; }
            public string ErrorMessage { get; set; }
        }

        public async Task<ConnectResult> Connect(string deviceName, string deviceID)
        {
            _bleDevice = null;
            
            var dev = await BluetoothLEDevice.FromIdAsync(deviceID);
            if (dev == null)
            {
                return new ConnectResult { Status = ConnectStatus.DeviceNotFound, Type = DeviceType.Unknown };
            }
            
            var gattServices = await dev.GetGattServicesForUuidAsync(BATTERY_UUID, BluetoothCacheMode.Uncached).AsTask();
            if (gattServices == null)
            {
                return new ConnectResult { Status = ConnectStatus.BatteryServiceNotFound, Type = DeviceType.Unknown };
            }

            List<GattCharacteristicsResult?> gattCharacteristicsResults = new List<GattCharacteristicsResult?>();
            for (int i = 0; i < gattServices.Services.Count; i++)
            {
                var gattService = gattServices.Services[i];
                var gattCharacteristics = await gattService.GetCharacteristicsForUuidAsync(BATTERY_LEVEL_UUID, BluetoothCacheMode.Uncached);
                gattCharacteristicsResults.Add(gattCharacteristics);
            }

            DeviceType deviceType = DeviceType.Generic;

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
                deviceType = DeviceType.ZMK;
            }

            // generic BLE device with one battery?
            if (selectedIndex == -1)
            {
                selectedIndex = gattCharacteristicsResults.FindIndex(c => c != null && c.Characteristics.Count == 1);
            }

            if (selectedIndex != -1)
            {
                _bleDevice = new BLEDevice(deviceName, dev, gattServices.Services[selectedIndex], gattCharacteristicsResults[selectedIndex]!.Characteristics);
                return new ConnectResult { Status = ConnectStatus.Connected, Type = deviceType };
            }
            else
            {
                return new ConnectResult { Status = ConnectStatus.BatteryLevelCharacteristicNotFound, Type = deviceType };
            }
        }

        public bool IsConnected()
        {
            return _bleDevice != null;
        }

        public enum ReadStatus
        {
            Success,
            NotConnected,
            Failure,
        }

        public struct ReadBatteryLevelResult
        {
            public ReadStatus Status { get; set; }
            public List<(string, int)> BatteryLevels { get; set; }
            public string ErrorMessage { get; set; }

            public ReadBatteryLevelResult(ReadStatus status, int singleValue)
            {
                Status = status;
                BatteryLevels = new List<(string, int)>() { ("", singleValue) };
                ErrorMessage = "";
            }

            public ReadBatteryLevelResult(ReadStatus status, string errorMessage)
            {
                Status = status;
                BatteryLevels = new List<(string, int)>();
                ErrorMessage = errorMessage;
            }
        }

        private async Task<ReadBatteryLevelResult> ReadBatteryLevel(GattCharacteristic gc)
        {
            try
            {
                var res = await gc.ReadValueAsync(BluetoothCacheMode.Uncached);
                if (res == null)
                {
                    return new ReadBatteryLevelResult(ReadStatus.Failure, "ReadValueAsync returned null");
                }

                if (res.Status != GattCommunicationStatus.Success)
                {
                    return new ReadBatteryLevelResult(ReadStatus.Failure, "ReadValueAsync returned status " + res.Status.ToString());
                }

                IBuffer buffer = res.Value;
                byte[] data = new byte[buffer.Length];
                DataReader.FromBuffer(buffer).ReadBytes(data);
                int lvl = data[0] == 255 ? -1 : data[0];
                return new ReadBatteryLevelResult(ReadStatus.Success, lvl);
            }
            catch (Exception e)
            {
                return new ReadBatteryLevelResult(ReadStatus.Failure, e.Message);
            }
        }

        public async Task<ReadBatteryLevelResult> ReadBatteryLevels()
        {
            if (_bleDevice == null)
                return new ReadBatteryLevelResult { Status = ReadStatus.NotConnected }; ;

            var bleDev = _bleDevice;
            List<(string, int)> batteryLevels = new List<(string, int)>();
            foreach (var gc in bleDev.GattCharacteristics)
            {
                var res = await ReadBatteryLevel(gc);
                if (res.Status != ReadStatus.Success)
                    return res;
                batteryLevels.Add((gc.UserDescription, res.BatteryLevels[0].Item2));
            }
            return new ReadBatteryLevelResult { Status = ReadStatus.Success, BatteryLevels = batteryLevels };
        }
    }
}
