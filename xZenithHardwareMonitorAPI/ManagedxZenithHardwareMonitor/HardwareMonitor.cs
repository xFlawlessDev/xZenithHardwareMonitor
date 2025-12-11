using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using Newtonsoft.Json;

namespace ManagedxZenithHardwareMonitor
{
    /// <summary>
    /// Event arguments for WMI events.
    /// </summary>
    /// <remarks>
    /// <para><b>Tauri Usage:</b> Serialize this to JSON and emit via Tauri event system.</para>
    /// <code>
    /// // Rust side (src-tauri/src/main.rs):
    /// app.emit_all("wmi-event", WmiEventPayload { 
    ///     event_type: e.Type, 
    ///     data: e.Data, 
    ///     message: e.Message 
    /// });
    /// 
    /// // Frontend (TypeScript):
    /// import { listen } from '@tauri-apps/api/event';
    /// await listen('wmi-event', (event) => {
    ///     console.log('WMI Event:', event.payload);
    /// });
    /// </code>
    /// </remarks>
    public class WmiEventArgs : EventArgs
    {
        /// <summary>Event type: "WMI_EVENT", "WMI_TEST", or "WMI_CLOSE".</summary>
        public string Type { get; set; }
        /// <summary>Raw event data as integer array (byte values).</summary>
        public int[] Data { get; set; }
        /// <summary>Human-readable event message.</summary>
        public string Message { get; set; }
        /// <summary>Additional event details with timestamp.</summary>
        public string Details { get; set; }
    }

    /// <summary>
    /// Event arguments for keyboard status changes (CapsLock/NumLock).
    /// </summary>
    /// <remarks>
    /// <para><b>Tauri Usage:</b> Emit keyboard state changes to frontend.</para>
    /// <code>
    /// // Rust side:
    /// app.emit_all("key-status", KeyStatusPayload { 
    ///     caps_lock: e.CapsLock, 
    ///     num_lock: e.NumLock 
    /// });
    /// 
    /// // Frontend (TypeScript):
    /// await listen('key-status', (event) => {
    ///     const { caps_lock, num_lock } = event.payload;
    ///     updateKeyboardIndicators(caps_lock, num_lock);
    /// });
    /// </code>
    /// </remarks>
    public class KeyStatusEventArgs : EventArgs
    {
        /// <summary>True if Caps Lock is enabled.</summary>
        public bool CapsLock { get; set; }
        /// <summary>True if Num Lock is enabled.</summary>
        public bool NumLock { get; set; }
        /// <summary>Timestamp when the status changed.</summary>
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Hardware monitoring library for CPU, GPU, Memory, Storage, and system events.
    /// Provides WMI event listening and keyboard status monitoring.
    /// </summary>
    /// <remarks>
    /// <para><b>Tauri Integration Guide:</b></para>
    /// <para>
    /// This library can be used with Tauri via C# interop (using a native DLL wrapper)
    /// or by creating a Tauri command that invokes the .NET runtime.
    /// </para>
    /// 
    /// <para><b>Option 1: Native DLL Export (Recommended)</b></para>
    /// <code>
    /// // Create a C++/CLI wrapper or use DllExport to expose functions:
    /// // NativeExports.cs
    /// [DllExport("GetHardwareReport", CallingConvention.Cdecl)]
    /// public static IntPtr GetHardwareReport() {
    ///     var monitor = new HardwareMonitor();
    ///     return Marshal.StringToHGlobalAnsi(monitor.GetReport());
    /// }
    /// 
    /// // Rust side (src-tauri/src/main.rs):
    /// #[link(name = "ManagedxZenithHardwareMonitor")]
    /// extern "C" {
    ///     fn GetHardwareReport() -> *const c_char;
    /// }
    /// 
    /// #[tauri::command]
    /// fn get_hardware_report() -> String {
    ///     unsafe { CStr::from_ptr(GetHardwareReport()).to_string_lossy().into_owned() }
    /// }
    /// </code>
    /// 
    /// <para><b>Option 2: Process-based Communication</b></para>
    /// <code>
    /// // Create a small .NET console app that outputs JSON:
    /// // Program.cs
    /// var monitor = new HardwareMonitor();
    /// Console.WriteLine(monitor.GetReport());
    /// 
    /// // Rust side:
    /// #[tauri::command]
    /// async fn get_hardware_report() -> Result&lt;String, String&gt; {
    ///     let output = Command::new("dotnet")
    ///         .args(["run", "--project", "path/to/helper"])
    ///         .output()
    ///         .map_err(|e| e.to_string())?;
    ///     Ok(String::from_utf8_lossy(&amp;output.stdout).to_string())
    /// }
    /// </code>
    /// 
    /// <para><b>Event Handling with Tauri:</b></para>
    /// <code>
    /// // For real-time events, use Tauri's event system:
    /// monitor.WmiEventReceived += (s, e) => {
    ///     // Emit to Tauri frontend
    ///     app.emit_all("wmi-event", new { type = e.Type, data = e.Data });
    /// };
    /// 
    /// monitor.KeyStatusChanged += (s, e) => {
    ///     app.emit_all("key-status", new { capsLock = e.CapsLock, numLock = e.NumLock });
    /// };
    /// 
    /// // Frontend listener:
    /// import { listen } from '@tauri-apps/api/event';
    /// 
    /// const unlisten = await listen('wmi-event', (event) => {
    ///     console.log('WMI:', event.payload);
    /// });
    /// 
    /// const unlistenKeys = await listen('key-status', (event) => {
    ///     document.getElementById('capslock').classList.toggle('active', event.payload.capsLock);
    ///     document.getElementById('numlock').classList.toggle('active', event.payload.numLock);
    /// });
    /// </code>
    /// </remarks>
    public class HardwareMonitor
    {
        private Computer _computer;

