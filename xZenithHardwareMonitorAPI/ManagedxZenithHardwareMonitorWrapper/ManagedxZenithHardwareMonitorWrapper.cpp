#include "pch.h"
#include "ManagedxZenithHardwareMonitorWrapper.h"

using namespace ManagedxZenithHardwareMonitorWrapper;

// function to create a new hardware monitor instance
void* CreateHardwareMonitor()
{
    // create a new instance of HardwareMonitorWrapper in managed memory
    HardwareMonitorWrapper^ instance = gcnew HardwareMonitorWrapper();

    // return a pointer to the instance wrapped in a gcroot
    return new gcroot<HardwareMonitorWrapper^>(instance);
}

// function to update the hardware monitor instance
void UpdateHardwareMonitor(void* handle)
{
    // cast the handle back to the original gcroot
    gcroot<HardwareMonitorWrapper^>* wrapperHandle = static_cast<gcroot<HardwareMonitorWrapper^>*>(handle);

    // call the Update method on the instance
    (*wrapperHandle)->Update();
}

// function to get a report from the hardware monitor instance
void GetReport(void* handle, char* buffer, int bufferSize)
{
    // cast the handle back to the original gcroot
    gcroot<HardwareMonitorWrapper^>* wrapperHandle = static_cast<gcroot<HardwareMonitorWrapper^>*>(handle);

    // call the GetReport method on the instance
    (*wrapperHandle)->GetReport(buffer, bufferSize);
}

// function to get the required buffer size for the report
int GetReportSize(void* handle)
{
    // cast the handle back to the original gcroot
    gcroot<HardwareMonitorWrapper^>* wrapperHandle = static_cast<gcroot<HardwareMonitorWrapper^>*>(handle);

    // call the GetReportSize method on the instance
    return (*wrapperHandle)->GetReportSize();
}

// function to destroy the hardware monitor instance and clean up memory
void DestroyHardwareMonitor(void* handle)
{
    // cast the handle back to the original gcroot
    gcroot<HardwareMonitorWrapper^>* wrapperHandle = static_cast<gcroot<HardwareMonitorWrapper^>*>(handle);

    // delete the gcroot, releasing the managed memory
    delete wrapperHandle;
}

// WMI Event Functions

// function to start the WMI event listener
bool StartWmiEventListener(void* handle)
{
    gcroot<HardwareMonitorWrapper^>* wrapperHandle = static_cast<gcroot<HardwareMonitorWrapper^>*>(handle);
    return (*wrapperHandle)->StartWmiEventListener();
}

// function to stop the WMI event listener
void StopWmiEventListener(void* handle)
{
    gcroot<HardwareMonitorWrapper^>* wrapperHandle = static_cast<gcroot<HardwareMonitorWrapper^>*>(handle);
    (*wrapperHandle)->StopWmiEventListener();
}

// function to poll for WMI events (non-blocking)
bool PollWmiEvent(void* handle, char* buffer, int bufferSize)
{
    gcroot<HardwareMonitorWrapper^>* wrapperHandle = static_cast<gcroot<HardwareMonitorWrapper^>*>(handle);
    return (*wrapperHandle)->PollWmiEvent(buffer, bufferSize);
}

// Key Status Functions

// function to start the keyboard status monitor
void StartKeyMonitor(void* handle)
{
    gcroot<HardwareMonitorWrapper^>* wrapperHandle = static_cast<gcroot<HardwareMonitorWrapper^>*>(handle);
    (*wrapperHandle)->StartKeyMonitor();
}

// function to stop the keyboard status monitor
void StopKeyMonitor(void* handle)
{
    gcroot<HardwareMonitorWrapper^>* wrapperHandle = static_cast<gcroot<HardwareMonitorWrapper^>*>(handle);
    (*wrapperHandle)->StopKeyMonitor();
}

// function to get the current keyboard status (CapsLock, NumLock)
void GetKeyStatus(void* handle, char* buffer, int bufferSize)
{
    gcroot<HardwareMonitorWrapper^>* wrapperHandle = static_cast<gcroot<HardwareMonitorWrapper^>*>(handle);
    (*wrapperHandle)->GetKeyStatus(buffer, bufferSize);
}