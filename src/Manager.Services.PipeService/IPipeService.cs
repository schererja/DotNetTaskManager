namespace Manager.Services.PipeService
{
    /// <summary>
    ///     PipeService Sub-Project Interface for Dependency Injection
    /// </summary>
    public interface IPipeService
    {
        /// <summary>
        ///     Starts a Pipe Server within
        ///     this Pipe Service for Inter-Process Communication (IPC)
        /// </summary>
        /// <param name="newPipeServer">
        ///     Is this a new Pipe Server to start?
        /// </param>
        void Start();

        /// <summary>
        ///     Stops a Pipe Server within this Pipe Service for
        ///     Inter-Process Communication (IPC)
        /// </summary>
        void Stop();

        /// <summary>
        ///     Restarts a Pipe Server within this Pipe Service for
        ///     Inter-Process Communication (IPC)
        /// </summary>
        void Restart();
    }
}