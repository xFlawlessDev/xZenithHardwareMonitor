# Hardware Types Reference

Complete reference for all hardware types supported by xZenith Hardware Monitor, including available sensors, typical values, and usage examples.

## Table of Contents

- [Overview](#overview)
- [Hardware Types](#hardware-types)
  - [CPU (Cpu)](#cpu-cpu)
  - [GPU - NVIDIA (GpuNvidia)](#gpu---nvidia-gpunvidia)
  - [GPU - AMD (GpuAmd)](#gpu---amd-gpuamd)
  - [GPU - Intel (GpuIntel)](#gpu---intel-gpuintel)
  - [Memory](#memory)
  - [Motherboard](#motherboard)
  - [SuperIO](#superio)
  - [Storage](#storage)
  - [Network](#network)
  - [Battery](#battery)
  - [PSU (Psu)](#psu-psu)
  - [Cooler](#cooler)
  - [Embedded Controller (EmbeddedController)](#embedded-controller-embeddedcontroller)
- [Sensor Types Reference](#sensor-types-reference)
- [Code Examples](#code-examples)

---

## Overview

xZenith Hardware Monitor supports 13 hardware types, each providing specific sensor data:

| Hardware Type | Description | Common Use Cases |
|---------------|-------------|------------------|
| `Cpu` | Central Processing Unit | Temperature monitoring, performance analysis |
| `GpuNvidia` | NVIDIA Graphics Cards | Gaming monitoring, thermal management |
| `GpuAmd` | AMD Graphics Cards | Gaming monitoring, thermal management |
| `GpuIntel` | Intel Graphics | Integrated GPU monitoring |
| `Memory` | System RAM | Memory usage tracking |
| `Motherboard` | System Board | Overall system health |
| `SuperIO` | Super I/O Chip | Fan control, voltage monitoring |
| `Storage` | HDDs, SSDs, NVMe | Drive health, temperatures |
| `Network` | Network Adapters | Bandwidth monitoring |
| `Battery` | Laptop Battery | Battery health, charge level |
| `Psu` | Power Supply Unit | Power delivery monitoring |
| `Cooler` | Cooling Devices | Cooling system monitoring |
| `EmbeddedController` | EC Chip | Laptop-specific sensors |

---

## Hardware Types

### CPU (Cpu)

The CPU hardware type provides comprehensive processor monitoring for Intel and AMD processors.

#### Available Sensors

| Sensor Type | Sensor Names | Unit | Description |
|-------------|--------------|------|-------------|
| `Temperature` | **Intel**: `CPU Package`, `CPU Core #1` - `#N` (or `P-Core #1` - `#N`, `E-Core #1` - `#N` for hybrid CPUs), `Core Max`, `Core Average`, `CPU Core #1 Distance to TjMax` - `#N Distance to TjMax`<br>**AMD**: `Core (Tctl)`, `Core (Tdie)`, `Core (Tctl/Tdie)`, `CCD1 (Tdie)` - `CCD8 (Tdie)`, `CCDs Max (Tdie)`, `CCDs Average (Tdie)` | C | Core and package temperatures |
| `Load` | `CPU Total`, `CPU Core #1` - `#N` (or `P-Core #1` - `#N`, `E-Core #1` - `#N`), `CPU Core Max`, `CPU Core #1 Thread #1`, `CPU Core #1 Thread #2` | % | Utilization per core/thread and total |
| `Clock` | `CPU Core #1` - `#N` (or `P-Core #1` - `#N`, `E-Core #1` - `#N`), `Bus Speed` | MHz | Operating frequencies |
| `Power` | **Intel**: `CPU Package`, `CPU Cores`, `CPU Graphics`, `CPU Memory`, `CPU Platform`<br>**AMD**: `Package` (plus SMU sensors if available) | W | Power consumption |
| `Voltage` | **Intel**: `CPU Core`, `CPU Core #1` - `#N` (VID readings)<br>**AMD**: `Core (SVI2 TFN)`, `SoC (SVI2 TFN)` (plus SMU sensors if available) | V | Core voltages |

#### JSON Example

```json
{
  "HardwareType": "Cpu",
  "Name": "AMD Ryzen 9 5900X",
  "Sensors": [
    {
      "SensorType": "Temperature",
      "Name": "Core (Tctl/Tdie)",
      "Index": 0,
      "Value": 45.5,
      "Min": 32.0,
      "Max": 78.5
    },
    {
      "SensorType": "Temperature",
      "Name": "CCD1 (Tdie)",
      "Index": 1,
      "Value": 48.0,
      "Min": 34.0,
      "Max": 82.0
    },
    {
      "SensorType": "Temperature",
      "Name": "CCDs Average (Tdie)",
      "Index": 3,
      "Value": 46.5,
      "Min": 33.0,
      "Max": 80.0
    },
    {
      "SensorType": "Load",
      "Name": "CPU Total",
      "Index": 0,
      "Value": 15.2,
      "Min": 0.5,
      "Max": 100.0
    },
    {
      "SensorType": "Load",
      "Name": "CPU Core Max",
      "Index": 1,
      "Value": 42.5,
      "Min": 1.0,
      "Max": 100.0
    },
    {
      "SensorType": "Load",
      "Name": "CPU Core #1",
      "Index": 2,
      "Value": 12.5,
      "Min": 0.0,
      "Max": 100.0
    },
    {
      "SensorType": "Clock",
      "Name": "CPU Core #1",
      "Index": 1,
      "Value": 4200.5,
      "Min": 2200.0,
      "Max": 4950.0
    },
    {
      "SensorType": "Clock",
      "Name": "Bus Speed",
      "Index": 0,
      "Value": 100.0,
      "Min": 100.0,
      "Max": 100.0
    },
    {
      "SensorType": "Power",
      "Name": "Package",
      "Index": 0,
      "Value": 65.3,
      "Min": 10.5,
      "Max": 142.0
    },
    {
      "SensorType": "Voltage",
      "Name": "Core (SVI2 TFN)",
      "Index": 0,
      "Value": 1.325,
      "Min": 0.200,
      "Max": 1.550
    },
    {
      "SensorType": "Voltage",
      "Name": "SoC (SVI2 TFN)",
      "Index": 1,
      "Value": 1.100,
      "Min": 0.900,
      "Max": 1.200
    }
  ],
  "SubHardware": []
}
```

#### Rust Implementation

```rust
use serde::Deserialize;

#[derive(Debug, Deserialize)]
pub struct CpuData {
    pub name: String,
    pub temperature: Option<f32>,
    pub total_load: Option<f32>,
    pub core_loads: Vec<CoreLoad>,
    pub clocks: Vec<ClockSpeed>,
    pub power: Option<f32>,
}

#[derive(Debug, Deserialize)]
pub struct CoreLoad {
    pub core_name: String,
    pub load: f32,
}

#[derive(Debug, Deserialize)]
pub struct ClockSpeed {
    pub core_name: String,
    pub clock_mhz: f32,
}

impl CpuData {
    pub fn from_hardware(hw: &Hardware) -> Option<Self> {
        if hw.hardware_type != "Cpu" {
            return None;
        }

        // Try to find package temperature (Intel) or Tctl/Tdie (AMD)
        let temperature = hw.sensors.iter()
            .find(|s| s.sensor_type == "Temperature" && 
                (s.name.contains("Package") || 
                 s.name.contains("Tctl") || 
                 s.name.contains("Tdie")))
            .map(|s| s.value);

        let total_load = hw.sensors.iter()
            .find(|s| s.sensor_type == "Load" && s.name == "CPU Total")
            .map(|s| s.value);

        let core_loads = hw.sensors.iter()
            .filter(|s| s.sensor_type == "Load" && s.name.starts_with("CPU Core #"))
            .map(|s| CoreLoad {
                core_name: s.name.clone(),
                load: s.value,
            })
            .collect();

        let clocks = hw.sensors.iter()
            .filter(|s| s.sensor_type == "Clock" && s.name.starts_with("CPU Core #"))
            .map(|s| ClockSpeed {
                core_name: s.name.clone(),
                clock_mhz: s.value,
            })
            .collect();

        // Try to find Package power sensor (both Intel and AMD use this name)
        let power = hw.sensors.iter()
            .find(|s| s.sensor_type == "Power" && s.name.contains("Package"))
            .map(|s| s.value);

        Some(CpuData {
            name: hw.name.clone(),
            temperature,
            total_load,
            core_loads,
            clocks,
            power,
        })
    }
}
```

---

### GPU - NVIDIA (GpuNvidia)

NVIDIA graphics cards provide extensive monitoring through NVML/NVAPI.

#### Available Sensors

| Sensor Type | Sensor Names | Unit | Description |
|-------------|--------------|------|-------------|
| `Temperature` | `GPU Core`, `GPU Memory`, `GPU Hot Spot`, `GPU Memory Junction`, `GPU Power Supply`, `GPU Board`, `GPU Visual Computing Board`, `GPU Visual Computing Inlet`, `GPU Visual Computing Outlet` | C | GPU temperatures |
| `Load` | `GPU Core`, `GPU Memory Controller`, `GPU Memory`, `GPU Video Engine`, `GPU Bus`, `D3D 3D`, `D3D Copy`, `D3D Video Decode`, `D3D Video Encode`, `D3D Cuda`, `GPU Power`*, `GPU Board Power`* | % | Utilization metrics |
| `Clock` | `GPU Core`, `GPU Memory`, `GPU Shader`, `GPU Video` | MHz | Clock speeds |
| `Power` | `GPU Package` | W | Power consumption via NVML |
| `Fan` | `GPU Fan`, `GPU Fan #1`, `GPU Fan #2` | RPM | Fan speeds |
| `Control` | `GPU Fan`, `GPU Fan #1`, `GPU Fan #2` | % | Fan control level |
| `SmallData` | `GPU Memory Used`, `GPU Memory Free`, `GPU Memory Total`, `D3D Dedicated Memory Used`, `D3D Shared Memory Used` | MB | VRAM usage |
| `Throughput` | `GPU PCIe Rx`, `GPU PCIe Tx` | B/s | PCIe bandwidth |

**Note**: `GPU Power` and `GPU Board Power` are incorrectly reported as `Load` type (bug in implementation line 303), but represent power consumption in watts. Use `GPU Package` (Power type) for accurate power readings via NVML.

#### JSON Example

```json
{
  "HardwareType": "GpuNvidia",
  "Name": "NVIDIA GeForce RTX 4080",
  "Sensors": [
    {
      "SensorType": "Temperature",
      "Name": "GPU Core",
      "Index": 0,
      "Value": 52.0,
      "Min": 35.0,
      "Max": 83.0
    },
    {
      "SensorType": "Temperature",
      "Name": "GPU Hot Spot",
      "Index": 1,
      "Value": 68.0,
      "Min": 42.0,
      "Max": 95.0
    },
    {
      "SensorType": "Temperature",
      "Name": "GPU Memory Junction",
      "Index": 2,
      "Value": 64.0,
      "Min": 38.0,
      "Max": 88.0
    },
    {
      "SensorType": "Load",
      "Name": "GPU Core",
      "Index": 0,
      "Value": 45.0,
      "Min": 0.0,
      "Max": 100.0
    },
    {
      "SensorType": "Load",
      "Name": "GPU Memory Controller",
      "Index": 1,
      "Value": 12.0,
      "Min": 0.0,
      "Max": 100.0
    },
    {
      "SensorType": "Load",
      "Name": "GPU Memory",
      "Index": 3,
      "Value": 25.6,
      "Min": 0.0,
      "Max": 100.0
    },
    {
      "SensorType": "Clock",
      "Name": "GPU Core",
      "Index": 0,
      "Value": 2100.0,
      "Min": 210.0,
      "Max": 2850.0
    },
    {
      "SensorType": "Clock",
      "Name": "GPU Memory",
      "Index": 1,
      "Value": 11400.0,
      "Min": 810.0,
      "Max": 11400.0
    },
    {
      "SensorType": "SmallData",
      "Name": "GPU Memory Used",
      "Index": 1,
      "Value": 4096.0,
      "Min": 512.0,
      "Max": 15360.0
    },
    {
      "SensorType": "SmallData",
      "Name": "GPU Memory Free",
      "Index": 0,
      "Value": 12288.0,
      "Min": 512.0,
      "Max": 15872.0
    },
    {
      "SensorType": "SmallData",
      "Name": "GPU Memory Total",
      "Index": 2,
      "Value": 16384.0,
      "Min": 16384.0,
      "Max": 16384.0
    },
    {
      "SensorType": "Fan",
      "Name": "GPU Fan",
      "Index": 0,
      "Value": 1200.0,
      "Min": 0.0,
      "Max": 2400.0
    },
    {
      "SensorType": "Control",
      "Name": "GPU Fan",
      "Index": 0,
      "Value": 45.0,
      "Min": 0.0,
      "Max": 100.0
    },
    {
      "SensorType": "Power",
      "Name": "GPU Package",
      "Index": 0,
      "Value": 185.5,
      "Min": 15.0,
      "Max": 320.0
    },
    {
      "SensorType": "Throughput",
      "Name": "GPU PCIe Rx",
      "Index": 0,
      "Value": 104857600.0,
      "Min": 0.0,
      "Max": 2000000000.0
    },
    {
      "SensorType": "Throughput",
      "Name": "GPU PCIe Tx",
      "Index": 1,
      "Value": 52428800.0,
      "Min": 0.0,
      "Max": 2000000000.0
    }
  ],
  "SubHardware": []
}
```

---

### GPU - AMD (GpuAmd)

AMD graphics cards monitoring through ADL/ADLX.

#### Available Sensors

| Sensor Type | Sensor Names | Unit | Description |
|-------------|--------------|------|-------------|
| `Temperature` | `GPU Core`, `GPU Hot Spot`, `GPU Memory`, `GPU VR SoC`, `GPU VR VDDC`, `GPU VR MVDD`, `GPU Liquid`, `GPU PLX` | C | Various temperature points |
| `Load` | `GPU Core`, `GPU Memory Controller`, `GPU Memory`, `D3D 3D`, `D3D Copy`, `D3D Video Decode`, `D3D Video Encode` | % | Utilization |
| `Clock` | `GPU Core`, `GPU Memory`, `GPU SoC` | MHz | Clock speeds |
| `Power` | `GPU Core`, `GPU SoC`, `GPU PPT`, `GPU Package` | W | Power consumption |
| `Fan` | `GPU Fan`, `GPU Fan #1`, `GPU Fan #2` | RPM | Fan speed |
| `Control` | `GPU Fan`, `GPU Fan #1`, `GPU Fan #2` | % | Fan control percentage |
| `Voltage` | `GPU Core`, `GPU SoC`, `GPU Memory` | V | Voltages |
| `SmallData` | `GPU Memory Used`, `GPU Memory Free`, `GPU Memory Total`, `D3D Dedicated Memory Used`, `D3D Dedicated Memory Free`, `D3D Dedicated Memory Total`, `D3D Shared Memory Used`, `D3D Shared Memory Free`, `D3D Shared Memory Total` | MB | VRAM usage |
| `Factor` | `Fullscreen FPS` | 1 | Frames per second in fullscreen applications |

#### JSON Example

```json
{
  "HardwareType": "GpuAmd",
  "Name": "AMD Radeon RX 7900 XTX",
  "Sensors": [
    {
      "SensorType": "Temperature",
      "Name": "GPU Core",
      "Index": 0,
      "Value": 55.0,
      "Min": 38.0,
      "Max": 85.0
    },
    {
      "SensorType": "Temperature",
      "Name": "GPU Hot Spot",
      "Index": 7,
      "Value": 72.0,
      "Min": 45.0,
      "Max": 98.0
    },
    {
      "SensorType": "Load",
      "Name": "GPU Core",
      "Index": 0,
      "Value": 56.0,
      "Min": 0.0,
      "Max": 100.0
    },
    {
      "SensorType": "Load",
      "Name": "GPU Memory",
      "Index": 1,
      "Value": 35.0,
      "Min": 0.0,
      "Max": 100.0
    },
    {
      "SensorType": "Clock",
      "Name": "GPU Core",
      "Index": 0,
      "Value": 2400.0,
      "Min": 500.0,
      "Max": 2700.0
    },
    {
      "SensorType": "Clock",
      "Name": "GPU Memory",
      "Index": 2,
      "Value": 2500.0,
      "Min": 96.0,
      "Max": 2500.0
    },
    {
      "SensorType": "SmallData",
      "Name": "GPU Memory Used",
      "Index": 0,
      "Value": 8192.0,
      "Min": 256.0,
      "Max": 23552.0
    },
    {
      "SensorType": "SmallData",
      "Name": "GPU Memory Free",
      "Index": 1,
      "Value": 16192.0,
      "Min": 832.0,
      "Max": 24128.0
    },
    {
      "SensorType": "SmallData",
      "Name": "GPU Memory Total",
      "Index": 2,
      "Value": 24576.0,
      "Min": 24576.0,
      "Max": 24576.0
    },
    {
      "SensorType": "Fan",
      "Name": "GPU Fan",
      "Index": 0,
      "Value": 1450.0,
      "Min": 0.0,
      "Max": 3200.0
    },
    {
      "SensorType": "Control",
      "Name": "GPU Fan",
      "Index": 0,
      "Value": 42.0,
      "Min": 0.0,
      "Max": 100.0
    },
    {
      "SensorType": "Power",
      "Name": "GPU Package",
      "Index": 3,
      "Value": 295.0,
      "Min": 20.0,
      "Max": 355.0
    },
    {
      "SensorType": "Voltage",
      "Name": "GPU Core",
      "Index": 0,
      "Value": 1.075,
      "Min": 0.550,
      "Max": 1.200
    }
  ],
  "SubHardware": []
}
```

---

### GPU - Intel (GpuIntel)

Intel integrated and discrete graphics monitoring. Supports both Intel UHD/Iris integrated GPUs and Intel Arc discrete GPUs.

#### Intel Arc Discrete GPU Support

Intel Arc GPUs (A380, A580, A750, A770, etc.) are supported via the Intel Graphics Control Library (GCL). Requires Intel GPU drivers with ControlLib.dll.

#### Available Sensors

**Integrated GPU (Intel UHD/Iris):**

| Sensor Type | Sensor Names | Unit | Description |
|-------------|--------------|------|-------------|
| `Temperature` | `GPU Core` | C | GPU temperature |
| `Load` | `GPU Core`, `D3D 3D`, `D3D Copy`, `D3D Video Decode`, `D3D Video Encode` | % | Utilization |
| `Clock` | `GPU Core` | MHz | Operating frequency |
| `Power` | `GPU Power`, `GPU Package` | W | Power consumption |
| `SmallData` | `D3D Dedicated Memory Used`, `D3D Shared Memory Used`, `D3D Shared Memory Free`, `D3D Shared Memory Total` | MB | Memory usage |

**Discrete GPU (Intel Arc):**

| Sensor Type | Sensor Names | Unit | Description |
|-------------|--------------|------|-------------|
| `Temperature` | `GPU Core`, `GPU Memory` | C | GPU and VRAM temperatures |
| `Load` | `GPU Core`, `GPU Render/Compute`, `GPU Media` | % | Utilization metrics |
| `Clock` | `GPU Core`, `GPU Memory` | MHz | Clock speeds |
| `Power` | `GPU Package`, `GPU Total` | W | Power consumption |
| `Voltage` | `GPU Core`, `GPU Memory` | V | Voltage readings |
| `Fan` | `GPU Fan`, `GPU Fan 1`, `GPU Fan 2` | RPM | Fan speeds |
| `SmallData` | `GPU Memory Used`, `GPU Memory Free`, `GPU Memory Total` | MB | VRAM usage |
| `Throughput` | `GPU Memory Read`, `GPU Memory Write` | B/s | Memory bandwidth |

#### JSON Example (Integrated GPU)

```json
{
  "HardwareType": "GpuIntel",
  "Name": "Intel UHD Graphics 770",
  "Sensors": [
    {
      "SensorType": "SmallData",
      "Name": "D3D Dedicated Memory Used",
      "Index": 0,
      "Value": 128.0,
      "Min": 0.0,
      "Max": 256.0
    },
    {
      "SensorType": "SmallData",
      "Name": "D3D Shared Memory Used",
      "Index": 1,
      "Value": 512.0,
      "Min": 0.0,
      "Max": 8192.0
    },
    {
      "SensorType": "SmallData",
      "Name": "D3D Shared Memory Free",
      "Index": 2,
      "Value": 7680.0,
      "Min": 0.0,
      "Max": 8192.0
    },
    {
      "SensorType": "SmallData",
      "Name": "D3D Shared Memory Total",
      "Index": 3,
      "Value": 8192.0,
      "Min": 8192.0,
      "Max": 8192.0
    },
    {
      "SensorType": "Load",
      "Name": "D3D 3D",
      "Index": 0,
      "Value": 12.5,
      "Min": 0.0,
      "Max": 95.0
    },
    {
      "SensorType": "Load",
      "Name": "D3D Copy",
      "Index": 1,
      "Value": 2.5,
      "Min": 0.0,
      "Max": 45.0
    },
    {
      "SensorType": "Power",
      "Name": "GPU Power",
      "Index": 0,
      "Value": 8.5,
      "Min": 2.0,
      "Max": 15.0
    }
  ],
  "SubHardware": []
}
```

#### JSON Example (Intel Arc Discrete GPU)

```json
{
  "HardwareType": "GpuIntel",
  "Name": "Intel Arc A770",
  "Sensors": [
    {
      "SensorType": "Temperature",
      "Name": "GPU Core",
      "Index": 0,
      "Value": 55.0,
      "Min": 32.0,
      "Max": 78.0
    },
    {
      "SensorType": "Temperature",
      "Name": "GPU Memory",
      "Index": 1,
      "Value": 52.0,
      "Min": 30.0,
      "Max": 72.0
    },
    {
      "SensorType": "Clock",
      "Name": "GPU Core",
      "Index": 0,
      "Value": 2100.0,
      "Min": 300.0,
      "Max": 2400.0
    },
    {
      "SensorType": "Clock",
      "Name": "GPU Memory",
      "Index": 1,
      "Value": 2187.5,
      "Min": 625.0,
      "Max": 2187.5
    },
    {
      "SensorType": "Load",
      "Name": "GPU Core",
      "Index": 0,
      "Value": 65.0,
      "Min": 0.0,
      "Max": 100.0
    },
    {
      "SensorType": "Load",
      "Name": "GPU Render/Compute",
      "Index": 1,
      "Value": 58.0,
      "Min": 0.0,
      "Max": 100.0
    },
    {
      "SensorType": "Load",
      "Name": "GPU Media",
      "Index": 2,
      "Value": 5.0,
      "Min": 0.0,
      "Max": 45.0
    },
    {
      "SensorType": "Power",
      "Name": "GPU Package",
      "Index": 0,
      "Value": 185.0,
      "Min": 15.0,
      "Max": 225.0
    },
    {
      "SensorType": "Power",
      "Name": "GPU Total",
      "Index": 1,
      "Value": 210.0,
      "Min": 20.0,
      "Max": 275.0
    },
    {
      "SensorType": "Voltage",
      "Name": "GPU Core",
      "Index": 0,
      "Value": 1.05,
      "Min": 0.65,
      "Max": 1.15
    },
    {
      "SensorType": "SmallData",
      "Name": "GPU Memory Used",
      "Index": 1,
      "Value": 8192.0,
      "Min": 256.0,
      "Max": 15360.0
    },
    {
      "SensorType": "SmallData",
      "Name": "GPU Memory Free",
      "Index": 0,
      "Value": 8192.0,
      "Min": 1024.0,
      "Max": 16128.0
    },
    {
      "SensorType": "SmallData",
      "Name": "GPU Memory Total",
      "Index": 2,
      "Value": 16384.0,
      "Min": 16384.0,
      "Max": 16384.0
    },
    {
      "SensorType": "Fan",
      "Name": "GPU Fan",
      "Index": 0,
      "Value": 1800.0,
      "Min": 0.0,
      "Max": 2800.0
    },
    {
      "SensorType": "Throughput",
      "Name": "GPU Memory Read",
      "Index": 0,
      "Value": 25769803776.0,
      "Min": 0.0,
      "Max": 560000000000.0
    },
    {
      "SensorType": "Throughput",
      "Name": "GPU Memory Write",
      "Index": 1,
      "Value": 17179869184.0,
      "Min": 0.0,
      "Max": 560000000000.0
    }
  ],
  "SubHardware": []
}
```

---

### Memory

System RAM monitoring.

#### Available Sensors

| Sensor Type | Sensor Names | Unit | Description |
|-------------|--------------|------|-------------|
| `Load` | `Memory`, `Virtual Memory` | % | Memory utilization |
| `Data` | `Memory Used`, `Memory Available`, `Virtual Memory Used`, `Virtual Memory Available` | GB | Memory amounts |

#### JSON Example

```json
{
  "HardwareType": "Memory",
  "Name": "Generic Memory",
  "Sensors": [
    {
      "SensorType": "Load",
      "Name": "Memory",
      "Index": 0,
      "Value": 45.2,
      "Min": 25.0,
      "Max": 92.5
    },
    {
      "SensorType": "Data",
      "Name": "Memory Used",
      "Index": 0,
      "Value": 14.5,
      "Min": 8.0,
      "Max": 29.6
    },
    {
      "SensorType": "Data",
      "Name": "Memory Available",
      "Index": 1,
      "Value": 17.5,
      "Min": 2.4,
      "Max": 24.0
    }
  ],
  "SubHardware": []
}
```

---

### Motherboard

System board monitoring, typically contains SuperIO as SubHardware.

#### Available Sensors

| Sensor Type | Sensor Names | Unit | Description |
|-------------|--------------|------|-------------|
| `Temperature` | `System`, `PCH`, `CPU`, `Motherboard`, `T_Sensor`, `VRM` | C | Board temperatures |
| `Voltage` | `Vcore`, `VDIMM`, `+3.3V`, `+5V`, `+12V`, `-12V`, `-5V`, `3VSB`, `VBAT`, `AVCC` | V | Voltage rails |
| `Fan` | `CPU Fan`, `CPU OPT`, `System Fan #1` - `#6`, `Chassis Fan #1` - `#3`, `AIO Pump`, `W_PUMP+` | RPM | Connected fan speeds |
| `Control` | `CPU Fan`, `System Fan #1` - `#6` | % | Fan control levels |

#### JSON Example

```json
{
  "HardwareType": "Motherboard",
  "Name": "ASUS ROG STRIX X570-E GAMING",
  "Sensors": [],
  "SubHardware": [
    {
      "HardwareType": "SuperIO",
      "Name": "Nuvoton NCT6798D",
      "Sensors": [
        {
          "SensorType": "Voltage",
          "Name": "Vcore",
          "Index": 0,
          "Value": 1.25,
          "Min": 0.95,
          "Max": 1.45
        },
        {
          "SensorType": "Temperature",
          "Name": "CPU Core",
          "Index": 0,
          "Value": 45.0,
          "Min": 32.0,
          "Max": 78.0
        },
        {
          "SensorType": "Fan",
          "Name": "CPU Fan",
          "Index": 0,
          "Value": 1200.0,
          "Min": 600.0,
          "Max": 2000.0
        }
      ],
      "SubHardware": []
    }
  ]
}
```

---

### SuperIO

Super I/O chip providing voltage, temperature, and fan monitoring. Common SuperIO chips include Nuvoton NCT6798D, ITE IT8688E, Fintek F71882.

#### Available Sensors

| Sensor Type | Sensor Names | Unit | Description |
|-------------|--------------|------|-------------|
| `Voltage` | `Vcore`, `Voltage #1` - `#15`, `VIN0` - `VIN7`, `+3.3V`, `+5V`, `+12V`, `-12V`, `-5V`, `3VSB`, `VBAT`, `AVCC`, `AVSB`, `VTT` | V | System voltages |
| `Temperature` | `CPU Core`, `Temperature #1` - `#6`, `System`, `Auxiliary`, `PCH`, `VRM MOS`, `Chipset`, `TSI0 Temp` | C | Board temperatures |
| `Fan` | `CPU Fan`, `System Fan #1` - `#6`, `Chassis Fan #1` - `#3`, `Power Fan`, `Auxiliary Fan` | RPM | Fan speeds |
| `Control` | `CPU Fan`, `System Fan #1` - `#6`, `Chassis Fan #1` - `#3` | % | Fan PWM control levels |

---

### Storage

HDD, SSD, and NVMe drive monitoring. Sensor availability varies by drive type and manufacturer.

#### Available Sensors

| Sensor Type | Sensor Names | Unit | Description |
|-------------|--------------|------|-------------|
| `Temperature` | `Temperature`, `Temperature 1`-`Temperature N`, `Airflow Temperature` | °C | Drive temperature sensors (SMART or NVMe) |
| `Load` | `Used Space`, `Read Activity`, `Write Activity`, `Total Activity` | % | Drive capacity usage and I/O activity |
| `Throughput` | `Read Rate`, `Write Rate` | B/s | Current transfer speeds |
| `Data` | `Data Read`, `Data Written`, `Total Bytes Written`, `Host Reads`, `Host Writes`, `Host Writes to Controller`, `Controller Writes to NAND`, `Host Read Commands`, `Host Write Commands` | GB or millions | Lifetime data transferred or command counts |
| `Level` | `Remaining Life`, `Available Spare`, `Available Spare Threshold`, `Percentage Used` | % | Drive health and wear indicators |
| `Factor` | `Write Amplification` | 1 | SSD write efficiency (Controller Writes / Host Writes) |
| `SmallData` | `Power Cycles`, `Unsafe Shutdowns`, `Media Errors`, `Error Info Log Entries` | count | Reliability and lifecycle counters |
| `TimeSpan` | `Power On Hours`, `Controller Busy Time`, `Warning Temperature Time`, `Critical Temperature Time` | s | Time-based metrics |

**Common SMART Attributes** (varies by drive model):
- Temperature sensors from attributes 0xC2, 0xE7, 0xBE, 0x194
- Power-On Hours (attribute 0x09) - displayed as raw value, not as sensor
- Start/Stop Count, Power Cycle Count
- Reallocated Sectors Count, Program/Erase Fail Counts
- Uncorrectable Error Count, CRC Error Count

**NVMe-Specific Sensors**:
- **Health Indicators**:
  - Available Spare / Available Spare Threshold (Level) - Remaining reserve capacity
  - Percentage Used (Level) - Wear indicator (0% = new, 100% = worn out)
  - Data Units Read/Written (Data) - Converted to GB: units × 512 / 1,000,000

- **Power & Lifecycle Metrics**:
  - Power Cycles (SmallData) - Total number of power-on events
  - Power On Hours (TimeSpan) - Cumulative operating time in seconds
  - Unsafe Shutdowns (SmallData) - Count of improper shutdowns (critical for reliability)

- **Error & Reliability Tracking**:
  - Media Errors (SmallData) - Unrecovered data integrity errors
  - Error Info Log Entries (SmallData) - Total entries in error log

- **I/O Activity Metrics**:
  - Host Read Commands (Data) - Total read operations in millions
  - Host Write Commands (Data) - Total write operations in millions
  - Controller Busy Time (TimeSpan) - Time spent processing I/O (minutes → seconds)

- **Temperature History** (if supported):
  - Warning Temperature Time (TimeSpan) - Cumulative time above warning threshold
  - Critical Temperature Time (TimeSpan) - Cumulative time above critical threshold
  - Multiple temperature sensors (composite + sensor 1-N)

**SSD-Specific Sensors** (vendor-dependent):
- **Remaining Life** (Level, SensorType.Level, Index 0):
  - **Intel SSD**: SMART attribute 0xE8 (direct value, 0-100%)
  - **Sandforce SSD**: SMART attribute 0xE7 (direct value, 0-100%)
  - **Samsung SSD**: SMART attribute 0xB4 - "Unused Reserved Block Count (Total)" (direct value, 0-100%)
  - **Micron SSD**: SMART attribute 0xCA - calculated as `100 - raw_value` (inverted scale)
  - **Indilinx SSD**: SMART attribute 0xD1 (direct value, 0-100%)
  - Represents percentage of drive life remaining before wear-out
  
- **Host Reads/Writes** (Data, GB):
  - Intel: 0xE1, 0xF1 (Host Writes), 0xF2 (Host Reads) - converted by `/0x20`
  - Sandforce: 0xF1 (Host Writes), 0xF2 (Host Reads) - raw value
  - Samsung: 0xF1 (Total LBAs Written) - complex 6-byte conversion to GB
  - Plextor: 0xF1 (Host Writes), 0xF2 (Host Reads) - converted by `/32`
  - Micron: 0xF6 (Total LBAs Written) - 6-byte conversion to GB
  
- **Write Amplification** (Factor):
  - Sandforce: Calculated as `Controller Writes (0xE9) / Host Writes (0xEA)`
  - Micron: Calculated as `(Host Pages (0xF7) + FTL Pages (0xF8)) / Host Pages`
  - Represents SSD write efficiency (lower is better, ideal ~1.0)

#### JSON Example

```json
{
  "HardwareType": "Storage",
  "Name": "Samsung SSD 980 PRO 1TB",
  "Sensors": [
    {
      "SensorType": "Temperature",
      "Name": "Temperature",
      "Index": 0,
      "Value": 42.0,
      "Min": 25.0,
      "Max": 65.0
    },
    {
      "SensorType": "Load",
      "Name": "Used Space",
      "Index": 0,
      "Value": 65.5,
      "Min": 10.0,
      "Max": 95.2
    },
    {
      "SensorType": "Load",
      "Name": "Read Activity",
      "Index": 31,
      "Value": 12.5,
      "Min": 0.0,
      "Max": 100.0
    },
    {
      "SensorType": "Load",
      "Name": "Write Activity",
      "Index": 32,
      "Value": 8.2,
      "Min": 0.0,
      "Max": 100.0
    },
    {
      "SensorType": "Load",
      "Name": "Total Activity",
      "Index": 33,
      "Value": 20.7,
      "Min": 0.0,
      "Max": 100.0
    },
    {
      "SensorType": "Throughput",
      "Name": "Read Rate",
      "Index": 34,
      "Value": 524288000.0,
      "Min": 0.0,
      "Max": 7000000000.0
    },
    {
      "SensorType": "Throughput",
      "Name": "Write Rate",
      "Index": 35,
      "Value": 104857600.0,
      "Min": 0.0,
      "Max": 5000000000.0
    },
    {
      "SensorType": "Level",
      "Name": "Remaining Life",
      "Index": 0,
      "Value": 98.0,
      "Min": 98.0,
      "Max": 100.0
    },
    {
      "SensorType": "Level",
      "Name": "Available Spare",
      "Index": 1,
      "Value": 100.0,
      "Min": 100.0,
      "Max": 100.0
    },
    {
      "SensorType": "Level",
      "Name": "Percentage Used",
      "Index": 3,
      "Value": 2.0,
      "Min": 0.0,
      "Max": 2.0
    },
    {
      "SensorType": "Data",
      "Name": "Total Bytes Written",
      "Index": 0,
      "Value": 15420.5,
      "Min": 0.0,
      "Max": 15420.5
    },
    {
      "SensorType": "Data",
      "Name": "Host Writes",
      "Index": 1,
      "Value": 8256.3,
      "Min": 0.0,
      "Max": 8256.3
    },
    {
      "SensorType": "Data",
      "Name": "Host Reads",
      "Index": 2,
      "Value": 12840.7,
      "Min": 0.0,
      "Max": 12840.7
    },
    {
      "SensorType": "SmallData",
      "Name": "Power Cycles",
      "Index": 6,
      "Value": 1247.0,
      "Min": 0.0,
      "Max": 1247.0
    },
    {
      "SensorType": "TimeSpan",
      "Name": "Power On Hours",
      "Index": 7,
      "Value": 5832000.0,
      "Min": 0.0,
      "Max": 5832000.0
    },
    {
      "SensorType": "SmallData",
      "Name": "Unsafe Shutdowns",
      "Index": 8,
      "Value": 12.0,
      "Min": 0.0,
      "Max": 12.0
    },
    {
      "SensorType": "SmallData",
      "Name": "Media Errors",
      "Index": 9,
      "Value": 0.0,
      "Min": 0.0,
      "Max": 0.0
    },
    {
      "SensorType": "SmallData",
      "Name": "Error Info Log Entries",
      "Index": 10,
      "Value": 3.0,
      "Min": 0.0,
      "Max": 3.0
    },
    {
      "SensorType": "Data",
      "Name": "Host Read Commands",
      "Index": 11,
      "Value": 254.8,
      "Min": 0.0,
      "Max": 254.8
    },
    {
      "SensorType": "Data",
      "Name": "Host Write Commands",
      "Index": 12,
      "Value": 187.3,
      "Min": 0.0,
      "Max": 187.3
    },
    {
      "SensorType": "TimeSpan",
      "Name": "Controller Busy Time",
      "Index": 13,
      "Value": 45600.0,
      "Min": 0.0,
      "Max": 45600.0
    }
  ],
  "SubHardware": []
}
```

---

### Network

Network adapter monitoring.

#### Available Sensors

| Sensor Type | Sensor Names | Unit | Description |
|-------------|--------------|------|-------------|
| `Throughput` | `Upload Speed`, `Download Speed` | B/s | Current network speeds |
| `Data` | `Data Uploaded`, `Data Downloaded` | GB | Session data transferred |
| `Load` | `Network Utilization` | % | Bandwidth utilization |

#### JSON Example

```json
{
  "HardwareType": "Network",
  "Name": "Intel I225-V",
  "Sensors": [
    {
      "SensorType": "Throughput",
      "Name": "Upload Speed",
      "Index": 0,
      "Value": 1250000.0,
      "Min": 0.0,
      "Max": 125000000.0
    },
    {
      "SensorType": "Throughput",
      "Name": "Download Speed",
      "Index": 1,
      "Value": 52500000.0,
      "Min": 0.0,
      "Max": 125000000.0
    },
    {
      "SensorType": "Data",
      "Name": "Data Downloaded",
      "Index": 0,
      "Value": 2.45,
      "Min": 0.0,
      "Max": 2.45
    }
  ],
  "SubHardware": []
}
```

---

### Battery

Laptop battery monitoring.

#### Available Sensors

| Sensor Type | Sensor Names | Unit | Description |
|-------------|--------------|------|-------------|
| `Level` | `Charge Level`, `Degradation Level` | % | Charge and wear percentages |
| `Voltage` | `Voltage` | V | Battery voltage |
| `Current` | `Current` | A | Charge/discharge current |
| `Power` | `Charge Rate`, `Discharge Rate` | W | Power flow |
| `Energy` | `Designed Capacity`, `Full Charged Capacity`, `Remaining Capacity` | mWh | Energy levels |
| `TimeSpan` | `Remaining Time (Estimated)` | s | Estimated time remaining |

#### JSON Example

```json
{
  "HardwareType": "Battery",
  "Name": "Generic Battery",
  "Sensors": [
    {
      "SensorType": "Level",
      "Name": "Charge Level",
      "Index": 0,
      "Value": 85.0,
      "Min": 15.0,
      "Max": 100.0
    },
    {
      "SensorType": "Voltage",
      "Name": "Voltage",
      "Index": 0,
      "Value": 12.6,
      "Min": 10.8,
      "Max": 13.2
    },
    {
      "SensorType": "Power",
      "Name": "Discharge Rate",
      "Index": 0,
      "Value": 25.5,
      "Min": 5.0,
      "Max": 65.0
    },
    {
      "SensorType": "TimeSpan",
      "Name": "Remaining Time",
      "Index": 0,
      "Value": 10800.0,
      "Min": 1800.0,
      "Max": 21600.0
    },
    {
      "SensorType": "Level",
      "Name": "Degradation Level",
      "Index": 1,
      "Value": 8.5,
      "Min": 0.0,
      "Max": 8.5
    }
  ],
  "SubHardware": []
}
```

---

### PSU (Psu)

Power Supply Unit monitoring (supported models: Corsair HXi, RMi, AXi series via Corsair Link).

#### Available Sensors

| Sensor Type | Sensor Names | Unit | Description |
|-------------|--------------|------|-------------|
| `Voltage` | `+12V`, `+5V`, `+3.3V`, `Input Voltage` | V | Rail voltages |
| `Current` | `+12V Current`, `+5V Current`, `+3.3V Current` | A | Rail currents |
| `Power` | `Input Power`, `Output Power` | W | Power input/output |
| `Temperature` | `Temperature #1`, `Temperature #2` | C | PSU temperatures |
| `Fan` | `Fan` | RPM | PSU fan speed |
| `Control` | `Fan Control` | % | Fan PWM level |
| `Load` | `Power Factor` | % | Power efficiency |

---

### Cooler

External cooling devices (AIO coolers, custom loop controllers like Corsair Commander, NZXT Kraken, Aquacomputer).

#### Available Sensors

| Sensor Type | Sensor Names | Unit | Description |
|-------------|--------------|------|-------------|
| `Temperature` | `Liquid Temperature`, `Water Temperature`, `VRM Temperature` | C | Coolant and component temperatures |
| `Fan` | `Fan #1` - `#6`, `Pump`, `Pump Speed` | RPM | Fan/pump speeds |
| `Control` | `Fan #1` - `#6`, `Pump` | % | PWM control levels |
| `Flow` | `Flow Rate` | L/h | Coolant flow rate |
| `Level` | `Fill Level` | % | Reservoir fill level |

---

### Embedded Controller (EmbeddedController)

Laptop embedded controller sensors (varies by manufacturer: Dell, HP, Lenovo, ASUS, etc.).

#### Available Sensors

| Sensor Type | Sensor Names | Unit | Description |
|-------------|--------------|------|-------------|
| `Temperature` | `CPU`, `GPU`, `Ambient`, `Motherboard`, `Charger`, `Memory` | C | EC-reported temperatures |
| `Fan` | `CPU Fan`, `GPU Fan`, `Fan #1`, `Fan #2` | RPM | EC-controlled fan speeds |
| `Voltage` | `CPU Core`, `Battery` | V | EC voltage readings |
| `Current` | `Charger Current` | A | Charging current |

---

## Sensor Types Reference

Complete list of all sensor types and their units:

| SensorType | Unit | Symbol | Description |
|------------|------|--------|-------------|
| `Voltage` | Volts | V | Electrical potential |
| `Current` | Amperes | A | Electrical current |
| `Power` | Watts | W | Power consumption |
| `Clock` | Megahertz | MHz | Frequency |
| `Temperature` | Celsius | C | Temperature |
| `Load` | Percent | % | Utilization |
| `Frequency` | Hertz | Hz | Generic frequency |
| `Fan` | RPM | RPM | Rotational speed |
| `Flow` | Liters/hour | L/h | Liquid flow rate |
| `Control` | Percent | % | Control level |
| `Level` | Percent | % | Fill/capacity level |
| `Factor` | Unitless | 1 | Multiplication factor |
| `Data` | Gigabytes | GB | Large data amounts |
| `SmallData` | Megabytes | MB | Small data amounts |
| `Throughput` | Bytes/second | B/s | Transfer rate |
| `TimeSpan` | Seconds | s | Time duration |
| `Energy` | Milliwatt-hours | mWh | Energy capacity |
| `Noise` | Decibels A-weighted | dBA | Sound level |
| `Conductivity` | Microsiemens/cm | S/cm | Liquid conductivity |
| `Humidity` | Percent | % | Relative humidity |

---

## Code Examples (Rust)

### Base Types and Hardware Monitor

```rust
use serde::Deserialize;
use std::ffi::{c_char, c_void, CStr};

// FFI bindings to the DLL
#[link(name = "ManagedxZenithHardwareMonitorWrapper")]
extern "C" {
    fn CreateHardwareMonitor() -> *mut c_void;
    fn UpdateHardwareMonitor(instance: *mut c_void);
    fn GetReport(instance: *mut c_void, buffer: *mut c_char, buffer_size: i32);
    fn GetReportSize(instance: *mut c_void) -> i32;
    fn DestroyHardwareMonitor(instance: *mut c_void);
}

// Core data structures
#[derive(Debug, Clone, Deserialize)]
pub struct Hardware {
    #[serde(rename = "HardwareType")]
    pub hardware_type: String,
    #[serde(rename = "Name")]
    pub name: String,
    #[serde(rename = "Sensors")]
    pub sensors: Vec<Sensor>,
    #[serde(rename = "SubHardware")]
    pub sub_hardware: Vec<Hardware>,
}

#[derive(Debug, Clone, Deserialize)]
pub struct Sensor {
    #[serde(rename = "SensorType")]
    pub sensor_type: String,
    #[serde(rename = "Name")]
    pub name: String,
    #[serde(rename = "Index")]
    pub index: i32,
    #[serde(rename = "Value")]
    pub value: f32,
    #[serde(rename = "Min")]
    pub min: f32,
    #[serde(rename = "Max")]
    pub max: f32,
}

// Hardware monitor wrapper
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

    pub fn get_report_size(&self) -> usize {
        unsafe { GetReportSize(self.instance) as usize }
    }

    pub fn get_report(&self) -> Vec<Hardware> {
        unsafe {
            // Get required size with minimum of 128KB for safety
            let size = (GetReportSize(self.instance) as usize).max(131072);
            let mut buffer = vec![0u8; size];
            GetReport(self.instance, buffer.as_mut_ptr() as *mut c_char, buffer.len() as i32);
            let json = CStr::from_ptr(buffer.as_ptr() as *const c_char)
                .to_string_lossy();
            serde_json::from_str(&json).unwrap_or_default()
        }
    }
}

impl Drop for HardwareMonitor {
    fn drop(&mut self) {
        unsafe { DestroyHardwareMonitor(self.instance) }
    }
}
```

### GPU Data Extraction

```rust
#[derive(Debug, Clone)]
pub struct GpuData {
    pub name: String,
    pub gpu_type: GpuType,
    pub core_temperature: Option<f32>,
    pub hot_spot_temperature: Option<f32>,
    pub core_load: Option<f32>,
    pub memory_load: Option<f32>,
    pub core_clock: Option<f32>,
    pub memory_clock: Option<f32>,
    pub power: Option<f32>,
    pub fan_rpm: Option<f32>,
    pub fan_percent: Option<f32>,
    pub memory_used_mb: Option<f32>,
    pub memory_total_mb: Option<f32>,
}

#[derive(Debug, Clone, PartialEq)]
pub enum GpuType {
    Nvidia,
    Amd,
    Intel,
}

impl GpuData {
    pub fn from_hardware(hw: &Hardware) -> Option<Self> {
        let gpu_type = match hw.hardware_type.as_str() {
            "GpuNvidia" => GpuType::Nvidia,
            "GpuAmd" => GpuType::Amd,
            "GpuIntel" => GpuType::Intel,
            _ => return None,
        };

        Some(GpuData {
            name: hw.name.clone(),
            gpu_type,
            core_temperature: hw.find_sensor("Temperature", "GPU Core"),
            hot_spot_temperature: hw.find_sensor("Temperature", "GPU Hot Spot"),
            core_load: hw.find_sensor("Load", "GPU Core"),
            memory_load: hw.find_sensor("Load", "GPU Memory Controller")
                .or_else(|| hw.find_sensor("Load", "GPU Memory")),
            core_clock: hw.find_sensor("Clock", "GPU Core"),
            memory_clock: hw.find_sensor("Clock", "GPU Memory"),
            power: hw.find_sensor("Power", "GPU Package")
                .or_else(|| hw.find_sensor("Power", "GPU Power")),
            fan_rpm: hw.find_sensor("Fan", "GPU Fan"),
            fan_percent: hw.find_sensor("Control", "GPU Fan"),
            memory_used_mb: hw.find_sensor("SmallData", "GPU Memory Used"),
            memory_total_mb: hw.find_sensor("SmallData", "GPU Memory Total"),
        })
    }
}

// Helper trait for Hardware
impl Hardware {
    pub fn find_sensor(&self, sensor_type: &str, name: &str) -> Option<f32> {
        self.sensors.iter()
            .find(|s| s.sensor_type == sensor_type && s.name == name)
            .map(|s| s.value)
    }

    pub fn find_sensor_containing(&self, sensor_type: &str, name_part: &str) -> Option<f32> {
        self.sensors.iter()
            .find(|s| s.sensor_type == sensor_type && s.name.contains(name_part))
            .map(|s| s.value)
    }
}
```

### Memory Data Extraction

```rust
#[derive(Debug, Clone)]
pub struct MemoryData {
    pub load_percent: Option<f32>,
    pub used_gb: Option<f32>,
    pub available_gb: Option<f32>,
    pub virtual_load_percent: Option<f32>,
    pub virtual_used_gb: Option<f32>,
}

impl MemoryData {
    pub fn from_hardware(hw: &Hardware) -> Option<Self> {
        if hw.hardware_type != "Memory" {
            return None;
        }

        Some(MemoryData {
            load_percent: hw.find_sensor("Load", "Memory"),
            used_gb: hw.find_sensor("Data", "Memory Used"),
            available_gb: hw.find_sensor("Data", "Memory Available"),
            virtual_load_percent: hw.find_sensor("Load", "Virtual Memory"),
            virtual_used_gb: hw.find_sensor("Data", "Virtual Memory Used"),
        })
    }
}
```

### Storage Data Extraction

```rust
#[derive(Debug, Clone)]
pub struct StorageData {
    pub name: String,
    pub temperature: Option<f32>,
    pub used_space_percent: Option<f32>,
    pub remaining_life_percent: Option<f32>,
    pub read_rate_bytes: Option<f32>,
    pub write_rate_bytes: Option<f32>,
    pub total_bytes_written_gb: Option<f32>,
}

impl StorageData {
    pub fn from_hardware(hw: &Hardware) -> Option<Self> {
        if hw.hardware_type != "Storage" {
            return None;
        }

        Some(StorageData {
            name: hw.name.clone(),
            temperature: hw.find_sensor("Temperature", "Temperature"),
            used_space_percent: hw.find_sensor("Load", "Used Space"),
            remaining_life_percent: hw.find_sensor("Level", "Remaining Life"),
            read_rate_bytes: hw.find_sensor("Throughput", "Read Rate"),
            write_rate_bytes: hw.find_sensor("Throughput", "Write Rate"),
            total_bytes_written_gb: hw.find_sensor_containing("Data", "Written"),
        })
    }
}
```

### Network Data Extraction

```rust
#[derive(Debug, Clone)]
pub struct NetworkData {
    pub name: String,
    pub upload_speed_bytes: Option<f32>,
    pub download_speed_bytes: Option<f32>,
    pub data_uploaded_gb: Option<f32>,
    pub data_downloaded_gb: Option<f32>,
}

impl NetworkData {
    pub fn from_hardware(hw: &Hardware) -> Option<Self> {
        if hw.hardware_type != "Network" {
            return None;
        }

        Some(NetworkData {
            name: hw.name.clone(),
            upload_speed_bytes: hw.find_sensor("Throughput", "Upload Speed"),
            download_speed_bytes: hw.find_sensor("Throughput", "Download Speed"),
            data_uploaded_gb: hw.find_sensor("Data", "Data Uploaded"),
            data_downloaded_gb: hw.find_sensor("Data", "Data Downloaded"),
        })
    }
}
```

### Battery Data Extraction

```rust
#[derive(Debug, Clone)]
pub struct BatteryData {
    pub charge_level_percent: Option<f32>,
    pub degradation_percent: Option<f32>,
    pub voltage: Option<f32>,
    pub discharge_rate_watts: Option<f32>,
    pub charge_rate_watts: Option<f32>,
    pub remaining_time_seconds: Option<f32>,
    pub designed_capacity_mwh: Option<f32>,
    pub full_capacity_mwh: Option<f32>,
    pub remaining_capacity_mwh: Option<f32>,
}

impl BatteryData {
    pub fn from_hardware(hw: &Hardware) -> Option<Self> {
        if hw.hardware_type != "Battery" {
            return None;
        }

        Some(BatteryData {
            charge_level_percent: hw.find_sensor("Level", "Charge Level"),
            degradation_percent: hw.find_sensor("Level", "Degradation Level"),
            voltage: hw.find_sensor("Voltage", "Voltage"),
            discharge_rate_watts: hw.find_sensor("Power", "Discharge Rate"),
            charge_rate_watts: hw.find_sensor("Power", "Charge Rate"),
            remaining_time_seconds: hw.find_sensor_containing("TimeSpan", "Remaining"),
            designed_capacity_mwh: hw.find_sensor("Energy", "Designed Capacity"),
            full_capacity_mwh: hw.find_sensor("Energy", "Full Charged Capacity"),
            remaining_capacity_mwh: hw.find_sensor("Energy", "Remaining Capacity"),
        })
    }
}
```

### Motherboard/SuperIO Data Extraction

```rust
#[derive(Debug, Clone)]
pub struct MotherboardData {
    pub name: String,
    pub cpu_fan_rpm: Option<f32>,
    pub system_fans: Vec<FanData>,
    pub voltages: Vec<VoltageData>,
    pub temperatures: Vec<TemperatureData>,
}

#[derive(Debug, Clone)]
pub struct FanData {
    pub name: String,
    pub rpm: f32,
    pub control_percent: Option<f32>,
}

#[derive(Debug, Clone)]
pub struct VoltageData {
    pub name: String,
    pub voltage: f32,
}

#[derive(Debug, Clone)]
pub struct TemperatureData {
    pub name: String,
    pub temperature: f32,
}

impl MotherboardData {
    pub fn from_hardware(hw: &Hardware) -> Option<Self> {
        if hw.hardware_type != "Motherboard" {
            return None;
        }

        let mut data = MotherboardData {
            name: hw.name.clone(),
            cpu_fan_rpm: None,
            system_fans: Vec::new(),
            voltages: Vec::new(),
            temperatures: Vec::new(),
        };

        // Process SubHardware (SuperIO chips)
        for sub in &hw.sub_hardware {
            if sub.hardware_type == "SuperIO" {
                data.process_superio(sub);
            }
        }

        Some(data)
    }

    fn process_superio(&mut self, superio: &Hardware) {
        for sensor in &superio.sensors {
            match sensor.sensor_type.as_str() {
                "Fan" => {
                    if sensor.name == "CPU Fan" {
                        self.cpu_fan_rpm = Some(sensor.value);
                    }
                    self.system_fans.push(FanData {
                        name: sensor.name.clone(),
                        rpm: sensor.value,
                        control_percent: superio.find_sensor("Control", &sensor.name),
                    });
                }
                "Voltage" => {
                    self.voltages.push(VoltageData {
                        name: sensor.name.clone(),
                        voltage: sensor.value,
                    });
                }
                "Temperature" => {
                    self.temperatures.push(TemperatureData {
                        name: sensor.name.clone(),
                        temperature: sensor.value,
                    });
                }
                _ => {}
            }
        }
    }
}
```

### Complete System Overview

```rust
#[derive(Debug, Clone)]
pub struct SystemOverview {
    pub cpu: Option<CpuData>,
    pub gpu: Option<GpuData>,
    pub memory: Option<MemoryData>,
    pub storage: Vec<StorageData>,
    pub network: Vec<NetworkData>,
    pub battery: Option<BatteryData>,
    pub motherboard: Option<MotherboardData>,
}

impl SystemOverview {
    pub fn from_report(report: &[Hardware]) -> Self {
        let mut overview = SystemOverview {
            cpu: None,
            gpu: None,
            memory: None,
            storage: Vec::new(),
            network: Vec::new(),
            battery: None,
            motherboard: None,
        };

        for hw in report {
            match hw.hardware_type.as_str() {
                "Cpu" => overview.cpu = CpuData::from_hardware(hw),
                "GpuNvidia" | "GpuAmd" | "GpuIntel" => {
                    if overview.gpu.is_none() {
                        overview.gpu = GpuData::from_hardware(hw);
                    }
                }
                "Memory" => overview.memory = MemoryData::from_hardware(hw),
                "Storage" => {
                    if let Some(storage) = StorageData::from_hardware(hw) {
                        overview.storage.push(storage);
                    }
                }
                "Network" => {
                    if let Some(network) = NetworkData::from_hardware(hw) {
                        overview.network.push(network);
                    }
                }
                "Battery" => overview.battery = BatteryData::from_hardware(hw),
                "Motherboard" => overview.motherboard = MotherboardData::from_hardware(hw),
                _ => {}
            }
        }

        overview
    }
}

// Usage example
fn main() -> Result<(), Box<dyn std::error::Error>> {
    let monitor = HardwareMonitor::new()?;
    
    loop {
        monitor.update();
        let report = monitor.get_report();
        let overview = SystemOverview::from_report(&report);
        
        if let Some(cpu) = &overview.cpu {
            println!("CPU: {} - Temp: {:?}C, Load: {:?}%", 
                cpu.name, cpu.temperature, cpu.total_load);
        }
        
        if let Some(gpu) = &overview.gpu {
            println!("GPU: {} - Temp: {:?}C, Load: {:?}%", 
                gpu.name, gpu.core_temperature, gpu.core_load);
        }
        
        if let Some(mem) = &overview.memory {
            println!("Memory: {:?}% used ({:?} GB)", 
                mem.load_percent, mem.used_gb);
        }
        
        std::thread::sleep(std::time::Duration::from_secs(1));
    }
}
```

### Temperature Alert System

```rust
#[derive(Debug, Clone)]
pub struct TemperatureAlert {
    pub hardware_name: String,
    pub sensor_name: String,
    pub temperature: f32,
    pub threshold: f32,
}

pub fn check_temperatures(report: &[Hardware], threshold: f32) -> Vec<TemperatureAlert> {
    let mut alerts = Vec::new();
    
    fn check_hardware(hw: &Hardware, threshold: f32, alerts: &mut Vec<TemperatureAlert>) {
        for sensor in &hw.sensors {
            if sensor.sensor_type == "Temperature" && sensor.value >= threshold {
                alerts.push(TemperatureAlert {
                    hardware_name: hw.name.clone(),
                    sensor_name: sensor.name.clone(),
                    temperature: sensor.value,
                    threshold,
                });
            }
        }
        for sub in &hw.sub_hardware {
            check_hardware(sub, threshold, alerts);
        }
    }
    
    for hw in report {
        check_hardware(hw, threshold, &mut alerts);
    }
    
    alerts
}
```

---

## Best Practices

1. **Poll at reasonable intervals** - 500ms to 2000ms is recommended
2. **Handle missing sensors** - Not all systems have all sensor types
3. **Check SubHardware** - Motherboard sensors are often in SuperIO SubHardware
4. **Monitor Min/Max values** - Useful for tracking thermal throttling
5. **Filter by SensorType first** - More efficient than iterating all sensors
