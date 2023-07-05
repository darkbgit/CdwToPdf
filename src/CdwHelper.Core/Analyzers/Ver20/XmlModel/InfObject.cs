using System.ComponentModel;
using System.Xml.Serialization;

namespace CdwHelper.Core.Analyzers.Ver20.XmlModel;

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
public class InfObject
{
    [XmlElement("property")]
    public List<Property> Properties { get; set; }

    [XmlElement("infObject")]
    public List<InfObject> InfObjects { get; set; }
}
