namespace Manager.Services.TaskService
{
    /// <summary>
    ///     Interface for the Task Service
    /// </summary>
    public interface ITaskService
    {
        void Start();
        void Stop();
        void Restart();
    }
}