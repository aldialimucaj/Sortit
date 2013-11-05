using Sortit.al.aldi.sortit.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sortit.al.aldi.sortit.control
{
    public abstract class SortImpl : ISort
    {
        bool Copy { get; set; }

        public abstract string RenameFunc(model.File2Sort file);

        public delegate String SortFunction(File2Sort file);

        public SortImpl(bool copy)
        {
            Copy = copy;
        }

        /// <summary>
        /// Sorts the files by moving or copying, depending on the attribute <b>Copy</b>
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public async Task<bool> SortAsync(IList<File2Sort> files)
        {
            SortFunction rename = RenameFunc;
            bool everythingSuccessful = true;

            foreach (File2Sort file in files)
            {
                file.SetDestinationFullPath(_ => rename(_));
                Console.WriteLine(file.FullDestination);
                if (Copy)
                {
                    everythingSuccessful &= await IOUtils.SafeCopyAsync(file);
                }
                else
                {
                    everythingSuccessful &= await IOUtils.SafeRenameAsync(file);
                }
            }

            Console.WriteLine(files);
            return everythingSuccessful;
        }



        /// <summary>
        /// Prepares the files by generating the desired path.
        /// This task runs on its own thread.
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public IList<File2Sort> PrepareForSorting(IList<File2Sort> files)
        {
            Thread th1 = new Thread(delegate()
            {

                foreach (File2Sort file in files)
                {
                    PrepareForSorting(file);
                }
            });

            th1.Priority = ThreadPriority.Highest;
            th1.Start();

            return files;
        }

        /// <summary>
        /// Prepare a single file for sorting.
        /// </summary>
        /// <param name="file"></param>
        public File2Sort PrepareForSorting(File2Sort file)
        {
            SortFunction rename = RenameFunc;
            file.SetDestinationFullPath(_ => rename(_));
            return file;
        }


        /// <summary>
        /// Registers observers for the list of files that will trigger if they change
        /// </summary>
        /// <param name="files">list of lifes</param>
        /// <param name="updateFileDelegate">function to call when each file changes</param>
        protected void RegisterObserver(IList<File2Sort> files, File2Sort.UpdateFileDelegate updateFileDelegate)
        {
            foreach (File2Sort file in files)
            {
                file.UpdateFileChanged += new File2Sort.UpdateFileDelegate(updateFileDelegate);
            }
        }

        /// <summary>
        /// Register for this file the delegate which should react to changes on the file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="updateFileDelegate"></param>
        public void RegisterObserver(File2Sort file, File2Sort.UpdateFileDelegate updateFileDelegate)
        {
            file.UpdateFileChanged += new File2Sort.UpdateFileDelegate(updateFileDelegate);
        }

    }
}
