using System;
using System.IO;
using System.IO.Pipes;
using System.Text;

namespace Manager.LIBS.Pipe
{
    /// <inheritdoc cref="BasePipe">Inherits from BasePipe</inheritdoc>
    /// <summary>
    ///     The PipeClient Class.
    ///     Contains an abstraction to making a Pipe Client.
    /// </summary>
    public class PipeClient : BasePipe, IDisposable
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
        private NamedPipeClientStream _pc;

        /// <summary>
        ///     Basic Constructor for PipeClient, sets the pipe as core.pipe
        /// </summary>
        public PipeClient()
        {
            _pipeName = DefaultPipeName;
            _streamEncoding = new UnicodeEncoding();
        }

        /// <summary>
        ///     Constructor to allow passing of the pipe name to be used
        /// </summary>
        /// <param name="pipeName">
        ///     Requires a String of the pipeName to be used
        /// </param>
        public PipeClient(string pipeName)
        {
            _pipeName = pipeName;
            _streamEncoding = new UnicodeEncoding();
        }


        /// <inheritdoc />
        /// <summary>
        ///     Implementation of the Dispose method.
        ///     Releases all resources of IO Stream and Pipe Client
        /// </summary>
        public void Dispose()
        {
            _ioStream?.Dispose();
            _pc?.Dispose();
        }

        /// <summary>
        ///     Used to create the pipe based on the pipename
        /// </summary>
        public void CreatePipe()
        {
            try
            {
                _pc = new NamedPipeClientStream(".",
                    _pipeName, PipeDirection.InOut);
                _pc.Connect();
                //Console.Write("Attempting to connect to pipe...");
                //Console.WriteLine("Connected to pipe.");

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
        }

        /// <summary>
        ///     Used to close the Pipe
        /// </summary>
        public void ClosePipe()
        {
            Dispose();
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
                if (Environment.OSVersion.Platform == PlatformID.Win32Windows)
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
    }
}