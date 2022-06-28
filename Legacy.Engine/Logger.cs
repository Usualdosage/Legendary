// <copyright file="Logger.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
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
