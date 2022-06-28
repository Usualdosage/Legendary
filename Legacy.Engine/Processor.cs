// <copyright file="Processor.cs" company="Legendary™">
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
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

        /// <summary>
        /// Gets the world.
        /// </summary>
        public IWorld World
        {
            get
            {
                return this.world;
            }
        }

        /// <inheritdoc/>
        public async Task ProcessMessage(UserData user, string? input, CancellationToken cancellationToken = default)
        {
            if (input == null)
            {
                return;
            }

            // Encode the string, otherwise the player can input HTML and have it actually render.
            input = HttpUtility.HtmlEncode(input);

            string[] args = input.Split(' ');

            // See if this is a single emote
            var emote = Emotes.Get(args[0]);

            if (emote != null)
            {
                await this.communicator.SendToPlayer(user.Connection, emote.ToSelf, cancellationToken);
                await this.communicator.SendToRoom(user.Character.Location, user.ConnectionId, emote.ToRoom.Replace("{0}", user.Character.FirstName), cancellationToken);
            }
            else
            {
                var command = args[0].ToLower();

                if (user.Character.HasSkill(command))
                {
                    var skill = user.Character.GetSkill(command);

                    if (skill != null)
                    {
                        var targetName = args.Length > 1 ? args[1] : string.Empty;

                        // We may or may not have a target. The skill will figure that bit out.
                        var target = Communicator.Users?.FirstOrDefault(u => u.Value.Username == targetName);

                        await skill.PreAction(user, target?.Value, cancellationToken);
                        await skill.Act(user, target?.Value, cancellationToken);
                        await skill.PostAction(user, target?.Value, cancellationToken);
                    }
                }
                else
                {
                    // Not a skill, so parse the command.
                    switch (command)
                    {
                        default:
                            {
                                await this.communicator.SendToPlayer(user.Connection, "Unknown command.", cancellationToken);
                                break;
                            }

                        case "cast":
                            {
                                if (args.Length < 2)
                                {
                                    await this.communicator.SendToPlayer(user.Connection, $"Cast what?", cancellationToken);
                                    break;
                                }
                                else
                                {
                                    await this.Cast(args, user, cancellationToken);
                                    break;
                                }
                            }

                        case "drop":
                            {
                                if (args.Length < 2)
                                {
                                    await this.communicator.SendToPlayer(user.Connection, $"Drop what?", cancellationToken);
                                    break;
                                }
                                else
                                {
                                    await this.DropItem(user, args[1], cancellationToken);
                                    break;
                                }
                            }

                        case "get":
                            {
                                if (args.Length < 2)
                                {
                                    await this.communicator.SendToPlayer(user.Connection, $"Get what?", cancellationToken);
                                    break;
                                }
                                else
                                {
                                    await this.GetItem(user, args[1], cancellationToken);
                                    break;
                                }
                            }

                        case "goto":
                            {
                                if (args.Length < 2)
                                {
                                    await this.communicator.SendToPlayer(user.Connection, $"Goto where?", cancellationToken);
                                    break;
                                }
                                else
                                {
                                    await this.GotoRoom(user, args[1], cancellationToken);
                                    break;
                                }
                            }

                        case "h":
                        case "help":
                            {
                                await this.communicator.SendToPlayer(user.Connection, "Help text.", cancellationToken);
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

                                await this.communicator.SendToPlayer(user.Connection, sb.ToString(), cancellationToken);

                                break;
                            }

                        case "l":
                        case "lo":
                        case "loo":
                        case "look":
                            {
                                await this.ShowRoomToPlayer(user, cancellationToken);
                                break;
                            }

                        case "newbie":
                            {
                                var sentence = string.Join(' ', args, 1, args.Length - 1);
                                sentence = char.ToUpper(sentence[0]) + sentence[1..];
                                var channel = this.communicator.Channels.FirstOrDefault(c => c.Name.ToLower() == "newbie");
                                if (channel != null)
                                {
                                    await this.communicator.SendToPlayer(user.Connection, $"You newbie chat \"<span class='newbie'>{sentence}</span>\"", cancellationToken);
                                    await this.communicator.SendToChannel(channel, user.ConnectionId, $"{user.Character.FirstName} newbie chats \"<span class='newbie'>{sentence}</span>\"", cancellationToken);
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
                                    await this.communicator.SendToPlayer(user.Connection, $"You pray \"<span class='pray'>{sentence}</span>\"", cancellationToken);
                                    await this.communicator.SendToChannel(channel, user.ConnectionId, $"{user.Character.FirstName} prays \"<span class='newbie'>{sentence}</span>\"", cancellationToken);
                                }

                                break;
                            }

                        case "quit":
                            {
                                await this.communicator.SendToPlayer(user.Connection, $"You have disconnected.", cancellationToken);
                                await this.communicator.SendToRoom(user.Character.Location, user.ConnectionId, $"{user.Character.FirstName} has left the realms.", cancellationToken);
                                await this.communicator.Quit(user.Connection, user.Character.FirstName ?? "Someone", cancellationToken);
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
                                await this.MovePlayer(user, ParseDirection(args[0]), cancellationToken);
                                break;
                            }

                        case "save":
                            {
                                await this.communicator.SaveCharacter(user);
                                await this.communicator.SendToPlayer(user.Connection, $"Character saved.", cancellationToken);
                                break;
                            }

                        case "say":
                            {
                                var sentence = string.Join(' ', args, 1, args.Length - 1);
                                sentence = char.ToUpper(sentence[0]) + sentence[1..];
                                await this.communicator.SendToPlayer(user.Connection, $"You say \"<span class='say'>{sentence}</b>\"", cancellationToken);
                                await this.communicator.SendToRoom(user.Character.Location, user.ConnectionId, $"{user.Character.FirstName} says \"<span class='say'>{sentence}</span>\"", cancellationToken);
                                break;
                            }

                        case "sc":
                        case "sco":
                        case "scor":
                        case "score":
                            {
                                await this.ShowPlayerScore(user, cancellationToken);
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

                                await this.communicator.SendToPlayer(user.Connection, builder.ToString(), cancellationToken);
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

                                await this.communicator.SendToPlayer(user.Connection, builder.ToString(), cancellationToken);
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
                                        await this.communicator.SendToPlayer(user.Connection, $"You have subscribed to the {channel.Name} channel.", cancellationToken);
                                    }
                                    else
                                    {
                                        await this.communicator.SendToPlayer(user.Connection, $"Unable to subscribe to the {channel.Name} channel.", cancellationToken);
                                    }
                                }
                                else
                                {
                                    await this.communicator.SendToPlayer(user.Connection, $"Unable to subscribe to {name}. Channel does not exist.", cancellationToken);
                                }

                                break;
                            }

                        case "tell":
                            {
                                if (args.Length < 3)
                                {
                                    await this.communicator.SendToPlayer(user.Connection, $"Tell whom what?", cancellationToken);
                                    break;
                                }
                                else
                                {
                                    var sentence = string.Join(' ', args, 2, args.Length - 2);
                                    await this.Tell(user, args[1], sentence, cancellationToken);
                                    break;
                                }
                            }

                        case "time":
                            {
                                await this.communicator.SendToPlayer(user.Connection, $"The system time is {DateTime.UtcNow}.", cancellationToken);
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
                                        await this.communicator.SendToPlayer(user.Connection, $"You have unsubscribed from the {channel.Name} channel.", cancellationToken);
                                    }
                                    else
                                    {
                                        await this.communicator.SendToPlayer(user.Connection, $"Unable to unsubscribe from the {channel.Name} channel.", cancellationToken);
                                    }
                                }
                                else
                                {
                                    await this.communicator.SendToPlayer(user.Connection, $"Unable to unsubscribe from {name}. Channel does not exist.", cancellationToken);
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

                                        await this.communicator.SendToPlayer(user.Connection, sb.ToString(), cancellationToken);
                                    }

                                    await this.communicator.SendToPlayer(user.Connection, $"There are {Communicator.Users?.Count} players connected.", cancellationToken);
                                }

                                break;
                            }

                        case "wiznet":
                            {
                                // TODO: Check if user is an IMM

                                // Sub/unsub to wiznet channel
                                if (this.communicator.IsSubscribed("wiznet", user.ConnectionId, user))
                                {
                                    await this.communicator.SendToPlayer(user.Connection, $"Unsubscribed from WIZNET.", cancellationToken);
                                    this.communicator.RemoveFromChannel("wiznet", user.ConnectionId, user);
                                }
                                else
                                {
                                    await this.communicator.SendToPlayer(user.Connection, $"Welcome to WIZNET!", cancellationToken);
                                    this.communicator.AddToChannel("wiznet", user.ConnectionId, user);
                                }

                                break;
                            }

                        case "yell":
                            {
                                var sentence = string.Join(' ', args, 1, args.Length - 1);
                                sentence = char.ToUpper(sentence[0]) + sentence[1..];
                                await this.communicator.SendToPlayer(user.Connection, $"You yell \"<span class='yell'>{sentence}!</b>\"", cancellationToken);
                                await this.communicator.SendToArea(user.Character.Location, user.ConnectionId, $"{user.Character.FirstName} yells \"<span class='yell'>{sentence}!</span>\"", cancellationToken);
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
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

            await this.communicator.SendToPlayer(user.Connection, sb.ToString(), cancellationToken);
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
        /// <param name="user">The target user.</param>
        /// <param name="room">The room to go to.</param>
        /// <param name="cancellationToken">The dancellation token.</param>
        /// <returns>Task.</returns>
        private async Task GotoRoom(UserData user, string room, CancellationToken cancellationToken = default)
        {
            if (long.TryParse(room, out long roomId))
            {
                foreach (var area in this.World.Areas)
                {
                    var targetRoom = area.Rooms.FirstOrDefault(r => r.RoomId == roomId);
                    if (targetRoom == null)
                    {
                        continue;
                    }
                    else
                    {
                        await this.communicator.SendToPlayer(user.Connection, $"You suddenly teleport to {targetRoom.Name}.", cancellationToken);
                        await this.communicator.SendToRoom(user.Character.Location, user.ConnectionId, $"{user.Character.FirstName} vanishes.", cancellationToken);
                        user.Character.Location = targetRoom;
                        this.communicator.SendToServer(user, "look");
                    }
                }
            }
        }

        /// <summary>
        /// Casts a spell on a player or target.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="user">The user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        private async Task Cast(string[] args, UserData user, CancellationToken cancellationToken = default)
        {
            // cast <spell>
            // cast <spell> <target>
            var spellName = args[1].ToLower();

            // Check if the user has the spell.
            if (user.Character.HasSpell(spellName))
            {
                var spell = user.Character.GetSpell(spellName);

                if (spell != null)
                {
                    var targetName = args.Length > 1 ? args[1] : string.Empty;

                    // We may or may not have a target. The spell will figure that bit out.
                    var target = Communicator.Users?.FirstOrDefault(u => u.Value.Username == targetName);

                    await spell.Act(user, target?.Value, cancellationToken);
                }
            }
            else
            {
                await this.communicator.SendToPlayer(user.Connection, "You don't know how to cast that.", cancellationToken);
            }
        }

        /// <summary>
        /// Moves an item from a room's resets to a user's inventory.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="target">The target.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        private async Task GetItem(UserData user, string target, CancellationToken cancellationToken = default)
        {
            // TODO: This area call is probably not necessary as long as room IDs are unique.
            var area = this.World.Areas.FirstOrDefault(a => a.AreaId == user.Character.Location.AreaId);
            if (area != null)
            {
                var room = area.Rooms.FirstOrDefault(r => r.RoomId == user.Character.Location.RoomId);
                if (room != null)
                {
                    if (target.ToLower() == "all")
                    {
                        List<Item> itemsToRemove = new ();

                        if (room.Items == null || room.Items.Count == 0)
                        {
                            await this.communicator.SendToPlayer(user.Connection, $"There isn't anything here to get.", cancellationToken);
                            return;
                        }

                        foreach (var item in room.Items)
                        {
                            if (item != null)
                            {
                                user.Character.Inventory.Add(item);
                                await this.communicator.SendToPlayer(user.Connection, $"You get {item.Name}.", cancellationToken);
                                await this.communicator.SendToRoom(room, user.ConnectionId, $"{user.Character.FirstName} gets {item.Name}.", cancellationToken);
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
                            await this.communicator.SendToPlayer(user.Connection, $"There isn't anything here to get.", cancellationToken);
                            return;
                        }

                        List<Item> itemsToRemove = new ();

                        var count = 0;

                        foreach (var item in room.Items)
                        {
                            if (item != null)
                            {
                                count++;
                                user.Character.Inventory.Add(item);
                                await this.communicator.SendToPlayer(user.Connection, $"You get {item.Name}.", cancellationToken);
                                await this.communicator.SendToRoom(room, user.ConnectionId, $"{user.Character.FirstName} gets {item.Name}.", cancellationToken);
                                itemsToRemove.Add(item);
                            }
                        }

                        if (count == 0)
                        {
                            await this.communicator.SendToPlayer(user.Connection, $"That isn't here.", cancellationToken);
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        private async Task DropItem(UserData user, string target, CancellationToken cancellationToken = default)
        {
            var area = this.World.Areas.FirstOrDefault(a => a.AreaId == user.Character.Location.AreaId);
            if (area != null)
            {
                var room = area.Rooms.FirstOrDefault(r => r.RoomId == user.Character.Location.RoomId);
                if (room != null)
                {
                    if (target.ToLower() == "all")
                    {
                        List<Item> itemsToRemove = new ();

                        if (user.Character.Inventory == null || user.Character.Inventory.Count == 0)
                        {
                            await this.communicator.SendToPlayer(user.Connection, $"You don't have anything to drop.", cancellationToken);
                            return;
                        }

                        foreach (var item in user.Character.Inventory)
                        {
                            if (item != null)
                            {
                                room.Items.Add(item);
                                await this.communicator.SendToPlayer(user.Connection, $"You drop {item.Name}.", cancellationToken);
                                await this.communicator.SendToRoom(room, user.ConnectionId, $"{user.Character.FirstName} drops {item.Name}.", cancellationToken);
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
                            await this.communicator.SendToPlayer(user.Connection, $"You don't have anything to drop.", cancellationToken);
                            return;
                        }

                        List<Item> itemsToRemove = new ();

                        var count = 0;

                        foreach (var item in user.Character.Inventory)
                        {
                            if (item != null)
                            {
                                count++;
                                room.Items.Add(item);
                                await this.communicator.SendToPlayer(user.Connection, $"You drop {item.Name}.", cancellationToken);
                                await this.communicator.SendToRoom(room, user.ConnectionId, $"{user.Character.FirstName} drops {item.Name}.", cancellationToken);
                                itemsToRemove.Add(item);
                            }
                        }

                        if (count == 0)
                        {
                            await this.communicator.SendToPlayer(user.Connection, $"You don't have that.", cancellationToken);
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        private async Task Tell(UserData user, string target, string message, CancellationToken cancellationToken = default)
        {
            message = char.ToUpper(message[0]) + message[1..];
            target = char.ToUpper(target[0]) + target[1..];

            var commResult = await this.communicator.SendToPlayer(user.Connection, target, $"{user.Character.FirstName} tells you \"<span class='tell'>{message}</span>\"", cancellationToken);
            switch (commResult)
            {
                default:
                case Types.CommResult.NotAvailable:
                    {
                        await this.communicator.SendToPlayer(user.Connection, $"{target} can't hear you.", cancellationToken);
                        break;
                    }

                case Types.CommResult.NotConnected:
                    {
                        await this.communicator.SendToPlayer(user.Connection, $"{target} is not here.", cancellationToken);
                        break;
                    }

                case Types.CommResult.Ignored:
                    {
                        await this.communicator.SendToPlayer(user.Connection, $"{target} is ignoring you.", cancellationToken);
                        break;
                    }

                case Types.CommResult.Ok:
                    {
                        await this.communicator.SendToPlayer(user.Connection, $"You tell {target} \"<span class='tell'>{message}</span>\"", cancellationToken);
                        break;
                    }
            }
        }

        /// <summary>
        /// Moves a player in a particular direction.
        /// </summary>
        /// <param name="user">UserData.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        private async Task MovePlayer(UserData user, Direction direction, CancellationToken cancellationToken = default)
        {
            if (user.Character.Movement.Current == 0)
            {
                await this.communicator.SendToPlayer(user.Connection, $"You are too exhausted.", cancellationToken);
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
                    await this.communicator.SendToPlayer(user.Connection, $"You go {dir}.<br/>", cancellationToken);
                    await this.communicator.SendToRoom(room, user.ConnectionId, $"{user.Character.FirstName} leaves {dir}.", cancellationToken);

                    user.Character.Location = newRoom;

                    // TODO: Update this based on the terrain.
                    user.Character.Movement.Current -= 1;

                    await this.communicator.SendToRoom(newRoom, user.ConnectionId, $"{user.Character.FirstName} enters.", cancellationToken);
                    await this.ShowRoomToPlayer(user, cancellationToken);
                }
                else
                {
                    await this.communicator.SendToPlayer(user.Connection, $"You are unable to go that way.<br/>", cancellationToken);
                }
            }
            else
            {
                await this.communicator.SendToPlayer(user.Connection, $"You can't go that way.<br/>", cancellationToken);
            }
        }

        /// <summary>
        /// Shows the players score information.
        /// </summary>
        /// <param name="user">The connected user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        private async Task ShowPlayerScore(UserData user, CancellationToken cancellationToken = default)
        {
            StringBuilder sb = new ();

            string homeTown = user.Character.Home?.Name ?? "nowhere";

            sb.Append("<div class='player-score'><table><tr><td colspan='4'>");
            sb.Append($"<span class='player-score-title'>{user.Character.FirstName} {user.Character.MiddleName} {user.Character.LastName} {user.Character.Title}</span></td></tr>");
            sb.Append($"<tr><td colspan='4'>You are a level {user.Character.Level} {user.Character.Race} from {homeTown}.</td></tr>");
            sb.Append($"<tr><td colspan='2'>You are {user.Character.Age} years of age.</td><td>Experience:</td><td>{user.Character.Experience}</td></tr>");

            sb.Append($"<tr><td>Health:</td><td>{user.Character.Health.Current}/{user.Character.Health.Max}</td><td>Str:</td><td>{user.Character.Str}</td></tr>");

            sb.Append($"<tr><td>Mana:</td><td>{user.Character.Mana.Current}/{user.Character.Mana.Max}</td><td>Int:</td><td>{user.Character.Int}</td></tr>");

            sb.Append($"<tr><td>Movement:</td><td>{user.Character.Movement.Current}/{user.Character.Movement.Max}</td><td>Wis:</td><td>{user.Character.Wis}</td></tr>");

            sb.Append($"<tr><td>Currency:</td><td>{user.Character.Currency}</td><td>Dex:</td><td>{user.Character.Dex}</td></tr>");

            sb.Append($"<tr><td colspan='2'>&nbsp;</td><td>Con:</td><td>{user.Character.Con}</td></tr>");

            sb.Append($"<tr><td colspan='4'>You are not affected by any skills or spells.</td></tr>");

            sb.Append("</table></div>");

            await this.communicator.SendToPlayer(user.Connection, sb.ToString(), cancellationToken);
        }

        /// <summary>
        /// Shows the information in a room to a single player.
        /// </summary>
        /// <param name="user">The connected user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        private async Task ShowRoomToPlayer(UserData user, CancellationToken cancellationToken = default)
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

            await this.communicator.SendToPlayer(user.Connection, sb.ToString(), cancellationToken);

            // Update player stats
            await this.ShowPlayerInfo(user, cancellationToken);
        }
    }
}
