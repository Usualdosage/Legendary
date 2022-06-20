// <copyright file="ILogger.cs" company="Legendary">
//  Copyright © 2021 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

using System;
using System.Threading.Tasks;

namespace Legendary.Engine.Contracts
{
    /// <summary>
    /// Implementation contract for a logger.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>Task.</returns>
        void Debug(string message);

        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>Task.</returns>
        void Info(string message);

        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>Task.</returns>
        void Warn(string message);

        /// <summary>
        /// Logs a message and an exception.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        /// <returns>Task.</returns>
        void Warn(string message, Exception exception);

        /// <summary>
        /// Logs a message and an exception.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        /// <returns>Task.</returns>
        void Error(string message, Exception exception);

        /// <summary>
        /// Logs an exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>Task.</returns>
        void Error(Exception exception);

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>Task.</returns>
        void Error(string message);
    }
}



