using PDF_Edit_Froms.Util;
using PDF_Edit_Froms.Controls;
using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PDF_Edit_Froms.Forms;

#pragma warning disable IDE0044 // Modifizierer "readonly" hinzufügen
#pragma warning disable IDE1006 // Benennungsstile
namespace PDF_Edit_Froms
{
    partial class Form1
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
        //private Coord lastLoc = new Coord(0, 0, "lastLoc");
        private Color   GrayColor = Color.FromArgb(100, 100, 100),
                        DarkGrayColor = Color.FromArgb(50, 50, 50),
                        GrayFontColor = Color.FromArgb(200, 200, 200),
                        inb4;
        private string startupFile = "", settingsFilePath = Application.StartupPath + "\\settings.txt";
        private Dictionary<string, string> settings = new Dictionary<string, string>();
        public bool isDarkMode = true;

        private OpenFileDialog openFileDialog;
        private Label WindowBar, fileInfo;
        private PDFDisplay view;
        private IconButton openFile, combine, split, edit;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Initialisiert das Haupt-Fenster
        /// </summary>
        private void InitializeComponent()
        {
            if (!File.Exists(settingsFilePath))
            {
                File.Create(settingsFilePath);
                while (Utils.IsFileLocked(settingsFilePath)) Utils.freeFile(startupFile);
                File.WriteAllText(settingsFilePath, @"darkMode;true" + "\n" + @"startupFile;\\nt-file\home$\evlehmann\Eigene Dateien\My Pictures\Dokumente\ÄÖÜßäöü.pdf");
            }
            foreach (string s in File.ReadAllLines(settingsFilePath))
                settings.Add(s.Split(';')[0], s.Split(';')[1]);

            #region Hauptfenstereigenschaften
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = DarkGrayColor;
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainWindow";
            this.FormBorderStyle = 0;

            this.SizeChanged += new EventHandler(onFormSizeChanged);
            this.Paint +=       new PaintEventHandler(onFormPaint);
            this.DragEnter +=   new DragEventHandler(onFormDragEnter);
            this.DragDrop +=    new DragEventHandler(onFormDragDrop);
            this.Disposed +=    new EventHandler(onFormClosed);
            #endregion

            #region Controls
            fileInfo = new Label()
            {
                Name = "FileInfoBar",
                Text = "Name: - | Größe: - | Ordner: -",
                TextAlign = ContentAlignment.MiddleRight,
                Bounds = new Rectangle(1, this.Height - 30, this.Width / 2 - 15, 29),
                ForeColor = GrayFontColor
            };
            this.Controls.Add(fileInfo);

            WindowBar = new Label()
            {
                Size = new Size(this.Size.Width, 30),
                BackColor = Color.FromArgb(55, 55, 55),
                ForeColor = GrayFontColor,
                Text = "    PDF-Edit",
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Consolas", 10.0F, FontStyle.Bold, GraphicsUnit.Point, 0)
            };
            WindowBar.MouseDown += new MouseEventHandler(onWindowBarGrabbed);
            WindowBar.Paint += new PaintEventHandler(onWindowBarPaint);

            view = new PDFDisplay()
            {
                Bounds = new Rectangle(this.Width / 2, 31, (this.Width / 2) - 1, this.Height - 32),
                BackColor = this.BackColor,
                ForeColor = Color.Black,
                BorderStyle = BorderStyle.None
            };
            view.setPath(settings[Settings.startupFile]);
            view.Paint += new PaintEventHandler(onPdfPaint);
            view.LoadingFinishedEventHandler += new EventHandler<PDFLoadingEventArgs>(onViewLoadingFinished);

            openFile = new IconButton((Icon)resources.GetObject("$this.PDF_File_Open_Def"), (Icon)resources.GetObject("$this.PDF_File_Open_Hov"))
            {
                Bounds = new Rectangle( (int)Math.Round(this.Width / 120.0), 
                                        (int)Math.Round(this.Width / 40.0) + WindowBar.Height, 
                                        (int)Math.Round((this.Width / 9.0) * 1.5), 
                                        (int)Math.Round(this.Width / 9.0))
            };
            openFile.Click += new EventHandler(onOpenFileClick);
            combine = new IconButton((Icon)resources.GetObject("$this.PDF_File_Combine_Def"), (Icon)resources.GetObject("$this.PDF_File_Combine_Hov"))
            {
                Bounds = new Rectangle( (int)Math.Round(this.Width / 6.0),
                                        (int)Math.Round(this.Height / 2.5),
                                        (int)Math.Round((this.Width / 9.0) * 1.5),
                                        (int)Math.Round(this.Width / 9.0))
            };
            combine.Click += new EventHandler(onCombineClick);
            split = new IconButton((Icon)resources.GetObject("$this.PDF_File_Seperate_Def"), (Icon)resources.GetObject("$this.PDF_File_Seperate_Hov"))
            {
                Bounds = new Rectangle( (int)Math.Round(this.Width / 110.0),
                                        (int)Math.Round(this.Height / 1.3) - 30,
                                        (int)Math.Round((this.Width / 9.0) * 1.5),
                                        (int)Math.Round(this.Width / 9.0))
            };
            edit = new IconButton((Icon)resources.GetObject("$this.PDF_File_Edit_Def"), (Icon)resources.GetObject("$this.PDF_File_Edit_Hov"))
            {
                Bounds = new Rectangle( (int)Math.Round(this.Width / 3.0),
                                        (int)Math.Round(this.Height / 1.3) - 30,
                                        (int)Math.Round((this.Width / 9.0) * 1.5),
                                        (int)Math.Round(this.Width / 9.0))
            };
            #endregion


            this.Controls.Add(WindowBar);
            this.Controls.Add(view);
            this.Controls.Add(openFile);
            this.Controls.Add(combine);
            this.Controls.Add(split);
            this.Controls.Add(edit);



            if (!Convert.ToBoolean(settings[Settings.darkMode]))
            {
                isDarkMode = false;
                ColorSwitch();
            }

            this.CenterToScreen();
            this.Refresh();
        }



