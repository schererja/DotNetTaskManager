using System;

namespace Manager.Services.PipeService
{
    /// <inheritdoc />
    /// <summary>
    ///     PipeServiceException: Exception: Class abstraction of Pipe Service
    ///     Exceptions. Implements Exception for custom Exceptions
    /// </summary>
    public class PipeServiceException : Exception
    {
        /// <inheritdoc />
        /// <summary>
        ///     PipeServiceException(): Base constructor for Pipe Service
        ///     Exceptions
        /// </summary>
        public PipeServiceException()
        {
        }

        /// <inheritdoc />
        /// <summary>
        ///     Constructor for Pipe Service Exception with pre-existing
        ///     message
        /// </summary>
        /// <param name="message">
        ///     The Pipe Service Exception message to set
        /// </param>
        public PipeServiceException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        /// <summary>
        ///     Constructor for Pipe Service Exception with pre-existing
        ///     message and pre-existing inner Exception
        /// </summary>
        /// <param name="message">
        ///     The Pipe Service Exception message to set
        /// </param>
        /// <param name="inner">
        ///     Inner Exception Message that caused a Pipe Service Exception
        /// </param>
        public PipeServiceException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}