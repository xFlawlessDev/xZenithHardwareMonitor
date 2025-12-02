# xZenith Hardware Monitor API Reference

Complete reference for the C-compatible API exported by `ManagedxZenithHardwareMonitorWrapper.dll`.

## Table of Contents

- [Initialization](#initialization)
- [Core Functions](#core-functions)
- [WMI Event Functions](#wmi-event-functions)
- [Key Status Functions](#key-status-functions)
- [Data Structures](#data-structures)
- [TypeScript Interfaces](#typescript-interfaces)
- [Sensor Types](#sensor-types)
- [Hardware Types](#hardware-types)
- [Error Handling](#error-handling)

---

## Initialization

Before using any API functions, ensure:

1. All required DLLs are in the application directory
2. Application has administrator privileges
3. .NET Framework 4.7.2 runtime is installed

### Required Files

```
ManagedxZenithHardwareMonitorWrapper.dll  (C API)
ManagedxZenithHardwareMonitor.dll         (Managed layer)
xZenithHardwareMonitorLib.dll             (Core library)
Newtonsoft.Json.dll                       (JSON serialization)
```

---

## Core Functions

### CreateHardwareMonitor

Creates and initializes a new hardware monitor instance.

```c
void* CreateHardwareMonitor();
```

**Returns:** Opaque pointer to the hardware monitor instance, or `NULL` on failure.

**Notes:**
- Opens connections to all hardware sensors
- Enables CPU, GPU, Memory, Motherboard, Network, Battery, and Storage monitoring
- Controller monitoring is disabled by default

**Example:**
```c
void* monitor = CreateHardwareMonitor();
if (monitor == NULL) {
    // Handle initialization error
}
```

---

### UpdateHardwareMonitor

Updates all sensor readings with current values.

```c
void UpdateHardwareMonitor(void* instance);
```

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `instance` | `void*` | Pointer returned by `CreateHardwareMonitor` |

**Notes:**
- Call this before `GetReport` to get fresh data
- Recommended polling interval: 500ms - 2000ms
- Calling too frequently may impact performance

**Example:**
```c
UpdateHardwareMonitor(monitor);
```

---

### GetReport

Retrieves a JSON-formatted report of all hardware and sensor data.

```c
void GetReport(void* instance, char* buffer, int bufferSize);
```

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `instance` | `void*` | Pointer returned by `CreateHardwareMonitor` |
| `buffer` | `char*` | Pre-allocated buffer for JSON output |
| `bufferSize` | `int` | Size of the buffer in bytes |

**Notes:**
- Buffer is always null-terminated (even if truncated)
- Use `GetReportSize` to determine required buffer size
- Recommended minimum: 131072 bytes (128KB) when motherboard monitoring is enabled

**Buffer Size Guidelines:**
| Configuration | Recommended Size |
|---------------|------------------|
| CPU + GPU + Memory only | 65536 (64KB) |
| With Storage + Network | 65536 (64KB) |
| With Motherboard/SuperIO | 131072 (128KB) |
| All hardware enabled | 262144 (256KB) |

**Example:**
```c
// Option 1: Fixed large buffer
char buffer[131072];
GetReport(monitor, buffer, sizeof(buffer));
printf("%s\n", buffer);

// Option 2: Dynamic buffer (recommended)
int size = GetReportSize(monitor);
char* buffer = (char*)malloc(size);
GetReport(monitor, buffer, size);
printf("%s\n", buffer);
free(buffer);
```

---

### GetReportSize

Returns the required buffer size for the current hardware report.

```c
int GetReportSize(void* instance);
```

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `instance` | `void*` | Pointer returned by `CreateHardwareMonitor` |

**Returns:** Required buffer size in bytes (includes null terminator).

**Notes:**
- Call `UpdateHardwareMonitor` before this to get accurate size
- Size varies based on enabled hardware and number of sensors
- Use this for dynamic buffer allocation

**Example:**
```c
UpdateHardwareMonitor(monitor);
int requiredSize = GetReportSize(monitor);
char* buffer = (char*)malloc(requiredSize);
GetReport(monitor, buffer, requiredSize);
// ... use buffer ...
free(buffer);
```

---

### DestroyHardwareMonitor

Destroys the hardware monitor instance and releases resources.

```c
void DestroyHardwareMonitor(void* instance);
```

**Parameters:**
| Name | Type | Description |
|------|------|-------------|
| `instance` | `void*` | Pointer returned by `CreateHardwareMonitor` |

**Notes:**
- Always call this to prevent memory leaks
- Do not use the instance pointer after calling this function

**Example:**
```c
DestroyHardwareMonitor(monitor);
monitor = NULL;
```

---

## WMI Event Functions

Listen for system WMI events (IP3_WMIEvent). Only available on certain OEM devices.

### Functions

| Function | Signature | Description |
|----------|-----------|-------------|
| `StartWmiEventListener` | `bool (void*)` | Start listening. Returns `false` if unavailable |
| `StopWmiEventListener` | `void (void*)` | Stop listening |
| `PollWmiEvent` | `bool (void*, char*, int)` | Get next event. Returns `true` if event available |

### Event Types

| Type | Description |
|------|-------------|
| `WMI_TEST` | Listener started successfully |
| `WMI_EVENT` | System event received |
| `WMI_CLOSE` | Listener stopped |
| `WMI_UNAVAILABLE` | Not supported on this device |

### Event JSON Format

```json
{"type":"WMI_EVENT","data":[1,2,3],"message":"WMI Event received","details":"..."}
```

### Usage

```c
// Start listener
bool success = StartWmiEventListener(monitor);
if (!success) {
    // WMI not available on this device
}

// Poll for events (call in loop, ~50ms interval)
char buffer[4096];
if (PollWmiEvent(monitor, buffer, sizeof(buffer))) {
    // Process event JSON in buffer
}

// Stop when done
StopWmiEventListener(monitor);
```

---

## Key Status Functions

Monitor CapsLock and NumLock keyboard status.

### Functions

| Function | Signature | Description |
|----------|-----------|-------------|
| `StartKeyMonitor` | `void (void*)` | Start monitoring (fires events on change) |
| `StopKeyMonitor` | `void (void*)` | Stop monitoring |
| `GetKeyStatus` | `void (void*, char*, int)` | Get current status (one-time check) |

### Status JSON Format

```json
{"caps_lock":true,"num_lock":false,"timestamp":"2024-01-15T10:30:00Z"}
```

### Usage

```c
// One-time status check
char buffer[256];
GetKeyStatus(monitor, buffer, sizeof(buffer));

// Or continuous monitoring (poll every ~100ms)
StartKeyMonitor(monitor);
// ... poll with GetKeyStatus() ...
StopKeyMonitor(monitor);
```

---

## Data Structures

### JSON Output Format

```json
[
  {
    "HardwareType": "string",
    "Name": "string",
    "Sensors": [
      {
        "SensorType": "string",
        "Name": "string",
        "Index": 0,
        "Value": 0.0,
        "Min": 0.0,
        "Max": 0.0
      }
    ],
    "SubHardware": []
  }
]
```

### Hardware Object

| Field | Type | Description |
|-------|------|-------------|
| `HardwareType` | string | Type of hardware (see Hardware Types) |
| `Name` | string | Display name of the hardware |
| `Sensors` | array | Array of Sensor objects |
| `SubHardware` | array | Nested Hardware objects |

### Sensor Object

| Field | Type | Description |
|-------|------|-------------|
| `SensorType` | string | Type of sensor (see Sensor Types) |
| `Name` | string | Display name of the sensor |
| `Index` | int | Sensor index (for multiple sensors of same type) |
| `Value` | float | Current sensor value |
| `Min` | float | Minimum recorded value |
| `Max` | float | Maximum recorded value |

---

## TypeScript Interfaces

```typescript
// WMI Event
interface WmiEvent {
    type: 'WMI_EVENT' | 'WMI_TEST' | 'WMI_CLOSE' | 'WMI_UNAVAILABLE';
    data: number[] | null;
    message: string;
    details: string;
}

// Key Status
interface KeyStatus {
    caps_lock: boolean;
    num_lock: boolean;
    timestamp: string;  // ISO 8601
}
```

---

## Sensor Types

| Type | Unit | Description |
|------|------|-------------|
| `Voltage` | V | Voltage readings |
| `Current` | A | Current readings |
| `Power` | W | Power consumption |
| `Clock` | MHz | Clock frequencies |
| `Temperature` | C | Temperatures |
| `Load` | % | Utilization percentages |
| `Frequency` | Hz | Generic frequencies |
| `Fan` | RPM | Fan speeds |
| `Flow` | L/h | Liquid flow rates |
| `Control` | % | Control levels |
| `Level` | % | Generic levels |
| `Factor` | 1 | Multiplication factors |
| `Data` | GB | Data amounts |
| `SmallData` | MB | Small data amounts |
| `Throughput` | B/s | Transfer rates |
| `TimeSpan` | s | Time durations |
| `Energy` | mWh | Energy consumption |
| `Noise` | dBA | Noise levels |

---

## Hardware Types

| Type | Description |
|------|-------------|
| `Motherboard` | System motherboard |
| `SuperIO` | Super I/O chip |
| `Cpu` | Central processor |
| `Memory` | System RAM |
| `GpuNvidia` | NVIDIA graphics card |
| `GpuAmd` | AMD graphics card |
| `GpuIntel` | Intel integrated/discrete graphics |
| `Storage` | HDD/SSD/NVMe drives |
| `Network` | Network adapters |
| `Cooler` | Cooling devices |
| `EmbeddedController` | Embedded controller |
| `Psu` | Power supply unit |
| `Battery` | Laptop battery |

---

## Error Handling

The API does not provide explicit error codes. Handle potential issues:

1. **CreateHardwareMonitor returns NULL**
   - Insufficient permissions (run as Administrator)
   - Missing DLL dependencies
   - .NET Framework not installed

2. **GetReport returns empty/incomplete JSON**
   - Hardware not detected
   - Driver issues
   - Sensor access denied

3. **Application crash**
   - Invalid instance pointer
   - Buffer overflow
   - Memory corruption

### Recommended Error Handling Pattern

```c
void* monitor = CreateHardwareMonitor();
if (monitor == NULL) {
    fprintf(stderr, "Failed to initialize hardware monitor\n");
    fprintf(stderr, "Ensure running as Administrator\n");
    return -1;
}

char buffer[65536];
UpdateHardwareMonitor(monitor);
GetReport(monitor, buffer, sizeof(buffer));

if (buffer[0] == '\0' || strcmp(buffer, "[]") == 0) {
    fprintf(stderr, "No hardware detected\n");
}

DestroyHardwareMonitor(monitor);
```

---

## Thread Safety

- `CreateHardwareMonitor`: Not thread-safe, create one instance per thread
- `UpdateHardwareMonitor`: Thread-safe for single instance
- `GetReport`: Thread-safe for single instance
- `DestroyHardwareMonitor`: Not thread-safe

For multi-threaded applications, use a mutex or create separate instances per thread.

---

## Performance Considerations

1. **Polling Frequency**
   - Minimum: 100ms (may cause high CPU usage)
   - Recommended: 500ms - 1000ms
   - Maximum: No limit (data becomes stale)

2. **Buffer Size**
   - Minimum: 4096 bytes (basic systems)
   - Recommended: 65536 bytes (full reports)
   - Maximum: Limited by available memory

3. **Memory Usage**
   - Instance: ~10-50 MB (varies by hardware)
   - Report: ~10-100 KB per call

---

## Example: Complete Workflow

```c
#include <stdio.h>
#include <stdlib.h>
#include <windows.h>

typedef void* (*CreateFunc)();
typedef void (*UpdateFunc)(void*);
typedef void (*GetReportFunc)(void*, char*, int);
typedef void (*DestroyFunc)(void*);

int main() {
    HMODULE dll = LoadLibrary(L"ManagedxZenithHardwareMonitorWrapper.dll");
    if (!dll) {
        fprintf(stderr, "Failed to load DLL\n");
        return 1;
    }

    CreateFunc create = (CreateFunc)GetProcAddress(dll, "CreateHardwareMonitor");
    UpdateFunc update = (UpdateFunc)GetProcAddress(dll, "UpdateHardwareMonitor");
    GetReportFunc getReport = (GetReportFunc)GetProcAddress(dll, "GetReport");
    DestroyFunc destroy = (DestroyFunc)GetProcAddress(dll, "DestroyHardwareMonitor");

    void* monitor = create();
    if (!monitor) {
        fprintf(stderr, "Failed to create monitor\n");
        FreeLibrary(dll);
        return 1;
    }

    char* buffer = (char*)malloc(65536);
    
    for (int i = 0; i < 10; i++) {
        update(monitor);
        getReport(monitor, buffer, 65536);
        printf("Report %d:\n%s\n\n", i + 1, buffer);
        Sleep(1000);
    }

    free(buffer);
    destroy(monitor);
    FreeLibrary(dll);
    
    return 0;
}
```
