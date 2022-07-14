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
    using System.Linq;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Channels;
    using System.Threading.Tasks;
    using Legendary.Core.Contracts;
    using Legendary.Engine.Contracts;

    /// <summary>
    /// Creates a new instance of a logger.
    /// </summary>
    public class Logger : ILogger
    {
        /// <inheritdoc/>
        public void Debug(string message, ICommunicator? communicator = default)
        {
            System.Diagnostics.Debug.Write(DateTime.UtcNow.ToString());
            System.Diagnostics.Debug.WriteLine($" - {message}");
        }

        /// <inheritdoc/>
        public void Info(string message, ICommunicator? communicator = default)
        {
            System.Diagnostics.Debug.Write(DateTime.UtcNow.ToString());
            System.Diagnostics.Debug.WriteLine($" - {message}");

            if (communicator != null)
            {
                SendToWiznet(message, communicator);
            }
        }

        /// <inheritdoc/>
        public void Warn(string message, Exception exception, ICommunicator? communicator = default)
        {
            System.Diagnostics.Debug.Write(DateTime.UtcNow.ToString());
            System.Diagnostics.Debug.WriteLine($" - {message} - {exception}");

            if (communicator != null)
            {
                SendToWiznet($"{message} - {exception.Message}", communicator);
            }
        }

        /// <inheritdoc/>
        public void Warn(string message, ICommunicator? communicator = default)
        {
            System.Diagnostics.Debug.Write(DateTime.UtcNow.ToString());
            System.Diagnostics.Debug.WriteLine($" - {message}");

            if (communicator != null)
            {
                SendToWiznet(message, communicator);
            }
        }

        /// <inheritdoc/>
        public void Error(string message, ICommunicator? communicator = default)
        {
            System.Diagnostics.Debug.Write(DateTime.UtcNow.ToString());
            System.Diagnostics.Debug.WriteLine($" - {message}");

            if (communicator != null)
            {
                SendToWiznet(message, communicator);
            }
        }

        /// <inheritdoc/>
        public void Error(string message, Exception exception, ICommunicator? communicator = default)
        {
            System.Diagnostics.Debug.Write(DateTime.UtcNow.ToString());
            System.Diagnostics.Debug.WriteLine($" - {message} - {exception}");

            if (communicator != null)
            {
                SendToWiznet($"{message} - {exception.Message}", communicator);
            }
        }

        /// <inheritdoc/>
        public void Error(Exception exception, ICommunicator? communicator = default)
        {
            System.Diagnostics.Debug.Write(DateTime.UtcNow.ToString());
            System.Diagnostics.Debug.WriteLine($" - {exception}");

            if (communicator != null)
            {
                SendToWiznet(exception.Message.ToString(), communicator);
            }
        }

        /// <summary>
        /// Sends a message to Wiznet.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="communicator">The communicator instance.</param>
        private static void SendToWiznet(string message, ICommunicator communicator)
        {
            var channel = communicator.Channels.FirstOrDefault(c => c.Name.ToLower() == "wiznet");
            if (channel != null)
            {
                communicator.SendToChannel(channel, string.Empty, $"<span class='wizmessage'><i>WIZNET</i>: {DateTime.UtcNow} - {message}</span>");
            }
        }
    }
}
