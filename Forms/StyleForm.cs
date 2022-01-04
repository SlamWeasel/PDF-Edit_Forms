using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PDF_Edit_Forms.Util
{
    /// <summary>
    /// Vordesigntes Fenster für den PDF-Editor
    /// </summary>
    class StyleForm : Form
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        private Color   GrayColor = Color.FromArgb(100, 100, 100),
                        DarkGrayColor = Color.FromArgb(50, 50, 50),
                        GrayFontColor = Color.FromArgb(200, 200, 200);
        private string startupFile = "", settingsFilePath = Application.StartupPath + "\\settings.txt";
        private Dictionary<string, string> settings = new Dictionary<string, string>();

        public bool isDarkMode = true;


        private Label WindowBar;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="windowTitle"></param>
        /// <param name="windowName"></param>
        public StyleForm(string windowTitle, string windowName)
        {
            if (!File.Exists(settingsFilePath))
            {
                File.Create(settingsFilePath);
                while (Utils.IsFileLocked(settingsFilePath)) 
                    Utils.freeFile(startupFile);
                File.WriteAllText(settingsFilePath, @"darkMode;true" + "\n" + @"startupFile;\\nt-file\home$\evlehmann\Eigene Dateien\My Pictures\Dokumente\ÄÖÜßäöü.pdf");
            }
            foreach (string s in File.ReadAllLines(settingsFilePath))
                settings.Add(s.Split(';')[0], s.Split(';')[1]);


            this.Icon = (Icon)resources.GetObject("$this.Icon");
            this.Name = windowName;
            this.Text = windowTitle;
            this.FormBorderStyle = 0;
            this.BackColor = DarkGrayColor;

            this.SizeChanged += new EventHandler(onFormSizeChanged);
            this.Paint += new PaintEventHandler(onFormPaint);



            WindowBar = new Label()
            {
                Size = new Size(this.Size.Width, 30),
                BackColor = Color.FromArgb(55, 55, 55),
                ForeColor = GrayFontColor,
                Text = "    " + this.Text,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Consolas", 10.0F, FontStyle.Bold, GraphicsUnit.Point, 0)
            };
            WindowBar.MouseDown += new MouseEventHandler(onWindowBarGrabbed);
            WindowBar.Paint += new PaintEventHandler(onWindowBarPaint);



            Controls.Add(WindowBar);

            if (!Convert.ToBoolean(settings[Settings.darkMode]))
            {
                isDarkMode = false;
                ColorSwitch();
            }
        }

        private void onFormSizeChanged(object sender, EventArgs e)
        {
            WindowBar.Width = this.Width;
            this.Update();
            WindowBar.Update();
            Graphics g = this.CreateGraphics();
            g.Clear(this.BackColor);
            if (this.WindowState == FormWindowState.Normal)
            {
                g.DrawLine(new Pen(Color.Black, 1), 0, 0, 0, this.Height);
                g.DrawLine(new Pen(Color.Black, 1), 0, this.Height - 1, this.Width - 1, this.Height - 1);
                g.DrawLine(new Pen(Color.Black, 1), this.Width - 1, 0, this.Width - 1, this.Height - 1);
            }
        }
        private void onFormPaint(object sender, PaintEventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                e.Graphics.DrawLine(new Pen(Color.Black, 1), 0, 0, 0, this.Height);
                e.Graphics.DrawLine(new Pen(Color.Black, 1), 0, this.Height - 1, this.Width - 1, this.Height - 1);
                e.Graphics.DrawLine(new Pen(Color.Black, 1), this.Width - 1, 0, this.Width - 1, this.Height - 1);
            }
        }
        private void onWindowBarPaint(object sender, PaintEventArgs e)
        {
            Icon ico = (Icon)resources.GetObject("$this.Icon");
            Icon x = (Icon)resources.GetObject("$this.X");
            Icon moon = (Icon)resources.GetObject("$this.Moon");
            Icon sun = (Icon)resources.GetObject("$this.Sun");
            Image fixLogoLight = (Image)resources.GetObject("Logo_Fix_White");
            e.Graphics.DrawImage(ico.ToBitmap(), 2, 2, 26, 26);
            e.Graphics.DrawImage(x.ToBitmap(), ((Control)sender).Width - 30, 0, 30, 30);
            e.Graphics.DrawImage(fixLogoLight, ((Control)sender).Width - 130, 7, 100, 18);
            if (this.WindowState == FormWindowState.Normal)
            {
                e.Graphics.DrawLine(new Pen(Color.Black, 1), 0, 0, 0, this.Height);
                e.Graphics.DrawLine(new Pen(Color.Black, 1), this.Width - 1, 0, this.Width - 1, this.Height - 1);
                e.Graphics.DrawLine(new Pen(Color.Black, 1), 0, 0, this.Width - 1, 0);
            }
        }
        #region Sorgt dafür, das Das Objekt, dem die Funktion als MouseDownEvent hinzugefügt wird, zum Fensterhandle wird, um es zu verschieben etc.
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private void onWindowBarGrabbed(object sender, MouseEventArgs e)
        {
            Console.WriteLine(e.Location.X);
            if (e.Button == MouseButtons.Left)
            {
                if (e.Location.X > ((Control)sender).Width - 30)
                {
                    this.Close();
                    return;
                }

                this.WindowState = (this.WindowState == FormWindowState.Maximized) ? FormWindowState.Normal : this.WindowState;
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);

                int screenOffset = Screen.FromPoint(Cursor.Position).Bounds.Y;
                if (Cursor.Position.Y - screenOffset == 0)
                    this.WindowState = FormWindowState.Maximized;
            }
        }
        #endregion

        private void ColorSwitch()
        {
            switch (isDarkMode)
            {
                case true:  // Dark Theme

                    this.BackColor = DarkGrayColor;
                    WindowBar.BackColor = Color.FromArgb(55, 55, 55);
                    WindowBar.ForeColor = GrayFontColor;

                    break;

                case false: // Light Theme

                    this.BackColor = Color.FromArgb(240, 240, 240);
                    WindowBar.BackColor = Color.FromArgb(200, 200, 200);
                    WindowBar.ForeColor = DarkGrayColor;

                    break;
            }
        }

        #region FormWindow-DropShadow
        /*protected override CreateParams CreateParams
        {
            get
            {
                const int CS_DROPSHADOW = 0x20000;
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }*/
        #endregion
        #region FormWindow-SmoothAroundBoxShadow
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect, // x-coordinate of upper-left corner
            int nTopRect, // y-coordinate of upper-left corner
            int nRightRect, // x-coordinate of lower-right corner
            int nBottomRect, // y-coordinate of lower-right corner
            int nWidthEllipse, // height of ellipse
            int nHeightEllipse // width of ellipse
        );

        [DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("dwmapi.dll")]
        public static extern int DwmIsCompositionEnabled(ref int pfEnabled);

        private bool m_aeroEnabled;                     // variables for box shadow
        private const int CS_DROPSHADOW = 0x00020000;
        private const int WM_NCPAINT = 0x0085;

        public struct MARGINS                           // struct for box shadow
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                m_aeroEnabled = CheckAeroEnabled();

                CreateParams cp = base.CreateParams;
                if (!m_aeroEnabled)
                    cp.ClassStyle |= CS_DROPSHADOW;

                return cp;
            }
        }

        private bool CheckAeroEnabled()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                int enabled = 0;
                DwmIsCompositionEnabled(ref enabled);
                return (enabled == 1) ? true : false;
            }
            return false;
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_NCPAINT:                        // box shadow
                    if (m_aeroEnabled)
                    {
                        var v = 2;
                        DwmSetWindowAttribute(this.Handle, 2, ref v, 4);
                        MARGINS margins =
                            new MARGINS()
                            {
                                bottomHeight = 1,
                                leftWidth = 1,
                                rightWidth = 1,
                                topHeight = 1
                            };
                        DwmExtendFrameIntoClientArea(this.Handle, ref margins);

                    }
                    break;
                default:
                    break;
            }
            base.WndProc(ref m);

        }
        #endregion

    }

    static class Settings
    {
        public const string 
            darkMode = "darkMode", 
            startupFile = "startupFile";
    }
}
