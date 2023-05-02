using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Windows;
using System.Windows.Controls;
using CoreController.Classes;
using NLog;

namespace CoreController
{
    public static class CoreManager
    {
        static Logger log = LogManager.GetCurrentClassLogger();
        public static void GetLogicalCores()
        {
            GetProcessorInformation();

            CoreControllerMain.Instance.Save();
            ResetAffinity();
        }

        public static void ResetAffinity()
        {
            // Get the current process
            Process currentProcess = Process.GetCurrentProcess();
            long bitmask = 0;
            
            if (CoreControllerMain.Instance.Config.AllowedProcessors.Count == 0)
            {
                foreach (var core in CoreControllerMain.LogicalCores)
                {
                    CoreControllerMain.Instance.Config.AllowedProcessors.Add(core.ConvertToUnRaw());
                }
                return;
            }
            
            foreach (LogicalProcessors core in CoreControllerMain.Instance.Config.AllowedProcessors)
            {
                bitmask |= (1L << core.ID);
            }

            currentProcess.ProcessorAffinity = (IntPtr) bitmask;
        }

        public static void GetProcessorInformation()
        {
            List<LogicalProcessorRaw> logicalProcessorInfoList = new List<LogicalProcessorRaw>();

            ManagementScope scope = new ManagementScope("\\\\.\\root\\cimv2");
            ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_Processor");

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query))
            {
                ManagementObjectCollection results = searcher.Get();
                
                int logicalProcessorCount = 0;
                foreach (ManagementObject obj in results)
                {
                    uint numberOfLogicalProcessors = (uint)obj["NumberOfLogicalProcessors"];
                    string deviceId =  obj["DeviceID"].ToString().Replace("CPU", "");

                    for (int i = 0; i < numberOfLogicalProcessors; i++)
                    {
                        LogicalProcessorRaw logicalProcessorInfo = new LogicalProcessorRaw
                        {
                            ID = logicalProcessorCount,
                            PhysicalProcessorID = Convert.ToInt32(deviceId),
                            PID = (Convert.ToInt32(deviceId) + 1) * 1000 + (i+1),
                        };
                        logicalProcessorCount++;
                        logicalProcessorInfoList.Add(logicalProcessorInfo);
                    }
                }

                foreach (LogicalProcessorRaw raw in logicalProcessorInfoList)
                {
                    CoreControllerMain.LogicalCores.Add(raw);
                }
            }
        }
        
        public static void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button?.Tag == null) return;
            int PID = (int) button.Tag;
            LogicalProcessorRaw processor = null;

            for (int index = CoreControllerMain.LogicalCores.Count - 1; index >= 0; index--)
            {
                if (CoreControllerMain.LogicalCores[index].PID != PID) continue;
                processor = CoreControllerMain.LogicalCores[index];
                break;
            }

            if (processor == null) return;

            bool found = false;
            for (int index = CoreControllerMain.Instance.Config.AllowedProcessors.Count - 1; index >= 0; index--)
            {
                if (CoreControllerMain.Instance.Config.AllowedProcessors.Count == 1) continue;
                if (CoreControllerMain.Instance.Config.AllowedProcessors[index].PID != PID) continue;
                CoreControllerMain.Instance.Config.AllowedProcessors.RemoveAt(index);
                found = true;
                break;
            }

            if (!found)
            {
                CoreControllerMain.Instance.Config.AllowedProcessors.Add(processor.ConvertToUnRaw());
            }
            CoreControllerMain.Instance.Save();
            ResetAffinity();
        }
    }
}