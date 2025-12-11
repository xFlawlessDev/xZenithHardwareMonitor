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
| `Temperature` | **Intel**: `CPU Package`, `CPU Core #1` - `#N`, `Core Max`, `Core Average`, `CPU Core #1 Distance to TjMax` - `#N Distance to TjMax`<br>**AMD Ryzen**: `Core (Tctl)`, `Core (Tdie)`, `Core (Tctl/Tdie)`, `CCD1 (Tdie)` - `CCD8 (Tdie)`, `CCDs Max (Tdie)`, `CCDs Average (Tdie)`<br>**AMD Legacy**: `CPU Cores`, `Core #N` | C | Core and package temperatures |
| `Load` | `CPU Total`, `CPU Core #1` - `#N`, `CPU Core Max`, `CPU Core #1 Thread #1`, `CPU Core #1 Thread #2` | % | Utilization per core/thread and total |
| `Clock` | **All**: `Bus Speed`, `CPU Core #1` - `#N`<br>**AMD Ryzen**: `Cores (Average)`, `Cores (Average Effective)`, `Core #N (Effective)` | MHz | Operating frequencies |
| `Power` | **Intel**: `CPU Package`, `CPU Cores`, `CPU Graphics`, `CPU Memory`, `CPU Platform`<br>**AMD Ryzen**: `Package`, `Core #N (SMU)` (plus SMU sensors if available) | W | Power consumption |
| `Voltage` | **Intel**: `CPU Core`, `CPU Core #1` - `#N` (VID readings)<br>**AMD Ryzen**: `Core (SVI2 TFN)`, `SoC (SVI2 TFN)`, `Core #N VID`<br>**AMD Legacy**: `CPU Cores`, `Northbridge` | V | Core voltages |
| `Factor` | **AMD Ryzen**: `Core #N` (multiplier) | 1 | CPU multiplier per core |
| `Level` | **AMD Legacy**: `CPU Package C2`, `CPU Package C3` | % | C-state residency |

#### Per-Core Sensor Details

**Intel CPUs** expose per-core sensors with the naming pattern `CPU Core #N` where N is 1-based:

| Sensor Type | Per-Core Pattern | Description |
|-------------|------------------|-------------|
| `Temperature` | `CPU Core #1`, `CPU Core #2`, ... | Individual core temperatures |
| `Temperature` | `CPU Core #1 Distance to TjMax`, ... | Distance from thermal throttle point |
| `Load` | `CPU Core #1`, `CPU Core #2`, ... | Per-core utilization percentage |
| `Load` | `CPU Core #1 Thread #1`, `CPU Core #1 Thread #2` | Per-thread load (SMT/HT enabled) |
| `Clock` | `CPU Core #1`, `CPU Core #2`, ... | Per-core clock frequency |

**Intel 12th Gen+** (Hybrid Architecture) also provides:

| Sensor Type | Pattern | Description |
|-------------|---------|-------------|
| `Load` | `P-Core #1`, `P-Core #2`, ... | Performance core utilization |
| `Load` | `E-Core #1`, `E-Core #2`, ... | Efficiency core utilization |

**AMD Ryzen CPUs** expose extensive per-core monitoring:

| Sensor Type | Per-Core Pattern | Description |
|-------------|------------------|-------------|
| `Clock` | `Core #1`, `Core #2`, ... | Per-core clock frequency |
| `Clock` | `Core #1 (Effective)`, `Core #2 (Effective)`, ... | Actual effective clock accounting for idle |
| `Power` | `Core #1 (SMU)`, `Core #2 (SMU)`, ... | Per-core power consumption via SMU |
| `Voltage` | `Core #1 VID`, `Core #2 VID`, ... | Per-core voltage ID requested |
| `Factor` | `Core #1`, `Core #2`, ... | Per-core multiplier |

**AMD Ryzen Aggregate Sensors**:

