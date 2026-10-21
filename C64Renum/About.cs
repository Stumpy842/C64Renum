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
*/using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace C64Renum
{
    public partial class About : Form
    {
        private const string pageLink = @"https://github.com/Stumpy842/C64Renum";
        private readonly string nl = Environment.NewLine;
        private readonly string appName;

        public About()
        {
            InitializeComponent();
            appName = Form1.AppName;
            Text = $"About {appName}";
            lbTitle.Text = $"{appName} v{Form1.AppVersion}";
            lbInfo.Text = $"Comodore C64 Basic (prg) renumber/refactor utility{nl}© 2026 Steven J. Stover";
            lbLicense.Text = $"This program is free software. You may redistribute it and/or modify{nl}" +
                $"it under the terms of the GNU General Public License as published{nl}" +
                $"by the Free Software Foundation, either version 3 of the License,{nl}" +
                "or (at your option) any later version.";
            lbDisclaimer.Text = $"{appName} and its author are not affiliated{nl}with Commodore International Corporation.";
            lbPageLink.Text = pageLink;
        }

        private void About_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                this.Close();
            }
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void About_Load(object sender, EventArgs e)
        {
            int pWidth = panel1.Width;
            lbTitle.Left = (pWidth - lbTitle.Size.Width) / 2;
            lbInfo.Left = (pWidth - lbInfo.Size.Width) / 2;
            lbLicense.Left = (pWidth - lbLicense.Size.Width) / 2;
            lbDisclaimer.Left = (pWidth - lbDisclaimer.Size.Width) / 2;
            lbPageLink.Left = (pWidth - lbPageLink.Size.Width) / 2;
            btOK.Left = (pWidth - btOK.Size.Width) / 2;
        }

        private void lbPageLink_Click(object sender, EventArgs e)
        {
            try
            {
                using Process? p = Process.Start(new ProcessStartInfo(pageLink) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Tools.TimedMessage($@"Cannot open page {pageLink}{nl}{ex}", appName);
            }
        }
    }
}