        // WMI event listener
        private ManagementEventWatcher _wmiWatcher;
        private Thread _keyMonitorThread;
        private volatile bool _isMonitoring;
        private bool? _lastCapsLockStatus;
        private bool? _lastNumLockStatus;

        // P/Invoke for keyboard state
        [DllImport("user32.dll")]
        private static extern short GetKeyState(int nVirtKey);
        private const int VK_CAPITAL = 0x14; // Caps Lock
        private const int VK_NUMLOCK = 0x90; // Num Lock

        /// <summary>Raised when a WMI event is received from the system.</summary>
        public event EventHandler<WmiEventArgs> WmiEventReceived;
        /// <summary>Raised when CapsLock or NumLock status changes.</summary>
        public event EventHandler<KeyStatusEventArgs> KeyStatusChanged;

        /// <summary>
        /// Initializes the hardware monitor and opens connection to system hardware.
        /// </summary>
        public HardwareMonitor()
        {
            _computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsMotherboardEnabled = true,
                IsControllerEnabled = false,
                IsNetworkEnabled = true,
                IsBatteryEnabled = true,
                IsStorageEnabled = true
            };

            // open connection to the computer and update hardware information
            _computer.Open();
            _computer.Accept(new UpdateVisitor());
        }

        /// <summary>
        /// Updates all hardware sensor readings.
        /// </summary>
        /// <remarks>
        /// <b>Tauri:</b> Call periodically (e.g., every 1-2 seconds) to refresh sensor data.
        /// </remarks>
        public void Update()
        {
            _computer.Accept(new UpdateVisitor());
        }

        /// <summary>
        /// Gets a JSON report of all hardware information including sensors.
        /// </summary>
        /// <returns>JSON string containing hardware data.</returns>
        /// <remarks>
        /// <b>Tauri:</b> Return this directly from a Tauri command to the frontend.
        /// <code>
        /// #[tauri::command]
        /// fn get_report() -> String { monitor.GetReport() }
        /// 
        /// // Frontend:
        /// const data = await invoke('get_report');
        /// const hardware = JSON.parse(data);
        /// </code>
        /// </remarks>
        public string GetReport()
        {
            Hardware[] parsed_hardware = ParseHardware(_computer.Hardware);
            string jsonString = JsonConvert.SerializeObject(parsed_hardware, new JsonSerializerSettings
            {
                FloatFormatHandling = FloatFormatHandling.DefaultValue
            });
            return jsonString;
        }

        /// <summary>
        /// Gets the buffer size needed for the JSON report (for native interop).
        /// </summary>
        /// <returns>Length of JSON string plus null terminator.</returns>
        public int GetReportSize()
        {
            return GetReport().Length + 1;
        }

