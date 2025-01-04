using Hangfire.Storage;
using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireExtension.CustomMemoryAndFileStorage.Models
{
    internal class TableHangFireSet : BaseHangFireTable
    {
        public string Key { get; set; } = string.Empty;
        public double Score { get; set; }
        public string Value { get; set; } = string.Empty;
        public DateTime? ExpireAt { get; set; }

        public override T DecodeFromJson<T>(string json)
        {
            var decoded = System.Text.Json.JsonSerializer.Deserialize<TableHangFireSet>(json);
            if (decoded == null)
            {
                throw new InvalidCastException();
            }
            Key = decoded.Key;
            Score = decoded.Score;
            Value = decoded.Value;
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