        #region Event-Funktionen
        private void onViewLoadingFinished(object sender, PDFLoadingEventArgs e)
        {
            Console.WriteLine(e.Document.Name + "? -> " + e.Finished + " | (" + e.PageAt + "/" + e.PageAmount + ")");
        }
        /// <summary>
        /// Lässt einen Button hell erscheinen, wenn man sich mit der Maus darüber bewegt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onMouseEnterButton(object sender, EventArgs e)
        {
            inb4 = ((Button)sender).ForeColor;
            ((Button)sender).BackColor = Color.LightGray;
            ((Button)sender).ForeColor = Color.Black;
        }
        /// <summary>
        /// Lässt einen Button wieder abdunkeln, wenn man sich mit der Maus davon runter bewegt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onMouseLeaveButton(object sender, EventArgs e)
        {
            ((Button)sender).BackColor = Color.FromArgb(100, 100, 100);
            ((Button)sender).ForeColor = inb4;
        }
        /// <summary>
        /// Passt die Größe und Position der Elemente an, um sie der veränderten Größe des Fensters anzupassen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

            view.Bounds = new Rectangle(this.Width / 2, 31, (this.Width / 2) - 1, this.Height - 32);

            openFile.Bounds = new Rectangle((int)Math.Round(this.Width / 120.0),
                                            (int)Math.Round(this.Width / 40.0) + WindowBar.Height,
                                            (int)Math.Round((this.Width / 9.0) * 1.5),
                                            (int)Math.Round(this.Width / 9.0));
            combine.Bounds = new Rectangle( (int)Math.Round(this.Width / 6.0),
                                            (int)Math.Round(this.Height / 2.5),
                                            (int)Math.Round((this.Width / 9.0) * 1.5),
                                            (int)Math.Round(this.Width / 9.0));
            split.Bounds = new Rectangle(   (int)Math.Round(this.Width / 110.0),
                                            (int)Math.Round(this.Height / 1.3) - 30,
                                            (int)Math.Round((this.Width / 9.0) * 1.5),
                                            (int)Math.Round(this.Width / 9.0));
            edit.Bounds = new Rectangle(    (int)Math.Round(this.Width / 3.0),
                                            (int)Math.Round(this.Height / 1.3) - 30,
                                            (int)Math.Round((this.Width / 9.0) * 1.5),
                                            (int)Math.Round(this.Width / 9.0));

