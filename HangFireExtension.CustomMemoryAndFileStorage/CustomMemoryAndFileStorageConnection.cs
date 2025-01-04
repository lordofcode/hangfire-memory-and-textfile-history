using Hangfire.Annotations;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using HangFireExtension.CustomMemoryAndFileStorage.Constants;
using HangFireExtension.CustomMemoryAndFileStorage.Helpers;
using HangFireExtension.CustomMemoryAndFileStorage.Models;
using Microsoft.AspNetCore.Hosting.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireExtension.CustomMemoryAndFileStorage
{
    internal class CustomMemoryAndFileStorageConnection : IStorageConnection
    {
        private IWriteOnlyTransaction _writeOnlyTransaction = new MemoryAndFileStorageWriteOnlyTransaction();//null;


        /// <summary>
        /// TODO: implement with valid IDisposable
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public IDisposable AcquireDistributedLock(string resource, TimeSpan timeout)
        {
            return null;
        }

        public void AnnounceServer(string serverId, ServerContext context)
        {
            var data = new TableHangFireServerData
            {
                WorkerCount = context.WorkerCount,
                Queues = context.Queues,
                StartedAt = DateTime.UtcNow
            };

            var resultList = MemoryCacheHelper.GetListOfItemsFromMemory<TableHangFireServer>(ConstantsTableNames.Server);
            resultList.Add(new TableHangFireServer() { ServerId = serverId, Data = SerializationHelper.Serialize(data), LastHeartBeat = DateTime.UtcNow });
            MemoryAndFileStorageMemoryCache.SetItemInCache(ConstantsTableNames.Server, resultList);
        }

        public string CreateExpiredJob(Job job, IDictionary<string, string> parameters, DateTime createdAt, TimeSpan expireIn)
        {
            var resultList = MemoryCacheHelper.GetListOfItemsFromMemory<TableHangFireJob>(ConstantsTableNames.Job);

            var invocationData = InvocationData.SerializeJob(job);
            var payload = invocationData.SerializePayload(excludeArguments: true);

            var createdJob = new TableHangFireJob() { InvocationData = payload, Arguments = invocationData.Arguments, CreatedAt = createdAt, ExpireAt = createdAt.Add(expireIn) };
            foreach (var p in parameters)
            {
                var par = new TableHangFireJobParameter() { JobId = createdJob.Id, Name = p.Key, Value = p.Value };
                createdJob.Parameters.Add(par);
            }
            resultList.Add(createdJob);
            MemoryAndFileStorageMemoryCache.SetItemInCache(ConstantsTableNames.Job, resultList);

            return createdJob.Id.ToString();
        }

        public IWriteOnlyTransaction CreateWriteTransaction()
        {
            //if (_writeOnlyTransaction == null)
            //{
            //    _writeOnlyTransaction = new MemoryAndFileStorageWriteOnlyTransaction();
            //}
            return _writeOnlyTransaction;
        }

        public void Dispose()
        {
            /*
            if (_writeOnlyTransaction == null)
            {
                return;
            }
            _writeOnlyTransaction.Dispose();
            */
        }

        public IFetchedJob FetchNextJob(string[] queues, CancellationToken cancellationToken)
        {
            foreach (var queue in queues)
            {
                var resultList = MemoryCacheHelper.GetListOfItemsFromMemory<TableHangFireQueue>($"{ConstantsTableNames.QueuePrefix}{queue}");
                var record = resultList.FirstOrDefault(rec => rec.Name == queue);
                if (record == null)
                {
                    continue;
                }
                if (record.Queue.Any() == false)
                {
                    continue;
                }
                var item = record!.Queue.Dequeue();
                MemoryAndFileStorageMemoryCache.SetItemInCache($"{ConstantsTableNames.QueuePrefix}{queue}", resultList);
                return new HangFireFetchedJob(item, queue);
            }
            // a null value causes some error messaging of Hangfire Core. But I see no other way :shrug
            return null;
        }

        public Dictionary<string, string> GetAllEntriesFromHash([NotNull] string key)
        {
            var resultList = MemoryCacheHelper.GetListOfItemsFromMemory<TableHangFireHash>(ConstantsTableNames.Hash);
            if (!resultList.Any(rec => rec.Key == key))
            {
                return new Dictionary<string, string>();
            }
            var result = new Dictionary<string, string>();
            resultList.Where(rec => rec.Key == key).ToList().ForEach(rec => { result.Add(rec.Field, rec.Value); });
            return result;
        }

        public HashSet<string> GetAllItemsFromSet([NotNull] string key)
        {
            var resultList = MemoryCacheHelper.GetListOfItemsFromMemory<TableHangFireSet>(ConstantsTableNames.Set);
            return resultList.Where(rec => rec.Key == key).Select(rec => rec.Value).ToHashSet<string>();
        }

        public string GetFirstByLowestScoreFromSet(string key, double fromScore, double toScore)
        {
            var resultList = MemoryCacheHelper.GetListOfItemsFromMemory<TableHangFireSet>(ConstantsTableNames.Set);
            var keyItems = resultList.Where(rec => rec.Key == key && rec.Score >= fromScore && rec.Score <= toScore).OrderBy(rec => rec.Score).ToList();
            if (keyItems.Any() && !string.IsNullOrEmpty(keyItems.First().Value))
            {
                return keyItems.First().Value;
            }
            return null;
        }

        public JobData GetJobData([NotNull] string jobId)
        {
            if (string.IsNullOrEmpty(jobId))
            {
                return null;
            }
            var jobList = MemoryCacheHelper.GetJobById(jobId, out TableHangFireJob? record);
            if (record == null)
            {
                return null;
            }

            var invocationData = InvocationData.DeserializePayload(record.InvocationData);

            Job job = null;
            JobLoadException loadException = null;

            try
            {
                job = invocationData.DeserializeJob();
            }
            catch (JobLoadException ex)
            {
                loadException = ex;
            }

            var parameters = new Dictionary<string, string>();
            record.Parameters.ForEach(rec => parameters.Add(rec.Name, rec.Value));

            return new JobData
            {
                Job = job,
                InvocationData = invocationData,
                State = record.StateName,
                CreatedAt = record.CreatedAt,
                LoadException = loadException,
                ParametersSnapshot = parameters
            };
        }

        public string GetJobParameter(string id, string name)
        {
            MemoryCacheHelper.GetJobById(id, out TableHangFireJob? record);
            if (record == null || (record != null && record.Parameters.Any(rec => rec.Name == name) == false))
            {
                return null;
            }
            return record!.Parameters.First(rec => rec.Name == name).Value;
        }

        public StateData GetStateData([NotNull] string jobId)
        {
            var iJobId = Convert.ToInt64(jobId);
            var stateList = MemoryCacheHelper.GetListOfItemsFromMemory<TableHangFireState>(ConstantsTableNames.State);
            var record = stateList.FirstOrDefault(rec => rec.JobId == iJobId);
            if (record == null) {
                return null;
            }

            return new StateData
            {
                Name = record.Name,
                Reason = record.Reason,
                Data = record.DeserializeData()
            };
        }

        public void Heartbeat(string serverId)
        {
            var resultList = MemoryCacheHelper.GetListOfItemsFromMemory<TableHangFireServer>(ConstantsTableNames.Server);
            var record = resultList.FirstOrDefault(rec => rec.ServerId == serverId);
            if (record == null) { return; }
            record.LastHeartBeat = DateTime.UtcNow;
            MemoryAndFileStorageMemoryCache.SetItemInCache(ConstantsTableNames.Server, resultList);
        }

        public void RemoveServer(string serverId)
        {
            var resultList = MemoryCacheHelper.GetListOfItemsFromMemory<TableHangFireServer>(ConstantsTableNames.Server);
            var filterList = new List<TableHangFireServer>();
            foreach (var server in resultList)
            {
                if (server.ServerId == serverId)
                {
                    continue;
                }
                filterList.Add(server);
            }
            MemoryAndFileStorageMemoryCache.SetItemInCache(ConstantsTableNames.Server, filterList);
        }

        public int RemoveTimedOutServers(TimeSpan timeOut)
        {
            return 0;
        }

        public void SetJobParameter(string id, string name, string value)
        {
            
        }

        public void SetRangeInHash([NotNull] string key, [NotNull] IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            var resultList = MemoryCacheHelper.GetListOfItemsFromMemory<TableHangFireHash>(ConstantsTableNames.Hash);
            foreach (var item in keyValuePairs)
            {
                if (resultList.Any(rec => rec.Key == key && rec.Field == item.Key))
                {
                    resultList.First(rec => rec.Key == key && rec.Field == item.Key).Value = item.Value;
                }
                else
                {
                    resultList.Add(new TableHangFireHash() { Key = key, Field = item.Key, Value = item.Value });
                }
            }
            MemoryAndFileStorageMemoryCache.SetItemInCache(ConstantsTableNames.Hash, resultList);
        }
    }
}
