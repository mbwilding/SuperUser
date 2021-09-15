using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace SuperUser
{
    internal static class Elevate
    {
        public static void Run(string program)
        {
            string host = Environment.MachineName.ToLower() + @"$";
            if (Environment.UserName.ToLower() == "system" || Environment.UserName.ToLower() == host) return;
            Process process = Process.GetCurrentProcess();
            if (process.MainModule != null)
            {
                string currentPath = process.MainModule.FileName;
                if (!Admin.IsAdministrator())
                {
                    Task.Run(() => Admin.Run(currentPath, program)).Wait();
                    Environment.Exit(0);
                }
                AdminSuper.RunWithTokenOf(
                    "winlogon.exe",
                    true,
                    program,
                    string.Empty,
                    Path.GetPathRoot(Environment.SystemDirectory)
                    );
            }
            Environment.Exit(0);
        }
    }
}
