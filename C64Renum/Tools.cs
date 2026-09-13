/*
C64Renum C64 Basic program renumbering utility
© Steven J. Stover, 2026

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

        // Provides a MessageBox display which times out after a delay
        public static async void TimedMessage(string message, string title, Form owner = null!, MessageBoxIcon mi = MessageBoxIcon.Error, int time = -1)
        {
            if (time < 0) time = WaitTime;
            // A new form as owner for the MessageBox
            using var invOwner = new Form { Visible = false };
            // Force the creation of the window handle
            // Otherwise the BeginInvoke will not work
            var handle = invOwner.Handle;
            invOwner.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate
            {
                if (owner is not null) using (new CenterWinDialog(owner))
                    { MessageBox.Show(invOwner, text: message, title, MessageBoxButtons.OK, mi); }
                else { MessageBox.Show(invOwner, text: message, title, MessageBoxButtons.OK, mi); }
            });
            await Task.Delay(TimeSpan.FromSeconds(time));
            // When the form is disposed the MessageBox is too!
            invOwner.Dispose();
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
                SystemColors.HighlightText;
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
}

