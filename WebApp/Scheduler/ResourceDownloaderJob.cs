using System.Threading.Tasks;
using Coravel.Invocable;
using WebApp.Core;

namespace WebApp.Scheduler
{
    public class ResourceDownloaderJob: IInvocable
    {
        private ResourceDownloader ResourceDownloader { get; }
        private JobProcessor JobProcessor { get; }

        public ResourceDownloaderJob(ResourceDownloader resourceDownloader, JobProcessor jobProcessor)
        {
            ResourceDownloader = resourceDownloader;
            JobProcessor = jobProcessor;
        }

        public async Task Invoke()
        {
            await ResourceDownloader.CacheAllData();
            JobProcessor.SetBusy(false);
        }
    }
}
