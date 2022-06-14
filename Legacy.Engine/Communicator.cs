// <copyright file="Communicator.cs" company="Legendary">
//  Copyright © 2021 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Engine
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.WebSockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Models;
    using Legendary.Engine.Types;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Handles communication between the engine and connected sockets.
    /// </summary>
    public class Communicator : ICommunicator, IDisposable
    {
        private readonly RequestDelegate requestDelegate;
        private readonly ILogger logger;
        private readonly IApiClient apiClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="Communicator"/> class.
        /// </summary>
        /// <param name="requestDelegate">RequestDelegate.</param>
        /// <param name="logger">ILogger.</param>
        /// <param name="apiClient">The api client.</param>
        /// <param name="world">The world.</param>
        public Communicator(RequestDelegate requestDelegate, ILogger logger, IApiClient apiClient, IWorld world)
        {
            this.logger = logger;
            this.requestDelegate = requestDelegate;
            this.apiClient = apiClient;
            this.World = world;

            // Add public channels.
            this.Channels = new List<CommChannel>()
                {
                    new CommChannel("pray", false),
                    new CommChannel("newbie", false),
                };
        }

        /// <inheritdoc/>
        public event EventHandler? InputReceived;

        /// <summary>
        /// Gets a concurrent dictionary of all currently connected sockets.
        /// </summary>
        public static ConcurrentDictionary<string, UserData>? Users { get; private set; } = new ConcurrentDictionary<string, UserData>();

        /// <inheritdoc/>
        public IWorld World { get; internal set; }

        /// <summary>
        /// Gets the communication channels.
        /// </summary>
        public IList<CommChannel> Channels { get; private set; }

        /// <inheritdoc/>
        public async Task Invoke(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await this.requestDelegate.Invoke(context);
                return;
            }

            CancellationToken ct = context.RequestAborted;
            WebSocket currentSocket = await context.WebSockets.AcceptWebSocketAsync();
            var socketId = Guid.NewGuid().ToString();

            // Ensure user has authenticated
            if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
            {
                var ip = context.Request.HttpContext.Connection.RemoteIpAddress;
                string? user = context.User.Identity?.Name;

                // Load the character by name 
                var character = await this.World.FindCharacter(c => c.FirstName == user);

                if (character == null)
                {
                    await this.logger.Warn($"{DateTime.UtcNow}: {user} ({socketId}) {ip} was not found.");
                    throw new Exception($"{DateTime.UtcNow}: {user} ({socketId}) {ip} was not found.");
                }

                var userData = new UserData(socketId, currentSocket, user ?? "Unknown", character);

                Users?.TryAdd(socketId, userData);

                await this.logger.Info($"{DateTime.UtcNow}: {user} ({socketId}) has connected from {ip}.");

                // Display the welcome content
                await this.ShowWelcomeScreen(userData);

                // Add the user to public channels
                this.AddToChannels(socketId, userData);

                // Force the user to run the look command
                this.SendToServer(userData, "look");

                while (true)
                {
                    if (ct.IsCancellationRequested)
                    {
                        break;
                    }

                    // Handle input from socket
                    var response = await this.ReceiveStringAsync(userData, ct);

                    if (string.IsNullOrEmpty(response))
                    {
                        if (currentSocket.State != WebSocketState.Open)
                        {
                            break;
                        }

                        continue;
                    }
                }

                // Disconnected, remove the socket & user from the dictionary, remove from channels
                UserData? dummy = null;
                Users?.TryRemove(socketId, out dummy);
                this.RemoveFromChannels(socketId, dummy);

                await this.logger.Info($"{DateTime.UtcNow}: {user} {socketId} has disconnected.");

                await currentSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct);
                currentSocket.Dispose();
            }
        }

        /// <inheritdoc/>
        public async Task<CommResult> SendGlobal(string message, CancellationToken ct = default)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(buffer);

            if (Users != null)
            {
                foreach (var user in Users)
                {
                    await user.Value.Connection.SendAsync(segment, WebSocketMessageType.Text, true, ct);
                }
            }

            return CommResult.Ok;
        }

        /// <inheritdoc/>
        public async Task<CommResult> SendToPlayer(WebSocket socket, string message, CancellationToken ct = default)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(buffer);
            await socket.SendAsync(segment, WebSocketMessageType.Text, true, ct);
            return CommResult.Ok;
        }

        /// <inheritdoc/>
        public async Task Quit(WebSocket socket, string? player, CancellationToken ct = default)
        {
            await this.logger.Info($"{DateTime.UtcNow}: {player} has quit.");
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, $"{player} has quit.", ct);
        }

        /// <inheritdoc/>
        public async Task Save(WebSocket socket, UserData userData, CancellationToken ct = default)
        {
            try
            {
                await this.World.ReplaceOneCharacterAsync(c => c.CharacterId == userData.Character.CharacterId, userData.Character);
            }
            catch (Exception exc)
            {
                await this.logger.Error($"{DateTime.UtcNow}: Error saving player information for {userData.Username}!", exc);
            }
        }

        /// <inheritdoc/>
        public async Task<CommResult> SendToPlayer(WebSocket socket, string target, string message, CancellationToken ct = default)
        {
            var user = Users?.FirstOrDefault(u => u.Value.Username == target);

            if (user?.Value?.Character != null)
            {
                var buffer = Encoding.UTF8.GetBytes(message);
                var segment = new ArraySegment<byte>(buffer);
                await user.Value.Value.Connection.SendAsync(segment, WebSocketMessageType.Text, true, ct);
                return CommResult.Ok;
            }

            return CommResult.NotConnected;
        }

        /// <inheritdoc/>
        public async Task<CommResult> SendToRoom(Room room, string socketId, string message, CancellationToken ct = default)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(buffer);

            if (Users != null)
            {
                foreach (var user in Users)
                {
                    if (user.Key != socketId && user.Value.Character.Location.Equals(room))
                    {
                        await user.Value.Connection.SendAsync(segment, WebSocketMessageType.Text, true, ct);
                    }
                }
            }

            return CommResult.Ok;
        }

        /// <inheritdoc/>
        public async Task<CommResult> SendToArea(Room room, string socketId, string message, CancellationToken ct = default)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(buffer);

            if (Users != null)
            {
                foreach (var user in Users)
                {
                    if (user.Key != socketId && user.Value.Character.Location.AreaId == room.AreaId)
                    {
                        await user.Value.Connection.SendAsync(segment, WebSocketMessageType.Text, true, ct);
                    }
                }
            }

            return CommResult.Ok;
        }

        /// <inheritdoc/>
        public async Task<CommResult> SendToChannel(CommChannel? channel, string socketId, string message, CancellationToken ct = default)
        {
            var comm = this.Channels.FirstOrDefault(c => c.Name.ToLower() == channel?.Name.ToLower());

            if (comm != null && !comm.IsMuted)
            {
                foreach (var sub in comm.Subscribers)
                {
                    if (sub.Value.ConnectionId != socketId)
                    {
                        await this.SendToPlayer(sub.Value.Connection, message, ct);
                    }
                }
            }

            return CommResult.Ok;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Raises the InputReceived event.
        /// </summary>
        /// <param name="sender">The sender of the message (userdata).</param>
        /// <param name="e">CommunicationEventArgs.</param>
        protected virtual void OnInputReceived(object sender, CommunicationEventArgs e)
        {
            EventHandler? handler = this.InputReceived;
            handler?.Invoke(sender, e);
        }

        /// <summary>
        /// Sends a command to the server automatically from the given user.
        /// </summary>
        /// <param name="userData">UserData.</param>
        /// <param name="command">The command to send.</param>
        private void SendToServer(UserData userData, string command)
        {
            this.OnInputReceived(userData, new CommunicationEventArgs(userData.ConnectionId, command));
        }

        /// <summary>
        /// Receives a message from a connected socket.
        /// </summary>
        /// <param name="userData">UserData.</param>
        /// <param name="ct">CancellationToken.</param>
        /// <returns>Task.</returns>
        private async Task<string?> ReceiveStringAsync(UserData userData, CancellationToken ct = default)
        {
            var buffer = new ArraySegment<byte>(new byte[8192]);

            using var ms = new MemoryStream();
            WebSocketReceiveResult result;
            do
            {
                ct.ThrowIfCancellationRequested();
                result = await userData.Connection.ReceiveAsync(buffer, ct);
                if (buffer.Array != null)
                {
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                }
            }
            while (!result.EndOfMessage);

            ms.Seek(0, SeekOrigin.Begin);
            if (result.MessageType != WebSocketMessageType.Text)
            {
                return null;
            }

            // Encoding UTF8: https://tools.ietf.org/html/rfc6455#section-5.6
            using var reader = new StreamReader(ms, Encoding.UTF8);
            var message = await reader.ReadToEndAsync();

            this.OnInputReceived(userData, new CommunicationEventArgs(userData.ConnectionId, message));

            return message;
        }

        /// <summary>
        /// Displays the welcome screen to the user.
        /// </summary>
        /// <param name="user">The player's data.</param>
        /// <param name="ct">CancellationToken.</param>
        private async Task ShowWelcomeScreen(UserData user, CancellationToken ct = default)
        {
            var content = await this.apiClient.GetContent($"welcome?playerName={user.Character.FirstName}");
            if (content != null)
            {
                await this.SendToPlayer(user.Connection, content, ct);
            }

            await this.SendToRoom(user.Character.Location, user.ConnectionId, $"{user.Character.FirstName} suddenly appears.", ct);
        }

        private void AddToChannels(string socketId, UserData user)
        {
            foreach (var channel in this.Channels)
            {
                channel.AddUser(socketId, user);
            }
        }

        private void RemoveFromChannels(string socketId, UserData? user)
        {
            if (user == null)
            {
                return;
            }

            foreach (var channel in this.Channels)
            {
                channel.RemoveUser(socketId);
            }
        }
    }
}



