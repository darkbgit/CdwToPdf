using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace CdwToPdf.Core.ver20
{
    [Serializable]
    [System.ComponentModel.DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class Document
    {
        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlElement("property")]
        public List<Property> Properties { get; set; }
    }
}
