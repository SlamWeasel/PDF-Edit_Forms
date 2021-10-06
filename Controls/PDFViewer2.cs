using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Drawing;
using Microsoft.VisualBasic.FileIO;

namespace PDF_Edit_Froms.Controls
{
#pragma warning disable IDE0052 // Ungelesene private Member entfernen
#pragma warning disable IDE0044 // Modifizierer "readonly" hinzufügen
#pragma warning disable IDE1006 // Benennungsstile
    /// <summary>
    /// Forms Element to Display the Pages of a a PDF File
    /// </summary>
    class PdfViewer2 : Panel
    {
        //------------------------------------------------------------
        public bool noViewerMode { get; set; } = false;
        //------------------------------------------------------------


        private string          path;
        private PdfDocument     document;
        private int             pageAmount;
        private double          pageAt,
                                nowAt,
                                imgX, imgY;
        private List<Image>     thumbnails = new List<Image>();
        private List<Rectangle> thumbSquares = new List<Rectangle>();
        private Rectangle       backtangle;
        private bool            pageIndicVisible = false;

        private Label           pageIndic = new Label();
        private FlowLayoutPanel view = new FlowLayoutPanel();
        private LoadingBar      loadBar;



        public PdfViewer2()
        {
            path = "";
            document = null;
            pageAmount = 0;
            pageAt = 0;
            pageIndicVisible = false;
            view.AutoScroll = true;
        }
        public PdfViewer2(string filePath)
        {
            setPath(filePath);
        }
        public PdfViewer2(string filePath, bool onePage)
        {
            noViewerMode = onePage;
            setPath(filePath);
        }

