using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TeamsRemoteWindow.Properties;

namespace TeamsRemote
{
    static class Program
    {
        private static TraceSource Trace = new TraceSource("Trace");
        private static CancellationTokenSource tokenSource;

        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            var worker = new Task(() =>
            {
                Service myService = new Service();
                myService.Start();
                Trace.TraceInformation("Service started...");

            }, token);

            worker.Start();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MyCustomApplicationContext());
            
        }

        public class MyCustomApplicationContext : ApplicationContext
        {
            private NotifyIcon trayIcon;
            private MenuItem checkedItem = new MenuItem("Run at startup");
            public MyCustomApplicationContext()
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey
                        ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                var runAtStartup = registryKey.GetValue("TeamsRemote");
                // Initialize Tray Icon
                checkedItem.Checked = runAtStartup == null? false : true;
                checkedItem.Click += RegisterInStartup;
                trayIcon = new NotifyIcon()
                {
                    Icon = Resources.AppIcon,
                    ContextMenu = new ContextMenu(new MenuItem[] {
                        checkedItem,
                new MenuItem("Exit", Exit)
            }),
                    Visible = true
                };
                trayIcon.BalloonTipTitle = "Teams Remote";
                trayIcon.BalloonTipText = "running in Tray";
                trayIcon.BalloonTipIcon = ToolTipIcon.Info;
                trayIcon.ShowBalloonTip(1000);
                
            }

            void Exit(object sender, EventArgs e)
            {
                // Hide tray icon, otherwise it will remain shown until user mouses over it
                trayIcon.Visible = false;
                tokenSource.Cancel();
                Application.Exit();
            }

            private void RegisterInStartup(object sender, EventArgs e)
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey
                        ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                var runAtStartup = registryKey.GetValue("TeamsRemote");
                if (runAtStartup == null)
                {
                    registryKey.SetValue("TeamsRemote", Application.ExecutablePath);
                    checkedItem.Checked = true;
                }
                else
                {
                    registryKey.DeleteValue("TeamsRemote");
                    checkedItem.Checked = false;
                }
            }
        }
    }
}
