using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System;
using System.Diagnostics;

namespace TeamsRemote
{
    public class TeamsRemote
    {
        private static TraceSource Trace = new TraceSource("Trace");
        
        // Delegate to filter which windows to include 
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        public static List<string> windowNames = new List<string>();
        /// <summary> Get the text for the window pointed to by hWnd </summary>
        private string GetWindowText(IntPtr hWnd)
        {
            int size = GetWindowTextLength(hWnd);
            if (size > 0)
            {
                var builder = new StringBuilder(size + 1);
                GetWindowText(hWnd, builder, builder.Capacity);
                return builder.ToString();
            }

            return String.Empty;
        }

        /// <summary> Find all windows that match the given filter </summary>
        /// <param name="filter"> A delegate that returns true for windows
        ///    that should be returned and false for windows that should
        ///    not be returned </param>
        private IEnumerable<IntPtr> FindWindows(EnumWindowsProc filter)
        {
            IntPtr found = IntPtr.Zero;
            List<IntPtr> windows = new List<IntPtr>();

            EnumWindows(delegate (IntPtr wnd, IntPtr param)
            {
                if (filter(wnd, param))
                {
                    // only add the windows that pass the filter
                    windows.Add(wnd);
                }

                // but return true here so that we iterate all windows
                return true;
            }, IntPtr.Zero);

            return windows;
        }

        /// <summary> Find all windows that contain the given title text </summary>
        /// <param name="titleText"> The text that the window title must contain. </param>
        private IEnumerable<IntPtr> FindWindowsWithText(string titleText)
        {
            return FindWindows(delegate (IntPtr wnd, IntPtr param)
            {
                var name = GetWindowText(wnd);
                uint pid = 0;
                uint threadid = GetWindowThreadProcessId(wnd, out pid);
                if (name.Contains(titleText)) windowNames.Add(name);
                return GetWindowText(wnd).Contains(titleText);
            });
        }

        public void ToggleMute()
        {
            var currentWindow = GetForegroundWindow();
            var windows = FindWindowsWithText("| Microsoft Teams");

            if (windows.Count() > 0)
            {
                var teams = windows.First();
                

                if (SetForegroundWindow(teams))
                {

                    SendKeys.SendWait("^+M");
                    SendKeys.Flush();

                }
                Thread.Sleep(200);
                SetForegroundWindow(currentWindow);

            }
            else
            {
                Trace.TraceEvent(TraceEventType.Error,4,"Teams not running...");
            }
        }

        public void ToggleRaiseHand()
        {
            var currentWindow = GetForegroundWindow();
            var windows = FindWindowsWithText("| Microsoft Teams");

            if (windows.Count() > 0)
            {
                foreach (var teams in windows)
                {

                    if (SetForegroundWindow(teams))
                    {

                        SendKeys.SendWait("^+K");
                        SendKeys.Flush();

                    }
                    Thread.Sleep(200);
                    SetForegroundWindow(currentWindow);
                }
            } else
            {
                Trace.TraceEvent(TraceEventType.Error,4,"Teams not running...");
            }
        }

        public void ToggleCamera()
        {
            var currentWindow = GetForegroundWindow();
            var windows = FindWindowsWithText("| Microsoft Teams");

            if (windows.Count() > 0)
            {
                var teams = windows.First();


                if (SetForegroundWindow(teams))
                {

                    SendKeys.SendWait("^+O");
                    SendKeys.Flush();

                }
                Thread.Sleep(200);
                SetForegroundWindow(currentWindow);

            }
            else
            {
                Trace.TraceEvent(TraceEventType.Error,4,"Teams not running...");
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
    }

}
