using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireExtension.CustomMemoryAndFileStorage.Models
{
    internal class TableHangFireState : BaseHangFireTable
    {
        public long JobId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Data { get; set; } = string.Empty;

        public override T DecodeFromJson<T>(string json)
        {
            //throw new NotImplementedException();
            return default(T);
        }

        public Dictionary<string, string> DeserializeData()
        {
            if (string.IsNullOrEmpty(Data))
            {
                return new Dictionary<string, string>();
            }
            var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(Data);
            if (result == null)
            {
                return new Dictionary<string, string>();
            }
            return result;
        }
    }
}
