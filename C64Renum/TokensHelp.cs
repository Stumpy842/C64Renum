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
    public partial class TokensHelp : Form
    {
        public TokensHelp()
        {
            InitializeComponent();
            this.Icon = Properties.Resources.C64Renum;
        }

        private void TokensHelp_Load(object sender, EventArgs e)
        {
            this.SuspendLayout();
            lbTokens.Text = string.Join(", ", Tokens.Tk);
            lbReserved.Text = string.Join(", ", Tokens.ResV);
            this.ResumeLayout();
            this.KeyPreview = true;
        }

        private void btClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void TokensHelp_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;
                Close();
            }
        }
    }
}
