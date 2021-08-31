using CdwToPdf.Enums;

namespace CdwToPdf.Core
{
    public class FileInfo
    {
        public string Name { get; set; }
        public string Designation { get; set; }
        public string Path { get; set; }
        public DrawingType DrawingType {  get; set; }
        public bool IsAssemblyDrawing { get; set; }
        public override string ToString()
        {
            return Designation + " - " + Name;
        }
    }
}