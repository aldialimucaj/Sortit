using log4net;
using Sortit.al.aldi.sortit.model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sortit.al.aldi.sortit.control;

namespace Sortit
{
    class IOUtils
    {
        static ILog x = LogManager.GetLogger("IOUtils");
        public delegate bool CheckFile(File2Sort fileInfo);


        /// <summary>
        /// Get files recursively.
        /// </summary>
        /// <param name="path">Path to be crawled</param>
        /// <param name="mask">Mask to filter the files with. Can be separated by a pipe like *.txt|*.pdf</param>
        /// <param name="checkFile">Delegate to check for</param>
        /// <returns></returns>
        public static IEnumerable<File2Sort> GetAllFiles(String path, String mask, Func<File2Sort, bool> checkFile = null)
        {
            path = Path.GetDirectoryName(path);

            List<string> files = new List<string>();
            string[] fileMasks = mask.Split('|');
            foreach (string fMaks in fileMasks)
            {
                try
                {
                    string[] t_files = Directory.GetFiles(path, fMaks, SearchOption.AllDirectories);
                    files.AddRange(t_files);
                }
                catch (Exception e)
                {
                    x.Error(e.Message);
                }
            }

            foreach (string file in files)
            {
                if (null != file && (checkFile == null || checkFile(new File2Sort(file))))
                    yield return new File2Sort(file);
            }
        }

        /// <summary>
        /// Get all files recursively with default file mask *.*
        /// </summary>
        /// <param name="path"></param>
        /// <param name="checkFile"></param>
        /// <returns></returns>
        public static IEnumerable<File2Sort> GetAllFiles(String path, Func<File2Sort, bool> checkFile = null)
        {
            String mask = "*.*";
            return IOUtils.GetAllFiles(path, mask, checkFile);
        }

        /// <summary>
        /// Fetch all directories from the specified path recursively. 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="checkDir">delegate for checking the requirements for this filter</param>
        /// <returns></returns>
        public static IEnumerable<DirectoryInfo> GetAllDirectories(String path, Func<DirectoryInfo, bool> checkDir = null)
        {
            String mask = "*.*";
            string[] t_directories = Directory.GetDirectories(path, mask, SearchOption.AllDirectories);
            foreach (string file in t_directories)
            {
                if (null != file && (checkDir == null || checkDir(new DirectoryInfo(file))))
                    yield return new DirectoryInfo(file);
            }

        }

        /// <summary>
        /// Deletes all empty directories. Does not even check for hidden files
        /// so the directory has to be complitely empty to fit in the filter.
        /// </summary>
        /// <param name="path">Path to be crawled</param>
        public static void CleanEmptyDirs(String path)
        {
            IEnumerable<DirectoryInfo> t_directories = GetAllDirectories(path, _ => !_.EnumerateFiles().Any() && !_.EnumerateDirectories().Any());
            foreach (DirectoryInfo dir in t_directories)
            {
                Console.WriteLine("[DEL_DIR] "+dir.FullName);
                dir.Delete();
            }
        }

        /// <summary>
        /// Renames file after checking that destination doesnt exist.
        /// </summary>
        /// <param name="file"></param>
        public static bool SafeRename(File2Sort file)
        {
            if (!file.DestinationFileExists())
            {
                try
                {
                    SafeCreateParents(file.FullDestination);
                    File.Move(file.FullPath, file.FullDestination);
                    return true;
                }
                catch (Exception e)
                {
                    x.Error(e.Message);
                    return false;
                }
            }
            else
            {
                x.Error("Destination file already exists: " + file.FullDestination);
                return false;
            }
        }

        /// <summary>
        /// Copy files to the destination within the file attribute
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool SafeCopy(File2Sort file)
        {
            if (!file.DestinationFileExists())
            {
                try
                {
                    SafeCreateParents(file.FullDestination);
                    File.Copy(file.FullPath, file.FullDestination);
                    return true;
                }
                catch (Exception e)
                {
                    x.Error(e.Message);
                    return false;
                }
            }
            else
            {
                x.Error("Destination file already exists: " + file.FullDestination);
                return false;
            }
        }

        /// <summary>
        /// Create parent directories recursively
        /// </summary>
        /// <param name="file">Path to create</param>
        public static void SafeCreateParents(String file)
        {
            FileInfo file2check = new FileInfo(file);
            if (!file2check.Directory.Exists)
            {
                SafeCreateParents(file2check.Directory.FullName);
                Directory.CreateDirectory(file2check.Directory.FullName);
            }
        }
    }
}
