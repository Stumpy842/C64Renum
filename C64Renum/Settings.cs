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
        private static readonly string SettingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{Form1.AppName}.json");
        private static readonly string defaultInputFilePath = Form1.defaultPath;
        private static readonly string defaultOutputFilePath = Form1.defaultPath;
        private static readonly string title = Form1.AppName;
        private static SettingsWindow? _settingsWindow = null;

        public static SettingsData CurrentSettings { get; private set; } = new();

        public static void Init(SettingsWindow settingsWindow)
        {
            _settingsWindow = settingsWindow;
        }

        public static void Save(Form1 form, bool allSettings = true)
        {
            if (_settingsWindow != null && allSettings)
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
                CurrentSettings.CloseOnEscape = _settingsWindow.cbCloseOnEsc.Checked;
                CurrentSettings.ClearOnInvalid = _settingsWindow.cbClearOnInvalid.Checked;
                CurrentSettings.MaxVarLength = (int)_settingsWindow.nudMaxVarLength.Value;
                CurrentSettings.EnableWarnings = _settingsWindow.cbEnableWarnings.Checked;
                CurrentSettings.CloseRefOnEsc = _settingsWindow.cbCloseRefOnEsc.Checked;
                CurrentSettings.LineLength = _settingsWindow.cbLineLength.Checked ? LineCharLimit.Eighty : LineCharLimit.TwoFifty;
                CurrentSettings.OverwriteOnTrim = _settingsWindow.cbOverwriteOnTrim.Checked;
                CurrentSettings.ClrOutFilePath = _settingsWindow.cbClrOutFilePath.Checked;
            } 
            CurrentSettings.rMode = form.optMode;
            CurrentSettings.StartLineNo = form.optStartLineNo;
            CurrentSettings.StepIncNo = form.optStepIncNo;

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
                    _settingsWindow.cbCloseOnEsc.Checked = CurrentSettings.CloseOnEscape;
                    _settingsWindow.cbClearOnInvalid.Checked = CurrentSettings.ClearOnInvalid;
                    _settingsWindow.nudMaxVarLength.Value = CurrentSettings.MaxVarLength;
                    _settingsWindow.cbEnableWarnings.Checked = CurrentSettings.EnableWarnings;
                    _settingsWindow.cbCloseRefOnEsc.Checked = CurrentSettings.CloseRefOnEsc;
                    _settingsWindow.cbLineLength.Checked = CurrentSettings.LineLength == LineCharLimit.Eighty;
                    _settingsWindow.cbOverwriteOnTrim.Checked = CurrentSettings.OverwriteOnTrim;
                    _settingsWindow.cbClrOutFilePath.Checked = CurrentSettings.ClrOutFilePath;
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
            public Form1.Mode rMode { get; set; } = Form1.Mode.Renumber;
            public bool CloseOnEscape { get; set; } = false;
            public bool ClearOnInvalid { get; set; } = false;
            public int MaxVarLength { get; set; } = 16;
            public bool EnableWarnings { get; set; } = false;
            public bool CloseRefOnEsc { get; set; } = false;
            public bool OverwriteOnTrim { get; set; } = false;
            public bool ClrOutFilePath { get; set; } = false;
            public int StartLineNo { get; set; } = 10;
            public int StepIncNo { get; set; } = 10;
            public LineCharLimit LineLength { get; set; } = LineCharLimit.Eighty;
        }
        public enum LineCharLimit
        {
            Eighty = 80,
            TwoFifty = 250
        }
    }
}
