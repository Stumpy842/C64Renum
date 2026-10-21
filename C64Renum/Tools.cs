/*
C64Renum C64 Basic program renumber/refactor utility
© 2026 Steven J. Stover

This program is free software: you can redistribute it and/or modify it under the terms of the
GNU Affero General Public License as published by the Free Software Foundation, either version 3 
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even
the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General
Public License for more details.

You should have received a copy of the GNU Affero General Public License along with this program.
If not, see <https://www.gnu.org/licenses/>.
*/
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace C64Renum
{
    internal static class Tools
    {
        // Delay time for TimedMessage()
        private static readonly int WaitTime = 5;

        internal static async void WaitSomeTime(int delayTime, Action action)
        {
            await Task.Delay(delayTime);
            action?.Invoke();
        }

        internal static string GetRelativePath(string path)
        {
            return path.Replace(AppDomain.CurrentDomain.BaseDirectory, String.Empty);
        }

        internal static string GetPathWithAppDomain(string path)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
        }

        // OVERLOAD 1: Standard Timed Message (No Help Button / No Form needed)
        public static async void TimedMessage(string message, string title, Form owner = null!,
            MessageBoxIcon mi = MessageBoxIcon.Error, int time = -1)
        {
            if (time < 0) time = WaitTime;

            using var invOwner = new Form { Visible = false };
            var handle = invOwner.Handle; // Force creation of window handle

            invOwner.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate
            {
                // Notice we do NOT pass a dummy URL string here. No Help button appears.
                MessageBox.Show(invOwner, text: message, title, MessageBoxButtons.OK, mi);
            });

            await Task.Delay(TimeSpan.FromSeconds(time));

            if (!invOwner.IsDisposed)
            {
                invOwner.Dispose();
            }
        }

        // OVERLOAD 2: Help-Enabled Timed Message (Requires a Help Form Type in < >)
        public static async void TimedMessage<THelpForm>(string message, string title, Form owner = null!,
            MessageBoxIcon mi = MessageBoxIcon.Error, int time = -1)
            where THelpForm : Form, new()
        {
            if (time < 0) time = WaitTime;

            Form fallbackOwner = owner
                ?? (Application.OpenForms.Count > 0 ? Application.OpenForms[0]! : new Form());
            var invOwner = new TimeoutHelpForm<THelpForm>(fallbackOwner) { Visible = false };
            var handle = invOwner.Handle;

            invOwner.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate
            {
                // Passing the dummy URL here activates the Help architecture safely
                MessageBox.Show(invOwner, message, title, MessageBoxButtons.OK, mi, MessageBoxDefaultButton.Button1, 0, "http://localhost");
            });

            await Task.Delay(TimeSpan.FromSeconds(time));

            if (!invOwner.IsDisposed)
            {
                invOwner.Dispose();
            }
        }

        private class TimeoutHelpForm<T>(Form actualOwner) : Form where T : Form, new()
        {
            private readonly Form _actualOwner = actualOwner;

            protected override void WndProc(ref Message m)
            {
                // Intercept the help click right on this dummy form instance!
                // We pass '_actualOwner' so your custom help dialog centers on your real visible form, not this invisible one.
                if (HelpTools.HandleHelpMessage<T>(ref m, _actualOwner))
                {
                    return;
                }
                base.WndProc(ref m);
            }
        }

        internal static TaskDialogButton ShowTaskDlg(Form owner, string caption, string heading, string footnote = "",
            string expandertext = "", TaskDialogButtonCollection buttons = null!, int defaultbutton = -1, TaskDialogIcon icon = null!)
        {
            TaskDialogPage TaskError = new ()
            {
                Caption = caption,
                Heading = heading,
                AllowCancel = true,
                AllowMinimize = false,
                SizeToContent = true
            };
            if (footnote != "") TaskError.Footnote = new () { Text = footnote };
            if (expandertext != "") TaskError.Expander = new() { Text = expandertext, Position = TaskDialogExpanderPosition.AfterFootnote };
            if (buttons is not null)
            {
                TaskError.Buttons = buttons;
                if ((defaultbutton > -1) && (defaultbutton < buttons.Count))
                    TaskError.DefaultButton = buttons[defaultbutton];
            }
            if (icon is not null) TaskError.Icon = icon;

            if (owner is null) return TaskDialog.ShowDialog(TaskError);
                else return TaskDialog.ShowDialog(owner, TaskError, TaskDialogStartupLocation.CenterOwner); 
        }


        // Color thingie
        // Steve - 02/06/2025 21:28:41
        internal static Color GetContrastColor(Color color)
        {
            return (color.R * 0.299M) + (color.G * 0.587M) + (color.B * 0.114M) > 149 ?
                SystemColors.WindowText :
                SystemColors.Highlight;
        }

        // Helper method to accurately convert HSL values to a WinForms System.Drawing.Color
        internal static Color ColorFromHsl(double hue, double saturation, double lightness)
        {
            double c = (1.0 - Math.Abs(2.0 * lightness - 1.0)) * saturation;
            double x = c * (1.0 - Math.Abs((hue / 60.0) % 2.0 - 1.0));
            double m = lightness - c / 2.0;

            double r = 0, g = 0, b = 0;

            if (hue >= 0 && hue < 60) { r = c; g = x; b = 0; }
            else if (hue >= 60 && hue < 120) { r = x; g = c; b = 0; }
            else if (hue >= 120 && hue < 180) { r = 0; g = c; b = x; }
            else if (hue >= 180 && hue < 240) { r = 0; g = x; b = c; }
            else if (hue >= 240 && hue < 300) { r = x; g = 0; b = c; }
            else if (hue >= 300 && hue < 360) { r = c; g = 0; b = x; }

            int red = (int)Math.Round((r + m) * 255.0);
            int green = (int)Math.Round((g + m) * 255.0);
            int blue = (int)Math.Round((b + m) * 255.0);

            // Keep RGB bounds clamped between 0 and 255
            return Color.FromArgb(
                Math.Max(0, Math.Min(255, red)),
                Math.Max(0, Math.Min(255, green)),
                Math.Max(0, Math.Min(255, blue))
            );
        }

        internal static bool GetTextFromResourceFile(string TextFilename, out string Text, bool ParagraphBreak = true)
        {
            string errText = $"Failed to read {TextFilename}";
            bool ret = false;
            var assem = Assembly.GetExecutingAssembly();
            string myNamespace = assem.GetName().Name!;
            try
            {
                using Stream inst = assem.GetManifestResourceStream($"{myNamespace}.{TextFilename}")!;
                if (inst is not null)
                {
                    using StreamReader streamReader = new(inst);
                    string line;
                    StringBuilder sb = new();
                    while ((line = streamReader.ReadLine()!) is not null)
                        sb.Append(line == "" && ParagraphBreak ? "\n\n" : line);
                    Text = sb.ToString();
                    ret = true;
                }
                else
                {
                    Text = errText;
                    ret = false;
                }
            }
            catch (Exception ex)
            {
                Text = $"{errText}\n{ex}";
                ret = false;
            }
            return ret;
        }
    }

    public static class HelpTools
    {
        private const int WM_HELP = 0x0053;
        private const uint WM_COMMAND = 0x0111;
        private const uint WM_CLOSE = 0x0010;

        // Standard Win32 Control Identifiers
        private static readonly IntPtr IDOK = (IntPtr)1;     // OK Button
        private static readonly IntPtr IDCANCEL = (IntPtr)2; // Cancel Button
        private static readonly IntPtr IDNO = (IntPtr)7;     // No Button

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct HELPINFO
        {
            public uint cbSize;
            public int iContextType;
            public int iCtrlId;
            public IntPtr hItemHandle;
            public UIntPtr dwContextId;
            public System.Drawing.Point MousePos;
        }

        public static bool HandleHelpMessage<T>(ref Message m, Form owner) where T : Form, new()
        {
            if (m.Msg == WM_HELP)
            {
                HELPINFO helpInfo = Marshal.PtrToStructure<HELPINFO>(m.LParam);
                IntPtr msgBoxHandle = helpInfo.hItemHandle;

                if (msgBoxHandle != IntPtr.Zero)
                {
                    // 1. Try a standard Close message first (Works for OKCancel, RetryCancel, YesNoCancel)
                    PostMessage(msgBoxHandle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);

                    // 2. Queue up button-click fallbacks. Windows will execute the one that 
                    // matches a button on the screen and ignore the rest safely.
                    PostMessage(msgBoxHandle, WM_COMMAND, IDNO, IntPtr.Zero);     // Closes Yes/No
                    PostMessage(msgBoxHandle, WM_COMMAND, IDCANCEL, IntPtr.Zero); // Closes Cancel types
                    PostMessage(msgBoxHandle, WM_COMMAND, IDOK, IntPtr.Zero);     // Closes OK layouts
                }
                // Open your custom project help form smoothly
                using (T helpForm = new())
                {
                    helpForm.ShowDialog(owner);
                }
                m.Result = IntPtr.Zero;
                return true;
            }
            return false;
        }
    }
}

