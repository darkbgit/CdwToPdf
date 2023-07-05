using System.ComponentModel;
using System.Xml.Serialization;

namespace CdwHelper.Core.Analyzers.Ver21.XmlModel;

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public class InfObject
{
    [XmlAttribute("id")]
    public string Id { get; set; }

    [XmlAttribute("ownDocument")]
    public List<string> OwnDocument { get; set; }

    [XmlElement("property")]
    public List<Property> Properties { get; set; }

    [XmlElement("infObject")]
    public List<InfObject> InfObjects { get; set; }
}
