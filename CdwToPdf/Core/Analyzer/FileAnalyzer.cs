using System.IO;
using System.Xml.Serialization;
using CdwToPdf.Core.ver20;

namespace CdwToPdf.Core.Analyzer
{
    public class FileAnalyzer
    {
        public DrawingFileInfo Drawing;

        private readonly Root? _doc20;

        public FileAnalyzer(Stream fileStream, string path)
        {
            IsCompleted = false;
            //drawing.Path = path;

            var z = new ZFile();
            if (!z.IsZip(fileStream)) return;

            z.ExtractFileToMemoryStream(fileStream, "MetaProductInfo");

            if (z.OutputMemStream == null) return;

            var ms = z.OutputMemStream;

            //z.OutputMemStream.CopyTo(ms);
            
            IsCompleted = false;

            Opened = false;
            try
            {
                ms.Position = 0;
                XmlSerializer formatter = new(typeof(Root));
                _doc20 = (Root?)formatter.Deserialize(ms);

                Opened = true;
            }
            catch
            {
                Opened = false;
                IsCompleted = false;
            }

            if (!Opened) return;

            if (_doc20 == null) return;

            try
            {
               // Drawing = new DrawingFileInfo(_doc20, path);
            }
            catch
            {
                Opened = false;
                IsCompleted = false;
            }
        }



        public bool Opened { get; private set; }
        public bool IsCompleted { get; private set; }
    }
}
