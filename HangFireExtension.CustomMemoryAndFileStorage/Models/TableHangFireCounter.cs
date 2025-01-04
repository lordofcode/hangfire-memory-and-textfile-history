using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireExtension.CustomMemoryAndFileStorage.Models
{
    internal class TableHangFireCounter : BaseHangFireTable
    {
        public string Key { get; set; } = string.Empty;
        public int Value { get; set; }
        public DateTime? ExpireAt { get; set; }

        public override T DecodeFromJson<T>(string json)
        {
            //throw new NotImplementedException();
            return default(T);
        }
    }
}
