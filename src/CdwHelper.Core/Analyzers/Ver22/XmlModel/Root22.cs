using System.ComponentModel;
using System.Xml.Serialization;

namespace CdwHelper.Core.Analyzers.Ver22.XmlModel;

[Serializable]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
[XmlRoot(Namespace = "", IsNullable = false, ElementName = "document")]
public class Root22
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
