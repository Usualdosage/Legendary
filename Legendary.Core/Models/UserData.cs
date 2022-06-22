// <copyright file="UserData.cs" company="Legendary">
//  Copyright © 2021-2022 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Core.Models
{
    using System.Net.WebSockets;
    using Legendary.Core.Models;

    /// <summary>
    /// Used to track a player with their connection.
    /// </summary>
    public class UserData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserData"/> class.
        /// </summary>
        /// <param name="connectionId">The connection Id.</param>
        /// <param name="connection">The socket.</param>
        /// <param name="userName">The username.</param>
        /// <param name="character">The character.</param>
        public UserData(string connectionId, WebSocket connection, string userName, Character character)
        {
            this.ConnectionId = connectionId;
            this.Connection = connection;
            this.Username = userName;
            this.Character = character;
        }

        /// <summary>
        /// Gets the connection Id.
        /// </summary>
        public string ConnectionId { get; private set; }

        /// <summary>
        /// Gets the active user connection (socket).
        /// </summary>
        public WebSocket Connection { get; private set; }

        /// <summary>
        /// Gets the username.
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// Gets the character.
        /// </summary>
        public Character Character { get; private set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Username: {this.Username}";
        }
    }
}



