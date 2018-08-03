namespace Manager.Services.InteractiveConsoleService
{
    /// <summary>
    ///     Interface for the Interactive Console Service
    /// </summary>
    public interface IInteractiveConsoleService
    {
        void Start();
        void Stop();
        void Restart();
    }
}