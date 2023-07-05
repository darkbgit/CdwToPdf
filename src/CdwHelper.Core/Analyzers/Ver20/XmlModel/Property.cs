using System.ComponentModel;
using System.Xml.Serialization;

namespace CdwHelper.Core.Analyzers.Ver20.XmlModel;

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public class Property
{
    [XmlAttribute("id")]
    public string Id { get; set; }

    [XmlAttribute("value")]
    public string Value { get; set; }

    [XmlElement("property")]
    public List<Property> Properties { get; set; }
}
