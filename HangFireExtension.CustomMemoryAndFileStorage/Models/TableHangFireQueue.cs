using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireExtension.CustomMemoryAndFileStorage.Models
{
    internal class TableHangFireQueue : BaseHangFireTable
    {
        public string Name { get; set; } = string.Empty;

        public Queue<string> Queue { get; set; } = new Queue<string>();

        public override T DecodeFromJson<T>(string json)
        {
            //throw new NotImplementedException();
            return default(T);
        }
    }
}
