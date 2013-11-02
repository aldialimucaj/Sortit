using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sortit.al.aldi.sortit.control
{
    class SortFilesDate : SortImpl
    {

        String Destination {get; set;}
        String DatePattern { get; set; }
        bool Copy { get; set; }

        public SortFilesDate(String dest, String pattern, bool copy) : base(copy)
        {
            Destination = dest;
            DatePattern = pattern;
        }

        /// <summary>
        /// Setting Destination path according to the date pattern
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public override string RenameFunc(model.File2Sort file)
        {
            String returnPath = Destination;
            returnPath = returnPath.EndsWith("\\") ? returnPath : returnPath + "\\";

            DateTime dt = file.CreatedDateTime;
            String dateReverse = dt.Year.ToString() + dt.Month.ToString() + dt.Day.ToString() + "\\";
            returnPath += dateReverse + file.FileName;

            return returnPath;
        }
    }
}