        /// <summary>
        /// Starts listening for WMI events (IP3_WMIEvent).
        /// </summary>
        /// <returns>True if listener started successfully, false if not available on this device.</returns>
        /// <remarks>
        /// Subscribe to <see cref="WmiEventReceived"/> before calling this method.
        /// Note: IP3_WMIEvent is only available on certain devices (e.g., some OEM laptops).
        /// </remarks>
        public bool StartWmiEventListener()
        {
            try
            {
                // Check if IP3_WMIEvent class exists on this device
                if (!WmiClassExists("root\\WMI", "IP3_WMIEvent"))
                {
                    OnWmiEventReceived(new WmiEventArgs
                    {
                        Type = "WMI_UNAVAILABLE",
                        Data = null,
                        Message = "WMI Event Listener not available",
                        Details = "IP3_WMIEvent is not supported on this device"
                    });
                    return false;
                }

                var query = new WqlEventQuery("SELECT * FROM IP3_WMIEvent");
                _wmiWatcher = new ManagementEventWatcher(new ManagementScope("root\\WMI"), query);

                _wmiWatcher.EventArrived += (sender, e) =>
                {
                    try
                    {
                        var wmiEvent = e.NewEvent;
                        var eventDetail = (byte[])wmiEvent.Properties["EventDetail"].Value;
                        var dataAsIntArray = eventDetail.Select(b => (int)b).ToArray();

                        OnWmiEventReceived(new WmiEventArgs
                        {
                            Type = "WMI_EVENT",
                            Data = dataAsIntArray,
                            Message = "WMI Event received",
                            Details = $"Event received at {DateTime.Now}"
                        });
                    }
                    catch { }
                };

                _wmiWatcher.Start();

                // Send test event
                OnWmiEventReceived(new WmiEventArgs
                {
                    Type = "WMI_TEST",
                    Data = null,
                    Message = "WMI Event Listener Started",
                    Details = "Connection verified"
                });

                return true;
            }
            catch
            {
                // Clean up on failure
                if (_wmiWatcher != null)
                {
                    try
                    {
                        _wmiWatcher.Stop();
                        _wmiWatcher.Dispose();
                    }
                    catch { }
                    _wmiWatcher = null;
                }

                OnWmiEventReceived(new WmiEventArgs
                {
                    Type = "WMI_UNAVAILABLE",
                    Data = null,
                    Message = "WMI Event Listener failed to start",
                    Details = "An error occurred while starting the WMI event listener"
                });

                return false;
            }
        }

        private bool WmiClassExists(string scope, string className)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher(scope, $"SELECT * FROM meta_class WHERE __Class = '{className}'"))
                {
                    return searcher.Get().Count > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Stops the WMI event listener and releases resources.
        /// </summary>
        public void StopWmiEventListener()
        {
            if (_wmiWatcher != null)
            {
                try
                {
                    _wmiWatcher.Stop();
                    _wmiWatcher.Dispose();
                    _wmiWatcher = null;

                    OnWmiEventReceived(new WmiEventArgs
                    {
                        Type = "WMI_CLOSE",
                        Data = null,
                        Message = "WMI Event Listener Closed",
                        Details = "The event listener has been stopped"
                    });
                }
                catch { }
            }
        }

        /// <summary>
        /// Starts monitoring keyboard status (CapsLock/NumLock) in a background thread.
        /// </summary>
        /// <remarks>
        /// Subscribe to <see cref="KeyStatusChanged"/> before calling this method.
        /// Polls keyboard state every 100ms and fires event on change.
        /// </remarks>
        public void StartKeyMonitor()
        {
            if (_isMonitoring) return;

            _isMonitoring = true;
            _keyMonitorThread = new Thread(MonitorKeyStatus)
            {
                IsBackground = true
            };
            _keyMonitorThread.Start();
        }

        /// <summary>
        /// Stops the keyboard status monitoring thread.
        /// </summary>
        public void StopKeyMonitor()
        {
            _isMonitoring = false;
            _keyMonitorThread?.Join(500);
            _keyMonitorThread = null;
        }

        /// <summary>
        /// Gets the current CapsLock and NumLock status.
        /// </summary>
        /// <returns>Current keyboard status with timestamp.</returns>
        /// <remarks>
        /// <b>Tauri:</b> Use for one-time status check without starting the monitor.
        /// <code>
        /// #[tauri::command]
        /// fn get_key_status() -> KeyStatus {
        ///     let status = monitor.GetKeyStatus();
        ///     KeyStatus { caps_lock: status.CapsLock, num_lock: status.NumLock }
        /// }
        /// </code>
        /// </remarks>
        public KeyStatusEventArgs GetKeyStatus()
        {
            return new KeyStatusEventArgs
            {
                CapsLock = (GetKeyState(VK_CAPITAL) & 1) != 0,
                NumLock = (GetKeyState(VK_NUMLOCK) & 1) != 0,
                Timestamp = DateTime.Now
            };
        }

        private void MonitorKeyStatus()
        {
            Thread.Sleep(200);

            _lastCapsLockStatus = (GetKeyState(VK_CAPITAL) & 1) != 0;
            _lastNumLockStatus = (GetKeyState(VK_NUMLOCK) & 1) != 0;

            while (_isMonitoring)
            {
                try
                {
                    bool isCapsLockOn = (GetKeyState(VK_CAPITAL) & 1) != 0;
                    bool isNumLockOn = (GetKeyState(VK_NUMLOCK) & 1) != 0;

                    if (isCapsLockOn != _lastCapsLockStatus || isNumLockOn != _lastNumLockStatus)
                    {
                        OnKeyStatusChanged(new KeyStatusEventArgs
                        {
                            CapsLock = isCapsLockOn,
                            NumLock = isNumLockOn,
                            Timestamp = DateTime.Now
                        });

                        _lastCapsLockStatus = isCapsLockOn;
                        _lastNumLockStatus = isNumLockOn;
                    }
                }
                catch { }

                Thread.Sleep(100);
            }
        }

        protected virtual void OnWmiEventReceived(WmiEventArgs e)
        {
            WmiEventReceived?.Invoke(this, e);
        }

        protected virtual void OnKeyStatusChanged(KeyStatusEventArgs e)
        {
            KeyStatusChanged?.Invoke(this, e);
        }

        // parse the hardware data into a custom data structure
        private Hardware[] ParseHardware(IEnumerable<IHardware> hardwareList)
        {
            return hardwareList.Select(h => new Hardware
            {
                HardwareType = h.HardwareType,
                Name = h.Name,
                SubHardware = ParseHardware(h.SubHardware),
                Sensors = parseSensors(h.Sensors)
            }).ToArray();
        }

        // parse the sensor data into a custom data structure
        private Sensor[] parseSensors(IEnumerable<ISensor> sensorList)
        {
            return sensorList.Select(s => new Sensor
            {
                SensorType = s.SensorType,
                Name = s.Name,
                Index = s.Index,
                Value = s.Value.HasValue ? s.Value.Value : 0.0f,
                Min = s.Min.HasValue ? s.Min.Value : 0.0f,
                Max = s.Max.HasValue ? s.Max.Value : 0.0f
            }).ToArray();
        }
    }

