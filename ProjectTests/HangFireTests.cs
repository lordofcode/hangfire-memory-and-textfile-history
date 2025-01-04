using Hangfire;
using HangFireExtension.CustomMemoryAndFileStorage;
using Microsoft.AspNetCore.Builder;
using ProjectTests.Folder;

namespace ProjectTests
{
    public class Tests
    {
        private string SaveLocation;

        [SetUp]
        public void Setup()
        {
            SaveLocation = System.IO.Path.Combine(Environment.CurrentDirectory, "App_Data");
            if (!Directory.Exists(SaveLocation))
            {
                Directory.CreateDirectory(SaveLocation);
            }
        }

        [Test]
        public void TestRecurringJob()
        {
            var builder = WebApplication.CreateBuilder(new string[] { });

            var myStorage = new CustomMemoryAndFileStorage(SaveLocation);
            builder.Services.AddHangfire(configuration => configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_180).UseSimpleAssemblyNameTypeSerializer().UseRecommendedSerializerSettings().UseStorage<CustomMemoryAndFileStorage>(myStorage));
            builder.Services.AddHangfireServer(myStorage);

            var app = builder.Build();
            app.UseHangfireDashboard();

            RecurringJob.AddOrUpdate("check-for-maintenance", () => TestWriteToFileTask.WriteToOutput(), Hangfire.Cron.Minutely());

            // sleep for 1.5 minutes
            //System.Threading.Thread.Sleep(1000 * 90);

            app.Run();

            // our job should have executed at least 3 times
            var succeededJobs = myStorage.GetMonitoringApi().SucceededJobs(0, 100);

            Assert.True(succeededJobs.Count() > 0);    
            //Assert.Pass(succeededJobs.Count() > 0);
        }

        [Test]
        public void TestMonitoringApiOfJobs()
        {
            var builder = WebApplication.CreateBuilder(new string[] { });

            var myStorage = new CustomMemoryAndFileStorage(SaveLocation);
            builder.Services.AddHangfire(configuration => configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_180).UseSimpleAssemblyNameTypeSerializer().UseRecommendedSerializerSettings().UseStorage<CustomMemoryAndFileStorage>(myStorage));
            builder.Services.AddHangfireServer(myStorage);

            var app = builder.Build();
            app.UseHangfireDashboard();

            RecurringJob.AddOrUpdate("check-for-maintenance", () => TestWriteToFileTask.WriteToOutput(), Hangfire.Cron.Minutely());


            // our job should have executed at least 3 times
            var succeededJobs = myStorage.GetMonitoringApi().SucceededJobs(0, 100);

            Assert.True(succeededJobs.Count() > 0);
            //Assert.Pass(succeededJobs.Count() > 0);

            var statistics = myStorage.GetMonitoringApi().GetStatistics();
            Assert.True(statistics.Recurring > 0);
        }
    }
}