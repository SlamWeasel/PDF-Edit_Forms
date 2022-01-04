using PDFLibNet64;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace PDF_Edit_Forms.Util
{
#pragma warning disable IDE1006 // Benennungsstile
    class PdfActions
    {
        const string TEMP = @"C:\Temp\PDFEditson.pdf";

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
        public static PdfDocument removePages(PdfDocument doc, List<int> pageNumbers, PDFWrapper wrapRemove)
        {
            PdfDocument reduced = doc;

            doc.Close();
            Utils.freeFile(TEMP);
            while (Utils.IsFileLocked(TEMP))
                Console.WriteLine("Datei lädt noch");
            doc.Dispose();

            int buffer = 0;
            foreach(int p in pageNumbers)
            {
                reduced.Pages.RemoveAt(p - buffer);
                buffer++;
            }

            tempSaveFile(reduced, PDFOperations.RemovePages);

            return reduced;
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
                doc.Save(TEMP);
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
        public static void saveFileTo(PdfDocument doc, string path, int operation)
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
                    MessageBox.Show(
                        "Beim Versuch die Dateien zusammenzufügen ist ein Fehler aufgetreten, bitte versuchen Sie es erneut.\n" +
                        "Sollte der Fehler weiterhin bestehen, machen Sie einen Screenshot dieses Fehlers und senden sie ihn an die EDV-Abteilung:\n\n\n" +
                        w.Message + "\n" + w.StackTrace,
                        "Dateifehler",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error); ;
                    break;
                case PDFOperations.RemovePages:
                    MessageBox.Show(
                        "Beim Versuch die Seiten zu entfernen ist ein Fehler aufgetreten, bitte versuchen Sie es erneut.\n" +
                        "Sollte der Fehler weiterhin bestehen, machen Sie einen Screenshot dieses Fehlers und senden sie ihn an die EDV-Abteilung:\n\n\n" +
                        w.Message + "\n" + w.StackTrace,
                        "Dateifehler",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    break;
                case PDFOperations.SplitDocument:
                    MessageBox.Show(
                        "Beim Versuch einen der Teile zu speichern ist ein Fehler aufgetreten, bitte versuchen Sie es erneut.\n" +
                        "Sollte der Fehler weiterhin bestehen, machen Sie einen Screenshot dieses Fehlers und senden sie ihn an die EDV-Abteilung:\n\n\n" +
                        w.Message + "\n" + w.StackTrace,
                        "Dateifehler",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    break;
                default:
                    MessageBox.Show(
                        "Es ist ein unerwarteter Fehler aufgetreten.\n" +
                        "Sollte der Fehler weiterhin bestehen, machen Sie einen Screenshot dieses Fehlers und senden sie ihn an die EDV-Abteilung:\n\n\n" +
                        w.Message + "\n" + w.StackTrace,
                        "Dateifehler",
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
