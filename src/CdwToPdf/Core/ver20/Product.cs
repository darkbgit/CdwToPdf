using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace CdwToPdf.Core.ver20
{
    /// <remarks/>
    [Serializable]
    [System.ComponentModel.DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class Product
    {
        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlAttribute("thisDocument")]
        public string ThisDocument { get; set; }

        [XmlElement("document")]
        public List<Document> Documents { get; set; }

        [XmlElement("infObject")]
        public List<InfObject> InfObjects { get; set; }
    }
}
