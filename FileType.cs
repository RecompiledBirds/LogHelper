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
        LOG,
        OTHER
    }

    public static class FileTypeReader
    {
        public static FileType GetFileType(string extension)
        {
            extension = extension.Replace(".", "").ToLower();
            FileType res = FileType.OTHER;
            switch (extension)
            {
                case "xml":
                    res= FileType.XML;
                    break;
                case "evtx":
                    res= FileType.EVTX;
                    break;
                case "kml":
                    res= FileType.KML;
                    break;
                case "log":
                    res=FileType.LOG;
                    break;
            }
            return res;
        }
    }
}
