# xZenithHardwareMonitorLib

[![GitHub](https://img.shields.io/badge/GitHub-xZenithhardwareMonitor-blue?logo=github)](https://github.com/xFlawlessDev/xZenithhardwareMonitor)
[![License: MPL 2.0](https://img.shields.io/badge/License-MPL%202.0-brightgreen.svg)](LICENSE)

The core hardware monitoring library for the xZenith Hardware Monitor project. This is a customized fork of [LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor) optimized for integration with xZenithHardwareMonitorAPI.

## Overview

xZenithHardwareMonitorLib is a .NET library that provides low-level access to hardware sensors on Windows systems. It reads temperature, fan speed, voltage, load, clock speed, and other sensor data from various hardware components.

## Components

| Name | Description | .NET Framework |
|------|-------------|----------------|
| **xZenithHardwareMonitorLib** | Core library for hardware sensor access | .NET Framework 4.7.2 |
| **xZenithHardwareMonitor** | Windows Forms GUI application | .NET Framework 4.7.2 |
| **Aga.Controls** | UI tree view controls | .NET Framework 4.7.2 |

## Supported Hardware

| Category | Devices |
|----------|---------|
| **Motherboards** | Various manufacturers (ASUS, MSI, Gigabyte, ASRock, etc.) |
| **Processors** | Intel and AMD CPUs |
| **Graphics Cards** | NVIDIA, AMD, and Intel GPUs |
| **Storage** | HDD, SSD, and NVMe drives |
| **Network** | Network adapters |
| **Memory** | System RAM |
| **Battery** | Laptop batteries |
| **PSU** | Supported power supplies |

## Sensor Types

- **Temperature** - Component temperatures in Celsius
- **Fan** - Fan speeds in RPM
- **Voltage** - Voltage readings in Volts
- **Load** - Utilization percentages
- **Clock** - Operating frequencies in MHz
- **Power** - Power consumption in Watts
- **Data** - Data amounts in GB
- **Throughput** - Transfer rates in MB/s

## Building

### Requirements

- Visual Studio 2017 or later
- .NET Framework 4.7.2 SDK
- Windows SDK

### Build Steps

1. Open `xZenithHardwareMonitor.sln` in Visual Studio
2. Select configuration (Release/Any CPU recommended)
3. Build Solution (Ctrl+Shift+B)

### Output

```
bin/Release/
├── xZenithHardwareMonitorLib.dll    (Core library)
├── xZenithHardwareMonitor.exe       (GUI application)
└── Aga.Controls.dll                 (UI controls)
```

## Usage

### Basic Example

```csharp
using xZenithHardwareMonitor.Hardware;

public class UpdateVisitor : IVisitor
{
    public void VisitComputer(IComputer computer)
    {
        computer.Traverse(this);
    }
    
    public void VisitHardware(IHardware hardware)
    {
        hardware.Update();
        foreach (IHardware subHardware in hardware.SubHardware) 
            subHardware.Accept(this);
    }
    
    public void VisitSensor(ISensor sensor) { }
    public void VisitParameter(IParameter parameter) { }
}

public void Monitor()
{
    Computer computer = new Computer
    {
        IsCpuEnabled = true,
        IsGpuEnabled = true,
        IsMemoryEnabled = true,
        IsMotherboardEnabled = true,
        IsControllerEnabled = true,
        IsNetworkEnabled = true,
        IsStorageEnabled = true,
        IsBatteryEnabled = true
    };

    computer.Open();
    computer.Accept(new UpdateVisitor());

    foreach (IHardware hardware in computer.Hardware)
    {
        Console.WriteLine("Hardware: {0}", hardware.Name);
        
        foreach (IHardware subhardware in hardware.SubHardware)
        {
            Console.WriteLine("\tSubhardware: {0}", subhardware.Name);
            
            foreach (ISensor sensor in subhardware.Sensors)
            {
                Console.WriteLine("\t\tSensor: {0}, value: {1}", sensor.Name, sensor.Value);
            }
        }

        foreach (ISensor sensor in hardware.Sensors)
        {
            Console.WriteLine("\tSensor: {0}, value: {1}", sensor.Name, sensor.Value);
        }
    }
    
    computer.Close();
}
```

## Administrator Rights

Hardware sensor access requires administrator privileges. Either:

1. Run your IDE/application as Administrator, or
2. Add an [app.manifest](https://learn.microsoft.com/en-us/windows/win32/sbscs/application-manifests) with `requestedExecutionLevel` set to `requireAdministrator`

## Project Structure

```
xZenithHardwareMonitorLib/
├── xZenithHardwareMonitor.sln          # Solution file
├── xZenithHardwareMonitorLib/          # Core library
│   └── Hardware/                       # Hardware implementations
│       ├── Battery/                    # Battery sensors
│       ├── Controller/                 # Fan/LED controllers
│       ├── Cpu/                        # CPU sensors
│       ├── Gpu/                        # GPU sensors
│       ├── Memory/                     # RAM sensors
│       ├── Motherboard/                # Motherboard sensors
│       ├── Network/                    # Network sensors
│       ├── Psu/                        # PSU sensors
│       └── Storage/                    # Storage sensors
├── xZenithHardwareMonitor/             # GUI application
├── Aga.Controls/                       # UI controls
├── WinRing0/                           # Kernel driver
├── InpOut/                             # I/O driver
└── Licenses/                           # Third-party licenses
```

## Integration with xZenithHardwareMonitorAPI

This library serves as the foundation for [xZenithHardwareMonitorAPI](../xZenithHardwareMonitorAPI/), which wraps this library with a C-compatible interface for use with Tauri, Rust, C++, and other languages.

## Acknowledgments

Based on [LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor), which is a fork of Open Hardware Monitor.

## License

This library is licensed under the Mozilla Public License 2.0 (MPL-2.0). You can use it in private and commercial projects, but you must include a copy of the license and make source code available for any modifications to the covered files.

See the [LICENSE](LICENSE) file for the full license text.
