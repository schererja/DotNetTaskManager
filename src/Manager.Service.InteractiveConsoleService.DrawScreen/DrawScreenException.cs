using System;

namespace Manager.Services.InteractiveConsoleService.DrawScreen
{
    /// <inheritdoc />
    /// <summary>
    ///     Class abstraction of Draw Screen Exceptions. Implements Exception
    ///     for custom Exceptions
    /// </summary>
    public class DrawScreenException : Exception
    {
        /// <inheritdoc />
        /// <summary>
        ///     Base constructor for Draw Screen Exceptions
        /// </summary>
        public DrawScreenException()
        {
        }

        /// <inheritdoc />
        /// <summary>
        ///     Constructor for Draw Screen Exception
        ///     with pre-existing message
        /// </summary>
        /// <param name="message">
        ///     The Draw Screen Exception message to set
        /// </param>
        public DrawScreenException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        /// <summary>
        ///     Constructor for Draw Screen Exception with pre-existing
        ///     message and pre-existing inner Exception
        /// </summary>
        /// <param name="message">
        ///     The Draw Screen Exception message to set
        /// </param>
        /// <param name="inner">
        ///     Inner Exception Message that caused a Draw Screen Exception
        /// </param>
        public DrawScreenException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}