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
        DateSortType SortType { get; set; }

        public enum DateSortType
        {
            YYYYMMDD,
            YYYYMMDD_subs,
            YYYYMMDDHH_subs
        }

        public SortFilesDate(String dest, String pattern, bool copy) : base(copy)
        {
            Destination = dest;
            DatePattern = pattern;
            SortType = DateSortType.YYYYMMDD;
        }

        public SortFilesDate(String dest, String pattern, bool copy, DateSortType sortType)
            : base(copy)
        {
            Destination = dest;
            DatePattern = pattern;
            SortType = sortType;
        }

        public SortFilesDate(String dest, String pattern, bool copy, String sortType)
            : base(copy)
        {
            Destination = dest;
            DatePattern = pattern;
            switch (sortType)
            {
                case "ymd":
                    SortType = DateSortType.YYYYMMDD;
                    break;
                case "ymd_subs":
                    SortType = DateSortType.YYYYMMDD_subs;
                    break;
                case "ymdh_subs":
                    SortType = DateSortType.YYYYMMDDHH_subs;
                    break;
            }
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
            String dateReverse = "";
            switch (SortType)
            {
                case DateSortType.YYYYMMDD:
                    dateReverse = dt.Year.ToString() + dt.Month.ToString() + dt.Day.ToString() + "\\";
                    break;
                case DateSortType.YYYYMMDD_subs:
                    dateReverse = dt.Year.ToString() + "\\" + dt.Month.ToString() + "\\" + dt.Day.ToString() + "\\";
                    break;
                case DateSortType.YYYYMMDDHH_subs:
                    dateReverse = dt.Year.ToString() + "\\" + dt.Month.ToString() + "\\" + dt.Day.ToString() + "\\" + dt.Hour.ToString()  + "\\";
                    break;
            }
            
            returnPath += dateReverse + file.FileName;

            return returnPath;
        }
    }
}
