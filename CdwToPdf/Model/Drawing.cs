using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CdwToPdf.Model
{
    public class Drawing : GeneralProp, IGeneralDocEntity
    {
        public List<DrawingSheet> Sheets { get; set; }

        public Drawing()
        {
            Sheets = new List<DrawingSheet>();
        }

        public string GetName()
        {
            return Name;
        }

        public string GetDesignation()
        {
            return Designation;
        }

        public List<DocProp> GetProps()
        {
            return ListSpcProps;
        }

        public void SetGlobalId(Guid value)
        {
            GlobalId = value;
        }
    }
}

