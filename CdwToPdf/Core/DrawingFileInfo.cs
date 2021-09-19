using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Xml.Linq;
using System.Xml.Serialization;
using CdwToPdf.Annotations;
using CdwToPdf.Core.Analyzer;
using CdwToPdf.Core.ver20;
using CdwToPdf.Enums;

namespace CdwToPdf.Core
{
    public class DrawingFileInfo //:INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Designation { get; set; }
        public string Path { get; set; }
        public DrawingType DrawingType {  get; set; }
        public List<DrawingSheet> Sheets { get; set; }
        public bool IsAssemblyDrawing { get; set; }


        
        public bool IsGoodName =>
            Path.Split('\\').Last()[..^4] == Designation + " - " + Name;

        public override string ToString()
        {
            return Designation + (IsAssemblyDrawing && DrawingType == DrawingType.Drawing ? " СБ" : "") + " - " + Name;
        }

        private void AnalyzeRoot(Root doc)
        {
            var docSection = doc.Product.Documents
                    .FirstOrDefault(d => d.Properties.Exists(p => p.Id == "marking"));

            if (docSection == null) return;

            var isAssemblyDrawing = docSection.Properties
                .FirstOrDefault(p => p.Id == "marking")?.Properties
                .FirstOrDefault(p=> p.Id == "documentNumber")?.Value == "СБ";

            IsAssemblyDrawing = isAssemblyDrawing;

            var name = docSection.Properties
                .FirstOrDefault(p => p.Id == "name")?.Value;
                

            if (name != null)
            {
                name = name.Replace("@/", ". ");
                if (isAssemblyDrawing && name[^16..] == "Сборочный чертеж")
                {
                    name = name[..^18];
                }

                name = Regex.Replace(name, @"[\/?:*""><|]+", "", RegexOptions.Compiled);
                Name = name;
            }



            var designation = docSection.Properties
                    .FirstOrDefault(p => p.Id == "marking")?.Properties
                    .FirstOrDefault(p => p.Id == "base")?.Value;


            if (designation != null)
            {
                Designation = designation;
            }

        }

        public DrawingFileInfo(string path)
        {

            Root? doc20;

            Path = path;

            using FileStream fs = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);

            var z = new ZFile();
            if (!z.IsZip(fs)) return;

            z.ExtractFileToMemoryStream(fs, "MetaProductInfo");

            if (z.OutputMemStream == null) return;

            var ms = z.OutputMemStream;

            try
            {
                ms.Position = 0;
                XmlSerializer formatter = new(typeof(Root));
                doc20 = (Root?)formatter.Deserialize(ms);
            }
            catch
            {
                return;
            }

            if (doc20 == null) return;