| Sensor Type | Name | Description |
|-------------|------|-------------|
| `Clock` | `Cores (Average)` | Average clock across all cores |
| `Clock` | `Cores (Average Effective)` | Average effective clock (accounts for idle) |
| `Temperature` | `CCDs Max (Tdie)` | Hottest CCD temperature |
| `Temperature` | `CCDs Average (Tdie)` | Average temperature across CCDs |

**Thread-Level Load Monitoring** (SMT/Hyperthreading):

For CPUs with SMT (AMD) or Hyperthreading (Intel), each physical core may report multiple thread loads:

```
CPU Core #1 Thread #1  -> First logical processor on Core 1
CPU Core #1 Thread #2  -> Second logical processor on Core 1
CPU Core #2 Thread #1  -> First logical processor on Core 2
CPU Core #2 Thread #2  -> Second logical processor on Core 2
...
```

**Note**: The number of threads per core varies:
- Intel P-cores: 2 threads (with HT enabled)
- Intel E-cores: 1 thread (no HT)
- AMD Ryzen: 2 threads per core (with SMT enabled)

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

#### Per-Core Monitoring Example (Rust)

```rust
use serde::Deserialize;
use regex::Regex;

#[derive(Debug, Clone)]
pub struct PerCoreData {
    pub core_id: u32,
    pub temperature: Option<f32>,
    pub load: Option<f32>,
    pub thread_loads: Vec<f32>,      // Per-thread loads if SMT/HT enabled
    pub clock_mhz: Option<f32>,
    pub effective_clock_mhz: Option<f32>,  // AMD Ryzen only
    pub power_watts: Option<f32>,          // AMD Ryzen SMU
    pub voltage: Option<f32>,              // VID
    pub multiplier: Option<f32>,           // AMD Ryzen only
}

#[derive(Debug, Clone)]
pub struct CpuPerCoreMonitor {
    pub name: String,
    pub core_count: usize,
    pub thread_count: usize,
    pub package_temp: Option<f32>,
    pub total_load: Option<f32>,
    pub package_power: Option<f32>,
    pub cores: Vec<PerCoreData>,
    // AMD Ryzen specific
    pub avg_clock: Option<f32>,
    pub avg_effective_clock: Option<f32>,
}

impl CpuPerCoreMonitor {
    pub fn from_hardware(hw: &Hardware) -> Option<Self> {
        if hw.hardware_type != "Cpu" {
            return None;
        }

        let core_re = Regex::new(r"(?:CPU )?Core #(\d+)").unwrap();
        let thread_re = Regex::new(r"Core #(\d+) Thread #(\d+)").unwrap();
        
        // Find max core number to determine core count
        let mut max_core = 0u32;
        for sensor in &hw.sensors {
            if let Some(caps) = core_re.captures(&sensor.name) {
                if let Ok(n) = caps[1].parse::<u32>() {
                    max_core = max_core.max(n);
                }
            }
        }
        
        let core_count = max_core as usize;
        let mut cores: Vec<PerCoreData> = (1..=core_count as u32)
            .map(|id| PerCoreData {
                core_id: id,
                temperature: None,
                load: None,
                thread_loads: Vec::new(),
                clock_mhz: None,
                effective_clock_mhz: None,
                power_watts: None,
                voltage: None,
                multiplier: None,
            })
            .collect();
        
        // Populate per-core data
        for sensor in &hw.sensors {
            // Extract core number from sensor name
            if let Some(caps) = core_re.captures(&sensor.name) {
                if let Ok(core_num) = caps[1].parse::<u32>() {
                    if let Some(core) = cores.get_mut((core_num - 1) as usize) {
                        match sensor.sensor_type.as_str() {
                            "Temperature" if !sensor.name.contains("Distance") => {
                                core.temperature = Some(sensor.value);
                            }
                            "Load" if sensor.name.contains("Thread") => {
                                // Thread-level load
                                if let Some(tcaps) = thread_re.captures(&sensor.name) {
                                    if let Ok(thread_num) = tcaps[2].parse::<usize>() {
                                        while core.thread_loads.len() < thread_num {
                                            core.thread_loads.push(0.0);
                                        }
                                        core.thread_loads[thread_num - 1] = sensor.value;
                                    }
                                }
                            }
                            "Load" => {
                                core.load = Some(sensor.value);
                            }
                            "Clock" if sensor.name.contains("Effective") => {
                                core.effective_clock_mhz = Some(sensor.value);
                            }
                            "Clock" => {
                                core.clock_mhz = Some(sensor.value);
                            }
                            "Power" if sensor.name.contains("SMU") => {
                                core.power_watts = Some(sensor.value);
                            }
                            "Voltage" if sensor.name.contains("VID") => {
                                core.voltage = Some(sensor.value);
                            }
                            "Factor" => {
                                core.multiplier = Some(sensor.value);
                            }
                            _ => {}
                        }
                    }
                }
            }
        }
        
        // Calculate thread count
        let thread_count = cores.iter()
            .map(|c| c.thread_loads.len().max(1))
            .sum();
        
        Some(CpuPerCoreMonitor {
            name: hw.name.clone(),
            core_count,
            thread_count,
            package_temp: hw.find_sensor_containing("Temperature", "Package")
                .or_else(|| hw.find_sensor_containing("Temperature", "Tctl")),
            total_load: hw.find_sensor("Load", "CPU Total"),
            package_power: hw.find_sensor_containing("Power", "Package"),
            cores,
            avg_clock: hw.find_sensor("Clock", "Cores (Average)"),
            avg_effective_clock: hw.find_sensor("Clock", "Cores (Average Effective)"),
        })
    }
    
    /// Get the hottest core
    pub fn hottest_core(&self) -> Option<(u32, f32)> {
        self.cores.iter()
            .filter_map(|c| c.temperature.map(|t| (c.core_id, t)))
            .max_by(|a, b| a.1.partial_cmp(&b.1).unwrap())
    }
    
    /// Get the most loaded core
    pub fn busiest_core(&self) -> Option<(u32, f32)> {
        self.cores.iter()
            .filter_map(|c| c.load.map(|l| (c.core_id, l)))
            .max_by(|a, b| a.1.partial_cmp(&b.1).unwrap())
    }
    
    /// Get average clock across all cores
    pub fn average_clock(&self) -> Option<f32> {
        let clocks: Vec<f32> = self.cores.iter()
            .filter_map(|c| c.clock_mhz)
            .collect();
        if clocks.is_empty() {
            None
        } else {
            Some(clocks.iter().sum::<f32>() / clocks.len() as f32)
        }
    }
}

// Usage example
fn monitor_per_core(hw: &Hardware) {
    if let Some(cpu) = CpuPerCoreMonitor::from_hardware(hw) {
        println!("CPU: {} ({} cores, {} threads)", cpu.name, cpu.core_count, cpu.thread_count);
        println!("Package: {:?}°C, {:?}W, {:?}% load", 
            cpu.package_temp, cpu.package_power, cpu.total_load);
        
        for core in &cpu.cores {
            print!("  Core #{}: ", core.core_id);
            if let Some(temp) = core.temperature {
                print!("{:.1}°C ", temp);
            }
            if let Some(load) = core.load {
                print!("{:.1}% ", load);
            }
            if let Some(clock) = core.clock_mhz {
                print!("{:.0} MHz ", clock);
            }
            if !core.thread_loads.is_empty() {
                print!("(threads: {:?})", core.thread_loads);
            }
            println!();
        }
        
        if let Some((id, temp)) = cpu.hottest_core() {
            println!("Hottest: Core #{} at {:.1}°C", id, temp);
        }
    }
}
```

