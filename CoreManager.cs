using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
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
            foreach (LogicalProcessors core in CoreControllerMain.Instance.Config.AllowedProcessors)
            {
                bitmask |= (1L << (core.ID - 1));
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

                foreach (ManagementObject obj in results)
                {
                    uint numberOfLogicalProcessors = (uint)obj["NumberOfLogicalProcessors"];
                    string deviceId =  obj["DeviceID"].ToString().Replace("CPU", "");

                    for (int i = 0; i < numberOfLogicalProcessors; i++)
                    {
                        LogicalProcessorRaw logicalProcessorInfo = new LogicalProcessorRaw
                        {
                            ID = i+1,
                            PhysicalProcessorID = Convert.ToInt32(deviceId),
                            PID = Convert.ToInt32(deviceId) + "~" + (i+1),
                        };

                        logicalProcessorInfoList.Add(logicalProcessorInfo);
                    }
                }
                
                for (int i = 0; i < logicalProcessorInfoList.Count; i++)
                {
                    logicalProcessorInfoList[i].AffinityMask = new IntPtr(1L << i);
                }

                foreach (LogicalProcessorRaw raw in logicalProcessorInfoList)
                {
                    CoreControllerMain.LogicalCores.Add(raw);
                }
            }
        }
    }
}