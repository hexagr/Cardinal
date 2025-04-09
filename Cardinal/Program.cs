using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class Program
{
    [DllImport("ntdll.dll")]
    public static extern uint NtQuerySystemInformation(uint SystemInformationClass, IntPtr SystemInformation, uint SystemInformationLength, out uint ReturnLength);

    [StructLayout(LayoutKind.Sequential)]
    public struct UNICODE_STRING
    {
        public ushort Length;
        public ushort MaximumLength;
        public IntPtr Buffer;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SYSTEM_PROCESS_INFORMATION
    {
        public uint NextEntryOffset;
        public uint NumberOfThreads;
        public LARGE_INTEGER WorkingSetPrivateSize;
        public uint HardFaultCount;
        public uint NumberOfThreadsHighWatermark;
        public ulong CycleTime;
        public LARGE_INTEGER CreateTime;
        public LARGE_INTEGER UserTime;
        public LARGE_INTEGER KernelTime;
        public UNICODE_STRING ImageName;
        public int BasePriority;
        public IntPtr UniqueProcessId;
        public IntPtr InheritedFromUniqueProcessId;
        public uint HandleCount;
        public uint SessionId;
        public IntPtr UniqueProcessKey;
        public IntPtr PeakVirtualSize;
        public IntPtr VirtualSize;
        public uint PageFaultCount;
        public IntPtr PeakWorkingSetSize;
        public IntPtr WorkingSetSize;
        public IntPtr QuotaPeakPagedPoolUsage;
        public IntPtr QuotaPagedPoolUsage;
        public IntPtr QuotaPeakNonPagedPoolUsage;
        public IntPtr QuotaNonPagedPoolUsage;
        public IntPtr PagefileUsage;
        public IntPtr PeakPagefileUsage;
        public IntPtr PrivatePageCount;
        public LARGE_INTEGER ReadOperationCount;
        public LARGE_INTEGER WriteOperationCount;
        public LARGE_INTEGER OtherOperationCount;
        public LARGE_INTEGER ReadTransferCount;
        public LARGE_INTEGER WriteTransferCount;
        public LARGE_INTEGER OtherTransferCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LARGE_INTEGER
    {
        public long QuadPart;
    }

    public static void Main()
    {
        uint dwRet;
        uint dwSize = 0x0;
        uint dwStatus = 0xC0000004;
        IntPtr p = IntPtr.Zero;

        while (true)
        {
            if (p != IntPtr.Zero) Marshal.FreeHGlobal(p);

            p = Marshal.AllocHGlobal((int)dwSize);
            dwStatus = NtQuerySystemInformation(5, p, dwSize, out dwRet);

            if (dwStatus == 0) { break; }
            else if (dwStatus != 0xC0000004)
            {
                Marshal.FreeHGlobal(p);
                p = IntPtr.Zero;
                Console.WriteLine("Data retrieval failed");
                return;
            }

            dwSize = dwRet + (2 << 12);
        }

        IntPtr currentPtr = p;
        do
        {
            var processInfo = (SYSTEM_PROCESS_INFORMATION)Marshal.PtrToStructure(currentPtr, typeof(SYSTEM_PROCESS_INFORMATION));

            Console.WriteLine($"[*] Image name: {(processInfo.ImageName.Buffer != IntPtr.Zero ? Marshal.PtrToStringUni(processInfo.ImageName.Buffer) : "")}");
            Console.WriteLine($"    > PID: {processInfo.UniqueProcessId.ToInt64()}");
            Console.WriteLine();

            // Calculate the offset to the next process entry
            int offset = (int)processInfo.NextEntryOffset;
            if (offset == 0)
                break;

            // Move to the next process entry
            currentPtr = IntPtr.Add(currentPtr, offset);
        } while (true);

        Marshal.FreeHGlobal(p);
    }
}
