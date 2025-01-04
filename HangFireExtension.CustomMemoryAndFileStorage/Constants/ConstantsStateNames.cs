using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireExtension.CustomMemoryAndFileStorage.Constants
{
    internal static class ConstantsStateNames
    {
        internal readonly static string Awaiting =  "Awaiting";
        internal readonly static string Deleted = "Deleted";
        internal readonly static string Enqueued = "Enqueued";
        internal readonly static string Failed = "Failed";
        internal readonly static string Fetched = "Fetched";
        internal readonly static string Processing = "Processing";
        internal readonly static string Scheduled = "Scheduled";
        internal readonly static string Succeeded = "Succeeded";
        internal readonly static string Statistics_Deleted = "stats:deleted";
        internal readonly static string Statistics_Succeeded = "stats:succeeded";
    }
}