---

### GPU - NVIDIA (GpuNvidia)

NVIDIA graphics cards provide extensive monitoring through NVML/NVAPI.

#### Available Sensors

| Sensor Type | Sensor Names | Unit | Description |
|-------------|--------------|------|-------------|
| `Temperature` | GPU sensors from thermal settings, `GPU Hot Spot`, `GPU Memory Junction` | C | GPU temperatures |
| `Load` | `GPU Core`, `GPU Memory Controller`, `GPU Memory`, `GPU Video Engine`, `GPU Bus`, `D3D 3D`, `D3D Copy`, `D3D Video Decode`, `D3D Video Encode`, `D3D Cuda` (D3D nodes), power utilization sensors | % | Utilization metrics |
| `Clock` | Dynamic based on available clocks from NVAPI | MHz | Clock speeds |
| `Power` | `GPU Package` | W | Power consumption via NVML |
| `Fan` | `GPU` or named fans based on cooler info | RPM | Fan speeds |
| `Control` | Fan control sensors matching fan sensors | % | Fan control level |
| `SmallData` | `GPU Memory Free`, `GPU Memory Used`, `GPU Memory Total`, `D3D Dedicated Memory Used`, `D3D Shared Memory Used` | MB | VRAM usage |
| `Throughput` | `GPU PCIe Rx`, `GPU PCIe Tx` | B/s | PCIe bandwidth |

