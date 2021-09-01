using CdwToPdf.Core;
using CdwToPdf.Enums;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml.Linq;


namespace CdwToPdf.Analyzer
{
    public class CdwAnalyzer
    {
        private XDocument _xDoc;
        private string str;

        public bool Opened { get; private set; }
        public bool IsCompleted { get; private set; }
        public DrawingFileInfo DrawingFileInfo { get; private set; } = new DrawingFileInfo();



        public CdwAnalyzer(Stream fileStream, string path)
        {
            IsCompleted = false;
            DrawingFileInfo.Path = path;
            var z = new ZFile();
            if (!z.IsZip(fileStream)) return;

            z.ExtractFileToMemoryStream(fileStream, "FileInfo");

            IsCompleted = false;

            LoadFromMemoryStream(z.OutputMemStream, true);
            if (!Opened) return;
            try
            {
                RunParseCdw_FileInfo();
            }
            catch
            {
                Opened = false;
                IsCompleted = false;
            }
            z.ExtractFileToMemoryStream(fileStream, "MetaProductInfo");
            LoadFromMemoryStream(z.OutputMemStream, false);
            if (!Opened) return;
            try
            {
                RunParseCdw_MetaProductInfo();
            }
            catch
            {
                Opened = false;
                IsCompleted = false;
            }
        }


        private void RunParseCdw_MetaProductInfo()
        {
            if (_xDoc == null) return;

            if (!IsCompleted) return;

            var docSection = _xDoc
                .Element("document")
                ?.Element("product")
                ?.Elements("document");


            var isAssemblyDrawing = docSection
                ?.Elements("property")
                .FirstOrDefault(e => e.Attribute("id").Value == "marking")
                ?.Elements("property")
                .FirstOrDefault(e => e.Attribute("id").Value == "documentNumber")
                ?.Attribute("value")
                ?.Value == "СБ";

            DrawingFileInfo.IsAssemblyDrawing = isAssemblyDrawing;

            var name = docSection
                ?.Elements("property")
                .FirstOrDefault(e => e.Attribute("id").Value == "name")
                ?.Attribute("value")
                ?.Value;

            if (name != null)
            {
                name = name.Replace("@/", ". ");
                if (isAssemblyDrawing)
                {
                    name = name[..^18];
                }

                name = Regex.Replace(name, @"[\/?:*""><|]+", "", RegexOptions.Compiled);
                DrawingFileInfo.Name = name;
            }



            var designation = docSection
                ?.Elements("property")
                .FirstOrDefault(e => e.Attribute("id").Value == "marking")
                ?.Elements("property")
                .FirstOrDefault(e => e.Attribute("id").Value == "base")
                ?.Attribute("value")
                ?.Value;

            if (designation != null)
            {
                DrawingFileInfo.Designation = designation;
            }


            IsCompleted = true;
        }

        private void RunParseCdw_FileInfo()
        {
            if (!str.Any())
                return;
            const string appName = "AppVersion=";

            var version = str.Split('\n')
                .FirstOrDefault(st => st.Contains(appName))
                ?[appName.Length..];

            if (version == null)
            {
                MessageBox.Show($"File {DrawingFileInfo.Path} version undefined");
                return;
            }

            var ver = Convert.ToDouble(version.Split('_').LastOrDefault(), CultureInfo.InvariantCulture);

            if (ver < 19)
            {
                MessageBox.Show($"File {DrawingFileInfo.Path} version undo 19");
                return;
            }


            var t = str.Split('\n')
                .FirstOrDefault(st => st.Contains("FileType="))
                ?.Last()
                .ToString();

            if (t == null) return;

            DrawingFileInfo.DrawingType = (DrawingType)Convert.ToInt32(t);

            IsCompleted = true;
        }

        private void LoadFromMemoryStream(Stream ms, bool isString)
        {
            Opened = false;
            try
            {
                var p = ms.Position;
                ms.Position = 0;
                var reader = new StreamReader(ms);
                var s = reader.ReadToEnd();
                ms.Position = p;
                if (isString)
                {
                    str = s;
                }
                else
                {
                    _xDoc = XDocument.Parse(s);
                }
                Opened = true;
            }
            catch
            {
                Opened = false;
                IsCompleted = false;
            }
        }
    }
}
