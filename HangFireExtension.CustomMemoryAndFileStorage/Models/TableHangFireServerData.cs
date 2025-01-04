using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireExtension.CustomMemoryAndFileStorage.Models
{
    internal class TableHangFireServerData
    {
        public int WorkerCount { get; set; }
        public string[] Queues { get; set; } = new string[0];
        public DateTime? StartedAt { get; set; }
    }
}