            try
            {
                AnalyzeRoot(doc20);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public void RenameInZipFile()
        {
            bool isCompleted;
            bool Opened = false;

            //Root? _doc20 = new();

            using var packageStream = new MemoryStream();

            using FileStream fs = new(Path, FileMode.Open, FileAccess.Read, FileShare.Read);

            fs.CopyTo(packageStream);
            
            packageStream.Position = 0;

            using var zipPackage = new ZipArchive(packageStream, ZipArchiveMode.Update);

            var infoEntry = zipPackage.GetEntry("MetaProductInfo");

            if (infoEntry == null) return;

            
            //XmlSerializer formatter = new(typeof(Root));
            //_doc20 = (Root?)formatter.Deserialize(infoEntry.Open());

            XDocument xDoc;
            using var reader = new StreamReader(infoEntry.Open());

            xDoc = XDocument.Load(infoEntry.Open());

            if (xDoc == null) return;


            var docSection = xDoc
                .Element("document")
                ?.Element("product")
                ?.Elements("document");

            if (docSection == null) return;

            var isAssemblyDrawing = docSection
                .Elements("property")
                .FirstOrDefault(e => e.Attribute("id").Value == "marking")
                ?.Elements("property")
                .FirstOrDefault(e => e.Attribute("id").Value == "documentNumber")
                ?.Attribute("value")
                ?.Value == "СБ";

            var name = docSection
                .Elements("property")
                .FirstOrDefault(e => e.Attribute("id").Value == "name")
                ?.Attribute("value")
                ?.Value;

            if (name == null) return;
            
            name = name.Replace("@/", ". ");
            if (isAssemblyDrawing)
            {
                name = name[..^18];
            }

            name = Regex.Replace(name, @"[\/?:*""><|]+", "", RegexOptions.Compiled);

            var designation = docSection
                ?.Elements("property")
                .FirstOrDefault(e => e.Attribute("id").Value == "marking")
                ?.Elements("property")
                .FirstOrDefault(e => e.Attribute("id").Value == "base")
                ?.Attribute("value")
                ?.Value;

            if (designation == null) return;

            if (xDoc.Element("document")
                .Element("product")
                .Elements("document")
                .Elements("property")
                .FirstOrDefault(e => e.Attribute("id").Value == "fullFileName")
                .Attribute("value").Value != ToString())
            {
                xDoc.Element("document")
                    .Element("product")
                    .Elements("document")
                    .Elements("property")
                    .FirstOrDefault(e => e.Attribute("id").Value == "fullFileName")
                    .Attribute("value").Value = ToString();
            }


            using var writer = new StreamWriter(infoEntry.Open());
            writer.Write(xDoc.ToString());

            packageStream.ToArray();

            //zipPackage.CreateEntry();

            
        }

        public void RenameFile()
        {
            bool isCompleted;
            bool Opened = false;

            //Root? _doc20 = new();

            using var packageStream = new MemoryStream();

            using FileStream fs = new(Path, FileMode.Open, FileAccess.Read, FileShare.Read);

            fs.CopyTo(packageStream);

            packageStream.Position = 0;

            using var zipPackage = new ZipArchive(packageStream, ZipArchiveMode.Update);

            var infoEntry = zipPackage.GetEntry("MetaProductInfo");

            if (infoEntry == null) return;


            //XmlSerializer formatter = new(typeof(Root));
            //_doc20 = (Root?)formatter.Deserialize(infoEntry.Open());

            XDocument xDoc;
            using var reader = new StreamReader(infoEntry.Open());

            xDoc = XDocument.Load(reader);

            if (xDoc == null) return;


            var docSection = xDoc
                .Element("document")
                ?.Element("product")
                ?.Elements("document");

            if (docSection == null) return;

            var isAssemblyDrawing = docSection
                .Elements("property")
                .FirstOrDefault(e => e.Attribute("id").Value == "marking")
                ?.Elements("property")
                .FirstOrDefault(e => e.Attribute("id").Value == "documentNumber")
                ?.Attribute("value")
                ?.Value == "СБ";

            var name = docSection
                .Elements("property")
                .FirstOrDefault(e => e.Attribute("id").Value == "name")
                ?.Attribute("value")
                ?.Value;

            if (name == null) return;

            name = name.Replace("@/", ". ");
            if (isAssemblyDrawing)
            {
                name = name[..^18];
            }

            name = Regex.Replace(name, @"[\/?:*""><|]+", "", RegexOptions.Compiled);

            var designation = docSection
                ?.Elements("property")
                .FirstOrDefault(e => e.Attribute("id").Value == "marking")
                ?.Elements("property")
                .FirstOrDefault(e => e.Attribute("id").Value == "base")
                ?.Attribute("value")
                ?.Value;

            if (designation == null) return;

            if (xDoc.Element("document")
                .Element("product")
                .Elements("document")
                .Elements("property")
                .FirstOrDefault(e => e.Attribute("id").Value == "fullFileName")
                .Attribute("value").Value != ToString())
            {
                xDoc.Element("document")
                    .Element("product")
                    .Elements("document")
                    .Elements("property")
                    .FirstOrDefault(e => e.Attribute("id").Value == "fullFileName")
                    .Attribute("value").Value = ToString();
            }


            using var writer = new StreamWriter(infoEntry.Open());
            writer.Write(xDoc.ToString());

            packageStream.ToArray();

            //zipPackage.CreateEntry();


        }

        public DrawingFileInfo()
        {

        }

        //public event PropertyChangedEventHandler? PropertyChanged;

        //[NotifyPropertyChangedInvocator]
        //protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}
    }
}