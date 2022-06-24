// <copyright file="Processor.cs" company="Legendary">
//  Copyright © 2021-2022 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Core.Types;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Models;

    /// <summary>
    /// Handles command processing.
    /// </summary>
    public class Processor : IProcessor, IDisposable
    {
        private readonly ILogger logger;
        private readonly ICommunicator communicator;
        private readonly IWorld world;

        /// <summary>
        /// Initializes a new instance of the <see cref="Processor"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="world">The world.</param>
        public Processor(ILogger logger, ICommunicator communicator, IWorld world)
        {
            this.communicator = communicator;
            this.logger = logger;
            this.world = world;
        }

        /// <inheritdoc/>
        public IWorld World
        {
            get
            {
                return this.world;
            }
        }

        /// <inheritdoc/>
        public async Task ProcessMessage(UserData user, string? input)
        {
            if (input == null)
            {
                return;
            }

            string[] args = input.Split(' ');

            // See if this is a single emote
            var emote = Emotes.Get(args[0]);

            if (emote != null)
            {
                await this.communicator.SendToPlayer(user.Connection, emote.ToSelf);
                await this.communicator.SendToRoom(user.Character.Location, user.ConnectionId, emote.ToRoom.Replace("{0}", user.Character.FirstName));
            }
            else
            {
                var command = args[0].ToLower();

                // Check skills first and foremost.

                if (user.Character.HasSkill(command))
                {
                    var skill = user.Character.GetSkill(command);
                    
                    if (skill != null)
                    {
                        var targetName = args.Length > 1 ? args[1] : string.Empty;

                        // We may or may not have a target. The skill will figure that bit out.
                        var target = Communicator.Users?.FirstOrDefault(u => u.Value.Username == targetName);

                        skill.Act(user, target?.Value);
                    }
                }
                else
                {

                    // Not a skill, so parse the command.
                    switch (command)
                    {
                        default:
                            {
                                await this.communicator.SendToPlayer(user.Connection, "Unknown command.");
                                break;
                            }
                        case "cast":
                            {
                                if (args.Length < 2)
                                {
                                    await this.communicator.SendToPlayer(user.Connection, $"Cast what?");
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        case "drop":
                            {
                                if (args.Length < 2)
                                {
                                    await this.communicator.SendToPlayer(user.Connection, $"Drop what?");
                                    break;
                                }
                                else
                                {
                                    await this.DropItem(user, args[1]);
                                    break;
                                }
                            }

                        case "get":
                            {
                                if (args.Length < 2)
                                {
                                    await this.communicator.SendToPlayer(user.Connection, $"Get what?");
                                    break;
                                }
                                else
                                {
                                    await this.GetItem(user, args[1]);
                                    break;
                                }
                            }

                        case "goto":
                            {
                                if (args.Length < 2)
                                {
                                    await this.communicator.SendToPlayer(user.Connection, $"Goto where?");
                                    break;
                                }
                                else
                                {
                                    await this.GotoRoom(user, args[1]);
                                    break;
                                }
                            }

                        case "h":
                        case "help":
                            {
                                await this.communicator.SendToPlayer(user.Connection, "Help text.");
                                break;
                            }

                        case "inv":
                        case "inventory":
                            {
                                var sb = new StringBuilder();
                                sb.AppendLine("<span class='inventory'>You are carrying:</span>");
                                foreach (var item in user.Character.Inventory)
                                {
                                    sb.AppendLine($"<span class='inventory-item'>{item.Name}</span>");
                                }

                                await this.communicator.SendToPlayer(user.Connection, sb.ToString());

                                break;
                            }

                        case "l":
                        case "lo":
                        case "loo":
                        case "look":
                            {
                                await this.ShowRoomToPlayer(user);
                                break;
                            }

                        case "newbie":
                            {
                                var sentence = string.Join(' ', args, 1, args.Length - 1);
                                sentence = char.ToUpper(sentence[0]) + sentence[1..];
                                var channel = this.communicator.Channels.FirstOrDefault(c => c.Name.ToLower() == "newbie");
                                if (channel != null)
                                {
                                    await this.communicator.SendToPlayer(user.Connection, $"You newbie chat \"<span class='newbie'>{sentence}</span>\"");
                                    await this.communicator.SendToChannel(channel, user.ConnectionId, $"{user.Character.FirstName} newbie chats \"<span class='newbie'>{sentence}</span>\"");
                                }

                                break;
                            }

                        case "pray":
                            {
                                var sentence = string.Join(' ', args, 1, args.Length - 1);
                                sentence = char.ToUpper(sentence[0]) + sentence[1..];
                                var channel = this.communicator.Channels.FirstOrDefault(c => c.Name.ToLower() == "pray");
                                if (channel != null)
                                {
                                    await this.communicator.SendToPlayer(user.Connection, $"You pray \"<span class='pray'>{sentence}</span>\"");
                                    await this.communicator.SendToChannel(channel, user.ConnectionId, $"{user.Character.FirstName} prays \"<span class='newbie'>{sentence}</span>\"");
                                }

                                break;
                            }

                        case "quit":
                            {
                                await this.communicator.SendToPlayer(user.Connection, $"You have disconnected.");
                                await this.communicator.SendToRoom(user.Character.Location, user.ConnectionId, $"{user.Character.FirstName} has left the realms.");
                                await this.communicator.Quit(user.Connection, user.Character.FirstName ?? "Someone");
                                break;
                            }

                        case "n":
                        case "north":
                        case "s":
                        case "south":
                        case "e":
                        case "east":
                        case "w":
                        case "west":
                        case "u":
                        case "up":
                        case "d":
                        case "down":
                        case "ne":
                        case "northeast":
                        case "nw":
                        case "northwest":
                        case "se":
                        case "southeast":
                        case "sw":
                        case "southwest":
                            {
                                await this.MovePlayer(user, ParseDirection(args[0]));
                                break;
                            }

                        case "say":
                            {
                                var sentence = string.Join(' ', args, 1, args.Length - 1);
                                sentence = char.ToUpper(sentence[0]) + sentence[1..];
                                await this.communicator.SendToPlayer(user.Connection, $"You say \"<span class='say'>{sentence}</b>\"");
                                await this.communicator.SendToRoom(user.Character.Location, user.ConnectionId, $"{user.Character.FirstName} says \"<span class='say'>{sentence}</span>\"");
                                break;
                            }
                        case "sc":
                        case "sco":
                        case "scor":
                        case "score":
                            {
                                await this.ShowPlayerScore(user);
                                break;
                            }
                        case "skill":
                        case "skills":
                            {
                                var builder = new StringBuilder();
                                builder.AppendLine("Your skills are:<br/>");

                                if (user.Character.Skills.Count > 0)
                                {
                                    foreach (var skill in user.Character.Skills)
                                    {
                                        builder.AppendLine($"{skill.Skill.Name} {skill.Proficiency}%");
                                    }
                                }
                                else
                                {
                                    builder.Append("You currently have no skills.");
                                }

                                await this.communicator.SendToPlayer(user.Connection, builder.ToString());
                                break;
                            }
                        case "spell":
                        case "spells":
                            {
                                var builder = new StringBuilder();
                                builder.AppendLine("Your spells are:<br/>");

                                if (user.Character.Spells.Count > 0)
                                {
                                    foreach (var spell in user.Character.Spells)
                                    {
                                        builder.AppendLine($"{spell.Spell.Name} {spell.Proficiency}%");
                                    }
                                }
                                else
                                {
                                    builder.Append("You currently have no spells.");
                                }

                                await this.communicator.SendToPlayer(user.Connection, builder.ToString());
                                break;
                            }
                        case "sub":
                        case "subscribe":
                            {
                                var name = string.Join(' ', args, 1, args.Length - 1);
                                var channel = this.communicator.Channels.FirstOrDefault(f => f.Name.ToLower() == name.ToLower());
                                if (channel != null)
                                {
                                    if (channel.AddUser(user.ConnectionId, user))
                                    {
                                        await this.communicator.SendToPlayer(user.Connection, $"You have subscribed to the {channel.Name} channel.");
                                    }
                                    else
                                    {
                                        await this.communicator.SendToPlayer(user.Connection, $"Unable to subscribe to the {channel.Name} channel.");
                                    }
                                }
                                else
                                {
                                    await this.communicator.SendToPlayer(user.Connection, $"Unable to subscribe to {name}. Channel does not exist.");
                                }

                                break;
                            }

                        case "tell":
                            {
                                if (args.Length < 3)
                                {
                                    await this.communicator.SendToPlayer(user.Connection, $"Tell whom what?");
                                    break;
                                }
                                else
                                {
                                    var sentence = string.Join(' ', args, 2, args.Length - 2);
                                    await this.Tell(user, args[1], sentence);
                                    break;
                                }
                            }

                        case "time":
                            {
                                await this.communicator.SendToPlayer(user.Connection, $"The system time is {DateTime.UtcNow}.");
                                break;
                            }

                        case "unsub":
                        case "unsubscribe":
                            {
                                var name = string.Join(' ', args, 1, args.Length - 1);
                                var channel = this.communicator.Channels.FirstOrDefault(f => f.Name.ToLower() == name.ToLower());
                                if (channel != null)
                                {
                                    if (channel.RemoveUser(user.ConnectionId))
                                    {
                                        await this.communicator.SendToPlayer(user.Connection, $"You have unsubscribed from the {channel.Name} channel.");
                                    }
                                    else
                                    {
                                        await this.communicator.SendToPlayer(user.Connection, $"Unable to unsubscribe from the {channel.Name} channel.");
                                    }
                                }
                                else
                                {
                                    await this.communicator.SendToPlayer(user.Connection, $"Unable to unsubscribe from {name}. Channel does not exist.");
                                }

                                break;
                            }

                        case "who":
                            {
                                if (Communicator.Users != null)
                                {
                                    foreach (KeyValuePair<string, UserData>? player in Communicator.Users)
                                    {
                                        var sb = new StringBuilder();
                                        sb.Append($"<span class='who'>{player?.Value.Character.FirstName}");

                                        if (!string.IsNullOrWhiteSpace(player?.Value.Character.LastName))
                                        {
                                            sb.Append($" {player?.Value.Character.LastName}");
                                        }

                                        if (!string.IsNullOrWhiteSpace(player?.Value.Character.Title))
                                        {
                                            sb.Append($" {player?.Value.Character.Title}");
                                        }

                                        sb.Append("</span");

                                        await this.communicator.SendToPlayer(user.Connection, sb.ToString());
                                    }

                                    await this.communicator.SendToPlayer(user.Connection, $"There are {Communicator.Users?.Count} players connected.");
                                }

                                break;
                            }

                        case "yell":
                            {
                                var sentence = string.Join(' ', args, 1, args.Length - 1);
                                sentence = char.ToUpper(sentence[0]) + sentence[1..];
                                await this.communicator.SendToPlayer(user.Connection, $"You yell \"<span class='yell'>{sentence}!</b>\"");
                                await this.communicator.SendToArea(user.Character.Location, user.ConnectionId, $"{user.Character.FirstName} yells \"<span class='yell'>{sentence}!</span>\"");
                                break;
                            }
                    }
                }
            }
        }

        /// <summary>
        /// Shows/updates the player information box.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Task.</returns>
        public async Task ShowPlayerInfo(UserData user)
        {
            StringBuilder sb = new();

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

            await this.communicator.SendToPlayer(user.Connection, sb.ToString());
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Parses a direction enum from a string value.
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <returns>Direction.</returns>
        private static Direction ParseDirection(string direction)
        {
            return direction switch
            {
                "s" or "south" => Direction.South,
                "e" or "east" => Direction.East,
                "w" or "west" => Direction.West,
                "u" or "up" => Direction.Up,
                "d" or "down" => Direction.Down,
                "ne" or "northeast" => Direction.NorthEast,
                "nw" or "northwest" => Direction.NorthWest,
                "se" or "southeast" => Direction.SouthEast,
                "sw" or "southwest" => Direction.SouthWest,
                _ => Direction.North,
            };
        }

        /// <summary>
        /// Moves the player to the specified room.
        /// </summary>
        /// <returns></returns>
        private async Task GotoRoom(UserData user, string room)
        {
            if (long.TryParse(room, out long roomId))
            {
                foreach (var area in this.World.Areas)
                {
                    var targetRoom = area.Rooms.FirstOrDefault(r => r.RoomId == roomId);
                    if (targetRoom == null)
                        continue;
                    else
                    {
                        await this.communicator.SendToPlayer(user.Connection, $"You suddenly teleport to {targetRoom.Name}.");
                        await this.communicator.SendToRoom(user.Character.Location, user.ConnectionId, $"{user.Character.FirstName} vanishes.");
                        user.Character.Location = targetRoom;
                        this.communicator.SendToServer(user, "look");
                    }
                }                
            }
        }

        /// <summary>
        /// Moves an item from a room's resets to a user's inventory.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="target">The target.</param>
        /// <returns>Task.</returns>
        private async Task GetItem(UserData user, string target)
        {
            var area = this.World.Areas.FirstOrDefault(a => a.AreaId == user.Character.Location.AreaId);
            if (area != null)
            {
                var room = area.Rooms.FirstOrDefault(r => r.RoomId == user.Character.Location.RoomId);
                if (room != null)
                {
                    if (target.ToLower() == "all")
                    {
                        List<Item> itemsToRemove = new();

                        if (room.Items == null || room.Items.Count == 0)
                        {
                            await this.communicator.SendToPlayer(user.Connection, $"There isn't anything here to get.");
                            return;
                        }

                        foreach (var item in room.Items)
                        {
                            if (item != null)
                            {
                                user.Character.Inventory.Add(item);
                                await this.communicator.SendToPlayer(user.Connection, $"You get {item.Name}.");
                                await this.communicator.SendToRoom(room, user.ConnectionId, $"{user.Character.FirstName} gets {item.Name}.");
                                itemsToRemove.Add(item);
                            }
                        }

                        foreach (var itemToRemove in itemsToRemove)
                        {
                            room.Items.Remove(itemToRemove);
                        }
                    }
                    else
                    {
                        if (room.Items == null || room.Items.Count == 0)
                        {
                            await this.communicator.SendToPlayer(user.Connection, $"There isn't anything here to get.");
                            return;
                        }

                        List<Item> itemsToRemove = new();

                        var count = 0;

                        foreach (var item in room.Items)
                        {
                            if (item != null)
                            {
                                count++;
                                user.Character.Inventory.Add(item);
                                await this.communicator.SendToPlayer(user.Connection, $"You get {item.Name}.");
                                await this.communicator.SendToRoom(room, user.ConnectionId, $"{user.Character.FirstName} gets {item.Name}.");
                                itemsToRemove.Add(item);
                            }
                        }

                        if (count == 0)
                        {
                            await this.communicator.SendToPlayer(user.Connection, $"That isn't here.");
                        }

                        foreach (var itemToRemove in itemsToRemove)
                        {
                            room.Items.Remove(itemToRemove);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Moves an item from a user's inventory into the room.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="target">The target.</param>
        /// <returns>Task.</returns>
        private async Task DropItem(UserData user, string target)
        {
            var area = this.World.Areas.FirstOrDefault(a => a.AreaId == user.Character.Location.AreaId);
            if (area != null)
            {
                var room = area.Rooms.FirstOrDefault(r => r.RoomId == user.Character.Location.RoomId);
                if (room != null)
                {
                    if (target.ToLower() == "all")
                    {
                        List<Item> itemsToRemove = new();

                        if (user.Character.Inventory == null || user.Character.Inventory.Count == 0)
                        {
                            await this.communicator.SendToPlayer(user.Connection, $"You don't have anything to drop.");
                            return;
                        }

                        foreach (var item in user.Character.Inventory)
                        {
                            if (item != null)
                            {
                                room.Items.Add(item);
                                await this.communicator.SendToPlayer(user.Connection, $"You drop {item.Name}.");
                                await this.communicator.SendToRoom(room, user.ConnectionId, $"{user.Character.FirstName} drops {item.Name}.");
                                itemsToRemove.Add(item);
                            }
                        }

                        foreach (var itemToRemove in itemsToRemove)
                        {
                            user.Character.Inventory.Remove(itemToRemove);
                        }
                    }
                    else
                    {
                        if (user.Character.Inventory == null || user.Character.Inventory.Count == 0)
                        {
                            await this.communicator.SendToPlayer(user.Connection, $"You don't have anything to drop.");
                            return;
                        }

                        List<Item> itemsToRemove = new();

                        var count = 0;

                        foreach (var item in user.Character.Inventory)
                        {
                            if (item != null)
                            {
                                count++;
                                room.Items.Add(item);
                                await this.communicator.SendToPlayer(user.Connection, $"You drop {item.Name}.");
                                await this.communicator.SendToRoom(room, user.ConnectionId, $"{user.Character.FirstName} drops {item.Name}.");
                                itemsToRemove.Add(item);
                            }
                        }

                        if (count == 0)
                        {
                            await this.communicator.SendToPlayer(user.Connection, $"You don't have that.");
                        }

                        foreach (var itemToRemove in itemsToRemove)
                        {
                            user.Character.Inventory.Remove(itemToRemove);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sends a message to a specific target.
        /// </summary>
        /// <param name="user">The sender.</param>
        /// <param name="target">The target.</param>
        /// <param name="message">The message.</param>
        /// <returns>Task.</returns>
        private async Task Tell(UserData user, string target, string message)
        {
            message = char.ToUpper(message[0]) + message[1..];
            target = char.ToUpper(target[0]) + target[1..];

            var commResult = await this.communicator.SendToPlayer(user.Connection, target, $"{user.Character.FirstName} tells you \"<span class='tell'>{message}</span>\"");
            switch (commResult)
            {
                default:
                case Types.CommResult.NotAvailable:
                    {
                        await this.communicator.SendToPlayer(user.Connection, $"{target} can't hear you.");
                        break;
                    }

                case Types.CommResult.NotConnected:
                    {
                        await this.communicator.SendToPlayer(user.Connection, $"{target} is not here.");
                        break;
                    }

                case Types.CommResult.Ignored:
                    {
                        await this.communicator.SendToPlayer(user.Connection, $"{target} is ignoring you.");
                        break;
                    }

                case Types.CommResult.Ok:
                    {
                        await this.communicator.SendToPlayer(user.Connection, $"You tell {target} \"<span class='tell'>{message}</span>\"");
                        break;
                    }
            }
        }

        /// <summary>
        /// Moves a player in a particular direction.
        /// </summary>
        /// <param name="user">UserData.</param>
        /// <param name="direction">The direction.</param>
        /// <returns>Task.</returns>
        private async Task MovePlayer(UserData user, Direction direction)
        {
            if (user.Character.Movement.Current == 0)
            {
                await this.communicator.SendToPlayer(user.Connection, $"You are too exhausted.");
                return;
            }

            var area = await this.World.FindArea(a => a.AreaId == user.Character.Location.AreaId);

            if (area == null)
            {
                this.logger.Warn($"MovePlayer: Null area found for user. {user} {user.Character.Location}!");
                return;
            }

            var room = area.Rooms.FirstOrDefault(r => r.RoomId == user.Character.Location.RoomId);

            if (room == null)
            {
                this.logger.Warn($"MovePlayer: Null room found for user. {user} {user.Character.Location}!");
                return;
            }

            Exit? exit = room.Exits?.FirstOrDefault(e => e.Direction == direction);

            if (exit != null)
            {
                var newArea = await this.World.FindArea(a => a.AreaId == exit.ToArea);
                var newRoom = newArea?.Rooms?.FirstOrDefault(r => r.RoomId == exit.ToRoom);

                if (newArea != null && newRoom != null)
                {
                    string? dir = Enum.GetName(typeof(Direction), exit.Direction)?.ToLower();
                    await this.communicator.SendToPlayer(user.Connection, $"You go {dir}.<br/>");
                    await this.communicator.SendToRoom(room, user.ConnectionId, $"{user.Character.FirstName} leaves {dir}.");

                    user.Character.Location = newRoom;

                    // TODO: Update this based on the terrain.
                    user.Character.Movement.Current -= 1;

                    await this.communicator.SendToRoom(newRoom, user.ConnectionId, $"{user.Character.FirstName} enters.");
                    await this.ShowRoomToPlayer(user);
                }
                else
                {
                    await this.communicator.SendToPlayer(user.Connection, $"You are unable to go that way.<br/>");
                }
            }
            else
            {
                await this.communicator.SendToPlayer(user.Connection, $"You can't go that way.<br/>");
            }
        }

        /// <summary>
        /// Shows the players score information.
        /// </summary>
        /// <param name="user">The connected user.</param>
        /// <returns>Task.</returns>
        private async Task ShowPlayerScore(UserData user)
        {
            StringBuilder sb = new();

            sb.Append("<div class='player-score'><table><tr><td colspan='4'>");
            sb.Append($"<span class='player-score-title'>{user.Character.FirstName} {user.Character.MiddleName} {user.Character.LastName} {user.Character.Title}</span></td></tr>");

            sb.Append($"<tr><td>Level</td><td>{user.Character.Level}</td><td>Experience</td><td>{user.Character.Experience}</td></tr>");

            sb.Append($"<tr><td>Health</td><td>{user.Character.Health.Current}/{user.Character.Health.Max}</td><td>Str</td><td>{user.Character.Str}</td></tr>");

            sb.Append($"<tr><td>Mana</td><td>{user.Character.Mana.Current}/{user.Character.Mana.Max}</td><td>Int</td><td>{user.Character.Int}</td></tr>");

            sb.Append($"<tr><td>Movement</td><td>{user.Character.Movement.Current}/{user.Character.Movement.Max}</td><td>Wis</td><td>{user.Character.Wis}</td></tr>");

            sb.Append($"<tr><td>Currency</td><td>{user.Character.Currency}</td><td>Dex</td><td>{user.Character.Dex}</td></tr>");

            sb.Append($"<tr><td colspan='2'></td><td>Con</td><td>{user.Character.Con}</td></tr>");

            sb.Append("</table></div>");

            await this.communicator.SendToPlayer(user.Connection, sb.ToString());
        }

        /// <summary>
        /// Shows the information in a room to a single player.
        /// </summary>
        /// <param name="user">The connected user.</param>
        /// <returns>Task.</returns>
        private async Task ShowRoomToPlayer(UserData user)
        {
            var area = this.World.Areas.FirstOrDefault(a => a.AreaId == user.Character.Location.AreaId);

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

            StringBuilder sb = new();

            var terrainClass = (room != null && room.Terrain.HasValue) ? Enum.GetName(room.Terrain.Value).ToLower() : "city";

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

            await this.communicator.SendToPlayer(user.Connection, sb.ToString());

            // Update player stats
            await this.ShowPlayerInfo(user);
        }
    }
}



