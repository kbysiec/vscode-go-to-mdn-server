using System;
using WebApp.Models;
using WebApp.Scheduler;

namespace WebApp.Core
{
    public class DataService
    {
        private DataRepository DataRepository { get; }
        private JobProcessor JobProcessor { get; }

        public DataService(DataRepository dataRepository, JobProcessor jobProcessor)
        {
            DataRepository = dataRepository;
            JobProcessor = jobProcessor;
        }

        public MdnData Get()
        {
            MdnData mdnData = DataRepository.GetLast();
            var today = DateTime.Today;

            if (DataRepository.Exists(today)) return mdnData;
            JobProcessor.QueueResourceDownloaderJob();

            return mdnData;
        }

        public int CountItems()
        {
            return DataRepository.Count();
        }
    }
}