using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using CoreController.Classes;
using NLog.Fluent;

namespace CoreController
{
    public static class NumaManager
    {
        [DllImport("kernel32.dll")]
        public static extern bool GetNumaHighestNodeNumber(out uint HighestNodeNumber);

        [DllImport("kernel32.dll")]
        public static extern bool GetNumaProcessorNodeEx(ref PROCESSOR_NUMBER Processor, out ushort Node);

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESSOR_NUMBER
        {
            public ushort Group;
            public byte Number;
            public byte Reserved;
        }
        
        public static void UpdateNumaTopology()
        {
            if (GetNumaHighestNodeNumber(out uint highestNodeNumber))
            {
                Console.WriteLine("Highest NUMA node number: " + highestNodeNumber);
                for (int i = 0; i < sizeof(ulong) * 8; ++i)
                {
                    PROCESSOR_NUMBER procNumber = new PROCESSOR_NUMBER { Group = 0, Number = (byte)i };
                    if (GetNumaProcessorNodeEx(ref procNumber, out ushort node))
                    {
                        LogicalProcessorRaw matchingProcessor = CoreControllerMain.LogicalCores.FirstOrDefault(lp => lp.ID == i - 1);
                        if (matchingProcessor != null)
                            matchingProcessor.Node = node;
                    }
                }
            }
            else
            {
                CoreControllerMain.Log.Warn("Error: " + Marshal.GetLastWin32Error());
            }
        }
        
        
    }
}