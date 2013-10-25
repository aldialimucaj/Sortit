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
        void sort(IList<File2Sort> fiels);
    }
}
