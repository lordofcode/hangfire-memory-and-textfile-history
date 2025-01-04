using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireExtension.CustomMemoryAndFileStorage.Models
{
    internal class TableHangFireJob : BaseHangFireTable
    {
        public long StateId { get; set; }
        public string StateName { get; set; } = string.Empty;
        public string InvocationData { get; set; } = string.Empty;
        public string Arguments { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpireAt { get; set; }

        public List<TableHangFireJobParameter> Parameters { get; set; } = new List<TableHangFireJobParameter>();

        public override T DecodeFromJson<T>(string json)
        {
            var decoded = System.Text.Json.JsonSerializer.Deserialize<TableHangFireJob>(json);
            if (decoded == null)
            {
                throw new InvalidCastException();
            }
            InvocationData = decoded.InvocationData;
            Arguments = decoded.Arguments;
            CreatedAt = decoded.CreatedAt;
            ExpireAt = decoded.ExpireAt;
            if (this is T && this != null)
            {
                T? result = this as T;
                if (result != null)
                {
                    return result;
                }
            }
            throw new InvalidCastException();
        }
    }
}
