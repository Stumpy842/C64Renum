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
*/using System;
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
    public partial class About : Form
    {
        private readonly string nl = Environment.NewLine;

        public About()
        {
            InitializeComponent();
            Text = $"About {Form1.AppName}";
            lbTitle.Text = Form1.AppName + " v" + Form1.AppVersion;
            lbInfo.Text = $"Comodore C64 Basic (prg) renumbering utility{nl}© Steven J. Stover, 2026";
            lbWhat.Text = $"This program is free software. You may redistribute it and/or modify{nl}" +
                $"it under the terms of the GNU General Public License as published{nl}" +
                $"by the Free Software Foundation, either version 3 of the License,{nl}" +
                "or (at your option) any later version.";
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
            lbTitle.Left = (lbTitle.Parent!.Width - lbTitle.Size.Width) / 2;
            lbInfo.Left = (lbInfo.Parent!.Width - lbInfo.Size.Width) / 2;
            lbWhat.Left = (lbWhat.Parent!.Width - lbWhat.Size.Width) / 2;
            btOK.Left = (btOK.Parent!.Width - btOK.Size.Width) / 2;
        }
    }
}
