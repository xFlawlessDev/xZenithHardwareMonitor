// ManagedxZenithHardwareMonitorWrapper.h

#pragma once

using namespace System;
using namespace System::Runtime::InteropServices;

#include <locale>
#include <codecvt>
#include <vcclr.h>

// managed wrapper for the xZenith hardware monitor
namespace ManagedxZenithHardwareMonitorWrapper
{
    public ref class HardwareMonitorWrapper
    {
    private:
        // declare an instance of the managed hardware monitor
        ManagedxZenithHardwareMonitor::HardwareMonitor^ _hardwareMonitor;

    public:
        // constructor initializes the hardware monitor
        HardwareMonitorWrapper()
        {
            _hardwareMonitor = gcnew ManagedxZenithHardwareMonitor::HardwareMonitor();
        }

        // method to update the hardware monitor
        void Update()
        {
            _hardwareMonitor->Update();
        }

        // method to get a report from the hardware monitor and store it in a buffer
        void GetReport(char* buffer, int bufferSize)
        {
            if (bufferSize <= 0) return;
            
            String^ report = _hardwareMonitor->GetReport();
            pin_ptr<const wchar_t> wstr = PtrToStringChars(report);
            
            // reserve space for null terminator
            size_t maxBytes = static_cast<size_t>(bufferSize - 1);
            size_t result = wcstombs(buffer, wstr, maxBytes);
            
            if (result == static_cast<size_t>(-1)) {
                buffer[0] = '\0';
            } else {
                // ensure null termination
                buffer[result < maxBytes ? result : maxBytes] = '\0';
            }
        }

        // method to get the required buffer size for the report
        int GetReportSize()
        {
            return _hardwareMonitor->GetReportSize();
        }
    };
}

// declare C-style functions for interacting with the wrapper
extern "C" __declspec(dllexport) void* CreateHardwareMonitor();
extern "C" __declspec(dllexport) void UpdateHardwareMonitor(void* instance);
extern "C" __declspec(dllexport) void GetReport(void* instance, char* buffer, int bufferSize);
extern "C" __declspec(dllexport) int GetReportSize(void* instance);
extern "C" __declspec(dllexport) void DestroyHardwareMonitor(void* instance);