        #region Getter&Setter
        /// <summary>
        /// Returns the full path of the PDF File the Element is displaying
        /// </summary>
        /// <returns></returns>
        public string getPath()
        {
            return path;
        }
        /// <summary>
        /// Insert a PDF File via full directory path into the Viewer Element
        /// </summary>
        /// <param name="p">Path to the PDF File</param>
        public void setPath(string p)
        {

            this.Controls.Clear();
            view.Controls.Clear();
            this.Controls.Add(pageIndic);
            view.Dock = DockStyle.Fill;
            this.Controls.Add(view);
            this.Disposed += new EventHandler(onDispose);

            /*foreach (string fi in FileSystem.GetFiles(@"C:\Temp"))
                if (fi.Contains("converted"))*/
                    //PDFEditson.tryDelete(fi);

            path = p;
            document = PdfReader.Open(path, PdfDocumentOpenMode.Import);
            pageAmount = document.PageCount;
            pageAt = 1;

            //Bar();

            thumbnails.Clear();
            thumbSquares.Clear();
            nowAt = 0;
            int index = 0;
            foreach (PdfPage page in document.Pages)
            {
                index++;
                if (!noViewerMode)
                {
                    Label pay = pageToControl(page, index);
                    imgX = (page.Width.Value < page.Height.Value) ? page.Width.Value / (page.Height.Value / (this.Width - 40)) : page.Width.Value / (page.Width.Value / (this.Width - 40));
                    imgY = (page.Width.Value < page.Height.Value) ? page.Height.Value / (page.Height.Value / (this.Width - 40)) : page.Height.Value / (page.Width.Value / (this.Width - 40));
                    thumbSquares.Add(new Rectangle(0, 0, Convert.ToInt32(imgX), Convert.ToInt32(imgY)));
                    pay.Bounds = new Rectangle(30, Convert.ToInt32(10 + nowAt), Convert.ToInt32(imgX), Convert.ToInt32(imgY));
                    //MessageBox.Show(page.Width.Value + ", " + page.Height.Value);
                    nowAt += pay.Height + 2;
                    pay.Padding = new Padding(300);

                    //pay.BorderStyle = BorderStyle.FixedSingle;

                    /*
                    Label background = new Label();
                        background.BackColor = Color.FromArgb(255, 255, 255);
                    imgX = (page.Width.Value < page.Height.Value) ? (int) Math.Round(page.Width.Value / (page.Height.Value / pay.Height)) : (int)Math.Round(page.Width.Value / (page.Width.Value / pay.Height));
                    imgY = (page.Width.Value > page.Height.Value) ? (int) Math.Round(page.Height.Value / (page.Height.Value / pay.Height)) : (int)Math.Round(page.Height.Value / (page.Width.Value / pay.Height));
                    backtangle = (imgX < imgY)? new Rectangle((int)Math.Round((pay.Height - imgX) / 2.0), 0, imgX, imgY):new Rectangle(0, (int)Math.Round((pay.Width - imgY) / 2.0), imgX, imgY);

                    background.Paint += new PaintEventHandler(onDrawPage);*/

                    pay.ResumeLayout();
                    view.Controls.Add(pay);
                }
            }

            pageIndic.Bounds = new Rectangle(0, 0, 40, 40);
            pageIndic.Text = $"1/{pageAmount}";
            pageIndic.BorderStyle = BorderStyle.FixedSingle;
            pageIndic.BackColor = Color.FromArgb(100, 100, 100);
            pageIndic.ForeColor = Color.FromArgb(200, 200, 200);
            pageIndic.Font = new Font("Verdana", 10F, FontStyle.Bold, GraphicsUnit.World, 0);
            pageIndic.TextAlign = ContentAlignment.MiddleCenter;
            pageIndic.Visible = pageIndicVisible;

            //loadBar.Dispose();

            AutoScroll = true;
            //this.BorderStyle = BorderStyle.FixedSingle;
            view.SizeChanged += new EventHandler(onSizeChange);
            view.Scroll += new ScrollEventHandler(onScroll);
            view.MouseWheel += new MouseEventHandler(onScroll);
        }
        /// <summary>
        /// Returns the Amount of Pages in the loaded Document
        /// </summary>
        /// <returns></returns>
        public int getPageAmount()
        {
            return pageAmount;
        }
        /// <summary>
        /// Returns the Page the scroller is on right now
        /// </summary>
        /// <returns></returns>
        public double getPageAt()
        {
            return pageAt;
        }
        /// <summary>
        /// Returns, if the page indicator is visible
        /// </summary>
        /// <returns></returns>
        public bool isPageIndicVisible()
        {
            return pageIndicVisible;
        }
        /// <summary>
        /// Hide or Show the Page Indicator
        /// </summary>
        /// <param name="val">Visible</param>
        public void setPageIndicVisible(bool val)
        {
            pageIndicVisible = val;
            pageIndic.Visible = val;
        }
        public void setPage(double at, int of)
        {
            pageIndic.Text = (at < 10 && of < 100) ? $"{at}/{of}" : $"{at}\n/{of}";
        }
        public void setPage()
        {
            setPage(pageAt, pageAmount);
        }
        /// <summary>
        /// Determine, if it only shows one page or multiple
        /// </summary>
        /// <param name="onePage"></param>
        public void setViewerMode(bool onePage)
        {
            noViewerMode = onePage;
        }
        public bool getViewerMode()
        {
            return noViewerMode;
        }
        #endregion
        private void onDispose(object sender, EventArgs e)
        {
            view.Controls.Clear();
            this.Controls.Clear();
        }
        private void onSizeChange(object sender, EventArgs e)
        {

        }
        private void onScroll(object sender, EventArgs e)
        {
            pageAt = (int)Math.Round((((double)view.AutoScrollPosition.Y * -1) / nowAt) * pageAmount) + 1;
            //Console.WriteLine($"{view.AutoScrollPosition.Y * -1}/{nowAt}");
            setPage();
        }
        private void onDrawPage(object sender, PaintEventArgs e)
        {
            int number = int.Parse(((Control)sender).Name) - 1;

            //MessageBox.Show(((Control)sender).Name+ ", " + number + ", "+ thumbnails.Count() + "->AD is " + AllDone);

            e.Graphics.DrawImage(thumbnails[number], thumbSquares[number]);
        }

