// <copyright file="ICommunicator.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Core.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core.Models;
    using Legendary.Core.Types;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Implemenation contract for a class that handles socket communication.
    /// </summary>
    public interface ICommunicator : IDisposable
    {
        /// <summary>
        /// Gets the channels for this communicator.
        /// </summary>
        IList<CommChannel> Channels { get; }

        /// <summary>
        /// Gets the language processor.
        /// </summary>
        ILanguageProcessor LanguageProcessor { get; }

        /// <summary>
        /// When invoked, handles adding/removing sockets.
        /// </summary>
        /// <param name="context">The HttpContext.</param>
        /// <returns>Task.</returns>
        Task Invoke(HttpContext context);

        /// <summary>
        /// Send a sound file to the console.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="channel">The sound channel.</param>
        /// <param name="sound">The url of the sound.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        Task PlaySound(Character user, AudioChannel channel, string sound, CancellationToken cancellationToken = default);

        /// <summary>
        /// Plays a sound to everyone in the room except the actor and the target.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <param name="target">The target.</param>
        /// <param name="sound">The sound.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        Task PlaySoundToRoom(Character actor, Character? target, string sound, CancellationToken cancellationToken = default);

        /// <summary>
        /// Resolves a given player to the connected user, given that the character is a player, and not a mobile.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <returns>UserData.</returns>
        UserData? ResolveCharacter(Character character);

        /// <summary>
        /// Gets the item by item Id.
        /// </summary>
        /// <param name="itemId">Item id.</param>
        /// <returns>Item.</returns>
        Item ResolveItem(long itemId);

        /// <summary>
        /// Gets a room by location.
        /// </summary>
        /// <param name="location">The area ID and room ID.</param>
        /// <returns>Room.</returns>
        Room ResolveRoom(KeyValuePair<long, long> location);

        /// <summary>
        /// Resolves which character someone is fighting. Could be a player or a mob.
        /// </summary>
        /// <param name="actor">The character.</param>
        /// <returns>Character.</returns>
        Character? ResolveFightingCharacter(Character actor);

        /// <summary>
        /// Sends a global message to all connected sockets.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="cancellationToken">CancellationToken.</param>
        /// <returns>Task with result.</returns>
        Task<CommResult> SendGlobal(string message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves a player to the database.
        /// </summary>
        /// <param name="userData">The player.</param>
        /// <returns>Task.</returns>
        Task SaveCharacter(UserData userData);

        /// <summary>
        /// Saves a player to the database.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <returns>Task.</returns>
        Task SaveCharacter(Character character);

        /// <summary>
        /// Shows the room to the player.
        /// </summary>
        /// <param name="user">The player.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        Task ShowRoomToPlayer(UserData user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Shows/updates the player information box.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        Task ShowPlayerInfo(UserData user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Shows a target player or mobile to the actor.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="target">The target.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        Task ShowPlayerToPlayer(UserData user, string target, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a message to a specified socket.
        /// </summary>
        /// <param name="socket">The socket to send to.</param>
        /// <param name="message">The message.</param>
        /// <param name="cancellationToken">CancellationToken.</param>
        /// <returns>Task with result.</returns>
        Task<CommResult> SendToPlayer(WebSocket socket, string message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a message to a player by resolving the socket from the character name.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="message">The message.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task with result.</returns>
        Task<CommResult> SendToPlayer(Character character, string message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Allows one target to attack another.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="player">The target name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        Task Attack(UserData user, string player, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks to see if the player advances after experience granted. If so, plays a sound and advances the level.
        /// </summary>
        /// <param name="character">The user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if player advances.</returns>
        Task<bool> CheckLevelAdvance(Character character, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a message to a specified target.
        /// </summary>
        /// <param name="socket">The socket to send to.</param>
        /// <param name="target">The target name to send to.</param>
        /// <param name="message">The message.</param>
        /// <param name="cancellationToken">CancellationToken.</param>
        /// <returns>Task with result.</returns>
        Task<CommResult> SendToPlayer(WebSocket socket, string target, string message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a message to everyone in the room, EXCEPT the sender.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="location">The room.</param>
        /// <param name="socketId">The socket ID of the sender.</param>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">CancellationToken.</param>
        /// <returns>Task with result.</returns>
        Task<CommResult> SendToRoom(Character? sender, KeyValuePair<long, long> location, string socketId, string message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a message to everyone in the room, EXCEPT the actor and target.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="actor">The sender.</param>
        /// <param name="target">The target.</param>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">CancellationToken.</param>
        /// <returns>Task with result.</returns>
        Task<CommResult> SendToRoom(KeyValuePair<long, long> location, Character actor, Character target, string message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a message to everyone in an area, EXCEPT the sender.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="socketId">The socket ID of the sender.</param>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">CancellationToken.</param>
        /// <returns>Task with result.</returns>
        Task<CommResult> SendToArea(KeyValuePair<long, long> location, string socketId, string message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a message to a channel.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="socketId">The player socket.</param>
        /// <param name="message">The message.</param>
        /// <param name="cancellationToken">CancellationToken.</param>
        /// <returns>Task with result.</returns>
        Task<CommResult> SendToChannel(CommChannel? channel, string socketId, string message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a command from the given user to the server.
        /// </summary>
        /// <param name="userData">The connected user.</param>
        /// <param name="command">The command to send.</param>
        /// <param name="cancellationToken">CancellationToken.</param>
        /// <returns>Task.</returns>
        Task SendToServer(UserData userData, string command, CancellationToken cancellationToken = default);

        /// <summary>
        /// Disconnects the connected player.
        /// </summary>
        /// <param name="socket">The player socket.</param>
        /// <param name="player">The player name.</param>
        /// <param name="cancellationToken">CancellationToken.</param>
        /// <returns>Task.</returns>
        Task Quit(WebSocket socket, string? player, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns true if the target is in the provided room.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="target">The target.</param>
        /// <returns>True if the target is in the room.</returns>
        bool IsInRoom(KeyValuePair<long, long> location, Character target);

        /// <summary>
        /// Add a user to the specified channel (by name).
        /// </summary>
        /// <param name="channelName">The channel name.</param>
        /// <param name="socketId">The socket Id.</param>
        /// <param name="user">The user.</param>
        void AddToChannel(string channelName, string socketId, UserData user);

        /// <summary>
        /// Remove the user from the channel.
        /// </summary>
        /// <param name="channelName">The channel name.</param>
        /// <param name="socketId">The socket Id.</param>
        /// <param name="user">The user.</param>
        void RemoveFromChannel(string channelName, string socketId, UserData user);

        /// <summary>
        /// Check if a user is subscribed to a channel.
        /// </summary>
        /// <param name="channelName">The channel name.</param>
        /// <param name="socketId">The socket Id.</param>
        /// <param name="user">The user.</param>
        /// <returns>True if subscribed.</returns>
        bool IsSubscribed(string channelName, string socketId, UserData user);

        /// <summary>
        /// Gets all of the mobiles currently in the given location.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns>List of mobiles.</returns>
        List<Mobile>? GetMobilesInRoom(KeyValuePair<long, long> location);

        /// <summary>
        /// Gets all players in a given location.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <param name="location">The location.</param>
        /// <returns>List of players.</returns>
        List<Character>? GetPlayersInRoom(Character actor, KeyValuePair<long, long> location);

        /// <summary>
        /// Gets all of the mobiles currently in the given area.
        /// </summary>
        /// <param name="areaId">The area.</param>
        /// <returns>List of mobiles.</returns>
        List<Mobile>? GetMobilesInArea(long areaId);

        /// <summary>
        /// Gets all of the items currently in the given location.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns>List of items.</returns>
        List<Item>? GetItemsInRoom(KeyValuePair<long, long> location);

        /// <summary>
        /// Allows mobs with personalities to communicate to characters who say things.
        /// </summary>
        /// <param name="character">The speaking character.</param>
        /// <param name="location">The location.</param>
        /// <param name="message">The character's message.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        Task CheckMobCommunication(Character character, KeyValuePair<long, long> location, string message, CancellationToken cancellationToken = default);
    }
}
