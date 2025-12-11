# Intel CPU & GPU Enhancements - Implementation Summary

## Changes Made (December 11, 2025)

### Files Created/Modified

1. **Hardware/CoreType.cs** - New enum for P-Core/E-Core detection
2. **Hardware/Cpu/CpuId.cs** - Added CoreType property with CPUID detection
3. **Hardware/Cpu/IntelCpu.cs** - P-Core/E-Core naming for hybrid CPUs
4. **Interop/IntelGcl.cs** - Intel Graphics Control Library interop (new)
5. **Hardware/Gpu/IntelDiscreteGpu.cs** - Intel Arc GPU support (new)
6. **Hardware/Gpu/IntelGpuGroup.cs** - Updated for discrete GPU detection
7. **docs/HARDWARE_TYPES.md** - Updated documentation

---

## Feature 1: Intel Hybrid CPU P-Core/E-Core Detection

### Overview

Intel hybrid CPUs (Alder Lake, Raptor Lake, Meteor Lake, Arrow Lake, Lunar Lake) have two types of cores:
- **P-Cores (Performance)**: High-performance cores for demanding tasks
- **E-Cores (Efficient)**: Power-efficient cores for background tasks

xZenith now detects and labels these cores correctly using CPUID leaf 0x1A.

### Implementation Details

#### CoreType Enum (CoreType.cs)

```csharp
public enum CoreType
{
    Unknown = 0,
    Performance = 0x40,  // Intel Core (P-Core)
    Efficient = 0x20     // Intel Atom (E-Core)
}
```

#### CPUID Detection (CpuId.cs)

```csharp
if (Vendor == Vendor.Intel && maxCpuid >= 0x1A)
{
    // Leaf 0x1A, SubLeaf 0, EAX Bits 31:24 = Core Type ID
    uint coreType = (Data[0x1A, 0] >> 24) & 0xFF;

    CoreType = coreType switch
    {
        0x40 => CoreType.Performance,  // P-Core
        0x20 => CoreType.Efficient,    // E-Core
        _ => CoreType.Unknown
    };
}
```

#### Sensor Naming (IntelCpu.cs)

```csharp
// Initialize core names with P-Core/E-Core detection
_coreNames = new string[_coreCount];
int pCoreIndex = 1;
int eCoreIndex = 1;

for (int i = 0; i < _coreCount; i++)
{
    CoreType coreType = _cpuId[i][0].CoreType;
    
    _coreNames[i] = coreType switch
    {
        CoreType.Performance => $"P-Core #{pCoreIndex++}",
        CoreType.Efficient => $"E-Core #{eCoreIndex++}",
        _ => CoreString(i)  // Fallback: "Core #N"
    };
}
```

### Supported Processors

| Architecture | Model ID | Examples |
|--------------|----------|----------|
| Alder Lake | 0x97, 0x9A, 0xBE | Core i9-12900K, i7-12700K |
| Raptor Lake | 0xB7, 0xBA, 0xBF | Core i9-13900K, i7-13700K |
| Meteor Lake | 0xAC, 0xAA | Core Ultra 7 155H |
| Arrow Lake | 0xC5, 0xC6 | Core Ultra 9 285K |
| Lunar Lake | 0xBD | Core Ultra 7 258V |

### Example Output

**Before (all cores named generically):**
```
Core #1: 45°C, 4500 MHz
Core #2: 42°C, 4200 MHz
...
Core #24: 38°C, 3200 MHz
```

**After (P-Core/E-Core differentiation):**
```
P-Core #1: 45°C, 4500 MHz
P-Core #2: 42°C, 4200 MHz
...
P-Core #8: 44°C, 4300 MHz
E-Core #1: 38°C, 3200 MHz
E-Core #2: 37°C, 3100 MHz
...
E-Core #16: 36°C, 3000 MHz
```

---

## Feature 2: Intel Arc Discrete GPU Support

### Overview

Intel Arc GPUs (A380, A580, A750, A770, etc.) are now supported via the Intel Graphics Control Library (GCL/ControlLib.dll).

### Requirements

- Intel Arc GPU (Alchemist or later)
- Intel GPU drivers with ControlLib.dll (included with recent drivers)
- No additional package dependencies (uses manual P/Invoke)

### Implementation Details

#### IntelGcl.cs (Interop)

Key structures for Intel GCL communication:

```csharp
// Device handle for GPU operations
public struct ctl_device_adapter_handle_t { IntPtr pNext; }

// Power telemetry data structure
public struct ctl_power_telemetry_t
{
    public ctl_oc_telemetry_item_t gpuCurrentTemperature;
    public ctl_oc_telemetry_item_t gpuCurrentClockFrequency;
    public ctl_oc_telemetry_item_t gpuVoltage;
    public ctl_oc_telemetry_item_t gpuEnergyCounter;
    public ctl_oc_telemetry_item_t globalActivityCounter;
    // ... more telemetry items
}

// P/Invoke declarations
[DllImport("ControlLib.dll", CallingConvention = CallingConvention.Cdecl)]
private static extern int ctlInit(ref ctl_init_args_t pInitDesc, ref ctl_api_handle_t phAPIHandle);

[DllImport("ControlLib.dll", CallingConvention = CallingConvention.Cdecl)]
public static extern int ctlPowerTelemetryGet(ctl_device_adapter_handle_t hDeviceHandle, 
    ref ctl_power_telemetry_t pTelemetryInfo);
```

