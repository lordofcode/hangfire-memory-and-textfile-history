using Hangfire.Storage;

namespace HangFireExtension.CustomMemoryAndFileStorage
{
    public class CustomMemoryAndFileStorage : Hangfire.JobStorage
    {
        private readonly CustomMemoryAndFileStorageConnection _customMemoryAndFileStorageConnection;

        public CustomMemoryAndFileStorage(string fileLocation)
        { 
            _customMemoryAndFileStorageConnection = new CustomMemoryAndFileStorageConnection();
            MemoryAndFileStorageMemoryCache.SetFilePath(fileLocation);
            MemoryAndFileStorageMemoryCache.LoadHistoryFile();
        }

        public override IStorageConnection GetConnection()
        {
            return _customMemoryAndFileStorageConnection;
        }

        public override IMonitoringApi GetMonitoringApi()
        {
            return new CustomMemoryAndFileStorageMonitoringApi();
        }
    }
}