            fileInfo.Bounds = new Rectangle(1, this.Height - 30, this.Width / 2 - 15, 29);

            this.Refresh();
        }
        /// <summary>
        /// Zeichnet einige optisch aufhübschende Linien
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onFormPaint(object sender, PaintEventArgs e)
        {
            //Trennlinie zur unteren Info-Leiste
            e.Graphics.DrawLine(new Pen(Color.FromArgb(65, 65, 65), 1), 0, this.Height - 31, 301, this.Height - 31);
            e.Graphics.DrawLine(new Pen(Color.FromArgb(55, 55, 55), 1), 0, this.Height - 32, 300, this.Height - 32);

            //Äußere Umrandung
            e.Graphics.DrawLine(new Pen(Color.Black, 1), 0, 0, 0, this.Height);
            e.Graphics.DrawLine(new Pen(Color.Black, 1), 0, this.Height - 1, this.Width - 1, this.Height - 1);
            e.Graphics.DrawLine(new Pen(Color.Black, 1), this.Width - 1, 0, this.Width - 1, this.Height - 1);
        }
        /// <summary>
        /// Muss aktiviert werden, damit eine Datei in das Fenster gezogen werden kann
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onFormDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }
        /// <summary>
        /// Öffnet die Datei die per Drag and Drop in das Fenster reingezogen wurde
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onFormDragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if(files.Length == 1 && files[0].Substring(files[0].Length - 4).Contains("pdf"))
                view.setPath(files[0]);
        }
        private void onFormClosed(object sender, EventArgs e)
        {
            Utils.CleanUp();
        }
        #endregion

        #region Button-Funktionen
        private void onOpenFileClick(object sender, EventArgs e)
        {
            openFileDialog = new OpenFileDialog()
            {
                Filter = "Portable Document Format (*.pdf)|*.pdf"
            };
            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                view.setPath(openFileDialog.FileName);
            }
        }
        private void onCombineClick(object sender, EventArgs e)
        {
            CombineFilesForm test = new CombineFilesForm("Test", "CombineFilesWindow");
            test.Show();
        }
        #endregion



        /// <summary>
        /// Zeichnet die Trennlinie der zwei Hälften beim PaintEvent des PDFDisplay-Elements
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onPdfPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawLine(new Pen(Color.FromArgb(55, 55, 55), 1), 0, ((Control)sender).Height / 30, 0, ((Control)sender).Height - (((Control)sender).Height / 30));
            e.Graphics.DrawLine(new Pen(Color.FromArgb(65, 65, 65), 1), 1, ((Control)sender).Height / 30, 1, ((Control)sender).Height - (((Control)sender).Height / 30));
        }
        /// <summary>
        /// Zeichnet Icons sowie Trennlinien in das Fenster
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onWindowBarPaint(object sender, PaintEventArgs e)
        {
            Icon ico = (Icon)resources.GetObject("$this.Icon");
            Icon x = (Icon)resources.GetObject("$this.X");
            Icon moon = (Icon)resources.GetObject("$this.Moon");
            Icon sun = (Icon)resources.GetObject("$this.Sun");
            Image fixLogoLight = (Image)resources.GetObject("Logo_Fix_White");

            e.Graphics.DrawImage(ico.ToBitmap(), 2, 2, 26, 26);
            e.Graphics.DrawImage(x.ToBitmap(), ((Control)sender).Width - 30, 0, 30, 30);
            if (isDarkMode)
                e.Graphics.DrawImage(sun.ToBitmap(), ((Control)sender).Width - 60, 0, 30, 30);
            else
                e.Graphics.DrawImage(moon.ToBitmap(), ((Control)sender).Width - 60, 0, 30, 30);
            e.Graphics.DrawImage(fixLogoLight, ((Control)sender).Width - 160, 7, 100, 18);
            e.Graphics.DrawLine(new Pen(Color.Black, 1), 0, 0, 0, this.Height);
            e.Graphics.DrawLine(new Pen(Color.Black, 1), this.Width - 1, 0, this.Width - 1, this.Height - 1);
            e.Graphics.DrawLine(new Pen(Color.Black, 1), 0, 0, this.Width - 1, 0);
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
                else if(e.Location.X > ((Control)sender).Width - 60)
                {
                    isDarkMode = !isDarkMode;
                    ColorSwitch();
                    this.Refresh();

                    settings[Settings.darkMode] = isDarkMode.ToString();
                    File.WriteAllText(settingsFilePath, Utils.settingsToString(settings));
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



        /// <summary>
        /// Applies the Dark Design to a Button with specific buttoncolor, textcolor and font
        /// </summary>
        /// <param name="butt"></param>
        void turnDark(Button butt, Color fore, Color back, Font font)
        {
            butt.ForeColor = fore;
            butt.BackColor = back;
            butt.Font = font;
            butt.FlatStyle = FlatStyle.Popup;
            butt.MouseEnter += new EventHandler(onMouseEnterButton);
            butt.MouseLeave += new EventHandler(onMouseLeaveButton);
        }
        /// <summary>
        /// Applies the Dark Design to a Button with specific textcolor
        /// </summary>
        /// <param name="butt"></param>
        void turnDark(Button butt, Color fore)
        {
            butt.ForeColor = fore;
            butt.BackColor = Color.FromArgb(100, 100, 100);
            butt.Font = new Font("Verdana", 8.0F, FontStyle.Regular, GraphicsUnit.Point, 0);
            butt.FlatStyle = FlatStyle.Popup;
            butt.MouseEnter += new EventHandler(onMouseEnterButton);
            butt.MouseLeave += new EventHandler(onMouseLeaveButton);
        }
        /// <summary>
        /// Applies the Dark Design to a Button with specific  font
        /// </summary>
        /// <param name="butt"></param>
        void turnDark(Button butt, Font font)
        {
            butt.ForeColor = Color.FromArgb(200, 200, 200);
            butt.BackColor = Color.FromArgb(100, 100, 100);
            butt.Font = font;
            butt.FlatStyle = FlatStyle.Popup;
            butt.MouseEnter += new EventHandler(onMouseEnterButton);
            butt.MouseLeave += new EventHandler(onMouseLeaveButton);
        }
        /// <summary>
        /// Applies the Dark Design to a Button 
        /// </summary>
        /// <param name="butt"></param>
        void turnDark(Button butt)
        {
            butt.ForeColor = Color.FromArgb(200, 200, 200);
            butt.BackColor = Color.FromArgb(100, 100, 100);
            butt.Font = new Font("Verdana", 8.0F, FontStyle.Regular, GraphicsUnit.Point, 0);
            butt.FlatStyle = FlatStyle.Popup;
            butt.MouseEnter += new EventHandler(onMouseEnterButton);
            butt.MouseLeave += new EventHandler(onMouseLeaveButton);
        }
        /// <summary>
        /// Recolors the Window from dark to light and vis versa depending on the global variable <param name="isDarkMode"><c>isDarkMode</c></param>
        /// </summary>
        private void ColorSwitch()
        {
            switch(isDarkMode)
            {
                case true:  // Dark Theme

                    this.BackColor = DarkGrayColor;
                    view.BackColor = this.BackColor;
                    WindowBar.BackColor = Color.FromArgb(55, 55, 55);
                    WindowBar.ForeColor = GrayFontColor;
                    fileInfo.ForeColor = GrayFontColor;

                    break;

                case false: // Light Theme

                    this.BackColor = Color.FromArgb(240, 240, 240);
                    view.BackColor = this.BackColor;
                    WindowBar.BackColor = Color.FromArgb(200, 200, 200);
                    WindowBar.ForeColor = DarkGrayColor;
                    fileInfo.ForeColor = DarkGrayColor;

                    break;
            }
        }
    }

    static class Settings
    {
        public const string 
            darkMode = "darkMode", 
            startupFile = "startupFile";
    }
}