**Note**: Power utilization may also be reported as Load type sensors. Use `GPU Package` (Power type) for accurate power readings via NVML.

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
| `Temperature` | `GPU Core`, `GPU Memory`, `GPU VR VDDC`, `GPU VR MVDD`, `GPU VR SoC`, `GPU Liquid`, `GPU PLX`, `GPU Hot Spot` | C | Various temperature points |
| `Load` | `GPU Core`, `GPU Memory`, `D3D 3D`, `D3D Copy`, `D3D Video Decode`, `D3D Video Encode` (D3D nodes) | % | Utilization |
| `Clock` | `GPU Core`, `GPU SoC`, `GPU Memory` | MHz | Clock speeds |
| `Power` | `GPU Core`, `GPU PPT`, `GPU SoC`, `GPU Package` | W | Power consumption |
| `Fan` | `GPU Fan` | RPM | Fan speed |
| `Control` | `GPU Fan` | % | Fan control percentage |
| `Voltage` | `GPU Core`, `GPU Memory`, `GPU SoC` | V | Voltages |
| `SmallData` | `GPU Memory Used`, `GPU Memory Free`, `GPU Memory Total`, `D3D Dedicated Memory Used/Free/Total`, `D3D Shared Memory Used/Free/Total` | MB | VRAM usage |
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

Intel integrated and discrete graphics monitoring. Supports both integrated (UHD/Iris) and discrete (Arc) GPUs.

#### Available Sensors (Integrated GPU)

| Sensor Type | Sensor Names | Unit | Description |
|-------------|--------------|------|-------------|
| `Load` | `D3D 3D`, `D3D Copy`, `D3D Video Decode`, `D3D Video Encode` | % | D3D utilization |
| `Power` | `GPU Power` | W | Power consumption |
| `SmallData` | `D3D Dedicated Memory Used`, `D3D Shared Memory Used`, `D3D Shared Memory Free`, `D3D Shared Memory Total` | MB | Memory usage |

#### Available Sensors (Discrete GPU - Intel Arc)

