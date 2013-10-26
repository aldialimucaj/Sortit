using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Sortit.al.aldi.sortit.control;

namespace Sortit.al.aldi.sortit.model
{
    class File2Sort
    {
        private String _destination;
        public String FullPath { get; set; } // the path and name of file
        public String FileName { get; set; } // just the name of the file
        public String FilePath { get; set; } // just the path of the parten directory
        public FileInfo RawSourceFile { get; set; }    // the raw c# file representation
        public FileInfo RawDestinationFile { get; set; }    // the raw c# file representation
        public String FullDestination { get; private set; } // the destination full path with changes and filename
        public String Destination
        {
            get { return Destination; }
            set { _destination = value.StartsWith("\\") ? value : value + "\\"; }
        }    // the destination root file
        public bool IsAlphaNumeric
        {
            get { return SortFilesAlpha.alphabet.Any(s => FileName.ToUpper().StartsWith(s)); }
        }
        public bool IsAlreadySorted
        {
            get
            {
                return null != FullPath && null != FullDestination && !FullPath.Equals(FullDestination);
            }

        }

        public File2Sort(FileInfo file)
        {
            RawSourceFile = file;
            FullPath = file.FullName;
            FileName = file.Name;
            FilePath = file.Directory.FullName;
        }

        public File2Sort(String file)
            : this(new FileInfo(file))
        {
            
        }

        public void SetDestinationFullPath(Func<File2Sort, String> sortFunc)
        {
            FullDestination = sortFunc(this);
        }

        public bool SourceFileExists()
        {
            return RawSourceFile.Exists;
        }

        public bool DestinationFileExists()
        {
            return null != RawDestinationFile && RawDestinationFile.Exists;
        }

        public void SetRawDestinationFile(String file)
        {
            RawDestinationFile = new FileInfo(file);
        }


        override public String ToString()
        {
            return FileName;
        }
    }
}
