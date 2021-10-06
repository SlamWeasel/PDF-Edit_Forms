using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PDF_Edit_Froms.Util
{
#pragma warning disable IDE1006 // Benennungsstile
    class PdfActions
    {
        /// <summary>
        /// Fügt alle Dokumente in dem Array zu einem zusammen
        /// </summary>
        /// <param name="paths"></param>
        public static void combineDocuments(string[] paths)
        {
            PdfDocument fused = new PdfDocument();

            foreach (string path in paths)
            {
                PdfDocument d = PdfReader.Open(path, PdfDocumentOpenMode.Import);
                foreach (PdfPage p in d.Pages)
                    fused.AddPage(p);
                d.Close();
            }

            tempSaveFile(fused, PDFOperations.CombineDocuments);
        }

        /// <summary>
        /// Entfernt die Liste der Seiten von dem Dokument
        /// </summary>
        /// <param name="pageNumbers">List of the pages, that get removed</param>
        /// <param name="ogPath">Pfad der Datei</param>
        public static void removePages(List<int> pageNumbers, string ogPath)
        {
            PdfDocument reduced = new PdfDocument();
            PdfDocument source = PdfReader.Open(ogPath);

            int i = 0;
            foreach(PdfPage p in source.Pages)
            {
                if (!pageNumbers.Contains(i))
                    reduced.AddPage(p);
                i++;
            }

            source.Close();
            tempSaveFile(reduced, PDFOperations.RemovePages);
        }

        /// <summary>
        /// Teilt die Datei in die angegebene Anzahl an Teile auf
        /// </summary>
        /// <param name="partAmount">Anzahl der Teile</param>
        public static void splitDocument(PdfDocument doc, int partAmount, string destination, string suffix)
        {
            int perPart = (doc.PageCount - (doc.PageCount % partAmount)) / partAmount,
                rest = doc.PageCount % partAmount,
                PartNumber = 1;

            for(int pages = 0; pages < doc.PageCount;)
            {
                PdfDocument p = new PdfDocument();

                for(int i = 0; i < perPart; i++)
                {
                    p.AddPage(doc.Pages[pages]);
                    pages++;
                }
                if (rest > 0)
                {
                    p.AddPage(doc.Pages[pages]);
                    pages++;
                    rest--;
                }

                saveFileTo(p, destination + suffix + " (" + PartNumber + ")", PDFOperations.SplitDocument);
                PartNumber++;
            }
        }
        /// <summary>
        /// Teilt die Datei in Teile auf, die die angegebene Anzahl an Seiten haben
        /// </summary>
        /// <param name="pageAmount">Seitenanzahl pro Teil</param>
        /// <param name="rest">Die Anzahl der Seiten im letzten Teil</param>
        public static void splitDocument(PdfDocument doc, int pageAmount, out int rest, string destination, string suffix)
        {
            int fullParts = (doc.PageCount - (doc.PageCount % pageAmount)) / pageAmount,
                PartNumber = 1;
                rest = doc.PageCount % pageAmount;

            for(int pages = 0; pages < doc.PageCount;)
            {
                PdfDocument p = new PdfDocument();

                for(int i = 0; i < fullParts && pages < doc.PageCount; i++)
                {
                    p.AddPage(doc.Pages[pages]);
                    pages++;
                }

                saveFileTo(p, destination + suffix + " (" + PartNumber + ")", PDFOperations.SplitDocument);
                PartNumber++;
            }
        }
        /// <summary>
        /// Teilt die Datei in Teile auf, die jeweils die angegebene Dateigröße nicht überschreiten dürfen.
        /// </summary>
        /// <param name="partSize">Dateigröße in KiloByte</param>
        public static void splitDocument(PdfDocument doc, float partSize, string destination, string suffix)
        {

        }

        /// <summary>
        /// Versucht die Datei im Temp-Ordner als <c>PDFEditson</c> abzuspeichern, gibt eine Fehlermeldung der <paramref name="operation"/> entsprechend aus, falls es scheitert!
        /// </summary>
        /// <param name="doc">Dokument, das zwischengespeichert wird</param>
        /// <param name="operation">
        ///     <list type="number">
        ///         <listheader>Default</listheader>
        ///         <item>Dateien kombinieren/zusammenführen</item>
        ///         <item>Einzelne Seiten entfernen</item>
        ///         <item>Datei automatisch aufteilen</item>
        ///     </list>
        /// </param>
        private static void tempSaveFile(PdfDocument doc, int operation)
        {
            try
            {
                doc.Save(@"C:\Temp\PDFEditson.pdf");
            }
            catch (Exception w)
            {
                showErrorMessage(operation, w);
            }
        }
        /// <summary>
        /// Versucht die Datei abzuspeichern, gibt eine Fehlermeldung der <paramref name="operation"/> entsprechend aus, falls es scheitert!
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="path"></param>
        /// <param name="operation"></param>
        private static void saveFileTo(PdfDocument doc, string path, int operation)
        {
            try
            {
                doc.Save(path);
            }
            catch(Exception w)
            {
                showErrorMessage(operation, w);
            }
        }
        
        private static void showErrorMessage(int errorCase, Exception w)
        {
            switch (errorCase)
            {
                case PDFOperations.CombineDocuments:
                    MessageBox.Show(w.Message + "\n" + w.StackTrace,
                        "Beim Versuch die Dateien zusammenzufügen ist ein Fehler aufgetreten, bitte versuchen Sie es erneut.\n" +
                        "Sollte der Fehler weiterhin bestehen, machen Sie einen Screenshot dieses Fehlers und senden sie ihn an die EDV-Abteilung:\n",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    break;
                case PDFOperations.RemovePages:
                    MessageBox.Show(w.Message + "\n" + w.StackTrace,
                        "Beim Versuch die Seiten zu entfernen ist ein Fehler aufgetreten, bitte versuchen Sie es erneut.\n" +
                        "Sollte der Fehler weiterhin bestehen, machen Sie einen Screenshot dieses Fehlers und senden sie ihn an die EDV-Abteilung:\n",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    break;
                case PDFOperations.SplitDocument:
                    MessageBox.Show(w.Message + "\n" + w.StackTrace,
                        "Beim Versuch einen der Teile zu speichern ist ein Fehler aufgetreten, bitte versuchen Sie es erneut.\n" +
                        "Sollte der Fehler weiterhin bestehen, machen Sie einen Screenshot dieses Fehlers und senden sie ihn an die EDV-Abteilung:\n",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    break;
                default:
                    MessageBox.Show(w.Message + "\n" + w.StackTrace,
                        "Es ist ein unerwarteter Fehler aufgetreten.\n" +
                        "Sollte der Fehler weiterhin bestehen, machen Sie einen Screenshot dieses Fehlers und senden sie ihn an die EDV-Abteilung:\n",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    break;
            }
        }
    }

    class PDFOperations
    {
        public const int 
            Default = 0,
            CombineDocuments = 1,
            RemovePages = 2,
            SplitDocument = 3;
    }
}
