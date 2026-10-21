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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace C64Renum
{
    public class FocusableLabel : UserControl
    {
        public FocusableLabel()
        {
            // 1. Configure the control to behave like a focusable graphics square
            this.SetStyle(ControlStyles.Selectable, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            this.TabStop = true;
            this.Size = new Size(25, 25); // Default convenient swatch size
            this.BackColor = Color.Red;   // Default sample fallback color
        }

        // 2. Allow hitting 'Enter' or 'Space' to trigger the color picker when tabbed onto
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
            {
                this.OnClick(EventArgs.Empty);
                e.Handled = true;
            }
        }

        // 3. Re-draw the control to show or hide the focus ring
        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            this.Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            this.Invalidate();
        }

        // Custom border color for control when focused
        private Color highlightColor;

        // 4. Cleanly render the background color block and its visual focus state
        protected override void OnPaint(PaintEventArgs e)
        {
            // 1. Draw the primary background color fill
            using (Brush brush = new SolidBrush(this.BackColor))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }

            // 2. Draw a prominent, thick, button-style solid border if focused
            if (this.Focused)
            {
                // Use SystemColors.Highlight (typically Blue) or Color.Black/Color.DimGray 
                // depending on your preferred visual aesthetic.
                highlightColor = Tools.GetContrastColor(this.BackColor);
                using Pen focusPen = new(highlightColor, 1);

                // Custom pattern: [Dash length in pixels, Space length in pixels]
                // This creates highly precise 2px dashes separated by 2px spaces
                focusPen.DashPattern = new float[] { 2.0f, 2.0f };

                Rectangle borderRect = new(
                           this.ClientRectangle.X,
                           this.ClientRectangle.Y,
                           this.ClientRectangle.Width - 1,
                           this.ClientRectangle.Height - 1
                       );

                e.Graphics.DrawRectangle(focusPen, borderRect);
            }
            else
            {
                // Optional: Draw a very subtle 1px gray border when NOT focused 
                // so light colors (like white swatches) don't disappear into a light form background.
                using Pen neutralPen = new(Color.LightGray, 1);
                Rectangle neutralRect = new(
                    this.ClientRectangle.X,
                    this.ClientRectangle.Y,
                    this.ClientRectangle.Width - 1,
                    this.ClientRectangle.Height - 1
                );
                e.Graphics.DrawRectangle(neutralPen, neutralRect);
            }
        }
    }
}
