using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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
        public string StampAuthor { get; set; }
        public string CheckedBy { get; set; }
        public string RateOfInspection { get; set; }
        public string Format { get; set; }
        public int SheetsNumber { get; set; }
        public DrawingType DrawingType { get; set; }
        //public List<DrawingSheet> Sheets { get; set; }
        public bool IsAssemblyDrawing { get; set; }


        
        public bool IsGoodName =>
            Path.Split('\\').Last()[..^4] == Designation + " - " + Name;

        public override string ToString()
        {
            return Designation + (IsAssemblyDrawing && DrawingType == DrawingType.Drawing ? " СБ" : "") + " - " + Name;
        }

        private void AnalyzeRoot(Root xmlDoc)
        {
            var docSection = xmlDoc.Product.Documents
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
            }

            Name = name ?? Name;

            var designation = docSection.Properties
                    .FirstOrDefault(p => p.Id == "marking")?.Properties
                    .FirstOrDefault(p => p.Id == "base")?.Value;

            Designation = designation ?? Designation;

            var stampAuthor = docSection.Properties
                .FirstOrDefault(p => p.Id == "stampAuthor")?.Value;

            StampAuthor = stampAuthor ?? StampAuthor;

            CheckedBy = docSection.Properties.FirstOrDefault(p => p.Id == "checkedBy")?.Value ?? CheckedBy;

            RateOfInspection = docSection.Properties.FirstOrDefault(p => p.Id == "rateOfInspection")?.Value 
                               ?? RateOfInspection;
            Format = docSection.Properties.FirstOrDefault(p => p.Id == "format")?.Value ?? Format;

            var sheetsNumber = docSection.Properties.FirstOrDefault(p => p.Id == "sheetsNumber")?.Value;
            
            if (sheetsNumber != null)
            {
                SheetsNumber = Convert.ToInt32(sheetsNumber);
            }

        }

        public DrawingFileInfo(string path)
        {
            Name = string.Empty;
            Designation = string.Empty;
            //Sheets = new List<DrawingSheet>();
            StampAuthor = string.Empty;
            CheckedBy = string.Empty;
            RateOfInspection = string.Empty;
            Format = string.Empty;
            SheetsNumber = 0;

            Path = path;
            Root? xmlDoc;

            using (var packageStream = new MemoryStream())
            {

                using (FileStream fs = new(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    fs.CopyTo(packageStream);
                }

                packageStream.Position = 0;

                using (var z = new ZipArchive(packageStream, ZipArchiveMode.Read))
                {
                    var infoEntry = z.GetEntry("MetaProductInfo");

                    try
                    {
                        XmlSerializer formatter = new(typeof(Root));
                        xmlDoc = (Root?) formatter.Deserialize(infoEntry.Open());
                    }
                    catch
                    {
                        return;
                    }
                }

            }

            if (xmlDoc == null) return;

            AnalyzeRoot(xmlDoc);
        }

        public void RenameInZipFile1()
        {
            bool isCompleted;
            bool Opened = false;

            //Root? _doc20 = new();

            using (var packageStream = new MemoryStream())
            {

                using (FileStream fs = new(Path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    fs.CopyTo(packageStream);
                }

                    

                packageStream.Position = 0;

                using (var zipPackage = new ZipArchive(packageStream, ZipArchiveMode.Update))
                {

                    StringBuilder document;

                    var infoEntry = zipPackage.GetEntry("MetaProductInfo");

                    if (infoEntry == null) return;


                    //XmlSerializer formatter = new(typeof(Root));
                    //_doc20 = (Root?)formatter.Deserialize(infoEntry.Open());

                    XDocument xDoc;
                    using (var reader = new StreamReader(infoEntry.Open()))
                    {

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

                        //var fullFileName = doc

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


                    }

                    infoEntry.Delete();
                    infoEntry = zipPackage.CreateEntry("MetaProductInfo");
                    using (var writer = new StreamWriter(infoEntry.Open()))
                    {
                        writer.Write(xDoc.ToString());
                    }
                }

                //packageStream.ToArray();

                //zipPackage.CreateEntry();

            }
        }

        public void RenameInZipFile()
        {

            using (var archive = ZipFile.Open(Path, ZipArchiveMode.Update))
            {

                var infoEntry = archive.GetEntry("MetaProductInfo");

                if (infoEntry == null) return;

                XDocument xDoc;

                using (var reader = new StreamReader(infoEntry.Open()))
                {
                    xDoc = XDocument.Load(reader);
                }

                if (xDoc == null) return;


                var documentElements = xDoc
                    .Element("document")
                    ?.Element("product")
                    ?.Elements("document")
                    .ToList();

                if (documentElements == null) return;


                var isAssemblyDrawing = documentElements
                    .Elements("property")
                    .FirstOrDefault(e => e.Attribute("id")?.Value == "marking")
                    ?.Elements("property")
                    .FirstOrDefault(e => e.Attribute("id")?.Value == "documentNumber")
                    ?.Attribute("value")
                    ?.Value == "СБ";

                var name = documentElements
                    .Elements("property")
                    .FirstOrDefault(e => e.Attribute("id")?.Value == "name")
                    ?.Attribute("value")
                    ?.Value;

                if (name == null) return;

                name = name.Replace("@/", ". ");
                if (isAssemblyDrawing)
                {
                    name = name[..^18];
                }

                name = Regex.Replace(name, @"[\/?:*""><|]+", "", RegexOptions.Compiled);

                var designation = documentElements
                    ?.Elements("property")
                    .FirstOrDefault(e => e.Attribute("id")?.Value == "marking")
                    ?.Elements("property")
                    .FirstOrDefault(e => e.Attribute("id")?.Value == "base")
                    ?.Attribute("value")
                    ?.Value;

                if (designation == null) return;

                var infObjects = xDoc.Element("document")
                    ?.Element("product")
                    ?.Elements("infObject")
                    .Select(i => i.Attribute("id")?.Value)
                    .ToList();

                var fullFileName = documentElements
                    .FirstOrDefault(d => d.Attribute("prodCopy") == null)
                    ?.Elements("property")
                    .FirstOrDefault(e => e.Attribute("id")?.Value == "fullFileName")
                    ?.Attribute("value")?.Value;


                if (fullFileName == null || fullFileName == ToString()) return;


                documentElements
                    .First(d => d.Attribute("prodCopy") == null)
                    .Elements("property")
                    .First(e => e.Attribute("id").Value == "fullFileName")
                    .Attribute("value").Value = Path + @"\" + ToString() + ".cdw";
                

                infoEntry.Delete();

                infoEntry = archive.CreateEntry("MetaProductInfo");

                using (var writer = new StreamWriter(infoEntry.Open()))
                {
                    writer.Write(xDoc.ToString());
                }
            }
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


        //public event PropertyChangedEventHandler? PropertyChanged;

        //[NotifyPropertyChangedInvocator]
        //protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}
    }
}