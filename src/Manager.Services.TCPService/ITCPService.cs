namespace Manager.Services.TCPService
{
    /// <summary>
    ///     Interface for the TCP Service
    /// </summary>
    public interface ITcpService
    {
        void Restart();
        void Start();
        void Stop();
    }
}