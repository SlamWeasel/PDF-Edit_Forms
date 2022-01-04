using PDF_Edit_Forms.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PDF_Edit_Forms.Forms
{
    /// <summary>
    /// Vordesigntes Fenster zum zusammenfügen von Dateien
    /// </summary>
    class CombineFilesForm : StyleForm
    {
        private List<string> Documents = new List<string>();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));

        public CombineFilesForm(string windowTitle, string windowName) : base(windowTitle, windowName)
        {
            
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = CreateGraphics();
            /*
            PointF[] ps = new PointF[] { new PointF(20, 20), new PointF(20, 200), new PointF(200, 20) };
            Brush b = new TextureBrush(Image.FromFile(@"H:\Ablage\55555555_wallpaper smallu smol.jpg"));

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
            g.FillPolygon(b, ps);
            */
            


            base.OnPaint(e);
        }
        public void addDocument(string path)
        {
            Documents.Add(path);
        }
        public void addDocuments(string[] paths)
        {
            foreach(string path in paths)
                Documents.Add(path);
        }

        public void drawDocuments()
        {

        }
    }
}
