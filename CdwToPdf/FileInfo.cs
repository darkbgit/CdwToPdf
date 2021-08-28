namespace CdwToPdf
{
    public class FileInfo
    {
        public string Name { get; set; }

        public string Designation { get; set; }

        public string Path { get; set; }

        public override string ToString()
        {
            return Designation + " - " + Name;
        }
    }
}