        private Label pageToControl(PdfPage page, int i)
        {
            PdfDocument t = new PdfDocument();
            t.AddPage(page); t.Save(@"C:\Temp\PDFPagey.pdf"); t.Close(); t.Dispose();
            //PDFEditson.Wait(200);
            XPdfForm pdfPagey = XPdfForm.FromFile(@"C:\Temp\PDFPagey.pdf");
            //pdfPagey.AddPage(page);
            //pdfPagey.Pages.RemoveAt(0);
            PdfDocument output = new PdfDocument();
            output.AddPage();
            output.Pages[0].Size = PdfSharp.PageSize.A3;

            double width = output.Pages[0].Width;
            double height = output.Pages[0].Height;
            XGraphics gfx = XGraphics.FromPdfPage(output.Pages[0]);
            XRect box = new XRect(0, 0, width, height);
            gfx.DrawImage(pdfPagey, 0, 0, width, height);

            output.Save($@"C:\Temp\PDFPagey{i}.pdf");
            output.Close();
            output.Dispose();
            pdfPagey.Dispose();


            Label background = new Label();
            background.BackColor = Color.FromArgb(255, 255, 255);
            background.BorderStyle = BorderStyle.FixedSingle;
            backtangle = (imgX < imgY) ?
                new Rectangle(Convert.ToInt32(((this.Width - 40) - imgX) / 2.0), 0, Convert.ToInt32(imgX), Convert.ToInt32(imgY)) :
                new Rectangle(0, Convert.ToInt32(((this.Width - 40) - imgY) / 2.0), Convert.ToInt32(imgX), Convert.ToInt32(imgY));
            background.Name = "";
            background.Name = $"{i}";
            thumbnails.Add(getImg($@"C:\Temp\PDFPagey{i}.pdf"));

            background.Paint += new PaintEventHandler(onDrawPage);

            /*PictureBox pic = new PictureBox();
            pic.Image = getImg($@"C:\Temp\PDFPagey{i}.pdf", i, p);
            pic.SizeMode = PictureBoxSizeMode.Zoom;
            pic.Paint += new PaintEventHandler(onDrawPage);*/

            return background;
        }
        private Image getImg(string p)
        {
            string OUT = $@"C:\Temp\converted{randString(5)}.jpg";


            PDFLibNet64.PDFWrapper wrapper = new PDFLibNet64.PDFWrapper();

            wrapper.LoadPDF(p);
            wrapper.ExportJpg(OUT, 100);

            #region Hall of Shame PDF2Image Conversion
            //GhostscriptSharp.GhostscriptWrapper.GeneratePageThumb(p, OUT, 1, 100, 100);

            /*MagickNET.Initialize();
            var image = new MagickImage();
            image.Density = new Density(100);
            image.RenderingIntent = RenderingIntent.Absolute;
            image.Read(p);
            image.Format = MagickFormat.Png;
            image.BackgroundColor = MagickColor.FromRgba(255, 255, 255, 0);
            image.ColorType = ColorType.TrueColorAlpha;
            image.Interpolate = PixelInterpolateMethod.Bilinear;
            image.Write(OUT);*/


            /*PDFConvert c = new PDFConvert();
            c.RenderingThreads = -1;
            c.TextAlphaBit = -1;
            c.TextAlphaBit = -1;
            c.FitPage = true;
            c.JPEGQuality = 80;
            c.OutputFormat = "jpeg";
            c.OutputToMultipleFile = false;
            c.FirstPageToConvert = -1;
            c.LastPageToConvert = -1;
            c.ResolutionX = 130;
            c.ResolutionY = 130;
            c.Convert(p, OUT);*/
            #endregion

            for (; ; )
                if (!wrapper.IsJpgBusy)
                    break;
            wrapper.Dispose();

            //MessageBox.Show($"Bild fertig für {p}");
            //((ProgressBar)loadBar.Controls[0]).Increment(1);

            return Image.FromFile(OUT, true);
        }

        public string randString(int chars)
        {
            string OUT = "";
            Random a = new Random();

            for (int i = 0; i < chars; i++)
            {
                char insert;
                for (; ; )
                {
                    insert = (char)((a.Next() % 94) + 33);
                    if (insert == 92  ||
                        insert == '/' ||
                        insert == ':' ||
                        insert == '*' ||
                        insert == '?' ||
                        insert == 34  ||
                        insert == '<' ||
                        insert == '>' ||
                        insert == '|' ||
                        insert == '%')
                        continue;
                    else break;
                }
                //Console.WriteLine((int)insert);
                OUT = OUT + insert;
            }

            //Console.WriteLine(OUT);

            return OUT;
        }
        private void Bar()
        {
            loadBar = new LoadingBar(pageAmount);
            loadBar.Show();
            ((ProgressBar)loadBar.Controls[0]).Increment(1);
            loadBar.Location = this.DisplayRectangle.Location;
            loadBar.Size = new Size(386, 26);
        }
    }
}
