using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Sortit.al.aldi.sortit.control;
using log4net;

namespace Sortit.al.aldi.sortit.model
{
    public class File2Sort
    {
        private static readonly ILog x = LogManager.GetLogger("File2Sort");

        public enum FileChangesType
        {
            FULL_DESTINATION_CHANGED, OPERATION_STARTED, OPERATION_ENDED
        }

        public delegate void UpdateFileDelegate(File2Sort file, FileChangesType changeType);

        /// <summary>
        /// Event triggered when primary attributes have changed
        /// </summary>
        public event UpdateFileDelegate UpdateFileChanged;

        /// <summary>
        /// the path and name of file
        /// </summary>
        public String FullPath { get; set; }

        /// <summary>
        /// just the name of the file
        /// </summary>
        public String FileName { get; set; }

        /// <summary>
        /// just the path of the parten directory
        /// </summary>
        public String FilePath { get; set; }

        /// <summary>
        /// the raw c# file representation
        /// </summary>
        public FileInfo RawSourceFile { get; set; }

        /// <summary>
        /// the raw c# file representation
        /// </summary>
        public FileInfo RawDestinationFile { get; set; }

        /// <summary>
        /// the destination full path with changes and filename
        /// </summary>
        public String FullDestination { get; private set; }

        /// <summary>
        /// when operation starts on this object this flag is set to true
        /// </summary>
        public bool Operating { get; private set; }

        /// <summary>
        /// when operation is finished on this object then this flag is set to true
        /// </summary>
        public bool OperationFinished { get; private set; }

        /// <summary>
        /// DateTime of Raw Source File
        /// </summary>
        public DateTime CreatedDateTime
        {
            get
            {
                return RawSourceFile.CreationTimeUtc;
            }
        }

        /// <summary>
        /// Does this file fit alpha-numeric naming i.e. starts with a-z 0-9
        /// </summary>
        public bool IsAlphaNumeric
        {
            get { return SortFilesAlpha.alphabet.Any(s => FileName.ToUpper().StartsWith(s)); }
        }

        /// <summary>
        /// File exists at the destination
        /// </summary>
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

        public File2Sort(File2Sort file)
            : this(file.FullPath)
        {
            if (null != file.FullDestination)
            {
                this.FullDestination = file.FullDestination;
            }
        }

        public void SetDestinationFullPath(Func<File2Sort, String> sortFunc)
        {
            FullDestination = sortFunc(this);
            RawDestinationFile = new FileInfo(FullDestination);
            if (null != UpdateFileChanged)
                UpdateFileChanged(this, FileChangesType.FULL_DESTINATION_CHANGED);
        }

        public bool Move(bool overwrite = false)
        {
            if (!String.IsNullOrEmpty(FullPath) && !String.IsNullOrEmpty(FullDestination))
            {
                // File.Move does not have an overwrite flag so we need to delete destination file ahead of moving
                // Also make sure that destination and source are not the same otherwise you might delete all your files
                if (DestinationFileExists() && !FullDestination.Equals(FullPath) && overwrite) File.Delete(FullDestination);
                try
                {
                    UpdateFileChanged(this, File2Sort.FileChangesType.OPERATION_STARTED);
                    File.Move(FullPath, FullDestination);
                    UpdateFileChanged(this, File2Sort.FileChangesType.OPERATION_ENDED);
                    return true;
                }
                catch (IOException e)
                {
                    x.Error(e.Message + " - " + this);
                }
                
            }
            return false;
        }

        public bool Copy(bool overwrite = false)
        {
            if (!String.IsNullOrEmpty(FullPath) && !String.IsNullOrEmpty(FullDestination))
            {
                try
                {
                    UpdateFileChanged(this, File2Sort.FileChangesType.OPERATION_STARTED);
                    File.Copy(FullPath, FullDestination, overwrite);
                    UpdateFileChanged(this, File2Sort.FileChangesType.OPERATION_ENDED);
                    return true;
                }
                catch (IOException e)
                {
                    x.Error(e.Message + " - " + this);
                }
                
            }
            return false;
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
