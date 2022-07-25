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
    public class ActionProcessor
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
                    .Where(a => a.Key.StartsWith(args.Action))
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
                            .Where(a => a.Key.StartsWith(args.Action))
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
        /// Configures all of the wizard (immortal) actions. Actions from this list cannot be accessed by mortal players.
        /// </summary>
        private void ConfigureWizActions()
        {
            this.wizActions.Add("goto", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoGoTo)));
            this.wizActions.Add("peace", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoPeace)));
            this.wizActions.Add("slay", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(4, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoSlay)));
            this.wizActions.Add("title", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(4, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoTitle)));
            this.wizActions.Add("transfer", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(4, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoTransfer)));
            this.wizActions.Add("wiznet", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(4, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoWiznet)));
        }

        /// <summary>
        /// Configures all of the actions based on the input. The numeric value in the KVP is the PRIORITY in which the command will
        /// be executed. So, if someone types "n", it will check "north", "ne", "nw", and "newbie" in that order.
        /// </summary>
        private void ConfigureActions()
        {
            this.actions.Add("affects", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoAffects)));
            this.actions.Add("commands", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoCommands)));
            this.actions.Add("dice", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(4, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoDice)));
            this.actions.Add("down", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoMove)));
            this.actions.Add("drink", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoDrink)));
            this.actions.Add("drop", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(3, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoDrop)));
            this.actions.Add("east", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoMove)));
            this.actions.Add("eat", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoEat)));
            this.actions.Add("emote", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(4, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoEmote)));
            this.actions.Add("equipment", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(3, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoEquipment)));
            this.actions.Add("examine", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(5, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoExamine)));
            this.actions.Add("flee", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoFlee)));
            this.actions.Add("get", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoGet)));
            this.actions.Add("give", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoGive)));
            this.actions.Add("help", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoHelp)));
            this.actions.Add("inventory", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoInventory)));
            this.actions.Add("kill", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoCombat)));
            this.actions.Add("look", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoLook)));
            this.actions.Add("murder", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoCombat)));
            this.actions.Add("newbie", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(4, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoNewbieChat)));
            this.actions.Add("north", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoMove)));
            this.actions.Add("ne", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoMove)));
            this.actions.Add("nw", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(3, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoMove)));
            this.actions.Add("pray", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoPray)));
            this.actions.Add("quit", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoQuit)));
            this.actions.Add("rest", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoRest)));
            this.actions.Add("remove", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoRemove)));
            this.actions.Add("reply", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoReply)));
            this.actions.Add("save", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(8, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoSave)));
            this.actions.Add("say", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(7, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoSay)));
            this.actions.Add("scan", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(4, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoScan)));
            this.actions.Add("score", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(4, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoScore)));
            this.actions.Add("skills", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(5, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoSkills)));
            this.actions.Add("sleep", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(6, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoSleep)));
            this.actions.Add("south", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoMove)));
            this.actions.Add("se", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoMove)));
            this.actions.Add("sw", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(3, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoMove)));
            this.actions.Add("spells", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(8, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoSpells)));
            this.actions.Add("subscribe", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(9, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoSubscribe)));
            this.actions.Add("tell", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(0, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoTell)));
            this.actions.Add("time", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoTime)));
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
                        sbToRoom.Append($"{actor.Character.FirstName} rolls {numDice} six-sided dice.<br/>");

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

                            sbToPlayer.Append($"<i class='fa-solid {dice}'></i>");
                            sbToRoom.Append($"<i class='fa-solid {dice}'></i>");
                            total += roll;
                        }

                        sbToPlayer.Append($"<br/>Your total is {total}.");
                        sbToRoom.Append($"<br/>{actor.Character.FirstName}'s total is {total}.");

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

            if (room != null)
            {
                var spring = room.Items.FirstOrDefault(i => i.ItemType == ItemType.Spring);
                if (spring != null)
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"You drink cool water from {spring.Name}.", cancellationToken);
                    actor.Character.Thirst.Current = Math.Min(8, actor.Character.Thirst.Current);
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
                    // TODO Implement drinking containers.
                    await this.communicator.SendToPlayer(actor.Connection, $"Not implemented.", cancellationToken);
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
                var sentence = args.Method;
                if (!string.IsNullOrWhiteSpace(sentence))
                {
                    sentence = sentence.ToLower();
                    await this.communicator.SendToPlayer(actor.Connection, $"{actor.Character.FirstName} {sentence}.", cancellationToken);
                    await this.communicator.SendToRoom(actor.Character, actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} {sentence}.", cancellationToken);
                }
            }
        }

        private async Task DoFlee(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            var room = this.communicator.ResolveRoom(actor.Character.Location);

            if (room.Exits.Count == 0)
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
                    await this.communicator.SendToRoom(actor.Character, actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} flees {dir}.", cancellationToken);

                    await this.communicator.PlaySound(actor.Character, AudioChannel.BackgroundSFX, Sounds.WALK, cancellationToken);

                    actor.Character.Location = new KeyValuePair<long, long>(randomExit.ToArea, randomExit.ToRoom);

                    await this.communicator.SendToRoom(actor.Character, actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} runs in!", cancellationToken);
                    await this.communicator.ShowRoomToPlayer(actor.Character, cancellationToken);
                }
            }
        }

        private async Task DoGet(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Get what?", cancellationToken);
            }
            else
            {
                await this.GetItem(actor, args.Method, cancellationToken);
            }
        }

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
                        await this.communicator.SendToPlayer(actor.Connection, $"{actor.Character.FirstName} gives you {itemToGive.Name}.", cancellationToken);
                        await this.communicator.SendToRoom(actor.Character.Location, actor.Character, targetPlayer.Character, $"{actor.Character.FirstName} gives {itemToGive.Name} to {targetPlayer.Character.FirstName}.", cancellationToken);
                    }
                    else
                    {
                        var targetMob = this.communicator.ResolveMobile(args.Target);

                        if (targetMob != null)
                        {
                            actor.Character.Inventory.Remove(itemToGive);
                            targetMob.Inventory.Add(itemToGive);

                            await this.communicator.SendToPlayer(actor.Connection, $"You give {itemToGive.Name} to {targetMob.FirstName}.", cancellationToken);
                            await this.communicator.SendToRoom(actor.Character.Location, actor.Character, targetMob, $"{actor.Character.FirstName} gives {itemToGive.Name} to {targetMob.FirstName}.", cancellationToken);
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
                    await this.communicator.SendToPlayer(actor.Connection, $"You examine {item.Name}.<br/>", cancellationToken);
                    await this.communicator.SendToRoom(actor.Character.Location, actor.Character, null, $"{actor.Character.FirstName} examines {item.Name}.<br/>", cancellationToken);

                    StringBuilder sb = new StringBuilder();

                    if (!string.IsNullOrWhiteSpace(item.Image))
                    {
                        sb.Append($"<div class='room-image'><img src='{item.Image}'/></div>");
                    }
                    else
                    {
                        sb.Append($"<div class='room-image room-image-none'></div>");
                    }

                    sb.Append($"{item.LongDescription}<br/>");
                    sb.Append($"{item.Name.FirstCharToUpper()} is of type {Enum.GetName<ItemType>(item.ItemType)?.ToString().ToLower()} and appears to have a durability of {item.Durability.Current}.<br/>");
                    sb.Append($"You value it at approximately {this.random.Next(Math.Max((int)item.Value - 100, 2), Math.Max((int)item.Value + 100, 4))} gold.");

                    await this.communicator.SendToPlayer(actor.Connection, sb.ToString(), cancellationToken);
                }
                else
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"You don't have that.", cancellationToken);
                }
            }
        }

        [MinimumLevel(90)]
        private async Task DoGoTo(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (actor.Character.Level < 100)
            {
                await this.communicator.SendToPlayer(actor.Connection, "Unknown command.", cancellationToken);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(args.Method))
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"Goto where?", cancellationToken);
                }
                else
                {
                    if (long.TryParse(args.Method, out long result))
                    {
                        await this.GotoRoom(actor, result, cancellationToken);
                    }
                    else
                    {
                        var player = this.communicator.ResolveCharacter(args.Method);

                        if (player != null)
                        {
                            var room = this.communicator.ResolveRoom(player.Character.Location);

                            if (room != null)
                            {
                                await this.GotoRoom(actor, player.Character.Location.Value, cancellationToken);
                            }
                        }
                        else
                        {
                            var location = this.communicator.ResolveMobileLocation(args.Method);

                            if (location != null)
                            {
                                await this.GotoRoom(actor, location.Value.Value, cancellationToken);
                            }
                            else
                            {
                                await this.communicator.SendToPlayer(actor.Connection, $"There's nobody here by that name.", cancellationToken);
                            }
                        }
                    }
                }
            }
        }

        private async Task DoHelp(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            var helpCommand = args.Method;

            var directoryInfo = new DirectoryInfo(@"Data/HelpFiles/");

            var file = directoryInfo.GetFiles().Where(f => f.Name.ToLower() == helpCommand + ".html").FirstOrDefault();

            if (file != null)
            {
                var content = File.ReadAllText(file.FullName);

                await this.communicator.SendToPlayer(actor.Connection, content, cancellationToken);
            }
            else
            {
                await this.communicator.SendToPlayer(actor.Connection, "No help files found by that name. You can start by typing HELP NEWBIE.", cancellationToken);
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

        private async Task DoLook(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(args.Method))
            {
#warning TODO: Need to be able to look IN stuff (e.g. "look in corpse")
                await this.communicator.ShowPlayerToPlayer(actor.Character, args.Method, cancellationToken);
            }
            else
            {
                await this.communicator.ShowRoomToPlayer(actor.Character, cancellationToken);
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
                    await this.communicator.SendToChannel(channel, actor.ConnectionId, $"{actor.Character.FirstName} newbie chats \"<span class='newbie'>{sentence}</span>\"", cancellationToken);
                }
            }
            else
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Newbie chat what?", cancellationToken);
            }
        }

        [MinimumLevel(90)]
        private async Task DoPeace(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (actor.Character.Level < 100)
            {
                await this.communicator.SendToPlayer(actor.Connection, "Unknown command.", cancellationToken);
            }
            else
            {
                if (Communicator.Users != null)
                {
                    var users = Communicator.Users.Where(u => u.Value.Character.Location.InSamePlace(actor.Character.Location));

                    // Stop all the users from fighting
                    foreach (var user in users)
                    {
                        user.Value.Character.CharacterFlags?.RemoveIfExists(CharacterFlags.Fighting);
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
                await this.communicator.SendToRoom(actor.Character, actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} stops all fighting in the room.", cancellationToken);
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
                    await this.communicator.SendToChannel(channel, actor.ConnectionId, $"{actor.Character.FirstName} prays \"<span class='newbie'>{sentence}</span>\"", cancellationToken);
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

        private async Task DoRest(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            await this.communicator.SendToPlayer(actor.Connection, $"You kick back and rest.", cancellationToken);
            await this.communicator.SendToRoom(actor.Character, actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} kicks back and rests.", cancellationToken);
            actor.Character.CharacterFlags.AddIfNotExists(CharacterFlags.Resting);
        }

        private async Task DoMove(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            await this.MovePlayer(actor, ParseDirection(args.Action), cancellationToken);
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
            await this.communicator.SendToRoom(actor.Character, actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} says \"<span class='say'>{sentence}</span>\"", cancellationToken);
        }

        private async Task DoScore(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            await this.ShowPlayerScore(actor, cancellationToken);
        }

        private async Task DoScan(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            var room = this.communicator.ResolveRoom(actor.Character.Location);

            await this.communicator.SendToPlayer(actor.Connection, $"You scan in all directions.", cancellationToken);
            await this.communicator.SendToRoom(actor.Character, actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} scans all around.", cancellationToken);

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
                        sb.Append($"<span class='scan'>{player.FirstName}</span>");
                    }
                }

                if (mobs != null)
                {
                    foreach (var mob in mobs)
                    {
                        sb.Append($"<span class='scan'>{mob.FirstName}</span>");
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
            await this.communicator.SendToRoom(actor.Character, actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} goes to sleep.", cancellationToken);
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

        [MinimumLevel(100)]
        private async Task DoSlay(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Slay whom?", cancellationToken);
            }
            else
            {
                var target = args.Method;

                var player = this.communicator.ResolveCharacter(target);

                if (player != null)
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"You SLAY {player.Character.FirstName} in cold blood!", cancellationToken);
                    await this.communicator.SendToRoom(actor.Character.Location, actor.Character, player.Character, $"{actor.Character.FirstName} SLAYS {player.Character.FirstName} in cold blood!", cancellationToken);
                    await this.communicator.SendToPlayer(player.Connection, $"{actor.Character} has SLAIN you!", cancellationToken);
                    await this.combat.KillPlayer(player.Character, actor.Character, cancellationToken);
                }
                else
                {
                    var mobile = this.communicator.ResolveMobile(target);

                    if (mobile != null)
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"You SLAY {mobile.FirstName} in cold blood!", cancellationToken);
                        await this.communicator.SendToRoom(actor.Character.Location, actor.Character, null, $"{actor.Character.FirstName} SLAYS {mobile.FirstName} in cold blood!", cancellationToken);
                        await this.combat.KillMobile(mobile, actor.Character, cancellationToken);
                    }
                    else
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"They aren't here.", cancellationToken);
                    }
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

        [MinimumLevel(95)]
        private async Task DoTitle(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method) && string.IsNullOrWhiteSpace(args.Target))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Set title to what? (title [\"title\"] [player (optional)])", cancellationToken);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(args.Target))
                {
                    var player = this.communicator.ResolveCharacter(args.Target);

                    if (player != null)
                    {
                        player.Character.Title = args.Method;
                        await this.communicator.SaveCharacter(player.Character);
                        await this.communicator.SendToPlayer(actor.Connection, $"{player.Character.FirstName}'s title set to \"{args.Method}\".", cancellationToken);
                    }
                    else
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"They aren't here.", cancellationToken);
                    }
                }
                else
                {
                    actor.Character.Title = args.Method;
                    await this.communicator.SaveCharacter(actor.Character);
                    await this.communicator.SendToPlayer(actor.Connection, $"Title set to \"{args.Method}\".", cancellationToken);
                }
            }
        }

        [MinimumLevel(90)]
        private async Task DoTransfer(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Transfer whom?", cancellationToken);
            }
            else
            {
                var target = args.Method;

                var player = this.communicator.ResolveCharacter(target);

                if (player != null)
                {
                    player.Character.Location = actor.Character.Location;
                    await this.communicator.SendToPlayer(actor.Connection, $"You have transferred {player.Character.FirstName} here.", cancellationToken);
                    await this.communicator.SendToRoom(actor.Character.Location, actor.Character, player.Character, $"{player.Character.FirstName} arrives in a puff of smoke.", cancellationToken);
                    await this.communicator.SendToPlayer(player.Connection, $"{actor.Character} has summoned you!", cancellationToken);
                    await this.communicator.SendToRoom(player.Character.Location, actor.Character, player.Character, $"{player.Character.FirstName} vanishes in a flash of light.", cancellationToken);
                }
                else
                {
                    var mobile = this.communicator.ResolveMobile(target);

                    if (mobile != null)
                    {
                        var oldRoom = this.communicator.ResolveRoom(mobile.Location);
                        var newRoom = this.communicator.ResolveRoom(actor.Character.Location);

                        var oldMob = oldRoom.Mobiles.FirstOrDefault(m => m.CharacterId == mobile.CharacterId);

                        if (oldMob != null)
                        {
                            oldRoom.Mobiles.Remove(oldMob);
                        }

                        newRoom.Mobiles.Add(mobile);

                        mobile.Location = actor.Character.Location;

                        await this.communicator.SendToPlayer(actor.Connection, $"You have transferred {mobile.FirstName} here.", cancellationToken);
                        await this.communicator.SendToRoom(actor.Character.Location, actor.Character, null, $"{mobile.FirstName} arrives in a puff of smoke.", cancellationToken);
                        await this.communicator.SendToRoom(mobile.Location, actor.Character, null, $"{mobile.FirstName} vanishes in a flash of light.", cancellationToken);
                    }
                    else
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"They aren't here.", cancellationToken);
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

                var mobilesInArea = this.communicator.GetMobilesInArea(actor.Character.Location.Key)?.Where(m => m.FirstName.ToLower().StartsWith(args.Method.ToLower())).ToList();

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
                await this.communicator.SendToRoom(actor.Character, actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} wakes and stands up.", cancellationToken);
                actor.Character.CharacterFlags.RemoveIfExists(CharacterFlags.Resting);
                actor.Character.CharacterFlags.RemoveIfExists(CharacterFlags.Sleeping);
            }
            else
            {
                await this.communicator.SendToPlayer(actor.Connection, $"You are already awake and standing.", cancellationToken);
            }
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
                        await this.communicator.SendToRoom(actor.Character, actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} removes {target.Name}.", cancellationToken);
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
                        await this.communicator.SendToRoom(actor.Character, actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} removes {target.Name}.", cancellationToken);
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
                                await this.communicator.SendToRoom(actor.Character, actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} removes {targetLocationItem.Name}.", cancellationToken);

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
                            await this.communicator.SendToRoom(actor.Character, actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} stops wielding {targetLocationItem.Name}.", cancellationToken);
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

        [MinimumLevel(90)]
        private async Task DoWiznet(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
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

        private async Task DoYell(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            var sentence = args.Method;

            if (!string.IsNullOrWhiteSpace(sentence))
            {
                sentence = char.ToUpper(sentence[0]) + sentence[1..];
                await this.communicator.SendToPlayer(actor.Connection, $"You yell \"<span class='yell'>{sentence}</b>\"", cancellationToken);
                await this.communicator.SendToArea(actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} yells \"<span class='yell'>{sentence}!</span>\"", cancellationToken);
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
            await this.communicator.SendToRoom(actor.Character, actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} {verb}s {item.Name}.", cancellationToken);
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
                    await this.communicator.SendToRoom(null, user.Character.Location, user.ConnectionId, $"{user.Character.FirstName} vanishes.", cancellationToken);
                    user.Character.Location = new KeyValuePair<long, long>(targetRoom.AreaId, targetRoom.RoomId);
                    await this.communicator.ShowRoomToPlayer(user.Character, cancellationToken);
                    return;
                }
            }

            await this.communicator.SendToPlayer(user.Connection, $"You were unable to teleport there.", cancellationToken);
        }

        private async Task GetItem(UserData user, string target, CancellationToken cancellationToken = default)
        {
            var room = this.communicator.ResolveRoom(user.Character.Location);

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
                        if (item.WearLocation.Contains(WearLocation.None))
                        {
                            await this.communicator.SendToPlayer(user.Connection, $"You can't get {item.Name}.", cancellationToken);
                        }
                        else
                        {
                            user.Character.Inventory.Add(item.Clone());
                            await this.communicator.SendToPlayer(user.Connection, $"You get {item.Name}.", cancellationToken);
                            await this.communicator.SendToRoom(user.Character, user.Character.Location, user.ConnectionId, $"{user.Character.FirstName} gets {item.Name}.", cancellationToken);
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

                var item = room.Items.ParseItemName(target);

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
                        await this.communicator.SendToRoom(user.Character, user.Character.Location, user.ConnectionId, $"{user.Character.FirstName} gets {item.Name}.", cancellationToken);
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

            var item = user.Character.Inventory.ParseItemName(target);

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
                    await this.communicator.SendToRoom(user.Character, user.Character.Location, user.ConnectionId, $"{user.Character.FirstName} eats {item.Name}.", cancellationToken);
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
                            await this.communicator.SendToRoom(user.Character, user.Character.Location, user.ConnectionId, $"{user.Character.FirstName} drops {item.Name}.", cancellationToken);
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

                    var item = user.Character.Inventory.ParseItemName(target);

                    if (item != null)
                    {
                        room.Items.Add(item.Clone());
                        await this.communicator.SendToPlayer(user.Connection, $"You drop {item.Name}.", cancellationToken);
                        await this.communicator.SendToRoom(user.Character, user.Character.Location, user.ConnectionId, $"{user.Character.FirstName} drops {item.Name}.", cancellationToken);
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

            var commResult = await this.communicator.SendToPlayer(user.Character.FirstName, target, $"{user.Character.FirstName} tells you \"<span class='tell'>{message}</span>\"", cancellationToken);

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

            var room = this.communicator.ResolveRoom(user.Character.Location);

            Exit? exit = room.Exits?.FirstOrDefault(e => e.Direction == direction);

            if (exit != null)
            {
                var newArea = this.world.Areas.FirstOrDefault(a => a.AreaId == exit.ToArea);
                var newRoom = newArea?.Rooms?.FirstOrDefault(r => r.RoomId == exit.ToRoom);

                if (newArea != null && newRoom != null)
                {
                    string? dir = Enum.GetName(typeof(Direction), exit.Direction)?.ToLower();
                    await this.communicator.SendToPlayer(user.Connection, $"You go {dir}.<br/>", cancellationToken);
                    await this.communicator.SendToRoom(user.Character, user.Character.Location, user.ConnectionId, $"{user.Character.FirstName} leaves {dir}.", cancellationToken);

                    await this.communicator.PlaySound(user.Character, AudioChannel.BackgroundSFX, Sounds.WALK, cancellationToken);

                    user.Character.Location = new KeyValuePair<long, long>(exit.ToArea, exit.ToRoom);

                    user.Character.Movement.Current -= GetTerrainMovementPenalty(newRoom);

                    await this.communicator.SendToRoom(user.Character, user.Character.Location, user.ConnectionId, $"{user.Character.FirstName} enters.", cancellationToken);
                    await this.communicator.ShowRoomToPlayer(user.Character, cancellationToken);
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

        private async Task ShowCharactersInArea(UserData actor, List<Character>? characters, List<Mobile>? mobiles, CancellationToken cancellationToken)
        {
            var sb = new StringBuilder();

            if (characters != null)
            {
                sb.Append($"Near you:<br/>");

                foreach (var character in characters)
                {
                    sb.Append($"<span class='scan'>{character.FirstName} is in ");

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
                    sb.Append($"<span class='scan'>{mobile.FirstName} is in ");

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