using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogHelper
{
    public enum FileType
    {
        XML,
        EVTX,
        KML,
        OTHER
    }

    public static class FileTypeReader
    {
        public static FileType GetFileType(string extension)
        {
            extension = extension.Replace(".", "").ToLower();
            if(extension == "xml")return FileType.XML;
            if(extension =="evtx")return FileType.EVTX;
            if (extension == "kml") return FileType.KML;
            return FileType.OTHER;
        }
    }
}
