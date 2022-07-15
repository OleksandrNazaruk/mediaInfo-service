using _MediaInfoService.Models;

namespace _MediaInfoService.Extensions
{


    public static class BackgroundTaskExtensions
    {
        public static BackgroundTaskDTO ConvertToDTO(this BackgroundTask backgroundTask)
        {
            return new BackgroundTaskDTO()
            {
                ID = backgroundTask.ID,
                Status = backgroundTask.Status,
                CreatedAt = backgroundTask.CreatedAt,
                RunningAt = backgroundTask.RunningAt,
                FinishedAt = backgroundTask.FinishedAt,
                TaskSettings = backgroundTask.TaskSettings,
                Result = backgroundTask.Result,
            };
        }
    }
}
