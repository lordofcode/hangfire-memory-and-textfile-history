using Hangfire.Common;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using HangFireExtension.CustomMemoryAndFileStorage.Constants;
using HangFireExtension.CustomMemoryAndFileStorage.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HangFireExtension.CustomMemoryAndFileStorage
{
    internal class CustomMemoryAndFileStorageMonitoringApi : IMonitoringApi
    {
        public JobList<DeletedJobDto> DeletedJobs(int from, int count)
        {
            var result = new List<KeyValuePair<string, DeletedJobDto>>();
            var jobs = GetStateItems(ConstantsStateNames.Deleted);
            foreach (var jobItem in jobs)
            {
                var job = ParseJobToRequestedJobType<DeletedJobDto>(jobItem, out InvocationData? invocationData, out JobLoadException? loadException);
                if (job == null)
                {
                    continue;
                }
                result.Add(new KeyValuePair<string, DeletedJobDto>(jobItem.Id.ToString(), job));
            }
            return new JobList<DeletedJobDto>(result);
        }

        public long DeletedListCount()
        {
            return GetStateItems(ConstantsStateNames.Deleted).Count();
        }

        public long EnqueuedCount(string queue)
        {
            return GetStateItems(ConstantsStateNames.Enqueued).Count();
        }

        /// <summary>
        /// TODO: implement
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="from"></param>
        /// <param name="perPage"></param>
        /// <returns></returns>
        public JobList<EnqueuedJobDto> EnqueuedJobs(string queue, int from, int perPage)
        {
            var result = new List<KeyValuePair<string, EnqueuedJobDto>>();
            var jobs = GetStateItems(ConstantsStateNames.Enqueued);
            foreach (var jobItem in jobs) 
            {
                var job = ParseJobToRequestedJobType<EnqueuedJobDto>(jobItem, out InvocationData? invocationData, out JobLoadException? loadException);
                if (job == null)
                {
                    continue;
                }
                result.Add(new KeyValuePair<string, EnqueuedJobDto>(jobItem.Id.ToString(), job));
            }
            return new JobList<EnqueuedJobDto>(result);
        }

        /// <summary>
        /// TODO: implement
        /// </summary>
        /// <returns></returns>
        public IDictionary<DateTime, long> FailedByDatesCount()
        {
            return new Dictionary<DateTime, long>();
        }

        public long FailedCount()
        {
            return GetStateItems(ConstantsStateNames.Failed).Count();
        }

        /// <summary>
        /// TODO implement
        /// </summary>
        /// <param name="from"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public JobList<FailedJobDto> FailedJobs(int from, int count)
        {
            var result = new List<KeyValuePair<string, FailedJobDto>>();
            var jobs = GetStateItems(ConstantsStateNames.Failed);
            foreach (var jobItem in jobs)
            {
                var job = ParseJobToRequestedJobType<FailedJobDto>(jobItem, out InvocationData? invocationData, out JobLoadException? loadException);
                if (job == null)
                {
                    continue;
                }
                result.Add(new KeyValuePair<string, FailedJobDto>(jobItem.Id.ToString(), job));
            }
            return new JobList<FailedJobDto>(result);
        }

        public long FetchedCount(string queue)
        {
            return GetStateItems(ConstantsStateNames.Fetched).Count();
        }

        /// <summary>
        /// TODO: implement
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="from"></param>
        /// <param name="perPage"></param>
        /// <returns></returns>
        public JobList<FetchedJobDto> FetchedJobs(string queue, int from, int perPage)
        {
            var result = new List<KeyValuePair<string, FetchedJobDto>>();
            var jobs = GetStateItems(ConstantsStateNames.Fetched);
            foreach (var jobItem in jobs)
            {
                var job = ParseJobToRequestedJobType<FetchedJobDto>(jobItem, out InvocationData? invocationData, out JobLoadException? loadException);
                if (job == null)
                {
                    continue;
                }
                result.Add(new KeyValuePair<string, FetchedJobDto>(jobItem.Id.ToString(), job));
            }
            return new JobList<FetchedJobDto>(result);
        }

        public StatisticsDto GetStatistics()
        {
            var jobs = MemoryCacheHelper.GetAllJobs();
            var counters = MemoryCacheHelper.GetAllCounters();
            var servers = MemoryCacheHelper.GetAllServers();
            var recurringJobCount = MemoryCacheHelper.GetAllRecurringJobs().Count();

            return new StatisticsDto()
            {
                Enqueued = jobs.Count(rec => rec.StateName == ConstantsStateNames.Enqueued),
                Failed = jobs.Count(rec => rec.StateName == ConstantsStateNames.Failed),
                Processing = jobs.Count(rec => rec.StateName == ConstantsStateNames.Processing),
                Scheduled = jobs.Count(rec => rec.StateName == ConstantsStateNames.Scheduled),
                Awaiting = jobs.Count(rec => rec.StateName == ConstantsStateNames.Awaiting),
                Succeeded = counters.Where(rec => rec.Key == ConstantsStateNames.Statistics_Succeeded).Sum(rec => rec.Value),
                Deleted = counters.Where(rec => rec.Key == ConstantsStateNames.Statistics_Deleted).Sum(rec => rec.Value),
                Servers = servers.Count(),
                Recurring = recurringJobCount
            };
        }

        /// <summary>
        /// TODO: implement
        /// </summary>
        /// <returns></returns>
        public IDictionary<DateTime, long> HourlyFailedJobs()
        {
            return new Dictionary<DateTime, long>();
        }

        /// <summary>
        /// TODO: implement
        /// </summary>
        /// <returns></returns>
        public IDictionary<DateTime, long> HourlySucceededJobs()
        {
            return new Dictionary<DateTime, long>();
        }

        /// <summary>
        /// TODO: implement
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public JobDetailsDto JobDetails(string jobId)
        {
            var jobs = MemoryCacheHelper.GetJobById(jobId, out Models.TableHangFireJob? job);
            if (job == null)
            {
                return null;
            }

            var i = InvocationData.DeserializePayload(job.InvocationData);
            return new JobDetailsDto
            {
                CreatedAt = job.CreatedAt,
                ExpireAt = job.ExpireAt,
                Job = i.DeserializeJob(),
                InvocationData = i,
                LoadException = null,
                History = new List<StateHistoryDto>(),
                Properties = new Dictionary<string, string>()
            };

        }

        public long ProcessingCount()
        {
            return GetStateItems(ConstantsStateNames.Processing).Count();
        }

        /// <summary>
        /// TODO: impelement
        /// </summary>
        /// <param name="from"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public JobList<ProcessingJobDto> ProcessingJobs(int from, int count)
        {
            var result = new List<KeyValuePair<string, ProcessingJobDto>>();
            var jobs = GetStateItems(ConstantsStateNames.Processing);
            foreach (var jobItem in jobs)
            {
                var job = ParseJobToRequestedJobType<ProcessingJobDto>(jobItem, out InvocationData? invocationData, out JobLoadException? loadException);
                if (job == null)
                {
                    continue;
                }
                result.Add(new KeyValuePair<string, ProcessingJobDto>(jobItem.Id.ToString(), job));
            }
            return new JobList<ProcessingJobDto>(result);
        }

        /// <summary>
        /// TODO: implement
        /// </summary>
        /// <returns></returns>
        public IList<QueueWithTopEnqueuedJobsDto> Queues()
        {
            return new List<QueueWithTopEnqueuedJobsDto>();
        }

        public long ScheduledCount()
        {
            return GetStateItems(ConstantsStateNames.Scheduled).Count();
        }

        /// <summary>
        /// TODO: implement
        /// </summary>
        /// <param name="from"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public JobList<ScheduledJobDto> ScheduledJobs(int from, int count)
        {
            var result = new List<KeyValuePair<string, ScheduledJobDto>>();
            var jobs = GetStateItems(ConstantsStateNames.Scheduled);
            foreach (var jobItem in jobs)
            {
                var job = ParseJobToRequestedJobType<ScheduledJobDto>(jobItem, out InvocationData? invocationData, out JobLoadException? loadException);
                if (job == null)
                {
                    continue;
                }
                result.Add(new KeyValuePair<string, ScheduledJobDto>(jobItem.Id.ToString(), job));
            }
            return new JobList<ScheduledJobDto>(result);
        }

        public IList<ServerDto> Servers()
        {
            return MemoryCacheHelper.GetAllServersAsServerDtoList();
        }

        /// <summary>
        /// TODO: implement
        /// </summary>
        /// <returns></returns>
        public IDictionary<DateTime, long> SucceededByDatesCount()
        {
            return new Dictionary<DateTime, long>();
        }

        public JobList<SucceededJobDto> SucceededJobs(int from, int count)
        {
            var result = new List<KeyValuePair<string, SucceededJobDto>>();
            var jobs = GetStateItems(ConstantsStateNames.Succeeded);
            foreach (var jobItem in jobs)
            {
                var job = ParseJobToRequestedJobType<SucceededJobDto>(jobItem, out InvocationData? invocationData, out JobLoadException? loadException);
                if (job == null)
                {
                    continue;
                }
                result.Add(new KeyValuePair<string, SucceededJobDto>(jobItem.Id.ToString(), job));
            }
            return new JobList<SucceededJobDto>(result);
        }

        public long SucceededListCount()
        {
            return GetStateItems(ConstantsStateNames.Succeeded).Count();
        }

        private List<Models.TableHangFireJob> GetStateItems(string stateName)
        {
            return MemoryCacheHelper.GetAllJobs().Where(rec => rec.StateName == stateName).ToList();
        }

        private T? ParseJobToRequestedJobType<T>(Models.TableHangFireJob job, out InvocationData? invocationData, out JobLoadException? loadException) where T : class
        {
            invocationData = InvocationData.DeserializePayload(job.InvocationData);
            Job? resultJob = null;
            loadException = null;

            try
            {
                resultJob = invocationData.DeserializeJob();
            }
            catch (JobLoadException ex)
            {
                loadException = ex;
            }

            if (typeof(T) == typeof(DeletedJobDto))
            {
                return new DeletedJobDto() { Job = resultJob, LoadException = loadException, InvocationData = invocationData } as T;
            }
            if (typeof(T) == typeof(EnqueuedJobDto))
            {
                return new EnqueuedJobDto() { Job = resultJob, LoadException = loadException, InvocationData = invocationData, State = job.StateName } as T;
            }
            if (typeof(T) == typeof(FailedJobDto))
            {
                return new FailedJobDto() { Job = resultJob, LoadException = loadException, InvocationData = invocationData } as T;
            }
            if (typeof(T) == typeof(FetchedJobDto))
            {
                return new FetchedJobDto() { Job = resultJob, LoadException = loadException, InvocationData = invocationData, State = job.StateName } as T;
            }
            if (typeof(T) == typeof(ProcessingJobDto))
            {
                return new ProcessingJobDto() { Job = resultJob, LoadException = loadException, InvocationData = invocationData } as T;
            }
            if (typeof(T) == typeof(ScheduledJobDto))
            {
                return new ScheduledJobDto() { Job = resultJob, LoadException = loadException, InvocationData = invocationData } as T;
            }
            if (typeof(T) == typeof(SucceededJobDto))
            {
                return new SucceededJobDto() { Job = resultJob, LoadException = loadException, InvocationData = invocationData } as T;
            }

            return null;
        }
    }
}
