# xZenith Hardware Monitor

[![GitHub](https://img.shields.io/badge/GitHub-xZenithHardwareMonitor-blue?logo=github)](https://github.com/xFlawlessDev/xZenithhardwareMonitor)
[![License](https://img.shields.io/badge/License-MIT%20%2F%20MPL--2.0-green)](LICENSE)

A comprehensive hardware monitoring solution for Windows applications, designed for seamless integration with Tauri, Rust, C++, and other languages through a C-compatible API.

## Overview

xZenith Hardware Monitor is a two-component system that provides real-time hardware telemetry data:

| Component                     | Description                                                     | Technology |
| ----------------------------- | --------------------------------------------------------------- | ---------- |
| **LibreHardwareMonitorLib**   | Core hardware monitoring library (enhanced fork of LibreHardwareMonitor) | C# / .NET  |
| **xZenithHardwareMonitorAPI** | C-compatible DLL wrapper for FFI integration                    | C++/CLI    |

## Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                       Your Application                               │
│                   (Tauri, Rust, C++, Python, etc.)                  │
└─────────────────────────────┬───────────────────────────────────────┘
                              │ C API (DLL Exports)
┌─────────────────────────────▼───────────────────────────────────────┐
│                   xZenithHardwareMonitorAPI                          │
│  ┌─────────────────────────────────────────────────────────────┐    │
│  │         ManagedxZenithHardwareMonitorWrapper (C++/CLI)       │    │
│  │  Exports: CreateHardwareMonitor, UpdateHardwareMonitor,      │    │
│  │           GetReport, DestroyHardwareMonitor                  │    │
│  └─────────────────────────────┬───────────────────────────────┘    │
│                                │                                     │
│  ┌─────────────────────────────▼───────────────────────────────┐    │
│  │           ManagedxZenithHardwareMonitor (C#)                 │    │
│  │  - Hardware initialization via Visitor pattern               │    │
│  │  - Sensor data collection and JSON serialization            │    │
│  └─────────────────────────────────────────────────────────────┘    │
└─────────────────────────────┬───────────────────────────────────────┘
                              │
┌─────────────────────────────▼───────────────────────────────────────┐
│                   LibreHardwareMonitorLib                            │
│  - Low-level hardware access via kernel drivers                     │
│  - Sensor drivers for CPU, GPU, Memory, Storage, etc.              │
│  - Enhanced fork with DiskInfoToolkit, RAMSPDToolkit integration    │
└─────────────────────────────────────────────────────────────────────┘
```

## Supported Hardware

| Category        | Supported Devices                                       |
| --------------- | ------------------------------------------------------- |
| **CPU**         | Intel, AMD processors (temperature, load, clock, power, VID, C-states) |
| **GPU**         | NVIDIA, AMD, Intel (including Arc discrete GPUs)        |
| **Memory**      | System RAM + DIMM SPD data (DDR4/DDR5 timing parameters) |
| **Motherboard** | Various manufacturers (voltages, temperatures, fans)    |
| **Storage**     | HDD, SSD, NVMe drives (CrystalDiskInfo-level monitoring) |
| **Network**     | Network adapters (throughput, bandwidth)                |
| **Battery**     | Laptop batteries (charge, health, voltage)              |
| **EC**          | Embedded Controllers (ASUS motherboards, ChromeOS)      |

## Sensor Types

| Type          | Unit       | Description            |
| ------------- | ---------- | ---------------------- |
| `Temperature` | Celsius    | Component temperatures |
| `Load`        | Percentage | Utilization levels     |
| `Clock`       | MHz        | Operating frequencies  |
| `Voltage`     | Volts      | Voltage readings       |
| `Fan`         | RPM        | Fan speeds             |
| `Power`       | Watts      | Power consumption      |
| `Data`        | GB         | Data amounts           |
| `SmallData`   | MB         | Small data amounts     |
| `Throughput`  | MB/s       | Transfer rates         |
| `TimeSpan`    | Seconds    | Time durations         |
| `Timing`      | ns         | Timing parameters      |
| `Level`       | Percentage | Fill/health levels     |
| `Factor`      | Count      | Multipliers/counts     |
| `Flow`        | L/h        | Liquid flow rates      |
| `Current`     | Amps       | Current readings       |

## Quick Start

### C API Functions

```c
// Create a new hardware monitor instance
void* CreateHardwareMonitor();

// Update all sensor readings
void UpdateHardwareMonitor(void* instance);

// Get JSON report of all hardware and sensors
void GetReport(void* instance, char* buffer, int bufferSize);

// Get required buffer size for the report (use for dynamic allocation)
int GetReportSize(void* instance);

// Destroy the hardware monitor instance
void DestroyHardwareMonitor(void* instance);
```

### JSON Output Example

```json
[
  {
    "HardwareType": "Cpu",
    "Name": "AMD Ryzen 9 5900X",
    "Sensors": [
      {
        "SensorType": "Temperature",
        "Name": "Core (Tctl/Tdie)",
        "Index": 0,
        "Value": 45.0,
        "Min": 32.0,
        "Max": 78.5
      }
    ],
    "SubHardware": []
  }
]
```

### Rust Integration (Tauri)

```rust
use std::ffi::{c_char, c_void, CStr};

#[link(name = "ManagedxZenithHardwareMonitorWrapper")]
extern "C" {
    fn CreateHardwareMonitor() -> *mut c_void;
    fn UpdateHardwareMonitor(instance: *mut c_void);
    fn GetReport(instance: *mut c_void, buffer: *mut c_char, buffer_size: i32);
    fn GetReportSize(instance: *mut c_void) -> i32;
    fn DestroyHardwareMonitor(instance: *mut c_void);
}

pub struct HardwareMonitor {
    instance: *mut c_void,
}

impl HardwareMonitor {
    pub fn new() -> Self {
        unsafe { Self { instance: CreateHardwareMonitor() } }
    }

    pub fn get_report(&self) -> String {
        unsafe {
            UpdateHardwareMonitor(self.instance);
            // Dynamic buffer allocation with 128KB minimum
            let size = (GetReportSize(self.instance) as usize).max(131072);
            let mut buffer = vec![0u8; size];
            GetReport(self.instance, buffer.as_mut_ptr() as *mut c_char, buffer.len() as i32);
            CStr::from_ptr(buffer.as_ptr() as *const c_char)
                .to_string_lossy()
                .into_owned()
        }
    }
}

impl Drop for HardwareMonitor {
    fn drop(&mut self) {
        unsafe { DestroyHardwareMonitor(self.instance) }
    }
}
```

## Building

### Requirements

- Windows 10/11
- Visual Studio 2017 or later
- .NET Framework 4.7.2
- Windows SDK

### Build Steps

1. **Build LibreHardwareMonitorLib first:**

   ```
   cd LibreHardwareMonitorLib
   Open LibreHardwareMonitor.sln in Visual Studio
   Build -> Build Solution (Release/x64)
   ```

2. **Build xZenithHardwareMonitorAPI:**
   ```
   cd xZenithHardwareMonitorAPI
   Open xZenithHardwareMonitorAPI.sln in Visual Studio
   Build -> Build Solution (Release/x64)
   ```
   
3. **Copy dependencies** from `LibreHardwareMonitorLib/bin/Release/x64/net472/` to API output directory.

### Output Files

Copy these files to your application directory:

```
xZenithHardwareMonitorAPI/x64/Release/
├── ManagedxZenithHardwareMonitorWrapper.dll  (C API wrapper)
├── ManagedxZenithHardwareMonitor.dll         (Managed core)
├── LibreHardwareMonitorLib.dll               (Hardware monitoring library)
├── DiskInfoToolkit.dll                       (Storage monitoring)
├── HidSharp.dll                              (HID device access)
├── RAMSPDToolkit-NDD.dll                     (Memory SPD data)
├── BlackSharp.Core.dll                       (Core utilities)
├── Newtonsoft.Json.dll                       (JSON serialization)
├── System.Memory.dll                         (Runtime dependency)
├── System.Buffers.dll                        (Runtime dependency)
├── System.Numerics.Vectors.dll               (Runtime dependency)
└── System.Runtime.CompilerServices.Unsafe.dll (Runtime dependency)
```

## Project Structure

```
xZenithhardwareMonitor/
├── README.md                          # This file
├── LICENSE                            # Project license
├── CONTRIBUTING.md                    # Contribution guidelines
├── docs/                              # Documentation
│   ├── API_REFERENCE.md               # API reference
│   ├── HARDWARE_TYPES.md              # Hardware and sensor types
│   └── INTEGRATION_GUIDE.md           # Integration guide
│
├── LibreHardwareMonitorLib/           # Core hardware monitoring library
│   ├── LibreHardwareMonitor.sln       # Solution file
│   ├── LibreHardwareMonitorLib/       # Library project
│   │   └── Hardware/                  # Hardware sensor implementations
│   │       ├── Cpu/
│   │       ├── Gpu/
│   │       ├── Memory/
│   │       ├── Storage/
│   │       └── ...
│   ├── LibreHardwareMonitor/          # GUI application
│   ├── Aga.Controls/                  # UI controls
│   └── Licenses/                      # Third-party licenses
│
└── xZenithHardwareMonitorAPI/         # C-compatible API wrapper
    ├── xZenithHardwareMonitorAPI.sln  # Solution file
    ├── ManagedxZenithHardwareMonitor/ # C# managed wrapper
    └── ManagedxZenithHardwareMonitorWrapper/ # C++/CLI exports
```

## Requirements

- **Runtime:** .NET Framework 4.7.2
- **Platform:** Windows 10/11 (x64)
- **Privileges:** Administrator rights required for hardware sensor access

## Documentation

- [API Reference](./docs/API_REFERENCE.md) - API functions and usage
- [Hardware Types](./docs/HARDWARE_TYPES.md) - Supported hardware and sensors
- [Integration Guide](./docs/INTEGRATION_GUIDE.md) - Step-by-step integration

## Acknowledgments

This project builds upon excellent open-source work:

- **[LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor)** - The foundation hardware monitoring library (MPL-2.0)
- **[DiskInfoToolkit](https://github.com/)** - CrystalDiskInfo-level storage monitoring
- **[RAMSPDToolkit](https://github.com/)** - Memory SPD/timing data access
- **[HidSharp](https://github.com/IntergatedCircuits/HidSharp)** - HID device communication
- **[corroded-monitor](https://github.com/chanderlud/corroded-monitor)** - Inspiration for Rust FFI integration patterns

## License

This project is dual-licensed:

- **xZenithHardwareMonitorAPI** - MIT License
- **LibreHardwareMonitorLib** - Mozilla Public License 2.0 (MPL-2.0)

See the [LICENSE](LICENSE) file for details.

---

**Note:** Administrator privileges are required to access low-level hardware sensors. Run your application as Administrator or add an [app.manifest](https://learn.microsoft.com/en-us/windows/win32/sbscs/application-manifests) with `requestedExecutionLevel` set to `requireAdministrator`.
