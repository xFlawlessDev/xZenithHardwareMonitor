# xZenith Hardware Monitor Integration Guide

This guide covers integrating xZenithHardwareMonitorAPI with various programming languages and frameworks.

## Table of Contents

- [Rust / Tauri](#rust--tauri)
- [C / C++](#c--c)
- [Python](#python)
- [Node.js](#nodejs)
- [C# / .NET](#c--net)
- [Troubleshooting](#troubleshooting)

---

## Rust / Tauri

### Setup

1. Copy DLLs to your Tauri project's resource directory
2. Configure `tauri.conf.json` to bundle the DLLs
3. Create Rust bindings

### Rust Bindings

```rust
use std::ffi::{c_char, c_void, CStr};
use std::sync::Mutex;

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

unsafe impl Send for HardwareMonitor {}
unsafe impl Sync for HardwareMonitor {}

impl HardwareMonitor {
    pub fn new() -> Result<Self, &'static str> {
        let instance = unsafe { CreateHardwareMonitor() };
        if instance.is_null() {
            return Err("Failed to create hardware monitor. Run as Administrator.");
        }
        Ok(Self { instance })
    }

    pub fn update(&self) {
        unsafe { UpdateHardwareMonitor(self.instance) }
    }

    /// Get required buffer size for the report
    pub fn get_report_size(&self) -> usize {
        unsafe { GetReportSize(self.instance) as usize }
    }

    /// Get hardware report with dynamic buffer allocation
    pub fn get_report(&self) -> String {
        unsafe {
            // Get required size, with minimum of 128KB for safety
            let size = (GetReportSize(self.instance) as usize).max(131072);
            let mut buffer = vec![0u8; size];
            
            GetReport(
                self.instance,
                buffer.as_mut_ptr() as *mut c_char,
                buffer.len() as i32,
            );
            CStr::from_ptr(buffer.as_ptr() as *const c_char)
                .to_string_lossy()
                .into_owned()
        }
    }

    pub fn get_report_json(&self) -> Result<serde_json::Value, serde_json::Error> {
        let report = self.get_report();
        serde_json::from_str(&report)
    }
}

impl Drop for HardwareMonitor {
    fn drop(&mut self) {
        unsafe { DestroyHardwareMonitor(self.instance) }
    }
}
```

### Tauri Command

```rust
use std::sync::Mutex;
use tauri::State;

pub struct MonitorState(pub Mutex<Option<HardwareMonitor>>);

#[tauri::command]
pub fn init_monitor(state: State<MonitorState>) -> Result<(), String> {
    let monitor = HardwareMonitor::new().map_err(|e| e.to_string())?;
    *state.0.lock().unwrap() = Some(monitor);
    Ok(())
}

#[tauri::command]
pub fn get_hardware_data(state: State<MonitorState>) -> Result<String, String> {
    let guard = state.0.lock().unwrap();
    match &*guard {
        Some(monitor) => {
            monitor.update();
            Ok(monitor.get_report())
        }
        None => Err("Monitor not initialized".to_string()),
    }
}
```

### tauri.conf.json

```json
{
  "tauri": {
    "bundle": {
      "resources": [
        "resources/ManagedxZenithHardwareMonitorWrapper.dll",
        "resources/ManagedxZenithHardwareMonitor.dll",
        "resources/xZenithHardwareMonitorLib.dll",
        "resources/Newtonsoft.Json.dll"
      ]
    }
  }
}
```

---

## C / C++

### Dynamic Loading (Recommended)

```cpp
#include <windows.h>
#include <iostream>
#include <string>

class HardwareMonitor {
private:
    HMODULE dll;
    void* instance;
    
    typedef void* (*CreateFunc)();
    typedef void (*UpdateFunc)(void*);
    typedef void (*GetReportFunc)(void*, char*, int);
    typedef void (*DestroyFunc)(void*);
    
    CreateFunc create;
    UpdateFunc update;
    GetReportFunc getReport;
    DestroyFunc destroy;

public:
    HardwareMonitor() : dll(nullptr), instance(nullptr) {
        dll = LoadLibraryW(L"ManagedxZenithHardwareMonitorWrapper.dll");
        if (!dll) throw std::runtime_error("Failed to load DLL");
        
        create = (CreateFunc)GetProcAddress(dll, "CreateHardwareMonitor");
        update = (UpdateFunc)GetProcAddress(dll, "UpdateHardwareMonitor");
        getReport = (GetReportFunc)GetProcAddress(dll, "GetReport");
        destroy = (DestroyFunc)GetProcAddress(dll, "DestroyHardwareMonitor");
        
        instance = create();
        if (!instance) throw std::runtime_error("Failed to create monitor");
    }
    
    ~HardwareMonitor() {
        if (instance) destroy(instance);
        if (dll) FreeLibrary(dll);
    }
    
    void Update() {
        update(instance);
    }
    
    std::string GetReport() {
        char buffer[65536];
        getReport(instance, buffer, sizeof(buffer));
        return std::string(buffer);
    }
};

int main() {
    try {
        HardwareMonitor monitor;
        
        for (int i = 0; i < 5; i++) {
            monitor.Update();
            std::cout << monitor.GetReport() << std::endl;
            Sleep(1000);
        }
    } catch (const std::exception& e) {
        std::cerr << "Error: " << e.what() << std::endl;
        return 1;
    }
    return 0;
}
```

### Static Linking

```cpp
#pragma comment(lib, "ManagedxZenithHardwareMonitorWrapper.lib")

extern "C" {
    void* CreateHardwareMonitor();
    void UpdateHardwareMonitor(void* instance);
    void GetReport(void* instance, char* buffer, int bufferSize);
    void DestroyHardwareMonitor(void* instance);
}
```

---

## Python

### Using ctypes

```python
import ctypes
import json
from typing import Optional, Dict, List, Any

class HardwareMonitor:
    def __init__(self, dll_path: str = "ManagedxZenithHardwareMonitorWrapper.dll"):
        self.dll = ctypes.CDLL(dll_path)
        
        # Define function signatures
        self.dll.CreateHardwareMonitor.restype = ctypes.c_void_p
        self.dll.CreateHardwareMonitor.argtypes = []
        
        self.dll.UpdateHardwareMonitor.restype = None
        self.dll.UpdateHardwareMonitor.argtypes = [ctypes.c_void_p]
        
        self.dll.GetReport.restype = None
        self.dll.GetReport.argtypes = [ctypes.c_void_p, ctypes.c_char_p, ctypes.c_int]
        
        self.dll.GetReportSize.restype = ctypes.c_int
        self.dll.GetReportSize.argtypes = [ctypes.c_void_p]
        
        self.dll.DestroyHardwareMonitor.restype = None
        self.dll.DestroyHardwareMonitor.argtypes = [ctypes.c_void_p]
        
        # Create instance
        self.instance = self.dll.CreateHardwareMonitor()
        if not self.instance:
            raise RuntimeError("Failed to create hardware monitor. Run as Administrator.")
    
    def update(self) -> None:
        self.dll.UpdateHardwareMonitor(self.instance)
    
    def get_report_size(self) -> int:
        return self.dll.GetReportSize(self.instance)
    
    def get_report(self) -> str:
        # Get required size with minimum of 128KB
        size = max(self.get_report_size(), 131072)
        buffer = ctypes.create_string_buffer(size)
        self.dll.GetReport(self.instance, buffer, size)
        return buffer.value.decode('utf-8')
    
    def get_report_json(self) -> List[Dict[str, Any]]:
        return json.loads(self.get_report())
    
    def get_cpu_temperature(self) -> Optional[float]:
        data = self.get_report_json()
        for hw in data:
            if hw['HardwareType'] == 'Cpu':
                for sensor in hw['Sensors']:
                    if sensor['SensorType'] == 'Temperature':
                        return sensor['Value']
        return None
    
    def __del__(self):
        if hasattr(self, 'instance') and self.instance:
            self.dll.DestroyHardwareMonitor(self.instance)

# Usage
if __name__ == "__main__":
    import time
    
    monitor = HardwareMonitor()
    
    for _ in range(5):
        monitor.update()
        temp = monitor.get_cpu_temperature()
        print(f"CPU Temperature: {temp}C")
        time.sleep(1)
```

---

## Node.js

### Using node-ffi-napi

```javascript
const ffi = require('ffi-napi');
const ref = require('ref-napi');

const voidPtr = ref.refType(ref.types.void);

const lib = ffi.Library('ManagedxZenithHardwareMonitorWrapper', {
    'CreateHardwareMonitor': [voidPtr, []],
    'UpdateHardwareMonitor': ['void', [voidPtr]],
    'GetReport': ['void', [voidPtr, 'char *', 'int']],
    'DestroyHardwareMonitor': ['void', [voidPtr]]
});

class HardwareMonitor {
    constructor() {
        this.instance = lib.CreateHardwareMonitor();
        if (this.instance.isNull()) {
            throw new Error('Failed to create hardware monitor. Run as Administrator.');
        }
    }

    update() {
        lib.UpdateHardwareMonitor(this.instance);
    }

    getReport() {
        const buffer = Buffer.alloc(65536);
        lib.GetReport(this.instance, buffer, 65536);
        return buffer.toString('utf8').replace(/\0/g, '');
    }

    getReportJson() {
        return JSON.parse(this.getReport());
    }

    destroy() {
        if (this.instance) {
            lib.DestroyHardwareMonitor(this.instance);
            this.instance = null;
        }
    }
}

// Usage
const monitor = new HardwareMonitor();

setInterval(() => {
    monitor.update();
    const data = monitor.getReportJson();
    console.log(JSON.stringify(data, null, 2));
}, 1000);

process.on('exit', () => monitor.destroy());
```

---

## C# / .NET

### Direct Reference

```csharp
using ManagedxZenithHardwareMonitor;
using Newtonsoft.Json;
using System;

class Program
{
    static void Main()
    {
        var monitor = new HardwareMonitor();
        
        for (int i = 0; i < 5; i++)
        {
            monitor.Update();
            string report = monitor.GetReport();
            
            var hardware = JsonConvert.DeserializeObject<Hardware[]>(report);
            
            foreach (var hw in hardware)
            {
                Console.WriteLine($"{hw.HardwareType}: {hw.Name}");
                foreach (var sensor in hw.Sensors)
                {
                    Console.WriteLine($"  {sensor.SensorType} - {sensor.Name}: {sensor.Value}");
                }
            }
            
            System.Threading.Thread.Sleep(1000);
        }
    }
}
```

---

## Troubleshooting

### Common Issues

#### "Failed to load DLL"

1. Ensure all DLLs are in the same directory
2. Install Visual C++ Redistributable
3. Install .NET Framework 4.7.2

#### "Failed to create hardware monitor"

1. Run application as Administrator
2. Check Windows Event Viewer for errors
3. Verify .NET Framework installation

#### "No hardware detected"

1. Some hardware requires specific drivers
2. Virtual machines have limited hardware access
3. Check if Windows recognizes the hardware

#### "Access denied"

1. Ensure Administrator privileges
2. Disable antivirus temporarily for testing
3. Check Windows Security settings

### Debug Logging

Enable verbose logging by checking Windows Event Viewer:
- Application Log
- System Log

### Platform Requirements

| Requirement | Version |
|-------------|---------|
| Windows | 10/11 (x64) |
| .NET Framework | 4.7.2+ |
| Visual C++ Runtime | 2015-2022 |
| Administrator Rights | Required |

### Performance Tips

1. **Caching**: Cache hardware list, update sensors only
2. **Polling Rate**: Don't poll faster than 500ms
3. **Selective Monitoring**: Disable unused hardware types
4. **Memory**: Reuse buffer allocations
