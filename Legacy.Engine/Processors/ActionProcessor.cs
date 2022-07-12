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
    using Legendary.Engine.Helpers;

    /// <summary>
    /// Used to perform quick lookups of skills.
    /// </summary>
    public class ActionProcessor
    {
        private readonly ICommunicator communicator;
        private readonly IWorld world;
        private readonly ILogger logger;
        private IDictionary<string, KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>> actions = new Dictionary<string, KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionProcessor"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        public ActionProcessor(ICommunicator communicator, IWorld world, ILogger logger)
        {
            this.communicator = communicator;
            this.world = world;
            this.logger = logger;

            this.ConfigureActions();
        }

        /// <summary>
        /// Executes the action provided by the command.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <param name="args">The input args.</param>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task DoAction(UserData actor, string[] args, string command, CancellationToken cancellationToken)
        {
            try
            {
                // Get the matching actions for the command word.
                var action = this.actions
                    .Where(a => a.Key.StartsWith(command))
                    .OrderBy(a => a.Value.Key)
                    .FirstOrDefault();

                if (action.Value.Value != null)
                {
                    await action.Value.Value(actor, args, cancellationToken);
                }
                else
                {
                    await this.communicator.SendToPlayer(actor.Connection, "Unknown command.", cancellationToken);
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex);
                await this.communicator.SendToPlayer(actor.Connection, "<span class='error'>Unable to process command. This has been logged.</span>", cancellationToken);
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
        /// Configures all of the actions based on the input. The numeric value in the KVP is the PRIORITY in qhich the command will
        /// be executed. So, if someone types "n", it will check "north", "ne", "nw", and "newbie" in that order.
        /// </summary>
        private void ConfigureActions()
        {
            this.actions.Add("commands", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(1, new Func<UserData, string[], CancellationToken, Task>(this.DoCommands)));
            this.actions.Add("down", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(1, new Func<UserData, string[], CancellationToken, Task>(this.DoMove)));
            this.actions.Add("drop", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(2, new Func<UserData, string[], CancellationToken, Task>(this.DoDrop)));
            this.actions.Add("east", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(1, new Func<UserData, string[], CancellationToken, Task>(this.DoMove)));
            this.actions.Add("eat", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(2, new Func<UserData, string[], CancellationToken, Task>(this.DoEat)));
            this.actions.Add("emote", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(4, new Func<UserData, string[], CancellationToken, Task>(this.DoEmote)));
            this.actions.Add("equipment", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(3, new Func<UserData, string[], CancellationToken, Task>(this.DoEquipment)));
            this.actions.Add("get", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(1, new Func<UserData, string[], CancellationToken, Task>(this.DoGet)));
            this.actions.Add("goto", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(2, new Func<UserData, string[], CancellationToken, Task>(this.DoGoTo)));
            this.actions.Add("help", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(1, new Func<UserData, string[], CancellationToken, Task>(this.DoHelp)));
            this.actions.Add("inventory", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(1, new Func<UserData, string[], CancellationToken, Task>(this.DoInventory)));
            this.actions.Add("kill", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(1, new Func<UserData, string[], CancellationToken, Task>(this.DoCombat)));
            this.actions.Add("look", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(1, new Func<UserData, string[], CancellationToken, Task>(this.DoLook)));
            this.actions.Add("murder", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(1, new Func<UserData, string[], CancellationToken, Task>(this.DoCombat)));
            this.actions.Add("newbie", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(4, new Func<UserData, string[], CancellationToken, Task>(this.DoNewbieChat)));
            this.actions.Add("north", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(1, new Func<UserData, string[], CancellationToken, Task>(this.DoMove)));
            this.actions.Add("ne", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(2, new Func<UserData, string[], CancellationToken, Task>(this.DoMove)));
            this.actions.Add("nw", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(3, new Func<UserData, string[], CancellationToken, Task>(this.DoMove)));
            this.actions.Add("peace", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(1, new Func<UserData, string[], CancellationToken, Task>(this.DoPeace)));
            this.actions.Add("pray", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(2, new Func<UserData, string[], CancellationToken, Task>(this.DoPray)));
            this.actions.Add("quit", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(1, new Func<UserData, string[], CancellationToken, Task>(this.DoQuit)));
            this.actions.Add("rest", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(1, new Func<UserData, string[], CancellationToken, Task>(this.DoRest)));
            this.actions.Add("remove", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(2, new Func<UserData, string[], CancellationToken, Task>(this.DoRemove)));
            this.actions.Add("save", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(8, new Func<UserData, string[], CancellationToken, Task>(this.DoSave)));
            this.actions.Add("say", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(7, new Func<UserData, string[], CancellationToken, Task>(this.DoSay)));
            this.actions.Add("score", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(4, new Func<UserData, string[], CancellationToken, Task>(this.DoScore)));
            this.actions.Add("skills", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(5, new Func<UserData, string[], CancellationToken, Task>(this.DoSkills)));
            this.actions.Add("sleep", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(6, new Func<UserData, string[], CancellationToken, Task>(this.DoSleep)));
            this.actions.Add("south", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(1, new Func<UserData, string[], CancellationToken, Task>(this.DoMove)));
            this.actions.Add("se", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(2, new Func<UserData, string[], CancellationToken, Task>(this.DoMove)));
            this.actions.Add("sw", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(3, new Func<UserData, string[], CancellationToken, Task>(this.DoMove)));
            this.actions.Add("spells", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(8, new Func<UserData, string[], CancellationToken, Task>(this.DoSpells)));
            this.actions.Add("subscribe", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(9, new Func<UserData, string[], CancellationToken, Task>(this.DoSubscribe)));
            this.actions.Add("tell", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(0, new Func<UserData, string[], CancellationToken, Task>(this.DoTell)));
            this.actions.Add("time", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(1, new Func<UserData, string[], CancellationToken, Task>(this.DoTime)));
            this.actions.Add("unsubscribe", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(2, new Func<UserData, string[], CancellationToken, Task>(this.DoUnsubscribe)));
            this.actions.Add("up", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(1, new Func<UserData, string[], CancellationToken, Task>(this.DoMove)));
            this.actions.Add("west", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(1, new Func<UserData, string[], CancellationToken, Task>(this.DoMove)));
            this.actions.Add("wear", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(1, new Func<UserData, string[], CancellationToken, Task>(this.DoWear)));
            this.actions.Add("who", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(2, new Func<UserData, string[], CancellationToken, Task>(this.DoWho)));
            this.actions.Add("wake", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(3, new Func<UserData, string[], CancellationToken, Task>(this.DoWake)));
            this.actions.Add("wiznet", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(4, new Func<UserData, string[], CancellationToken, Task>(this.DoWiznet)));
            this.actions.Add("yell", new KeyValuePair<int, Func<UserData, string[], CancellationToken, Task>>(0, new Func<UserData, string[], CancellationToken, Task>(this.DoYell)));
        }

        private async Task DoCombat(UserData actor, string[] args, CancellationToken cancellationToken)
        {
            if (args.Length < 2)
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Who do you want to kill?", cancellationToken);
            }
            else
            {
                await this.communicator.Attack(actor, args[1], cancellationToken);
            }
        }

        private async Task DoCommands(UserData actor, string[] args, CancellationToken cancellationToken)
        {
            await this.communicator.SendToPlayer(actor.Connection, $"Available Commands:<br/>", cancellationToken);

            StringBuilder sb = new StringBuilder();

            var commands = this.actions.OrderBy(a => a.Key);

            foreach (var kvp in commands)
            {
                sb.Append($"<span class='command'>{kvp.Key}</span>");
            }

            await this.communicator.SendToPlayer(actor.Connection, sb.ToString(), cancellationToken);
        }

        private async Task DoDrop(UserData actor, string[] args, CancellationToken cancellationToken)
        {
            if (args.Length < 2)
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Drop what?", cancellationToken);
            }
            else
            {
                await this.DropItem(actor, args[1], cancellationToken);
            }
        }

        private async Task DoEat(UserData actor, string[] args, CancellationToken cancellationToken)
        {
            if (args.Length < 2)
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Eat what?", cancellationToken);
            }
            else
            {
                await this.EatItem(actor, args[1], cancellationToken);
            }
        }

        private async Task DoEmote(UserData actor, string[] args, CancellationToken cancellationToken)
        {
            if (args.Length < 2)
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Emote what?", cancellationToken);
            }
            else
            {
                var sentence = string.Join(' ', args, 1, args.Length - 1);
                if (!string.IsNullOrWhiteSpace(sentence))
                {
                    sentence = sentence.ToLower();
                    await this.communicator.SendToPlayer(actor.Connection, $"{actor.Character.FirstName} {sentence}.", cancellationToken);
                    await this.communicator.SendToRoom(actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} {sentence}.", cancellationToken);
                }
            }
        }

        private async Task DoGet(UserData actor, string[] args, CancellationToken cancellationToken)
        {
            if (args.Length < 2)
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Get what?", cancellationToken);
            }
            else
            {
                await this.GetItem(actor, args[1], cancellationToken);
            }
        }

        private async Task DoEquipment(UserData actor, string[] args, CancellationToken cancellationToken)
        {
            var equipment = ActionHelper.GetEquipment(actor.Character);
            await this.communicator.SendToPlayer(actor.Connection, equipment, cancellationToken);
        }

        private async Task DoGoTo(UserData actor, string[] args, CancellationToken cancellationToken)
        {
            if (actor.Character.Level < 100)
            {
                await this.communicator.SendToPlayer(actor.Connection, "Unknown command.", cancellationToken);
            }
            else
            {
                if (args.Length < 2)
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"Goto where?", cancellationToken);
                }
                else
                {
                    await this.GotoRoom(actor, args[1], cancellationToken);
                }
            }
        }

        private async Task DoHelp(UserData actor, string[] args, CancellationToken cancellationToken)
        {
            await this.communicator.SendToPlayer(actor.Connection, "Help text.", cancellationToken);
        }

        private async Task DoInventory(UserData actor, string[] args, CancellationToken cancellationToken)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<span class='inventory'>You are carrying:</span>");
            foreach (var item in actor.Character.Inventory)
            {
                sb.AppendLine($"<span class='inventory-item'>{item.Name}</span>");
            }

            await this.communicator.SendToPlayer(actor.Connection, sb.ToString(), cancellationToken);
        }

        private async Task DoLook(UserData actor, string[] args, CancellationToken cancellationToken)
        {
            if (args.Length > 1)
            {
                await this.communicator.ShowPlayerToPlayer(actor, args[1], cancellationToken);
            }
            else
            {
                await this.communicator.ShowRoomToPlayer(actor, cancellationToken);
            }
        }

        private async Task DoNewbieChat(UserData actor, string[] args, CancellationToken cancellationToken)
        {
            var sentence = string.Join(' ', args, 1, args.Length - 1);
            sentence = char.ToUpper(sentence[0]) + sentence[1..];
            var channel = this.communicator.Channels.FirstOrDefault(c => c.Name.ToLower() == "newbie");
            if (channel != null)
            {
                await this.communicator.SendToPlayer(actor.Connection, $"You newbie chat \"<span class='newbie'>{sentence}</span>\"", cancellationToken);
                await this.communicator.SendToChannel(channel, actor.ConnectionId, $"{actor.Character.FirstName} newbie chats \"<span class='newbie'>{sentence}</span>\"", cancellationToken);
            }
        }

        private async Task DoPeace(UserData actor, string[] args, CancellationToken cancellationToken)
        {
            if (actor.Character.Level < 100)
            {
                await this.communicator.SendToPlayer(actor.Connection, "Unknown command.", cancellationToken);
            }
            else
            {
                if (Communicator.Users != null)
                {
                    var users = Communicator.Users.Where(u => u.Value.Character.Location.RoomId == actor.Character.Location.RoomId);

                    // Stop all the users from fighting
                    foreach (var user in users)
                    {
                        user.Value.Character.CharacterFlags?.RemoveIfExists(CharacterFlags.Fighting);
                        user.Value.Character.Fighting = null;
                        user.Value.Character.Fighting = null;
                    }

                    // Stop all the mobiles from fighting
                    var mobiles = this.communicator.GetMobilesInRoom(actor.Character.Location);
                    if (mobiles != null)
                    {
                        foreach (var mob in mobiles)
                        {
                            mob.CharacterFlags?.RemoveIfExists(CharacterFlags.Fighting);
                            mob.MobileFlags?.RemoveIfExists(MobileFlags.Aggressive);
                            mob.Fighting = null;
                        }
                    }
                }

                await this.communicator.SendToPlayer(actor.Connection, "You stop all fighting in the room.", cancellationToken);
                await this.communicator.SendToRoom(actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} stops all fighting in the room.", cancellationToken);
            }
        }

        private async Task DoPray(UserData actor, string[] args, CancellationToken cancellationToken)
        {
            var sentence = string.Join(' ', args, 1, args.Length - 1);
            if (!string.IsNullOrWhiteSpace(sentence))
            {
                sentence = char.ToUpper(sentence[0]) + sentence[1..];
                var channel = this.communicator.Channels.FirstOrDefault(c => c.Name.ToLower() == "pray");
                if (channel != null)
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"You pray \"<span class='pray'>{sentence}</span>\"", cancellationToken);
                    await this.communicator.SendToChannel(channel, actor.ConnectionId, $"{actor.Character.FirstName} prays \"<span class='newbie'>{sentence}</span>\"", cancellationToken);
                }
            }
            else
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Pray what?", cancellationToken);
            }
        }

        private async Task DoQuit(UserData actor, string[] args, CancellationToken cancellationToken)
        {
            await this.communicator.SendToPlayer(actor.Connection, $"You have disconnected.", cancellationToken);
            await this.communicator.SendToRoom(actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} has left the realms.", cancellationToken);
            await this.communicator.Quit(actor.Connection, actor.Character.FirstName ?? "Someone", cancellationToken);
        }

        private async Task DoRest(UserData actor, string[] args, CancellationToken cancellationToken)
        {
            await this.communicator.SendToPlayer(actor.Connection, $"You kick back and rest.", cancellationToken);
            await this.communicator.SendToRoom(actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} kicks back and rests.", cancellationToken);
            actor.Character.CharacterFlags.AddIfNotExists(CharacterFlags.Resting);
        }

        private async Task DoMove(UserData actor, string[] args, CancellationToken cancellationToken)
        {
            await this.MovePlayer(actor, ParseDirection(args[0]), cancellationToken);
        }

        private async Task DoSave(UserData actor, string[] args, CancellationToken cancellationToken)
        {
            await this.communicator.SaveCharacter(actor);
            await this.communicator.SendToPlayer(actor.Connection, $"Character saved.", cancellationToken);
        }

        private async Task DoSay(UserData actor, string[] args, CancellationToken cancellationToken)
        {
            var sentence = string.Join(' ', args, 1, args.Length - 1);
            sentence = char.ToUpper(sentence[0]) + sentence[1..];
            await this.communicator.SendToPlayer(actor.Connection, $"You say \"<span class='say'>{sentence}</b>\"", cancellationToken);
            await this.communicator.SendToRoom(actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} says \"<span class='say'>{sentence}</span>\"", cancellationToken);
        }

        private async Task DoScore(UserData actor, string[] args, CancellationToken cancellationToken)
        {
            await this.ShowPlayerScore(actor, cancellationToken);
        }

        private async Task DoSkills(UserData actor, string[] args, CancellationToken cancellationToken)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Your skills are:<br/>");

            if (actor.Character.Skills.Count > 0)
            {
                foreach (var skill in actor.Character.Skills)
                {
                    builder.AppendLine($"{skill.SkillName} {skill.Proficiency}%");
                }
            }
            else
            {
                builder.Append("You currently have no skills.");
            }

            await this.communicator.SendToPlayer(actor.Connection, builder.ToString(), cancellationToken);
        }

        private async Task DoSleep(UserData actor, string[] args, CancellationToken cancellationToken)
        {
            await this.communicator.SendToPlayer(actor.Connection, $"You go to sleep.", cancellationToken);
            await this.communicator.SendToRoom(actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} goes to sleep.", cancellationToken);
            actor.Character.CharacterFlags.AddIfNotExists(CharacterFlags.Sleeping);
        }

        private async Task DoSpells(UserData actor, string[] args, CancellationToken cancellationToken)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Your spells are:<br/>");

            if (actor.Character.Spells.Count > 0)
            {
                foreach (var spell in actor.Character.Spells)
                {
                    builder.AppendLine($"{spell.SpellName} {spell.Proficiency}%");
                }
            }
            else
            {
                builder.Append("You currently have no spells.");
            }

            await this.communicator.SendToPlayer(actor.Connection, builder.ToString(), cancellationToken);
        }

        private async Task DoSubscribe(UserData actor, string[] args, CancellationToken cancellationToken)
        {
            var name = string.Join(' ', args, 1, args.Length - 1);
            var channel = this.communicator.Channels.FirstOrDefault(f => f.Name.ToLower() == name.ToLower());
            if (channel != null)
            {
                if (channel.AddUser(actor.ConnectionId, actor))
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"You have subscribed to the {channel.Name} channel.", cancellationToken);
                }
                else
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"Unable to subscribe to the {channel.Name} channel.", cancellationToken);
                }
            }
            else
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Unable to subscribe to {name}. Channel does not exist.", cancellationToken);
            }
        }

        private async Task DoTell(UserData actor, string[] args, CancellationToken cancellationToken)
        {
            if (args.Length < 3)
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Tell whom what?", cancellationToken);
            }
            else
            {
                var sentence = string.Join(' ', args, 2, args.Length - 2);
                await this.Tell(actor, args[1], sentence, cancellationToken);
            }
        }

        private async Task DoTime(UserData actor, string[] args, CancellationToken cancellationToken)
        {
            await this.communicator.SendToPlayer(actor.Connection, $"The system time is {DateTime.UtcNow}.", cancellationToken);
        }

        private async Task DoUnsubscribe(UserData actor, string[] args, CancellationToken cancellationToken)
        {
            var name = string.Join(' ', args, 1, args.Length - 1);

            if (string.IsNullOrWhiteSpace(name))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Unsubscribe from what?", cancellationToken);
            }
            else
            {
                var channel = this.communicator.Channels.FirstOrDefault(f => f.Name.ToLower() == name.ToLower());
                if (channel != null)
                {
                    if (channel.RemoveUser(actor.ConnectionId))
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"You have unsubscribed from the {channel.Name} channel.", cancellationToken);
                    }
                    else
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"Unable to unsubscribe from the {channel.Name} channel.", cancellationToken);
                    }
                }
                else
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"Unable to unsubscribe from {name}. Channel does not exist.", cancellationToken);
                }
            }
        }

        private async Task DoWho(UserData actor, string[] args, CancellationToken cancellationToken)
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

                    await this.communicator.SendToPlayer(actor.Connection, sb.ToString(), cancellationToken);
                }

                await this.communicator.SendToPlayer(actor.Connection, $"There are {Communicator.Users?.Count} players connected.", cancellationToken);
            }
        }

        private async Task DoWake(UserData actor, string[] args, CancellationToken cancellationToken)
        {
            await this.communicator.SendToPlayer(actor.Connection, $"You wake and and stand up.", cancellationToken);
            await this.communicator.SendToRoom(actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} wakes and stands up.", cancellationToken);
            actor.Character.CharacterFlags.RemoveIfExists(CharacterFlags.Resting);
            actor.Character.CharacterFlags.RemoveIfExists(CharacterFlags.Sleeping);
        }

        private async Task DoRemove(UserData actor, string[] args, CancellationToken cancellationToken)
        {
            // remove <argument>
            // See if they are wearing the item.
            if (args.Length > 1)
            {
                var itemName = args[1].ToLower();

                if (itemName == "all")
                {
                    foreach (var target in actor.Character.Equipment)
                    {
                        // Un-equip each item and put back in inventory.
                        actor.Character.Equipment.Remove(target);
                        actor.Character.Inventory.Add(target);
                        await this.communicator.SendToPlayer(actor.Connection, $"You remove {target.Name}.", cancellationToken);
                        await this.communicator.SendToRoom(actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} removes {target.Name}.", cancellationToken);
                    }
                }
                else
                {
                    var target = actor.Character.Equipment.FirstOrDefault(i => i.Name.Contains(itemName));

                    if (target != null)
                    {
                        // Un-equip the item and put back in inventory.
                        actor.Character.Inventory.Add(target);
                        actor.Character.Equipment.Remove(target);
                        await this.communicator.SendToPlayer(actor.Connection, $"You remove {target.Name}.", cancellationToken);
                        await this.communicator.SendToRoom(actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} removes {target.Name}.", cancellationToken);
                    }
                    else
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"You don't have that.", cancellationToken);
                    }
                }
            }
            else
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Remove what?", cancellationToken);
            }
        }

        private async Task DoWear(UserData actor, string[] args, CancellationToken cancellationToken)
        {
            // wear <argument>
            // See if they have it in their inventory.
            if (args.Length > 1)
            {
                var itemName = args[1].ToLower();

                if (itemName == "all")
                {
                    // Get everything in the player's inventory that can be worn without replacing stuff that is already worn.
                    var wornLocations = actor.Character.Equipment.Select(w => w.WearLocation).ToList();
                    var inventoryCanWear = actor.Character.Inventory.Where(i => !wornLocations.Contains(i.WearLocation)).ToList();

                    foreach (var item in inventoryCanWear)
                    {
                        // Equip the item.
                        actor.Character.Equipment.Add(item);
                        actor.Character.Inventory.Remove(item);
                        await this.communicator.SendToPlayer(actor.Connection, $"You wear {item.Name}.", cancellationToken);
                        await this.communicator.SendToRoom(actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} wears {item.Name}.", cancellationToken);
                    }

                    await this.communicator.SaveCharacter(actor);
                }
                else
                {
                    var target = actor.Character.Inventory.FirstOrDefault(i => i.Name.Contains(itemName));

                    if (target != null)
                    {
                        var equipmentToReplace = new List<Item>();

                        foreach (var wearLocation in target.WearLocation)
                        {
                            var targetLocationItem = actor.Character.Equipment.FirstOrDefault(a => a.WearLocation.Contains(wearLocation));

                            if (targetLocationItem == null)
                            {
                                // Equip the item.
                                actor.Character.Equipment.Add(target);
                                actor.Character.Inventory.Remove(target);
                                await this.communicator.SendToPlayer(actor.Connection, $"You wear {target.Name}.", cancellationToken);
                                await this.communicator.SendToRoom(actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} wears {target.Name}.", cancellationToken);
                            }
                            else
                            {
                                // Swap out the equipment.
                                equipmentToReplace.Add(targetLocationItem);
                                actor.Character.Equipment.Add(target);
                                await this.communicator.SendToPlayer(actor.Connection, $"You remove {targetLocationItem.Name}.", cancellationToken);
                                await this.communicator.SendToPlayer(actor.Connection, $"You wear {target.Name}.", cancellationToken);
                                await this.communicator.SendToRoom(actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} removes {targetLocationItem.Name}.", cancellationToken);
                                await this.communicator.SendToRoom(actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} wears {target.Name}.", cancellationToken);
                            }
                        }

                        // Remove the previously equipped items and place in inventory.
                        equipmentToReplace.ForEach(e =>
                        {
                            actor.Character.Equipment.Remove(e);
                            actor.Character.Inventory.Add(e);
                        });

                        await this.communicator.SaveCharacter(actor);
                    }
                    else
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"You don't have that.", cancellationToken);
                    }
                }
            }
            else
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Wear what?", cancellationToken);
            }
        }

        private async Task DoWiznet(UserData actor, string[] args, CancellationToken cancellationToken)
        {
            // TODO: Check if user is an IMM

            // Sub/unsub to wiznet channel
            if (this.communicator.IsSubscribed("wiznet", actor.ConnectionId, actor))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Unsubscribed from WIZNET.", cancellationToken);
                this.communicator.RemoveFromChannel("wiznet", actor.ConnectionId, actor);
            }
            else
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Welcome to WIZNET!", cancellationToken);
                this.communicator.AddToChannel("wiznet", actor.ConnectionId, actor);
            }
        }

        private async Task DoYell(UserData actor, string[] args, CancellationToken cancellationToken)
        {
            var sentence = string.Join(' ', args, 1, args.Length - 1);

            if (!string.IsNullOrWhiteSpace(sentence))
            {
                sentence = char.ToUpper(sentence[0]) + sentence[1..];
                await this.communicator.SendToPlayer(actor.Connection, $"You yell \"<span class='yell'>{sentence}!</b>\"", cancellationToken);
                await this.communicator.SendToArea(actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} yells \"<span class='yell'>{sentence}!</span>\"", cancellationToken);
            }
            else
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Yell what?", cancellationToken);
            }
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
        /// Eats an item, resetting the hunger counter.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="target">The item.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        private async Task EatItem(UserData user, string target, CancellationToken cancellationToken = default)
        {
            if (user.Character.Inventory == null || user.Character.Inventory.Count == 0)
            {
                await this.communicator.SendToPlayer(user.Connection, $"You don't have anything to drop.", cancellationToken);
                return;
            }

            foreach (var item in user.Character.Inventory)
            {
                if (item != null && item.ItemType == ItemType.Food)
                {
                    if (user.Character.Hunger.Current <= 2)
                    {
                        await this.communicator.SendToPlayer(user.Connection, $"You are too full to eat anything.", cancellationToken);
                        return;
                    }
                    else if (user.Character.Hunger.Current >= 8)
                    {
                        user.Character.Inventory.Remove(item);
                        await this.communicator.SendToPlayer(user.Connection, $"You eat {item.Name}.", cancellationToken);
                        await this.communicator.SendToPlayer(user.Connection, $"You are no longer hungry.", cancellationToken);
                        user.Character.Hunger = new MaxCurrent(24, Math.Max(user.Character.Hunger.Current - 8, 0));
                        await this.communicator.SendToRoom(user.Character.Location, user.ConnectionId, $"{user.Character.FirstName} eats {item.Name}.", cancellationToken);
                        return;
                    }
                }
            }

            await this.communicator.SendToPlayer(user.Connection, $"You can't eat that.", cancellationToken);
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
                case CommResult.NotAvailable:
                    {
                        await this.communicator.SendToPlayer(user.Connection, $"{target} can't hear you.", cancellationToken);
                        break;
                    }

                case CommResult.NotConnected:
                    {
                        await this.communicator.SendToPlayer(user.Connection, $"{target} is not here.", cancellationToken);
                        break;
                    }

                case CommResult.Ignored:
                    {
                        await this.communicator.SendToPlayer(user.Connection, $"{target} is ignoring you.", cancellationToken);
                        break;
                    }

                case CommResult.Ok:
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

            sb.Append($"<tr><td>Pierce:</td><td>{user.Character.Equipment.Sum(a => a.Pierce)}%</td><td>Blunt:</td><td>{user.Character.Equipment.Sum(a => a.Blunt)}%</td></tr>");

            sb.Append($"<tr><td>Edged:</td><td>{user.Character.Equipment.Sum(a => a.Edged)}%</td><td>Magic:</td><td>{user.Character.Equipment.Sum(a => a.Magic)}%</td></tr>");

            sb.Append($"<tr><td colspan='4'>You are not affected by any skills or spells.</td></tr>");

            sb.Append("</table></div>");

            await this.communicator.SendToPlayer(user.Connection, sb.ToString(), cancellationToken);
        }
    }
}