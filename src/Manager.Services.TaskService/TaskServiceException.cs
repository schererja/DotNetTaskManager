using System;

namespace Manager.Services.TaskService
{
    /// <inheritdoc />
    /// <summary>
    ///     TaskServiceException: Exception: Class abstraction of Task Service
    ///     Exceptions. Implements Exception for custom Exceptions
    /// </summary>
    public class TaskServiceException : Exception
    {
        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="T:Core.Service.Task.TaskServiceException" /> class
        /// </summary>
        public TaskServiceException()
        {
        }

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="T:Core.Service.Task.TaskServiceException" />
        ///     class with pre-existing message
        /// </summary>
        /// <param name="message">
        ///     The Task Service Exception message to set
        /// </param>
        public TaskServiceException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="T:Core.Service.Task.TaskServiceException" />
        ///     class with pre-existing message
        ///     and a pre-existing inner Exception
        /// </summary>
        /// <param name="message">
        ///     The Task Service Exception message to set
        /// </param>
        /// <param name="inner">
        ///     Inner Exception Message that caused a Task Service Exception
        /// </param>
        /// s
        public TaskServiceException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}