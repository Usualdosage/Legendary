// <copyright file="UserData.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Core.Models
{
    using System.Net.WebSockets;
    using Legendary.Core.Contracts;
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

        /// <summary>
        /// Gets or sets the character's environment.
        /// </summary>
        public IEnvironment? Environment { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Username: {this.Username}";
        }
    }
}
