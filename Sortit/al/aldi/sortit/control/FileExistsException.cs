using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sortit.al.aldi.sortit.control
{
    [Serializable]
    class FileExistsException : Exception
    {
        private string p;

        public FileExistsException(string p) : base(p)
        {
            this.p = p;
        }
    }
}
