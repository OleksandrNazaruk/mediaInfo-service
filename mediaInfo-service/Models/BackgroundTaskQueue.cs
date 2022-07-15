using System.Collections.Concurrent;
using System.Threading.Tasks.Schedulers;

namespace _MediaInfoService.Models
{
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly ILogger<BackgroundTaskQueue> _logger;

        private TaskFactory _factory;

        private readonly ConcurrentDictionary<Guid, BackgroundTask> _tasks = new ConcurrentDictionary<Guid, BackgroundTask>();
        private readonly LimitedConcurrencyLevelTaskScheduler _taskScheduler = new LimitedConcurrencyLevelTaskScheduler(Environment.ProcessorCount);

        public BackgroundTaskQueue(ILogger<BackgroundTaskQueue> logger)
        {
            this._factory = new TaskFactory(this._taskScheduler);
            this._logger = logger;
        }

        public void Enqueue(BackgroundTask backgroundTask)
        {
            if (backgroundTask == null)
            {
                throw new ArgumentNullException(nameof(backgroundTask));
            }

            if (this._tasks.TryAdd(backgroundTask.ID, backgroundTask))
            {
                this._logger?.LogInformation($"Add task with ID: { backgroundTask.ID }");

                backgroundTask.task = this._factory.StartNew(backgroundTask.action, backgroundTask.CancellationTokenSource.Token);
                backgroundTask.OnRemoveAfterFinished((object? ID) => {
                    if (this.TryDequeue((Guid)(ID ?? Guid.Empty), out BackgroundTask? _backgroundTask))
                        this._logger?.LogInformation($"Task { _backgroundTask?.ID } is removed after life time is over.");
                });
            }    
        }

        public IEnumerable<BackgroundTask> ToList()
        {
            return this._tasks.Values;
        }

        public BackgroundTask Get(Guid taskID)
        {
            return this._tasks[taskID];
        }

        public bool TryGet(Guid taskID, out BackgroundTask? backgroundTask)
        {
            try
            {
                backgroundTask = Get(taskID);
                return true;
            }
            catch
            {
                backgroundTask = null;
                return false;
            }
        }

        public BackgroundTask Dequeue(Guid taskID)
        {
            this._tasks.Remove(taskID, out BackgroundTask? backgroundTask);
            return backgroundTask;
        }

        public bool TryDequeue(Guid taskID, out BackgroundTask? backgroundTask)
        {
            try
            {
                backgroundTask = Dequeue(taskID);
                return true;
            } 
            catch
            {
                backgroundTask = null;
                return false;
            }
        }
    } 

}
