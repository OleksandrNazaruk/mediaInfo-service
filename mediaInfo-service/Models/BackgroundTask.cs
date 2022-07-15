using System.Text.Json.Serialization;

namespace _MediaInfoService.Models
{
    public class BackgroundTask
    {
        private Action _action;
        private CancellationTokenSource _taskToLifeTokenSource = new CancellationTokenSource();
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private Task? _task;
        public Guid _id;
        private string? _taskSettings;

        private Object? _result;

        public BackgroundTask(string? taskSettings, Action<BackgroundTask> action)
        {
            this._taskSettings = taskSettings;
            this._id = Guid.NewGuid();
            this.CreatedAt = DateTime.UtcNow;
            this._action = delegate () {
                try
                {
                    RunningAt = DateTime.UtcNow;
                    action(this);
                }
                catch
                {

                }
                finally
                {
                    FinishedAt = DateTime.UtcNow;
                    _taskToLifeTokenSource.CancelAfter(TimeSpan.FromHours(6));
                }
            };
        }
        ~BackgroundTask()
        {
            if (!_taskToLifeTokenSource.IsCancellationRequested)
                _taskToLifeTokenSource.Cancel();
            _taskToLifeTokenSource.Dispose();

            _cancellationTokenSource.Dispose();
        }

        public void OnRemoveAfterFinished(Action<object?> callback)
        {
            this._taskToLifeTokenSource.Token.Register(callback, this.ID);
        }
        public Action action
        {
            get => _action;
        }
        public CancellationTokenSource CancellationTokenSource
        {
            get => _cancellationTokenSource;
        }
        public Task? task
        {
            get => _task;
            set => _task = value;
        }

        public Guid ID { get => this._id; }
        public string Status { get => _task?.Status.ToString() ?? TaskStatus.Faulted.ToString(); }
        public DateTime? CreatedAt { get; set; }
        public DateTime? RunningAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public string? TaskSettings { get => this._taskSettings; }
        public Object? Result { get => this._result; set => this._result = value; }

    }
}

