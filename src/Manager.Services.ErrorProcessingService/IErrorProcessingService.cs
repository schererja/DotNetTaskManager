namespace Manager.Services.ErrorProcessingService
{
    /// <summary>
    ///     Interface for the Error Processing Service
    /// </summary>
    public interface IErrorProcessingService
    {
        void Restart();
        void Start();
        void Stop();
    }
}