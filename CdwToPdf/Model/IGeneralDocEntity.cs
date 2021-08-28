using System;
using System.Collections.Generic;

namespace CdwToPdf.Model
{
    public interface IGeneralDocEntity
    {
        string GetName();

        string GetDesignation();

        List<DocProp> GetProps();

        void SetGlobalId(Guid value);
    }
}