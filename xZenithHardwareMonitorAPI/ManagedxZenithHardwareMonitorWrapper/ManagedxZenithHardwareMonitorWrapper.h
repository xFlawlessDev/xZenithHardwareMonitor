// ManagedxZenithHardwareMonitorWrapper.h

#pragma once

// Native includes must come before managed namespaces
#include <vcclr.h>
#include <cstring>

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;
using namespace System::Threading;

// managed wrapper for the xZenith hardware monitor
namespace ManagedxZenithHardwareMonitorWrapper
{
    public ref class HardwareMonitorWrapper
    {
    private:
        // declare an instance of the managed hardware monitor
        ManagedxZenithHardwareMonitor::HardwareMonitor^ _hardwareMonitor;
        
        // queue for WMI events (for polling)
        Queue<String^>^ _wmiEventQueue;
        Object^ _wmiQueueLock;

        // WMI event handler
        void OnWmiEventReceived(Object^ sender, ManagedxZenithHardwareMonitor::WmiEventArgs^ e)
        {
            String^ json = String::Format(
                "{{\"type\":\"{0}\",\"data\":{1},\"message\":\"{2}\",\"details\":\"{3}\"}}",
                e->Type,
                e->Data != nullptr ? "[" + String::Join(",", Array::ConvertAll(e->Data, gcnew Converter<int, String^>(Convert::ToString))) + "]" : "null",
                e->Message != nullptr ? e->Message->Replace("\"", "\\\"") : "",
                e->Details != nullptr ? e->Details->Replace("\"", "\\\"") : ""
            );
            
            Monitor::Enter(_wmiQueueLock);
            try {
                _wmiEventQueue->Enqueue(json);
            }
            finally {
                Monitor::Exit(_wmiQueueLock);
            }
        }

    public:
        // constructor initializes the hardware monitor
        HardwareMonitorWrapper()
        {
            _hardwareMonitor = gcnew ManagedxZenithHardwareMonitor::HardwareMonitor();
            _wmiEventQueue = gcnew Queue<String^>();
            _wmiQueueLock = gcnew Object();
            
            // subscribe to WMI events
            _hardwareMonitor->WmiEventReceived += gcnew EventHandler<ManagedxZenithHardwareMonitor::WmiEventArgs^>(
                this, &HardwareMonitorWrapper::OnWmiEventReceived);
        }

        // method to update the hardware monitor
        void Update()
        {
            _hardwareMonitor->Update();
        }

        // helper method to copy managed string to native buffer
        void CopyStringToBuffer(String^ str, char* buffer, int bufferSize)
        {
            if (bufferSize <= 0 || str == nullptr) {
                if (bufferSize > 0) buffer[0] = '\0';
                return;
            }
            
            IntPtr ptr = Marshal::StringToHGlobalAnsi(str);
            try {
                char* src = static_cast<char*>(ptr.ToPointer());
                int len = static_cast<int>(strlen(src));
                int copyLen = (len < bufferSize - 1) ? len : bufferSize - 1;
                memcpy(buffer, src, copyLen);
                buffer[copyLen] = '\0';
            }
            finally {
                Marshal::FreeHGlobal(ptr);
            }
        }

        // method to get a report from the hardware monitor and store it in a buffer
        void GetReport(char* buffer, int bufferSize)
        {
            String^ report = _hardwareMonitor->GetReport();
            CopyStringToBuffer(report, buffer, bufferSize);
        }

        // method to get the required buffer size for the report
        int GetReportSize()
        {
            return _hardwareMonitor->GetReportSize();
        }

        // WMI Event Methods
        bool StartWmiEventListener()
        {
            return _hardwareMonitor->StartWmiEventListener();
        }

        void StopWmiEventListener()
        {
            _hardwareMonitor->StopWmiEventListener();
        }

        bool PollWmiEvent(char* buffer, int bufferSize)
        {
            if (bufferSize <= 0) return false;
            
            String^ eventJson = nullptr;
            
            Monitor::Enter(_wmiQueueLock);
            try {
                if (_wmiEventQueue->Count > 0) {
                    eventJson = _wmiEventQueue->Dequeue();
                }
            }
            finally {
                Monitor::Exit(_wmiQueueLock);
            }
            
            if (eventJson == nullptr) {
                buffer[0] = '\0';
                return false;
            }
            
            CopyStringToBuffer(eventJson, buffer, bufferSize);
            return true;
        }

        // Key Status Methods
        void StartKeyMonitor()
        {
            _hardwareMonitor->StartKeyMonitor();
        }

        void StopKeyMonitor()
        {
            _hardwareMonitor->StopKeyMonitor();
        }

        void GetKeyStatus(char* buffer, int bufferSize)
        {
            auto status = _hardwareMonitor->GetKeyStatus();
            String^ json = String::Format(
                "{{\"caps_lock\":{0},\"num_lock\":{1},\"timestamp\":\"{2}\"}}",
                status->CapsLock ? "true" : "false",
                status->NumLock ? "true" : "false",
                status->Timestamp.ToString("o")
            );
            CopyStringToBuffer(json, buffer, bufferSize);
        }
    };
}

// declare C-style functions for interacting with the wrapper
extern "C" __declspec(dllexport) void* CreateHardwareMonitor();
extern "C" __declspec(dllexport) void UpdateHardwareMonitor(void* instance);
extern "C" __declspec(dllexport) void GetReport(void* instance, char* buffer, int bufferSize);
extern "C" __declspec(dllexport) int GetReportSize(void* instance);
extern "C" __declspec(dllexport) void DestroyHardwareMonitor(void* instance);

// WMI Event functions
extern "C" __declspec(dllexport) bool StartWmiEventListener(void* instance);
extern "C" __declspec(dllexport) void StopWmiEventListener(void* instance);
extern "C" __declspec(dllexport) bool PollWmiEvent(void* instance, char* buffer, int bufferSize);

// Key Status functions
extern "C" __declspec(dllexport) void StartKeyMonitor(void* instance);
extern "C" __declspec(dllexport) void StopKeyMonitor(void* instance);
extern "C" __declspec(dllexport) void GetKeyStatus(void* instance, char* buffer, int bufferSize);