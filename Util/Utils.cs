using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace PDF_Edit_Forms.Util
{
    public abstract class Utils
    {
        public static List<char> FileNameBlacklist = new List<char>(){'\\','\"','/',':','*','?','<','>','|','%' };
        private static Random a = new Random();

        /// <summary>
        /// Generiert einen zufälligen String aus Zeichen, die in dem Namen einer Datei stehen dürfen
        /// </summary>
        /// <param name="length">Länge des strings</param>
        /// <returns></returns>
        public static string randString(int length)
        {
            string OUT = "";

            for (int i = 0; i < length; i++)
            {
                char insert;
                for (; ; )
                {
                    insert = (char)a.Next(33,127);
                    if (FileNameBlacklist.Contains(insert))
                        continue;
                    else break;
                }
                OUT += insert;
            }

            return OUT;
        }

        /// <summary>
        /// Versucht eine Datie freizugeben, die möglicherweise noch von einem anderen Handle geöffnet ist
        /// </summary>
        /// <param name="path">Voller Pfad der Datei</param>
        public static void freeFile(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    FileStream myFile = File.Create(path);
                }
                else
                {
                    using (FileStream myFile = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
                        if(myFile.Length > 0)
                        {
                            myFile.Close();
                            myFile.Flush();
                            myFile.Dispose();
                        }
                }
            }
            catch(Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// Versucht, im C:\Temp Ordner alle vom Programm erstellten Dateien zu löschen
        /// </summary>
        public static void CleanUp()
        {
            DirectoryInfo temp = new DirectoryInfo(@"C:\Temp");
            foreach (FileInfo f in temp.GetFiles())
                if (f.Name.Contains("PDFPagey") || (f.Name.Contains("converted") && f.Extension.Contains("jpg")))
                    try
                    {
                        File.Delete(f.FullName);
                    }
                    catch (Exception) { }
        }

        /// <summary>
        /// Gibt <c>true</c> zurück wenn die Datei benutzt wird und <c>false</c> wenn sie frei ist.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsFileLocked(string path)
        {
            FileInfo file = new FileInfo(path);

            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                    stream.Close();
            }
            catch (IOException)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Wandelt das Dictionary der Einstellungen in einen String um, der dann in der settings.txt Datei abgespeichert werden kann
        /// </summary>
        /// <param name="sets">Einstellungen in Dictionary-Form</param>
        /// <returns>Einen String der die Einstellungen repräsentiert</returns>
        public static string settingsToString(Dictionary<string, string> sets)
        {
            string OUT = "";

            foreach (KeyValuePair<string, string> kv in sets)
                OUT += kv.Key + ";" + kv.Value + "\n";

            return OUT;
        }

        /// <summary>
        /// Gibt die Größe der Datei die am angegebenen Ort liegt als string zurück
        /// </summary>
        /// <param name="file">Voller Pfad der Datei</param>
        /// <returns>Größe der Datei als lesbarer <c>string</c></returns>
        public static string getSizeAsString(string file)
        {
            string size = "0 Byte";

            try
            {
                FileInfo fi = new FileInfo(file);
                if (fi.Length < 1500)
                    size = $"{fi.Length} Byte";
                else if (fi.Length < 1500 * 1024)
                    size = $"{Math.Round(fi.Length / 1024.0, 2)} KB";
                else if (fi.Length < 1500 * 1024 * 1024)
                    size = $"{Math.Round(fi.Length / 1024.0 / 1024.0, 2)} MB";
                else
                    size = $"{Math.Round(fi.Length / 1024.0 / 1024.0 / 1024.0, 2)} GB";
            }
            catch (Exception) { }

            return size;
        }
    }
}