| Sensor Type | Sensor Names | Unit | Description |
|-------------|--------------|------|-------------|
| `Temperature` | `GPU Core`, `GPU Memory` | C | GPU temperatures |
| `Clock` | `GPU Core`, `GPU Memory` | MHz | Clock speeds |
| `Voltage` | `GPU Core`, `GPU Memory` | V | Voltages |
| `Power` | `GPU Package`, `GPU Total` | W | Power consumption |
| `Load` | `GPU Core`, `GPU Render/Compute`, `GPU Media`, `GPU Memory` | % | Utilization |
| `SmallData` | `GPU Memory Free`, `GPU Memory Used`, `GPU Memory Total` | MB | VRAM usage |
| `Throughput` | `GPU Memory Read`, `GPU Memory Write` | B/s | Memory bandwidth |
| `Fan` | `Fan #1`, `Fan #2`, etc. | RPM | Fan speeds |

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
      "SensorType": "Load",
      "Name": "D3D 3D",
      "Index": 0,
      "Value": 12.5,
      "Min": 0.0,
      "Max": 95.0
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

#### JSON Example (Discrete GPU - Intel Arc)

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
      "Min": 35.0,
      "Max": 75.0
    },
    {
      "SensorType": "Temperature",
      "Name": "GPU Memory",
      "Index": 1,
      "Value": 52.0,
      "Min": 32.0,
      "Max": 70.0
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
      "Value": 2187.0,
      "Min": 2187.0,
      "Max": 2187.0
    },
    {
      "SensorType": "Power",
      "Name": "GPU Package",
      "Index": 0,
      "Value": 175.0,
      "Min": 15.0,
      "Max": 225.0
    },
    {
      "SensorType": "Load",
      "Name": "GPU Core",
      "Index": 0,
      "Value": 85.0,
      "Min": 0.0,
      "Max": 100.0
    },
    {
      "SensorType": "Load",
      "Name": "GPU Render/Compute",
      "Index": 1,
      "Value": 80.0,
      "Min": 0.0,
      "Max": 100.0
    },
    {
      "SensorType": "SmallData",
      "Name": "GPU Memory Used",
      "Index": 1,
      "Value": 8192.0,
      "Min": 256.0,
      "Max": 16384.0
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
      "SensorType": "Throughput",
      "Name": "GPU Memory Read",
      "Index": 0,
      "Value": 450000000000.0,
      "Min": 0.0,
      "Max": 560000000000.0
    }
  ],
  "SubHardware": []
}
```

---

### Memory

System RAM monitoring including total/virtual memory usage and per-DIMM SPD data.

#### Available Sensors (Total/Virtual Memory)

| Sensor Type | Sensor Names | Unit | Description |
|-------------|--------------|------|-------------|
| `Load` | `Memory`, `Virtual Memory` | % | Memory utilization |
| `Data` | `Memory Used`, `Memory Available`, `Virtual Memory Used`, `Virtual Memory Available` | GB | Memory amounts |

#### Available Sensors (DIMM Memory - SubHardware)

Individual RAM modules are exposed as SubHardware with SPD (Serial Presence Detect) data.

| Sensor Type | Sensor Names | Unit | Description |
|-------------|--------------|------|-------------|
| `Temperature` | `DIMM #N`, `Temperature Sensor Resolution` | C | DIMM temperature (DDR4/DDR5 with thermal sensor) |
| `Timing` | `tCKAVGmin`, `tCKAVGmax`, `tAA`, `tRCD`, `tRP`, `tRAS`, `tRC`, `tRFC1`-`tRFC4`, `tFAW`, `tRRD_S`, `tRRD_L`, `tCCD_L`, `tWR`, `tWTR_S`, `tWTR_L` | ns | SDRAM timing parameters |
| `Data` | `Capacity` | GB | DIMM capacity |

**DDR4 Timing Sensors:**
- `tCKAVGmin/max` - Minimum/Maximum Cycle Time
- `tAA` - CAS Latency Time
- `tRCD` - RAS to CAS Delay Time
- `tRP` - Row Precharge Delay Time
- `tRAS` - Active to Precharge Delay Time
- `tRC` - Active to Active/Refresh Delay Time
- `tRFC1/2/4` - Refresh Recovery Delay Times
- `tFAW` - Four Activate Window Time
- `tRRD_S/L` - Activate to Activate Delay (Different/Same Bank Group)
- `tCCD_L` - CAS to CAS Delay Time (Same Bank Group)
- `tWR` - Write Recovery Time
- `tWTR_S/L` - Write to Read Time (Different/Same Bank Group)

