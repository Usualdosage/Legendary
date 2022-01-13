// <copyright file="Processor.cs" company="Legendary">
//  Copyright © 2021 Legendary
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
    using Legendary.Data.Contracts;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Models;

    /// <summary>
    /// Handles command processing.
    /// </summary>
    public class Processor : IProcessor, IDisposable
    {
        private readonly ILogger logger;
        private readonly ICommunicator communicator;

        /// <summary>
        /// Initializes a new instance of the <see cref="Processor"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        /// <param name="logger">The logger.</param>
        public Processor(ILogger logger, ICommunicator communicator)
        {
            this.communicator = communicator;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public IWorld World
        {
            get
            {
                return this.communicator.World;
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
                switch (args[0].ToLower())
                {
                    default:
                        {
                            await this.communicator.SendToPlayer(user.Connection, "Unknown command.");
                            break;
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

                    case "save":
                        {
                            await this.communicator.SendToPlayer(user.Connection, $"Saving...");
                            await this.communicator.Save(user.Connection, user);
                            await this.communicator.SendToPlayer(user.Connection, $"Save complete.");
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
            var area = await this.World.FindArea(a => a.AreaId == user.Character.Location.AreaId);

            if (area == null)
            {
                await this.logger.Warn($"MovePlayer: Null area found for user. {user} {user.Character.Location}!");
                return;
            }

            var room = area.Rooms.FirstOrDefault(r => r.RoomId == user.Character.Location.RoomId);

            if (room == null)
            {
                await this.logger.Warn($"MovePlayer: Null room found for user. {user} {user.Character.Location}!");
                return;
            }

            Exit? exit = room.Exits?.FirstOrDefault(e => e.Direction == direction);

            if (exit != null)
            {
                string? dir = Enum.GetName(typeof(Direction), exit.Direction)?.ToLower();
                await this.communicator.SendToPlayer(user.Connection, $"You go {dir}.<br/>");
                await this.communicator.SendToRoom(room, user.ConnectionId, $"{user.Character.FirstName} leaves {dir}.");

                var newRoom = area?.Rooms?.FirstOrDefault(r => r.RoomId == exit.ToRoom);
                if (newRoom != null)
                {
                    user.Character.Location = newRoom;
                    await this.communicator.SendToRoom(newRoom, user.ConnectionId, $"{user.Character.FirstName} enters.");
                    await this.ShowRoomToPlayer(user);
                }
            }
            else
            {
                await this.communicator.SendToPlayer(user.Connection, $"You can't go that way.");
            }
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
                await this.logger.Warn($"ShowRoomToPlayer: Null area found for user. {user} {user.Character.Location}!");
                return;
            }

            var room = area.Rooms.FirstOrDefault(r => r.RoomId == user.Character.Location.RoomId);

            if (room == null)
            {
                await this.logger.Warn($"ShowRoomToPlayer: Null room found for user. {user} {user.Character.Location}!");
                return;
            }

            StringBuilder sb = new();

            sb.Append($"<span class='room-title'>{room?.Name}</span><br/>");

            if (!string.IsNullOrWhiteSpace(room?.Image))
            {
                sb.Append($"<span class='room-image'><img src='data:image/jpeg;charset=utf-8;base64, {room?.Image}'</img><span><br/>");
            }

            sb.Append($"<span class='room-description'>{room?.Description}<span><br/>");

            // Show the items
            if (room?.Items != null)
            {
                foreach (var item in room.Items)
                {
                    if (item == null)
                    {
                        await this.logger.Warn($"ShowRoomToPlayer: Null item found for item!");
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
            if (room?.MobileResets != null)
            {
                // TODO
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
        }
    }
}



