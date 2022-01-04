using PDFLibNet64;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PDF_Edit_Forms.Util;
using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using PdfSharp.Pdf.Advanced;

namespace PDF_Edit_Forms.Controls
{
    class PDFDisplay : Panel
    {
        public string Path { get; set; } = "";
        public bool Loading { get; private set; } = false;

        private PDFWrapper docWrapper = new PDFWrapper();
        public PdfDocument document = new PdfDocument();
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
            docWrapper.Dispose();
            docWrapper = new PDFWrapper();

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
            this.Controls.Add(LoadingBox);
            try
            {
                FileInfo fi = new FileInfo(Path);
                this.Parent.Controls.Find("FileInfoBar", false)[0].Text = $"Name: {fi.Name} | Größe: {Util.Utils.getSizeAsString(Path)} | Ordner: {fi.DirectoryName}";
            }catch(Exception){ }

            this.Refresh();

            docWrapper.LoadPDF(@"C:\Temp\PDFPagey.pdf");

            Initialize();
        }
        public void setDocument(PdfDocument _document)
        {
            docWrapper.Dispose();
            docWrapper = new PDFWrapper();
            document.Close();

            Utils.freeFile(Path);
            this.Invoke(new Action(() => CreateLoadingBox()));
            while (Utils.IsFileLocked(@"C:\Temp\PDFEditson.pdf"))
            {
                Console.WriteLine("Datei lädt noch");
                Utils.freeFile(@"C:\Temp\PDFEditson.pdf");
            }

            document = _document;
            docWrapper.LoadPDF(@"C:\Temp\PDFEditson.pdf");
            documentPages = document.Pages;

            this.Controls.Clear();
            pageThumbs.Clear();
            this.Controls.Add(LoadingBox);

            this.Refresh();

            Initialize();
        }

        public void Initialize()
        {
            nowAt = 10;
            thumbnails.Clear();

            int i = 0;

            foreach (PdfPage p in document.Pages)
            {
                Console.WriteLine($"Sharp: {p.Width}, {p.Height}");

                thumbnails.Add(RenderPage(docWrapper, i, (int)Math.Round(p.Width.Value), (int) Math.Round(p.Height.Value)));

                int calculatedHeight = (int)Math.Round((((double)(this.Width - 20)) / thumbnails[i].Width) * (double)thumbnails[i].Height);
                PictureBox background = new PictureBox()
                {
                    Name = (i + 1).ToString(),
                    BackColor = this.BackColor,
                    Image = thumbnails[i],
                    Bounds = new Rectangle(10, nowAt, this.Width - 20, calculatedHeight),
                    SizeMode = PictureBoxSizeMode.StretchImage
                };
                background.ContextMenu = CreateContext(background);

                nowAt += calculatedHeight + 20;

                pageThumbs.Add(background);
                i++;

                if (LoadingBox != null)
                {
                    LoadingBox.Text = ChangeLoad(LoadingBox.Text.Length);
                    LoadingBox.Refresh();
                }

                OnLoading(new PDFLoadingEventArgs(new FileInfo(Path), document.Pages.Count, i, !Loading));
            }

            this.Controls.Clear();
            foreach(Control c in pageThumbs)
                this.Controls.Add(c);

            this.Refresh();
            Loading = false;

            OnLoading(new PDFLoadingEventArgs(new FileInfo(Path), document.Pages.Count, i, !Loading));
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
        ContextMenu CreateContext(PictureBox thumb)
        {
            MenuItem[] items = new MenuItem[]
            {
                new MenuItem("Seite " + thumb.Name)
                {
                    Enabled = false,
                    DefaultItem = true,
                    BarBreak = true,
                },
                new MenuItem("Drehen", new MenuItem[] 
                { 
                    new MenuItem("Nach links"), 
                    new MenuItem("Nach rechts"),
                    new MenuItem("180°")
                }),
                new MenuItem("Verschieben", new MenuItem[]
                {
                    new MenuItem("Nach oben"),
                    new MenuItem("Nach unten"),
                    new MenuItem("An den Anfang"),
                    new MenuItem("An das Ende")
                }),
                new MenuItem("Entfernen", new EventHandler(onContextClickRemove))
            };

            ContextMenu contextMenu = new ContextMenu(items)
            {
                Name = "thumbnailContext_" + thumb.Name
            };

            return contextMenu;
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
            if (Path == "")
                return;

            if (Controls[0].Location.Y < 0 || e.Delta < 0)
                foreach(Control c in this.Controls)
                    c.Location = new Point(c.Location.X, c.Location.Y + (int)Math.Round((e.Delta/5)*scrollFac));
            this.Refresh();
        }
        public void onContextClickRemove(object sender, EventArgs e)
        {
            setDocument(PdfActions.removePages(document, new List<int> { int.Parse(((MenuItem)sender).Parent.Name.Substring(17)) - 1 }, docWrapper));
        }

        private void OnLoading(PDFLoadingEventArgs e)
        {
            if(LoadingFinishedEventHandler != null)
                LoadingFinishedEventHandler(this, e);
            if (e.Finished)
            {
                try
                {
                    foreach (PdfPage p in documentPages)
                    {
                        PdfDictionary pageResources = p.Elements.GetDictionary("/Resources");
                        PdfDictionary xObjects = pageResources.Elements.GetDictionary("/XObject");
                        ICollection<PdfItem> items = xObjects.Elements.Values;
                        foreach (PdfItem item in items)
                        {
                            PdfDictionary xObject = (PdfDictionary)((PdfReference)item).Value;
                            string xObjectName = string.Join("\n", xObject.Elements) + "\n----------";
                            Console.WriteLine(xObjectName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                //Console.WriteLine("Done!");
            }

            /*
            foreach (var obj in document.Internals.FirstDocumentGuid.ToString())
            {
                Console.WriteLine("{");
                Console.WriteLine(obj.ToString());
                Console.WriteLine(obj.Reference.ToString());
                Console.WriteLine("}");
            }*/
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
