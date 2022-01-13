// <copyright file="CommunicationEventArgs.cs" company="Legendary">
//  Copyright © 2021 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
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



