using Hangfire.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFireExtension.CustomMemoryAndFileStorage.Models
{
    internal class HangFireFetchedJob : IFetchedJob
    {
        public HangFireFetchedJob(string jobId, string queue)
        {
            JobId = jobId;
            Queue = queue;
        }

        public string JobId { get; }
        public string Queue { get; }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public void RemoveFromQueue()
        {
            //throw new NotImplementedException();
        }

        public void Requeue()
        {
            //throw new NotImplementedException();
            Console.WriteLine("we moeten opnieuw aanmaken!");
        }
    }
}
