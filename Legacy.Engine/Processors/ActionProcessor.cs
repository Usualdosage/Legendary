// <copyright file="ActionProcessor.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Processors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core.Contracts;
    using Legendary.Core.Extensions;
    using Legendary.Core.Models;
    using Legendary.Core.Types;
    using Legendary.Engine.Contracts;

    /// <summary>
    /// Used to perform quick lookups of skills.
    /// </summary>
    public class ActionProcessor
    {
        private readonly UserData actor;
        private readonly ICommunicator communicator;
        private readonly IWorld world;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionProcessor"/> class.
        /// </summary>
        /// <param name="actor">The this.actor.</param>
        /// <param name="communicator">The communicator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        public ActionProcessor(UserData actor, ICommunicator communicator, IWorld world, ILogger logger)
        {
            this.actor = actor;
            this.communicator = communicator;
            this.world = world;
            this.logger = logger;
        }

        /// <summary>
        /// Executes the action provided by the command.
        /// </summary>
        /// <param name="args">The input args.</param>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task DoAction(string[] args, string command, CancellationToken cancellationToken)
        {
            // Not a skill or a spell, so parse the command.
            switch (command.ToLower())
            {
                default:
                    {
                        await this.communicator.SendToPlayer(this.actor.Connection, "Unknown command.", cancellationToken);
                        break;
                    }

                case "drop":
                    {
                        if (args.Length < 2)
                        {
                            await this.communicator.SendToPlayer(this.actor.Connection, $"Drop what?", cancellationToken);
                            break;
                        }
                        else
                        {
                            await this.DropItem(this.actor, args[1], cancellationToken);
                            break;
                        }
                    }

                case "eq" or "equip":
                    {
                        await this.ShowPlayerEquipment(this.actor, cancellationToken);
                        break;
                    }

                case "get":
                    {
                        if (args.Length < 2)
                        {
                            await this.communicator.SendToPlayer(this.actor.Connection, $"Get what?", cancellationToken);
                            break;
                        }
                        else
                        {
                            await this.GetItem(this.actor, args[1], cancellationToken);
                            break;
                        }
                    }

                case "goto":
                    {
                        if (this.actor.Character.Level < 100)
                        {
                            await this.communicator.SendToPlayer(this.actor.Connection, "Unknown command.", cancellationToken);
                            break;
                        }
                        else
                        {
                            if (args.Length < 2)
                            {
                                await this.communicator.SendToPlayer(this.actor.Connection, $"Goto where?", cancellationToken);
                                break;
                            }
                            else
                            {
                                await this.GotoRoom(this.actor, args[1], cancellationToken);
                                break;
                            }
                        }
                    }

                case "h":
                case "help":
                    {
                        await this.communicator.SendToPlayer(this.actor.Connection, "Help text.", cancellationToken);
                        break;
                    }

                case "inv" or "inventory":
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine("<span class='inventory'>You are carrying:</span>");
                        foreach (var item in this.actor.Character.Inventory)
                        {
                            sb.AppendLine($"<span class='inventory-item'>{item.Name}</span>");
                        }

                        await this.communicator.SendToPlayer(this.actor.Connection, sb.ToString(), cancellationToken);

                        break;
                    }

                case "l" or "lo" or "loo" or "look":
                    {
                        if (args.Length > 1)
                        {
                            await this.communicator.ShowPlayerToPlayer(this.actor, args[1], cancellationToken);
                        }
                        else
                        {
                            await this.communicator.ShowRoomToPlayer(this.actor, cancellationToken);
                        }

                        break;
                    }

                case "newbie":
                    {
                        var sentence = string.Join(' ', args, 1, args.Length - 1);
                        sentence = char.ToUpper(sentence[0]) + sentence[1..];
                        var channel = this.communicator.Channels.FirstOrDefault(c => c.Name.ToLower() == "newbie");
                        if (channel != null)
                        {
                            await this.communicator.SendToPlayer(this.actor.Connection, $"You newbie chat \"<span class='newbie'>{sentence}</span>\"", cancellationToken);
                            await this.communicator.SendToChannel(channel, this.actor.ConnectionId, $"{this.actor.Character.FirstName} newbie chats \"<span class='newbie'>{sentence}</span>\"", cancellationToken);
                        }

                        break;
                    }

                case "peace":
                    {
                        if (this.actor.Character.Level < 100)
                        {
                            await this.communicator.SendToPlayer(this.actor.Connection, "Unknown command.", cancellationToken);
                            break;
                        }
                        else
                        {
                            if (Communicator.Users != null)
                            {
                                var users = Communicator.Users.Where(u => u.Value.Character.Location.RoomId == this.actor.Character.Location.RoomId);

                                // Stop all the users from fighting
                                foreach (var user in users)
                                {
                                    user.Value.Character.CharacterFlags?.RemoveIfExists(CharacterFlags.Fighting);
                                    user.Value.Character.FightingCharacter = null;
                                    user.Value.Character.FightingMobile = null;
                                }

                                // Stop all the mobiles from fighting
                                var mobiles = this.communicator.GetMobilesInRoom(this.actor.Character.Location);
                                if (mobiles != null)
                                {
                                    foreach (var mob in mobiles)
                                    {
                                        mob.CharacterFlags?.RemoveIfExists(CharacterFlags.Fighting);
                                        mob.MobileFlags?.RemoveIfExists(MobileFlags.Aggressive);
                                        mob.FightingMobile = null;
                                        mob.FightingCharacter = null;
                                    }
                                }
                            }

                            await this.communicator.SendToPlayer(this.actor.Connection, "You stop all fighting in the room.", cancellationToken);
                            await this.communicator.SendToRoom(this.actor.Character.Location, this.actor.ConnectionId, $"{this.actor.Character.FirstName} stops all fighting in the room.", cancellationToken);

                            break;
                        }
                    }

                case "pray":
                    {
                        var sentence = string.Join(' ', args, 1, args.Length - 1);
                        sentence = char.ToUpper(sentence[0]) + sentence[1..];
                        var channel = this.communicator.Channels.FirstOrDefault(c => c.Name.ToLower() == "pray");
                        if (channel != null)
                        {
                            await this.communicator.SendToPlayer(this.actor.Connection, $"You pray \"<span class='pray'>{sentence}</span>\"", cancellationToken);
                            await this.communicator.SendToChannel(channel, this.actor.ConnectionId, $"{this.actor.Character.FirstName} prays \"<span class='newbie'>{sentence}</span>\"", cancellationToken);
                        }

                        break;
                    }

                case "quit":
                    {
                        await this.communicator.SendToPlayer(this.actor.Connection, $"You have disconnected.", cancellationToken);
                        await this.communicator.SendToRoom(this.actor.Character.Location, this.actor.ConnectionId, $"{this.actor.Character.FirstName} has left the realms.", cancellationToken);
                        await this.communicator.Quit(this.actor.Connection, this.actor.Character.FirstName ?? "Someone", cancellationToken);
                        break;
                    }

                case "rest":
                    {
                        await this.communicator.SendToPlayer(this.actor.Connection, $"You kick back and rest.", cancellationToken);
                        await this.communicator.SendToRoom(this.actor.Character.Location, this.actor.ConnectionId, $"{this.actor.Character.FirstName} kicks back and rests.", cancellationToken);
                        this.actor.Character.CharacterFlags.AddIfNotExists(CharacterFlags.Resting);
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
                        await this.MovePlayer(this.actor, ParseDirection(args[0]), cancellationToken);
                        break;
                    }

                case "save":
                    {
                        await this.communicator.SaveCharacter(this.actor);
                        await this.communicator.SendToPlayer(this.actor.Connection, $"Character saved.", cancellationToken);
                        break;
                    }

                case "say":
                    {
                        var sentence = string.Join(' ', args, 1, args.Length - 1);
                        sentence = char.ToUpper(sentence[0]) + sentence[1..];
                        await this.communicator.SendToPlayer(this.actor.Connection, $"You say \"<span class='say'>{sentence}</b>\"", cancellationToken);
                        await this.communicator.SendToRoom(this.actor.Character.Location, this.actor.ConnectionId, $"{this.actor.Character.FirstName} says \"<span class='say'>{sentence}</span>\"", cancellationToken);
                        break;
                    }

                case "sc" or "sco" or "scor" or "score":
                    {
                        await this.ShowPlayerScore(this.actor, cancellationToken);
                        break;
                    }

                case "skill" or "skills":
                    {
                        var builder = new StringBuilder();
                        builder.AppendLine("Your skills are:<br/>");

                        if (this.actor.Character.Skills.Count > 0)
                        {
                            foreach (var skill in this.actor.Character.Skills)
                            {
                                builder.AppendLine($"{skill.SkillName} {skill.Proficiency}%");
                            }
                        }
                        else
                        {
                            builder.Append("You currently have no skills.");
                        }

                        await this.communicator.SendToPlayer(this.actor.Connection, builder.ToString(), cancellationToken);
                        break;
                    }

                case "sleep":
                    {
                        await this.communicator.SendToPlayer(this.actor.Connection, $"You go to sleep.", cancellationToken);
                        await this.communicator.SendToRoom(this.actor.Character.Location, this.actor.ConnectionId, $"{this.actor.Character.FirstName} goes to sleep.", cancellationToken);
                        this.actor.Character.CharacterFlags.AddIfNotExists(CharacterFlags.Sleeping);
                        break;
                    }

                case "spell" or "spells":
                    {
                        var builder = new StringBuilder();
                        builder.AppendLine("Your spells are:<br/>");

                        if (this.actor.Character.Spells.Count > 0)
                        {
                            foreach (var spell in this.actor.Character.Spells)
                            {
                                builder.AppendLine($"{spell.SpellName} {spell.Proficiency}%");
                            }
                        }
                        else
                        {
                            builder.Append("You currently have no spells.");
                        }

                        await this.communicator.SendToPlayer(this.actor.Connection, builder.ToString(), cancellationToken);
                        break;
                    }

                case "sub" or "subscribe":
                    {
                        var name = string.Join(' ', args, 1, args.Length - 1);
                        var channel = this.communicator.Channels.FirstOrDefault(f => f.Name.ToLower() == name.ToLower());
                        if (channel != null)
                        {
                            if (channel.AddUser(this.actor.ConnectionId, this.actor))
                            {
                                await this.communicator.SendToPlayer(this.actor.Connection, $"You have subscribed to the {channel.Name} channel.", cancellationToken);
                            }
                            else
                            {
                                await this.communicator.SendToPlayer(this.actor.Connection, $"Unable to subscribe to the {channel.Name} channel.", cancellationToken);
                            }
                        }
                        else
                        {
                            await this.communicator.SendToPlayer(this.actor.Connection, $"Unable to subscribe to {name}. Channel does not exist.", cancellationToken);
                        }

                        break;
                    }

                case "tell":
                    {
                        if (args.Length < 3)
                        {
                            await this.communicator.SendToPlayer(this.actor.Connection, $"Tell whom what?", cancellationToken);
                            break;
                        }
                        else
                        {
                            var sentence = string.Join(' ', args, 2, args.Length - 2);
                            await this.Tell(this.actor, args[1], sentence, cancellationToken);
                            break;
                        }
                    }

                case "time":
                    {
                        await this.communicator.SendToPlayer(this.actor.Connection, $"The system time is {DateTime.UtcNow}.", cancellationToken);
                        break;
                    }

                case "unsub" or "unsubscribe":
                    {
                        var name = string.Join(' ', args, 1, args.Length - 1);
                        var channel = this.communicator.Channels.FirstOrDefault(f => f.Name.ToLower() == name.ToLower());
                        if (channel != null)
                        {
                            if (channel.RemoveUser(this.actor.ConnectionId))
                            {
                                await this.communicator.SendToPlayer(this.actor.Connection, $"You have unsubscribed from the {channel.Name} channel.", cancellationToken);
                            }
                            else
                            {
                                await this.communicator.SendToPlayer(this.actor.Connection, $"Unable to unsubscribe from the {channel.Name} channel.", cancellationToken);
                            }
                        }
                        else
                        {
                            await this.communicator.SendToPlayer(this.actor.Connection, $"Unable to unsubscribe from {name}. Channel does not exist.", cancellationToken);
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

                                await this.communicator.SendToPlayer(this.actor.Connection, sb.ToString(), cancellationToken);
                            }

                            await this.communicator.SendToPlayer(this.actor.Connection, $"There are {Communicator.Users?.Count} players connected.", cancellationToken);
                        }

                        break;
                    }

                case "wake":
                    {
                        await this.communicator.SendToPlayer(this.actor.Connection, $"You wake and and stand up.", cancellationToken);
                        await this.communicator.SendToRoom(this.actor.Character.Location, this.actor.ConnectionId, $"{this.actor.Character.FirstName} wakes and stands up.", cancellationToken);
                        this.actor.Character.CharacterFlags.RemoveIfExists(CharacterFlags.Resting);
                        this.actor.Character.CharacterFlags.RemoveIfExists(CharacterFlags.Sleeping);
                        break;
                    }

                case "wiznet":
                    {
                        // TODO: Check if user is an IMM

                        // Sub/unsub to wiznet channel
                        if (this.communicator.IsSubscribed("wiznet", this.actor.ConnectionId, this.actor))
                        {
                            await this.communicator.SendToPlayer(this.actor.Connection, $"Unsubscribed from WIZNET.", cancellationToken);
                            this.communicator.RemoveFromChannel("wiznet", this.actor.ConnectionId, this.actor);
                        }
                        else
                        {
                            await this.communicator.SendToPlayer(this.actor.Connection, $"Welcome to WIZNET!", cancellationToken);
                            this.communicator.AddToChannel("wiznet", this.actor.ConnectionId, this.actor);
                        }

                        break;
                    }

                case "yell":
                    {
                        var sentence = string.Join(' ', args, 1, args.Length - 1);
                        sentence = char.ToUpper(sentence[0]) + sentence[1..];
                        await this.communicator.SendToPlayer(this.actor.Connection, $"You yell \"<span class='yell'>{sentence}!</b>\"", cancellationToken);
                        await this.communicator.SendToArea(this.actor.Character.Location, this.actor.ConnectionId, $"{this.actor.Character.FirstName} yells \"<span class='yell'>{sentence}!</span>\"", cancellationToken);
                        break;
                    }
            }
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
                foreach (var area in this.world.Areas)
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
                        await this.communicator.ShowRoomToPlayer(user, cancellationToken);
                    }
                }
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
            var area = this.world.Areas.FirstOrDefault(a => a.AreaId == user.Character.Location.AreaId);
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
            var area = this.world.Areas.FirstOrDefault(a => a.AreaId == user.Character.Location.AreaId);
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
            if (user.Character.CharacterFlags.Contains(CharacterFlags.Resting))
            {
                await this.communicator.SendToPlayer(user.Connection, "You're far too relaxed.", cancellationToken);
                return;
            }

            if (user.Character.Movement.Current == 0)
            {
                await this.communicator.SendToPlayer(user.Connection, $"You are too exhausted.", cancellationToken);
                return;
            }

            var area = await this.world.FindArea(a => a.AreaId == user.Character.Location.AreaId);

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
                var newArea = await this.world.FindArea(a => a.AreaId == exit.ToArea);
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
                    await this.communicator.ShowRoomToPlayer(user, cancellationToken);
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

            sb.Append($"<tr><td class='player-armor' colspan='4'>Armor</td></tr>");

            sb.Append($"<tr><td>Pierce:</td><td>{user.Character.Armor.Sum(a => a.Pierce)}%</td><td>Blunt:</td><td>{user.Character.Armor.Sum(a => a.Blunt)}%</td></tr>");

            sb.Append($"<tr><td>Edged:</td><td>{user.Character.Armor.Sum(a => a.Edged)}%</td><td>Magic:</td><td>{user.Character.Armor.Sum(a => a.Magic)}%</td></tr>");

            sb.Append($"<tr><td colspan='4'>You are not affected by any skills or spells.</td></tr>");

            sb.Append("</table></div>");

            await this.communicator.SendToPlayer(user.Connection, sb.ToString(), cancellationToken);
        }

        /// <summary>
        /// Shows the equipment the player is wearing.
        /// </summary>
        /// <param name="user">The connected user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        private async Task ShowPlayerEquipment(UserData user, CancellationToken cancellationToken = default)
        {

        }
    }
}