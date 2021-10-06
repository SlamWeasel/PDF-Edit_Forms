using PDFLibNet64;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PDF_Edit_Froms.Util;
using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace PDF_Edit_Froms.Controls
{
    class PDFDisplay : Panel
    {
        public string Path { get; set; } = "";
        public bool Loading { get; private set; } = false;

        private PDFWrapper doc = new PDFWrapper();
        private PdfDocument document = new PdfDocument();
        private PdfPages documentPages;
        private List<Image> thumbnails = new List<Image>();
        private List<Control> pageThumbs = new List<Control>();
        private int nowAt = 10;
        private float scrollFac = 1f;
        private Size oldSize;
        private Label LoadingBox;

        public event EventHandler<PDFLoadingEventArgs> LoadingFinishedEventHandler;


        public PDFDisplay()
        {
            Path = "";
            document = null;
            documentPages = null;
            this.SizeChanged += new EventHandler(onSizeChanged);
            this.MouseWheel += new MouseEventHandler(onScroll);

            if (!Directory.Exists(@"C:\Temp")) 
                Directory.CreateDirectory(@"C:\Temp");
        }


        public void setPath(string path)
        {
            doc.Dispose();
            doc = new PDFWrapper();

            if (Path != "")
            {
                document.Close();
                Utils.freeFile(Path);
                this.Invoke(new Action(() => CreateLoadingBox()));
            }

            if (!Utils.IsFileLocked(path))
                File.Copy(path, @"C:\Temp\PDFPagey.pdf", true);
            else
            {
                MessageBox.Show("Datei kann nicht geöffnet werden!");
                return;
            }
            Path = path;

            while (Utils.IsFileLocked(@"C:\Temp\PDFPagey.pdf"))
                Console.WriteLine("Datei lädt noch");

            document = PdfReader.Open(@"C:\Temp\PDFPagey.pdf", PdfDocumentOpenMode.Import);
            documentPages = document.Pages;
            oldSize = this.Size;

            this.Controls.Clear();
            pageThumbs.Clear();
            thumbnails.Clear();
            this.Controls.Add(LoadingBox);
            try
            {
                FileInfo fi = new FileInfo(Path);
                this.Parent.Controls.Find("FileInfoBar", false)[0].Text = $"Name: {fi.Name} | Größe: {Util.Utils.getSizeAsString(Path)} | Ordner: {fi.DirectoryName}";
            }catch(Exception){ }

            this.Refresh();

            Initialize();
        }

        public void Initialize()
        {
            nowAt = 10;
            thumbnails.Clear();

            int i = 0;

            doc.LoadPDF(@"C:\Temp\PDFPagey.pdf");

            foreach (PdfPage p in documentPages)
            {
                Console.WriteLine($"Sharp: { p.Width}, {p.Height}");

                thumbnails.Add(RenderPage(doc, i, (int)Math.Round(p.Width.Value), (int) Math.Round(p.Height.Value)));

                int calculatedHeight = (int)Math.Round((((double)(this.Width - 20)) / thumbnails[i].Width) * (double)thumbnails[i].Height);
                PictureBox background = new PictureBox()
                {
                    BackColor = this.BackColor,
                    Image = thumbnails[i],
                    Bounds = new Rectangle(10, nowAt, this.Width - 20, calculatedHeight),
                    SizeMode = PictureBoxSizeMode.StretchImage
                };
                nowAt += calculatedHeight + 20;

                pageThumbs.Add(background);
                i++;

                if (LoadingBox != null)
                {
                    LoadingBox.Text = ChangeLoad(LoadingBox.Text.Length);
                    LoadingBox.Refresh();
                }

                OnLoading(new PDFLoadingEventArgs(new FileInfo(Path), documentPages.Count, i, !Loading));
            }

            this.Controls.Clear();
            foreach(Control c in pageThumbs)
                this.Controls.Add(c);

            this.Refresh();
            Loading = false;

            OnLoading(new PDFLoadingEventArgs(new FileInfo(Path), documentPages.Count, i, !Loading));
        }

        private string ChangeLoad(int atm)
        {
            switch (atm)
            {
                case 7:
                    return "Loading.";
                case 8:
                    return "Loading..";
                case 9:
                    return "Loading...";
                case 10:
                    return "Loading";
                default:
                    return "Loading";
            }
        }
        private void CreateLoadingBox()
        {
            Loading = true;
            LoadingBox = new Label()
            {
                Text = "Loading",
                Font = new Font("Consolas", 8.0F, FontStyle.Bold, GraphicsUnit.Point, 0),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.DarkGray,
                ForeColor = Color.Gray,
                Bounds = new Rectangle(40, (this.Height / 2) - 10, this.Width - 80, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                Name = "LoadingBox"
            };
        }


        private void onSizeChanged(object sender, EventArgs e)
        {
            if (oldSize.Width == 0) return;

            double fac = this.Size.Width / (double)oldSize.Width;
            scrollFac = (fac > 1) ? 1.6f : 1;

            foreach (Control c in this.Controls)
                c.Bounds = new Rectangle(c.Location.X, (int) Math.Round(c.Location.Y * fac), (int)Math.Round(c.Size.Width * fac), (int)Math.Round(c.Size.Height * fac));

            oldSize = this.Size;
        }
        /// <summary>
        /// Das Delta des MouseEvents ist -120 beim runterscrollen und 120 beim hochscrollen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void onScroll(object sender, MouseEventArgs e)
        {
            if(Controls[0].Location.Y < 0 || e.Delta < 0)
                foreach(Control c in this.Controls)
                    c.Location = new Point(c.Location.X, c.Location.Y + (int)Math.Round((e.Delta/5)*scrollFac));
            this.Refresh();
        }

        /// <summary>
        /// Gibt ein nutzbares <see cref="Image"/>-Objekt einer PDF-Datei zurück, ohne dieses als extra Datei hinterlegen zu müssen
        /// </summary>
        /// <param name="doc"><see cref="PDFWrapper"/>, des zu rendernden Dokuments</param>
        /// <param name="page">Seite, die gerendert werden soll (0 ist die erste Seite)</param>
        /// <param name="pageWidth">Stammt von der <see cref="PdfPage"/></param>
        /// <param name="pageHeight">Stammt von der <see cref="PdfPage"/></param>
        /// <returns><see cref="Image"/>-Objekt, das die angegebene Seit der angegeben Datei darstellt</returns>
        Image RenderPage(PDFWrapper doc, int page, int pageWidth, int pageHeight)
        {
            doc.CurrentPage = page + 1;
            doc.CurrentX = 0;
            doc.CurrentY = 0;

            using (PictureBox disposable = new PictureBox())
            {
                // Der Wrapper braucht einen Handle um das Bild zu erstellen, in dem Fall den von einer "Wegwerf-PictureBox"
                doc.RenderPage(disposable.Handle);

                // Erstellt das Bild und setzt die Bounds. (Aufgrund von einem Fehler der PDFLibNet-Library kann er nicht mit horizontalen Seiten umgehen,
                // daher nimmt das Programm hier die Dimensionen von PDF-Sharp, was damit besser umgehen kann
                Bitmap buffer = new Bitmap(pageWidth, pageHeight);
                doc.ClientBounds = new Rectangle(0, 0, pageWidth, pageHeight);

                using (Graphics g = Graphics.FromImage(buffer))
                {
                    IntPtr hdc = g.GetHdc();
                    try
                    {
                        doc.DrawPageHDC(hdc);
                    }
                    finally
                    {
                        g.ReleaseHdc();
                    }
                }

                return buffer;
            }
        }

        private void OnLoading(PDFLoadingEventArgs e)
        {
            if(LoadingFinishedEventHandler != null)
                LoadingFinishedEventHandler(this, e);
        }
    }

    public class PDFLoadingEventArgs : EventArgs
    {
        public FileInfo Document { get; set; }
        public int PageAmount { get; set; }
        public int PageAt { get; set; }
        public bool Finished { get; set; }

        public PDFLoadingEventArgs(FileInfo _file, int _total, int _current, bool _done)
        {
            Document = _file;
            PageAmount = _total;
            PageAt = _current;
            Finished = _done;
        }
    }
}
