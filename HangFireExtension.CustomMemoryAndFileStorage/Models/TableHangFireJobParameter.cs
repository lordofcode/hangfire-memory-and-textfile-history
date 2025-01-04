using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireExtension.CustomMemoryAndFileStorage.Models
{
    internal class TableHangFireJobParameter : BaseHangFireTable
    {
        public long JobId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public override T DecodeFromJson<T>(string json)
        {
            throw new NotImplementedException();
        }
    }
}
