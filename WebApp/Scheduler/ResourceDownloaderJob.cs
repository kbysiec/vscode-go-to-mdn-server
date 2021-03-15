using System.Threading.Tasks;
using Coravel.Invocable;
using WebApp.Core;

namespace WebApp.Scheduler
{
    public class ResourceDownloaderJob: IInvocable
    {
        private ResourceDownloader ResourceDownloader { get; }

        public ResourceDownloaderJob(ResourceDownloader resourceDownloader)
        {
            ResourceDownloader = resourceDownloader;
        }

        public Task Invoke()
        {
            return ResourceDownloader.CacheAllData();
        }
    }
}
