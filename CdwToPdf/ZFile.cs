﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zip;

namespace CdwToPdf
{
    public class ZFile
    {
        public MemoryStream OutputMemStream { get; private set; }

        public bool IsZip(Stream fileStream)
        {
            return ZipFile.IsZipFile(fileStream, false);
        }

        public void ExtractFileToMemoryStream(Stream fileStream, string fileInArchive)
        {
            var ms = new MemoryStream();
            try
            {
                fileStream.Position = 0;
                var zip = ZipFile.Read(fileStream);
                foreach (var entry in zip.Where(entry => entry.FileName == fileInArchive))
                {
                    entry.Extract(ms);  // extract uncompressed content into a memorystream
                    // the application can now access the MemoryStream here
                    OutputMemStream = ms;
                }
            }
            catch
            {
                OutputMemStream = null;
            }
        }
    }
}
