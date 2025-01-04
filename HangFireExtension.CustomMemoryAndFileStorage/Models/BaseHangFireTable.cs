using Hangfire.Storage;
using HangFireExtension.CustomMemoryAndFileStorage.Helpers;
using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireExtension.CustomMemoryAndFileStorage.Models
{
    internal abstract class BaseHangFireTable
    {
        public BaseHangFireTable() 
        {
            Id = IdentityHelper.GetIdentityForTable(GetType().Name);
        }

        public long Id { get; set; }

        public string EncodeToJson()
        {
            return System.Text.Json.JsonSerializer.Serialize(this, this.GetType());
        }

        public abstract T DecodeFromJson<T>(string json) where T : class;
        
    }
}