#### IntelDiscreteGpu.cs

Sensors implemented:

| Sensor Type | Names | Source |
|-------------|-------|--------|
| Temperature | GPU Core, GPU Memory | GCL Telemetry |
| Clock | GPU Core, GPU Memory | GCL Frequency API |
| Load | GPU Core, GPU Render/Compute, GPU Media | GCL Activity Counters |
| Power | GPU Package, GPU Total | GCL Energy Counters |
| Voltage | GPU Core, GPU Memory | GCL Telemetry |
| Fan | GPU Fan 1-N | GCL Fan API |
| SmallData | GPU Memory Used/Free/Total | D3D Memory API |
| Throughput | GPU Memory Read/Write | GCL Bandwidth |

#### IntelGpuGroup.cs

Initialization flow:

```csharp
// Try Intel GCL for discrete GPUs
if (IntelGcl.IsAvailable)
{
    gclInitialized = IntelGcl.Initialize();
    
    if (gclInitialized)
    {
        var handles = IntelGcl.GetDeviceHandles();
        foreach (var handle in handles)
        {
            var gpu = new IntelDiscreteGpu(handle, settings);
            if (gpu.IsValid)
                _hardware.Add(gpu);
        }
    }
}

// Then check for integrated GPUs via D3D
// (existing code unchanged)
```

### Supported GPUs

| Series | Models | Notes |
|--------|--------|-------|
| Arc A-Series | A380, A580, A750, A770 | Full support |
| Arc Pro | A40, A50, A60 | Workstation GPUs |

### Sensor Examples

```json
{
  "HardwareType": "GpuIntel",
  "Name": "Intel Arc A770",
  "Sensors": [
    { "SensorType": "Temperature", "Name": "GPU Core", "Value": 55.0 },
    { "SensorType": "Temperature", "Name": "GPU Memory", "Value": 52.0 },
    { "SensorType": "Clock", "Name": "GPU Core", "Value": 2100.0 },
    { "SensorType": "Clock", "Name": "GPU Memory", "Value": 2187.5 },
    { "SensorType": "Load", "Name": "GPU Core", "Value": 65.0 },
    { "SensorType": "Load", "Name": "GPU Render/Compute", "Value": 58.0 },
    { "SensorType": "Load", "Name": "GPU Media", "Value": 5.0 },
    { "SensorType": "Power", "Name": "GPU Package", "Value": 185.0 },
    { "SensorType": "Power", "Name": "GPU Total", "Value": 210.0 },
    { "SensorType": "Voltage", "Name": "GPU Core", "Value": 1.05 },
    { "SensorType": "SmallData", "Name": "GPU Memory Used", "Value": 8192.0 },
    { "SensorType": "SmallData", "Name": "GPU Memory Total", "Value": 16384.0 },
    { "SensorType": "Fan", "Name": "GPU Fan", "Value": 1800.0 }
  ]
}
```

---

## Compatibility Notes

### No External Dependencies

Unlike LibreHardwareMonitor which requires:
- Microsoft.Windows.CsWin32 package
- PawnIO driver (optional)

xZenith uses:
- **Manual P/Invoke** for kernel32.dll (LoadLibrary, GetProcAddress)
- **Direct DllImport** for Intel GCL (ControlLib.dll)
- **Embedded WinRing0** driver for MSR access

### Backwards Compatibility

- All existing CPU sensor indices unchanged
- Non-hybrid CPUs continue to use "Core #N" naming
- Integrated GPU detection unchanged (D3D-based)
- New discrete GPU sensors appear as additional hardware entries

---

## Testing Recommendations

### CPU P-Core/E-Core

1. Test on Alder Lake (12th Gen) system
2. Verify P-Core count matches physical P-cores
3. Verify E-Core count matches physical E-cores
4. Test on non-hybrid CPU (should show "Core #N")

### Intel Arc GPU

1. Verify ControlLib.dll present in `C:\Windows\System32`
2. Test sensor values against Intel Arc Control app
3. Verify memory values match Task Manager
4. Test fan speed readings (if applicable)

---

## Build Verification

```powershell
# Build all target frameworks
dotnet build xZenithHardwareMonitorLib.csproj

# Expected output:
# - 0 errors
# - Warnings unchanged from baseline
# - All 4 frameworks compile successfully (net472, netstandard2.0, net6.0, net8.0)
```

---

## References

- **Intel CPUID Documentation**: Leaf 0x1A Core Type detection
- **Intel Graphics Control Library**: ControlLib.dll API
- **Intel Arc Driver**: Version 31.0.101+ required for GCL
