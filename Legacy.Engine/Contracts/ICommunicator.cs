// <copyright file="ICommunicator.cs" company="Legendary">
//  Copyright © 2021-2022 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Engine.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core.Models;
    using Legendary.Engine.Types;
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
        /// When invoked, handles adding/removing sockets.
        /// </summary>
        /// <param name="context">The HttpContext.</param>
        /// <returns>Task.</returns>
        Task Invoke(HttpContext context);

        /// <summary>
        /// Sends a global message to all connected sockets.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="ct">CancellationToken.</param>
        /// <returns>Task with result.</returns>
        Task<CommResult> SendGlobal(string message, CancellationToken ct = default);

        /// <summary>
        /// Sends a message to a specified socket.
        /// </summary>
        /// <param name="socket">The socket to send to.</param>
        /// <param name="message">The message.</param>
        /// <param name="ct">CancellationToken.</param>
        /// <returns>Task with result.</returns>
        Task<CommResult> SendToPlayer(WebSocket socket, string message, CancellationToken ct = default);

        /// <summary>
        /// Sends a message to a specified target.
        /// </summary>
        /// <param name="socket">The socket to send to.</param>
        /// <param name="target">The target name to send to.</param>
        /// <param name="message">The message.</param>
        /// <param name="ct">CancellationToken.</param>
        /// <returns>Task with result.</returns>
        Task<CommResult> SendToPlayer(WebSocket socket, string target, string message, CancellationToken ct = default);

        /// <summary>
        /// Sends a message to everyone in the room, EXCEPT the sender.
        /// </summary>
        /// <param name="room">The room.</param>
        /// <param name="socketId">The socket ID of the sender.</param>
        /// <param name="message">The message to send.</param>
        /// <param name="ct">CancellationToken.</param>
        /// <returns>Task with result.</returns>
        Task<CommResult> SendToRoom(Room room, string socketId, string message, CancellationToken ct = default);

        /// <summary>
        /// Sends a message to everyone in an area, EXCEPT the sender.
        /// </summary>
        /// <param name="room">The room.</param>
        /// <param name="socketId">The socket ID of the sender.</param>
        /// <param name="message">The message to send.</param>
        /// <param name="ct">CancellationToken.</param>
        /// <returns>Task with result.</returns>
        Task<CommResult> SendToArea(Room room, string socketId, string message, CancellationToken ct = default);

        /// <summary>
        /// Sends a message to a channel.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="socketId">The player socket.</param>
        /// <param name="message">The message.</param>
        /// <param name="ct">CancellationToken.</param>
        /// <returns>Task with result.</returns>
        Task<CommResult> SendToChannel(CommChannel? channel, string socketId, string message, CancellationToken ct = default);

        /// <summary>
        /// Disconnects the connected player.
        /// </summary>
        /// <param name="socket">The player socket.</param>
        /// <param name="player">The player name.</param>
        /// <param name="ct">CancellationToken.</param>
        /// <returns>Task.</returns>
        Task Quit(WebSocket socket, string? player, CancellationToken ct = default);
    }
}



