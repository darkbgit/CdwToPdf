using System.ComponentModel;
using System.Xml.Serialization;

namespace CdwHelper.Core.Analyzers.Ver21.XmlModel;

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public class Document
{
    [XmlAttribute("prodCopy")]
    public bool ProdCopy { get; set; }

    [XmlElement("property")]
    public List<Property> Properties { get; set; }

    [XmlAttribute("id")]
    public string Id { get; set; }

    [XmlAttribute("curEmbKey")]
    public string CurEmbKey { get; set; }
}
