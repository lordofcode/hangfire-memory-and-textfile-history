using Hangfire.Annotations;
using Hangfire.States;
using Hangfire.Storage;
using HangFireExtension.CustomMemoryAndFileStorage.Constants;
using HangFireExtension.CustomMemoryAndFileStorage.Helpers;
using HangFireExtension.CustomMemoryAndFileStorage.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace HangFireExtension.CustomMemoryAndFileStorage
{
    internal class MemoryAndFileStorageWriteOnlyTransaction : IWriteOnlyTransaction
    {
        //private readonly Dictionary<string, Queue<string>> _queues = new Dictionary<string, Queue<string>>();

        public void AddJobState([NotNull] string jobId, [NotNull] IState state)
        {
            SetJobState(jobId, state);
        }

        public void AddToQueue([NotNull] string queue, [NotNull] string jobId)
        {
            var resultList = MemoryCacheHelper.GetListOfItemsFromMemory<TableHangFireQueue>($"{ConstantsTableNames.QueuePrefix}{queue}");
            var record = resultList.FirstOrDefault(rec => rec.Name == queue);
            if (record == null) {
                resultList.Add(new TableHangFireQueue() { Name = queue });
                record = resultList.FirstOrDefault(rec => rec.Name == queue);
            }
            record!.Queue.Enqueue(jobId);
            MemoryAndFileStorageMemoryCache.SetItemInCache($"{ConstantsTableNames.QueuePrefix}{queue}", resultList);
        }

        public void AddToSet([NotNull] string key, [NotNull] string value)
        {
            AddToSet(key, value, 0.0);
        }

        public void AddToSet([NotNull] string key, [NotNull] string value, double score)
        {
            AddOrRemoveFromSet(true, key, value, score);
        }

        public void Commit()
        {
            MemoryCacheHelper.CommitToFile();
        }

        public void DecrementCounter([NotNull] string key)
        {
            IncrementOrDecrementCounter(key, -1);
        }

        public void DecrementCounter([NotNull] string key, TimeSpan expireIn)
        {
            IncrementOrDecrementCounter(key, -1, expireIn);
        }

        public void Dispose()
        {
            // would be neat to release some resources, do it later?
            Console.WriteLine("dispose");
        }

        public void ExpireJob([NotNull] string jobId, TimeSpan expireIn)
        {
            var jobList = MemoryCacheHelper.GetJobById(jobId, out TableHangFireJob? record);
            if (record != null)
            {
                jobList.First(rec => rec.Id == record.Id).ExpireAt = DateTime.UtcNow.Add(expireIn);
            }
            MemoryAndFileStorageMemoryCache.SetItemInCache(ConstantsTableNames.Job, jobList);
        }

        public void IncrementCounter([NotNull] string key)
        {
            IncrementOrDecrementCounter(key, 1);
        }

        public void IncrementCounter([NotNull] string key, TimeSpan expireIn)
        {
            IncrementOrDecrementCounter(key, 1, expireIn);
        }

        public void InsertToList([NotNull] string key, [NotNull] string value)
        {
            //throw new NotImplementedException();
        }

        public void PersistJob([NotNull] string jobId)
        {
            var jobList = MemoryCacheHelper.GetJobById(jobId, out TableHangFireJob? record);
            if (record == null || (record != null && record.ExpireAt == null))
            {
                return;
            }
            jobList.First(rec => rec.Id == record!.Id).ExpireAt = null;
            MemoryAndFileStorageMemoryCache.SetItemInCache(ConstantsTableNames.Job, jobList);
        }

        public void RemoveFromList([NotNull] string key, [NotNull] string value)
        {
            //throw new NotImplementedException();
        }

        public void RemoveFromSet([NotNull] string key, [NotNull] string value)
        {
            AddOrRemoveFromSet(false, key, value);
        }

        public void RemoveHash([NotNull] string key)
        {
            var resultList = MemoryCacheHelper.GetListOfItemsFromMemory<TableHangFireHash>(ConstantsTableNames.Hash);
            resultList = resultList.Where(rec => rec.Key != key).ToList();
            MemoryAndFileStorageMemoryCache.SetItemInCache(ConstantsTableNames.Hash, resultList);
        }

        public void SetJobState([NotNull] string jobId, [NotNull] IState state)
        {
            var iJobId = Convert.ToInt64(jobId);
            var stateRecord = new TableHangFireState() { JobId = iJobId, Name = state.Name, Reason = state.Reason, CreatedAt = DateTime.UtcNow, Data = System.Text.Json.JsonSerializer.Serialize(state.SerializeData()) };
            var resultList = MemoryCacheHelper.GetListOfItemsFromMemory<TableHangFireState>(ConstantsTableNames.State);
            resultList.Add(stateRecord);
            MemoryAndFileStorageMemoryCache.SetItemInCache(ConstantsTableNames.State, resultList);

            var jobList = MemoryCacheHelper.GetJobById(jobId, out TableHangFireJob? record);
            if (record != null)
            {
                jobList.First(rec => rec.Id == record.Id).StateId = stateRecord.Id;
                jobList.First(rec => rec.Id == record.Id).StateName = stateRecord.Name;
            }
            MemoryAndFileStorageMemoryCache.SetItemInCache(ConstantsTableNames.Job, jobList);
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

        public void TrimList([NotNull] string key, int keepStartingFrom, int keepEndingAt)
        {
            //throw new NotImplementedException();
        }

        private void AddOrRemoveFromSet(bool add, [NotNull] string key, [NotNull] string value, double? score = null)
        {
            var resultList = MemoryCacheHelper.GetListOfItemsFromMemory<TableHangFireSet>(ConstantsTableNames.Set);
            var record = resultList.FirstOrDefault(rec => rec.Key == key && rec.Value == value);
            if (add)
            {
                if (score == null)
                {
                    return;
                }
                else if (record != null)
                {
                    if (record.Score == score)
                    {
                        return;
                    }
                    record.Score = score.Value;
                }
                else
                {
                    resultList.Add(new TableHangFireSet() { Key = key, Value = value, Score = score.Value });
                }
            }
            else {
                if (record == null)
                {
                    return;
                }
                resultList = resultList.Where(rec => rec.Id != record.Id).ToList();
            }
            MemoryAndFileStorageMemoryCache.SetItemInCache(ConstantsTableNames.Set, resultList);
        }

        private void IncrementOrDecrementCounter([NotNull] string key, int value, TimeSpan? expireIn = null)
        {
            var counterList = MemoryCacheHelper.GetListOfItemsFromMemory<TableHangFireCounter>(ConstantsTableNames.Counter);
            counterList.Add(new TableHangFireCounter() { Key = key, Value = value, ExpireAt = expireIn.HasValue ? DateTime.UtcNow.Add(expireIn.Value) : null });
            MemoryAndFileStorageMemoryCache.SetItemInCache(ConstantsTableNames.Counter, counterList);

        }
    }
}