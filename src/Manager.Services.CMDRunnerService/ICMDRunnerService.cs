namespace Manager.Services.CMDRunnerService
{
    public interface ICmdRunnerService
    {
        void Restart();
        void Start();
        void Stop();
    }
}