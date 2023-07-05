using System.Text.RegularExpressions;
using System.Xml.Serialization;
using CdwHelper.Core.Analyzers.Ver20.XmlModel;
using CdwHelper.Core.Enums;
using CdwHelper.Core.Interfaces;
using CdwHelper.Core.Models;

namespace CdwHelper.Core.Analyzers.Ver20;

internal class RootPartAnalyzer20 : IRootPartAnalyzer
{
    public KompasDocument AnalyzeXml(Stream xml, DocType type)
    {
        var root = Deserilyze(xml);

        KompasDocument? doc = type switch
        {
            DocType.Drawing or DocType.Specification => Analyze2D19(root),
            DocType.Part3D or DocType.Assembly3D => Analyze3D19(root),
            _ => throw new Exception($"Unsupported drawing type {type}."),
        }
            ?? throw new Exception($"Couldn't analyze root part.");

        return doc;
    }

    private Root19 Deserilyze(Stream xml)
    {
        try
        {
            XmlSerializer formatter = new(typeof(Root19));
            return formatter.Deserialize(xml) as Root19
                ?? throw new Exception("Couldn't deserialize entry.");
        }
        catch
        {
            throw new Exception("Couldn't deserialize entry.");
        }
    }

    private static KompasDocument? Analyze2D19(Root19 xmlDoc)
    {
        var docSection = xmlDoc.Product.Documents
                .FirstOrDefault(d => !d.ProdCopy && d.Properties.Exists(p => p.Id == "marking"));

        if (docSection == null)
            return null;

        var doc = new KompasDocument
        {
            IsAssemblyDrawing = docSection.Properties
                .FirstOrDefault(p => p.Id == "marking")?.Properties
                .FirstOrDefault(p => p.Id == "documentNumber")?.Value == "СБ",
            Marking = docSection.Properties
                .FirstOrDefault(p => p.Id == "marking")?.Properties
                .FirstOrDefault(p => p.Id == "base")?.Value
                ?? string.Empty,
            StampAuthor = docSection.Properties
                .FirstOrDefault(p => p.Id == "stampAuthor")?.Value
                ?? string.Empty,
            CheckedBy = docSection.Properties
                .FirstOrDefault(p => p.Id == "checkedBy")?.Value
                ?? string.Empty,
            RateOfInspection = docSection.Properties
                .FirstOrDefault(p => p.Id == "rateOfInspection")?.Value
                ?? string.Empty,
            Format = docSection.Properties
                .FirstOrDefault(p => p.Id == "format")?.Value
                ?? string.Empty,
            SheetsNumber = docSection.Properties
                .FirstOrDefault(p => p.Id == "sheetsNumber")?.Value != null ?
                Convert.ToInt32(docSection.Properties
                .FirstOrDefault(p => p.Id == "sheetsNumber")?.Value) : default,
        };

        var name = docSection.Properties
            .FirstOrDefault(p => p.Id == "name")?.Value;

        if (name != null)
        {
            name = name.Replace("@/", ". ");
            if (doc.IsAssemblyDrawing && name[^16..] == "Сборочный чертеж")
            {
                name = name[..^18];
            }

            name = Regex.Replace(name, @"[\/?:*""><|]+", "", RegexOptions.Compiled);

            doc.Name = name;
        }

        return doc;
    }

    private static KompasDocument? Analyze3D19(Root19 xmlDoc)
    {
        var docSection = xmlDoc.Product.InfObjects
                .FirstOrDefault(i => i.Properties.Exists(p => p.Id == "marking"));

        if (docSection == null)
            return null;

        var doc = new KompasDocument
        {
            IsAssemblyDrawing = docSection.Properties
                .FirstOrDefault(p => p.Id == "marking")?.Properties
                .FirstOrDefault(p => p.Id == "documentNumber")?.Value == "СБ",
            Marking = docSection.Properties
                .FirstOrDefault(p => p.Id == "marking")?.Properties
                .FirstOrDefault(p => p.Id == "base")?.Value
                ?? string.Empty,
            StampAuthor = docSection.Properties
                .FirstOrDefault(p => p.Id == "stampAuthor")?.Value
                ?? string.Empty,
            CheckedBy = docSection.Properties
                .FirstOrDefault(p => p.Id == "checkedBy")?.Value
                ?? string.Empty,
            RateOfInspection = docSection.Properties
                .FirstOrDefault(p => p.Id == "rateOfInspection")?.Value
                ?? string.Empty,
            Format = docSection.Properties
                .FirstOrDefault(p => p.Id == "format")?.Value
                ?? string.Empty,
            SheetsNumber = docSection.Properties
                .FirstOrDefault(p => p.Id == "sheetsNumber")?.Value != null ?
                Convert.ToInt32(docSection.Properties
                .FirstOrDefault(p => p.Id == "sheetsNumber")?.Value) : default,

        };

        var name = docSection.Properties
            .FirstOrDefault(p => p.Id == "name")?.Value;

        if (name != null)
        {
            name = name.Replace("@/", ". ");
            if (doc.IsAssemblyDrawing && name[^16..] == "Сборочный чертеж")
            {
                name = name[..^18];
            }

            name = Regex.Replace(name, @"[\/?:*""><|]+", "", RegexOptions.Compiled);

            doc.Name = name;
        }

        return doc;
    }

    //private Root19 DeserializeRoot19()
    //{
    //    using var packageStream = new MemoryStream();

    //    using (FileStream fs = new(Path, FileMode.Open, FileAccess.Read, FileShare.Read))
    //    {
    //        fs.CopyTo(packageStream);
    //    }

    //    packageStream.Position = 0;

    //    using (var z = new ZipArchive(packageStream, ZipArchiveMode.Read))
    //    {
    //        var infoEntry = z.GetEntry("MetaProductInfo");

