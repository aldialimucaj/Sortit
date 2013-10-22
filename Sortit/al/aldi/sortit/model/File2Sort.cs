using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Sortit.al.aldi.sortit.model
{
    class File2Sort
    {
        public File2Sort(FileInfo file)
        {
            RawFile = file;
            FullPath = file.FullName;
            FileName = file.Name;
            FilePath = file.Directory.FullName;
        }

        String FullPath { get; set; } // the path and name of file
        String FileName { get; set; } // just the name of the file
        String FilePath { get; set; } // just the path of the parten directory
        FileInfo RawFile { get; set; }    // the raw c# file representation
    }
}
