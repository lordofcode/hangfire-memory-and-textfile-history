using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http.HttpResults;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace HangFireExtension.CustomMemoryAndFileStorage.Models
{
    internal class TableHangFireServer : BaseHangFireTable
    {
        public string ServerId { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
        public DateTime? LastHeartBeat { get; set; }

        public override T DecodeFromJson<T>(string json)
        {
            var decoded = System.Text.Json.JsonSerializer.Deserialize<TableHangFireServer>(json);
            if (decoded == null)
            {
                throw new InvalidCastException();
            }
            Data = decoded.Data;
            LastHeartBeat = decoded.LastHeartBeat;
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
