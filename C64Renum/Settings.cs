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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C64Renum
{
    public static class Settings
    {
        private static readonly string SettingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "C64Renum.json");
        private static readonly string defaultInputFilePath = Form1.defaultPath;
        private static readonly string defaultOutputFilePath = Form1.defaultPath;
        private static readonly string title = Form1.AppName;
        private static SettingsWindow? _settingsWindow = null;
        public static SettingsData CurrentSettings { get; private set; } = new();

        public static void Init(SettingsWindow settingsWindow)
        {
            _settingsWindow = settingsWindow;
        }

        public static void Save()
        {
            string text = _settingsWindow!.tbInputFolder.Text.Trim();
            CurrentSettings.InputFilePath = string.IsNullOrEmpty(text) ? defaultInputFilePath : text;
            text = _settingsWindow.tbOutputFolder.Text.Trim();
            CurrentSettings.OutputFilePath = string.IsNullOrEmpty(text) ? defaultOutputFilePath : text;
            CurrentSettings.NoPromptOverwrite = _settingsWindow.cbNoPromptOverwrite.Checked;
            CurrentSettings.DeleteOnCancel = _settingsWindow.cbDeleteOnCancel.Checked;
            CurrentSettings.OutUseInputFolder = _settingsWindow.cbOutUseInputFolder.Checked;
            CurrentSettings.Background = _settingsWindow.lbBackground.BackColor;
            CurrentSettings.Foreground = _settingsWindow.lbForeground.BackColor;
            CurrentSettings.WrapText = _settingsWindow.cbWrap.Checked;
            CurrentSettings.DemoMode = _settingsWindow.cbDemoMode.Checked;
            CurrentSettings.DemoSpeed = _settingsWindow.tbrSpeed.Value;

            try
            {
                if (!File.Exists(SettingsFilePath)) File.WriteAllText(Tools.GetRelativePath(SettingsFilePath), String.Empty);
                string savedText = JsonConvert.SerializeObject(CurrentSettings);
                File.WriteAllText(SettingsFilePath, savedText);
            }
            catch //(Exception ex)
            {
                MessageBox.Show("Error saving settings", $"{title} - File Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void Load()
        {
            SettingsData? newItems = null;
            try
            {
                if (!File.Exists(SettingsFilePath)) File.WriteAllText(SettingsFilePath, String.Empty);
                var fileText = File.ReadAllText(SettingsFilePath);
                newItems = JsonConvert.DeserializeObject<SettingsData>(fileText);
            }
            catch (Exception ex)
            {
                if (_settingsWindow is not null)
                {
                    Tools.ShowTaskDlg(null!, title, "Settings File Error", "There was an error loading or creating the Settings",
                        ex.Message);
                }
            }
            finally
            {
                if (newItems is not null)
                {
                    CurrentSettings = newItems;
                }
                else
                {
                    if (_settingsWindow is not null)
                    {
                        Tools.TimedMessage("Error loading settings, using defaults.", $"{title} - File Load Error");
                    }
                }

                if (_settingsWindow is not null)
                {
                    _settingsWindow.tbInputFolder.Text = CurrentSettings.InputFilePath;
                    _settingsWindow.tbOutputFolder.Text = CurrentSettings.OutputFilePath;
                    _settingsWindow.cbNoPromptOverwrite.Checked = CurrentSettings.NoPromptOverwrite;
                    _settingsWindow.cbDeleteOnCancel.Checked = CurrentSettings.DeleteOnCancel;
                    _settingsWindow.cbOutUseInputFolder.Checked = CurrentSettings.OutUseInputFolder;
                    _settingsWindow.lbBackground.BackColor = CurrentSettings.Background;
                    _settingsWindow.lbForeground.BackColor = CurrentSettings.Foreground;
                    _settingsWindow.cbWrap.Checked = CurrentSettings.WrapText;
                    _settingsWindow.cbDemoMode.Checked = CurrentSettings.DemoMode;
                    _settingsWindow.tbrSpeed.Value = CurrentSettings.DemoSpeed;
                }
            }
        }

        [Serializable]
        public class SettingsData
        {
            public string InputFilePath { get; set; } = defaultInputFilePath;
            public string OutputFilePath { get; set; } = defaultOutputFilePath;
            public bool NoPromptOverwrite { get; set; } = false;
            public bool DeleteOnCancel { get; set; } = false;
            public bool OutUseInputFolder { get; set; } = false;
            public Color Foreground { get; set; } = Color.FromArgb(128,255,255);
            public Color Background { get; set; } = Color.Blue;
            public bool WrapText { get; set; } = true;
            public bool DemoMode { get; set; } = false;
            public int DemoSpeed { get; set; } = 1;
        }
    }
}
