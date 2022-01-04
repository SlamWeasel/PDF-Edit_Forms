using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PDF_Edit_Forms.Controls
{
    class IconButton : Label
    {
        public Icon Icon { get; set; }
        private Icon defaultIcon, hoverIcon;

        public IconButton(Icon def, Icon hov)
        {
            defaultIcon = def;
            hoverIcon = hov;
            Icon = defaultIcon;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            Icon = hoverIcon;

            base.OnMouseEnter(e);
            this.Refresh();
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            Icon = defaultIcon;

            base.OnMouseLeave(e);
            this.Refresh();
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            Bounds = new Rectangle(this.Location.X - 3, this.Location.Y - 3, this.Size.Width + 6, this.Size.Height + 6);

            this.Refresh();
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            Bounds = new Rectangle(this.Location.X + 3, this.Location.Y + 3, this.Size.Width - 6, this.Size.Height - 6);

            this.Refresh();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(Icon.ToBitmap(), new Rectangle(new Point(0, 0), this.Size));

            base.OnPaint(e);
        }
    }
}
