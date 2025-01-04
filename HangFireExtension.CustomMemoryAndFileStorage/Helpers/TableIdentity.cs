using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireExtension.CustomMemoryAndFileStorage.Helpers
{
    internal class TableIdentity
    {
        public string Name { get; set; } = string.Empty;  
        public long Identity { get; set; }
    }
}
