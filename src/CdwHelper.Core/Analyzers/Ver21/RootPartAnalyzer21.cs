using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using CdwHelper.Core.Analyzers.Ver21.XmlModel;
using CdwHelper.Core.Enums;
using CdwHelper.Core.Interfaces;
using CdwHelper.Core.Models;
using HtmlAgilityPack;

namespace CdwHelper.Core.Analyzers.Ver21;

internal class RootPartAnalyzer21 : IRootPartAnalyzer
{
    public KompasDocument AnalyzeXml(Stream xml, DocType type)
    {
        if (type == DocType.Specification)
        {
            CleanXmlStreamFromDuplicateAttributes(xml);
        }

        var root = Deserialize(xml);

        KompasDocument? doc = type switch
        {
            DocType.Drawing => Analyze2D(root),
            DocType.Specification => AnalyzeSpecification(root),
            DocType.Part3D or DocType.Assembly3D => Analyze3D(root),
            _ => throw new Exception($"Unsupported drawing type {type}."),
        };

        return doc ?? throw new Exception("Couldn't analyze root part.");
    }

    private static void CleanXmlStreamFromDuplicateAttributes(Stream stream)
    {
        var htmlDocument = new HtmlDocument
        {
            OptionOutputAsXml = true,
            OptionOutputOriginalCase = true,
            BackwardCompatibility = false
        };

        htmlDocument.Load(stream, Encoding.Unicode);

        stream.SetLength(0);
        htmlDocument.Save(stream);

        stream.Position = 0;
    }

    private static Root21 Deserialize(Stream xml)
    {
        try
        {
            XmlSerializer formatter = new(typeof(Root21));
            return formatter.Deserialize(xml) as Root21
                ?? throw new Exception("Couldn't deserialize entry.");
        }
        catch
        {
            throw new Exception("Couldn't deserialize entry.");
        }
    }

    private static KompasDocument? AnalyzeSpecification(Root21 xmlDoc)
    {
        var docSection = xmlDoc.Product.Documents
                .FirstOrDefault(d => d.Id == xmlDoc.Product.ThisDocument);

        if (docSection == null)
            return null;

        var doc = new KompasDocument
        {
            FileName = docSection.Properties
                .FirstOrDefault(p => p.Id == "name")?.Value
                ?? string.Empty,
            FullFileName = docSection.Properties
                           .FirstOrDefault(p => p.Id == "fullFileName")?.Value
                       ?? string.Empty,
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
            //if (doc.IsAssemblyDrawing && name[^16..] == "Сборочный чертеж")
            //{
            //    name = name[..^18];
            //}

            name = Regex.Replace(name, @"[\/?:*""><|]+", "", RegexOptions.Compiled);

            doc.Name = name;
        }

        return doc;
    }

    private static KompasDocument? Analyze2D(Root21 xmlDoc)
    {
        var docSection = xmlDoc.Product.Documents
                .FirstOrDefault(d => !d.ProdCopy && d.Properties.Exists(p => p.Id == "marking"));

        if (docSection == null)
            return null;

        var doc = new KompasDocument
        {
            FileName = docSection.Properties
                .FirstOrDefault(p => p.Id == "name")?.Value
                ?? string.Empty,
            FullFileName = docSection.Properties
                           .FirstOrDefault(p => p.Id == "fullFileName")?.Value
                       ?? string.Empty,
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
            //if (doc.IsAssemblyDrawing && name[^16..] == "Сборочный чертеж")
            //{
            //    name = name[..^18];
            //}

            name = Regex.Replace(name, @"[\/?:*""><|]+", "", RegexOptions.Compiled);

            doc.Name = name;
        }

        return doc;
    }

    private static KompasDocument? Analyze3D(Root21 xmlDoc)
    {
        var docSection = xmlDoc.Product.Documents
                .FirstOrDefault(d => !string.IsNullOrEmpty(d.CurEmbKey));

        if (docSection == null)
            return null;

        var doc = new KompasDocument
        {
            FullFileName = docSection.Properties
                               .FirstOrDefault(p => p.Id == "fullFileName")?.Value
                           ?? string.Empty
        };

        var infoObjectSection = xmlDoc.Product.InfObjects
            .FirstOrDefault(i => i.Id == docSection.CurEmbKey);

        if (infoObjectSection == null) return doc;


        doc.Name = infoObjectSection.Properties
                       .FirstOrDefault(p => p.Id == "name")?.Value
                   ?? string.Empty;

        doc.Marking = infoObjectSection.Properties
                          .FirstOrDefault(p => p.Id == "marking")?.Properties
                          .FirstOrDefault(p => p.Id == "base")?.Value
                      ?? string.Empty;

        return doc;
    }


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
