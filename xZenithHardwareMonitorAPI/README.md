# xZenithHardwareMonitorAPI

[![GitHub](https://img.shields.io/badge/GitHub-xZenithhardwareMonitor-blue?logo=github)](https://github.com/xFlawlessDev/xZenithhardwareMonitor)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

A C-compatible hardware monitoring API wrapper that exposes system hardware data (CPU, GPU, Memory, Storage, etc.) as JSON. Designed for seamless integration with Tauri, Rust, C++, and other unmanaged languages.

## Features

- Real-time hardware monitoring (CPU, GPU, RAM, Motherboard, Network, Battery, Storage)
- C-compatible DLL exports for FFI integration
- JSON-serialized sensor data with min/max/current values
- Lightweight wrapper around LibreHardwareMonitor

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    Your Application                              │
│                 (Tauri, Rust, C++, etc.)                        │
└─────────────────────────┬───────────────────────────────────────┘
                          │ C API (DLL Exports)
┌─────────────────────────▼───────────────────────────────────────┐
│           ManagedxZenithHardwareMonitorWrapper                  │
│                      (C++/CLI)                                   │
│  - Bridges managed/unmanaged code                               │
│  - Exports: CreateHardwareMonitor, UpdateHardwareMonitor,       │
│             GetReport, GetReportSize, DestroyHardwareMonitor    │
└─────────────────────────┬───────────────────────────────────────┘
                          │ Managed Reference
┌─────────────────────────▼───────────────────────────────────────┐
│              ManagedxZenithHardwareMonitor                      │
│                        (C#)                                      │
│  - Hardware initialization and traversal                        │
│  - Sensor data collection via Visitor pattern                   │
│  - JSON serialization with Newtonsoft.Json                      │
└─────────────────────────┬───────────────────────────────────────┘
                          │
┌─────────────────────────▼───────────────────────────────────────┐
│                  LibreHardwareMonitor                           │
│              (xZenithHardwareMonitorLib)                        │
│  - Low-level hardware access                                    │
│  - Sensor drivers and interfaces                                │
└─────────────────────────────────────────────────────────────────┘
```

## API Reference

### C Exports

```c
// Create a new hardware monitor instance
void* CreateHardwareMonitor();

// Update all sensor readings
void UpdateHardwareMonitor(void* instance);

// Get JSON report of all hardware and sensors
void GetReport(void* instance, char* buffer, int bufferSize);

// Get required buffer size for the report (for dynamic allocation)
int GetReportSize(void* instance);

// Destroy the hardware monitor instance
void DestroyHardwareMonitor(void* instance);
```

**Buffer Size Notes:**
- Use `GetReportSize()` for dynamic buffer allocation
- Minimum recommended: 128KB when motherboard monitoring is enabled
- Buffer is always null-terminated

### JSON Output Format

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
      },
      {
        "SensorType": "Load",
        "Name": "CPU Total",
        "Index": 0,
        "Value": 12.5,
        "Min": 0.0,
        "Max": 100.0
      }
    ],
    "SubHardware": []
  },
  {
    "HardwareType": "GpuNvidia",
    "Name": "NVIDIA GeForce RTX 3080",
    "Sensors": [
      {
        "SensorType": "Temperature",
        "Name": "GPU Core",
        "Index": 0,
        "Value": 52.0,
        "Min": 35.0,
        "Max": 83.0
      }
    ],
    "SubHardware": []
  }
]
```

### Sensor Types

| Type          | Description                |
| ------------- | -------------------------- |
| `Temperature` | Temperature in Celsius     |
| `Load`        | Utilization percentage     |
| `Clock`       | Frequency in MHz           |
| `Voltage`     | Voltage in Volts           |
| `Fan`         | Fan speed in RPM           |
| `Power`       | Power consumption in Watts |
| `Data`        | Data amount in GB          |
| `Throughput`  | Transfer rate in MB/s      |

### Hardware Types

| Type          | Description      |
| ------------- | ---------------- |
| `Cpu`         | Processor        |
| `GpuNvidia`   | NVIDIA GPU       |
| `GpuAmd`      | AMD GPU          |
| `GpuIntel`    | Intel GPU        |
| `Memory`      | System RAM       |
| `Motherboard` | Mainboard        |
| `Storage`     | HDDs/SSDs/NVMe   |
| `Network`     | Network adapters |
| `Battery`     | Laptop battery   |

## Usage Examples

### Rust (with Tauri)

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

    pub fn update(&self) {
        unsafe { UpdateHardwareMonitor(self.instance) }
    }

    pub fn get_report(&self) -> String {
        unsafe {
            // Dynamic buffer with 128KB minimum for motherboard data
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

### C++

```cpp
#include <iostream>
#include <windows.h>

typedef void* (*CreateHardwareMonitorFunc)();
typedef void (*UpdateHardwareMonitorFunc)(void*);
typedef void (*GetReportFunc)(void*, char*, int);
typedef void (*DestroyHardwareMonitorFunc)(void*);

int main() {
    HMODULE dll = LoadLibrary(L"ManagedxZenithHardwareMonitorWrapper.dll");

    auto create = (CreateHardwareMonitorFunc)GetProcAddress(dll, "CreateHardwareMonitor");
    auto update = (UpdateHardwareMonitorFunc)GetProcAddress(dll, "UpdateHardwareMonitor");
    auto getReport = (GetReportFunc)GetProcAddress(dll, "GetReport");
    auto destroy = (DestroyHardwareMonitorFunc)GetProcAddress(dll, "DestroyHardwareMonitor");

    void* monitor = create();
    char buffer[65536];

    update(monitor);
    getReport(monitor, buffer, sizeof(buffer));
    std::cout << buffer << std::endl;

    destroy(monitor);
    FreeLibrary(dll);
    return 0;
}
```

## Building

### Requirements

- Visual Studio 2017 or later
- .NET Framework 4.7.2
- Windows SDK

### Build Steps

1. Open `xZenithHardwareMonitorAPI.sln` in Visual Studio
2. Select configuration (Release/x64 recommended)
3. Build Solution (Ctrl+Shift+B)

### Output Files

After building, copy these files to your application:

```
x64/Release/
├── ManagedxZenithHardwareMonitorWrapper.dll  (C API wrapper)
├── ManagedxZenithHardwareMonitor.dll         (Managed core)
├── xZenithHardwareMonitorLib.dll             (LibreHardwareMonitor)
└── Newtonsoft.Json.dll                       (JSON serialization)
```

## Requirements

- Windows 10/11
- .NET Framework 4.7.2 runtime
- Administrator privileges (required for hardware sensor access)

## Acknowledgments

This project builds upon the excellent work of:

- **[LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor)** - The core hardware monitoring library that powers this wrapper. LibreHardwareMonitor is a free, open-source software that monitors temperature sensors, fan speeds, voltages, load, and clock speeds of computer hardware.

- **[corroded-monitor](https://github.com/chanderlud/corroded-monitor)** - Inspiration for the Rust FFI integration approach and architecture patterns for exposing hardware data to Rust applications.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

LibreHardwareMonitor is licensed under the Mozilla Public License 2.0.