    /// <summary>
    /// Visitor class to handle updating hardware information.
    /// </summary>
    public class UpdateVisitor : IVisitor
    {
        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }
        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
        }
        public void VisitSensor(ISensor Sensor) { }
        public void VisitParameter(IParameter Parameter) { }
    }

    /// <summary>
    /// Represents a hardware component (CPU, GPU, etc.) with its sensors.
    /// </summary>
    /// <remarks>
    /// <b>Tauri Frontend TypeScript Interface:</b>
    /// <code>
    /// interface Hardware {
    ///     HardwareType: number;  // Enum: CPU=1, GPU=2, Memory=3, etc.
    ///     Name: string;
    ///     SubHardware: Hardware[];
    ///     Sensors: Sensor[];
    /// }
    /// </code>
    /// </remarks>
    public class Hardware
    {
        /// <summary>Type of hardware (CPU, GPU, Memory, Motherboard, etc.)</summary>
        public HardwareType HardwareType { get; set; }
        /// <summary>Hardware display name (e.g., "Intel Core i7-12700K")</summary>
        public string Name { get; set; }
        /// <summary>Child hardware components</summary>
        public Hardware[] SubHardware { get; set; }
        /// <summary>Sensor readings for this hardware</summary>
        public Sensor[] Sensors { get; set; }
    }

    /// <summary>
    /// Represents a sensor reading (temperature, load, voltage, etc.).
    /// </summary>
    /// <remarks>
    /// <b>Tauri Frontend TypeScript Interface:</b>
    /// <code>
    /// interface Sensor {
    ///     SensorType: number;  // Enum: Voltage=0, Clock=1, Temperature=2, Load=3, etc.
    ///     Name: string;
    ///     Index: number;
    ///     Value: number;
    ///     Min: number;
    ///     Max: number;
    /// }
    /// </code>
    /// </remarks>
    public class Sensor
    {
        /// <summary>Type of sensor (Temperature, Load, Voltage, Clock, Fan, etc.)</summary>
        public SensorType SensorType { get; set; }
        /// <summary>Sensor display name (e.g., "CPU Core #1")</summary>
        public string Name { get; set; }
        /// <summary>Sensor index within the hardware component</summary>
        public int Index { get; set; }
        /// <summary>Current sensor value</summary>
        public float Value { get; set; }
        /// <summary>Minimum recorded value since monitoring started</summary>
        public float Min { get; set; }
        /// <summary>Maximum recorded value since monitoring started</summary>
        public float Max { get; set; }
    }
}