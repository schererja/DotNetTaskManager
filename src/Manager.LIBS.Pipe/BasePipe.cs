using System;
using System.IO;
using System.Text;

namespace Manager.LIBS.Pipe
{
    public abstract class BasePipe
    {
        /// <summary>
        ///     Named Pipe Server Stream
        /// </summary>
        //private readonly PipeStream _pc;
        /// <summary>
        ///     IO Stream for reading and writing
        /// </summary>
        public Stream _ioStream;

        /// <summary>
        ///     Unicode encoding for the stream encoding
        /// </summary>
        public UnicodeEncoding _streamEncoding;

        /// <summary>
        ///     Reads the string from the stream
        /// </summary>
        /// <returns>
        ///     Returns the string from the stream buffer
        /// </returns>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="DecoderFallbackException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public string ReadString()
        {
            int len;
            // TODO: Leave as something that can be triggered, error message of some sort.
            var result = "";
            try
            {
                if (_ioStream != null &&
                    _ioStream.CanRead)
                {
                    len = _ioStream.ReadByte() * 256;
                    len += _ioStream.ReadByte();
                }
                else
                {
                    return result;
                }
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (ObjectDisposedException)
            {
                throw;
            }

            try
            {
                if (len < 0) return result;
                var inBuffer = new byte[len];

                _ioStream.Read(inBuffer, 0, len);
                result = _streamEncoding.GetString(inBuffer);
                _ioStream.Flush();
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (ArgumentOutOfRangeException)
            {
                throw;
            }
            catch (IOException)
            {
                throw;
            }
            catch (DecoderFallbackException)
            {
                throw;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (OverflowException)
            {
                throw;
            }

            return result;
        }

        /// <summary>
        ///     Writes a string to the stream
        /// </summary>
        /// <param name="outString">
        ///     Requires a string of what will be written to the stream
        /// </param>
        /// <returns>
        ///     Returns an Int of the buffer + 2
        /// </returns>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="EncoderFallbackException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="OverflowException"></exception>
        public int WriteString(string outString)
        {
            int len;
            int bufferLength;
            byte[] outBuffer;
            if (string.IsNullOrEmpty(outString) ||
                string.IsNullOrWhiteSpace(outString))
                return -1;
            try
            {
                outBuffer = _streamEncoding.GetBytes(outString);
                len = outBuffer.Length;
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (EncoderFallbackException)
            {
                throw;
            }
            catch (OverflowException)
            {
                throw;
            }

            if (len > ushort.MaxValue) len = ushort.MaxValue;
            try
            {
                _ioStream.WriteByte((byte) (len / 256));
                _ioStream.WriteByte((byte) (len & 255));
                _ioStream.Write(outBuffer, 0, len);
                _ioStream.Flush();
            }
            catch (IOException)
            {
                throw;
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (ObjectDisposedException)
            {
                throw;
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (ArgumentOutOfRangeException)
            {
                throw;
            }

            try
            {
                bufferLength = outBuffer.Length + 2;
            }
            catch (OverflowException)
            {
                throw;
            }

            return bufferLength;
        }
    }
}