**DDR5 Timing Sensors:**
- Same as DDR4 plus:
- `tRFCsb` - Same Bank Refresh Recovery Time
- `tRFC1_dlr/tRFC2_dlr/tRFCsb_dlr` - 3DS (3D Stacked) timing variants

#### JSON Example (Total Memory)

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
  "SubHardware": [
    {
      "HardwareType": "Memory",
      "Name": "G.Skill DDR4-3600 16GB",
      "Sensors": [
        {
          "SensorType": "Temperature",
          "Name": "DIMM #0",
          "Index": 0,
          "Value": 42.0,
          "Min": 35.0,
          "Max": 55.0
        },
        {
          "SensorType": "Timing",
          "Name": "tAA (CAS Latency Time)",
          "Index": 3,
          "Value": 13.75,
          "Min": 13.75,
          "Max": 13.75
        },
        {
          "SensorType": "Timing",
          "Name": "tRCD (RAS to CAS Delay Time)",
          "Index": 4,
          "Value": 13.75,
          "Min": 13.75,
          "Max": 13.75
        },
        {
          "SensorType": "Timing",
          "Name": "tRP (Row Precharge Delay Time)",
          "Index": 5,
          "Value": 13.75,
          "Min": 13.75,
          "Max": 13.75
        },
        {
          "SensorType": "Timing",
          "Name": "tRAS (Active to Precharge Delay Time)",
          "Index": 6,
          "Value": 32.0,
          "Min": 32.0,
          "Max": 32.0
        },
        {
          "SensorType": "Data",
          "Name": "Capacity",
          "Index": 18,
          "Value": 16.0,
          "Min": 16.0,
          "Max": 16.0
        }
      ],
      "SubHardware": []
    }
  ]
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

HDD, SSD, and NVMe drive monitoring powered by DiskInfoToolkit. Sensor availability varies by drive type and manufacturer.

#### Core Sensors (All Drives)

| Sensor Type | Sensor Names | Unit | Description |
|-------------|--------------|------|-------------|
| `Temperature` | `Temperature` | °C | Primary drive temperature |
| `Load` | `Used Space` | % | Drive capacity usage |
| `Load` | `Read Activity`, `Write Activity`, `Total Activity` | % | Real-time I/O activity |
| `Throughput` | `Read Rate`, `Write Rate` | B/s | Current transfer speeds |
| `Level` | `Life` | % | Remaining drive life (if available) |
| `Data` | `Data read`, `Data written` | GB | Lifetime data transferred |
| `Factor` | `Power on count`, `Power on hours` | count/hours | Lifecycle counters |

#### NVMe-Specific Sensors

| Sensor Type | Sensor Names | Unit | Description |
|-------------|--------------|------|-------------|
| `Factor` | `Temperature warning`, `Temperature critical` | count | Temperature threshold breach counts |
| `Temperature` | `Temperature 1` - `Temperature 8` | °C | Additional NVMe temperature sensors (if available) |
| `Level` | `Available Spare`, `Available Spare Threshold`, `Percentage Used` | % | NVMe health indicators |
| `Data` | `Host Data Read`, `Host Data Written` | GB | Total data transferred |
| `Data` | `Host Read Commands`, `Host Write Commands` | millions | I/O command counts |
| `SmallData` | `Power Cycle Count`, `Unsafe Shutdown Count` | count | Lifecycle counters |
| `SmallData` | `Media and Data Integrity Errors`, `Error Information Log Entries` | count | Error tracking |
| `SmallData` | `TMT1 Transition Count`, `TMT2 Transition Count` | count | Thermal management events |
| `TimeSpan` | `Power On Hours`, `Controller Busy Time` | seconds | Time metrics |
| `TimeSpan` | `Warning Composite Temperature Time`, `Critical Composite Temperature Time` | seconds | Time above thermal thresholds |
| `TimeSpan` | `Total Time TMT1`, `Total Time TMT2` | seconds | Thermal management durations |

