using Sortit.al.aldi.sortit.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sortit.al.aldi.sortit.control
{
    interface ISort
    {
        /// <summary>
        /// Sort the files in the list
        /// </summary>
        /// <param name="fiels"></param>
        void Sort(IList<File2Sort> fiels);

        /// <summary>
        /// Rename the file after the predefined algorithm
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        String RenameFunc(File2Sort file);
    }
}
