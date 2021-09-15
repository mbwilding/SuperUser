using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

// ReSharper disable once StringLiteralTypo

namespace SuperUser
{
    internal class Program
    {
        private static void Main()
        {
            string host = Environment.MachineName.ToLower() + @"$";
            if (Environment.UserName.ToLower() == "system" || Environment.UserName.ToLower() == host) return;
            Process process = Process.GetCurrentProcess();
            if (process.MainModule != null)
            {
                string currentPath = process.MainModule.FileName;
                if (!Admin.IsAdministrator())
                {
                    Task.Run(() => Admin.Run(currentPath)).Wait();
                    Environment.Exit(0);
                }
                AdminSuper.RunWithTokenOf(
                    "winlogon.exe",
                    true,
                    "cmd.exe",
                    string.Empty,
                    RuntimeEnvironment.GetRuntimeDirectory());
            }
            Environment.Exit(0);
        }
    }
}
