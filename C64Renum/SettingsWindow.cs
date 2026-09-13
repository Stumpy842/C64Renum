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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace C64Renum
{
    public partial class SettingsWindow : Form
    {
        private static readonly string title = Form1.AppName + " Settings";
        public SettingsWindow(bool Show = true)
        {
            InitializeComponent();
            if (Show)
            {
                Text = title;
                this.Icon = Properties.Resources.C64Renum;
                Settings.Init(this);
                Settings.Load();
            }
        }

        private static bool GetFolder(string desc, ref string path, bool inp = true)
        {
            path = inp ? Settings.CurrentSettings.InputFilePath : Settings.CurrentSettings.OutputFilePath;
            if (!Directory.Exists(path)) { path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); }
            using FolderBrowserDialog fbd = new()
            {
                Description = desc,
                InitialDirectory = path
            };
            DialogResult result = fbd.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            { path = fbd.SelectedPath; return true; }
            return false;
        }

        private void btInputFolder_Click(object sender, EventArgs e)
        {
            string retpath = "";
            if (GetFolder("Select Input Folder", ref retpath))
            { tbInputFolder.Text = retpath; }
        }

        private void btOutputFolder_Click(object sender, EventArgs e)
        {
            string retpath = "";
            if (GetFolder("Select Output Folder", ref retpath, false))
            { tbOutputFolder.Text = retpath; }
        }

        private void SaveIt()
        {
            Settings.Save();
            DialogResult = DialogResult.OK;
            Close();
        }
        private void btSave_Click(object sender, EventArgs e)
        {
            SaveIt();
        }

        private void SettingsWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                SaveIt();
            }
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void cbOutUseInputFolder_CheckedChanged(object sender, EventArgs e)
        {
            bool unChecked = !cbOutUseInputFolder.Checked;
            tbOutputFolder.Enabled = unChecked;
            btOutputFolder.Enabled = unChecked;
        }

        private void FColor()
        {
            Color color = lbForeground.BackColor;
            if (NewColor(ref color))
            {
                lbForeground.BackColor = color;
                Settings.CurrentSettings.Foreground = color;
            }
        }

        private void lbForeground_Click(object sender, EventArgs e)
        {
            FColor();
        }

        private void BColor()
        {
            Color color = lbBackground.BackColor;
            if (NewColor(ref color))
            {
                lbBackground.BackColor = color;
                Settings.CurrentSettings.Background = color;
            }
        }

        private void lbBackground_Click(object sender, EventArgs e)
        {
            BColor();
        }
        private static bool NewColor(ref Color color)
        {
            ColorDialog colorDialog = new()
            {
                Color = color,
                FullOpen = true
            };
            DialogResult result = colorDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                color = colorDialog.Color;
                return true;
            }
            return false;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Alt | Keys.F))
            {
                FColor(); return true;
            }
            if (keyData == (Keys.Alt | Keys.B))
            {
                BColor(); return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void cbDemoMode_CheckedChanged(object sender, EventArgs e)
        {
            tbrSpeed.Enabled = cbDemoMode.Checked;
        }
    }
}

