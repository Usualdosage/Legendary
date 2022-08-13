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
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core;
    using Legendary.Core.Contracts;
    using Legendary.Core.Extensions;
    using Legendary.Core.Models;
    using Legendary.Core.Types;
    using Legendary.Engine.Attributes;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Extensions;
    using Legendary.Engine.Helpers;
    using Legendary.Engine.Models;
    using Microsoft.AspNetCore.Hosting;
    using MongoDB.Driver;

    /// <summary>
    /// Used to perform quick lookups of skills.
    /// </summary>
    public partial class ActionProcessor
    {
        private readonly ICommunicator communicator;
        private readonly IWorld world;
        private readonly ILogger logger;
        private readonly ActionHelper actionHelper;
        private readonly IRandom random;
        private readonly Combat combat;
        private IDictionary<string, KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>> actions = new Dictionary<string, KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>>();
        private IDictionary<string, KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>> wizActions = new Dictionary<string, KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionProcessor"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="random">The Random Number generator.</param>
        /// <param name="combat">The combat class.</param>
        public ActionProcessor(ICommunicator communicator, IWorld world, ILogger logger, IRandom random, Combat combat)
        {
            this.communicator = communicator;
            this.world = world;
            this.logger = logger;
            this.random = random;
            this.combat = combat;
            this.actionHelper = new ActionHelper(this.communicator, random, combat);

            this.ConfigureActions();
            this.ConfigureWizActions();
        }

        /// <summary>
        /// Executes the action provided by the command.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <param name="args">The input args.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task DoAction(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            try
            {
                // Get the matching actions for the command word.
                var action = this.actions
                    .Where(a => a.Key.StartsWith(args.Action.ToLower()))
                    .OrderBy(a => a.Value.Key)
                    .FirstOrDefault();

                if (action.Value.Value != null)
                {
                    var methodAttribs = (MinimumLevelAttribute?)action.Value.Value.GetMethodInfo()?.GetCustomAttribute(typeof(MinimumLevelAttribute));

                    if (methodAttribs != null & methodAttribs?.Level > actor.Character.Level)
                    {
                        await this.communicator.SendToPlayer(actor.Connection, "Unknown command.", cancellationToken);
                    }
                    else
                    {
                        await action.Value.Value(actor, args, cancellationToken);
                    }
                }
                else
                {
                    // If the player is a wiz, try those commands.
                    if (actor.Character.Level >= Constants.WIZLEVEL)
                    {
                        // Get the matching actions for the wizard command word.
                        var wizAction = this.wizActions
                            .Where(a => a.Key.StartsWith(args.Action.ToLower()))
                            .OrderBy(a => a.Value.Key)
                            .FirstOrDefault();

                        if (wizAction.Value.Value != null)
                        {
                            var methodAttribs = (MinimumLevelAttribute?)wizAction.Value.Value.GetMethodInfo().GetCustomAttribute(typeof(MinimumLevelAttribute));

                            if (methodAttribs != null & methodAttribs?.Level > actor.Character.Level)
                            {
                                await this.communicator.SendToPlayer(actor.Connection, "Unknown command.", cancellationToken);
                            }
                            else
                            {
                                await wizAction.Value.Value(actor, args, cancellationToken);
                            }
                        }
                        else
                        {
                            await this.communicator.SendToPlayer(actor.Connection, "Unknown command.", cancellationToken);
                        }
                    }
                    else
                    {
                        await this.communicator.SendToPlayer(actor.Connection, "Unknown command.", cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, this.communicator);
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
        /// Used for opening and closing doors.
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <returns>User friendly string.</returns>
        private static string ParseFriendlyDirection(Direction direction)
        {
            return direction switch
            {
                Direction.South => "to the south of",
                Direction.North => "to the north of",
                Direction.East => "to the south of",
                Direction.West => "to the north of",
                Direction.SouthEast => "to the southeast of",
                Direction.SouthWest => "to the southwest of",
                Direction.NorthEast => "to the northeast of",
                Direction.NorthWest => "to the northwest of",
                Direction.Up => "above",
                Direction.Down => "below",
                _ => "to the front of",
            };
        }

        private static int GetTerrainMovementPenalty(Room room)
        {
            switch (room.Terrain)
            {
                default:
                    return 1;
                case Core.Types.Terrain.Beach:
                case Core.Types.Terrain.Desert:
                case Core.Types.Terrain.Jungle:
                    return 2;
                case Core.Types.Terrain.Ethereal:
                    return 0;
                case Core.Types.Terrain.Swamp:
                case Core.Types.Terrain.Mountains:
                case Core.Types.Terrain.Snow:
                    return 3;
            }
        }

        

        /// <summary>
        /// Configures all of the actions based on the input. The numeric value in the KVP is the PRIORITY in which the command will
        /// be executed. So, if someone types "n", it will check "north", "ne", "nw", and "newbie" in that order.
        /// </summary>
        private void ConfigureActions()
        {
            this.actions.Add("advance", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoAdvance)));
            this.actions.Add("affects", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoAffects)));
            this.actions.Add("awards", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(3, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoAwards)));
            this.actions.Add("buy", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoBuy)));
            this.actions.Add("close", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoClose)));
            this.actions.Add("commands", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoCommands)));
            this.actions.Add("dice", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(4, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoDice)));
            this.actions.Add("down", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoMove)));
            this.actions.Add("drink", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoDrink)));
            this.actions.Add("drop", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(3, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoDrop)));
            this.actions.Add("east", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoMove)));
            this.actions.Add("eat", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoEat)));
            this.actions.Add("emote", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(3, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoEmote)));
            this.actions.Add("emotes", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(4, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoEmotes)));
            this.actions.Add("empty", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(5, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoEmpty)));
            this.actions.Add("equipment", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoEquipment)));
            this.actions.Add("examine", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoExamine)));
            this.actions.Add("fill", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoFill)));
            this.actions.Add("flee", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoFlee)));
            this.actions.Add("follow", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoFollow)));
            this.actions.Add("gain", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(3, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoGain)));
            this.actions.Add("get", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoGet)));
            this.actions.Add("give", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoGive)));
            this.actions.Add("help", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoHelp)));
            this.actions.Add("inventory", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoInventory)));
            this.actions.Add("kill", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoCombat)));
            this.actions.Add("list", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(3, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoList)));
            this.actions.Add("lock", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoLock)));
            this.actions.Add("look", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoLook)));
            this.actions.Add("murder", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoCombat)));
            this.actions.Add("newbie", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(4, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoNewbieChat)));
            this.actions.Add("north", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoMove)));
            this.actions.Add("ne", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoMove)));
            this.actions.Add("nw", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(3, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoMove)));
            this.actions.Add("open", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(3, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoOpen)));
            this.actions.Add("put", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoPut)));
            this.actions.Add("pray", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoPray)));
            this.actions.Add("practice", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(3, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoPractice)));
            this.actions.Add("quit", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoQuit)));
            this.actions.Add("rest", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoRest)));
            this.actions.Add("remove", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoRemove)));
            this.actions.Add("reply", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoReply)));
            this.actions.Add("sacrifice", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(8, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoSacrifice)));
            this.actions.Add("save", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(8, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoSave)));
            this.actions.Add("say", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(7, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoSay)));
            this.actions.Add("scan", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(4, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoScan)));
            this.actions.Add("sell", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(5, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoSell)));
            this.actions.Add("score", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(4, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoScore)));
            this.actions.Add("skills", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(6, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoSkills)));
            this.actions.Add("sleep", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(7, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoSleep)));
            this.actions.Add("south", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoMove)));
            this.actions.Add("se", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoMove)));
            this.actions.Add("sw", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(3, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoMove)));
            this.actions.Add("spells", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(8, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoSpells)));
            this.actions.Add("subscribe", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(9, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoSubscribe)));
            this.actions.Add("tell", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(0, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoTell)));
            this.actions.Add("time", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoTime)));
            this.actions.Add("train", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoTrain)));
            this.actions.Add("unlock", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoUnlock)));
            this.actions.Add("unsubscribe", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoUnsubscribe)));
            this.actions.Add("up", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoMove)));
            this.actions.Add("west", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoMove)));
            this.actions.Add("wear", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoWear)));
            this.actions.Add("where", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(3, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoWhere)));
            this.actions.Add("who", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoWho)));
            this.actions.Add("wield", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoWield)));
            this.actions.Add("wake", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(3, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoWake)));
            this.actions.Add("yell", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(0, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoYell)));
        }

        private async Task DoAffects(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            StringBuilder sb = new StringBuilder();

            if (actor.Character.AffectedBy.Count > 0)
            {
                foreach (var effect in actor.Character.AffectedBy)
                {
                    sb.Append($"<span class='player-affect'> - {effect.Name} for {effect.Duration} hours.</span>");
                }
            }
            else
            {
                sb.Append($"<span class='player-affect'>You are not affected by anything.</span>");
            }

            await this.communicator.SendToPlayer(actor.Connection, sb.ToString(), cancellationToken);
        }

        private async Task DoClose(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Close what?", cancellationToken);
                return;
            }

            // Check if there is a container in the room
            var room = this.communicator.ResolveRoom(actor.Character.Location);
            var container = room?.Items.FirstOrDefault(i => i.ItemType == ItemType.Container);

            if (container != null)
            {
                if (!container.IsClosed)
                {
                    container.IsClosed = true;
                    await this.communicator.SendToPlayer(actor.Connection, $"You close {container.Name}.", cancellationToken);
                    await this.communicator.SendToRoom(actor.Character.Location, actor.Character, null, $"{actor.Character.FirstName} closes {container.Name}.", cancellationToken);
                }
                else
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"It's already closed.", cancellationToken);
                }
            }
            else
            {
                // See if it's an item they are carrying.
                var item = actor.Character.Inventory.ParseTargetName(args.Method);

                if (item != null)
                {
                    if (item.ItemType == ItemType.Container)
                    {
                        if (!item.IsClosed)
                        {
                            item.IsClosed = true;
                            await this.communicator.SendToPlayer(actor.Connection, $"You close {item.Name}.", cancellationToken);
                            await this.communicator.SendToRoom(actor.Character.Location, actor.Character, null, $"{actor.Character.FirstName} closes {item.Name}.", cancellationToken);
                        }
                        else
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"It's already close.", cancellationToken);
                        }
                    }
                    else
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"You can't close that.", cancellationToken);
                    }
                }
                else
                {
                    // It has to be a door then.
                    var direction = ParseDirection(args.Method);
                    var friendlyDirection = ParseFriendlyDirection(direction);

                    Exit? exit = room?.Exits?.FirstOrDefault(e => e.Direction == direction);

                    if (exit != null)
                    {
                        if (exit.IsDoor && !exit.IsClosed)
                        {
                            // Need to close the door on BOTH sides
                            var oppRoom = this.communicator.ResolveRoom(new KeyValuePair<long, long>(exit.ToArea, exit.ToRoom));

                            var exitToThisRoom = oppRoom?.Exits.FirstOrDefault(e => e.ToArea == room?.AreaId && e.ToRoom == room?.RoomId);

                            if (exitToThisRoom != null)
                            {
                                exitToThisRoom.IsClosed = true;
                            }

                            exit.IsClosed = true;
                            await this.communicator.SendToPlayer(actor.Connection, $"You close the {exit.DoorName} {friendlyDirection} you.", cancellationToken);
                            await this.communicator.SendToRoom(actor.Character.Location, actor.Character, null, $"{actor.Character.FirstName.FirstCharToUpper()} closes the {exit.DoorName} {friendlyDirection} you.", cancellationToken);

                            await this.communicator.PlaySound(actor.Character, AudioChannel.BackgroundSFX2, Sounds.CLOSEDOOR, cancellationToken);
                            await this.communicator.PlaySoundToRoom(actor.Character, null, Sounds.CLOSEDOOR, cancellationToken);
                        }
                        else if (exit.IsDoor && exit.IsClosed)
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"The {exit.DoorName} is already closed.", cancellationToken);
                        }
                        else
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"There is no door in that direction.", cancellationToken);
                        }
                    }
                }
            }
        }

        private async Task DoCombat(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Who do you want to kill?", cancellationToken);
            }
            else
            {
                await this.communicator.Attack(actor, args.Method, cancellationToken);
            }
        }

        private async Task DoCommands(UserData actor, CommandArgs args, CancellationToken cancellationToken)
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

        private async Task DoDice(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"How many dice do you want to roll?", cancellationToken);
            }
            else
            {
                if (int.TryParse(args.Method, out int numDice))
                {
                    if (numDice > 5)
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"You can only throw up to five dice.", cancellationToken);
                    }
                    else
                    {
                        StringBuilder sbToPlayer = new StringBuilder();
                        StringBuilder sbToRoom = new StringBuilder();

                        sbToPlayer.Append($"You roll {numDice} six-sided dice.<br/>");
                        sbToRoom.Append($"{actor.Character.FirstName.FirstCharToUpper()} rolls {numDice} six-sided dice.<br/>");

                        int total = 0;
                        for (var x = 0; x < numDice; x++)
                        {
                            var roll = this.random.Next(1, 6);
                            string dice = string.Empty;
                            switch (roll)
                            {
                                case 1:
                                    dice = "fa-dice-one";
                                    break;
                                case 2:
                                    dice = "fa-dice-two";
                                    break;
                                case 3:
                                    dice = "fa-dice-three";
                                    break;
                                case 4:
                                    dice = "fa-dice-four";
                                    break;
                                case 5:
                                    dice = "fa-dice-five";
                                    break;
                                case 6:
                                    dice = "fa-dice-six";
                                    break;
                            }

                            sbToPlayer.Append($"<i class='dice fa-solid {dice}'></i>");
                            sbToRoom.Append($"<i class='dice fa-solid {dice}'></i>");
                            total += roll;
                        }

                        sbToPlayer.Append($"<br/>Your total is {total}.");
                        sbToRoom.Append($"<br/>{actor.Character.FirstName.FirstCharToUpper()}'s total is {total}.");

                        await this.communicator.SendToPlayer(actor.Connection, sbToPlayer.ToString(), cancellationToken);
                        await this.communicator.SendToRoom(actor.Character.Location, actor.Character, null, sbToRoom.ToString(), cancellationToken);
                    }
                }
                else
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"How many dice do you want to roll?", cancellationToken);
                }
            }
        }

        private async Task DoDrink(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (actor.Character.Thirst.Current <= 2)
            {
                await this.communicator.SendToPlayer(actor.Connection, $"You are not thirsty.", cancellationToken);
                return;
            }

            // Check if there is a spring in the room
            var room = this.communicator.ResolveRoom(actor.Character.Location);
            var spring = room?.Items.FirstOrDefault(i => i.ItemType == ItemType.Spring);

            if (spring != null && string.IsNullOrEmpty(args.Method))
            {
                var liquid = ActionHelper.GetLiquidDescription(spring.LiquidType);
                await this.communicator.SendToPlayer(actor.Connection, $"You drink {liquid} from {spring.Name}.", cancellationToken);
                await this.communicator.SendToRoom(actor.Character.Location, actor.Character, null, $"{actor.Character.FirstName.FirstCharToUpper()} drinks {liquid} from {spring.Name}.", cancellationToken);

                actor.Character.Thirst.Current = Math.Min(8, actor.Character.Thirst.Current);

                if (actor.Character.Thirst.Current <= 2)
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"You are no longer thirsty.", cancellationToken);
                }
            }
            else
            {
                // No spring, so see if they have a drinking vessel.
                if (string.IsNullOrWhiteSpace(args.Method))
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"Drink what?", cancellationToken);
                }
                else
                {
                    // See if it's an item they are carrying.
                    var item = actor.Character.Inventory.ParseTargetName(args.Method);

                    if (item != null)
                    {
                        if (item.ItemType == ItemType.Drink)
                        {
                            if (item.Drinks?.Current > 0)
                            {
                                item.Drinks.Current--;
                                var liquid = ActionHelper.GetLiquidDescription(item.LiquidType);
                                await this.communicator.SendToPlayer(actor.Connection, $"You drink {liquid} from {item.Name}.", cancellationToken);
                                await this.communicator.SendToRoom(actor.Character.Location, actor.Character, null, $"{actor.Character.FirstName.FirstCharToUpper()} drinks {liquid} from {item.Name}.", cancellationToken);
                                actor.Character.Thirst.Current = Math.Min(8, actor.Character.Thirst.Current);

                                if (actor.Character.Thirst.Current <= 2)
                                {
                                    await this.communicator.SendToPlayer(actor.Connection, $"You are no longer thirsty.", cancellationToken);
                                }
                            }
                            else
                            {
                                await this.communicator.SendToPlayer(actor.Connection, $"It's empty.", cancellationToken);
                            }
                        }
                    }
                    else
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"You can't drink from that.", cancellationToken);
                    }
                }
            }
        }

        private async Task DoDrop(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Drop what?", cancellationToken);
            }
            else
            {
                await this.DropItem(actor, args.Method, cancellationToken);
            }
        }

        private async Task DoEat(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Eat what?", cancellationToken);
            }
            else
            {
                await this.EatItem(actor, args.Method, cancellationToken);
            }
        }

        private async Task DoEmote(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Emote what?", cancellationToken);
            }
            else
            {
                var sentence = string.Join(' ', new string?[2] { args.Method, args.Target });
                if (!string.IsNullOrWhiteSpace(sentence))
                {
                    sentence = sentence.ToLower();
                    await this.communicator.SendToPlayer(actor.Connection, $"{actor.Character.FirstName.FirstCharToUpper()} {sentence}.", cancellationToken);
                    await this.communicator.SendToRoom(actor.Character, actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName.FirstCharToUpper()} {sentence}.", cancellationToken);
                }
            }
        }

        private async Task DoEmotes(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            await this.communicator.SendToPlayer(actor.Connection, $"Available Emotes:<br/>", cancellationToken);

            StringBuilder sb = new StringBuilder();

            var commands = Emotes.Actions.OrderBy(a => a.Key);

            foreach (var kvp in commands)
            {
                sb.Append($"<span class='command'>{kvp.Key}</span>");
            }

            await this.communicator.SendToPlayer(actor.Connection, sb.ToString(), cancellationToken);
        }

        private async Task DoEmpty(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Empty what?", cancellationToken);
            }
            else
            {
                var itemName = args.Method.ToLower();

                var target = actor.Character.Inventory.FirstOrDefault(i => i.Name.Contains(itemName));

                if (target != null && target.ItemType == ItemType.Drink)
                {
                    if (target.Drinks?.Current == 0)
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"{target.Name} is already empty.", cancellationToken);
                    }
                    else
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"You empty {target.Name}, pouring {ActionHelper.GetLiquidDescription(target.LiquidType)} out onto the ground.", cancellationToken);

                        if (target.Drinks != null)
                        {
                            target.Drinks.Current = 0;
                        }
                    }
                }
                else
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"You can't empty that.", cancellationToken);
                }
            }
        }

        private async Task DoFill(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Fill what?", cancellationToken);
            }
            else
            {
                var itemName = args.Method.ToLower();

                var target = actor.Character.Inventory.FirstOrDefault(i => i.Name.Contains(itemName));

                if (target != null && target.ItemType == ItemType.Drink)
                {
                    // See if there's a fountain in the room.
                    var room = this.communicator.ResolveRoom(actor.Character.Location);
                    var spring = room?.Items.FirstOrDefault(i => i.ItemType == ItemType.Spring);

                    if (spring != null)
                    {
                        if (target.Drinks?.Current > 0)
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"{target.Name} already has something in it.", cancellationToken);
                        }
                        else
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"You fill {target.Name} with {ActionHelper.GetLiquidDescription(spring.LiquidType)} from {spring.Name}.", cancellationToken);

                            if (target.Drinks != null)
                            {
                                target.Drinks.Current = target.Drinks.Max;
                            }

                            target.LiquidType = spring.LiquidType;
                        }
                    }
                    else
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"There's nothing here to fill that with.", cancellationToken);
                    }
                }
                else
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"You can't fill that.", cancellationToken);
                }
            }
        }

        private async Task DoFlee(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            var room = this.communicator.ResolveRoom(actor.Character.Location);

            if (room == null || room.Exits.Count == 0)
            {
                await this.communicator.SendToPlayer(actor.Connection, $"You couldn't escape!", cancellationToken);
            }
            else
            {
                if (Communicator.Users != null)
                {
                    // If the player is fighting another player, remove all fighting flags from them. If it's a mob, leave them.
                    var fightingUser = Communicator.Users.Where(u => u.Value.Character.CharacterId == actor.Character.Fighting).FirstOrDefault();

                    if (fightingUser.Value != null)
                    {
                        fightingUser.Value.Character.Fighting = null;
                        fightingUser.Value.Character.CharacterFlags?.RemoveIfExists(CharacterFlags.Fighting);
                    }
                }

                actor.Character.CharacterFlags?.RemoveIfExists(CharacterFlags.Fighting);
                actor.Character.Fighting = null;

                var randomExit = room.Exits[this.random.Next(0, room.Exits.Count - 1)];

                if (randomExit != null)
                {
                    string? dir = Enum.GetName(typeof(Direction), randomExit.Direction)?.ToLower();
                    await this.communicator.SendToPlayer(actor.Connection, $"You flee from combat!", cancellationToken);
                    await this.communicator.SendToRoom(actor.Character, actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName.FirstCharToUpper()} flees {dir}.", cancellationToken);

                    await this.communicator.PlaySound(actor.Character, AudioChannel.BackgroundSFX, Sounds.WALK, cancellationToken);

                    actor.Character.Location = new KeyValuePair<long, long>(randomExit.ToArea, randomExit.ToRoom);

                    await this.communicator.SendToRoom(actor.Character, actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName.FirstCharToUpper()} runs in!", cancellationToken);
                    await this.communicator.ShowRoomToPlayer(actor.Character, cancellationToken);
                }
            }
        }

        private async Task DoFollow(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Follow whom?", cancellationToken);
            }
            else
            {
                if (args.Method.ToLower() == "self")
                {
                    if (actor.Character.Following != null)
                    {
                        // Find the person they were following and remove them as a follower.
                        var target = this.communicator.ResolveCharacter(actor.Character.Following.Value);

                        if (target != null)
                        {
                            target.Character.Followers.Remove(actor.Character.CharacterId);
                            await this.communicator.SendToPlayer(target.Connection, $"{actor.Character.FirstName} stops following you.", cancellationToken);
                        }
                    }

                    actor.Character.Following = null;
                    await this.communicator.SendToPlayer(actor.Connection, $"You now follow yourself, and yourself alone.", cancellationToken);
                }
                else
                {
                    var target = this.communicator.ResolveCharacter(args.Method);

                    if (target != null)
                    {
                        // You can follow a person, but they have to be in the same room.
                        if (target.Character.Location.Value == actor.Character.Location.Value)
                        {
                            target.Character.Followers.Add(actor.Character.CharacterId);
                            actor.Character.Following = target.Character.CharacterId;
                            await this.communicator.SendToPlayer(actor.Connection, $"You begin following {target.Character.FirstName}.", cancellationToken);
                            await this.communicator.SendToRoom(actor.Character, actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName.FirstCharToUpper()} starts following {target.Character.FirstName}.", cancellationToken);
                            await this.communicator.SendToPlayer(target.Connection, $"{actor.Character.FirstName} now follows you.", cancellationToken);
                        }
                        else
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"They aren't here.", cancellationToken);
                        }
                    }
                    else
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"They aren't here.", cancellationToken);
                    }
                }
            }
        }

        [HelpText("<p>Gets one or more items and places it into your inventory.</p><ul><li>get <em>target</em></li><li>get all <em>target</em></li><ul>")]
        private async Task DoGet(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Get what?", cancellationToken);
            }
            else
            {
                await this.GetItem(actor, args.Method, args.Target, cancellationToken);
            }
        }

        [HelpText("<p>Gives an item from your inventory to a target.<p><ul><li>give <em>item</em> <em>target</em>")]
        private async Task DoGive(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method) || string.IsNullOrEmpty(args.Target))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Give what to whom?", cancellationToken);
            }
            else
            {
                // Get the thing first.
                var itemToGive = actor.Character.Inventory.FirstOrDefault(f => f.Name.ToLower().Contains(args.Method));

                if (itemToGive == null)
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"You don't have that.", cancellationToken);
                }
                else
                {
                    // Giving to player or mob?
                    var targetPlayer = this.communicator.ResolveCharacter(args.Target);

                    if (targetPlayer != null)
                    {
                        actor.Character.Inventory.Remove(itemToGive);
                        targetPlayer.Character.Inventory.Add(itemToGive);

                        await this.communicator.SendToPlayer(actor.Connection, $"You give {itemToGive.Name} to {targetPlayer.Character.FirstName}.", cancellationToken);
                        await this.communicator.SendToPlayer(actor.Connection, $"{actor.Character.FirstName.FirstCharToUpper()} gives you {itemToGive.Name}.", cancellationToken);

                        await this.communicator.SendToRoom(actor.Character, actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName.FirstCharToUpper()} gives {itemToGive.Name} to {targetPlayer.Character.FirstName}.", cancellationToken);
                    }
                    else
                    {
                        var targetMob = this.communicator.ResolveMobile(args.Target);

                        if (targetMob != null)
                        {
                            actor.Character.Inventory.Remove(itemToGive);
                            targetMob.Inventory.Add(itemToGive);

                            await this.communicator.SendToPlayer(actor.Connection, $"You give {itemToGive.Name} to {targetMob.FirstName}.", cancellationToken);
                            await this.communicator.SendToRoom(actor.Character.Location, actor.Character, targetMob, $"{actor.Character.FirstName.FirstCharToUpper()} gives {itemToGive.Name} to {targetMob.FirstName}.", cancellationToken);
                        }
                        else
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"They aren't here.", cancellationToken);
                        }
                    }
                }
            }
        }

        private async Task DoEquipment(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            var equipment = this.actionHelper.GetEquipment(actor.Character);
            await this.communicator.SendToPlayer(actor.Connection, equipment, cancellationToken);
        }

        [HelpText("<p>Examines an item in your inventory closely to determine some of its attributes.</p><ul><li>examine <em>item</em></li><ul>")]
        private async Task DoExamine(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Examine what?", cancellationToken);
            }
            else
            {
                if (actor.Character.Inventory == null || actor.Character.Inventory.Count == 0)
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"You don't have that.", cancellationToken);
                    return;
                }

                var item = actor.Character.Inventory.FirstOrDefault(i => i.Name.Contains(args.Method));

                if (item != null)
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"You examine {item.Name}.", cancellationToken);
                    await this.communicator.SendToRoom(actor.Character.Location, actor.Character, null, $"{actor.Character.FirstName.FirstCharToUpper()} examines {item.Name}.", cancellationToken);

                    StringBuilder sb = new StringBuilder();

                    if (!string.IsNullOrWhiteSpace(item.Image))
                    {
                        sb.Append($"<div class='room-image'><img class='room-image-content' onload='image_load(this);' onerror='image_error(this);'  loading='eager' src='{item.Image}'/></div>");
                    }
                    else
                    {
                        sb.Append($"<div class='room-image room-image-none'></div>");
                    }

                    sb.Append($"{item.LongDescription}<br/>");
                    sb.Append($"{item.Name.FirstCharToUpper()} is of type {Enum.GetName<ItemType>(item.ItemType)?.ToString().ToLower()} and appears to have a durability of {item.Durability.Current}.<br/>");
                    sb.Append($"You value it at approximately {this.random.Next(Math.Max((int)item.Value - 100, 2), Math.Max((int)item.Value + 100, 4))} gold.<br/>");

                    if (item.ItemType == ItemType.Drink)
                    {
                        sb.Append($"{item.Name.FirstCharToUpper()} is a drink container. It has around {item.Drinks?.Current} draughts of {ActionHelper.GetLiquidDescription(item.LiquidType)} inside of it.");
                    }
                    else if (item.ItemType == ItemType.Food)
                    {
                        sb.Append($"{item.Name.FirstCharToUpper()} is food. It has around {item.Food?.Current} meals remaining.");
                    }
                    else if (item.ItemType == ItemType.Container)
                    {
                        sb.Append($"{item.Name.FirstCharToUpper()} is a container. ");

                        if (item.IsClosed)
                        {
                            sb.Append("It is closed, so you can't see what is inside.");
                        }
                        else
                        {
                            sb.Append("It contains:<br/><ul>");

                            if (item.Contains != null)
                            {
                                var objGroups = item.Contains.GroupBy(i => i.ItemId);

                                foreach (var group in objGroups)
                                {
                                    if (group.Count() > 1)
                                    {
                                        sb.Append($"<li>({group.Count()}) {group.First().Name}</li>");
                                    }
                                    else
                                    {
                                        sb.Append($"<li>{group.First().Name}</li>");
                                    }
                                }
                            }
                            else
                            {
                                sb.Append($"<li>Nothing.</li>");
                            }

                            sb.Append("</ul>");
                        }
                    }

                    await this.communicator.SendToPlayer(actor.Connection, sb.ToString(), cancellationToken);
                }
                else
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"You don't have that.", cancellationToken);
                }
            }
        }

        private async Task DoHelp(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            // help put
            var helpCommand = args.Method;

            if (!string.IsNullOrWhiteSpace(helpCommand))
            {
                if (helpCommand.ToLower() == "commands")
                {
                    await this.DoCommands(actor, args, cancellationToken);
                }
                else if (helpCommand.ToLower() == "newbie")
                {
                    await this.communicator.SendToPlayer(actor.Connection, "You can type COMMANDS to see a list of all commands. Type help <command> to see what the command does.", cancellationToken);
                }
                else
                {
                    var type = this.GetType();
                    var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
                    var method = methods.FirstOrDefault(m => m.Name.ToLower() == "do" + helpCommand);
                    if (method != null)
                    {
                        var helpText = method.GetCustomAttribute(typeof(HelpTextAttribute));
                        if (helpText != null && helpText is HelpTextAttribute attrib)
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"<span class='help-text'><h4>{helpCommand.ToUpper()}</h4>{attrib.HelpText}</span>", cancellationToken);
                        }
                        else
                        {
                            await this.communicator.SendToPlayer(actor.Connection, "That is a valid command, but there is not currently help text available for it.", cancellationToken);
                        }
                    }
                    else
                    {
                        await this.communicator.SendToPlayer(actor.Connection, "No help files found by that name. You can start by typing HELP NEWBIE or HELP COMMANDS.", cancellationToken);
                    }
                }
            }
            else
            {
                await this.communicator.SendToPlayer(actor.Connection, "Usage: help <command>. You can start by typing HELP NEWBIE or HELP COMMANDS.", cancellationToken);
            }
        }

        private async Task DoInventory(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<span class='inventory'>You are carrying:</span>");

            var itemGroups = actor.Character.Inventory.GroupBy(g => g.Name);

            foreach (var itemGroup in itemGroups)
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

            await this.communicator.SendToPlayer(actor.Connection, sb.ToString(), cancellationToken);
        }

        private async Task DoLock(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Lock what?", cancellationToken);
                return;
            }

            // Check if there is a container in the room
            var room = this.communicator.ResolveRoom(actor.Character.Location);
            var container = room?.Items.FirstOrDefault(i => i.ItemType == ItemType.Container);

            if (container != null && container.Name.Contains(args.Method))
            {
                // Check if it's locked.
                if (container.IsClosed && container.IsLocked)
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"It's already locked.", cancellationToken);
                }
                else if (!container.IsClosed)
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"It's still open.", cancellationToken);
                }
                else if (container.IsClosed && !container.IsLocked)
                {
                    // Do we have a key?
                    var key = actor.Character.Inventory.FirstOrDefault(k => k.ItemId == container.KeyId);
                    if (key == null)
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"You lack the key.", cancellationToken);
                    }
                    else
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"You lock {container.Name} with {key.Name}.", cancellationToken);
                        await this.communicator.SendToRoom(actor.Character.Location, actor.Character, null, $"{actor.Character.FirstName} locks {container.Name} with {key.Name}.", cancellationToken);

                        await this.communicator.PlaySound(actor.Character, AudioChannel.BackgroundSFX2, Sounds.LOCKDOOR, cancellationToken);
                        await this.communicator.PlaySoundToRoom(actor.Character, null, Sounds.LOCKDOOR, cancellationToken);

                        container.IsLocked = true;
                    }
                }
                else
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"You can't lock that.", cancellationToken);
                }
            }
            else
            {
                // See if it's an item they are carrying.
                var item = actor.Character.Inventory.ParseTargetName(args.Method);

                if (item != null)
                {
                    if (item.ItemType == ItemType.Container)
                    {
                        // Check if it's locked.
                        if (item.IsClosed && item.IsLocked)
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"It's already locked.", cancellationToken);
                        }
                        else if (!item.IsClosed)
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"It's still open.", cancellationToken);
                        }
                        else if (item.IsClosed && !item.IsLocked)
                        {
                            // Do we have a key?
                            var key = actor.Character.Inventory.FirstOrDefault(k => k.ItemId == item.KeyId);
                            if (key == null)
                            {
                                await this.communicator.SendToPlayer(actor.Connection, $"You lack the key.", cancellationToken);
                            }
                            else
                            {
                                await this.communicator.SendToPlayer(actor.Connection, $"You lock {item.Name} with {key.Name}.", cancellationToken);
                                await this.communicator.SendToRoom(actor.Character.Location, actor.Character, null, $"{actor.Character.FirstName} locks {item.Name}.", cancellationToken);

                                item.IsLocked = true;
                            }
                        }
                        else
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"You can't lock that.", cancellationToken);
                        }
                    }
                    else
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"You can't lock that.", cancellationToken);
                    }
                }
                else
                {
                    // It has to be a door then.
                    var direction = ParseDirection(args.Method);
                    var friendlyDirection = ParseFriendlyDirection(direction);

                    Exit? exit = room?.Exits?.FirstOrDefault(e => e.Direction == direction);

                    if (exit != null)
                    {
                        if (exit.IsDoor && exit.IsClosed && exit.IsLocked)
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"The {exit.DoorName} is already locked.", cancellationToken);
                        }
                        else if (exit.IsDoor && exit.IsClosed && !exit.IsLocked)
                        {
                            var key = actor.Character.Inventory.FirstOrDefault(k => k.ItemId == exit.KeyId);
                            if (key == null)
                            {
                                await this.communicator.SendToPlayer(actor.Connection, $"You lack the key.", cancellationToken);
                            }
                            else
                            {
                                // Need to lock the door on BOTH sides
                                var oppRoom = this.communicator.ResolveRoom(new KeyValuePair<long, long>(exit.ToArea, exit.ToRoom));

                                var exitToThisRoom = oppRoom?.Exits.FirstOrDefault(e => e.ToArea == room?.AreaId && e.ToRoom == room?.RoomId);

                                if (exitToThisRoom != null)
                                {
                                    exitToThisRoom.IsLocked = true;
                                }

                                exit.IsLocked = true;
                                await this.communicator.SendToPlayer(actor.Connection, $"You lock the {exit.DoorName} {friendlyDirection} you with {key.Name}.", cancellationToken);
                                await this.communicator.SendToRoom(actor.Character.Location, actor.Character, null, $"{actor.Character.FirstName.FirstCharToUpper()} locks the {exit.DoorName} {friendlyDirection} you with {key.Name}.", cancellationToken);

                                await this.communicator.PlaySound(actor.Character, AudioChannel.BackgroundSFX2, Sounds.LOCKDOOR, cancellationToken);
                                await this.communicator.PlaySoundToRoom(actor.Character, null, Sounds.LOCKDOOR, cancellationToken);
                            }
                        }
                        else if (exit.IsDoor && !exit.IsClosed)
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"The {exit.DoorName} is open.", cancellationToken);
                        }
                        else
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"There is no door in that direction.", cancellationToken);
                        }
                    }
                }
            }
        }

        [HelpText("<p>Looks at another player, NPC, or item. Look at the sky if you're outside to help determine where you are. See also HELP EXAMINE.</p><ul><li>look <em>player name</em></li><li>look <em>NPC name</em></li><li>look <em>item</em></li><li>look <em>sky</em></li><ul>")]
        private async Task DoLook(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(args.Method))
            {
                if (args.Method.ToLower() == "sky")
                {
                    var room = this.communicator.ResolveRoom(actor.Character.Location);

                    if (room != null && room.Flags.Contains(RoomFlags.Indoors))
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"You're unable to see the sky from indoors.", cancellationToken);
                    }
                    else
                    {
                        var area = this.communicator.ResolveArea(actor.Character.Location);

                        if (area != null)
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"Looking at the sky, you seem to be in the vicinty of {area.Name}.", cancellationToken);
                        }
                    }
                }
                else
                {
                    // Are we looking at a player, a mobile, or at an item?
                    var items = this.communicator.GetItemsInRoom(actor.Character.Location);

                    if (items != null)
                    {
                        var item = items.ParseTargetName(args.Method);

                        if (item != null)
                        {
                            await this.communicator.ShowItemToPlayer(actor.Character, item, cancellationToken);
                        }
                        else
                        {
                            await this.communicator.ShowPlayerToPlayer(actor.Character, args.Method, cancellationToken);
                        }
                    }
                    else
                    {
                        await this.communicator.ShowPlayerToPlayer(actor.Character, args.Method, cancellationToken);
                    }
                }
            }
            else
            {
                await this.communicator.ShowRoomToPlayer(actor.Character, cancellationToken);
            }
        }

        [HelpText("<p>You can move in any cardinal direction simply by typing the direction and pressing enter.</p><ul><li>north, south, east, west, up, down, ne, se, sw, nw.</li><ul>")]
        private async Task DoMove(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (actor.Character.CharacterFlags.Contains(CharacterFlags.Resting))
            {
                await this.communicator.SendToPlayer(actor.Connection, "You're far too relaxed.", cancellationToken);
                return;
            }

            if (actor.Character.Movement.Current == 0)
            {
                await this.communicator.SendToPlayer(actor.Connection, $"You are too exhausted.", cancellationToken);
                return;
            }

            var room = this.communicator.ResolveRoom(actor.Character.Location);

            var direction = ParseDirection(args.Action);

            Exit? exit = room?.Exits?.FirstOrDefault(e => e.Direction == direction);

            if (exit != null)
            {
                if (exit.IsDoor && exit.IsClosed)
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"The {exit.DoorName} is closed.", cancellationToken);
                }
                else
                {
                    var newArea = this.world.Areas.FirstOrDefault(a => a.AreaId == exit.ToArea);
                    var newRoom = newArea?.Rooms?.FirstOrDefault(r => r.RoomId == exit.ToRoom);

                    if (newArea != null && newRoom != null)
                    {
                        string? dir = Enum.GetName(typeof(Direction), exit.Direction)?.ToLower();

                        if (actor.Character.Following == null)
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"You go {dir}.", cancellationToken);
                        }

                        await this.communicator.SendToRoom(actor.Character, actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName.FirstCharToUpper()} leaves {dir}.", cancellationToken);

                        await this.communicator.PlaySound(actor.Character, AudioChannel.BackgroundSFX, Sounds.WALK, cancellationToken);

                        actor.Character.Location = new KeyValuePair<long, long>(exit.ToArea, exit.ToRoom);

                        actor.Character.Movement.Current -= GetTerrainMovementPenalty(newRoom);

                        await this.communicator.SendToRoom(actor.Character, actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName.FirstCharToUpper()} enters.", cancellationToken);
                        await this.communicator.ShowRoomToPlayer(actor.Character, cancellationToken);

                        // Check if there are followers.
                        if (actor.Character.Followers.Count > 0)
                        {
                            foreach (var follower in actor.Character.Followers)
                            {
                                // Resolve the follower.
                                var target = this.communicator.ResolveCharacter(follower);

                                // Make sure they are still following the actor.
                                if (target != null && target.Character.Following == actor.Character.CharacterId)
                                {
                                    await this.communicator.SendToPlayer(target.Connection, $"You follow {actor.Character.FirstName} {dir}.", cancellationToken);
                                    await this.DoMove(target, args, cancellationToken);
                                }
                            }
                        }
                    }
                    else
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"You are unable to go that way.", cancellationToken);
                    }
                }
            }
            else
            {
                await this.communicator.SendToPlayer(actor.Connection, $"You can't go that way.", cancellationToken);
            }
        }

        private async Task DoNewbieChat(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(args.Method))
            {
                var sentence = args.Method;
                sentence = char.ToUpper(sentence[0]) + sentence[1..];
                var channel = this.communicator.Channels.FirstOrDefault(c => c.Name.ToLower() == "newbie");
                if (channel != null)
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"You newbie chat \"<span class='newbie'>{sentence}</span>\"", cancellationToken);
                    await this.communicator.SendToChannel(channel, actor.ConnectionId, $"{actor.Character.FirstName.FirstCharToUpper()} newbie chats \"<span class='newbie'>{sentence}</span>\"", cancellationToken);
                }
            }
            else
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Newbie chat what?", cancellationToken);
            }
        }

        private async Task DoOpen(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Open what?", cancellationToken);
                return;
            }

            // Check if there is a container in the room
            var room = this.communicator.ResolveRoom(actor.Character.Location);
            var container = room?.Items.FirstOrDefault(i => i.ItemType == ItemType.Container);

            if (container != null)
            {
                // Check if it's locked.
                if (container.IsClosed && container.IsLocked)
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"It's locked.", cancellationToken);
                }
                else if (container.IsClosed)
                {
                    container.IsClosed = false;
                    await this.communicator.SendToPlayer(actor.Connection, $"You open {container.Name}.", cancellationToken);
                    await this.communicator.SendToRoom(actor.Character.Location, actor.Character, null, $"{actor.Character.FirstName} opens {container.Name}.", cancellationToken);
                }
                else
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"It's already open.", cancellationToken);
                }
            }
            else
            {
                // See if it's an item they are carrying.
                var item = actor.Character.Inventory.ParseTargetName(args.Method);

                if (item != null)
                {
                    if (item.ItemType == ItemType.Container)
                    {
                        // Check if it's locked.
                        if (item.IsClosed && item.IsLocked)
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"It's locked.", cancellationToken);
                        }
                        else if (item.IsClosed)
                        {
                            item.IsClosed = false;
                            await this.communicator.SendToPlayer(actor.Connection, $"You open {item.Name}.", cancellationToken);
                            await this.communicator.SendToRoom(actor.Character.Location, actor.Character, null, $"{actor.Character.FirstName} opens {item.Name}.", cancellationToken);
                        }
                        else
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"It's already open.", cancellationToken);
                        }
                    }
                    else
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"You can't open that.", cancellationToken);
                    }
                }
                else
                {
                    // It has to be a door then.
                    var direction = ParseDirection(args.Method);
                    var friendlyDirection = ParseFriendlyDirection(direction);

                    Exit? exit = room?.Exits?.FirstOrDefault(e => e.Direction == direction);

                    if (exit != null)
                    {
                        if (exit.IsDoor && exit.IsClosed && exit.IsLocked)
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"The {exit.DoorName} is locked.", cancellationToken);
                        }
                        else if (exit.IsDoor && exit.IsClosed)
                        {
                            // Need to open the door on BOTH sides
                            var oppRoom = this.communicator.ResolveRoom(new KeyValuePair<long, long>(exit.ToArea, exit.ToRoom));

                            var exitToThisRoom = oppRoom?.Exits.FirstOrDefault(e => e.ToArea == room?.AreaId && e.ToRoom == room?.RoomId);

                            if (exitToThisRoom != null)
                            {
                                exitToThisRoom.IsClosed = false;
                            }

                            exit.IsClosed = false;
                            await this.communicator.SendToPlayer(actor.Connection, $"You open the {exit.DoorName} {friendlyDirection} you.", cancellationToken);
                            await this.communicator.SendToRoom(actor.Character.Location, actor.Character, null, $"{actor.Character.FirstName.FirstCharToUpper()} opens the {exit.DoorName} {friendlyDirection} you.", cancellationToken);

                            await this.communicator.PlaySound(actor.Character, AudioChannel.BackgroundSFX2, Sounds.OPENDOOR, cancellationToken);
                            await this.communicator.PlaySoundToRoom(actor.Character, null, Sounds.OPENDOOR, cancellationToken);
                        }
                        else if (exit.IsDoor)
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"The {exit.DoorName} is already open.", cancellationToken);
                        }
                        else
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"There is no door in that direction.", cancellationToken);
                        }
                    }
                }
            }
        }

        private async Task DoPray(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            var sentence = args.Method;
            if (!string.IsNullOrWhiteSpace(sentence))
            {
                sentence = char.ToUpper(sentence[0]) + sentence[1..];
                var channel = this.communicator.Channels.FirstOrDefault(c => c.Name.ToLower() == "pray");
                if (channel != null)
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"You pray \"<span class='pray'>{sentence}</span>\"", cancellationToken);
                    await this.communicator.SendToChannel(channel, actor.ConnectionId, $"{actor.Character.FirstName.FirstCharToUpper()} prays \"<span class='newbie'>{sentence}</span>\"", cancellationToken);
                }
            }
            else
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Pray what?", cancellationToken);
            }
        }

        private async Task DoQuit(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            await this.communicator.SendToPlayer(actor.Connection, $"You have disconnected.", cancellationToken);
            await this.communicator.SendToRoom(null, actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} has left the realms.", cancellationToken);
            await this.communicator.Quit(actor.Connection, actor.Character.FirstName ?? "Someone", cancellationToken);
        }

        private async Task DoRemove(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            // remove <argument>
            // See if they are wearing the item.
            if (!string.IsNullOrWhiteSpace(args.Method))
            {
                var itemName = args.Method.ToLower();

                if (itemName == "all")
                {
                    foreach (var target in actor.Character.Equipment)
                    {
                        // Un-equip each item and put back in inventory.
                        actor.Character.Equipment.Remove(target);
                        actor.Character.Inventory.Add(target);
                        await this.communicator.SendToPlayer(actor.Connection, $"You remove {target.Name}.", cancellationToken);
                        await this.communicator.SendToRoom(actor.Character, actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName.FirstCharToUpper()} removes {target.Name}.", cancellationToken);
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
                        await this.communicator.SendToRoom(actor.Character, actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName.FirstCharToUpper()} removes {target.Name}.", cancellationToken);
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

        private async Task DoRest(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            await this.communicator.SendToPlayer(actor.Connection, $"You kick back and rest.", cancellationToken);
            await this.communicator.SendToRoom(actor.Character, actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName.FirstCharToUpper()} kicks back and rests.", cancellationToken);
            actor.Character.CharacterFlags.AddIfNotExists(CharacterFlags.Resting);
        }

        private async Task DoSacrifice(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Sacrifice what?", cancellationToken);
            }
            else
            {
                await this.SacrificeItem(actor, args.Method, cancellationToken);
            }
        }

        private async Task DoSave(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            await this.communicator.SaveCharacter(actor);
            await this.communicator.SendToPlayer(actor.Connection, $"Character saved.", cancellationToken);
        }

        private async Task DoSay(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            var sentence = args.Method;
            await this.communicator.SendToPlayer(actor.Connection, $"You say \"<span class='say'>{sentence}</b>\"", cancellationToken);
            await this.communicator.SendToRoom(actor.Character, actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName.FirstCharToUpper()} says \"<span class='say'>{sentence}</span>\"", cancellationToken);
        }

        private async Task DoScore(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            await this.ShowPlayerScore(actor, cancellationToken);
        }

        private async Task DoScan(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            var room = this.communicator.ResolveRoom(actor.Character.Location);

            if (room == null)
            {
                return;
            }

            await this.communicator.SendToPlayer(actor.Connection, $"You scan in all directions.", cancellationToken);
            await this.communicator.SendToRoom(actor.Character, actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName.FirstCharToUpper()} scans all around.", cancellationToken);

            StringBuilder sb = new StringBuilder();

            foreach (var exit in room.Exits)
            {
                sb.Append($"Looking {exit.Direction.ToString().ToLower()} you see:<br/>");

                var location = new KeyValuePair<long, long>(exit.ToArea, exit.ToRoom);
                var mobs = this.communicator.GetMobilesInRoom(location);
                var players = this.communicator.GetPlayersInRoom(location);

                if (players != null)
                {
                    foreach (var player in players)
                    {
                        sb.Append($"<span class='scan'>{player.FirstName.FirstCharToUpper()}</span>");
                    }
                }

                if (mobs != null)
                {
                    foreach (var mob in mobs)
                    {
                        sb.Append($"<span class='scan'>{mob.FirstName.FirstCharToUpper()}</span>");
                    }
                }
            }

            await this.communicator.SendToPlayer(actor.Connection, sb.ToString(), cancellationToken);
        }

        private async Task DoSkills(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            var builder = new StringBuilder();
            builder.Append("<div class='skillgroups'>");
            var engine = Assembly.Load("Legendary.Engine");

            var skillTrees = engine.GetTypes().Where(t => t.Namespace == "Legendary.Engine.Models.SkillTrees");

            foreach (var tree in skillTrees)
            {
                var treeInstance = Activator.CreateInstance(tree, this.communicator, this.random, this.combat);

                if (treeInstance != null && treeInstance is IActionTree instance)
                {
                    var groupProps = tree.GetProperties();

                    builder.Append($"<div><span class='skillgroup'>{instance.Name}</span>");

                    bool hasSkillInGroup = false;

                    for (var x = 1; x <= 5; x++)
                    {
                        var spellGroup = groupProps.FirstOrDefault(g => g.Name == $"Group{x}");

                        if (spellGroup != null)
                        {
                            var obj = spellGroup.GetValue(treeInstance);

                            if (obj != null)
                            {
                                var group = (List<IAction>)obj;

                                foreach (var action in group)
                                {
                                    if (actor.Character.HasSkill(action.Name.ToLower()))
                                    {
                                        var proficiency = actor.Character.GetSkillProficiency(action.Name.ToLower());
                                        if (proficiency != null)
                                        {
                                            builder.Append($"<span class='skillinfo'>{proficiency.SkillName} {proficiency.Proficiency}% <progress class='skillprogress' max='100' value='{proficiency.Progress}'>{proficiency.Progress}%</progress></span>");
                                            hasSkillInGroup = true;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (!hasSkillInGroup)
                    {
                        builder.Append($"<span class='skillinfo'>No skills in this group.</span>");
                    }
                }

                builder.Append("</div>");
            }

            builder.Append("</div>");

            await this.communicator.SendToPlayer(actor.Connection, builder.ToString(), cancellationToken);
        }

        private async Task DoSleep(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            await this.communicator.SendToPlayer(actor.Connection, $"You go to sleep.", cancellationToken);
            await this.communicator.SendToRoom(actor.Character, actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName.FirstCharToUpper()} goes to sleep.", cancellationToken);
            actor.Character.CharacterFlags.AddIfNotExists(CharacterFlags.Sleeping);
        }

        private async Task DoSpells(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            var builder = new StringBuilder();
            builder.Append("<div class='spellgroups'>");
            var engine = Assembly.Load("Legendary.Engine");

            var spellTrees = engine.GetTypes().Where(t => t.Namespace == "Legendary.Engine.Models.SpellTrees");

            foreach (var tree in spellTrees)
            {
                var treeInstance = Activator.CreateInstance(tree, this.communicator, this.random, this.combat);

                if (treeInstance != null && treeInstance is IActionTree instance)
                {
                    var groupProps = tree.GetProperties();

                    builder.Append($"<div><span class='spellgroup'>{instance.Name}</span>");

                    bool hasSkillInGroup = false;

                    for (var x = 1; x <= 5; x++)
                    {
                        var spellGroup = groupProps.FirstOrDefault(g => g.Name == $"Group{x}");

                        if (spellGroup != null)
                        {
                            var obj = spellGroup.GetValue(treeInstance);

                            if (obj != null)
                            {
                                var group = (List<IAction>)obj;

                                foreach (var action in group)
                                {
                                    if (actor.Character.HasSpell(action.Name.ToLower()))
                                    {
                                        var proficiency = actor.Character.GetSpellProficiency(action.Name.ToLower());
                                        if (proficiency != null)
                                        {
                                            builder.Append($"<span class='spellinfo'>{proficiency.SpellName} {proficiency.Proficiency}% <progress class='spellprogress' max='100' value='{proficiency.Progress}'>{proficiency.Progress}%</progress></span>");
                                            hasSkillInGroup = true;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (!hasSkillInGroup)
                    {
                        builder.Append($"<span class='spellinfo'>No spells in this group.</span>");
                    }
                }

                builder.Append("</div>");
            }

            builder.Append("</div>");

            await this.communicator.SendToPlayer(actor.Connection, builder.ToString(), cancellationToken);
        }

        private async Task DoSubscribe(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            var name = args.Method;

            if (!string.IsNullOrWhiteSpace(name))
            {
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
            else
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Subscribe to what?", cancellationToken);
            }
        }

        private async Task DoTell(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method) || string.IsNullOrWhiteSpace(args.Target))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Tell whom what?", cancellationToken);
            }
            else
            {
                await this.Tell(actor, args.Target, args.Method, cancellationToken);
            }
        }

        private async Task DoReply(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Reply what?", cancellationToken);
            }
            else
            {
                if (Communicator.Tells.ContainsKey(actor.Character.FirstName))
                {
                    var target = Communicator.Tells[actor.Character.FirstName];
                    await this.Tell(actor, target, args.Method, cancellationToken);
                }
                else
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"You have no one to reply to.", cancellationToken);
                }
            }
        }

        private async Task DoTime(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            var metrics = this.world.GameMetrics;

            if (metrics != null)
            {
                var timeInfo = DateTimeHelper.GetDate(metrics.CurrentDay, metrics.CurrentMonth, metrics.CurrentYear, metrics.CurrentHour, DateTime.Now.Minute, DateTime.Now.Second);

                await this.communicator.SendToPlayer(actor.Connection, $"{timeInfo}", cancellationToken);
            }
        }

        private async Task DoUnlock(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Unlock what?", cancellationToken);
                return;
            }

            // Check if there is a container in the room
            var room = this.communicator.ResolveRoom(actor.Character.Location);
            var container = room?.Items.FirstOrDefault(i => i.ItemType == ItemType.Container);

            if (container != null && container.Name.Contains(args.Method))
            {
                // Check if it's locked.
                if (container.IsClosed && !container.IsLocked)
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"It's already unlocked.", cancellationToken);
                }
                else if (!container.IsClosed)
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"It's already open.", cancellationToken);
                }
                else if (container.IsClosed && container.IsLocked)
                {
                    // Do we have a key?
                    var key = actor.Character.Inventory.FirstOrDefault(k => k.ItemId == container.KeyId);
                    if (key == null)
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"You lack the key.", cancellationToken);
                    }
                    else
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"You unlock {container.Name} with {key.Name}.", cancellationToken);
                        await this.communicator.SendToRoom(actor.Character.Location, actor.Character, null, $"{actor.Character.FirstName} unlocks {container.Name} with {key.Name}.", cancellationToken);

                        await this.communicator.PlaySound(actor.Character, AudioChannel.BackgroundSFX2, Sounds.UNLOCKDOOR, cancellationToken);
                        await this.communicator.PlaySoundToRoom(actor.Character, null, Sounds.UNLOCKDOOR, cancellationToken);

                        container.IsLocked = false;
                    }
                }
                else
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"You can't unlock that.", cancellationToken);
                }
            }
            else
            {
                // See if it's an item they are carrying.
                var item = actor.Character.Inventory.ParseTargetName(args.Method);

                if (item != null)
                {
                    if (item.ItemType == ItemType.Container)
                    {
                        // Check if it's locked.
                        if (item.IsClosed && !item.IsLocked)
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"It's already unlocked.", cancellationToken);
                        }
                        else if (!item.IsClosed)
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"It's already open.", cancellationToken);
                        }
                        else if (item.IsClosed && item.IsLocked)
                        {
                            // Do we have a key?
                            var key = actor.Character.Inventory.FirstOrDefault(k => k.ItemId == item.KeyId);
                            if (key == null)
                            {
                                await this.communicator.SendToPlayer(actor.Connection, $"You lack the key.", cancellationToken);
                            }
                            else
                            {
                                await this.communicator.SendToPlayer(actor.Connection, $"You unlock {item.Name} with {key.Name}.", cancellationToken);
                                await this.communicator.SendToRoom(actor.Character.Location, actor.Character, null, $"{actor.Character.FirstName} unlocks {item.Name}.", cancellationToken);

                                item.IsLocked = false;
                            }
                        }
                        else
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"You can't unlock that.", cancellationToken);
                        }
                    }
                    else
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"You can't unlock that.", cancellationToken);
                    }
                }
                else
                {
                    // It has to be a door then.
                    var direction = ParseDirection(args.Method);
                    var friendlyDirection = ParseFriendlyDirection(direction);

                    Exit? exit = room?.Exits?.FirstOrDefault(e => e.Direction == direction);

                    if (exit != null)
                    {
                        if (exit.IsDoor && exit.IsClosed && !exit.IsLocked)
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"The {exit.DoorName} is already unlocked.", cancellationToken);
                        }
                        else if (exit.IsDoor && exit.IsClosed && !exit.IsLocked)
                        {
                            var key = actor.Character.Inventory.FirstOrDefault(k => k.ItemId == exit.KeyId);
                            if (key == null)
                            {
                                await this.communicator.SendToPlayer(actor.Connection, $"You lack the key.", cancellationToken);
                            }
                            else
                            {
                                // Need to lock the door on BOTH sides
                                var oppRoom = this.communicator.ResolveRoom(new KeyValuePair<long, long>(exit.ToArea, exit.ToRoom));

                                var exitToThisRoom = oppRoom?.Exits.FirstOrDefault(e => e.ToArea == room?.AreaId && e.ToRoom == room?.RoomId);

                                if (exitToThisRoom != null)
                                {
                                    exitToThisRoom.IsLocked = true;
                                }

                                exit.IsLocked = true;
                                await this.communicator.SendToPlayer(actor.Connection, $"You unlock the {exit.DoorName} {friendlyDirection} you with {key.Name}.", cancellationToken);
                                await this.communicator.SendToRoom(actor.Character.Location, actor.Character, null, $"{actor.Character.FirstName.FirstCharToUpper()} unlocks the {exit.DoorName} {friendlyDirection} you with {key.Name}.", cancellationToken);

                                await this.communicator.PlaySound(actor.Character, AudioChannel.BackgroundSFX2, Sounds.UNLOCKDOOR, cancellationToken);
                                await this.communicator.PlaySoundToRoom(actor.Character, null, Sounds.UNLOCKDOOR, cancellationToken);
                            }
                        }
                        else if (exit.IsDoor && !exit.IsClosed)
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"The {exit.DoorName} is open.", cancellationToken);
                        }
                        else
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"There is no door in that direction.", cancellationToken);
                        }
                    }
                }
            }
        }

        private async Task DoUnsubscribe(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            var name = args.Method;

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

        private async Task DoWho(UserData actor, CommandArgs args, CancellationToken cancellationToken)
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

        private async Task DoWhere(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            var actorRoom = this.communicator.ResolveRoom(actor.Character.Location);

            if (string.IsNullOrWhiteSpace(args.Method))
            {
                if (Communicator.Users != null)
                {
                    var playersInArea = Communicator.Users.Where(u => u.Value.Character.Location.Key == actor.Character.Location.Key).Select(u => u.Value.Character).ToList();

                    if (playersInArea != null && playersInArea.Count > 0)
                    {
                        await this.ShowCharactersInArea(actor, playersInArea, null, cancellationToken);
                    }
                    else
                    {
                        await this.communicator.SendToPlayer(actor.Connection, "There are no players near you.", cancellationToken);
                    }
                }
            }
            else
            {
                // Looking for a specific name (player or mobile)
                bool found = false;

                var mobilesInArea = this.communicator.GetMobilesInArea(actor.Character.Location.Key)?.Where(m => m.FirstName.ToLower().Contains(args.Method.ToLower())).ToList();

                if (mobilesInArea != null && mobilesInArea.Count > 0)
                {
                    found = true;
                }

                List<Character> playersInArea = new List<Character>();

                if (Communicator.Users != null)
                {
                    playersInArea = Communicator.Users
                        .Where(u => u.Value.Character.Location.Key == actor.Character.Location.Key && u.Value.Character.FirstName.ToLower()
                        .StartsWith(args.Method.ToLower()))
                        .Select(u => u.Value.Character).ToList();

                    if (playersInArea != null && playersInArea.Count > 0)
                    {
                        found = true;
                    }
                }

                if (found)
                {
                    await this.ShowCharactersInArea(actor, playersInArea, mobilesInArea, cancellationToken);
                }
                else
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"There is no '{args.Method}' near you.", cancellationToken);
                }
            }
        }

        private async Task DoWake(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (actor.Character.CharacterFlags.Contains(CharacterFlags.Resting) || actor.Character.CharacterFlags.Contains(CharacterFlags.Sleeping))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"You wake and and stand up.", cancellationToken);
                await this.communicator.SendToRoom(actor.Character, actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName.FirstCharToUpper()} wakes and stands up.", cancellationToken);
                actor.Character.CharacterFlags.RemoveIfExists(CharacterFlags.Resting);
                actor.Character.CharacterFlags.RemoveIfExists(CharacterFlags.Sleeping);
            }
            else
            {
                await this.communicator.SendToPlayer(actor.Connection, $"You are already awake and standing.", cancellationToken);
            }
        }

        private async Task DoWear(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            // wear <argument>
            // See if they have it in their inventory.
            if (!string.IsNullOrWhiteSpace(args.Method))
            {
                var itemName = args.Method.ToLower();

                if (itemName == "all")
                {
                    // Get everything in the player's inventory that can be worn without replacing stuff that is already worn.
                    var wornLocations = actor.Character.Equipment.Select(w => w.WearLocation).ToList();
                    var inventoryCanWear = actor.Character.Inventory.Where(i => !wornLocations.Contains(i.WearLocation)).ToList();

                    foreach (var item in inventoryCanWear)
                    {
                        if (item.WearLocation.Contains(WearLocation.None) || item.WearLocation.Contains(WearLocation.Inventory))
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"You can't wear {item.Name}.", cancellationToken);
                        }
                        else if (item.ItemType != ItemType.Weapon)
                        {
                            await this.EquipItem(actor, "wear", item, cancellationToken);
                        }
                    }

                    await this.communicator.SaveCharacter(actor);
                }
                else
                {
                    var target = actor.Character.Inventory.FirstOrDefault(i => i.Name.Contains(itemName));

                    if (target != null)
                    {
                        if (target.WearLocation.Contains(WearLocation.None) || target.WearLocation.Contains(WearLocation.Inventory))
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"You can't wear {target.Name}.", cancellationToken);
                            return;
                        }

                        var equipmentToReplace = new List<Item>();

                        foreach (var wearLocation in target.WearLocation)
                        {
                            var targetLocationItem = actor.Character.Equipment.FirstOrDefault(a => a.WearLocation.Contains(wearLocation));

                            if (targetLocationItem == null)
                            {
                                // Equip the item.
                                actor.Character.Inventory.Remove(target);
                                await this.EquipItem(actor, "wear", target, cancellationToken);
                            }
                            else
                            {
                                // Swap out the equipment.
                                equipmentToReplace.Add(targetLocationItem);
                                await this.communicator.SendToPlayer(actor.Connection, $"You remove {targetLocationItem.Name}.", cancellationToken);
                                await this.communicator.SendToRoom(actor.Character, actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName.FirstCharToUpper()} removes {targetLocationItem.Name}.", cancellationToken);

                                await this.EquipItem(actor, "wear", target, cancellationToken);
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

        private async Task DoWield(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            // wield <argument>
            // See if they have it in their inventory.
            if (!string.IsNullOrWhiteSpace(args.Method))
            {
                var itemName = args.Method.ToLower();

                var target = actor.Character.Inventory.FirstOrDefault(i => i.Name.Contains(itemName));

                if (target != null)
                {
                    if (target.WearLocation.Contains(WearLocation.Wielded))
                    {
                        var targetLocationItem = actor.Character.Equipment.FirstOrDefault(a => a.WearLocation.Contains(WearLocation.Wielded));

                        if (targetLocationItem == null)
                        {
                            // Equip the item.
                            actor.Character.Inventory.Remove(target);
                            await this.EquipItem(actor, "wield", target, cancellationToken);
                        }
                        else
                        {
                            // Swap out the equipment.
                            await this.communicator.SendToPlayer(actor.Connection, $"You stops wielding {targetLocationItem.Name}.", cancellationToken);
                            await this.communicator.SendToRoom(actor.Character, actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName.FirstCharToUpper()} stops wielding {targetLocationItem.Name}.", cancellationToken);
                            actor.Character.Equipment.Remove(targetLocationItem);
                            actor.Character.Inventory.Add(targetLocationItem);

                            await this.EquipItem(actor, "wield", target, cancellationToken);
                        }
                    }
                }
                else
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"You don't have that.", cancellationToken);
                }
            }
            else
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Wield what?", cancellationToken);
            }
        }

        private async Task DoYell(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            var sentence = args.Method;

            if (!string.IsNullOrWhiteSpace(sentence))
            {
                sentence = char.ToUpper(sentence[0]) + sentence[1..];
                await this.communicator.SendToPlayer(actor.Connection, $"You yell \"<span class='yell'>{sentence}</b>\"", cancellationToken);
                await this.communicator.SendToArea(actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName.FirstCharToUpper()} yells \"<span class='yell'>{sentence}!</span>\"", cancellationToken);
            }
            else
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Yell what?", cancellationToken);
            }
        }

        private async Task EquipItem(UserData actor, string verb, Item item, CancellationToken cancellationToken)
        {
            // Equip the item.
            actor.Character.Equipment.Add(item);
            await this.communicator.SendToPlayer(actor.Connection, $"You {verb} {item.Name}.", cancellationToken);
            await this.communicator.SendToRoom(actor.Character, actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName.FirstCharToUpper()} {verb}s {item.Name}.", cancellationToken);
        }

        private async Task GotoRoom(UserData user, long room, CancellationToken cancellationToken = default)
        {
            foreach (var area in this.world.Areas)
            {
                var targetRoom = area.Rooms.FirstOrDefault(r => r.RoomId == room);
                if (targetRoom == null)
                {
                    continue;
                }
                else
                {
                    await this.communicator.SendToPlayer(user.Connection, $"You suddenly teleport to {targetRoom.Name}.", cancellationToken);
                    await this.communicator.SendToRoom(null, user.Character.Location, user.ConnectionId, $"{user.Character.FirstName.FirstCharToUpper()} vanishes.", cancellationToken);
                    user.Character.Location = new KeyValuePair<long, long>(targetRoom.AreaId, targetRoom.RoomId);
                    await this.communicator.ShowRoomToPlayer(user.Character, cancellationToken);
                    return;
                }
            }

            await this.communicator.SendToPlayer(user.Connection, $"You were unable to teleport there.", cancellationToken);
        }

        private async Task SacrificeItem(UserData user, string target, CancellationToken cancellationToken = default)
        {
            var room = this.communicator.ResolveRoom(user.Character.Location);

            if (room == null)
            {
                return;
            }

            if (target.ToLower() == "all")
            {
                List<Item> itemsToRemove = new ();

                if (room.Items == null || room.Items.Count == 0)
                {
                    await this.communicator.SendToPlayer(user.Connection, $"There isn't anything here to sacrifice.", cancellationToken);
                    return;
                }

                foreach (var item in room.Items)
                {
                    if (item != null)
                    {
                        if (item.WearLocation.Contains(WearLocation.None) && !item.IsNPCCorpse)
                        {
                            await this.communicator.SendToPlayer(user.Connection, $"You can't sacrifice {item.Name}.", cancellationToken);
                        }
                        else
                        {
                            user.Character.DivineFavor += 1;
                            await this.communicator.SendToPlayer(user.Connection, $"You sacrifice {item.Name} to your deity for some divine favor.", cancellationToken);
                            await this.communicator.SendToRoom(user.Character, user.Character.Location, user.ConnectionId, $"{user.Character.FirstName.FirstCharToUpper()} sacrifices {item.Name} to their deity.", cancellationToken);
                            itemsToRemove.Add(item);
                        }
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
                    await this.communicator.SendToPlayer(user.Connection, $"There isn't anything here to sacrifice.", cancellationToken);
                    return;
                }

                List<Item> itemsToRemove = new ();

                var item = room.Items.ParseTargetName(target);

                if (item != null)
                {
                    if (item.WearLocation.Contains(WearLocation.None) && !item.IsNPCCorpse)
                    {
                        await this.communicator.SendToPlayer(user.Connection, $"You can't sacrifice {item.Name}.", cancellationToken);
                        return;
                    }
                    else
                    {
                        user.Character.DivineFavor += 1;
                        await this.communicator.SendToPlayer(user.Connection, $"You sacrifice {item.Name} to your deity for some divine favor.", cancellationToken);
                        await this.communicator.SendToRoom(user.Character, user.Character.Location, user.ConnectionId, $"{user.Character.FirstName.FirstCharToUpper()} sacrifices {item.Name} to their deity.", cancellationToken);
                        itemsToRemove.Add(item);
                    }
                }
                else
                {
                    await this.communicator.SendToPlayer(user.Connection, $"That isn't here.", cancellationToken);
                }

                foreach (var itemToRemove in itemsToRemove)
                {
                    room.Items.Remove(itemToRemove);
                }
            }
        }

        private async Task GetItem(UserData user, string method, string target, CancellationToken cancellationToken = default)
        {
            var room = this.communicator.ResolveRoom(user.Character.Location);

            if (room == null)
            {
                return;
            }

            // Get all corpse, get all chest, etc.
            if (!string.IsNullOrWhiteSpace(target))
            {
                // See if we have an item like that in the room.
                var items = this.communicator.GetItemsInRoom(user.Character.Location);

                if (items != null)
                {
                    var item = items.ParseTargetName(target);

                    if (item != null)
                    {
                        if (item.Contains != null && item.Contains.Count > 0)
                        {
                            List<IItem> itemsToRemove = new ();

                            foreach (var eq in item.Contains)
                            {
                                if (eq != null)
                                {
                                    var clone = (eq as Item).Clone();
                                    if (clone != null)
                                    {
                                        user.Character.Inventory.Add(clone);
                                        await this.communicator.SendToPlayer(user.Connection, $"You get {clone.Name} from {item.Name}.", cancellationToken);
                                        await this.communicator.SendToRoom(user.Character, user.Character.Location, user.ConnectionId, $"{user.Character.FirstName.FirstCharToUpper()} gets {eq.Name} from {item.Name}.", cancellationToken);
                                        itemsToRemove.Add(eq);
                                    }
                                }
                            }

                            foreach (var itemToRemove in itemsToRemove)
                            {
                                item.Contains.Remove(itemToRemove);
                            }
                        }
                        else
                        {
                            await this.communicator.SendToPlayer(user.Connection, $"There isn't anything in {item.Name} to get.", cancellationToken);
                        }
                    }
                    else
                    {
                        await this.communicator.SendToPlayer(user.Connection, $"That isn't here.", cancellationToken);
                    }
                }
            }
            else if (method.ToLower() == "all")
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
                        if (item.WearLocation.Contains(WearLocation.None))
                        {
                            await this.communicator.SendToPlayer(user.Connection, $"You can't get {item.Name}.", cancellationToken);
                        }
                        else
                        {
                            user.Character.Inventory.Add(item.Clone());
                            await this.communicator.SendToPlayer(user.Connection, $"You get {item.Name}.", cancellationToken);
                            await this.communicator.SendToRoom(user.Character, user.Character.Location, user.ConnectionId, $"{user.Character.FirstName.FirstCharToUpper()} gets {item.Name}.", cancellationToken);
                            itemsToRemove.Add(item);
                        }
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

                var item = room.Items.ParseTargetName(method);

                if (item != null)
                {
                    if (item.WearLocation.Contains(WearLocation.None))
                    {
                        await this.communicator.SendToPlayer(user.Connection, $"You can't get {item.Name}.", cancellationToken);
                        return;
                    }
                    else
                    {
                        user.Character.Inventory.Add(item.Clone());
                        await this.communicator.SendToPlayer(user.Connection, $"You get {item.Name}.", cancellationToken);
                        await this.communicator.SendToRoom(user.Character, user.Character.Location, user.ConnectionId, $"{user.Character.FirstName.FirstCharToUpper()} gets {item.Name}.", cancellationToken);
                        itemsToRemove.Add(item);
                    }
                }
                else
                {
                    await this.communicator.SendToPlayer(user.Connection, $"That isn't here.", cancellationToken);
                }

                foreach (var itemToRemove in itemsToRemove)
                {
                    room.Items.Remove(itemToRemove);
                }
            }
        }

        private async Task EatItem(UserData user, string target, CancellationToken cancellationToken = default)
        {
            if (user.Character.Inventory == null || user.Character.Inventory.Count == 0)
            {
                await this.communicator.SendToPlayer(user.Connection, $"You don't have anything to eat.", cancellationToken);
                return;
            }

            var item = user.Character.Inventory.ParseTargetName(target);

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
                    user.Character.Hunger = new MaxCurrent(24, Math.Max(user.Character.Hunger.Current - 8, item.Value));
                    await this.communicator.SendToRoom(user.Character, user.Character.Location, user.ConnectionId, $"{user.Character.FirstName.FirstCharToUpper()} eats {item.Name}.", cancellationToken);
                    return;
                }
            }
            else
            {
                await this.communicator.SendToPlayer(user.Connection, $"You can't eat that.", cancellationToken);
            }
        }

        private async Task DropItem(UserData user, string target, CancellationToken cancellationToken = default)
        {
            var room = this.communicator.ResolveRoom(user.Character.Location);

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
                            room.Items.Add(item.Clone());
                            await this.communicator.SendToPlayer(user.Connection, $"You drop {item.Name}.", cancellationToken);
                            await this.communicator.SendToRoom(user.Character, user.Character.Location, user.ConnectionId, $"{user.Character.FirstName.FirstCharToUpper()} drops {item.Name}.", cancellationToken);
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

                    var item = user.Character.Inventory.ParseTargetName(target);

                    if (item != null)
                    {
                        room.Items.Add(item.Clone());
                        await this.communicator.SendToPlayer(user.Connection, $"You drop {item.Name}.", cancellationToken);
                        await this.communicator.SendToRoom(user.Character, user.Character.Location, user.ConnectionId, $"{user.Character.FirstName.FirstCharToUpper()} drops {item.Name}.", cancellationToken);
                        itemsToRemove.Add(item);
                    }
                    else
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

        private async Task Tell(UserData user, string target, string message, CancellationToken cancellationToken = default)
        {
            message = char.ToUpper(message[0]) + message[1..];
            target = char.ToUpper(target[0]) + target[1..];

            var commResult = await this.communicator.SendToPlayer(user.Character.FirstName, target, $"{user.Character.FirstName.FirstCharToUpper()} tells you \"<span class='tell'>{message}</span>\"", cancellationToken);

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

                        // Create the link between the two who are engaged in conversation.
                        if (Communicator.Tells.ContainsKey(user.Character.FirstName))
                        {
                            Communicator.Tells[user.Character.FirstName] = target;
                        }
                        else
                        {
                            Communicator.Tells.TryAdd(user.Character.FirstName, target);
                        }

                        if (Communicator.Tells.ContainsKey(target))
                        {
                            Communicator.Tells[target] = user.Character.FirstName;
                        }
                        else
                        {
                            Communicator.Tells.TryAdd(target, user.Character.FirstName);
                        }

                        break;
                    }
            }
        }

        private async Task ShowCharactersInArea(UserData actor, List<Character>? characters, List<Mobile>? mobiles, CancellationToken cancellationToken)
        {
            var sb = new StringBuilder();

            if (characters != null)
            {
                sb.Append($"Near you:<br/>");

                foreach (var character in characters)
                {
                    sb.Append($"<span class='scan'>{character.FirstName.FirstCharToUpper()} is in ");

                    var room = this.communicator.ResolveRoom(character.Location);

                    if (room != null)
                    {
                        sb.Append($"{room?.Name} [{room?.RoomId}]");
                    }

                    sb.Append("</span>");
                }
            }

            if (mobiles != null)
            {
                foreach (var mobile in mobiles)
                {
                    sb.Append($"<span class='scan'>{mobile.FirstName.FirstCharToUpper()} is in ");

                    var room = this.communicator.ResolveRoom(mobile.Location);

                    if (room != null)
                    {
                        sb.Append($"{room?.Name} [{room?.RoomId}]");
                    }

                    sb.Append("</span>");
                }
            }

            await this.communicator.SendToPlayer(actor.Connection, sb.ToString(), cancellationToken);
        }

        private async Task ShowPlayerScore(UserData user, CancellationToken cancellationToken = default)
        {
            StringBuilder sb = new ();

            List<Item> equipment = user.Character.Equipment;
            List<Effect> effects = user.Character.AffectedBy;

            int pierceTotal = equipment.Sum(a => a?.Pierce ?? 0) + effects.Sum(a => a?.Pierce ?? 0);
            int slashTotal = equipment.Sum(a => a?.Edged ?? 0) + effects.Sum(a => a?.Slash ?? 0);
            int bluntTotal = equipment.Sum(a => a?.Blunt ?? 0) + effects.Sum(a => a?.Blunt ?? 0);
            int magicTotal = equipment.Sum(a => a?.Magic ?? 0) + effects.Sum(a => a?.Magic ?? 0);

            sb.Append("<div class='player-score'><table><tr><td colspan='4'>");

            sb.Append($"<span class='player-score-title'>{user.Character.FirstName} {user.Character.MiddleName} {user.Character.LastName} {user.Character.Title}</span></td></tr>");

            sb.Append($"<tr><td colspan='4'>You are a level {user.Character.Level} {user.Character.Race} from The Void.</td></tr>");

            sb.Append($"<tr><td colspan='2'>You are {user.Character.Age} years of age.</td><td>Experience:</td><td>{user.Character.Experience}</td></tr>");

            sb.Append($"<tr><td class='player-section' colspan='4'>Vital Statistics</td></tr>");

            sb.Append($"<tr><td>Health:</td><td>{user.Character.Health.Current}/{user.Character.Health.Max}</td><td>Str:</td><td>{user.Character.Str}</td></tr>");

            sb.Append($"<tr><td>Mana:</td><td>{user.Character.Mana.Current}/{user.Character.Mana.Max}</td><td>Int:</td><td>{user.Character.Int}</td></tr>");

            sb.Append($"<tr><td>Movement:</td><td>{user.Character.Movement.Current}/{user.Character.Movement.Max}</td><td>Wis:</td><td>{user.Character.Wis}</td></tr>");

            sb.Append($"<tr><td>Currency:</td><td>{user.Character.Currency}</td><td>Dex:</td><td>{user.Character.Dex}</td></tr>");

            sb.Append($"<tr><td colspan='2'>&nbsp;</td><td>Con:</td><td>{user.Character.Con}</td></tr>");

            sb.Append($"<tr><td class='player-section' colspan='4'>Combat Rolls</td></tr>");

            sb.Append($"<tr><td>Hit dice:</td><td>{user.Character.HitDice}</td><td>Damage dice:</td><td>{user.Character.DamageDice}</td></tr>");

            sb.Append($"<tr><td class='player-section' colspan='4'>Armor</td></tr>");

            sb.Append($"<tr><td>Pierce:</td><td>{pierceTotal}%</td><td>Blunt:</td><td>{bluntTotal}%</td></tr>");

            sb.Append($"<tr><td>Edged:</td><td>{slashTotal}%</td><td>Magic:</td><td>{magicTotal}%</td></tr>");

            sb.Append($"<tr><td class='player-armor' colspan='4'>Spell Affects</td></tr>");

            if (user.Character.AffectedBy.Count > 0)
            {
                foreach (var effect in user.Character.AffectedBy)
                {
                    sb.Append($"<tr><td colspan='4' class='player-affect'>- {effect.Name} for {effect.Duration} hours.</td></tr>");
                }
            }
            else
            {
                sb.Append($"<tr><td colspan='4'>You are not affected by anything.</td></tr>");
            }

            sb.Append("</table></div>");

            await this.communicator.SendToPlayer(user.Connection, sb.ToString(), cancellationToken);
        }
    }
}