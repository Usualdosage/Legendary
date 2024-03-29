﻿// <copyright file="Communicator.cs" company="Legendary™">
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
    using Legendary.Engine.Models.Output;
    using Legendary.Engine.Models.Skills;
    using Legendary.Engine.Models.Spells;
    using Legendary.Engine.Processors;
    using Legendary.Engine.Types;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.CodeAnalysis;
    using Newtonsoft.Json;

    /// <summary>
    /// Handles communication between the engine and connected sockets.
    /// </summary>
    public class Communicator : ICommunicator, IDisposable
    {
        private readonly RequestDelegate requestDelegate;
        private readonly TitleGenerator titleGenerator;
        private readonly CombatProcessor combat;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger logger;
        private readonly IApiClient apiClient;
        private readonly IDataService dataService;
        private readonly IEngine engine;
        private readonly IRandom random;
        private readonly IWorld world;
        private readonly IMessageProcessor messageProcessor;
        private readonly IEnvironment environment;
        private readonly IServerSettings serverSettings;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly SkillProcessor skillProcessor;
        private readonly SpellProcessor spellProcessor;
        private readonly ActionProcessor actionProcessor;
        private readonly ActionHelper actionHelper;
        private readonly AwardProcessor awardProcessor;
        private readonly QuestProcessor questProcessor;
        private readonly IMIRPProcessor mIRPProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="Communicator"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">The http context.</param>
        /// <param name="requestDelegate">RequestDelegate.</param>
        /// <param name="webHostEnvironment">The hosting environment.</param>
        /// <param name="logger">ILogger.</param>
        /// <param name="serverSettings">The server settings.</param>
        /// <param name="apiClient">The api client.</param>
        /// <param name="dataService">The data service.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="cache">The cache.</param>
        public Communicator(IHttpContextAccessor httpContextAccessor, RequestDelegate requestDelegate, IWebHostEnvironment webHostEnvironment, ILogger logger, IServerSettings serverSettings, IApiClient apiClient, IDataService dataService, IRandom random, ICacheService cache)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.webHostEnvironment = webHostEnvironment;
            this.logger = logger;
            this.requestDelegate = requestDelegate;
            this.apiClient = apiClient;
            this.dataService = dataService;
            this.serverSettings = serverSettings;
            this.random = random;

            this.world = new World(this.dataService, this.random, this.logger, this, cache);

            // Add public channels.
            this.Channels = new List<CommChannel>()
                {
                    new CommChannel("pray", false, true),
                    new CommChannel("newbie", false, true),
                    new CommChannel("imm", true, false),
                    new CommChannel("wiznet", true, false),
                };

            // Set the environment to handle global events.
            this.environment = new Environment(this, random, this.world, this.logger);

            // Start the engine.
            this.engine = new Engine(this.logger, this.world, this.environment);
            this.engine.Tick += this.Engine_Tick;
            this.engine.VioTick += this.Engine_VioTick;

            // Message processor for mail.
            this.messageProcessor = new MessageProcessor(this, this.world, this.dataService, this.logger);

            // Create the combat processor.
            this.combat = new CombatProcessor(this, this.world, this.environment, this.random, this.logger, this.messageProcessor, this.dataService);

            // Create the language generator.
            this.LanguageGenerator = new LanguageGenerator(this.random);

            // Create the action helper.
            this.actionHelper = new ActionHelper(this, this.random, this.world, this.logger, this.combat);

            // Create the award processor.
            this.awardProcessor = new AwardProcessor(this, this.world, this.logger, this.random, this.combat);

            // Create the quest processor.
            this.questProcessor = new QuestProcessor(this.logger, this, this.awardProcessor);

            // Create the language processor.
            this.LanguageProcessor = new LanguageProcessor(this.logger, this.serverSettings, this, this.random, this.environment, this.world, this.questProcessor);

            // Create the title generator.
            this.titleGenerator = new TitleGenerator(this, this.random, this.world, this.logger, this.combat);

            // Create instances of the action, skill, and spell processors.
            this.skillProcessor = new SkillProcessor(this, this.random, this.world, this.combat, this.logger);
            this.spellProcessor = new SpellProcessor(this, this.random, this.world, this.combat, this.logger);
            this.actionProcessor = new ActionProcessor(this, this.environment, this.world, this.logger, this.random, this.combat, this.messageProcessor, this.dataService);

            // Create the mob-item-room processor instance.
            this.mIRPProcessor = new MIRPProcessor(this, this.world, this.actionProcessor, this.combat, this.awardProcessor, this.skillProcessor, this.spellProcessor);
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
        /// Gets or sets a concurrent dictionary of players in groups. The key is the player who is leading the group.
        /// </summary>
        public static ConcurrentDictionary<long, List<long>> Groups { get; set; } = new ConcurrentDictionary<long, List<long>>();

        /// <summary>
        /// Gets or sets a concurrent dictionary of people ignoring other people for this session. Item1 is the ignorer, Item2 is the ignoree. (Bob ignores Alice).
        /// </summary>
        public static List<Tuple<string, string>> Ignores { get; set; } = new List<Tuple<string, string>>();

        /// <summary>
        /// Gets the language processor.
        /// </summary>
        public ILanguageProcessor LanguageProcessor { get; private set; }

        /// <summary>
        /// Gets the instance of the MIRPProcessor.
        /// </summary>
        public IMIRPProcessor MIRPProcessor => this.mIRPProcessor;

        /// <summary>
        /// Gets the language generator.
        /// </summary>
        public ILanguageGenerator LanguageGenerator { get; private set; }

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

            // Ensure user has authenticated
            if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
            {
                var ip = context.Request.HttpContext.Connection.RemoteIpAddress;
                string? user = context.User.Identity?.Name;

                // Load the character by name.
                var character = await this.dataService.FindCharacter(c => c.FirstName == user);

                WebSocket currentSocket = await context.WebSockets.AcceptWebSocketAsync();
                var socketId = Guid.NewGuid().ToString();

                if (character == null)
                {
                    string message = $"{DateTime.UtcNow}: {user} ({socketId}) {ip} was not found.";
                    this.logger.Info(message, this);
                    throw new Exception(message);
                }

                // Configure the user.
                var userData = new UserData(socketId, currentSocket, user ?? "Unknown", character);

                // If the user is already connected, remove, and then re-add.
                var connectedUser = Users?.FirstOrDefault(u => u.Value.Username == character.FirstName);

                if (connectedUser?.Value != null)
                {
                    string message = $"{DateTime.UtcNow}: {user} ({socketId}) had a zombie connection. Removing old connection.";
                    this.logger.Info(message, this);
                    await this.SendToPlayer(connectedUser.Value.Value.Connection, "You have logged in from another location. Disconnecting. Bye!", cancellationToken);
                    await this.Quit(connectedUser.Value.Value.Connection, connectedUser.Value.Value.Character.FirstName);
                    Users?.TryRemove(connectedUser.Value);
                }

                Users?.TryAdd(socketId, userData);

                string msg = $"{DateTime.UtcNow}: {user} ({socketId}) has connected from {ip}.";
                await this.SendToPlayer(userData.Character, $"You have connected from {ip}.", cancellationToken);
                this.logger.Info(msg, this);

                // Remove any fighting affects
                userData.Character.Fighting = null;
                userData.Character.CharacterFlags.RemoveIfExists(CharacterFlags.Fighting);

                // Remove any previous followers or followings.
                userData.Character.Followers = new List<long>();
                userData.Character.Following = null;

                // Clear any groups the members was in. This is usually done on quit, but do it again in case they didn't quit normally.
                GroupHelper.RemoveFromAllGroups(userData.Character.CharacterId);
                userData.Character.GroupId = null;

                // Update the user metrics
                await this.UpdateMetrics(userData, ip?.ToString());

                // Add the user to public channels.
                this.AddToChannels(socketId, userData);

                // See if this is a new character, if so, add the proper hometown, stats, skills, spells, etc.
                await this.CheckNewCharacter(userData.Character);

                // Make sure the character is in an existing room.
                var room = this.ResolveRoom(userData.Character.Location);

                // Wait for the game to load.
                while (room == null)
                {
                    room = this.ResolveRoom(userData.Character.Location);
                }

                // Update the metrics before we display the game updates.
                await this.world.UpdateGameMetrics(null, null, cancellationToken);

                // Update the player's console.
                await this.SendGameUpdate(userData.Character, null, null, cancellationToken);

                await this.SendToPlayer(userData.Character, "<p class='connected'><b>Welcome to the World of Mystra! Enjoy your stay.</b></p>", cancellationToken);

                // Show the room to the player.
                await this.ShowRoomToPlayer(userData.Character, cancellationToken);

                // Show new messages.
                var messageCount = await this.messageProcessor.GetNewMessagesForPlayer(userData.Character, cancellationToken);

                if (messageCount > 0)
                {
                    await this.SendToPlayer(userData.Character, $"<p>You have {messageCount} new messages.</p>", cancellationToken);
                }

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
                        this.logger.Error($"A socket was closed or moved into an error state.", this);
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

                // TODO: This is not working properly.
                await Logout(context);
            }
        }

        /// <inheritdoc/>
        public void RestartGameLoop()
        {
            this.logger.Info("Restarting main engine loop...", null);
            this.engine.StartGameLoop();
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
                    if (user.Value.Connection != null)
                    {
                        await user.Value.Connection.SendAsync(segment, WebSocketMessageType.Text, true, ct);
                    }
                }
            }

            return CommResult.Ok;
        }

        /// <inheritdoc/>
        public async Task<CommResult> SendToPlayer(WebSocket? socket, string message, CancellationToken cancellationToken = default)
        {
            if (socket != null)
            {
                var buffer = Encoding.UTF8.GetBytes(message);
                var segment = new ArraySegment<byte>(buffer);
                await socket.SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken);
                return CommResult.Ok;
            }
            else
            {
                return CommResult.NotConnected;
            }
        }

        /// <inheritdoc/>
        public async Task<CommResult> SendToPlayer(long characterId, string message, CancellationToken cancellationToken = default)
        {
            var userData = this.ResolveCharacter(characterId);

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
        public async Task Quit(WebSocket? socket, string? player, CancellationToken cancellationToken = default)
        {
            var user = Users?.FirstOrDefault(u => u.Value.Username == player);

            if (user != null)
            {
                if (user.Value.Value.Character.CharacterFlags.Contains(CharacterFlags.Fighting))
                {
                    await this.SendToPlayer(socket, "You can't quit, you're FIGHTING!", cancellationToken);
                }
                else
                {
                    // Remove from and/or disband group and followers/following.
                    await this.UpdateGroupAndFollowers(user.Value.Value, cancellationToken);

                    // Perform a final save.
                    await this.SaveCharacter(user.Value.Value);

                    // Remove from the users list.
                    Users?.TryRemove(user.Value);

                    await this.SendToPlayer(user.Value.Value.Connection, $"You have disconnected.", cancellationToken);
                    await this.SendToRoom(user.Value.Value.Character.Location, $"{user.Value.Value.Character.FirstName} has left the realms.", cancellationToken);

                    string message = $"{DateTime.UtcNow}: {player} has left Mystra.";
                    this.logger.Info(message, this);

                    if (socket != null)
                    {
                        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, $"{message}", cancellationToken);
                    }

                    // Sign out and redirect back to the login screen.
                    if (this.httpContextAccessor.HttpContext != null)
                    {
                        this.httpContextAccessor.HttpContext.Response.StatusCode = 401;
                    }
                }
            }
            else
            {
                // Should not get here if user was null. But if we do, just close the socket.
                string message = $"{DateTime.UtcNow}: {player} was not found, but issued QUIT command. Closing socket.";
                this.logger.Warn(message, this);

                if (socket != null)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, $"{message}", cancellationToken);
                }
            }
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
        public bool IsInRoom(KeyValuePair<long, long> location, string target)
        {
            if (Users != null)
            {
                return Users.Any(u => u.Value.Character.FirstName == target && u.Value.Character.Location.InSamePlace(location));
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
                    var mobiles = this.GetMobilesInRoom(user.Character.Location);

                    if (mobiles != null)
                    {
                        var mobile = mobiles.ParseTargetName(targetName);

                        if (mobile != null)
                        {
                            if (!PlayerHelper.CanPlayerSeePlayer(this.environment, this, user.Character, mobile))
                            {
                                await this.SendToPlayer(user.Connection, "They're not here.", cancellationToken);
                                return;
                            }

                            // If they're a ghost, remove the flag if they attack a mob.
                            if (user.Character.CharacterFlags.Contains(CharacterFlags.Ghost))
                            {
                                user.Character.CharacterFlags.Remove(CharacterFlags.Ghost);
                            }

                            await this.SendToPlayer(user.Connection, $"You attack {mobile.FirstName}!", cancellationToken);
                            await this.SendToRoom(user.Character, user.Character.Location, $"{user.Character.FirstName} attacks {mobile.FirstName}!", cancellationToken);

                            var room = this.ResolveRoom(user.Character.Location);

                            if ((int)mobile.Race <= 13 && room != null && room.Terrain == Terrain.City)
                            {
                                await this.SendToArea(user.Character.Location, string.Empty, $"{mobile.FirstName.FirstCharToUpper()} yells (in Common) \"<span class='yell'>Help, I'm being attacked by {user.Character.FirstName}!</span>\"", cancellationToken);
                            }

                            // Start the fight.
                            await this.combat.StartFighting(user.Character, mobile, cancellationToken);
                        }
                        else
                        {
                            await this.SendToPlayer(user.Connection, "They're not here.", cancellationToken);
                        }
                    }
                }
                else
                {
                    var targetChar = target.Value.Value.Character;

                    if (!PlayerHelper.CanPlayerSeePlayer(this.environment, this, user.Character, targetChar))
                    {
                        await this.SendToPlayer(user.Connection, "They're not here.", cancellationToken);
                        return;
                    }

                    // If they're a ghost, remove the flag if they attack a mob.
                    if (user.Character.CharacterFlags.Contains(CharacterFlags.Ghost))
                    {
                        await this.SendToPlayer(user.Connection, $"You can't attack {targetChar.FirstName} right now, because you're a ghost.", cancellationToken);
                    }
                    else if (targetChar.CharacterFlags.Contains(CharacterFlags.Ghost))
                    {
                        await this.SendToPlayer(user.Connection, $"You can't attack the ghost of {targetChar.FirstName}.", cancellationToken);
                    }
                    else if (!PlayerHelper.IsInPK(user.Character, targetChar))
                    {
                        await this.SendToPlayer(user.Connection, $"{targetChar.FirstName} is protected from you by the Gods.", cancellationToken);
                    }
                    else
                    {
                        if (user.Character.GroupId.HasValue)
                        {
                            var groupMembers = GroupHelper.GetAllGroupMembers(user.Character.GroupId.Value);

                            if (groupMembers != null && groupMembers.Contains(user.Character.CharacterId))
                            {
                                await this.SendToPlayer(user.Connection, $"You're in their group. Ungroup first, then attack.", cancellationToken);
                                return;
                            }
                        }

                        this.logger.Info($"{user.Character.FirstName.FirstCharToUpper()} has attacked {targetChar.FirstName} in room {user.Character.Location.Value}.", this);

                        await this.SendToPlayer(user.Connection, $"You attack {targetChar.FirstName}!", cancellationToken);
                        await this.SendToRoom(user.Character, user.Character.Location, $"{user.Character.FirstName.FirstCharToUpper()} attacks {targetChar.FirstName}!", cancellationToken);

                        var room = this.ResolveRoom(user.Character.Location);

                        if (room != null && room.Terrain == Terrain.City)
                        {
                            string sentence = $"Help, I'm being attacked by {user.Character.FirstName}!";
                            await this.actionProcessor.DoAction(target.Value.Value, new CommandArgs("yell", sentence, string.Empty), cancellationToken);
                        }

                        // Start the fight.
                        await this.combat.StartFighting(user.Character, target.Value.Value.Character, cancellationToken);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public async Task ShowPlayerToPlayer(Character actor, string targetName, CancellationToken cancellationToken = default)
        {
            targetName = targetName.ToLower();

            if (targetName == actor.FirstName || targetName == "self")
            {
                await this.SendToPlayer(actor, "You look at yourself.", cancellationToken);
                await this.SendToPlayer(actor, GetPlayerInfo(actor), cancellationToken);
                await this.SendToRoom(actor.Location, actor, null, $"{actor.FirstName.FirstCharToUpper()} looks at {actor.Pronoun}self.", cancellationToken);

                // Update player stats
                await this.SendGameUpdate(actor, actor.FirstName, actor.Images?[0], cancellationToken);
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
                            await this.SendToRoom(actor.Location, actor, null, $"{actor.FirstName.FirstCharToUpper()} looks at {mobile.FirstName}.", cancellationToken);
                            await this.SendToPlayer(actor, GetPlayerInfo(mobile), cancellationToken);

                            if (actor.HasSkill(nameof(Peek)))
                            {
                                var peek = actor.GetSkillProficiency(nameof(Peek));

                                if (peek != null && peek.Proficiency > 1)
                                {
                                    var peekResult = this.random.Next(1, 99);
                                    if (peekResult < peek.Proficiency)
                                    {
                                        await this.SendToPlayer(actor, $"You peek at {mobile.FirstName}'s inventory!<br/>", cancellationToken);

                                        var sb = new StringBuilder();

                                        sb.AppendLine($"<span class='inventory'>{mobile.FirstName.FirstCharToUpper()} is carrying:</span>");

                                        var itemGroups = mobile.Inventory.GroupBy(g => g.Name);

                                        foreach (var itemGroup in itemGroups)
                                        {
                                            if (itemGroup != null)
                                            {
                                                var item = itemGroup.First();

                                                if (itemGroup.Count() == 1)
                                                {
                                                    sb.AppendLine($"<span class='inventory-item'>{ActionHelper.DecorateItem(item, null)}</span>");
                                                }
                                                else
                                                {
                                                    sb.Append($"<span class='item'>({itemGroup.Count()}) {ActionHelper.DecorateItem(item, null)}</span>");
                                                }
                                            }
                                        }

                                        await this.SendToPlayer(actor, sb.ToString(), cancellationToken);

                                        Peek skill = new (this, this.random, this.world, this.logger, this.combat);
                                        await skill.CheckImprove(actor, cancellationToken);
                                    }
                                }
                            }

                            // Update player stats
                            if (mobile.XActive.HasValue && mobile.XActive.Value == true && mobile.XImages != null && mobile.XImages.Count > 0)
                            {
                                var xImage = mobile.XImages[this.random.Next(0, mobile.XImages.Count - 1)];
                                await this.SendGameUpdate(actor, mobile.FirstName, xImage, cancellationToken);
                            }
                            else
                            {
                                var image = mobile.Images?[this.random.Next(0, mobile.Images.Count - 1)];
                                await this.SendGameUpdate(actor, mobile.FirstName, image, cancellationToken);
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
                    if (target.Value.Value.Character.Location.InSamePlace(actor.Location))
                    {
                        await this.SendToPlayer(target.Value.Value.Character, $"{actor.FirstName.FirstCharToUpper()} looks at you.", cancellationToken);
                        await this.SendToRoom(actor.Location, actor, target.Value.Value.Character, $"{actor.FirstName} looks at {target.Value.Value.Character.FirstName}.", cancellationToken);
                        await this.SendToPlayer(actor, GetPlayerInfo(target.Value.Value.Character), cancellationToken);

                        if (actor.HasSkill(nameof(Peek)))
                        {
                            var peek = actor.GetSkillProficiency(nameof(Peek));
                            var character = target.Value.Value.Character;

                            if (peek != null && peek.Proficiency > 1)
                            {
                                var peekResult = this.random.Next(1, 99);
                                if (peekResult < peek.Proficiency)
                                {
                                    await this.SendToPlayer(actor, $"You peek at {character.FirstName}'s inventory!<br/>", cancellationToken);

                                    var sb = new StringBuilder();

                                    sb.AppendLine($"<span class='inventory'>{character.FirstName} is carrying:</span>");

                                    var itemGroups = character.Inventory.GroupBy(g => g.Name);

                                    foreach (var itemGroup in itemGroups)
                                    {
                                        if (itemGroup != null)
                                        {
                                            var item = itemGroup.First();

                                            if (itemGroup.Count() == 1)
                                            {
                                                sb.AppendLine($"<span class='inventory-item'>{ActionHelper.DecorateItem(item, null)}</span>");
                                            }
                                            else
                                            {
                                                sb.Append($"<span class='item'>({itemGroup.Count()}) {ActionHelper.DecorateItem(item, null)}</span>");
                                            }
                                        }
                                    }

                                    await this.SendToPlayer(actor, sb.ToString(), cancellationToken);

                                    Peek skill = new (this, this.random, this.world, this.logger, this.combat);
                                    await skill.CheckImprove(actor, cancellationToken);
                                }
                            }
                        }

                        // Update player stats
                        await this.SendGameUpdate(actor, target.Value.Value.Character.FirstName, target.Value.Value.Character.Images?[0], cancellationToken);
                    }
                    else
                    {
                        await this.SendToPlayer(actor, "They are not here.", cancellationToken);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public async Task ShowItemToPlayer(Character actor, Item item, CancellationToken cancellationToken = default)
        {
            await this.SendToPlayer(actor, $"You examine {item.Name}.", cancellationToken);

            StringBuilder sb = new ();

            // Update the player info
            this.SendGameUpdate(actor, item.ShortDescription, item.Image, cancellationToken).Wait(cancellationToken);

            sb.Append($"{item.LongDescription}<br/>");

            if (item.Contains?.Count > 0)
            {
                sb.Append($"{item.Name} contains:<br/>");

                foreach (var eq in item.Contains)
                {
                    sb.Append($"<span class='item'>{eq.Name}</span>");
                }
            }

            await this.SendToPlayer(actor, sb.ToString(), cancellationToken);
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

            if (!PlayerHelper.CanPlayerSee(this.environment, this, actor))
            {
                sb.Append("You can't see anything, it's pitch black.");
                actor.LastImage = null;
                await this.SendToPlayer(actor, sb.ToString(), cancellationToken);
                return;
            }

            var terrainClass = room?.Terrain.ToString().ToLower() ?? "city";

            sb.Append($"<span class='room-title {terrainClass}'>{room?.Name}</span> <span class='roomNum'>[{room?.RoomId}]</span><br/>");

            sb.Append($"<span class='room-description'>{room?.Description}</span><br/>");

            if (!string.IsNullOrWhiteSpace(room?.WatchKeyword) && !string.IsNullOrWhiteSpace(room?.Video))
            {
                sb.Append($"<span class='room-video'>You can <b>watch</b> the <i>{room.WatchKeyword}</i> here.</span><br/>");
            }

            sb.Append("<span class='exits'>[ Exits: ");

            // Show the exits
            if (room?.Exits != null)
            {
                foreach (var exit in room.Exits)
                {
                    if (exit.IsDoor && exit.IsClosed)
                    {
                        sb.Append("(" + Enum.GetName(typeof(Direction), exit.Direction)?.ToLower() + ") ");
                    }
                    else if (exit.IsHidden)
                    {
                        continue;
                    }
                    else
                    {
                        sb.Append(Enum.GetName(typeof(Direction), exit.Direction)?.ToLower() + " ");
                    }
                }
            }

            sb.Append("]</span>");

            // Show the items
            if (room?.Items != null)
            {
                var itemGroups = room.Items.GroupBy(g => g.Name);

                foreach (var itemGroup in itemGroups)
                {
                    if (itemGroup != null)
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
            }

            // Show the mobiles
            if (room?.Mobiles != null)
            {
                var mobGroups = room.Mobiles.GroupBy(g => g.CharacterId);

                foreach (var mobGroup in mobGroups)
                {
                    if (mobGroup != null)
                    {
                        var mob = mobGroup.First();
                        string effects = this.GetEffects(mob);

                        if (mobGroup.Count() == 1)
                        {
                            if (PlayerHelper.CanPlayerSeePlayer(this.environment, this, actor, mob))
                            {
                                if (mob.CharacterFlags.Contains(CharacterFlags.Sleeping))
                                {
                                    sb.Append($"<span class='mobile'>{effects}{mob.FirstName.FirstCharToUpper()} is sleeping here.</span>");
                                }
                                else
                                {
                                    sb.Append($"<span class='mobile'>{effects}{mob.ShortDescription}</span>");
                                }
                            }
                        }
                        else
                        {
                            var sleepingMobs = mobGroup.Any(m => m.CharacterFlags.Contains(CharacterFlags.Sleeping));

                            if (sleepingMobs)
                            {
                                var sleeping = mobGroup.Where(m => m.CharacterFlags.Contains(CharacterFlags.Sleeping));
                                var notSleeping = mobGroup.Where(m => !m.CharacterFlags.Contains(CharacterFlags.Sleeping));

                                if (PlayerHelper.CanPlayerSeePlayer(this.environment, this, actor, mob))
                                {
                                    if (sleeping.Count() == 1)
                                    {
                                        sb.Append($"<span class='mobile'>{effects}{mob.FirstName.FirstCharToUpper()} is sleeping here.</span>");
                                    }
                                    else
                                    {
                                        sb.Append($"<span class='mobile'>({sleeping.Count()}){effects}{mob.FirstName.FirstCharToUpper()} is sleeping here.</span>");
                                    }

                                    if (notSleeping.Count() == 1)
                                    {
                                        sb.Append($"<span class='mobile'>{effects}{mob.ShortDescription}</span>");
                                    }
                                    else
                                    {
                                        sb.Append($"<span class='mobile'>({notSleeping.Count()}) {effects}{mob.ShortDescription}</span>");
                                    }
                                }
                            }
                            else
                            {
                                if (PlayerHelper.CanPlayerSeePlayer(this.environment, this, actor, mob))
                                {
                                    sb.Append($"<span class='mobile'>({mobGroup.Count()}) {effects}{mob.ShortDescription}</span>");
                                }
                            }
                        }
                    }
                }

                // See if they get an award for discovery.
                await this.awardProcessor.CheckDiscovererAward(room.Mobiles, actor, cancellationToken);
            }

            // Show other players
            if (Communicator.Users != null)
            {
                var usersInRoom = Users.Where(u => u.Value.Character.Location.InSamePlace(actor.Location) && u.Value.Character.FirstName != actor.FirstName);

                foreach (var other in usersInRoom)
                {
                    string prefix = string.Empty;
                    string effects = this.GetEffects(other.Value.Character);

                    if (PlayerHelper.CanPlayerSeePlayer(this.environment, this, actor, other.Value.Character))
                    {
                        if (other.Value.Character.CharacterFlags.Contains(CharacterFlags.Ghost))
                        {
                            prefix = "The ghost of ";
                        }

                        if (other.Value.Character.CharacterFlags.Contains(CharacterFlags.Resting))
                        {
                            sb.Append($"<span class='player'>{effects}{prefix}{other.Value.Character.FirstName} is resting here.</span>");
                        }
                        else if (other.Value.Character.CharacterFlags.Contains(CharacterFlags.Sleeping))
                        {
                            sb.Append($"<span class='player'>{effects}{prefix}{other.Value.Character.FirstName} is sleeping here.</span>");
                        }
                        else
                        {
                            sb.Append($"<span class='player'>{effects}{prefix}{other.Value.Character.FirstName} is here.</span>");
                        }
                    }
                }
            }

            await this.SendToPlayer(actor, sb.ToString(), cancellationToken);

            // Update player stats
            if (room != null)
            {
                var area = this.ResolveArea(room.AreaId);

                if (area != null)
                {
                    await this.SendGameUpdate(actor, area.Description, room.Image, cancellationToken);
                }
                else
                {
                    await this.SendGameUpdate(actor, null, room.Image, cancellationToken);
                }
            }
            else
            {
                await this.SendGameUpdate(actor, null, null, cancellationToken);
            }

            // Play the music according to the terrain.
            if (room != null)
            {
                var soundIndex = this.random.Next(0, 7);
                await this.PlaySound(actor, AudioChannel.Background, $"https://legendaryweb.file.core.windows.net/audio/music/{room.Terrain.ToString().ToLower()}{soundIndex}.mp3" + Sounds.SASTOKEN, cancellationToken);

                // 40% chance to play the terrain SFX (e.g. forest, city)
                var randomSfx = this.random.Next(0, 10);

                if (randomSfx <= 4)
                {
                    await this.PlaySound(actor, AudioChannel.BackgroundSFX, $"https://legendaryweb.file.core.windows.net/audio/soundfx/{room.Terrain.ToString().ToLower()}.mp3" + Sounds.SASTOKEN, cancellationToken);
                }

                // Check aggro
                foreach (var mob in room.Mobiles)
                {
                    if (mob.MobileFlags != null && mob.MobileFlags.Contains(MobileFlags.Aggressive))
                    {
                        if (mob.Level >= actor.Level)
                        {
                            await this.SendToPlayer(actor, $"{mob.FirstName.FirstCharToUpper()} screams and attacks you!", cancellationToken);
                            await this.combat.StartFighting(mob, actor, cancellationToken);
                        }
                        else
                        {
                            await this.SendToPlayer(actor, $"{mob.FirstName.FirstCharToUpper()} glowers at you angrily, but holds their attack.", cancellationToken);
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        public async Task SendGameUpdate(Character actor, string? caption, string? image, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrWhiteSpace(image))
            {
                actor.LastImage = image;
            }

            var area = this.ResolveArea(actor.Location);
            var room = this.ResolveRoom(actor.Location);
            var metrics = this.world.GameMetrics;

            if (area != null && room != null)
            {
                var roomsExplored = actor.Metrics.RoomsExplored.FirstOrDefault(r => r.Key == area.AreaId).Value?.ToList();
                var totalVisited = actor.Metrics.RoomsExplored.Where(a => a.Key == area.AreaId).Sum(r => r.Value.Count);
                var total = area.Rooms?.Count ?? 0;
                var explorationPct = (double)((double)totalVisited / (double)total) * 100;

                try
                {
                    var outputMessage = new OutputMessage()
                    {
                        Message = new Models.Output.Message()
                        {
                            FirstName = actor.FirstName,
                            Level = actor.Level,
                            Title = actor.Title,
                            Alignment = actor.Alignment.ToString(),
                            Condition = CombatProcessor.GetPlayerCondition(actor),
                            Stats = new StatMessage()
                            {
                                Health = new Status()
                                {
                                    Current = actor.Health.Current,
                                    Max = actor.Health.Max,
                                },
                                Mana = new Status()
                                {
                                    Current = actor.Mana.Current,
                                    Max = actor.Mana.Max,
                                },
                                Movement = new Status()
                                {
                                    Current = actor.Movement.Current,
                                    Max = actor.Movement.Max,
                                },
                                Experience = new Status()
                                {
                                    Current = actor.Experience,
                                    Max = actor.Level >= 90 ? actor.Experience : this.GetTotalExperienceToLevel(actor, false),
                                },
                            },
                            ImageInfo = new ImageInfo()
                            {
                                Caption = caption ?? area?.Description,
                                Image = image ?? actor.LastImage,
                            },
                            Weather = new Models.Output.Weather()
                            {
                                Image = null,
                                Time = metrics != null ? DateTimeHelper.GetDate(metrics.CurrentDay, metrics.CurrentMonth, metrics.CurrentYear, metrics.CurrentHour, DateTime.Now.Minute, DateTime.Now.Second) : null,
                                Temp = null,
                            },
                            Map = new Models.Output.Map()
                            {
                                Current = actor.Location.Value,
                                Rooms = roomsExplored != null ? area?.Rooms?.Where(r => roomsExplored.Contains(r.RoomId)).ToArray() : null,
                                PlayersInArea = this.GetPlayersInArea(area?.AreaId)?.Select(p => new { p.CharacterId, p.FirstName, p.Location.Value })?.ToArray(),
                                MobsInArea = this.GetMobilesInArea(area?.AreaId)?.Select(m => new { m.CharacterId, m.FirstName, m.Location.Value })?.ToArray(),
                                PercentageExplored = explorationPct,
                            },
                        },
                    };

                    var output = JsonConvert.SerializeObject(outputMessage);

                    await this.SendToPlayer(actor, output, cancellationToken);
                }
                catch (Exception exc)
                {
                    this.logger.Error(exc, this);
                }
            }
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

                if (user.Value.Value.Connection != null)
                {
                    var buffer = Encoding.UTF8.GetBytes(message);
                    var segment = new ArraySegment<byte>(buffer);
                    await user.Value.Value.Connection.SendAsync(segment, WebSocketMessageType.Text, true, ct);
                    return CommResult.Ok;
                }
                else
                {
                    return CommResult.NotConnected;
                }
            }

            return CommResult.NotConnected;
        }

        /// <inheritdoc/>
        public async Task<CommResult> SendToRoom(Character actor, KeyValuePair<long, long> location, string message, CancellationToken cancellationToken = default)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(buffer);

            if (Users != null)
            {
                var usersInRoom = Users.Where(u => u.Value.Character.Location.Key == location.Key && u.Value.Character.Location.Value == location.Value).ToList();

                // Send to everyone in the room except the actor.
                foreach (var user in usersInRoom)
                {
                    if (user.Value.Character.CharacterId != actor.CharacterId)
                    {
                        if (!user.Value.Character.CharacterFlags.Contains(CharacterFlags.Sleeping))
                        {
                            if (user.Value.Connection != null)
                            {
                                await user.Value.Connection.SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken);
                            }
                        }
                    }
                }
            }

            return CommResult.Ok;
        }

        /// <inheritdoc/>
        public async Task<CommResult> SendToRoom(KeyValuePair<long, long> location, string message, CancellationToken cancellationToken = default)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(buffer);

            if (Users != null)
            {
                var usersInRoom = Users.Where(u => u.Value.Character.Location.Key == location.Key && u.Value.Character.Location.Value == location.Value).ToList();

                foreach (var user in usersInRoom)
                {
                    if (!user.Value.Character.CharacterFlags.Contains(CharacterFlags.Sleeping))
                    {
                        if (user.Value.Connection != null)
                        {
                            await user.Value.Connection.SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken);
                        }
                    }
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
                    if (!user.Value.Character.CharacterFlags.Contains(CharacterFlags.Sleeping))
                    {
                        if (user.Value.Connection != null)
                        {
                            await user.Value.Connection.SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken);
                        }
                    }
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
                        if (!user.Value.Character.CharacterFlags.Contains(CharacterFlags.Sleeping))
                        {
                            if (user.Value.Connection != null)
                            {
                                await user.Value.Connection.SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken);
                            }
                        }
                    }
                }
            }

            return CommResult.Ok;
        }

        /// <inheritdoc/>
        public async Task<CommResult> SendToArea(Character actor, KeyValuePair<long, long> location, string message, CancellationToken cancellationToken = default)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(buffer);

            if (Users != null)
            {
                foreach (var user in Users)
                {
                    if (user.Value.Character.CharacterId != actor.CharacterId && user.Value.Character.Location.Key == location.Key)
                    {
                        if (!user.Value.Character.CharacterFlags.Contains(CharacterFlags.Sleeping))
                        {
                            if (user.Value.Connection != null)
                            {
                                await user.Value.Connection.SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken);
                            }
                        }
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
        public UserData? ResolveCharacter(long characterId)
        {
            if (Users != null)
            {
                return Users.FirstOrDefault(u => u.Value.Character.CharacterId == characterId).Value;
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
        public Mobile? ResolveMobile(string? name, Character actor)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                List<Mobile> matching = new ();
                foreach (var area in this.world.Areas)
                {
                    if (area.Rooms != null)
                    {
                        foreach (var room in area.Rooms)
                        {
                            var mobs = room.Mobiles.Where(m => m.FirstName.ToLower().Contains(name.ToLower())).ToList();
                            matching.AddRange(mobs);
                        }
                    }
                }

                var mobInRoom = matching.FirstOrDefault(m => m.Location.Value == actor.Location.Value);

                if (mobInRoom != null)
                {
                    return mobInRoom;
                }
                else
                {
                    return matching.FirstOrDefault();
                }
            }

            return null;
        }

        /// <inheritdoc/>
        public Mobile? ResolveMobile(long? characterId)
        {
            if (characterId.HasValue)
            {
                foreach (var area in this.world.Areas)
                {
                    if (area.Rooms != null)
                    {
                        foreach (var room in area.Rooms)
                        {
                            var mob = room.Mobiles.FirstOrDefault(m => m.CharacterId == characterId);

                            if (mob != null)
                            {
                                return mob;
                            }
                        }
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
                if (area.Rooms != null)
                {
                    foreach (var room in area.Rooms)
                    {
                        var mob = room.Mobiles.FirstOrDefault(m => m.FirstName.ToLower().Contains(name.ToLower()));

                        if (mob != null)
                        {
                            return new KeyValuePair<long, long>(area.AreaId, room.RoomId);
                        }
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
        public Item? ResolveItem(UserData actor, string name)
        {
            var item = this.world.Items.FirstOrDefault(i => i.Name.ToLower().Contains(name.ToLower()));

            if (item != null)
            {
                // For it to be valid, must be in the player's inventory, or in the same room.
                var inventoryItem = actor.Character.Inventory.FirstOrDefault(i => i.ItemId == item.ItemId);

                if (inventoryItem != null)
                {
                    return inventoryItem;
                }
                else
                {
                    var room = this.ResolveRoom(actor.Character.Location);

                    if (room != null)
                    {
                        var roomItem = room.Items.FirstOrDefault(i => i.ItemId == item.ItemId);

                        if (roomItem != null)
                        {
                            return roomItem;
                        }
                    }
                }
            }

            return null;
        }

        /// <inheritdoc/>
        public Room? ResolveRoom(KeyValuePair<long, long> location)
        {
            return this.world.Areas.SingleOrDefault(a => a.AreaId == location.Key)?.Rooms?.SingleOrDefault(r => r.RoomId == location.Value);
        }

        /// <inheritdoc/>
        public Area? ResolveArea(KeyValuePair<long, long> location)
        {
            return this.world.Areas.SingleOrDefault(a => a.AreaId == location.Key);
        }

        /// <inheritdoc/>
        public Area? ResolveArea(long areaId)
        {
            return this.world.Areas.SingleOrDefault(a => a.AreaId == areaId);
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
        public List<Character>? GetPlayersInArea(Character actor, KeyValuePair<long, long> location)
        {
            if (Users != null)
            {
                return Users.Where(u => u.Value.Character.Location.Key == location.Key
                    && u.Value.Character.CharacterId != actor.CharacterId).Select(u => u.Value.Character).ToList();
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public List<Character>? GetPlayersInArea(long? areaId)
        {
            if (areaId.HasValue)
            {
                if (Users != null)
                {
                    return Users.Where(u => u.Value.Character.Location.Key == areaId).Select(u => u.Value.Character).ToList();
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public List<Character>? GetPlayersInRoom(Character actor, KeyValuePair<long, long> location)
        {
            if (Users != null)
            {
                return Users.Where(u => u.Value.Character.Location.Key == location.Key
                    && u.Value.Character.Location.Value == location.Value
                    && u.Value.Character.CharacterId != actor.CharacterId).Select(u => u.Value.Character).ToList();
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public List<Character>? GetPlayersInRoom(KeyValuePair<long, long> location)
        {
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
        public List<Mobile>? GetMobilesInArea(long? areaId)
        {
            List<Mobile>? mobiles = new ();
            var area = this.world.Areas.FirstOrDefault(a => a.AreaId == areaId);
            if (area != null)
            {
                mobiles = area?.Rooms?.SelectMany(r => r.Mobiles).ToList();
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

            // Don't include the speaker if it's a mob.
            if (character.IsNPC && mobiles != null)
            {
                mobiles = mobiles.Where(m => m.CharacterId != character.CharacterId).ToList();
            }

            if (mobiles != null && mobiles.Count > 0)
            {
                var (responses, mobile) = await this.LanguageProcessor.Process(character, mobiles, message, cancellationToken);

                if (responses != null && responses.Length > 0 && mobile != null)
                {
                    foreach (var response in responses)
                    {
                        CommandArgs? args = CommandArgs.ParseCommand(response);
                        if (args != null)
                        {
                            UserData userData = new (Guid.NewGuid().ToString(), null, mobile.FirstName, mobile);
                            await this.actionProcessor.DoAction(userData, args, cancellationToken);
                        }
                    }
                }
                else
                {
                    if (mobile != null)
                    {
                        // Mob did not want to communicate, so it may do an emote instead.
                        var emoteResponse = this.LanguageProcessor.ProcessEmote(character, mobile, message);

                        if (!string.IsNullOrWhiteSpace(emoteResponse))
                        {
                            await this.SendToRoom(mobile.Location, emoteResponse, cancellationToken);
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

            if (!user.CharacterFlags.Contains(CharacterFlags.Sleeping))
            {
                await this.SendToPlayer(user, $"[AUDIO]|{(int)channel}|{sound}", cancellationToken);
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
        public async Task PlaySoundToRoom(KeyValuePair<long, long> location, AudioChannel channel, string sound, CancellationToken cancellationToken = default)
        {
            var players = this.GetPlayersInRoom(location);

            if (players != null)
            {
                foreach (var player in players)
                {
                    await this.PlaySound(player, channel, sound, cancellationToken);
                }
            }
        }

        /// <inheritdoc/>
        public long GetRemainingExperienceToLevel(Character character, bool log)
        {
            var penalty = Races.RaceData.First(r => r.Key == character.Race).Value.ExperiencePenalty;

            // Total amount of experience needed to make to the player's next level.
            var totalExperienceRequired = PlayerHelper.GetTotalExperienceRequired(1, character.Level + 1, penalty);

            var amountNeededToLevel = totalExperienceRequired - character.Experience;

            if (log)
            {
                this.logger.Info($"{character.FirstName} is level {character.Level}, currently has a total of {character.Experience} experience and needs a total of {amountNeededToLevel} experience to get to level {character.Level + 1}.", this);
            }

            return amountNeededToLevel;
        }

        /// <inheritdoc/>
        public long GetTotalExperienceToLevel(Character character, bool log)
        {
            if (character.Level >= 90)
            {
                return -1;
            }
            else
            {
                var penalty = Races.RaceData.First(r => r.Key == character.Race).Value.ExperiencePenalty;

                var amountNeededToLevel = PlayerHelper.GetTotalExperienceRequired(character.Level, character.Level + 1, penalty);

                if (log)
                {
                    this.logger.Info($"{character.FirstName} is level {character.Level} and needs {amountNeededToLevel} to advance to next level. They have an experience penalty of {penalty}.", this);
                }

                return amountNeededToLevel;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> CheckLevelAdvance(Character character, CancellationToken cancellationToken = default)
        {
            var level = character.Level;

            if (level < 90)
            {
                bool didAdvance = this.GetRemainingExperienceToLevel(character, true) <= 0;

                if (didAdvance)
                {
                    await this.IncreaseLevel(character, cancellationToken);
                }

                return didAdvance;
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task ShowGroupToPlayer(UserData actor, CancellationToken cancellationToken)
        {
            if (actor.Character.GroupId.HasValue)
            {
                var group = GroupHelper.GetGroup(actor.Character.GroupId.Value);

                if (group != null && group.Value.Value.Count > 0)
                {
                    var owner = this.ResolveCharacter(actor.Character.GroupId.Value);

                    if (owner != null)
                    {
                        StringBuilder sb = new ();
                        sb.Append("<span class='group-info'>");
                        sb.Append($"<span class='group-name'>{owner.Character.FirstName}'s Group:</span>");

                        // Display the owner of the group first.
                        sb.Append($"<span class='group-member'><span class='group-member-level'>[ {owner.Character.Level} {owner.Character.RaceAbbrev} ]</span><span class='group-member-name'>{owner.Character.FirstName}</span>");
                        sb.Append($"<div class=\"group-progress\"><div class=\"progress-bar progress-bar-striped bg-danger\" role=\"progressbar\" style=\"width:{owner.Character.Health.GetPercentage()}%;\" aria-valuenow=\"{owner.Character.Health.GetPercentage()}\" aria-valuemin=\"0\" aria-valuemax=\"100\">{owner.Character.Health.GetPercentage()}%</div></div>");
                        sb.Append($"<div class=\"group-progress\"><div class=\"progress-bar progress-bar-striped\" role=\"progressbar\" style=\"width:{owner.Character.Mana.GetPercentage()}%;\" aria-valuenow=\"{owner.Character.Mana.GetPercentage()}\" aria-valuemin=\"0\" aria-valuemax=\"100\">{owner.Character.Mana.GetPercentage()}%</div></div>");
                        sb.Append($"<div class=\"group-progress\"><div class=\"progress-bar progress-bar-striped bg-success\" role=\"progressbar\" style=\"width:{owner.Character.Movement.GetPercentage()}%;\" aria-valuenow=\"{owner.Character.Movement.GetPercentage()}\" aria-valuemin=\"0\" aria-valuemax=\"100\">{owner.Character.Movement.GetPercentage()}%</div></div>");
                        sb.Append($"</span>");

                        foreach (var characterId in group.Value.Value)
                        {
                            var player = this.ResolveCharacter(characterId);

                            if (player != null && player.Character.CharacterId != owner.Character.CharacterId)
                            {
                                sb.Append($"<span class='group-member'><span class='group-member-level'>[ {player.Character.Level} {player.Character.RaceAbbrev} ]</span><span class='group-member-name'>{player.Character.FirstName}</span>");
                                sb.Append($"<div class=\"group-progress\"><div class=\"progress-bar progress-bar-striped bg-danger\" role=\"progressbar\" style=\"width:{player.Character.Health.GetPercentage()}%;\" aria-valuenow=\"{player.Character.Health.GetPercentage()}\" aria-valuemin=\"0\" aria-valuemax=\"100\">{player.Character.Health.GetPercentage()}%</div></div>");
                                sb.Append($"<div class=\"group-progress\"><div class=\"progress-bar progress-bar-striped\" role=\"progressbar\" style=\"width:{player.Character.Mana.GetPercentage()}%;\" aria-valuenow=\"{player.Character.Mana.GetPercentage()}\" aria-valuemin=\"0\" aria-valuemax=\"100\">{player.Character.Mana.GetPercentage()}%</div></div>");
                                sb.Append($"<div class=\"group-progress\"><div class=\"progress-bar progress-bar-striped bg-success\" role=\"progressbar\" style=\"width:{player.Character.Movement.GetPercentage()}%;\" aria-valuenow=\"{player.Character.Movement.GetPercentage()}\" aria-valuemin=\"0\" aria-valuemax=\"100\">{player.Character.Movement.GetPercentage()}%</div></div>");
                                sb.Append($"</span>");
                            }
                        }

                        sb.Append("</span>");

                        await this.SendToPlayer(actor.Connection, sb.ToString(), cancellationToken);
                    }
                    else
                    {
                        // Owner could not be resolved.
                        this.logger.Error("Found a group with no connected owner! Removing from group list.", this);
                        bool removed = Groups.TryRemove(actor.Character.GroupId.Value, out List<long>? dummy);
                        if (removed)
                        {
                            this.logger.Info($"Removed group {actor.Character.GroupId.Value}.", this);
                        }
                        else
                        {
                            this.logger.Error($"Unable to remove group {actor.Character.GroupId.Value}!", this);
                        }

                        await this.SendToPlayer(actor.Connection, "You are not in a group.", cancellationToken);
                    }
                }
                else
                {
                    // Zero members in group.
                    this.logger.Error("Found a group with no members! Removing from group list.", this);
                    bool removed = Groups.TryRemove(actor.Character.GroupId.Value, out List<long>? dummy);
                    if (removed)
                    {
                        this.logger.Info($"Removed group {actor.Character.GroupId.Value}.", this);
                    }
                    else
                    {
                        this.logger.Error($"Unable to remove group {actor.Character.GroupId.Value}!", this);
                    }

                    await this.SendToPlayer(actor.Connection, "You are not in a group.", cancellationToken);
                }
            }
            else
            {
                await this.SendToPlayer(actor.Connection, "You are not in a group.", cancellationToken);
            }
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
        /// Logs a user out of the game.
        /// </summary>
        /// <param name="context">The HttpContext.</param>
        /// <returns>Task.</returns>
        private static async Task Logout(HttpContext context)
        {
            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                RedirectUri = "/Login",
            };

            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme, authProperties);
        }

        /// <summary>
        /// Shows the player (or mobile) to another player.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns>String.</returns>
        private static string GetPlayerInfo(Character target)
        {
            var sb = new StringBuilder();

            sb.Append($"<span class='player-desc-title'>{target.FirstName.FirstCharToUpper()} {target.LastName}</span><br/>");
            sb.Append($"<span class='player-description'>{target.LongDescription}</span><br/>");

            // How beat up they are.
            sb.Append(CombatProcessor.GetPlayerCondition(target));

            // Worn items.
            if (target.IsNPC)
            {
                sb.Append(ActionHelper.GetOnlyEquipment(target));
            }
            else
            {
                sb.Append(ActionHelper.GetEquipment(target));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Removes the player from groups prior to quitting. Removes followers.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        private async Task UpdateGroupAndFollowers(UserData user, CancellationToken cancellationToken)
        {
            // Stop following a character.
            if (user.Character.Following.HasValue)
            {
                var following = this.ResolveCharacter(user.Character.Following.Value);

                if (following != null)
                {
                    // Remove the follower.
                    following.Character.Followers.Remove(user.Character.CharacterId);

                    await this.SendToPlayer(following.Character, $"{user.Character.FirstName} stops following you.", cancellationToken);
                    await this.SendToPlayer(user.Character, $"You stop following {following.Character.FirstName}.", cancellationToken);
                }

                user.Character.Following = null;
            }

            // Remove any followers.
            if (user.Character.Followers != null && user.Character.Followers.Count > 0)
            {
                foreach (var follower in user.Character.Followers)
                {
                    var followerChar = this.ResolveCharacter(follower);

                    if (followerChar != null)
                    {
                        await this.SendToPlayer(user.Character, $"{followerChar.Character.FirstName} stops following you.", cancellationToken);
                        await this.SendToPlayer(followerChar.Character, $"You stop following {user.Character.FirstName}.", cancellationToken);
                    }
                }

                user.Character.Followers = new List<long>();
            }

            // If they are in a group, remove or disband.
            if (user.Character.GroupId.HasValue)
            {
                // Dismantle any groups the player is leading.
                if (GroupHelper.IsGroupOwner(user.Character.CharacterId))
                {
                    Communicator.Groups.TryRemove(user.Character.CharacterId, out List<long>? players);

                    // Update the players who were removed from the group to have no group.
                    if (players != null)
                    {
                        foreach (var characterId in players)
                        {
                            var character = this.ResolveCharacter(characterId);

                            if (character != null)
                            {
                                await this.SendToPlayer(character.Character, $"{user.Character.FirstName} has disbanded the group.", cancellationToken);
                                character.Character.GroupId = null;
                                await this.SaveCharacter(character);
                            }
                        }
                    }
                }
                else
                {
                    // Not the owner, so just remove player from the group.
                    GroupHelper.RemoveFromGroup(user.Character.GroupId.Value, user.Character.CharacterId);

                    var owner = this.ResolveCharacter(user.Character.GroupId.Value);

                    if (owner != null)
                    {
                        await this.SendToPlayer(user.Character, $"You leave {owner.Character.FirstName}'s group.", cancellationToken);

                        var group = GroupHelper.GetAllGroupMembers(user.Character.GroupId.Value);

                        if (group != null)
                        {
                            foreach (var groupMember in group)
                            {
                                var member = this.ResolveCharacter(groupMember);

                                if (member != null)
                                {
                                    await this.SendToPlayer(member.Character, $"{user.Character.FirstName} has left the group.", cancellationToken);
                                }
                            }
                        }
                    }
                }

                // Ensure the player has been removed from any groups.
                GroupHelper.RemoveFromAllGroups(user.Character.CharacterId);

                // Remove from any groups they may be in.
                user.Character.GroupId = null;
            }
        }

        /// <summary>
        /// Displays spell or skill effects on a character.
        /// </summary>
        /// <param name="actor">The character.</param>
        /// <returns>string.</returns>
        private string GetEffects(Character actor)
        {
            StringBuilder sb = new ();

            if (actor.IsAffectedBy(nameof(PassDoor)) || actor.CharacterFlags.Contains(CharacterFlags.Ghost))
            {
                sb.Append("(<span class='translucent'>Translucent</span>) ");
            }

            if (actor.IsAffectedBy(nameof(Sanctuary)))
            {
                sb.Append("(<span class='sanctuary'>White Aura</span>) ");
            }

            if (actor.IsAffectedBy(nameof(Invisibility)))
            {
                sb.Append("(<span class='invisibility'>Invisible</span>) ");
            }

            if (actor.IsAffectedBy(nameof(Hide)))
            {
                sb.Append("(<span class='hide'>Hidden</span>) ");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Ensures a new character's location, home, and starting stats are all set.
        /// </summary>
        /// <param name="character">The character.</param>
        private async Task CheckNewCharacter(Character character)
        {
            if (character.Level == 1 && character.Experience == 0)
            {
                // New player starting point.
                character.Location = new KeyValuePair<long, long>(Constants.START_AREA, Constants.START_ROOM);

                switch (character.Alignment)
                {
                    case Alignment.Good:
                        {
                            character.Deity = Deities.Atrina;
                            character.Home = new KeyValuePair<long, long>(Constants.GRIFFONSHIREAREA, Constants.GRIFFONSHIRE_LIGHT_TEMPLE);
                            break;
                        }

                    case Alignment.Evil:
                        {
                            character.Deity = Deities.Saurath;
                            character.Home = new KeyValuePair<long, long>(Constants.GRIFFONSHIREAREA, Constants.GRIFFONSHIRE_DARK_TEMPLE);
                            break;
                        }

                    case Alignment.Neutral:
                        {
                            character.Deity = Deities.Khoda;
                            character.Home = new KeyValuePair<long, long>(Constants.GRIFFONSHIREAREA, Constants.GRIFFONSHIRE_NEUTRAL_TEMPLE);
                            break;
                        }
                }

                // Calculate the advance trains and practices.
                var trains = character.Int.Max / 4;
                character.Trains += (int)trains;

                var pracs = character.Wis.Max / 4;
                character.Practices += (int)pracs;

                character.Learns = 3;

                character.Experience = 1;

                var raceStats = Races.RaceData.First(r => r.Key == character.Race);

                if (raceStats.Value.Abilities != null)
                {
                    foreach (var ability in raceStats.Value.Abilities)
                    {
                        var skill = SkillHelper.ResolveSkill(ability, this, this.random, this.world, this.logger, this.combat);

                        if (skill != null)
                        {
                            character.Skills.Add(new SkillProficiency(skill.Name, 100));
                        }
                        else
                        {
                            var spell = SpellHelper.ResolveSpell(ability, this, this.random, this.world, this.logger, this.combat);

                            if (spell != null)
                            {
                                character.Spells.Add(new SpellProficiency(spell.Name, 100));
                            }
                        }
                    }
                }

                // All players speak common fluently.
                var common = SkillHelper.ResolveSkill("Common", this, this.random, this.world, this.logger, this.combat);

                if (common != null && !character.Skills.Contains(new SkillProficiency(common.Name, 100)))
                {
                    character.Skills.Add(new SkillProficiency(common.Name, 100));
                    character.Speaking = common.Name;
                }

                // All players get recall at max.
                if (!character.Skills.Contains(new SkillProficiency(nameof(Recall), 100)))
                {
                    character.Skills.Add(new SkillProficiency(nameof(Recall), 100));
                }

                // All players get their racial language at max.
                if (!character.Skills.Contains(new SkillProficiency(character.Race.ToString(), 100)))
                {
                    character.Skills.Add(new SkillProficiency(character.Race.ToString(), 100));
                }

                await this.SaveCharacter(character);
            }
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
            var hp = this.random.Next(10 + this.random.Next(2, 8), Math.Max((int)character.Con.Current + 7, 19));

            // Movement is based on dex
            var move = this.random.Next(10 + this.random.Next(2, 8), Math.Max((int)character.Dex.Current + 7, 19));

            // Mana is based on wis
            var mana = this.random.Next(10 + this.random.Next(2, 8), Math.Max((int)character.Wis.Current + 7, 19));

            character.Health.Max += hp;
            character.Mana.Max += mana;
            character.Movement.Max += move;

            // Calculate the advance trains and practices.

            // Every 5th level, a character gets a new set of trains based on their intelligence.
            if (character.Level % 5 == 0)
            {
                var trains = character.Int.Max / 4;
                character.Trains += (int)trains;
            }

            var pracs = character.Wis.Max / 4;

            // Every 5 levels, character gets a learning session.
            if (character.Level % 5 == 0)
            {
                character.Learns += 1;
            }

            character.Practices += (int)pracs;

            this.UpdateTitle(character);

            await this.SendToPlayer(character, $"You advanced a level! You gained {hp} health, {mana} mana, and {move} movement. You have {character.Trains} training sessions and {character.Practices} practices.", cancellationToken);

            this.logger.Info($"{character.FirstName} has advanced to level {character.Level}!", this);

            await this.PlaySound(character, AudioChannel.Actor, Sounds.LEVELUP, cancellationToken);

            if (character.Level % 10 == 0 && this.awardProcessor != null)
            {
                await this.awardProcessor.GrantAward((int)AwardType.Challenger, character, $"advanced to level {character.Level}", cancellationToken);
            }

            // Save all the changes.
            await this.SaveCharacter(character);
        }

        private void UpdateTitle(Character character)
        {
            var newTitle = this.titleGenerator.Generate(character);

            if (!string.IsNullOrWhiteSpace(newTitle))
            {
                character.Title = newTitle;
            }
        }

        /// <summary>
        /// Processes the user's command.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <param name="input">The input.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        private async Task ProcessMessage(UserData actor, string? input, CancellationToken cancellationToken = default)
        {
            if (input == null)
            {
                return;
            }

            try
            {
                // Check if we have received a JSON input. We only parse a select few of these.
                if (input.IsJson(out List<Command>? commandList))
                {
                    if (commandList != null)
                    {
                        foreach (var command in commandList)
                        {
                            if ((command?.Action?.ToLower() == "message") && long.TryParse(command?.Context, out long messageId))
                            {
                                // Someone sent a message, so deliver it.
                                var message = await this.messageProcessor.GetMessage(messageId, cancellationToken);
                                if (message != null)
                                {
                                    await this.messageProcessor.DeliverMessage(message, cancellationToken);
                                }
                            }
                        }
                    }
                }
                else
                {
                    CommandArgs? args = CommandArgs.ParseCommand(input);

                    if (args == null)
                    {
                        await this.SendToPlayer(actor.Connection, "You don't know how to do that.", cancellationToken);
                    }
                    else
                    {
                        // See if this is a single emote
                        var emote = Emotes.Get(args.Action);

                        if (emote != null)
                        {
                            await this.ProcessEmote(emote, actor, args, cancellationToken);
                        }
                        else
                        {
                            // See if this is a skill
                            if (!string.IsNullOrWhiteSpace(args.Action) && actor.Character.HasSkill(args.Action))
                            {
                                if (actor.Character.CharacterFlags.Contains(CharacterFlags.Sleeping))
                                {
                                    await this.SendToPlayer(actor.Connection, "You can't do that while you're sleeping.", cancellationToken);
                                    return;
                                }

                                if (this.skillProcessor != null)
                                {
                                    await this.skillProcessor.DoSkill(actor, args, cancellationToken);
                                    return;
                                }
                                else
                                {
                                    await this.SendToPlayer(actor.Connection, "You don't know how to do that.", cancellationToken);
                                    return;
                                }
                            }
                            else if (IsCasting(args.Action))
                            {
                                if (actor.Character.CharacterFlags.Contains(CharacterFlags.Sleeping))
                                {
                                    await this.SendToPlayer(actor.Connection, "You can't do that while you're sleeping.", cancellationToken);
                                    return;
                                }

                                // If casting, see what they are casting and see if they can cast it.
                                if (!string.IsNullOrWhiteSpace(args.Method))
                                {
                                    if (actor.Character.HasSpell(args.Method))
                                    {
                                        if (this.spellProcessor != null)
                                        {
                                            await this.spellProcessor.DoSpell(actor, args, cancellationToken);
                                            return;
                                        }
                                        else
                                        {
                                            await this.SendToPlayer(actor.Connection, "You don't know how to cast that.", cancellationToken);
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        await this.SendToPlayer(actor.Connection, "You don't know how to cast that.", cancellationToken);
                                        return;
                                    }
                                }
                                else
                                {
                                    await this.SendToPlayer(actor.Connection, "Commune or cast what?", cancellationToken);
                                }
                            }
                            else
                            {
                                // Not casting, using a skill, or emoting, so check actions.
                                if (this.actionProcessor != null)
                                {
                                    await this.actionProcessor.DoAction(actor, args, cancellationToken);
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                await this.SendToPlayer(actor.Connection, "You don't know how to do that.", cancellationToken);
                this.logger.Error(exc, this);
            }
        }

        private async Task ProcessEmote(Emote emote, UserData actor, CommandArgs args, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                // Simple emote with no target. "Bob nods." "You nod."
                await this.SendToPlayer(actor.Connection, emote.ToSelf, cancellationToken);

                // Get all players that are not the actor.
                var players = this.GetPlayersInRoom(actor.Character, actor.Character.Location);

                if (players != null)
                {
                    foreach (var player in players)
                    {
                        if (PlayerHelper.CanPlayerSeePlayer(this.environment, this, player, actor.Character))
                        {
                            await this.SendToPlayer(player, emote.ToRoom.Replace("{0}", actor.Character.FirstName), cancellationToken);
                        }
                        else
                        {
                            await this.SendToPlayer(player, emote.ToRoom.Replace("{0}", "someone").FirstCharToUpper(), cancellationToken);
                        }
                    }
                }

                // See if any AI mobs in the room will communicate with the player after this emote.
                var commsTask = this.CheckMobCommunication(actor.Character, actor.Character.Location, emote.ToRoom.Replace("{0}", actor.Character.FirstName), cancellationToken);

                // Run this task on a separate, synchronous thread, so we don't block. This is fire and forget.
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(async () => { await commsTask; }, cancellationToken);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            else
            {
                // Is target a player or mob?
                var targetChar = this.ResolveCharacter(args.Method);

                if (targetChar != null)
                {
                    await this.SendToPlayer(actor.Character, emote.SelfToTarget.Replace("{1}", targetChar.Character.FirstName.FirstCharToUpper()), cancellationToken);

                    // Get all players that are not the actor.
                    var players = this.GetPlayersInRoom(actor.Character, actor.Character.Location);

                    if (players != null)
                    {
                        foreach (var player in players)
                        {
                            if (PlayerHelper.CanPlayerSeePlayer(this.environment, this, player, actor.Character))
                            {
                                if (player.CharacterId == targetChar.Character.CharacterId)
                                {
                                    await this.SendToPlayer(player, emote.ToRoom.Replace("{0}", actor.Character.FirstName).Replace("{1}", "you"), cancellationToken);
                                }
                                else
                                {
                                    await this.SendToPlayer(player, emote.ToRoom.Replace("{0}", actor.Character.FirstName).Replace("{1}", targetChar.Character.FirstName), cancellationToken);
                                }
                            }
                        }
                    }
                }
                else
                {
                    var mobile = this.ResolveMobile(args.Method, actor.Character);

                    if (mobile != null)
                    {
                        await this.SendToPlayer(actor.Character, emote.SelfToTarget.Replace("{1}", mobile.FirstName), cancellationToken);

                        // Get all players that are not the actor.
                        var players = this.GetPlayersInRoom(actor.Character, actor.Character.Location);

                        if (players != null)
                        {
                            foreach (var player in players)
                            {
                                if (PlayerHelper.CanPlayerSeePlayer(this.environment, this, player, actor.Character))
                                {
                                    await this.SendToPlayer(player, emote.ToRoom.Replace("{0}", actor.Character.FirstName).Replace("{1}", mobile?.FirstName.FirstCharToUpper()), cancellationToken);
                                }
                            }
                        }

                        // See if any AI mobs in the room will communicate with the player after this emote.
                        var commsTask = this.CheckMobCommunication(actor.Character, actor.Character.Location, emote.ToRoom.Replace("{0}", actor.Character.FirstName).Replace("{1}", "you"), cancellationToken);

                        // Run this task on a separate, synchronous thread, so we don't block. This is fire and forget.
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        Task.Run(async () => { await commsTask; }, cancellationToken);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
                    else
                    {
                        await this.SendToPlayer(actor.Character, "They aren't here.", cancellationToken);
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
                    this.logger.Info("TICK.", this);

                    // Cleanup any empty groups.
                    this.CleanupGroups();

                    // See what's going on around the players.
                    var cts = new CancellationTokenSource();
                    this.environment.ProcessEnvironmentChanges(engineEventArgs.GameTicks, engineEventArgs.GameHour, cts.Token);

                    // Perform tasks on individual players.
                    foreach (var user in Users)
                    {
                        if (user.Value != null)
                        {
                            var metrics = user.Value.Character.Metrics;

                            if (metrics != null)
                            {
                                metrics.GameHoursPlayed++;
                            }

                            // Update the player info
                            this.SendGameUpdate(user.Value.Character, null, null).Wait();

                            // Autosave the user each tick.
                            this.SaveCharacter(user.Value).Wait();
                        }
                    }
                }

                // Handle any changes in the world (item rot, movement of mobs, etc).
                this.world.ProcessWorldChanges().Wait();
            }
            catch (Exception exc)
            {
                this.logger.Warn($"Error processing Tick: {exc}", this);
            }
        }

        /// <summary>
        /// Removes all groups from the communicator that are either empty, or their owner has disconnected. Updates the characters
        /// in the removed group to set their group ID to null.
        /// </summary>
        private void CleanupGroups()
        {
            List<long> groupsToRemove = new ();

            foreach (var group in Groups)
            {
                // No members in the group.
                if (group.Value == null || group.Value.Count == 0)
                {
                    groupsToRemove.Add(group.Key);
                }

                if (Users != null)
                {
                    var user = Users.Where(u => u.Value.Character.CharacterId == group.Key);

                    // Owner is no longer connected.
                    if (user == null)
                    {
                        groupsToRemove.Add(group.Key);
                    }
                }
            }

            if (groupsToRemove.Count > 0)
            {
                this.logger.Warn($"Found {groupsToRemove.Count} orphaned groups. Removing them.", this);

                foreach (var groupId in groupsToRemove)
                {
                    Groups.TryRemove(groupId, out List<long>? groupMembers);

                    if (groupMembers != null)
                    {
                        foreach (var characterId in groupMembers)
                        {
                            var character = this.ResolveCharacter(characterId);

                            if (character != null)
                            {
                                character.Character.GroupId = null;
                            }
                        }
                    }
                }
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
            if (userData.Connection == null)
            {
                return null;
            }

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
            var message = await reader.ReadToEndAsync(cancellationToken);

            await this.OnInputReceived(userData, new CommunicationEventArgs(userData.ConnectionId, message), cancellationToken);

            return message;
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

            if (user.Character.WizFlags.Contains(WizFlags.Wiznet))
            {
                this.AddToChannel("wiznet", user.ConnectionId, user);
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
