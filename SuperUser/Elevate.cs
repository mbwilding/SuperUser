using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

// ReSharper disable StringLiteralTypo

namespace SuperUser
{
    internal static class Elevate
    {
        public static void Run(string[] args)
        {
            if (AdminSuper.IsSystem()) return;
            string recall = Path.Combine(Path.GetTempPath(), "SuperUser.tmp");
            string program = @"cmd.exe";
            if (args.Length == 1)
            {
                program = args[0];
                File.WriteAllText(recall, program);
            }
            Process process = Process.GetCurrentProcess();
            if (process.MainModule != null)
            {
                string currentPath = process.MainModule.FileName;
                if (!Admin.IsAdministrator())
                {
                    Task.Run(() => Admin.Run(currentPath, program)).Wait();
                    Environment.Exit(0);
                }
                if (File.Exists(recall))
                {
                    program = File.ReadAllText(recall);
                    File.Delete(recall);
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
