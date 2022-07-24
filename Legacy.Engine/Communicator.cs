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
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using Legendary.Core;
    using Legendary.Core.Contracts;
    using Legendary.Core.Extensions;
    using Legendary.Core.Models;
    using Legendary.Core.Types;
    using Legendary.Data.Contracts;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Extensions;
    using Legendary.Engine.Generators;
    using Legendary.Engine.Helpers;
    using Legendary.Engine.Models;
    using Legendary.Engine.Processors;
    using Legendary.Engine.Types;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Handles communication between the engine and connected sockets.
    /// </summary>
    public class Communicator : ICommunicator, IDisposable
    {
        private readonly RequestDelegate requestDelegate;
        private readonly LanguageGenerator languageGenerator;
        private readonly Combat combat;
        private readonly ILogger logger;
        private readonly IApiClient apiClient;
        private readonly IDataService dataService;
        private readonly IEngine engine;
        private readonly IRandom random;
        private readonly IWorld world;
        private readonly IServerSettings serverSettings;
        private readonly IWebHostEnvironment webHostEnvironment;
        private SkillProcessor? skillProcessor;
        private SpellProcessor? spellProcessor;
        private ActionProcessor? actionProcessor;
        private ActionHelper actionHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="Communicator"/> class.
        /// </summary>
        /// <param name="requestDelegate">RequestDelegate.</param>
        /// <param name="webHostEnvironment">The hosting environment.</param>
        /// <param name="logger">ILogger.</param>
        /// <param name="serverSettings">The server settings.</param>
        /// <param name="apiClient">The api client.</param>
        /// <param name="dataService">The data service.</param>
        /// <param name="world">The world to use in this comm instance.</param>
        /// <param name="random">The random number generator.</param>
        public Communicator(RequestDelegate requestDelegate, IWebHostEnvironment webHostEnvironment, ILogger logger, IServerSettings serverSettings, IApiClient apiClient, IDataService dataService, IWorld world, IRandom random)
        {
            this.webHostEnvironment = webHostEnvironment;
            this.logger = logger;
            this.requestDelegate = requestDelegate;
            this.apiClient = apiClient;
            this.dataService = dataService;
            this.world = world;
            this.serverSettings = serverSettings;
            this.random = random;

            // Add public channels.
            this.Channels = new List<CommChannel>()
                {
                    new CommChannel("pray", false, true),
                    new CommChannel("newbie", false, true),
                    new CommChannel("wiznet", true, false),
                };

            // Start the engine.
            this.engine = new Engine(this.logger, this.world);
            this.engine.Tick += this.Engine_Tick;
            this.engine.VioTick += this.Engine_VioTick;

            // Create the combat processor.
            this.combat = new Combat(this, this.random, this.logger);

            // Create the language generator.
            this.languageGenerator = new LanguageGenerator(this.random);

            // Create the language processor.
            this.LanguageProcessor = new LanguageProcessor(this.logger, this.serverSettings, this.languageGenerator, this, this.random);

            // Create the action helper.
            this.actionHelper = new ActionHelper(this, this.random, this.combat);
        }

        /// <summary>
        /// Gets a concurrent dictionary of all currently connected sockets.
        /// </summary>
        public static ConcurrentDictionary<string, UserData>? Users { get; private set; } = new ConcurrentDictionary<string, UserData>();

        /// <summary>
        /// Gets or sets a concurrent dictionary of people talking to people through tells.
        /// </summary>
        public static ConcurrentDictionary<string, string> Tells { get; set; } = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Gets or sets a concurrent dictionary of people ignoring other people for this session. Item1 is the ignorer, Item2 is the ignoree. (Bob ignores Alice).
        /// </summary>
        public static List<Tuple<string, string>> Ignores { get; set; } = new List<Tuple<string, string>>();

        /// <summary>
        /// Gets the language processor.
        /// </summary>
        public ILanguageProcessor LanguageProcessor { get; private set; }

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
                    this.logger.Info(message, this);
                    throw new Exception(message);
                }

                // Configure the user and add the environment handler.
                // TODO: We may want to handle environmental changes globally.
                var userData = new UserData(socketId, currentSocket, user ?? "Unknown", character);
                var environment = new Environment(this, this.random, userData);
                userData.Environment = environment;

                // If the user is already connected, remove, and then re-add.
                var connectedUser = Users?.FirstOrDefault(u => u.Value.Username == character.FirstName);

                if (connectedUser?.Value != null)
                {
                    string message = $"{DateTime.UtcNow}: {user} ({socketId}) had a zombie connection. Removing old connection.";
                    this.logger.Info(message, this);
                    await this.SendToPlayer(connectedUser.Value.Value.Connection, "You have logged in from another location. Disconnecting. Bye!");
                    await this.Quit(connectedUser.Value.Value.Connection, connectedUser.Value.Value.Character.FirstName);
                    Users?.TryRemove(connectedUser.Value);
                }

                Users?.TryAdd(socketId, userData);

                string msg = $"{DateTime.UtcNow}: {user} ({socketId}) has connected from {ip}.";
                this.logger.Info(msg, this);

                // BUGFIX: Remove any fighting affects
                userData.Character.Fighting = null;
                userData.Character.CharacterFlags.RemoveIfExists(CharacterFlags.Fighting);

                // Update the user metrics
                await this.UpdateMetrics(userData, ip?.ToString());

                // TODO: Just all all skills and spells for now.
                this.ApplySkillsAndSpells(userData);

                // Create instances of the action, skill, and spell processors.
                // TODO: Can these be global somehow, so we don't have to create these for each character the logs in?
                this.skillProcessor = new SkillProcessor(this, this.random, this.combat, this.logger);
                this.spellProcessor = new SpellProcessor(this, this.random, this.combat, this.logger);
                this.actionProcessor = new ActionProcessor(this, this.world, this.logger, this.random, this.combat);

                // Display the welcome content.
                await this.ShowWelcomeScreen(userData);

                // Add the user to public channels.
                this.AddToChannels(socketId, userData);

                // Show the room to the player.
                await this.ShowRoomToPlayer(userData.Character, cancellationToken);

                while (true)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    try
                    {
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
                    catch
                    {
                        this.logger.Info("A socket was closed or in an error state.", this);
                        break;
                    }
                }

                // Disconnected, remove the socket & user from the dictionary, remove from channels.
                UserData? dummy = null;
                Users?.TryRemove(socketId, out dummy);
                this.RemoveFromChannels(socketId, dummy);

                string logout = $"{DateTime.UtcNow}: {user} ({socketId}) has disconnected.";
                this.logger.Info(logout, this);

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
        public async Task<CommResult> SendToPlayer(WebSocket socket, string message, CancellationToken cancellationToken = default)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(buffer);
            await socket.SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken);
            return CommResult.Ok;
        }

        /// <inheritdoc/>
        public async Task<CommResult> SendToPlayer(Character character, string message, CancellationToken cancellationToken = default)
        {
            if (character.IsNPC)
            {
                return CommResult.NotAvailable;
            }

            var userData = this.ResolveCharacter(character);
            if (userData != null)
            {
                return await this.SendToPlayer(userData.Connection, message, cancellationToken);
            }
            else
            {
                return CommResult.NotConnected;
            }
        }

        /// <inheritdoc/>
        public async Task Quit(WebSocket socket, string? player, CancellationToken cancellationToken = default)
        {
            var user = Users?.FirstOrDefault(u => u.Value.Username == player);
            if (user != null)
            {
                if (user.Value.Value.Character.CharacterFlags.Contains(CharacterFlags.Fighting))
                {
                    await this.SendToPlayer(socket, "You can't quit, you're FIGHTING!", cancellationToken);
                    return;
                }
                else
                {
                    Users?.TryRemove(user.Value);
                }
            }

            string message = $"{DateTime.UtcNow}: {player} has quit.";
            this.logger.Info(message, this);
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, $"{player} has quit.", cancellationToken);
        }

        /// <inheritdoc/>
        public bool IsInRoom(KeyValuePair<long, long> location, Character target)
        {
            if (Users != null)
            {
                return Users.Any(u => u.Value.Character.FirstName == target.FirstName && u.Value.Character.Location.InSamePlace(location));
            }

            return false;
        }

        /// <inheritdoc/>
        public async Task Attack(UserData user, string targetName, CancellationToken cancellationToken = default)
        {
            targetName = targetName.ToLower();

            if (targetName == user.Character.FirstName || targetName == "self")
            {
                await this.SendToPlayer(user.Connection, "You can't attack yourself.", cancellationToken);
            }
            else
            {
                var target = Users?.FirstOrDefault(u => u.Value.Character.FirstName?.ToLower() == targetName.ToLower());

                if (target == null || target.Value.Value == null)
                {
                    // Maybe a mobile?
                    var mobiles = this.GetMobilesInRoom(user.Character.Location);

                    if (mobiles != null)
                    {
                        var mobile = mobiles.ParseTargetName(targetName);

                        if (mobile != null)
                        {
                            await this.SendToPlayer(user.Connection, $"You attack {mobile.FirstName}!", cancellationToken);
                            await this.SendToRoom(user.Character, user.Character.Location, user.ConnectionId, $"{user.Character.FirstName} attacks {mobile.FirstName}!", cancellationToken);
                            await this.SendToArea(user.Character.Location, string.Empty, $"{mobile.FirstName} yells \"<span class='yell'>Help! I'm being attacked by {user.Character.FirstName}!</span>\"", cancellationToken);

                            // Start the fight.
                            Combat.StartFighting(user.Character, mobile);
                        }
                        else
                        {
                            await this.SendToPlayer(user.Connection, "They're not here.", cancellationToken);
                        }
                    }
                }
                else
                {
                    this.logger.Info($"{user.Character.FirstName} has attacked {target.Value.Value.Character.FirstName} in room {user.Character.Location.Value}.", this);

                    await this.SendToPlayer(user.Connection, $"You attack {target.Value.Value.Character.FirstName}!", cancellationToken);
                    await this.SendToRoom(user.Character, user.Character.Location, user.ConnectionId, $"{user.Character.FirstName} attacks {target.Value.Value.Character.FirstName}!", cancellationToken);
                    await this.SendToArea(user.Character.Location, string.Empty, $"{target.Value.Value.Character.FirstName} yells \"<span class='yell'>Help! I'm being attacked by {user.Character.FirstName}!</span>\"", cancellationToken);

                    // Start the fight.
                    Combat.StartFighting(user.Character, target.Value.Value.Character);
                }
            }
        }

        /// <inheritdoc/>
        public async Task ShowPlayerToPlayer(Character actor, string targetName, CancellationToken cancellationToken = default)
        {
            targetName = targetName.ToLower();

            // Update player stats
            await this.ShowPlayerInfo(actor, cancellationToken);

            if (targetName == actor.FirstName || targetName == "self")
            {
                await this.SendToPlayer(actor, "You look at yourself.", cancellationToken);
                await this.SendToPlayer(actor, this.GetPlayerInfo(actor), cancellationToken);
                await this.SendToRoom(actor.Location, actor, null, $"{actor.FirstName} looks at {actor.Pronoun}self.", cancellationToken);

                if (!string.IsNullOrWhiteSpace(actor.Image))
                {
                    await this.SendToPlayer(actor, $"<div class='room-image'><img src='{actor.Image}'/></div>", cancellationToken);
                }
            }
            else
            {
                var target = Users?.FirstOrDefault(u => u.Value.Character.FirstName?.ToLower() == targetName);

                if (target == null || target.Value.Value == null)
                {
                    // Maybe a mobile
                    var mobiles = this.GetMobilesInRoom(actor.Location);

                    if (mobiles != null)
                    {
                        var mobile = mobiles.ParseTargetName(targetName);

                        if (mobile != null)
                        {
                            await this.SendToPlayer(actor, $"You look at {mobile.FirstName}.", cancellationToken);
                            await this.SendToRoom(actor.Location, actor, null, $"{actor.FirstName} looks at {mobile.FirstName}.", cancellationToken);
                            await this.SendToPlayer(actor, this.GetPlayerInfo(mobile), cancellationToken);

                            if (!string.IsNullOrWhiteSpace(mobile.Image))
                            {
                                await this.SendToPlayer(actor, $"<div class='room-image'><img src='{mobile?.Image}'/></div>", cancellationToken);
                            }
                        }
                        else
                        {
                            await this.SendToPlayer(actor, "They are not here.", cancellationToken);
                        }
                    }
                }
                else
                {
                    await this.SendToPlayer(target.Value.Value.Character, $"{actor.FirstName} looks at you.", cancellationToken);
                    await this.SendToRoom(actor.Location, actor, target.Value.Value.Character, $"{actor.FirstName} looks at {target.Value.Value.Character.FirstName}.", cancellationToken);
                    await this.SendToPlayer(actor, this.GetPlayerInfo(target.Value.Value.Character), cancellationToken);

                    if (!string.IsNullOrWhiteSpace(target.Value.Value.Character.Image))
                    {
                        await this.SendToPlayer(actor, $"<div class='room-image'><img src='{target.Value.Value.Character.Image}'/></div>", cancellationToken);
                    }
                }
            }
        }

        /// <summary>
        /// Shows the information in a room to a single player.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task ShowRoomToPlayer(Character actor, CancellationToken cancellationToken = default)
        {
            var room = this.ResolveRoom(actor.Location);

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
                var itemGroups = room.Items.GroupBy(g => g.ItemId);

                foreach (var itemGroup in itemGroups)
                {
                    var item = itemGroup.First();

                    if (itemGroup.Count() == 1)
                    {
                        sb.Append($"<span class='item'>{ActionHelper.DecorateItem(item, item.ShortDescription)}</span>");
                    }
                    else
                    {
                        sb.Append($"<span class='item'>({itemGroup.Count()}) {ActionHelper.DecorateItem(item, item.ShortDescription)}</span>");
                    }
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
                        this.logger.Warn($"ShowRoomToPlayer: Null mob found for mob!", this);
                        return;
                    }

                    sb.Append($"<span class='mobile'>{mob.ShortDescription}</span>");
                }
            }

            // Show other players
            if (Communicator.Users != null)
            {
                var usersInRoom = Users.Where(u => u.Value.Character.Location.InSamePlace(actor.Location) && u.Value.Character.FirstName != actor.FirstName);

                foreach (var other in usersInRoom)
                {
                    sb.Append($"<span class='player'>{other.Value.Character.FirstName} is here.</span>");
                }
            }

            await this.SendToPlayer(actor, sb.ToString(), cancellationToken);

            // Update player stats
            await this.ShowPlayerInfo(actor, cancellationToken);

            // Play the music according to the terrain.
            if (room != null)
            {
                var soundIndex = this.random.Next(0, 4);
                await this.PlaySound(actor, AudioChannel.Background, $"../audio/music/{room.Terrain?.ToString().ToLower()}{soundIndex}.mp3", cancellationToken);

                // 40% chance to play the terrain SFX (e.g. forest, city)
                var randomSfx = this.random.Next(0, 10);

                if (randomSfx <= 4)
                {
                    await this.PlaySound(actor, AudioChannel.BackgroundSFX, $"../audio/soundfx/{room.Terrain?.ToString().ToLower()}.mp3", cancellationToken);
                }
            }
        }

        /// <inheritdoc/>
        public async Task ShowPlayerInfo(Character actor, CancellationToken cancellationToken = default)
        {
            StringBuilder sb = new ();

            sb.Append("<div class='player-info'><table><tr><td colspan='2'>");
            sb.Append($"<span class='player-title'>{actor.FirstName} {actor.LastName}</span></td></tr>");

            // Health bar
            double healthPct = (actor.Health.Current / actor.Health.Max) * 100;
            sb.Append($"<tr><td>Health</td><td><progress id='health' max='100' value='{healthPct}'>{healthPct}%</progress></td></tr>");

            // Mana bar
            double manaPct = (actor.Mana.Current / actor.Mana.Max) * 100;
            sb.Append($"<tr><td>Mana</td><td><progress id='mana' max='100' value='{manaPct}'>{manaPct}%</progress></td></tr>");

            // Movement bar
            double movePct = (actor.Movement.Current / actor.Movement.Max) * 100;
            sb.Append($"<tr><td>Move</td><td><progress id='move' max='100' value='{movePct}'>{movePct}%</progress></td></tr>");

            // Condition
            sb.Append($"<tr><td colspan='2' class='condition'>{this.combat.GetPlayerCondition(actor)}</td></tr>");

            sb.Append("</table></div>");

            await this.SendToPlayer(actor, sb.ToString(), cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<CommResult> SendToPlayer(string? sender, string target, string message, CancellationToken ct = default)
        {
            var user = Users?.FirstOrDefault(u => u.Value.Username == target);

            if (user?.Value?.Character != null)
            {
                var ignore = Ignores.FirstOrDefault(i => i.Item1 == target && i.Item2 == sender);

                if (ignore != null)
                {
                    return CommResult.Ignored;
                }

                if (user.Value.Value.Character.CharacterFlags.Contains(CharacterFlags.Sleeping))
                {
                    return CommResult.NotAvailable;
                }

                var buffer = Encoding.UTF8.GetBytes(message);
                var segment = new ArraySegment<byte>(buffer);
                await user.Value.Value.Connection.SendAsync(segment, WebSocketMessageType.Text, true, ct);
                return CommResult.Ok;
            }

            return CommResult.NotConnected;
        }

        /// <inheritdoc/>
        public async Task<CommResult> SendToRoom(Character? sender, KeyValuePair<long, long> location, string socketId, string message, CancellationToken cancellationToken = default)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(buffer);

            if (Users != null)
            {
                var usersInRoom = Users.Where(u => u.Value.Character.Location.InSamePlace(location)).ToList();

                foreach (var user in usersInRoom)
                {
                    if (user.Key != socketId)
                    {
                        await user.Value.Connection.SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken);
                    }
                }

                // Grab a random person in the room and see if they interact with any mobs in the room.
                if (sender != null && !sender.IsNPC)
                {
                    ThreadPool.QueueUserWorkItem(q =>
                    {
                        try
                        {
                            this.CheckMobCommunication(sender, location, message, cancellationToken).Wait();
                        }
                        catch (Exception exc)
                        {
                            this.logger.Warn($"Mob tried to communicate with player but lost the socket handle: {exc.Message}", this);
                        }
                    });
                }
            }

            return CommResult.Ok;
        }

        /// <inheritdoc/>
        public async Task<CommResult> SendToRoom(KeyValuePair<long, long> location, Character actor, Character? target, string message, CancellationToken cancellationToken = default)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(buffer);

            if (Users != null)
            {
                // Send message to everyone in the room except the actor and the target (combat messages).
                var usersToSendTo = Users.Where(u => u.Value.Character.Location.InSamePlace(location) && u.Value.Character.FirstName != actor.FirstName && u.Value.Character.FirstName != target?.FirstName);

                foreach (var user in usersToSendTo)
                {
                    await user.Value.Connection.SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken);
                }
            }

            return CommResult.Ok;
        }

        /// <inheritdoc/>
        public async Task<CommResult> SendToArea(KeyValuePair<long, long> location, string socketId, string message, CancellationToken cancellationToken = default)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(buffer);

            if (Users != null)
            {
                foreach (var user in Users)
                {
                    if (user.Key != socketId && user.Value.Character.Location.Key == location.Key)
                    {
                        await user.Value.Connection.SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken);
                    }
                }
            }

            return CommResult.Ok;
        }

        /// <inheritdoc/>
        public async Task<CommResult> SendToChannel(CommChannel? channel, string socketId, string message, CancellationToken cancellationToken = default)
        {
            var comm = this.Channels.FirstOrDefault(c => c.Name.ToLower() == channel?.Name.ToLower());

            if (comm != null && !comm.IsMuted)
            {
                foreach (var sub in comm.Subscribers)
                {
                    if (sub.Value.ConnectionId != socketId)
                    {
                        await this.SendToPlayer(sub.Value.Connection, message, cancellationToken);
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
        public UserData? ResolveCharacter(Character character)
        {
            if (!character.IsNPC && Users != null)
            {
                return Users.FirstOrDefault(u => u.Value.Character.FirstName == character.FirstName).Value;
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public UserData? ResolveCharacter(string name)
        {
            if (Users != null)
            {
                return Users.FirstOrDefault(u => u.Value.Character.FirstName?.ToLower() == name.ToLower()).Value;
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public Mobile? ResolveMobile(string name)
        {
            foreach (var area in this.world.Areas)
            {
                foreach (var room in area.Rooms)
                {
                    var mob = room.Mobiles.FirstOrDefault(m => m.FirstName.ToLower().StartsWith(name.ToLower()));

                    if (mob != null)
                    {
                        return mob;
                    }
                }
            }

            return null;
        }

        /// <inheritdoc/>
        public KeyValuePair<long, long>? ResolveMobileLocation(string name)
        {
            foreach (var area in this.world.Areas)
            {
                foreach (var room in area.Rooms)
                {
                    var mob = room.Mobiles.FirstOrDefault(m => m.FirstName.ToLower().StartsWith(name.ToLower()));

                    if (mob != null)
                    {
                        return new KeyValuePair<long, long>(area.AreaId, room.RoomId);
                    }
                }
            }

            return null;
        }

        /// <inheritdoc/>
        public Character? ResolveFightingCharacter(Character actor)
        {
            if (actor.Fighting != null)
            {
                var user = Users?.FirstOrDefault(u => u.Value.Character.CharacterId == actor.Fighting).Value;

                if (user != null)
                {
                    return user?.Character;
                }
                else
                {
                    var mobilesInRoom = this.GetMobilesInRoom(actor.Location);
                    return mobilesInRoom?.FirstOrDefault(m => m.CharacterId == actor.Fighting);
                }
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public Item ResolveItem(long itemId)
        {
            return this.world.Items.Single(i => i.ItemId == itemId);
        }

        /// <inheritdoc/>
        public Room ResolveRoom(KeyValuePair<long, long> location)
        {
            return this.world.Areas.Single(a => a.AreaId == location.Key).Rooms.Single(r => r.RoomId == location.Value);
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

        /// <summary>
        /// Saves the character to disk.
        /// </summary>
        /// <param name="character">The user character.</param>
        /// <returns>Task.</returns>
        public async Task SaveCharacter(Character character)
        {
            try
            {
                await this.dataService.SaveCharacter(character);
            }
            catch (Exception exc)
            {
                this.logger.Error(exc, this);
            }
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
        public List<Mobile>? GetMobilesInRoom(KeyValuePair<long, long> location)
        {
            var room = this.ResolveRoom(location);
            return room?.Mobiles.ToList();
        }

        /// <inheritdoc/>
        public List<Character>? GetPlayersInRoom(Character actor, KeyValuePair<long, long> location)
        {
            var room = this.ResolveRoom(location);
            if (Users != null)
            {
                return Users.Where(u => u.Value.Character.Location.Key == location.Key
                    && u.Value.Character.Location.Value == location.Value
                    && u.Value.Character.FirstName != actor.FirstName).Select(u => u.Value.Character).ToList();
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public List<Character>? GetPlayersInRoom(KeyValuePair<long, long> location)
        {
            var room = this.ResolveRoom(location);
            if (Users != null)
            {
                return Users.Where(u => u.Value.Character.Location.Key == location.Key
                    && u.Value.Character.Location.Value == location.Value).Select(u => u.Value.Character).ToList();
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public List<Mobile>? GetMobilesInArea(long areaId)
        {
            List<Mobile> mobiles = new List<Mobile>();
            var area = this.world.Areas.FirstOrDefault(a => a.AreaId == areaId);
            if (area != null)
            {
                mobiles = area.Rooms.SelectMany(r => r.Mobiles).ToList();
            }

            return mobiles;
        }

        /// <inheritdoc/>
        public List<Item>? GetItemsInRoom(KeyValuePair<long, long> location)
        {
            var room = this.ResolveRoom(location);
            return room?.Items.ToList();
        }

        /// <summary>
        /// Generates a situation based on the encounter.
        /// </summary>
        /// <param name="actor">The character.</param>
        /// <param name="target">The mobile.</param>
        /// <returns>string.</returns>
        public string GetSituation(Character actor, Character target)
        {
            var gen1 = actor.Gender == Gender.Male ? "boy" : "girl";
            var gen2 = target.Gender == Gender.Male ? "boy" : "girl";
            return $"{gen1} and {gen2} talking to each other";
        }

        /// <summary>
        /// Allows mobs with personalities to communicate to characters who say things.
        /// </summary>
        /// <param name="character">The speaking character.</param>
        /// <param name="location">The room.</param>
        /// <param name="message">The character's message.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task CheckMobCommunication(Character character, KeyValuePair<long, long> location, string message, CancellationToken cancellationToken = default)
        {
            var mobiles = this.GetMobilesInRoom(location);

            if (mobiles != null && mobiles.Count > 0)
            {
                foreach (var mobile in mobiles)
                {
                    var situation = this.GetSituation(character, mobile);

                    var response = await this.LanguageProcessor.Process(character, mobile, message, situation);

                    if (!string.IsNullOrWhiteSpace(response))
                    {
                        response = $"{mobile.FirstName} says \"<span class='say'>{response}</span>\"";
                        await this.SendToRoom(mobile, mobile.Location, string.Empty, response, cancellationToken);
                    }
                    else
                    {
                        // Mob did not want to communicate, so it may do an emote instead.
                        response = this.LanguageProcessor.ProcessEmote(character, mobile, message);

                        if (!string.IsNullOrWhiteSpace(response))
                        {
                            await this.SendToRoom(mobile, mobile.Location, string.Empty, response, cancellationToken);
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        public async Task PlaySound(Character user, AudioChannel channel, string sound, CancellationToken cancellationToken = default)
        {
            if (user.IsNPC)
            {
                return;
            }

            if ((int)channel > 7 || (int)channel < 0)
            {
                return;
            }

            var contentRootPath = this.webHostEnvironment.ContentRootPath;
            var soundFile = Path.Combine(contentRootPath, sound);

            if (File.Exists(soundFile.Replace("../", "/wwwroot/")))
            {
                await this.SendToPlayer(user, $"[AUDIO]|{(int)channel}|{sound}", cancellationToken);
            }
            else
            {
                return;
            }
        }

        /// <inheritdoc/>
        public async Task PlaySoundToRoom(Character actor, Character? target, string sound, CancellationToken cancellationToken = default)
        {
            var players = this.GetPlayersInRoom(actor, actor.Location);

            if (players != null)
            {
                foreach (var player in players)
                {
                    if (target != null && target.FirstName == player.FirstName)
                    {
                        continue;
                    }
                    else
                    {
                        await this.PlaySound(player, AudioChannel.Target, sound, cancellationToken);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public async Task<bool> CheckLevelAdvance(Character character, CancellationToken cancellationToken = default)
        {
            // TODO: This will need some tweaking.
            var level = character.Level;
            var experience = character.Experience;
            bool didAdvance = (level * 1500) > experience;

            if (didAdvance)
            {
                await this.IncreaseLevel(character, cancellationToken);
            }

            return didAdvance;
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
        /// Checks to see if this player is casting something.
        /// </summary>
        /// <param name="command">The first argument.</param>
        /// <returns>True if casting.</returns>
        private static bool IsCasting(string command)
        {
            return command switch
            {
                "c" or "ca" or "cas" or "cast" or "co" or "com" or "comm" or "commu" or "commun" or "commune" => true,
                _ => false
            };
        }

        /// <summary>
        /// Advances the user one level.
        /// </summary>
        /// <param name="character">The user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        private async Task IncreaseLevel(Character character, CancellationToken cancellationToken = default)
        {
            character.Level += 1;

            // HP is based on con
            var hp = this.random.Next(8 + this.random.Next(1, 4), (int)character.Con.Current);

            // Movement is based on dex
            var move = this.random.Next(8 + this.random.Next(1, 4), (int)character.Dex.Current);

            // Mana is based on wis
            var mana = this.random.Next(8 + this.random.Next(1, 4), (int)character.Wis.Current);

            character.Health.Max += hp;
            character.Mana.Max += mana;
            character.Movement.Max += move;

            // Calculate the advance trains and practices.
            var trains = character.Int.Max / 4;
            var pracs = character.Wis.Max / 4;

            character.Trains += (int)trains;
            character.Practices += (int)pracs;

            // Save all the changes.
            await this.SaveCharacter(character);

            await this.SendToPlayer(character, $"You advanced a level! You gained {hp} health, {mana} mana, and {move} movement. You have {character.Trains} training sessions and {character.Practices} practices.", cancellationToken);
            await this.PlaySound(character, AudioChannel.Actor, Sounds.LEVELUP, cancellationToken);
        }

        /// <summary>
        /// Shows the player (or mobile) to another player.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns>String.</returns>
        private string GetPlayerInfo(Character target)
        {
            var sb = new StringBuilder();

            sb.Append($"<span class='player-desc-title'>{target.FirstName} {target.LastName}</span><br/>");
            sb.Append($"<span class='player-description'>{target.LongDescription}</span><br/>");

            // How beat up they are.
            sb.Append(this.combat.GetPlayerCondition(target));

            // Worn items.
            if (target.IsNPC)
            {
                sb.Append(this.actionHelper.GetOnlyEquipment(target));
            }
            else
            {
                sb.Append(this.actionHelper.GetEquipment(target));
            }

            return sb.ToString();
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

            CommandArgs? args = CommandArgs.ParseCommand(input);

            if (args == null)
            {
                await this.SendToPlayer(user.Connection, "You don't know how to do that.", cancellationToken);
            }
            else
            {
                // See if this is a single emote
                var emote = Emotes.Get(args.Action);

                if (emote != null)
                {
                    await this.SendToPlayer(user.Connection, emote.ToSelf, cancellationToken);
                    await this.SendToRoom(user.Character, user.Character.Location, user.ConnectionId, emote.ToRoom.Replace("{0}", user.Character.FirstName), cancellationToken);
                }
                else
                {
                    // See if this is a skill
                    if (!string.IsNullOrWhiteSpace(args.Action) && user.Character.HasSkill(args.Action))
                    {
                        if (this.skillProcessor != null)
                        {
                            await this.skillProcessor.DoSkill(user, args, cancellationToken);
                            return;
                        }
                        else
                        {
                            await this.SendToPlayer(user.Connection, "You don't know how to do that.", cancellationToken);
                            return;
                        }
                    }
                    else if (IsCasting(args.Action))
                    {
                        // If casting, see what they are casting and see if they can cast it.
                        if (!string.IsNullOrWhiteSpace(args.Method))
                        {
                            if (user.Character.HasSpell(args.Method))
                            {
                                if (this.spellProcessor != null)
                                {
                                    await this.spellProcessor.DoSpell(user, args, cancellationToken);
                                    return;
                                }
                                else
                                {
                                    await this.SendToPlayer(user.Connection, "You don't know how to cast that.", cancellationToken);
                                    return;
                                }
                            }
                            else
                            {
                                await this.SendToPlayer(user.Connection, "You don't know how to cast that.", cancellationToken);
                                return;
                            }
                        }
                        else
                        {
                            await this.SendToPlayer(user.Connection, "Commune or cast what?", cancellationToken);
                        }
                    }
                    else
                    {
                        // Not casting, using a skill, or emoting, so check actions.
                        if (this.actionProcessor != null)
                        {
                            await this.actionProcessor.DoAction(user, args, cancellationToken);
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Applies all skills and spells to a character. This is just for testing.
        /// </summary>
        /// <param name="userData">The user.</param>
        private void ApplySkillsAndSpells(UserData userData)
        {
            var engine = Assembly.Load("Legendary.Engine");

            var spellTrees = engine.GetTypes().Where(t => t.Namespace == "Legendary.Engine.Models.SpellTrees");

            foreach (var tree in spellTrees)
            {
                var treeInstance = Activator.CreateInstance(tree, this, this.random, this.combat);

                var groupProps = tree.GetProperties();

                for (var x = 1; x <= 5; x++)
                {
                    var spellGroup = groupProps.FirstOrDefault(g => g.Name == $"Group{x}");

                    if (spellGroup != null)
                    {
                        var obj = spellGroup.GetValue(treeInstance);
                        if (obj != null)
                        {
                            var group = (List<IAction>)obj;

                            foreach (var kvp in group)
                            {
                                if (!userData.Character.HasSpell(kvp.Name.ToLower()))
                                {
                                    userData.Character.Spells.Add(new SpellProficiency(kvp.Name, 75));
                                }
                            }
                        }
                    }
                }
            }

            var skillTrees = engine.GetTypes().Where(t => t.Namespace == "Legendary.Engine.Models.SkillTrees");

            foreach (var tree in skillTrees)
            {
                var treeInstance = Activator.CreateInstance(tree, this, this.random, this.combat);

                var groupProps = tree.GetProperties();

                for (var x = 1; x <= 5; x++)
                {
                    var spellGroup = groupProps.FirstOrDefault(g => g.Name == $"Group{x}");

                    if (spellGroup != null)
                    {
                        var obj = spellGroup.GetValue(treeInstance);
                        if (obj != null)
                        {
                            var group = (List<IAction>)obj;

                            foreach (var kvp in group)
                            {
                                if (!userData.Character.HasSkill(kvp.Name.ToLower()))
                                {
                                    userData.Character.Skills.Add(new SkillProficiency(kvp.Name, 75));
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates the user's metrics on login.
        /// </summary>
        /// <param name="userData">The user to update.</param>
        /// <param name="ipAddress">The remote IP address.</param>
        /// <returns>Task.</returns>
        private async Task UpdateMetrics(UserData userData, string? ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                return;
            }

            var metrics = userData.Character.Metrics;

            // Log the IP address to the character
            if (!metrics.IPAddresses.Contains(ipAddress.ToString()))
            {
                metrics.IPAddresses.Add(ipAddress.ToString());
            }

            // Update last login.
            metrics.LastLogin = DateTime.UtcNow;

            userData.Character.Metrics = metrics;

            // Save the changes.
            await this.SaveCharacter(userData);
        }

        /// <summary>
        /// Occurs each second.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void Engine_VioTick(object? sender, EventArgs e)
        {
            try
            {
                // We want this to block so if it takes longer than 1 second to kill a player, we don't get multiple deaths.
                this.combat.HandleCombatTick().Wait();
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, this);
            }
        }

        /// <summary>
        /// Occurs every 30 seconds.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void Engine_Tick(object? sender, EventArgs e)
        {
            try
            {
                var engineEventArgs = (EngineEventArgs)e;

                if (Users != null)
                {
                    foreach (var user in Users)
                    {
                        if (user.Value != null)
                        {
                            // Update the player info
                            this.ShowPlayerInfo(user.Value.Character).Wait();

                            // See what's going on around the player.
                            if (user.Value.Environment != null)
                            {
                                user.Value.Environment.ProcessEnvironmentChanges(engineEventArgs.GameTicks, engineEventArgs.GameHour);
                            }

                            // Autosave the user each tick.
                            this.SaveCharacter(user.Value).Wait();
                        }
                    }
                }

                // Handle any changes in the world (item rot, movement of mobs, etc).
                this.world.ProcessWorldChanges(this, this.random).Wait();
            }
            catch (Exception exc)
            {
                this.logger.Warn($"Attempted to send information to disconnected sockets. Continuing. Error: {exc.Message}", this);
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
        /// <param name="cancellationToken">CancellationToken.</param>
        private async Task ShowWelcomeScreen(UserData user, CancellationToken cancellationToken = default)
        {
            var content = await this.apiClient.GetContent($"welcome?playerName={user.Character.FirstName}");
            if (content != null)
            {
                await this.SendToPlayer(user.Connection, content, cancellationToken);
            }

            await this.SendToRoom(user.Character, user.Character.Location, user.ConnectionId, $"{user.Character.FirstName} suddenly appears.", cancellationToken);
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
