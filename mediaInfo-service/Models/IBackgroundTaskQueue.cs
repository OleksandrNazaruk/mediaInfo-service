
namespace _MediaInfoService.Models
{
    public interface IBackgroundTaskQueue
    {
        public void Enqueue(BackgroundTask backgroundTask);

        public BackgroundTask? Get(Guid taskID);
        public bool TryGet(Guid taskID, out BackgroundTask? backgroundTask);
        public BackgroundTask? Dequeue(Guid taskID);
        public bool TryDequeue(Guid taskID, out BackgroundTask? backgroundTask);

        public IEnumerable<BackgroundTask> ToList();

    }
}
