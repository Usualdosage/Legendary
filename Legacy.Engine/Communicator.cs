// <copyright file="Communicator.cs" company="Legendary™">
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
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.WebSockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Core.Types;
    using Legendary.Data.Contracts;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Models;
    using Legendary.Engine.Models.Skills;
    using Legendary.Engine.Models.Spells;
    using Legendary.Engine.Processors;
    using Legendary.Engine.Types;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Handles communication between the engine and connected sockets.
    /// </summary>
    public class Communicator : ICommunicator, IDisposable
    {
        private readonly RequestDelegate requestDelegate;
        private readonly LanguageGenerator languageGenerator;
        private readonly ILogger logger;
        private readonly IApiClient apiClient;
        private readonly IDataService dataService;
        private readonly IEngine engine;
        private readonly IRandom random;
        private readonly IWorld world;
        private SkillProcessor? skillProcessor;
        private SpellProcessor? spellProcessor;
        private ActionProcessor? actionProcessor;
        private IEnvironment? environment;
        private KeyValuePair<string, UserData> connectedUser;

        /// <summary>
        /// Initializes a new instance of the <see cref="Communicator"/> class.
        /// </summary>
        /// <param name="requestDelegate">RequestDelegate.</param>
        /// <param name="logger">ILogger.</param>
        /// <param name="apiClient">The api client.</param>
        /// <param name="dataService">The data service.</param>
        /// <param name="engine">Singleton instance of the game engine.</param>
        /// <param name="world">The world to use in this comm instance.</param>
        public Communicator(RequestDelegate requestDelegate, ILogger logger, IApiClient apiClient, IDataService dataService, IEngine engine, IWorld world)
        {
            this.logger = logger;
            this.requestDelegate = requestDelegate;
            this.apiClient = apiClient;
            this.dataService = dataService;
            this.engine = engine;
            this.world = world;

            // Create the random generator for weather and user effects.
            this.random = new Random();

            // Add public channels.
            this.Channels = new List<CommChannel>()
                {
                    new CommChannel("pray", false, true),
                    new CommChannel("newbie", false, true),
                    new CommChannel("wiznet", true, false),
                };

            // Create the language generator.
            this.languageGenerator = new LanguageGenerator(this.random);

            this.engine.Tick += this.Engine_Tick;
            this.engine.VioTick += this.Engine_VioTick;
        }

        /// <summary>
        /// Gets a concurrent dictionary of all currently connected sockets.
        /// </summary>
        public static ConcurrentDictionary<string, UserData>? Users { get; private set; } = new ConcurrentDictionary<string, UserData>();

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

            CancellationToken cancellationToken = context.RequestAborted;
            WebSocket currentSocket = await context.WebSockets.AcceptWebSocketAsync();
            var socketId = Guid.NewGuid().ToString();

            // Ensure user has authenticated
            if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
            {
                var ip = context.Request.HttpContext.Connection.RemoteIpAddress;
                string? user = context.User.Identity?.Name;

                // Load the character by name.
                var character = await this.dataService.FindCharacter(c => c.FirstName == user);

                if (character == null)
                {
                    string message = $"{DateTime.UtcNow}: {user} ({socketId}) {ip} was not found.";
                    this.logger.Info(message);
                    await this.Wizlog(message);
                    throw new Exception(message);
                }

                var userData = new UserData(socketId, currentSocket, user ?? "Unknown", character);

                // If the user is already connected, remove, and then re-add.
                var connectedUser = Users?.FirstOrDefault(u => u.Value.Username == character.FirstName);
                if (connectedUser?.Value != null)
                {
                    string message = $"{DateTime.UtcNow}: {user} ({socketId}) had a zombie connection. Removing old connection.";
                    await this.Wizlog(message, cancellationToken);
                    this.logger.Info(message);
                    await this.SendToPlayer(connectedUser.Value.Value.Connection, "You have logged in from another location. Disconnecting. Bye!");
                    await this.Quit(connectedUser.Value.Value.Connection, connectedUser.Value.Value.Character.FirstName);
                    Users?.TryRemove(connectedUser.Value);
                }

                Users?.TryAdd(socketId, userData);

                string msg = $"{DateTime.UtcNow}: {user} ({socketId}) has connected from {ip}.";
                await this.Wizlog(msg, cancellationToken);
                this.logger.Info(msg);

                // Update the user metrics
                await this.UpdateMetrics(userData, ip.ToString());

                // Display the welcome content.
                await this.ShowWelcomeScreen(userData);

                // Add the user to public channels.
                this.AddToChannels(socketId, userData);

                // Show the room to the player.
                await this.ShowRoomToPlayer(userData, cancellationToken);

                this.connectedUser = new KeyValuePair<string, UserData>(socketId, userData);

                // Create the environment handler for this user.
                this.environment = new Environment(this, this.random, this.connectedUser);

                while (true)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    // Handle input from socket.
                    var response = await this.ReceiveStringAsync(userData, cancellationToken);

                    if (string.IsNullOrEmpty(response))
                    {
                        if (currentSocket.State != WebSocketState.Open)
                        {
                            break;
                        }

                        continue;
                    }
                }

                // Disconnected, remove the socket & user from the dictionary, remove from channels.
                UserData? dummy = null;
                Users?.TryRemove(socketId, out dummy);
                this.RemoveFromChannels(socketId, dummy);

                string logout = $"{DateTime.UtcNow}: {user} ({socketId}) has disconnected.";
                await this.Wizlog(logout, cancellationToken);
                this.logger.Info(logout);

                await currentSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
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
            var user = Users?.FirstOrDefault(u => u.Value.Username == player);
            if (user != null)
            {
                Users?.TryRemove(user.Value);
            }

            string message = $"{DateTime.UtcNow}: {player} has quit.";
            this.logger.Info(message);
            await this.Wizlog(message, ct);
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, $"{player} has quit.", ct);
        }

        /// <summary>
        /// Shows the information in a room to a single player.
        /// </summary>
        /// <param name="user">The connected user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task ShowRoomToPlayer(UserData user, CancellationToken cancellationToken = default)
        {
            var area = this.world.Areas.FirstOrDefault(a => a.AreaId == user.Character.Location.AreaId);

            if (area == null)
            {
                this.logger.Warn($"ShowRoomToPlayer: Null area found for user. {user} {user.Character.Location}!");
                return;
            }

            var room = area.Rooms.FirstOrDefault(r => r.RoomId == user.Character.Location.RoomId);

            if (room == null)
            {
                this.logger.Warn($"ShowRoomToPlayer: Null room found for user. {user} {user.Character.Location}!");
                return;
            }

            StringBuilder sb = new ();

            var terrainClass = room?.Terrain?.ToString().ToLower() ?? "city";

            sb.Append($"<span class='room-title {terrainClass}'>{room?.Name}</span> <span class='roomNum'>[{room?.RoomId}]</span><br/>");

            if (!string.IsNullOrWhiteSpace(room?.Image))
            {
                sb.Append($"<div class='room-image'><img src='{room?.Image}'/></div>");
            }
            else
            {
                sb.Append($"<div class='room-image room-image-none'></div>");
            }

            sb.Append($"<span class='room-description'>{room?.Description}</span><br/>");

            // Show the items
            if (room?.Items != null)
            {
                foreach (var item in room.Items)
                {
                    if (item == null)
                    {
                        this.logger.Warn($"ShowRoomToPlayer: Null item found for item!");
                        return;
                    }

                    sb.Append($"<span class='item'>{item.Name} is here.</span>");
                }
            }

            sb.Append("<span class='exits'>[ Exits: ");

            // Show the exits
            if (room?.Exits != null)
            {
                foreach (var exit in room.Exits)
                {
                    sb.Append(Enum.GetName(typeof(Direction), exit.Direction)?.ToLower() + " ");
                }
            }

            sb.Append("]</span>");

            // Show the mobiles
            if (room?.Mobiles != null)
            {
                foreach (var mob in room.Mobiles)
                {
                    if (mob == null)
                    {
                        this.logger.Warn($"ShowRoomToPlayer: Null mob found for mob!");
                        return;
                    }

                    sb.Append($"<span class='mobile'>{mob.FirstName} is standing here.</span>");
                }
            }

            // Show other players
            if (Communicator.Users != null)
            {
                foreach (var other in Communicator.Users)
                {
                    if (other.Key != user.ConnectionId &&
                        other.Value.Character.Location.AreaId == user.Character.Location.AreaId &&
                        other.Value.Character.Location.RoomId == user.Character.Location.RoomId)
                    {
                        sb.Append($"<span class='player'>{other.Value.Character.FirstName} is here.</span>");
                    }
                }
            }

            await this.SendToPlayer(user.Connection, sb.ToString(), cancellationToken);

            // Update player stats
            await this.ShowPlayerInfo(user, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task ShowPlayerInfo(UserData user, CancellationToken cancellationToken = default)
        {
            StringBuilder sb = new ();

            sb.Append("<div class='player-info'><table><tr><td colspan='2'>");
            sb.Append($"<span class='player-title'>{user.Character.FirstName} {user.Character.LastName}</span></td></tr>");

            // Health bar
            double healthPct = (user.Character.Health.Current / user.Character.Health.Max) * 100;
            sb.Append($"<tr><td>Health</td><td><progress id='health' max='100' value='{healthPct}'>{healthPct}%</progress></td></tr>");

            // Mana bar
            double manaPct = (user.Character.Mana.Current / user.Character.Mana.Max) * 100;
            sb.Append($"<tr><td>Mana</td><td><progress id='mana' max='100' value='{manaPct}'>{manaPct}%</progress></td></tr>");

            // Movement bar
            double movePct = (user.Character.Movement.Current / user.Character.Movement.Max) * 100;
            sb.Append($"<tr><td>Move</td><td><progress id='move' max='100' value='{movePct}'>{movePct}%</progress></td></tr>");

            // Condition
            sb.Append($"<tr><td colspan='2' class='condition'>You are in perfect health.</td></tr>");

            sb.Append("</table></div>");

            await this.SendToPlayer(user.Connection, sb.ToString(), cancellationToken);
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

        /// <summary>
        /// Sends a command to the server automatically from the given user.
        /// </summary>
        /// <param name="userData">UserData.</param>
        /// <param name="command">The command to send.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task SendToServer(UserData userData, string command, CancellationToken cancellationToken)
        {
            await this.OnInputReceived(userData, new CommunicationEventArgs(userData.ConnectionId, command), cancellationToken);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Saves the character to disk.
        /// </summary>
        /// <param name="userData">The user data.</param>
        /// <returns>Task.</returns>
        public async Task SaveCharacter(UserData userData)
        {
            await this.dataService.SaveCharacter(userData.Character);
        }

        /// <inheritdoc/>
        public void AddToChannel(string channelName, string socketId, UserData user)
        {
            foreach (var channel in this.Channels)
            {
                if (channel.Name.ToLower() == channelName && channel.CanUnsubscribe)
                {
                    channel.AddUser(socketId, user);
                    break;
                }
            }
        }

        /// <inheritdoc/>
        public void RemoveFromChannel(string channelName, string socketId, UserData user)
        {
            foreach (var channel in this.Channels)
            {
                if (channel.Name.ToLower() == channelName && channel.CanUnsubscribe)
                {
                    channel.RemoveUser(socketId);
                    break;
                }
            }
        }

        /// <inheritdoc/>
        public bool IsSubscribed(string channelName, string socketId, UserData user)
        {
            foreach (var channel in this.Channels)
            {
                if (channel.Name.ToLower() == channelName)
                {
                    return channel.Subscribers.Any(u => u.Value == user);
                }
            }

            return false;
        }

        /// <inheritdoc/>
        public Room? GetRoom(Room location)
        {
            var area = this.world.Areas.FirstOrDefault(a => a.AreaId == location.AreaId);
            return area?.Rooms.FirstOrDefault(r => r.RoomId == location.RoomId);
        }

        /// <inheritdoc/>
        public List<Mobile>? GetMobilesInRoom(Room location)
        {
            var room = this.GetRoom(location);
            return room?.Mobiles.ToList();
        }

        /// <inheritdoc/>
        public List<Item>? GetItemsInRoom(Room location)
        {
            var room = this.GetRoom(location);
            return room?.Items.ToList();
        }

        /// <summary>
        /// Logs a message to wiznet.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        protected async Task<CommResult> Wizlog(string message, CancellationToken cancellationToken = default)
        {
            var comm = this.Channels.FirstOrDefault(c => c.Name.ToLower() == "wiznet");

            if (comm != null && !comm.IsMuted)
            {
                foreach (var sub in comm.Subscribers)
                {
                    await this.SendToPlayer(sub.Value.Connection, $"<span class='wizmessage'>{DateTime.UtcNow}: {message}</span>", cancellationToken);
                }
            }

            return CommResult.Ok;
        }

        /// <summary>
        /// Raises the InputReceived event.
        /// </summary>
        /// <param name="sender">The sender of the message (userdata).</param>
        /// <param name="e">CommunicationEventArgs.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        protected virtual async Task OnInputReceived(object sender, CommunicationEventArgs e, CancellationToken cancellationToken = default)
        {
            var user = Users?.FirstOrDefault(u => u.Key == e.SocketId);
            if (user != null && user.HasValue)
            {
                await this.ProcessMessage(user.Value.Value, e.Message, cancellationToken);
            }
        }

        /// <summary>
        /// Processes the user's command.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="input">The input.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        private async Task ProcessMessage(UserData user, string? input, CancellationToken cancellationToken = default)
        {
            if (input == null)
            {
                return;
            }

            if (user.Character.CharacterFlags.Contains(CharacterFlags.Sleeping) && input.Trim().ToLower() != "wake")
            {
                await this.SendToPlayer(user.Connection, "You can't do that while you're asleep.", cancellationToken);
                return;
            }

            // Encode the string, otherwise the player can input HTML and have it actually render.
            input = HttpUtility.HtmlEncode(input);

            string[] args = input.Split(' ');

            // See if this is a single emote
            var emote = Emotes.Get(args[0]);

            if (emote != null)
            {
                await this.SendToPlayer(user.Connection, emote.ToSelf, cancellationToken);
                await this.SendToRoom(user.Character.Location, user.ConnectionId, emote.ToRoom.Replace("{0}", user.Character.FirstName), cancellationToken);
            }
            else
            {
                // Parse the command and see if the player is using one of their skills.
                var command = args[0].ToLower();

                if (user.Character.HasSkill(command))
                {
                    if (this.skillProcessor != null)
                    {
                        await this.skillProcessor.DoSkill(args, command, cancellationToken);
                        return;
                    }
                    else
                    {
                        await this.SendToPlayer(user.Connection, "You don't know how to do that.", cancellationToken);
                        return;
                    }
                }
                else if (this.IsCasting(command))
                {
                    if (user.Character.HasSpell(args[1]))
                    {
                        if (this.spellProcessor != null)
                        {
                            await this.spellProcessor.DoSpell(args, command, cancellationToken);
                            return;
                        }
                        else
                        {
                            await this.SendToPlayer(user.Connection, "You don't know how to cast that.", cancellationToken);
                            return;
                        }
                    }
                }
                else
                {
                    if (this.actionProcessor != null)
                    {
                        await this.actionProcessor.DoAction(args, command, cancellationToken);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Checks to see if this player is casting something.
        /// </summary>
        /// <param name="command">The first argument.</param>
        /// <returns>True if casting.</returns>
        private bool IsCasting(string command)
        {
            return command switch
            {
                "c" or "ca" or "cas" or "cast" or "co" or "com" or "comm" or "commu" or "commun" or "commune" => true,
                _ => false
            };
        }

        /// <summary>
        /// Updates the user's metrics on login.
        /// </summary>
        /// <param name="userData">The user to update.</param>
        /// <param name="ipAddress">The remote IP address.</param>
        /// <returns>Task.</returns>
        private async Task UpdateMetrics(UserData userData, string ipAddress)
        {
            var metrics = userData.Character.Metrics;

            // Log the IP address to the character
            if (!metrics.IPAddresses.Contains(ipAddress.ToString()))
            {
                metrics.IPAddresses.Add(ipAddress.ToString());
            }

            // Update last login.
            metrics.LastLogin = DateTime.UtcNow;

            // Give any user the Recall skill at max percentage if they don't have it.
            if (!userData.Character.HasSkill("recall"))
            {
                userData.Character.Skills.Add(new SkillProficiency(nameof(Recall), 100));
            }

            // TODO: Remove this after testing.
            if (!userData.Character.HasSpell("fireball"))
            {
                userData.Character.Spells.Add(new SpellProficiency(nameof(Fireball), 75));
            }

            userData.Character.Metrics = metrics;

            // Create instances of the skill and spell processors.
            this.skillProcessor = new SkillProcessor(userData, this, this.random, new Combat(this.random));
            this.spellProcessor = new SpellProcessor(userData, this, this.random, new Combat(this.random));
            this.actionProcessor = new ActionProcessor(userData, this, this.world, this.logger);

            // Make sure we have populated character flags
            if (userData.Character.CharacterFlags == null)
            {
                userData.Character.CharacterFlags = new List<CharacterFlags>();
            }

            // Save the changes.
            await this.SaveCharacter(userData);
        }

        /// <summary>
        /// Occurs each second.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private async void Engine_VioTick(object? sender, EventArgs e)
        {
            // var engineEventArgs = (EngineEventArgs)e;

            // TODO: Handle combat here.
        }

        /// <summary>
        /// Occurs every 30 seconds.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private async void Engine_Tick(object? sender, EventArgs e)
        {
            try
            {
                var engineEventArgs = (EngineEventArgs)e;

                if (this.environment != null)
                {
                    await this.environment.ProcessEnvironmentChanges(engineEventArgs.GameTicks, engineEventArgs.GameHour);
                }

                if (this.connectedUser.Value != null)
                {
                    await this.ShowPlayerInfo(this.connectedUser.Value);

                    // Autosave the user each tick.
                    await this.SaveCharacter(this.connectedUser.Value);
                }
            }
            catch (Exception)
            {
                this.logger.Warn("Attempted to send information to disconnected sockets. Continuing.");
            }
        }

        /// <summary>
        /// Receives a message from a connected socket.
        /// </summary>
        /// <param name="userData">UserData.</param>
        /// <param name="cancellationToken">CancellationToken.</param>
        /// <returns>Task.</returns>
        private async Task<string?> ReceiveStringAsync(UserData userData, CancellationToken cancellationToken = default)
        {
            var buffer = new ArraySegment<byte>(new byte[8192]);

            using var ms = new MemoryStream();
            WebSocketReceiveResult result;
            do
            {
                cancellationToken.ThrowIfCancellationRequested();
                result = await userData.Connection.ReceiveAsync(buffer, cancellationToken);
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

            await this.OnInputReceived(userData, new CommunicationEventArgs(userData.ConnectionId, message), cancellationToken);

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

        /// <summary>
        /// Adds user to all public channels.
        /// </summary>
        /// <param name="socketId">The socket Id.</param>
        /// <param name="user">The user.</param>
        private void AddToChannels(string socketId, UserData user)
        {
            foreach (var channel in this.Channels)
            {
                if (channel.IsPublic)
                {
                    channel.AddUser(socketId, user);
                }
            }
        }

        /// <summary>
        /// Removes user from all public channels.
        /// </summary>
        /// <param name="socketId">The socket Id.</param>
        /// <param name="user">The user.</param>
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
