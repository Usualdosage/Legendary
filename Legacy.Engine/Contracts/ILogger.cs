// <copyright file="ILogger.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Contracts
{
    using System;
    using Legendary.Core.Contracts;

    /// <summary>
    /// Implementation contract for a logger.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="communicator">The communicator.</param>
        void Debug(string message, ICommunicator? communicator);

        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="communicator">The communicator.</param>
        void Info(string message, ICommunicator? communicator);

        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="communicator">The communicator.</param>
        void Warn(string message, ICommunicator? communicator);

        /// <summary>
        /// Logs a message and an exception.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="communicator">The communicator.</param>
        void Warn(string message, Exception exception, ICommunicator? communicator);

        /// <summary>
        /// Logs a message and an exception.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="communicator">The communicator.</param>
        void Error(string message, Exception exception, ICommunicator? communicator);

        /// <summary>
        /// Logs an exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="communicator">The communicator.</param>
        void Error(Exception exception, ICommunicator? communicator);

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="communicator">The communicator.</param>
        void Error(string message, ICommunicator? communicator);
    }
}
