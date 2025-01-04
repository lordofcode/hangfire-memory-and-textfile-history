using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireExtension.CustomMemoryAndFileStorage.Helpers
{
    internal static class IdentityHelper
    {
        private static List<TableIdentity> identityValues = new List<TableIdentity>();

        internal static long GetIdentityForTable(string tableName)
        {
            lock (identityValues){
                if (identityValues.Any(rec => rec.Name == tableName))
                {
                    var item = identityValues.First(rec => rec.Name == tableName);
                    item.Identity++;
                    return item.Identity;
                }
                else
                {
                    identityValues.Add(new TableIdentity() { Name = tableName, Identity = 1 });
                    return 1;
                }
            }
        }
    }
}
