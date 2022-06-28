// <copyright file="CommunicationEventArgs.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Types
{
    using System;

    /// <summary>
    /// Event arguments for when an input is received from a socket.
    /// </summary>
    public class CommunicationEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationEventArgs"/> class.
        /// </summary>
        /// <param name="socketId">The socket Id.</param>
        /// <param name="message">The message.</param>
        public CommunicationEventArgs(string socketId, string message)
        {
            this.SocketId = socketId;
            this.Message = message;
        }

        /// <summary>
        /// Gets the unique socket ID.
        /// </summary>
        public string? SocketId { get; private set; }

        /// <summary>
        /// Gets the message.
        /// </summary>
        public string? Message { get; private set; }
    }
}
