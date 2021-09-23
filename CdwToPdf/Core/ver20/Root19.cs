using System;
using System.Xml.Serialization;

namespace CdwToPdf.Core.ver20
{
    [Serializable]
    [System.ComponentModel.DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false, ElementName = "document")]
    public class Root19
    {
        [XmlElement("product")]
        public Product Product { get; set; }

        [XmlAttribute("version")]
        public string Version { get; set; }

        [XmlAttribute("create_version")]
        public string CreateVersion { get; set; }

        [XmlAttribute("state")]
        public string State { get; set; }
    }
}
