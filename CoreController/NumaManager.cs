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
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetLogicalProcessorInformationEx(LOGICAL_PROCESSOR_RELATIONSHIP RelationshipType, IntPtr Buffer, ref uint ReturnLength);

        
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint GetActiveProcessorGroupCount();

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint GetActiveProcessorCount(ushort GroupNumber);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetNumaProcessorNodeEx(ref PROCESSOR_NUMBER Processor, out ushort NodeNumber);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetNumaHighestNodeNumber(out uint HighestNodeNumber);
        
        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX
        {
            public LOGICAL_PROCESSOR_RELATIONSHIP Relationship;
            public uint Size;
            public GROUP_AFFINITY Processor;
            public uint NumaNode;
        }
        
        [StructLayout(LayoutKind.Sequential)]
        public struct GROUP_AFFINITY
        {
            public ulong Mask;
            public ushort Group;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public ushort[] Reserved;
        }
        
        public const int ERROR_INSUFFICIENT_BUFFER = 122;

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESSOR_NUMBER
        {
            public ushort Group;
            public byte Number;
            public byte Reserved;
        }
        
        [StructLayout(LayoutKind.Sequential)]
        public struct CACHE_DESCRIPTOR
        {
            public byte Level;
            public byte Associativity;
            public ushort LineSize;
            public uint Size;
            public PROCESSOR_CACHE_TYPE Type;
        }

        public enum PROCESSOR_CACHE_TYPE
        {
            CacheUnified,
            CacheInstruction,
            CacheData,
            CacheTrace
        }
        
        public enum LOGICAL_PROCESSOR_RELATIONSHIP
        {
            RelationProcessorCore,
            RelationNumaNode,
            RelationCache,
            RelationProcessorPackage,
            RelationGroup,
            RelationAll = 0xffff
        }
        
        public static int CountSetBits(ulong value)
        {
            int count = 0;
            while (value != 0)
            {
                count += (int)(value & 1);
                value >>= 1;
            }
            return count;
        }

        public static void UpdateNumaTopology()
        {
            uint returnLength = 0;
            bool success = GetLogicalProcessorInformationEx(LOGICAL_PROCESSOR_RELATIONSHIP.RelationAll, IntPtr.Zero, ref returnLength);

            if (!success && Marshal.GetLastWin32Error() == ERROR_INSUFFICIENT_BUFFER)
            {
                IntPtr buffer = Marshal.AllocHGlobal((int)returnLength);
                success = GetLogicalProcessorInformationEx(LOGICAL_PROCESSOR_RELATIONSHIP.RelationAll, buffer, ref returnLength);

                if (success)
                {
                    int structSize = Marshal.SizeOf<SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX>();
                    int numEntries = (int)returnLength / structSize;

                    for (int i = 0; i < numEntries; i++)
                    {
                        SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX entry = Marshal.PtrToStructure<SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX>(buffer + i * structSize);

                        if (entry.Relationship == LOGICAL_PROCESSOR_RELATIONSHIP.RelationProcessorCore)
                        {
                            int processorCount = CountSetBits(entry.Processor.Mask);

                            for (int j = 0; j < processorCount; j++)
                            {
                                int logicalProcessorIndex = entry.Processor.Group * 64 + (int)Math.Log(entry.Processor.Mask >> j, 2);
                                if (logicalProcessorIndex < CoreControllerMain.LogicalCores.Count)
                                {
                                    LogicalProcessorRaw matchingProcessor = CoreControllerMain.LogicalCores[logicalProcessorIndex];

                                    PROCESSOR_NUMBER procNumber = new PROCESSOR_NUMBER { Group = entry.Processor.Group, Number = (byte)logicalProcessorIndex };
                                    if (GetNumaProcessorNodeEx(ref procNumber, out ushort node))
                                    {
                                        matchingProcessor.Node = node;
                                    }
                                }
                            }
                        }
                    }
                }
                Marshal.FreeHGlobal(buffer);
            }
            else
            {
                CoreControllerMain.Log.Warn("Error: " + Marshal.GetLastWin32Error());
            }


        }
    }
}