using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sortit
{
    class IOUtils
    {
        static ILog x = LogManager.GetLogger("IOUtils");
        public delegate bool CheckFile(FileInfo fileInfo);

        public static void ListDirectory(String dirPath)
        {
            DirectoryInfo di = new DirectoryInfo(dirPath);
            IEnumerable<DirectoryInfo> dirs = di.EnumerateDirectories();
        }

        public static IEnumerable<string> GetAllFiles(String path, String mask, Func<FileInfo, bool> checkFile = null)
        {
            path = Path.GetDirectoryName(path);
            
            string[] files = new string[1];

            try
            {
                files = Directory.GetFiles(path, mask, SearchOption.AllDirectories);
            }
            catch (Exception e)
            {
                x.Error(e.Message);
            }
            foreach (string file in files)
            {
                if (checkFile == null || checkFile(new FileInfo(file)))
                    yield return file;
            }
        }



        static CheckFile checkfile = delegate(FileInfo fileInfo)
        {
            return true;
        };
    }
}
