using System.Collections.Generic;
using System.Windows.Documents;
using CdwToPdf.Enums;

namespace CdwToPdf.Core
{
    public class DrawingFileInfo
    {
        public string Name { get; set; }
        public string Designation { get; set; }
        public string Path { get; set; }
        public DrawingType DrawingType {  get; set; }
        public List<DrawingSheet> Sheets { get; set; }
        public bool IsAssemblyDrawing { get; set; }
        public override string ToString()
        {
            return Designation + (IsAssemblyDrawing && DrawingType == DrawingType.Drawing ? " СБ" : "") + " - " + Name;
        }
    }
}