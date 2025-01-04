using Hangfire.Common;
using Hangfire.Storage.Monitoring;
using HangFireExtension.CustomMemoryAndFileStorage.Constants;
using HangFireExtension.CustomMemoryAndFileStorage.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireExtension.CustomMemoryAndFileStorage.Helpers
{
    internal class MemoryCacheHelper
    {
        public static void SetFilePath(string path)
        {
            MemoryAndFileStorageMemoryCache.SetFilePath(path);
        }

        public static List<T> GetListOfItemsFromMemory<T>(string key) where T : class
        {
            var resultList = new List<T>();
            var setItem = MemoryAndFileStorageMemoryCache.GetItemFromCache(key);
            if (setItem != null && setItem.Value is List<T>)
            {
                var currentData = setItem.Value as List<T>;
                if (currentData != null)
                {
                    currentData.ForEach(rec => resultList.Add(rec));
                }
            }
            return resultList;
        }

        public static void CommitToFile()
        {
            if (MemoryAndFileStorageMemoryCache.IsDirty)
            {
                MemoryAndFileStorageMemoryCache.IsDirty = false;
                MemoryAndFileStorageMemoryCache.SaveItemsToFileSystem();
            }
        }

        public static List<TableHangFireCounter> GetAllCounters()
        {
            return MemoryCacheHelper.GetListOfItemsFromMemory<TableHangFireCounter>(ConstantsTableNames.Counter);
        }


        public static List<TableHangFireJob> GetAllJobs()
        {
            return MemoryCacheHelper.GetListOfItemsFromMemory<TableHangFireJob>(ConstantsTableNames.Job);
        }

        public static List<TableHangFireJob> GetJobById(string jobId, out TableHangFireJob? jobResult)
        {
            var iJobId = Convert.ToInt64(jobId);
            var jobList = MemoryCacheHelper.GetListOfItemsFromMemory<TableHangFireJob>(ConstantsTableNames.Job);
            jobResult = jobList.FirstOrDefault(rec => rec.Id == iJobId);
            return jobList;
        }

        public static List<TableHangFireSet> GetAllRecurringJobs()
        {
            return MemoryCacheHelper.GetListOfItemsFromMemory<TableHangFireSet>(ConstantsTableNames.Set).Where(rec => rec.Key == ConstantsSetNames.RecurringJobs).ToList();
        }

        public static List<TableHangFireServer> GetAllServers()
        {
            return MemoryCacheHelper.GetListOfItemsFromMemory<TableHangFireServer>(ConstantsTableNames.Server);
        }

        public static List<ServerDto> GetAllServersAsServerDtoList()
        {
            var result = new List<ServerDto>();
            var items = GetAllServers();
            foreach (var item in items)
            {
                var deserialized = SerializationHelper.Deserialize(item.Data, typeof(TableHangFireServerData));
                if (deserialized == null)
                {
                    continue;
                }
                if (deserialized is TableHangFireServerData == false)
                {
                    continue;
                }
                TableHangFireServerData serverData = (TableHangFireServerData)deserialized;
                result.Add(new ServerDto() { Name = item.ServerId, Queues = serverData!.Queues, StartedAt = serverData.StartedAt!.Value, Heartbeat=item.LastHeartBeat, WorkersCount = serverData.WorkerCount });
            }
            return result;
        }
    }
}