    //        if (infoEntry == null)
    //        {
    //            throw new Exception("ZipFile entry not found");
    //        }

    //        Root19? xmlDoc;
    //        try
    //        {
    //            XmlSerializer formatter = new(typeof(Root19));
    //            xmlDoc = (Root19?)formatter.Deserialize(infoEntry.Open());
    //        }
    //        catch
    //        {
    //            throw new Exception("Couldn't deserialize entry");
    //        }

    //        return xmlDoc ?? throw new Exception("Couldn't deserialize entry");
    //    }
    //}

    //public DrawingFileInfo(string path)
    //{
    //    Name = string.Empty;
    //    Designation = string.Empty;
    //    //Sheets = new List<DrawingSheet>();
    //    StampAuthor = string.Empty;
    //    CheckedBy = string.Empty;
    //    RateOfInspection = string.Empty;
    //    Format = string.Empty;
    //    SheetsNumber = 0;
    //    AppVersion = string.Empty;

    //    Path = path;
    //    using (var packageStream = new MemoryStream())
    //    {

    //        using (FileStream fs = new(path, FileMode.Open, FileAccess.Read, FileShare.Read))
    //        {
    //            fs.CopyTo(packageStream);
    //        }

    //        packageStream.Position = 0;

    //        using (var z = new ZipArchive(packageStream, ZipArchiveMode.Read))
    //        {
    //            var fileInfo = z.GetEntry("FileInfo");

    //            if (fileInfo == null)
    //            {
    //                throw new Exception("AppVersion couldn't find");
    //            }

    //            using (var reader = new StreamReader(fileInfo.Open()))
    //            {
    //                var info = reader.ReadToEnd().Split('\n');

    //                AppVersion = info
    //                    .FirstOrDefault(s => s.Contains("AppVersion"))?[18..] ?? AppVersion;

    //                if (int.TryParse(info.FirstOrDefault(s => s.Contains("FileType="))?[^1].ToString(), out int type))
    //                {
    //                    DrawingType = (DocType)type;
    //                }
    //            }
    //        }
    //    }

    //    Root19 xmlDoc = AppVersion switch
    //    {
    //        "16.0" or "16.1" or "18.0" => throw new Exception($"AppVersion - {AppVersion} not supported"),
    //        "19.0" or "20.0" or "21.0" => DeserializeRoot19(),
    //        _ => throw new Exception($"AppVersion - {AppVersion} undefined"),
    //    };

    //    switch (DrawingType)
    //    {
    //        case DocType.Drawing:
    //        case DocType.Specification:
    //            Analyze2D19(xmlDoc);
    //            break;
    //        case DocType.Part3D:
    //        case DocType.Assembly3D:
    //            Analyze3D19(xmlDoc);
    //            break;
    //    }

    //}

    //public void RenameInZipFile()
    //{
    //    using (var archive = ZipFile.Open(Path, ZipArchiveMode.Update))
    //    {

    //        var infoEntry = archive.GetEntry("MetaProductInfo");

    //        if (infoEntry == null) return;

    //        XDocument xDoc;

    //        using (var reader = new StreamReader(infoEntry.Open()))
    //        {
    //            xDoc = XDocument.Load(reader);
    //        }

    //        if (xDoc == null) return;


    //        var documentElements = xDoc
    //            .Element("document")
    //            ?.Element("product")
    //            ?.Elements("document")
    //            .ToList();

    //        if (documentElements == null) return;


    //        var isAssemblyDrawing = documentElements
    //            .Elements("property")
    //            .FirstOrDefault(e => e.Attribute("id")?.Value == "marking")
    //            ?.Elements("property")
    //            .FirstOrDefault(e => e.Attribute("id")?.Value == "documentNumber")
    //            ?.Attribute("value")
    //            ?.Value == "СБ";

    //        var name = documentElements
    //            .Elements("property")
    //            .FirstOrDefault(e => e.Attribute("id")?.Value == "name")
    //            ?.Attribute("value")
    //            ?.Value;

    //        if (name == null) return;

    //        name = name.Replace("@/", ". ");
    //        if (isAssemblyDrawing)
    //        {
    //            name = name[..^18];
    //        }

    //        name = Regex.Replace(name, @"[\/?:*""><|]+", "", RegexOptions.Compiled);

    //        var designation = documentElements
    //            ?.Elements("property")
    //            .FirstOrDefault(e => e.Attribute("id")?.Value == "marking")
    //            ?.Elements("property")
    //            .FirstOrDefault(e => e.Attribute("id")?.Value == "base")
    //            ?.Attribute("value")
    //            ?.Value;

    //        if (designation == null) return;

    //        var infObjects = xDoc.Element("document")
    //            ?.Element("product")
    //            ?.Elements("infObject")
    //            .Select(i => i.Attribute("id")?.Value)
    //            .ToList();

    //        var fullFileName = documentElements
    //            !.FirstOrDefault(d => d.Attribute("prodCopy") == null)
    //            ?.Elements("property")
    //            .FirstOrDefault(e => e.Attribute("id")?.Value == "fullFileName")
    //            ?.Attribute("value")?.Value;


    //        if (fullFileName == null || fullFileName == ToString()) return;


    //        documentElements
    //            !.First(d => d.Attribute("prodCopy") == null)
    //            .Elements("property")
    //            .First(e => e.Attribute("id")?.Value == "fullFileName")
    //            .Attribute("value")!.Value = Path + @"\" + ToString() + ".cdw";


    //        infoEntry.Delete();

    //        infoEntry = archive.CreateEntry("MetaProductInfo");

    //        using (var writer = new StreamWriter(infoEntry.Open(), Encoding.Unicode))
    //        {
    //            writer.Write(xDoc.ToString());
    //        }
    //    }
    //}
}
