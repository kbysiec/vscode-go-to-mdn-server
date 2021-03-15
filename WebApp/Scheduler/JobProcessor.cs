using Coravel.Queuing.Interfaces;

namespace WebApp.Scheduler
{
    public class JobProcessor
    {
        private IQueue Queue { get; }
        private bool IsBusy { get; set; }

        public JobProcessor(IQueue queue)
        {
            Queue = queue;
        }

        public void QueueResourceDownloaderJob()
        {
            if (IsBusy) return;
            SetBusy(true);
            Queue.QueueInvocable<ResourceDownloaderJob>();
        }

        public void SetBusy(bool isBusy)
        {
            IsBusy = isBusy;
        }
    }
}
