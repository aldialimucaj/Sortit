using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sortit.al.aldi.sortit.control
{
    class FileExistsException : Exception
    {
        private string p;

        public FileExistsException(string p) : base(p)
        {
            // TODO: Complete member initialization
            this.p = p;
        }
    }
}
