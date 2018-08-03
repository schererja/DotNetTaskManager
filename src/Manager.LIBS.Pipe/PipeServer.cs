using System;
using System.IO;
using System.IO.Pipes;
using System.Text;

namespace Manager.LIBS.Pipe
{
    /// <inheritdoc cref="IDisposable" />
    /// <summary>
    ///     The PipeServer Class.
    ///     Contains an abstraction to creating a Pipe Server.
    /// </summary>
    public class PipeServer : BasePipe, IDisposable
    {
        /// <summary>
        ///     Default pipe name
        /// </summary>
        private const string DefaultPipeName = "CORE";

        /// <summary>
        ///     Name for the pipe
        /// </summary>
        private readonly string _pipeName;

        /// <summary>
        ///     Named Pipe Server Stream
        /// </summary>
        private NamedPipeServerStream _pc;

        /// <summary>
        ///     Creates a basic pipe server with the name of core.pipe
        /// </summary>
        public PipeServer()
        {
            _pipeName = DefaultPipeName;
            _streamEncoding = new UnicodeEncoding();
        }

        /// <summary>
        ///     Creates a pipe server with the name of the pipeName passed
        /// </summary>
        /// <param name="pipeName">Requires a string for the pipe name</param>
        public PipeServer(string pipeName)
        {
            _pipeName = pipeName;
            _streamEncoding = new UnicodeEncoding();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Implementation of the Dispose method.
        ///     Releases all resources of IO Stream and Pipe Server
        /// </summary>
        public void Dispose()
        {
            _ioStream?.Dispose();
            _pc?.Dispose();
        }

        /// <summary>
        ///     Creates the pipe based on the string of pipeName
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="PlatformNotSupportedException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="IOException"></exception>
        public void CreatePipe()
        {
            try
            {
                _pc = new NamedPipeServerStream(_pipeName,
                    PipeDirection.InOut,
                    NamedPipeServerStream.MaxAllowedServerInstances);
                _pc.WaitForConnection();
                StreamString(_pc);
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (ArgumentOutOfRangeException)
            {
                throw;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (PlatformNotSupportedException)
            {
                throw;
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (IOException)
            {
                throw;
            }
        }

        /// <summary>
        ///     Waits for connections to the pipe server
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="IOException"></exception>
        public void WaitForConnection()
        {
            try
            {
                _pc.WaitForConnection();
            }
            catch (ObjectDisposedException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (IOException)
            {
                throw;
            }
        }

        /// <summary>
        ///     Closes out the pipe
        /// </summary>
        public void ClosePipe()
        {
            _ioStream?.Dispose();

            _pc.Dispose();
        }

        /// <summary>
        ///     Waits for the pipe to drain
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public void WaitForDrain()
        {
            try
            {
                if (_pc.CanWrite && _pc.CanRead) return;
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    _pc.WaitForPipeDrain();
                else
                    _pc.Flush();
            }

            catch (ObjectDisposedException)
            {
                throw;
            }

            catch (NotSupportedException)
            {
                throw;
            }

            catch (IOException)
            {
                throw;
            }

            catch (InvalidOperationException)
            {
                throw;
            }
        }

        /// <summary>
        ///     Sets the ioStream to the Stream
        /// </summary>
        /// <param name="ioStream">
        ///     Requires a Stream for ioStream
        /// </param>
        public void StreamString(Stream ioStream)
        {
            _ioStream = ioStream;
        }

        /// <summary>
        ///     Disconnects the current connection of the Pipe
        /// </summary>
        public void Disconnect()
        {
            try
            {
                _pc.Disconnect();
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }
}