#### SMART Attribute Sensors (via SmartAttributeTranslator)

Additional sensors are automatically created based on the drive type (HDD, SSD vendor, NVMe). These include vendor-specific attributes from DiskInfoToolkit.

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
      "SensorType": "Level",
      "Name": "Life",
      "Index": 1,
      "Value": 98.0,
      "Min": 98.0,
      "Max": 100.0
    },
    {
      "SensorType": "Data",
      "Name": "Data read",
      "Index": 2,
      "Value": 12840.7,
      "Min": 0.0,
      "Max": 12840.7
    },
    {
      "SensorType": "Data",
      "Name": "Data written",
      "Index": 3,
      "Value": 8256.3,
      "Min": 0.0,
      "Max": 8256.3
    },
    {
      "SensorType": "Factor",
      "Name": "Power on count",
      "Index": 4,
      "Value": 1247.0,
      "Min": 0.0,
      "Max": 1247.0
    },
    {
      "SensorType": "Factor",
      "Name": "Power on hours",
      "Index": 5,
      "Value": 1620.0,
      "Min": 0.0,
      "Max": 1620.0
    },
    {
      "SensorType": "Factor",
      "Name": "Temperature warning",
      "Index": 6,
      "Value": 0.0,
      "Min": 0.0,
      "Max": 0.0
    },
    {
      "SensorType": "Factor",
      "Name": "Temperature critical",
      "Index": 7,
      "Value": 0.0,
      "Min": 0.0,
      "Max": 0.0
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
      "Index": 51,
      "Value": 12.5,
      "Min": 0.0,
      "Max": 100.0
    },
    {
      "SensorType": "Load",
      "Name": "Write Activity",
      "Index": 52,
      "Value": 8.2,
      "Min": 0.0,
      "Max": 100.0
    },
    {
      "SensorType": "Load",
      "Name": "Total Activity",
      "Index": 53,
      "Value": 20.7,
      "Min": 0.0,
      "Max": 100.0
    },
    {
      "SensorType": "Throughput",
      "Name": "Read Rate",
      "Index": 54,
      "Value": 524288000.0,
      "Min": 0.0,
      "Max": 7000000000.0
    },
    {
      "SensorType": "Throughput",
      "Name": "Write Rate",
      "Index": 55,
      "Value": 104857600.0,
      "Min": 0.0,
      "Max": 5000000000.0
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

Embedded controller sensors for motherboards (primarily ASUS boards with EC support). Also supports ChromeOS embedded controllers.

#### Available Sensors (ASUS Boards)

| Sensor Type | Sensor Names | Unit | Description |
|-------------|--------------|------|-------------|
| `Temperature` | `Chipset`, `CPU`, `CPU Package`, `Motherboard`, `T Sensor`, `T Sensor 2`, `VRM`, `Water In`, `Water Out`, `Water Block In` | C | EC-reported temperatures |
| `Fan` | `CPU Optional Fan`, `VRM Heat Sink Fan`, `Chipset Fan`, `Water Pump` | RPM | EC-controlled fan speeds |
| `Flow` | `Water Flow` | L/h | Custom loop water flow rate |
| `Voltage` | `CPU Core` | V | EC voltage readings |
| `Current` | `CPU` | A | CPU current draw |

#### Available Sensors (ChromeOS EC)

| Sensor Type | Sensor Names | Unit | Description |
|-------------|--------------|------|-------------|
| `Temperature` | Dynamic based on EC sensors | C | Temperature sensors |
| `Fan` | Dynamic based on EC fans | RPM | Fan speeds |

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
| `Timing` | Nanoseconds | ns | Memory timing values |
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
