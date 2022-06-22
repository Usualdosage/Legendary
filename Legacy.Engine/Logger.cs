// <copyright file="Logger.cs" company="Legendary">
//  Copyright © 2021-2022 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Engine
{
    using System;
    using System.Threading.Tasks;
    using Legendary.Engine.Contracts;

    /// <summary>
    /// Creates a new instance of a logger.
    /// </summary>
    public class Logger : ILogger
    {
        /// <inheritdoc/>
        public void Debug(string message)
        {
            System.Diagnostics.Debug.Write(DateTime.UtcNow.ToString());
            System.Diagnostics.Debug.WriteLine($" - {message}");
        }

        /// <inheritdoc/>
        public void Info(string message)
        {
            System.Diagnostics.Debug.Write(DateTime.UtcNow.ToString());
            System.Diagnostics.Debug.WriteLine($" - {message}");
        }

        /// <inheritdoc/>
        public void Warn(string message, Exception exception)
        {
            System.Diagnostics.Debug.Write(DateTime.UtcNow.ToString());
            System.Diagnostics.Debug.WriteLine($" - {message} - {exception}");
        }

        /// <inheritdoc/>
        public void Warn(string message)
        {
            System.Diagnostics.Debug.Write(DateTime.UtcNow.ToString());
            System.Diagnostics.Debug.WriteLine($" - {message}");
        }

        /// <inheritdoc/>
        public void Error(string message)
        {
            System.Diagnostics.Debug.Write(DateTime.UtcNow.ToString());
            System.Diagnostics.Debug.WriteLine($" - {message}");
        }

        /// <inheritdoc/>
        public void Error(string message, Exception exception)
        {
            System.Diagnostics.Debug.Write(DateTime.UtcNow.ToString());
            System.Diagnostics.Debug.WriteLine($" - {message} - {exception}");
        }

        /// <inheritdoc/>
        public void Error(Exception exception)
        {
            System.Diagnostics.Debug.Write(DateTime.UtcNow.ToString());
            System.Diagnostics.Debug.WriteLine($" - {exception}");
        }
    }
}




