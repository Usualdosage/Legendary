// <copyright file="CommChannel.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Types
{
    using System.Collections.Concurrent;
    using Legendary.Core.Models;
    using Legendary.Engine.Models;

    /// <summary>
    /// Represents a channel that users can subscribe/unscribe to.
    /// </summary>
    public class CommChannel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommChannel"/> class.
        /// </summary>
        /// <param name="name">The name of the channel.</param>
        /// <param name="canUnsubscribe">Whether a user can manually unsubscribe.</param>
        public CommChannel(string name, bool canUnsubscribe)
        {
            this.Name = name;
            this.CanUnsubscribe = canUnsubscribe;
            this.Subscribers = new ConcurrentDictionary<string, UserData>();
        }

        /// <summary>
        /// Gets the current subscribers.
        /// </summary>
        public ConcurrentDictionary<string, UserData> Subscribers { get; private set; }

        /// <summary>
        /// Gets the name of the channel.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets a value indicating whether users can manuall unsubscribe.
        /// </summary>
        public bool CanUnsubscribe { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the channel is muted or not.
        /// </summary>
        public bool IsMuted { get; set; }

        /// <summary>
        /// Adds a subscriber to the channel.
        /// </summary>
        /// <param name="socketId">The socket id.</param>
        /// <param name="user">The user.</param>
        /// <returns>True if added.</returns>
        public bool AddUser(string socketId, UserData user)
        {
            if (this.Subscribers == null)
            {
                return false;
            }

            return this.Subscribers.TryAdd(socketId, user);
        }

        /// <summary>
        /// Removes a subscriber from the channel.
        /// </summary>
        /// <param name="socketId">The socket id.</param>
        /// <returns>True if removed.</returns>
        public bool RemoveUser(string socketId)
        {
            if (this.Subscribers == null)
            {
                return false;
            }

            return this.Subscribers.TryRemove(socketId, out _);
        }
    }
}
