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
    using System.Numerics;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core;
    using Legendary.Core.Attributes;
    using Legendary.Core.Contracts;
    using Legendary.Core.Extensions;
    using Legendary.Core.Models;
    using Legendary.Core.Types;
    using Legendary.Data.Contracts;
    using Legendary.Engine.Attributes;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Extensions;
    using Legendary.Engine.Helpers;
    using Legendary.Engine.Models;
    using Legendary.Engine.Models.Output;
    using Legendary.Engine.Models.Skills;
    using Legendary.Engine.Models.Spells;
    using Legendary.Engine.Output;
    using Microsoft.AspNetCore.Hosting;
    using MongoDB.Driver;
    using Newtonsoft.Json;
    using SharpCompress.Compressors.Xz;

    /// <summary>
    /// Used to perform quick lookups of skills.
    /// </summary>
    public partial class ActionProcessor
    {
        private readonly ICommunicator communicator;
        private readonly IEnvironment environment;
        private readonly IWorld world;
        private readonly ILogger logger;
        private readonly IMessageProcessor messageProcessor;
        private readonly IDataService dataService;
        private readonly ActionHelper actionHelper;
        private readonly AwardProcessor awardProcessor;
        private readonly IRandom random;
        private readonly CombatProcessor combat;
        private IDictionary<string, KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>> actions = new Dictionary<string, KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>>();
        private IDictionary<string, KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>> wizActions = new Dictionary<string, KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionProcessor"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="random">The Random Number generator.</param>
        /// <param name="combat">The combat class.</param>
        /// <param name="messageProcessor">The message processor.</param>
        /// <param name="dataService">The data service.</param>
        public ActionProcessor(ICommunicator communicator, IEnvironment environment, IWorld world, ILogger logger, IRandom random, CombatProcessor combat, IMessageProcessor messageProcessor, IDataService dataService)
        {
            this.communicator = communicator;
            this.world = world;
            this.environment = environment;
            this.logger = logger;
            this.random = random;
            this.messageProcessor = messageProcessor;
            this.combat = combat;
            this.actionHelper = new ActionHelper(this.communicator, random, world, logger, combat);
            this.awardProcessor = new AwardProcessor(communicator, world, logger, random, combat);
            this.dataService = dataService;

            this.ConfigureActions();
            this.ConfigureWizActions();
        }

        /// <summary>
        /// Parses a direction enum from a string value.
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <returns>Direction.</returns>
        public static Direction ParseDirection(string direction)
        {
            return direction.ToLower() switch
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
        public static string ParseFriendlyDirection(Direction direction)
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

        /// <summary>
        /// Executes the action provided by the command.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <param name="args">The input args.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task DoAction(UserData actor, CommandArgs args, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the matching actions for the command word ordered by preference.
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
                        if (actor.Character.CharacterFlags.Contains(CharacterFlags.Sleeping) && action.Key.ToLower() != "wake" && action.Key.ToLower() != "quit")
                        {
                            await this.communicator.SendToPlayer(actor.Connection, "You can't do that while you're sleeping.", cancellationToken);
                            return;
                        }
                        else
                        {
                            var sightRequiredAttrib = (SightRequired?)action.Value.Value.GetMethodInfo()?.GetCustomAttribute(typeof(SightRequired));

                            if (sightRequiredAttrib != null)
                            {
                                if (!PlayerHelper.CanPlayerSee(this.environment, this.communicator, actor.Character))
                                {
                                    await this.communicator.SendToPlayer(actor.Connection, "You can't see anything, it's pitch black.", cancellationToken);
                                    return;
                                }
                                else
                                {
                                    await action.Value.Value(actor, args, cancellationToken);
                                }
                            }
                            else
                            {
                                await action.Value.Value(actor, args, cancellationToken);
                            }
                        }
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
        /// Gets all items from a given container and places them in the actor's inventory.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <param name="container">The container.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task ItemsFromContainer(Character actor, Item? container, CancellationToken cancellationToken)
        {
            if (container != null)
            {
                if (container.Contains != null && container.Contains.Count > 0)
                {
                    List<IItem> itemsToRemove = new ();

                    foreach (var item in container.Contains)
                    {
                        // Item, no container, check the item.
                        if (item.ItemType == ItemType.Currency)
                        {
                            await this.communicator.SendToPlayer(actor, $"You get {item.Name} from {container.Name}.", cancellationToken);
                            await this.communicator.PlaySound(actor, AudioChannel.BackgroundSFX2, Sounds.COINS_BUY, cancellationToken);
                            actor.Currency += item.Value;
                            itemsToRemove.Add(item);
                        }
                        else if (item.WearLocation.Contains(WearLocation.None))
                        {
                            await this.communicator.SendToPlayer(actor, $"You can't get {item.Name} from {container.Name}.", cancellationToken);
                        }
                        else
                        {
                            if (item != null)
                            {
                                if (ItemHelper.CanCarry(actor, item))
                                {
                                    await this.communicator.SendToPlayer(actor, $"You get {item.Name} from {container.Name}.", cancellationToken);
                                    await this.communicator.SendToRoom(actor.Location, actor, null, $"{actor.FirstName.FirstCharToUpper()} gets {item.Name} from {container.Name}.", cancellationToken);

                                    var itemToClone = (Item)item;
                                    actor.Inventory.Add(itemToClone.Clone());
                                    itemsToRemove.Add(item);
                                }
                                else
                                {
                                    await this.communicator.SendToPlayer(actor, $"You can't carry that much weight.", cancellationToken);
                                }
                            }
                        }
                    }

                    container.Contains.RemoveAll(i => itemsToRemove.Any(r => r.ItemId == i.ItemId));
                }
                else
                {
                    await this.communicator.SendToPlayer(actor, $"{container.Name.FirstCharToUpper()} has nothing in it.", cancellationToken);
                }
            }
            else
            {
                await this.communicator.SendToPlayer(actor, $"That isn't here.", cancellationToken);
            }
        }

        /// <summary>
        /// Moves all items from a corpse to the room. This happens if the corpse is sacrificed before being looted.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <param name="corpse">The corpse.</param>
        public void ItemsFromCorpseToRoom(Character actor, Item? corpse)
        {
            if (corpse != null)
            {
                if (corpse.Contains != null && corpse.Contains.Count > 0)
                {
                    List<IItem> itemsToRemove = new ();
                    var room = this.communicator.ResolveRoom(actor.Location);

                    foreach (var item in corpse.Contains)
                    {
                        if (item != null)
                        {
                            var itemToClone = (Item)item;
                            room?.Items.Add(itemToClone.Clone());
                            itemsToRemove.Add(item);
                        }
                    }

                    corpse.Contains.RemoveAll(i => itemsToRemove.Any(r => r.ItemId == i.ItemId));
                }
            }
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
            this.actions.Add("area", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(3, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoArea)));
            this.actions.Add("areas", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(3, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoAreas)));
            this.actions.Add("alist", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(3, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoAreas)));
            this.actions.Add("autoloot", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(4, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoAutoloot)));
            this.actions.Add("autosac", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(5, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoAutosac)));
            this.actions.Add("awards", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(6, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoAwards)));
            this.actions.Add("avatar", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoAvatar)));
            this.actions.Add("buy", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoBuy)));
            this.actions.Add("close", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoClose)));
            this.actions.Add("commands", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoCommands)));
            this.actions.Add("consider", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(3, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoConsider)));
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
            this.actions.Add("group", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoGroup)));
            this.actions.Add("gtell", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoGTell)));
            this.actions.Add("help", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoHelp)));
            this.actions.Add("inventory", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoInventory)));
            this.actions.Add("kill", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoCombat)));
            this.actions.Add("learn", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(4, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoLearn)));
            this.actions.Add("list", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(3, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoList)));
            this.actions.Add("lock", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoLock)));
            this.actions.Add("look", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoLook)));
            this.actions.Add("message", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoMessage)));
            this.actions.Add("messages", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoMessages)));
            this.actions.Add("murder", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoCombat)));
            this.actions.Add("newbie", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(4, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoNewbieChat)));
            this.actions.Add("north", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoMove)));
            this.actions.Add("ne", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoMove)));
            this.actions.Add("nw", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(3, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoMove)));
            this.actions.Add("open", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoOpen)));
            this.actions.Add("outfit", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoOutfit)));
            this.actions.Add("put", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoPut)));
            this.actions.Add("pray", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoPray)));
            this.actions.Add("practice", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(3, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoPractice)));
            this.actions.Add("quaff", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoQuaff)));
            this.actions.Add("quit", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoQuit)));
            this.actions.Add("rest", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoRest)));
            this.actions.Add("remove", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoRemove)));
            this.actions.Add("repair", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(5, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoRepair)));
            this.actions.Add("reply", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(3, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoReply)));
            this.actions.Add("report", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(4, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoReport)));
            this.actions.Add("sacrifice", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(5, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoSacrifice)));
            this.actions.Add("save", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(4, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoSave)));
            this.actions.Add("say", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoSay)));
            this.actions.Add("scan", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(7, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoScan)));
            this.actions.Add("sell", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(6, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoSell)));
            this.actions.Add("score", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(3, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoScore)));
            this.actions.Add("skills", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(8, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoSkills)));
            this.actions.Add("sleep", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(9, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoSleep)));
            this.actions.Add("smote", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(3, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoSmote)));
            this.actions.Add("south", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoMove)));
            this.actions.Add("se", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoMove)));
            this.actions.Add("sw", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(3, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoMove)));
            this.actions.Add("spells", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(8, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoSpells)));
            this.actions.Add("speak", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(9, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoSpeak)));
            this.actions.Add("subscribe", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(10, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoSubscribe)));
            this.actions.Add("tell", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(0, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoTell)));
            this.actions.Add("time", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoTime)));
            this.actions.Add("train", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoTrain)));
            this.actions.Add("ungroup", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(3, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoUngroup)));
            this.actions.Add("unlock", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoUnlock)));
            this.actions.Add("unsubscribe", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoUnsubscribe)));
            this.actions.Add("up", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoMove)));
            this.actions.Add("value", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoValue)));
            this.actions.Add("visible", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoVisible)));
            this.actions.Add("west", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoMove)));
            this.actions.Add("wear", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoWear)));
            this.actions.Add("where", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(3, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoWhere)));
            this.actions.Add("who", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(4, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoWho)));
            this.actions.Add("wield", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(5, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoWield)));
            this.actions.Add("wake", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(6, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoWake)));
            this.actions.Add("watch", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(7, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoWatch)));
            this.actions.Add("wimpy", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(7, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoWimpy)));
            this.actions.Add("worth", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(8, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoWorth)));
            this.actions.Add("yell", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(0, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoYell)));
        }

        [HelpText("<p>Displays a list of things your player is currently affected by.</p><ul><li>affects</li></ul>")]
        private async Task DoAffects(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            StringBuilder sb = new ();

            if (actor.Character.AffectedBy.Count > 0)
            {
                foreach (var effect in actor.Character.AffectedBy)
                {
                    if (effect.Name != nameof(Sneak) && effect.Name != nameof(Hide))
                    {
                        sb.Append($"<span class='player-affect'><li>{effect.Name} for {effect.Duration} hours.</li></span>");
                    }
                }
            }
            else
            {
                sb.Append($"<span class='player-affect'><li>You are not affected by anything.</li></span>");
            }

            await this.communicator.SendToPlayer(actor.Connection, sb.ToString(), cancellationToken);
        }

        [HelpText("<p>Generates an avatar based on your character description. May only be done once per level. Note: Only the first 400 chracters of your description are considered.</p>")]
        private async Task DoAvatar(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(actor.Character.LongDescription))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"You do not have a description. You must have a description to create an avatar.", cancellationToken);
            }
            else
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Your avatar is being generated based on your description. It will be saved to your profile when complete. <span class='wait'></span>", cancellationToken);
                var image = await this.communicator.LanguageProcessor.GenerateImage(actor.Character);
                if (!string.IsNullOrWhiteSpace(image))
                {
                    // TODO: actor.Character.HasGeneratedImage = true;
                    actor.Character.Image = image;
                    await this.communicator.SaveCharacter(actor);
                    await this.communicator.SendToPlayer(actor.Connection, $"Your avatar is generated. Type 'LOOK SELF' to review.", cancellationToken);
                }
                else
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"Unable to generate your avatar at this time.", cancellationToken);
                }
            }
        }

        [SightRequired]
        [HelpText("<p>Closes a closeable container or door. See also: HELP OPEN, HELP LOCK, HELP UNLOCK.</p><ul><li>close <em>item</em></li><li>close <em>direction</em></li></ul>")]
        private async Task DoClose(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Close what?", cancellationToken);
                return;
            }

            if (actor.Character.CharacterFlags.Contains(CharacterFlags.Ghost))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"You can't do that, you're a ghost.", cancellationToken);
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
                            await this.communicator.SendToPlayer(actor.Connection, $"It's already closed.", cancellationToken);
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

        [SightRequired]
        [HelpText("<p>Attacks another player or NPC. This will initiate combat. See also: HELP FLEE.</p><ul><li>kill <em>target</em></li><li>murder <em>target</em></li></ul>")]
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

        [HelpText("<p>Displays all available player commands. In many cases, a command can be shortened (e.g. 'l' for 'look'). For more information:</p><ul><li>help <em>command</em></li></ul>")]
        private async Task DoCommands(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            await this.communicator.SendToPlayer(actor.Connection, $"Available Commands:<br/>", cancellationToken);

            StringBuilder sb = new ();

            var commands = this.actions.OrderBy(a => a.Key);

            foreach (var kvp in commands)
            {
                sb.Append($"<span class='command'>{kvp.Key}</span>");
            }

            await this.communicator.SendToPlayer(actor.Connection, sb.ToString(), cancellationToken);
        }

        [SightRequired]
        [HelpText("<p>Rolls up to five six-sided dice, which will display in your current room.</p><ul><li>dice <em>3</em></li></ul>")]
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
                        StringBuilder sbToPlayer = new ();
                        StringBuilder sbToRoom = new ();

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

        [SightRequired]
        [HelpText("<p>Drinks a liquid from a container item in your inventory. Reduces thirst. See also: HELP EAT</p><ul><li>drink <em>item</em></li></ul>")]
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

                actor.Character.Thirst.Current = 0;

                // Bugfix.
                actor.Character.Thirst.Max = 72;

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

                                actor.Character.Thirst.Current -= 8;

                                // Bugfix.
                                actor.Character.Thirst.Max = 72;

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

        [HelpText("<p>Drops an item from your inventory to the ground. See also: HELP GET</p><ul><li>drop <em>item</em></li><li>drop <em>all</em></li></ul>")]
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

        [SightRequired]
        [HelpText("<p>Eats a consumable food item in your inventory. Reduces hunger. See also: HELP DRINK</p><ul><li>eat <em>item</em></li></ul>")]
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

        [HelpText("<p>Lets your player perform a custom action which will be visible in a room. See also: HELP EMOTES</p><ul><li>emote <em>message</em></li></ul>")]
        private async Task DoEmote(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Emote what?", cancellationToken);
            }
            else
            {
                var sentence = string.Join(' ', new string?[2] { args.Method, args.Target }).Trim();
                if (!string.IsNullOrWhiteSpace(sentence))
                {
                    if (!char.IsPunctuation(sentence[sentence.Length - 1]))
                    {
                        sentence += ".";
                    }

                    await this.communicator.SendToPlayer(actor.Connection, $"{actor.Character.FirstName.FirstCharToUpper()} {sentence.Trim()}", cancellationToken);

                    await this.communicator.SendToRoom(actor.Character, actor.Character.Location, sentence, cancellationToken);

                    var players = this.communicator.GetPlayersInRoom(actor.Character, actor.Character.Location);

                    if (players != null)
                    {
                        foreach (var player in players)
                        {
                            if (player != actor.Character)
                            {
                                if (PlayerHelper.CanPlayerSeePlayer(this.environment, this.communicator, player, actor.Character) && player.CharacterId != actor.Character.CharacterId)
                                {
                                    await this.communicator.SendToPlayer(actor.Character, $"{actor.Character.FirstName.FirstCharToUpper()} {sentence.Trim()}", cancellationToken);
                                }
                            }
                        }
                    }

                    // See if any AI mobs in the room will communicate with the player.
                    var commsTask = this.communicator.CheckMobCommunication(actor.Character, actor.Character.Location, $"{actor.Character.FirstName.FirstCharToUpper()} {sentence.Trim()}.", cancellationToken);

                    // Run this task on a separate, synchronous thread, so we don't block. This is fire and forget.
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Task.Run(async () => { await commsTask; }, cancellationToken);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
            }
        }

        [HelpText("<p>An emote is an automatic action your player can do, which will be visible in a room. For a list of emotes, type EMOTES. See also: HELP EMOTE</p>")]
        private async Task DoEmotes(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            await this.communicator.SendToPlayer(actor.Connection, $"Available Emotes:<br/>", cancellationToken);

            StringBuilder sb = new ();

            var commands = Emotes.Actions.OrderBy(a => a.Key);

            foreach (var kvp in commands)
            {
                sb.Append($"<span class='command'>{kvp.Key}</span>");
            }

            await this.communicator.SendToPlayer(actor.Connection, sb.ToString(), cancellationToken);
        }

        [HelpText("<p>Empties a drink container in your inventory of all of its contents.</p><ul><li>empty <em>item</em></li></ul>")]
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

        [SightRequired]
        [HelpText("<p>Fill a container in your inventory from a liquid source, like a fountain.</p><ul><li>fill <em>container</em></li></ul>")]
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

        [HelpText("<p>Flee from combat, as long as there is an exit to flee through.</p><ul><li>flee</li></ul>")]
        private async Task DoFlee(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (!actor.Character.CharacterFlags.Contains(CharacterFlags.Fighting))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"You're not in combat.", cancellationToken);
                return;
            }

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
                    await this.communicator.SendToRoom(actor.Character, actor.Character.Location, $"{actor.Character.FirstName.FirstCharToUpper()} flees {dir}.", cancellationToken);

                    await this.communicator.PlaySound(actor.Character, AudioChannel.BackgroundSFX, Sounds.WALK, cancellationToken);

                    actor.Character.Location = new KeyValuePair<long, long>(randomExit.ToArea, randomExit.ToRoom);

                    await this.communicator.SendToRoom(actor.Character, actor.Character.Location, $"{actor.Character.FirstName.FirstCharToUpper()} runs in!", cancellationToken);
                    await this.communicator.ShowRoomToPlayer(actor.Character, cancellationToken);
                }
            }
        }

        [SightRequired]
        [HelpText("<p>Follow a player. As the player moves about, so will you, unless you rest or sleep. To stop following, follow self.</p><ul><li>follow <em>target</em></li><li>follow self</li></ul>")]
        private async Task DoFollow(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Follow whom?", cancellationToken);
            }
            else
            {
                // Myself, yourself, self, etc.
                if (args.Method.ToLower().Trim().EndsWith("self"))
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

                    // If the player had followers, remove them.
                    if (actor.Character.Followers != null && actor.Character.Followers.Count > 0)
                    {
                        foreach (var follower in actor.Character.Followers)
                        {
                            var target = this.communicator.ResolveCharacter(follower);

                            if (target != null)
                            {
                                target.Character.Following = null;
                                await this.communicator.SendToPlayer(target.Connection, $"You stop following {actor.Character.FirstName}.", cancellationToken);
                            }
                        }

                        actor.Character.Followers = new List<long>();
                    }

                    await this.communicator.SendToPlayer(actor.Connection, $"You now follow yourself, and yourself alone, and nobody but you follows...yourself.", cancellationToken);
                }
                else
                {
                    var target = this.communicator.ResolveCharacter(args.Method);

                    if (target != null)
                    {
                        // You can follow a person, but they have to be in the same room.
                        if (target.Character.Location.Value == actor.Character.Location.Value)
                        {
                            if (!target.Character.Followers.Contains(actor.Character.CharacterId))
                            {
                                target.Character.Followers.Add(actor.Character.CharacterId);
                                actor.Character.Following = target.Character.CharacterId;
                                await this.communicator.SendToPlayer(actor.Connection, $"You begin following {target.Character.FirstName}.", cancellationToken);
                                await this.communicator.SendToRoom(actor.Character.Location, actor.Character, target.Character, $"{actor.Character.FirstName.FirstCharToUpper()} starts following {target.Character.FirstName}.", cancellationToken);
                                await this.communicator.SendToPlayer(target.Connection, $"{actor.Character.FirstName} now follows you.", cancellationToken);
                            }
                            else
                            {
                                await this.communicator.SendToPlayer(actor.Connection, $"You are already following {target.Character.FirstName}.", cancellationToken);
                            }
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

        [SightRequired]
        [HelpText("<p>Gets one or more items and places it into your inventory.</p><ul><li>get <em>target</em></li><li>get all <em>target</em></li></ul>")]
        private async Task DoGet(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Get what?", cancellationToken);
            }
            else
            {
                await this.GetItem(actor, args, cancellationToken);
            }
        }

        [SightRequired]
        [HelpText("<p>Gives an item from your inventory to a target.<p><ul><li>give <em>item</em> <em>target</em></li><li>give <em>10</em> gold <em>target</em></li></ul>")]
        private async Task DoGive(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method) || string.IsNullOrEmpty(args.Target))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Give what to whom?", cancellationToken);
            }
            else
            {
                switch (args.Method.ToLower())
                {
                    default:
                        break;
                    case "gold":
                    case "silver":
                    case "copper":
                        await this.GiveCurrency(actor, args, cancellationToken);
                        return;
                }

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
                        if (ItemHelper.CanCarry(targetPlayer.Character, itemToGive))
                        {
                            actor.Character.Inventory.Remove(itemToGive);
                            actor.Character.CarryWeight.Current -= (double)itemToGive.Weight;
                            targetPlayer.Character.Inventory.Add(itemToGive.DeepCopy());

                            await this.communicator.SendToPlayer(actor.Connection, $"You give {itemToGive.Name} to {targetPlayer.Character.FirstName}.", cancellationToken);
                            await this.communicator.SendToPlayer(targetPlayer.Connection, $"{actor.Character.FirstName.FirstCharToUpper()} gives you {itemToGive.Name}.", cancellationToken);

                            await this.communicator.SendToRoom(actor.Character, actor.Character.Location, $"{actor.Character.FirstName.FirstCharToUpper()} gives {itemToGive.Name} to {targetPlayer.Character.FirstName}.", cancellationToken);
                        }
                        else
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"{targetPlayer.Character.FirstName.FirstCharToUpper()} can't carry that much weight.", cancellationToken);
                        }
                    }
                    else
                    {
                        var targetMob = this.communicator.ResolveMobile(args.Target, actor.Character);

                        if (targetMob != null)
                        {
                            actor.Character.Inventory.Remove(itemToGive);
                            targetMob.Inventory.Add(itemToGive.DeepCopy());

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

        [HelpText("<p>Displays the current equipment worn by your player. See also: HELP INVENTORY, HELP WEAR.</p><ul><li>equipment</li></ul>")]
        private async Task DoEquipment(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            var equipment = ActionHelper.GetEquipment(actor.Character);
            await this.communicator.SendToPlayer(actor.Connection, equipment, cancellationToken);
        }

        [SightRequired]
        [HelpText("<p>Examines an item in your inventory closely to determine some of its attributes. If there are more than one you wish to examine, put a number and a period before it. See examples.</p><ul><li>examine <em>item</em></li><li>examine <em>2.item</em></li></ul>")]
        private async Task DoExamine(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Examine what?", cancellationToken);
            }
            else
            {
                // examine backpack
                // examine 2.backpack
                if (actor.Character.Inventory == null || actor.Character.Inventory.Count == 0)
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"You don't have that.", cancellationToken);
                    return;
                }

                Item? item = null;

                if (args.Index <= 1)
                {
                    item = actor.Character.Inventory.FirstOrDefault(i => i.Name.Contains(args.Method));
                }
                else
                {
                    var allItems = actor.Character.Inventory.Where(i => i.Name.Contains(args.Method)).ToList();
                    if (allItems.Count > 1)
                    {
                        item = allItems[args.Index - 1];
                    }
                    else
                    {
                        item = allItems.FirstOrDefault();
                    }
                }

                if (item != null)
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"You examine {item.Name}.", cancellationToken);
                    await this.communicator.SendToRoom(actor.Character.Location, actor.Character, null, $"{actor.Character.FirstName.FirstCharToUpper()} examines {item.Name}.", cancellationToken);

                    StringBuilder sb = new ();

                    // Update the player info
                    this.communicator.SendGameUpdate(actor.Character, item.ShortDescription, item.Image).Wait();

                    sb.Append($"{item.LongDescription}<br/>");
                    sb.Append($"{item.Name.FirstCharToUpper()} is of type {Enum.GetName<ItemType>(item.ItemType)?.ToString().ToLower()} and appears to have a durability of {item.Durability.Current}.<br/>");
                    sb.Append($"You value it at approximately {this.random.Next(Math.Max((int)item.Value - (item.Value * 2), 2), Math.Max((int)item.Value + (item.Value * 2), 4))} gold.<br/>");

                    if (item.ItemType == ItemType.Drink)
                    {
                        sb.Append($"{item.Name.FirstCharToUpper()} is a drink container. It has around {item.Drinks?.Current} draughts of {ActionHelper.GetLiquidDescription(item.LiquidType)} inside of it.");
                    }
                    else if (item.ItemType == ItemType.Map)
                    {
                        sb.Append($"<span class='map'><img src='{item.Image}'/></map>");
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

                            if (item.Contains != null && item.Contains.Count > 0)
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
            var helpCommand = args.Method?.ToLower();

            if (!string.IsNullOrWhiteSpace(helpCommand))
            {
                if (helpCommand == "commands")
                {
                    await this.DoCommands(actor, args, cancellationToken);
                }
                else if (helpCommand == "newbie")
                {
                    await this.communicator.SendToPlayer(actor.Connection, "You can type COMMANDS to see a list of all commands. Type help <command> to see what the command does.", cancellationToken);
                }
                else
                {
                    var type = this.GetType();
                    var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
                    MethodInfo? method = null;

                    if (helpCommand is "ne" or "nw" or "south" or "north" or "east" or "west" or "up" or "down" or "se" or "sw")
                    {
                        method = methods.FirstOrDefault(m => m.Name.ToLower() == "domove");
                    }
                    else if (helpCommand is "kill" or "murder")
                    {
                        method = methods.FirstOrDefault(m => m.Name.ToLower() == "docombat");
                    }
                    else
                    {
                        method = methods.FirstOrDefault(m => m.Name.ToLower() == "do" + helpCommand);
                    }

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

        [HelpText("<p>Lists all items in your inventory.</p><ul><li>inventory</li></ul>")]
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

        [SightRequired]
        [HelpText("<p>Locks a lockable container, as long as you have the proper key. See HELP UNLOCK, HELP OPEN.</p><ul><li>lock <em>container</em></li></ul>")]
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
                if (!container.KeyId.HasValue || container.KeyId == 0)
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"{container.Name.FirstCharToUpper()} has no lock.", cancellationToken);
                    return;
                }

                // Check if it's locked.
                if (container.IsClosed && container.IsLocked)
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"It's already locked.", cancellationToken);
                }
                else if (!container.IsClosed)
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"It's still open.", cancellationToken);
                }
                else if (!container.KeyId.HasValue || container.KeyId.Value == 0)
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"You can't lock that.", cancellationToken);
                }
                else if (container.IsClosed && !container.IsLocked)
                {
                    // Do we have a key?
                    var key = actor.Character.Inventory.FirstOrDefault(k => k.ItemId == container.KeyId && k.ItemType == ItemType.Key);
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
                        if (!item.KeyId.HasValue || item.KeyId == 0)
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"{item.Name.FirstCharToUpper()} has no lock.", cancellationToken);
                            return;
                        }

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
                            var key = actor.Character.Inventory.FirstOrDefault(k => k.ItemId == item.KeyId && k.ItemType == ItemType.Key);
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
                        if (!exit.KeyId.HasValue || exit.KeyId == 0)
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"There is no lock.", cancellationToken);
                            return;
                        }

                        if (exit.IsDoor && exit.IsClosed && exit.IsLocked)
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"The {exit.DoorName} is already locked.", cancellationToken);
                        }
                        else if (exit.IsDoor && exit.IsClosed && !exit.IsLocked)
                        {
                            var key = actor.Character.Inventory.FirstOrDefault(k => k.ItemId == exit.KeyId && k.ItemType == ItemType.Key);
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

        [SightRequired]
        [HelpText("<p>Looks at another player, NPC, or item. Look at the sky if you're outside to help determine where you are. See also HELP EXAMINE.</p><ul><li>look <em>player name</em></li><li>look <em>NPC name</em></li><li>look <em>item</em></li><li>look <em>sky</em></li><ul>")]
        private async Task DoLook(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(args.Method))
            {
                if (args.Method.ToLower() == "sky")
                {
                    var room = this.communicator.ResolveRoom(actor.Character.Location);

                    if (room != null && room.Flags != null && room.Flags.Contains(RoomFlags.Indoors))
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"You're unable to see the sky from indoors.", cancellationToken);
                    }
                    else
                    {
                        var area = this.communicator.ResolveArea(actor.Character.Location);

                        if (area != null)
                        {
                            StringBuilder sb = new ();
                            sb.Append($"Looking at the sky, you seem to be in the vicinity of {area.Name}. ");

                            if (Legendary.Engine.Environment.CurrentWeather.ContainsKey(area.AreaId))
                            {
                                var weather = Legendary.Engine.Environment.CurrentWeather[area.AreaId];

                                if (weather != null)
                                {
                                    sb.Append($"{weather.Status} You guess it to be around {weather.Temp} degrees outside.");
                                }
                            }

                            await this.communicator.SendToPlayer(actor.Connection, sb.ToString(), cancellationToken);
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

            bool isGhost = actor.Character.CharacterFlags.Contains(CharacterFlags.Ghost) || actor.Character.IsAffectedBy(nameof(PassDoor));
            bool isFlying = actor.Character.Race == Race.Avian || actor.Character.Race == Race.Faerie || actor.Character.IsAffectedBy(nameof(Fly));

            if (exit != null)
            {
                if (exit.IsDoor && exit.IsClosed && !isGhost)
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"The {exit.DoorName ?? "door"} is closed.", cancellationToken);
                }
                else
                {
                    var newArea = this.world.Areas.FirstOrDefault(a => a.AreaId == exit.ToArea);
                    var newRoom = newArea?.Rooms?.FirstOrDefault(r => r.RoomId == exit.ToRoom);

                    if (newArea != null && newRoom != null)
                    {
                        if (newRoom.Terrain == Terrain.Air && !isFlying && !isGhost)
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"You need to be flying to go there.", cancellationToken);
                            return;
                        }

                        if (newRoom.Terrain == Terrain.Water && !isFlying && !isGhost && actor.Character.Inventory.Any(i => i.ItemType == ItemType.Boat))
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"You need to be flying or have a boat to go there.", cancellationToken);
                            return;
                        }

                        string? dir = Enum.GetName(typeof(Direction), exit.Direction)?.ToLower();
                        var moves = GetTerrainMovementPenalty(newRoom);

                        if ((actor.Character.Movement.Current - moves) < 0)
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"You are too exhausted.", cancellationToken);
                            return;
                        }
                        else
                        {
                            if (actor.Character.Following == null)
                            {
                                if (isGhost)
                                {
                                    await this.communicator.SendToPlayer(actor.Connection, $"You float {dir}.", cancellationToken);
                                }
                                else if (isFlying)
                                {
                                    await this.communicator.SendToPlayer(actor.Connection, $"You fly {dir}.", cancellationToken);
                                }
                                else
                                {
                                    await this.communicator.SendToPlayer(actor.Connection, $"You go {dir}.", cancellationToken);
                                }
                            }

                            // Check if there are followers.
                            if (actor.Character.Followers.Count > 0)
                            {
                                foreach (var follower in actor.Character.Followers)
                                {
                                    // Resolve the follower.
                                    var target = this.communicator.ResolveCharacter(follower);

                                    // Make sure they are still following the actor and they're in the same room.
                                    if (target != null && target.Character.Following == actor.Character.CharacterId && target.Character.Location.InSamePlace(actor.Character.Location))
                                    {
                                        await this.communicator.SendToPlayer(target.Connection, $"You follow {actor.Character.FirstName} {dir}.", cancellationToken);
                                        await this.DoMove(target, args, cancellationToken);
                                    }
                                }
                            }

                            // Put the char in the new room.
                            actor.Character.Location = new KeyValuePair<long, long>(exit.ToArea, exit.ToRoom);

                            // Add the new room to the game map.
                            actor.Character.Metrics.GameMap.AddIfNotExists(newRoom);

                            // Send the leaving message. This HAS to be done after for mob engagement purposes or else the mob will think the player is
                            // still in the room with them.
                            if (actor.Character.IsAffectedBy(nameof(Hide)))
                            {
                                await this.communicator.SendToPlayer(actor.Connection, "You step out of the shadows.", cancellationToken);
                                await this.communicator.SendToRoom(actor.Character.Location, actor.Character, null, $"{actor.Character.FirstName.FirstCharToUpper()} steps out of the shadows.", cancellationToken);
                                actor.Character.AffectedBy.RemoveAll(a => a.Name == nameof(Hide));
                            }

                            if (isGhost)
                            {
                                await this.communicator.SendToRoom(actor.Character, actor.Character.Location, $"{actor.Character.FirstName.FirstCharToUpper()} floats {dir}.", cancellationToken);
                            }
                            else
                            {
                                await this.communicator.SendToRoom(actor.Character, actor.Character.Location, $"{actor.Character.FirstName.FirstCharToUpper()} leaves {dir}.", cancellationToken);
                                await this.communicator.PlaySound(actor.Character, AudioChannel.BackgroundSFX, Sounds.WALK, cancellationToken);
                            }

                            // Track exploration for award purposes.
                            if (actor.Character.Metrics.RoomsExplored.TryGetValue(exit.ToArea, out List<long>? value))
                            {
                                if (!value.Contains(exit.ToRoom))
                                {
                                    actor.Character.Metrics.RoomsExplored[exit.ToArea].Add(exit.ToRoom);
                                    await this.awardProcessor.CheckVoyagerAward(exit.ToArea, actor.Character, cancellationToken);
                                }
                            }
                            else
                            {
                                actor.Character.Metrics.RoomsExplored.Add(exit.ToArea, new List<long>() { exit.ToRoom });
                                await this.awardProcessor.CheckVoyagerAward(exit.ToArea, actor.Character, cancellationToken);
                            }

                            await this.communicator.ShowRoomToPlayer(actor.Character, cancellationToken);

                            if (!actor.Character.IsAffectedBy(nameof(Sneak)))
                            {
                                if (isGhost)
                                {
                                    await this.communicator.SendToRoom(actor.Character, actor.Character.Location, $"{actor.Character.FirstName.FirstCharToUpper()} floats in.", cancellationToken);
                                }
                                else if (isFlying)
                                {
                                    actor.Character.Movement.Current -= 1;
                                    await this.communicator.SendToRoom(actor.Character, actor.Character.Location, $"{actor.Character.FirstName.FirstCharToUpper()} flies in.", cancellationToken);
                                }
                                else
                                {
                                    actor.Character.Movement.Current -= moves;
                                    await this.communicator.SendToRoom(actor.Character, actor.Character.Location, $"{actor.Character.FirstName.FirstCharToUpper()} enters.", cancellationToken);
                                }

                                var mobsWithProgsInRoom = this.communicator.GetMobilesInRoom(actor.Character.Location)?.Where(m => !string.IsNullOrWhiteSpace(m.Program));

                                if (mobsWithProgsInRoom != null)
                                {
                                    // We will prefer mob programs over AI comms.
                                    foreach (var programmedMob in mobsWithProgsInRoom)
                                    {
                                        var program = this.communicator.MIRPProcessor.CreateProgramInstance(programmedMob);

                                        if (program != null && program is MIRP mirp)
                                        {
                                            await mirp.OnPlayerEnter(programmedMob, new Types.MIRPEventArgs() { Mobile = programmedMob, Player = actor.Character }, cancellationToken);
                                        }
                                    }
                                }
                                else
                                {
                                    // See if any AI mobs in the room will communicate with the player.
                                    var commsTask = this.communicator.CheckMobCommunication(actor.Character, actor.Character.Location, $"{actor.Character.FirstName.FirstCharToUpper()} enters the room.", cancellationToken);

                                    // Run this task on a separate, synchronous thread, so we don't block. This is fire and forget.
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                    Task.Run(async () => { await commsTask; }, cancellationToken);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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

        [SightRequired]
        [HelpText("<p>Opens a door or a container. If it is locked, you will need to unlock it first. See HELP UNLOCK, HELP LOCK.</p><ul><li>open <em>container</em></li><li>open <em>direction</em></li></ul>")]
        private async Task DoOpen(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Open what?", cancellationToken);
                return;
            }

            if (actor.Character.CharacterFlags.Contains(CharacterFlags.Ghost))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"You can't do that, you're a ghost.", cancellationToken);
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
                    await this.communicator.SendToPlayer(actor.Connection, $"{container.Name.FirstCharToUpper()} locked.", cancellationToken);
                }
                else if (container.IsClosed)
                {
                    container.IsClosed = false;
                    await this.communicator.SendToPlayer(actor.Connection, $"You open {container.Name}.", cancellationToken);
                    await this.communicator.SendToRoom(actor.Character.Location, actor.Character, null, $"{actor.Character.FirstName} opens {container.Name}.", cancellationToken);
                }
                else
                {
                    container.IsClosed = false;
                    await this.communicator.SendToPlayer(actor.Connection, $"{container.Name.FirstCharToUpper()} is already open.", cancellationToken);
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
                            await this.communicator.SendToPlayer(actor.Connection, $"{item.Name.FirstCharToUpper()} is locked.", cancellationToken);
                        }
                        else if (item.IsClosed)
                        {
                            item.IsClosed = false;
                            await this.communicator.SendToPlayer(actor.Connection, $"You open {item.Name}.", cancellationToken);
                            await this.communicator.SendToRoom(actor.Character.Location, actor.Character, null, $"{actor.Character.FirstName} opens {item.Name}.", cancellationToken);
                        }
                        else
                        {
                            item.IsClosed = false;
                            await this.communicator.SendToPlayer(actor.Connection, $"{item.Name.FirstCharToUpper()} is already open.", cancellationToken);
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
                            await this.communicator.SendToPlayer(actor.Connection, $"You open the {exit.DoorName ?? "door"} {friendlyDirection} you.", cancellationToken);
                            await this.communicator.SendToRoom(actor.Character.Location, actor.Character, null, $"{actor.Character.FirstName.FirstCharToUpper()} opens the {exit.DoorName ?? "door"} {friendlyDirection} you.", cancellationToken);

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

        [HelpText("<p>Sends a message to all available Gods. Do not abuse or spam this channel.</p><ul><li>pray <em>message</em></li></ul>")]
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
                    await this.communicator.SendToChannel(channel, actor.ConnectionId, $"{actor.Character.FirstName.FirstCharToUpper()} prays \"<span class='pray'>{sentence}</span>\"", cancellationToken);
                }
            }
            else
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Pray what?", cancellationToken);
            }
        }

        [HelpText("<p>Saves and exits the game.</p><ul><li>quit</li></ul>")]
        private async Task DoQuit(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            await this.communicator.SaveCharacter(actor);
            await this.communicator.Quit(actor.Connection, actor.Character.FirstName ?? "Someone", cancellationToken);
        }

        [HelpText("<p>Removes one or more items from your person and places the item(s) in your inventory. See also: HELP WEAR.</p><ul><li>remove <em>target</em></li><li>remove <em>all</em></li></ul>")]
        private async Task DoRemove(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            // remove <argument>
            // See if they are wearing the item.
            if (!string.IsNullOrWhiteSpace(args.Method))
            {
                var itemName = args.Method.ToLower();

                if (itemName == "all")
                {
                    Dictionary<WearLocation, Item> itemsToRemove = new ();

                    foreach (var target in actor.Character.Equipment)
                    {
                        // Un-equip each item and put back in inventory.
                        if (!target.Value.ItemFlags.Contains(ItemFlags.Cursed))
                        {
                            itemsToRemove.Add(target.Key, target.Value);
                            actor.Character.Inventory.Add(target.Value);
                            await this.communicator.SendToPlayer(actor.Connection, $"You remove {target.Value.Name}.", cancellationToken);
                            await this.communicator.SendToRoom(actor.Character, actor.Character.Location, $"{actor.Character.FirstName.FirstCharToUpper()} removes {target.Value.Name}.", cancellationToken);
                        }
                        else
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"You can't remove {target.Value.Name}.", cancellationToken);
                        }
                    }

                    foreach (var key in itemsToRemove.Keys)
                    {
                        actor.Character.Equipment.Remove(key);
                    }
                }
                else
                {
                    var target = actor.Character.Equipment.FirstOrDefault(i => i.Value.Name.ToLower().Contains(itemName.ToLower()));

                    if (target.Value != null)
                    {
                        // Un-equip the item and put back in inventory.
                        if (!target.Value.ItemFlags.Contains(ItemFlags.Cursed))
                        {
                            actor.Character.Inventory.Add(target.Value);
                            actor.Character.Equipment.Remove(target.Key);
                            await this.communicator.SendToPlayer(actor.Connection, $"You remove {target.Value.Name}.", cancellationToken);
                            await this.communicator.SendToRoom(actor.Character, actor.Character.Location, $"{actor.Character.FirstName.FirstCharToUpper()} removes {target.Value.Name}.", cancellationToken);
                        }
                        else
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"You can't remove {target.Value.Name}.", cancellationToken);
                        }
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

        [HelpText("<p>Rests. Your player will recover mana, health, and movement more quickly when resting. See also: HELP SLEEP, HELP WAKE.</p><ul><li>rest</li></ul>")]
        private async Task DoRest(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (actor.Character.CharacterFlags.Contains(CharacterFlags.Resting))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"You are already resting.", cancellationToken);
            }
            else
            {
                await this.communicator.SendToPlayer(actor.Connection, $"You kick back and rest.", cancellationToken);
                await this.communicator.SendToRoom(actor.Character, actor.Character.Location, $"{actor.Character.FirstName.FirstCharToUpper()} kicks back and rests.", cancellationToken);
                actor.Character.CharacterFlags.AddIfNotExists(CharacterFlags.Resting);
            }
        }

        [SightRequired]
        [HelpText("<p>Sacrifices an item to your deity in return for some divine favor. Not all items can be sacrificed.</p><ul><li>sacrifice <em>target</em></li><li>sacrifice <em>all</em></li></ul>")]
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

        [HelpText("<p>Saves all changes to your player. Note that the game autosaves every 30 seconds, and when you exit.</p><ul><li>save</li></ul>")]
        private async Task DoSave(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            await this.communicator.SaveCharacter(actor);
            await this.communicator.SendToPlayer(actor.Connection, $"Character saved.", cancellationToken);
        }

        [HelpText("<p>Sends a message to everyone in the same room as your player. See also: HELP TELL, HELP YELL.</p><ul><li>say <em>message</em></li></ul>")]
        private async Task DoSay(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            var sentence = args.Method;

            if (string.IsNullOrWhiteSpace(sentence))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"What do you want to say?", cancellationToken);
            }
            else
            {
                if (actor.Character.IsAffectedBy(nameof(Silence)))
                {
                    await this.communicator.SendToPlayer(actor.Connection, "You can't speak!", cancellationToken);
                    return;
                }

                var speakingLang = SkillHelper.ResolveSkill(actor.Character.Speaking, this.communicator, this.random, this.world, this.logger, this.combat);

                await this.communicator.SendToPlayer(actor.Connection, $"You say (in {speakingLang?.Name}): \"<span class='say'>{sentence}</span>\"", cancellationToken);

                // Check if the language improves each time it's used.
                if (speakingLang != null)
                {
                    await speakingLang.CheckImprove(actor.Character, cancellationToken);
                }

                var garbled = this.communicator.LanguageGenerator.BuildSentence(sentence);
                var skillRoll = this.random.Next(0, 99);

                var players = this.communicator.GetPlayersInRoom(actor.Character, actor.Character.Location);

                if (players != null)
                {
                    foreach (var player in players)
                    {
                        if (player.IsAffectedBy(nameof(Sleep)) || player.CharacterFlags.Contains(CharacterFlags.Sleeping))
                        {
                            continue;
                        }
                        else
                        {
                            if (PlayerHelper.CanPlayerSeePlayer(this.environment, this.communicator, player, actor.Character))
                            {
                                if (player.HasSkill(speakingLang?.Name) && player.GetSkillProficiency(speakingLang?.Name)?.Proficiency >= skillRoll)
                                {
                                    await this.communicator.SendToPlayer(player, $"{actor.Character.FirstName.FirstCharToUpper()} says (in {speakingLang?.Name}): \"<span class='say'>{sentence}</span>\"", cancellationToken);
                                }
                                else
                                {
                                    await this.communicator.SendToPlayer(player, $"{actor.Character.FirstName.FirstCharToUpper()} says (in {speakingLang?.Name}): \"<span class='say'><span class='{speakingLang?.Name?.Replace(" ", string.Empty)}'>{garbled}</span></span>\"", cancellationToken);
                                }
                            }
                            else
                            {
                                if (player.HasSkill(speakingLang?.Name) && player.GetSkillProficiency(speakingLang?.Name)?.Proficiency >= skillRoll)
                                {
                                    await this.communicator.SendToPlayer(player, $"Someone says (in {speakingLang?.Name}): \"<span class='say'>{sentence}</span>\"", cancellationToken);
                                }
                                else
                                {
                                    await this.communicator.SendToPlayer(player, $"Someone says (in {speakingLang?.Name}): \"<span class='say'><span class='{speakingLang?.Name?.Replace(" ", string.Empty)}'>{garbled}</span></span>\"", cancellationToken);
                                }
                            }
                        }
                    }
                }

                // See if any AI mobs in the room will communicate with the player. Will be added as a memory.
                var commsTask = this.communicator.CheckMobCommunication(actor.Character, actor.Character.Location, $"{actor.Character.FirstName.FirstCharToUpper()} said to you '{sentence}'.", cancellationToken);

                // Run this task on a separate, synchronous thread, so we don't block. This is fire and forget.
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(async () => { await commsTask; }, cancellationToken);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        [HelpText("<p>Displays your player's score card.</p><ul><li>score</li></ul>")]
        private async Task DoScore(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            await this.ShowPlayerScore(actor, cancellationToken);
        }

        [SightRequired]
        [HelpText("<p>Scans in all directions for NPCs or players.</p><ul><li>scan</li></ul>")]
        private async Task DoScan(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            var room = this.communicator.ResolveRoom(actor.Character.Location);

            if (room == null)
            {
                return;
            }

            await this.communicator.SendToPlayer(actor.Connection, $"You scan in all directions.", cancellationToken);
            await this.communicator.SendToRoom(actor.Character, actor.Character.Location, $"{actor.Character.FirstName.FirstCharToUpper()} scans all around.", cancellationToken);

            StringBuilder sb = new ();

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

                if (players == null && mobs == null)
                {
                    sb.Append($"<span class='scan'>Nothing.</span>");
                }
            }

            await this.communicator.SendToPlayer(actor.Connection, sb.ToString(), cancellationToken);
        }

        [HelpText("<p>Displays all current skill groups, skills, and skill progressions. Can also be used to see skills within a skill tree group.</p><ul><li>skills</li><li>skills martial group II</li></ul>")]
        private async Task DoSkills(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(args.Method) && !string.IsNullOrWhiteSpace(args.Target))
            {
                var skills = TreeHelper.GetSkillsInGroup(args.Method, args.Target, this.communicator, this.random, this.world, this.logger, this.combat);
                await this.communicator.SendToPlayer(actor.Connection, skills, cancellationToken);
            }
            else
            {
                var builder = new StringBuilder();

                var engine = Assembly.Load("Legendary.Engine");

                var skillTrees = engine.GetTypes().Where(t => t.Namespace == "Legendary.Engine.Models.SkillTrees");

                foreach (var tree in skillTrees)
                {
                    var treeInstance = Activator.CreateInstance(tree, this.communicator, this.random, this.world, this.logger, this.combat);

                    // If there are no skills in this instance, don't render it.
                    if (treeInstance != null && treeInstance is IActionTree instance)
                    {
                        var groupProps = tree.GetProperties();

                        var skillGroup1 = groupProps.FirstOrDefault(g => g.Name == $"Group1");
                        var skillGroup2 = groupProps.FirstOrDefault(g => g.Name == $"Group2");
                        var skillGroup3 = groupProps.FirstOrDefault(g => g.Name == $"Group3");
                        var skillGroup4 = groupProps.FirstOrDefault(g => g.Name == $"Group4");
                        var skillGroup5 = groupProps.FirstOrDefault(g => g.Name == $"Group5");

                        var skillGroup1Obj = skillGroup1?.GetValue(treeInstance);
                        var skillGroup2Obj = skillGroup2?.GetValue(treeInstance);
                        var skillGroup3Obj = skillGroup3?.GetValue(treeInstance);
                        var skillGroup4Obj = skillGroup4?.GetValue(treeInstance);
                        var skillGroup5Obj = skillGroup5?.GetValue(treeInstance);

                        var skillsGroup1 = skillGroup1Obj != null ? (List<IAction>)skillGroup1Obj : null;
                        var skillsGroup2 = skillGroup2Obj != null ? (List<IAction>)skillGroup2Obj : null;
                        var skillsGroup3 = skillGroup3Obj != null ? (List<IAction>)skillGroup3Obj : null;
                        var skillsGroup4 = skillGroup4Obj != null ? (List<IAction>)skillGroup4Obj : null;
                        var skillsGroup5 = skillGroup5Obj != null ? (List<IAction>)skillGroup5Obj : null;

                        var sumSkills = skillsGroup1?.Count + skillsGroup2?.Count + skillsGroup3?.Count + skillsGroup4?.Count + skillsGroup5?.Count;

                        // We have at least 1 skill in one of the groups, so render the tree category
                        if (sumSkills != null && sumSkills > 0)
                        {
                            builder.Append("<table class=\"tree-table\">");

                            // We have skills in this group, so render the group header.
                            builder.Append($"<tr><td colspan=\"10\" class=\"tree-header\">{instance.Name}</td></tr>");

                            builder.Append("<tr>");

                            // Build the whole header row.
                            for (var x = 1; x <= 5; x++)
                            {
                                builder.Append($"<td class=\"group-header\"><div class='ribbon-row'><div class='ribbon-block'><h1><span>{instance.Name} {x.ToRomanNumeral()}</span></h1></div></div></td>");
                            }

                            builder.Append("</tr>");

                            builder.Append("<tr>");

                            // Loop over each group and render the skills the player has.
                            for (var x = 1; x <= 5; x++)
                            {
                                var skillGroup = groupProps.FirstOrDefault(g => g.Name == $"Group{x}");

                                if (skillGroup != null)
                                {
                                    builder.Append("<td class=\"group-content\"><ul>");

                                    var skills = skillGroup.GetValue(treeInstance);

                                    if (skills != null)
                                    {
                                        var group = (List<IAction>)skills;

                                        foreach (var action in group.OrderBy(g => g.Name))
                                        {
                                            if (actor.Character.HasSkill(action.Name.ToLower()))
                                            {
                                                var proficiency = actor.Character.GetSkillProficiency(action.Name.ToLower());
                                                if (proficiency != null)
                                                {
                                                    if (proficiency.Proficiency > 1)
                                                    {
                                                        builder.Append($"<li>{proficiency.SkillName} <b>{proficiency.Proficiency}%</b> <progress class=\"skillprogress\" max=\"100\" value=\"{proficiency.Progress}\"></progress></li>");
                                                    }
                                                    else
                                                    {
                                                        builder.Append($"<li class='unlearned'>{proficiency.SkillName} <b>{proficiency.Proficiency}%</b> <progress class=\"skillprogress\" max=\"100\" value=\"{proficiency.Progress}\"></progress></li>");
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    builder.Append("</ul></td>");
                                }
                            }

                            builder.Append("</tr>");
                            builder.Append("</table>");
                        }
                    }
                }

                await this.communicator.SendToPlayer(actor.Connection, builder.ToString(), cancellationToken);
            }
        }

        [HelpText("<p>Sleeps. Your player will recover mana, health, and movement more quickly when sleeping, but will be unable to react to the environment around them. See also: HELP REST, HELP WAKE.</p><ul><li>rest</li></ul>")]
        private async Task DoSleep(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (actor.Character.CharacterFlags.Contains(CharacterFlags.Sleeping))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"You are already sleeping.", cancellationToken);
            }
            else
            {
                await this.communicator.SendToPlayer(actor.Connection, $"You go to sleep.", cancellationToken);
                await this.communicator.SendToRoom(actor.Character, actor.Character.Location, $"{actor.Character.FirstName.FirstCharToUpper()} goes to sleep.", cancellationToken);
                actor.Character.CharacterFlags.AddIfNotExists(CharacterFlags.Sleeping);
            }
        }

        [HelpText("<p>Lets your player perform a custom action in which you also provide a SAY command. See also: HELP EMOTES</p><ul><li>smote <em>smote nods and says Hello</em></li></ul>")]
        private async Task DoSmote(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Smote what?", cancellationToken);
            }
            else
            {
                var sentence = string.Join(' ', new string?[2] { args.Method, args.Target }).Trim();
                if (!string.IsNullOrWhiteSpace(sentence))
                {
                    if (sentence.Contains("says"))
                    {
                        // [smote] [nods and says Hello there]
                        var chunks = sentence.Split("says", StringSplitOptions.RemoveEmptyEntries);

                        // [nods and] [Hello there]
                        var say = $"says \"<span class='say'>{chunks[1].Trim()}</span>\"".Trim();

                        sentence = $"{chunks[0].Replace("smote", string.Empty)} {say}";

                        await this.communicator.SendToPlayer(actor.Connection, $"{actor.Character.FirstName.FirstCharToUpper()} {sentence.Trim()}", cancellationToken);

                        await this.communicator.SendToRoom(actor.Character, actor.Character.Location, sentence, cancellationToken);

                        var players = this.communicator.GetPlayersInRoom(actor.Character, actor.Character.Location);

                        if (players != null)
                        {
                            foreach (var player in players)
                            {
                                if (player != actor.Character)
                                {
                                    if (PlayerHelper.CanPlayerSeePlayer(this.environment, this.communicator, player, actor.Character) && player.CharacterId != actor.Character.CharacterId)
                                    {
                                        await this.communicator.SendToPlayer(actor.Character, $"{actor.Character.FirstName.FirstCharToUpper()} {sentence.Trim()}", cancellationToken);
                                    }
                                }
                            }
                        }

                        // See if any AI mobs in the room will communicate with the player.
                        var commsTask = this.communicator.CheckMobCommunication(actor.Character, actor.Character.Location, $"{actor.Character.FirstName.FirstCharToUpper()} {sentence.Trim()}.", cancellationToken);

                        // Run this task on a separate, synchronous thread, so we don't block. This is fire and forget.
    #pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        Task.Run(async () => { await commsTask; }, cancellationToken);
    #pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
                    else
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"Your SMOTE command should contain a SAYS within it.", cancellationToken);
                    }
                }
            }
        }

        [HelpText("<p>Displays all current spell groups, spells, and spell progressions. Can also be used to see spells within a spell tree group.</p><ul><li>spells</li><li>spells conjuring group II</li></ul>")]
        private async Task DoSpells(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(args.Method) && !string.IsNullOrWhiteSpace(args.Target))
            {
                var spells = TreeHelper.GetSpellsInGroup(args.Method, args.Target, this.communicator, this.random, this.world, this.logger, this.combat);
                await this.communicator.SendToPlayer(actor.Connection, spells, cancellationToken);
            }
            else
            {
                var builder = new StringBuilder();

                var engine = Assembly.Load("Legendary.Engine");

                var spellTrees = engine.GetTypes().Where(t => t.Namespace == "Legendary.Engine.Models.SpellTrees");

                foreach (var tree in spellTrees)
                {
                    var treeInstance = Activator.CreateInstance(tree, this.communicator, this.random, this.world, this.logger, this.combat);

                    // If there are no spells in this instance, don't render it.
                    if (treeInstance != null && treeInstance is IActionTree instance)
                    {
                        var groupProps = tree.GetProperties();

                        var spellGroup1 = groupProps.FirstOrDefault(g => g.Name == $"Group1");
                        var spellGroup2 = groupProps.FirstOrDefault(g => g.Name == $"Group2");
                        var spellGroup3 = groupProps.FirstOrDefault(g => g.Name == $"Group3");
                        var spellGroup4 = groupProps.FirstOrDefault(g => g.Name == $"Group4");
                        var spellGroup5 = groupProps.FirstOrDefault(g => g.Name == $"Group5");

                        var spellGroup1Obj = spellGroup1?.GetValue(treeInstance);
                        var spellGroup2Obj = spellGroup2?.GetValue(treeInstance);
                        var spellGroup3Obj = spellGroup3?.GetValue(treeInstance);
                        var spellGroup4Obj = spellGroup4?.GetValue(treeInstance);
                        var spellGroup5Obj = spellGroup5?.GetValue(treeInstance);

                        var spellsGroup1 = spellGroup1Obj != null ? (List<IAction>)spellGroup1Obj : null;
                        var spellsGroup2 = spellGroup2Obj != null ? (List<IAction>)spellGroup2Obj : null;
                        var spellsGroup3 = spellGroup3Obj != null ? (List<IAction>)spellGroup3Obj : null;
                        var spellsGroup4 = spellGroup4Obj != null ? (List<IAction>)spellGroup4Obj : null;
                        var spellsGroup5 = spellGroup5Obj != null ? (List<IAction>)spellGroup5Obj : null;

                        var sumSpells = spellsGroup1?.Count + spellsGroup2?.Count + spellsGroup3?.Count + spellsGroup4?.Count + spellsGroup5?.Count;

                        // We have at least 1 spell in one of the groups, so render the tree category
                        if (sumSpells != null && sumSpells > 0)
                        {
                            builder.Append("<table class=\"tree-table\">");

                            // We have spells in this group, so render the group header.
                            builder.Append($"<tr><td colspan=\"10\" class=\"tree-header\">{instance.Name}</td></tr>");

                            builder.Append("<tr>");

                            // Build the whole header row.
                            for (var x = 1; x <= 5; x++)
                            {
                                builder.Append($"<td class=\"group-header\"><div class='ribbon-row'><div class='ribbon-block'><h1><span>{instance.Name} {x.ToRomanNumeral()}</span></h1></div></div></td>");
                            }

                            builder.Append("</tr>");

                            builder.Append("<tr>");

                            // Loop over each group and render the spells the player has.
                            for (var x = 1; x <= 5; x++)
                            {
                                var spellGroup = groupProps.FirstOrDefault(g => g.Name == $"Group{x}");

                                if (spellGroup != null)
                                {
                                    builder.Append("<td class=\"group-content\"><ul>");

                                    var spells = spellGroup.GetValue(treeInstance);

                                    if (spells != null)
                                    {
                                        var group = (List<IAction>)spells;

                                        foreach (var action in group.OrderBy(g => g.Name))
                                        {
                                            if (actor.Character.HasSpell(action.Name.ToLower()))
                                            {
                                                var proficiency = actor.Character.GetSpellProficiency(action.Name.ToLower());
                                                if (proficiency != null)
                                                {
                                                    if (proficiency.Proficiency > 1)
                                                    {
                                                        builder.Append($"<li>{proficiency.SpellName} (<i>{action.ManaCost} mana</i>) <b>{proficiency.Proficiency}%</b> <progress class=\"skillprogress\" max=\"100\" value=\"{proficiency.Progress}\"></progress></li>");
                                                    }
                                                    else
                                                    {
                                                        builder.Append($"<li class='unlearned'>{proficiency.SpellName} (<i>{action.ManaCost} mana</i>) <b>{proficiency.Proficiency}%</b> <progress class=\"skillprogress\" max=\"100\" value=\"{proficiency.Progress}\"></progress></li>");
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    builder.Append("</ul></td>");
                                }
                            }

                            builder.Append("</tr>");
                            builder.Append("</table>");
                        }
                    }
                }

                await this.communicator.SendToPlayer(actor.Connection, builder.ToString(), cancellationToken);
            }
        }

        [HelpText("<p>Subscribes to a communication channel.</p><ul><li>subscribe <em>channel</em></li></ul>")]
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

        [SightRequired]
        [HelpText("<p>Tells a target player a message anywhere in the world. If player is sleeping, or ignoring you, the message will not go through. See also: HELP REPLY, HELP SAY, HELP YELL.</p><ul><li>tell <em>target</em> <em>message</em></li></ul>")]
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

        [HelpText("<p>Replies to the last player who sent you a tell. If player is sleeping, or ignoring you, the message will not go through. See also: HELP TELL.</p><ul><li>reply <em>message</em></li></ul>")]
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

        [SightRequired]
        [HelpText("<p>Displays the current date and time of the world.</p><ul><li>time</li></ul>")]
        private async Task DoTime(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            var metrics = this.world.GameMetrics;

            if (metrics != null)
            {
                var timeInfo = DateTimeHelper.GetDate(metrics.CurrentDay, metrics.CurrentMonth, metrics.CurrentYear, metrics.CurrentHour, DateTime.Now.Minute, DateTime.Now.Second);

                await this.communicator.SendToPlayer(actor.Connection, $"{timeInfo}", cancellationToken);
            }
        }

        [SightRequired]
        [HelpText("<p>Unlocks a locked container, as long as you have the proper key. See HELP LOCK, HELP OPEN.</p><ul><li>unlock <em>container</em></li></ul>")]
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
                if (!container.KeyId.HasValue || container.KeyId == 0)
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"{container.Name.FirstCharToUpper()} has no lock.", cancellationToken);
                    return;
                }

                // Check if it's locked.
                if (container.IsClosed && !container.IsLocked)
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"It's already unlocked.", cancellationToken);
                }
                else if (!container.IsClosed)
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"It's already open.", cancellationToken);
                }
                else if (!container.KeyId.HasValue || container.KeyId.Value == 0)
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"You can't unlock that.", cancellationToken);
                }
                else if (container.IsClosed && container.IsLocked)
                {
                    // Do we have a key?
                    var key = actor.Character.Inventory.FirstOrDefault(k => k.ItemId == container.KeyId && k.ItemType == ItemType.Key);
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
                            if (!item.KeyId.HasValue || item.KeyId == 0)
                            {
                                await this.communicator.SendToPlayer(actor.Connection, $"{item.Name.FirstCharToUpper()} has no lock.", cancellationToken);
                                return;
                            }

                            // Do we have a key?
                            var key = actor.Character.Inventory.FirstOrDefault(k => k.ItemId == item.KeyId && k.ItemType == ItemType.Key);
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

                    if (room != null && exit != null)
                    {
                        if (exit.IsDoor && exit.IsClosed && !exit.IsLocked)
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"The {exit.DoorName} is already unlocked.", cancellationToken);
                        }
                        else if (exit.IsDoor && exit.IsClosed && exit.IsLocked)
                        {
                            if (!exit.KeyId.HasValue || exit.KeyId == 0)
                            {
                                await this.communicator.SendToPlayer(actor.Connection, $"There is no key to this door.", cancellationToken);
                                return;
                            }

                            var key = actor.Character.Inventory.FirstOrDefault(k => k.ItemId == exit.KeyId && k.ItemType == ItemType.Key);
                            if (key == null)
                            {
                                await this.communicator.SendToPlayer(actor.Connection, $"You lack the key.", cancellationToken);
                            }
                            else
                            {
                                // Need to lock the door on BOTH sides
                                var oppRoom = this.communicator.ResolveRoom(new KeyValuePair<long, long>(exit.ToArea, exit.ToRoom));

                                var exitToThisRoom = oppRoom?.Exits.FirstOrDefault(e => e.ToArea == room?.AreaId && e.ToRoom == room?.RoomId);

                                if (oppRoom != null && exitToThisRoom != null)
                                {
                                    exitToThisRoom.IsLocked = false;
                                }

                                exit.IsLocked = false;

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

        [HelpText("<p>Unsubscribes from a communication channel.</p><ul><li>unsubscribe <em>channel</em></li></ul>")]
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

        [SightRequired]
        [HelpText("<p>Displays all players who are online, who are visible to you.</p><ul><li>who</li></ul>")]
        private async Task DoWho(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (Communicator.Users != null)
            {
                var sb = new StringBuilder();
                sb.Append("<h4>Online Players</h4>");

                foreach (KeyValuePair<string, UserData> player in Communicator.Users)
                {
                    if (PlayerHelper.CanPlayerSeePlayer(this.environment, this.communicator, actor.Character, player.Value.Character))
                    {
                        var isPK = PlayerHelper.IsInPK(actor.Character, player.Value.Character) ? " &lt;<i class='pk'>PK</i>&gt; " : string.Empty;

                        sb.Append($"<span class='who'>[{player.Value.Character.Level}]{isPK}{player.Value.Character.FirstName}");

                        if (!string.IsNullOrWhiteSpace(player.Value.Character.LastName))
                        {
                            sb.Append($" {player.Value.Character.LastName}");
                        }

                        if (!string.IsNullOrWhiteSpace(player.Value.Character.Title))
                        {
                            sb.Append($" {player.Value.Character.Title}");
                        }

                        sb.Append("</span>");
                    }
                }

                await this.communicator.SendToPlayer(actor.Connection, sb.ToString(), cancellationToken);

                await this.communicator.SendToPlayer(actor.Connection, $"There are {Communicator.Users?.Count} players in Mystra.", cancellationToken);
            }
        }

        [SightRequired]
        [HelpText("<p>Displays all players in your area who are visible to you. Provide a target to see where they are individually.</p><ul><li>where</li><li>where <em>target</em></li></ul>")]
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

                List<Character> playersInArea = new ();

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

        [HelpText("<p>Wake from resting or sleeping. See also: HELP SLEEP, HELP REST.</p><ul><li>who</li></ul>")]
        private async Task DoWake(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (actor.Character.IsAffectedBy(nameof(Sleep)))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"You can't wake up!", cancellationToken);
            }
            else if (actor.Character.CharacterFlags.Contains(CharacterFlags.Resting) || actor.Character.CharacterFlags.Contains(CharacterFlags.Sleeping))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"You wake and and stand up.", cancellationToken);
                await this.communicator.SendToRoom(actor.Character, actor.Character.Location, $"{actor.Character.FirstName.FirstCharToUpper()} wakes and stands up.", cancellationToken);
                actor.Character.CharacterFlags.RemoveIfExists(CharacterFlags.Resting);
                actor.Character.CharacterFlags.RemoveIfExists(CharacterFlags.Sleeping);
            }
            else
            {
                await this.communicator.SendToPlayer(actor.Connection, $"You are already awake and standing.", cancellationToken);
            }
        }

        [HelpText("<p>Wear an item or items from your inventory. See also: REMOVE.</p><ul><li>wear <em>target</em></li><li>wear <em>all</em></li></ul>")]
        private async Task DoWear(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            // wear <argument>
            // See if they have it in their inventory.
            if (!string.IsNullOrWhiteSpace(args.Method))
            {
                var itemName = args.Method.ToLower();

                if (itemName == "all")
                {
                    var itemsToEquip = actor.Character.Inventory.Where(i => i.ItemType != ItemType.Weapon).ToList();
                    foreach (var item in itemsToEquip)
                    {
                        if (item.ItemType != ItemType.Weapon)
                        {
                            await this.EquipItem(actor, "wear", item, false, cancellationToken);
                        }
                    }

                    await this.communicator.SaveCharacter(actor);
                }
                else
                {
                    var target = actor.Character.Inventory.FirstOrDefault(i => i.Name.ToLower().Contains(itemName.ToLower()));

                    if (target != null)
                    {
                        if (target.WearLocation.Contains(WearLocation.None))
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"You can't wear {target.Name}.", cancellationToken);
                        }
                        else
                        {
                            await this.EquipItem(actor, "wear", target, true, cancellationToken);
                            await this.communicator.SaveCharacter(actor);
                        }
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

        [SightRequired]
        [HelpText("<p>If the item is a weapon, your player will wield it. See also: WEAR.</p><ul><li>wield <em>target</em></li></ul>")]
        private async Task DoWield(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            // wield <argument>
            // See if they have it in their inventory.
            if (!string.IsNullOrWhiteSpace(args.Method))
            {
                var itemName = args.Method.ToLower();

                var target = actor.Character.Inventory.FirstOrDefault(i => i.Name.ToLower().Contains(itemName.ToLower()));

                if (target != null)
                {
                    if (target.WearLocation.Contains(WearLocation.Wielded))
                    {
                        await this.EquipItem(actor, "wield", target, true, cancellationToken);
                    }
                    else
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"You can't wield that.", cancellationToken);
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

        [HelpText("<p>Yells loudly so that everyone who is in your current area can hear you. See also: HELP SAY, HELP TELL.</p><ul><li>yell <em>message</em></li></ul>")]
        private async Task DoYell(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            var sentence = args.Method;

            if (!string.IsNullOrWhiteSpace(sentence))
            {
                if (actor.Character.IsAffectedBy(nameof(Silence)))
                {
                    await this.communicator.SendToPlayer(actor.Connection, "You can't speak!", cancellationToken);
                    return;
                }

                var speakingLang = SkillHelper.ResolveSkill("Common", this.communicator, this.random, this.world, this.logger, this.combat);
                var speaking = actor.Character.Speaking ?? speakingLang?.Name;
                sentence = char.ToUpper(sentence[0]) + sentence[1..];
                await this.communicator.SendToPlayer(actor.Connection, $"You yell (in {speaking}): \"<span class='yell'>{sentence}!</b>\"", cancellationToken);

                // Check if the language improves each time it's used.
                if (speakingLang != null)
                {
                    await speakingLang.CheckImprove(actor.Character, cancellationToken);
                }

                var garbled = this.communicator.LanguageGenerator.BuildSentence(sentence);
                var skillRoll = this.random.Next(0, 99);

                var players = this.communicator.GetPlayersInArea(actor.Character, actor.Character.Location);

                if (players != null)
                {
                    foreach (var player in players)
                    {
                        if (player.IsAffectedBy(nameof(Sleep)) || player.CharacterFlags.Contains(CharacterFlags.Sleeping))
                        {
                            continue;
                        }
                        else
                        {
                            if (PlayerHelper.CanPlayerSeePlayer(this.environment, this.communicator, player, actor.Character))
                            {
                                if (player.HasSkill(speaking) && player.GetSkillProficiency(speaking)?.Proficiency >= skillRoll)
                                {
                                    await this.communicator.SendToPlayer(player, $"{actor.Character.FirstName.FirstCharToUpper()} yells (in {speaking}): \"<span class='yell'>{sentence}!</span>\"", cancellationToken);
                                }
                                else
                                {
                                    await this.communicator.SendToPlayer(player, $"{actor.Character.FirstName.FirstCharToUpper()} yells (in {speaking}): \"<span class='yell'><span class='{speaking?.Replace(" ", string.Empty)}'>{garbled}!</span></span>\"", cancellationToken);
                                }
                            }
                            else
                            {
                                if (player.HasSkill(speaking) && player.GetSkillProficiency(speaking)?.Proficiency >= skillRoll)
                                {
                                    await this.communicator.SendToPlayer(player, $"Someone yells (in {speaking}): \"<span class='yell'>{sentence}!</span>\"", cancellationToken);
                                }
                                else
                                {
                                    await this.communicator.SendToPlayer(player, $"Someone yells (in {speaking}): \"<span class='yell'><span class='{speaking?.Replace(" ", string.Empty)}'>{garbled}!</span></span>\"", cancellationToken);
                                }
                            }
                        }
                    }
                }

                // See if any AI mobs in the room will communicate with the player.
                var commsTask = this.communicator.CheckMobCommunication(actor.Character, actor.Character.Location, $"{actor.Character.FirstName.FirstCharToUpper()} yells loudly, '{sentence}'.", cancellationToken);

                // Run this task on a separate, synchronous thread, so we don't block. This is fire and forget.
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(async () => { await commsTask; }, cancellationToken);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            else
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Yell what?", cancellationToken);
            }
        }

        /// <summary>
        /// Equips an item on a character, removing it from inventory and adding it to equipment. If the player is already wearing an item in that location,
        /// it replaces it.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <param name="verb">The verb.</param>
        /// <param name="item">The item.</param>
        /// <param name="removeIfEquipped">If equipped, will remove the item.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        private async Task EquipItem(UserData actor, string verb, Item item, bool removeIfEquipped, CancellationToken cancellationToken)
        {
            if (item.WearLocation.Contains(WearLocation.InventoryOnly) || item.WearLocation.Contains(WearLocation.None))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"You can't {verb} {item.Name}.", cancellationToken);
                return;
            }

            // Get the locations that are already occupied by this item.
            var usedLocations = actor.Character.Equipment.Where(e => item.WearLocation.Contains(e.Key)).ToList();

            // Easy, no used locations, so this can drop into the first one.
            if (usedLocations.Count == 0)
            {
                var wearDescription = item.WearLocation.First().ToWearDescription();

                actor.Character.Inventory.Remove(item);
                actor.Character.Equipment.Add(item.WearLocation.First(), item);

                await this.communicator.SendToPlayer(actor.Connection, $"You {verb} {item.Name} {wearDescription?.WearAction}.", cancellationToken);
                await this.communicator.SendToRoom(actor.Character, actor.Character.Location, $"{actor.Character.FirstName.FirstCharToUpper()} {verb}s {item.Name}.", cancellationToken);

                if (!string.IsNullOrWhiteSpace(item.Program))
                {
                    var program = this.communicator.MIRPProcessor.CreateProgramInstance(item);

                    if (program != null && program is MIRP mirp)
                    {
                        if (verb.ToLower() == "wield")
                        {
                            await mirp.OnItemWield(item, new Types.MIRPEventArgs() { Item = item, Player = actor.Character }, cancellationToken);
                        }
                        else
                        {
                            await mirp.OnItemWear(item, new Types.MIRPEventArgs() { Item = item, Player = actor.Character }, cancellationToken);
                        }
                    }
                }
            }
            else if (usedLocations.Count > 0 && usedLocations.Count < item.WearLocation.Count)
            {
                // This means there is at least one empty location we can put this item, so get the first one. Should never be an empty sequence.
                var newLocation = item.WearLocation.First(w => !actor.Character.Equipment.ContainsKey(w));

                var wearDescription = newLocation.ToWearDescription();

                actor.Character.Inventory.Remove(item);
                actor.Character.Equipment.Add(newLocation, item);

                await this.communicator.SendToPlayer(actor.Connection, $"You {verb} {item.Name} {wearDescription?.WearAction}.", cancellationToken);
                await this.communicator.SendToRoom(actor.Character, actor.Character.Location, $"{actor.Character.FirstName.FirstCharToUpper()} {verb}s {item.Name}.", cancellationToken);

                var program = this.communicator.MIRPProcessor.CreateProgramInstance(item);

                if (program != null && program is MIRP mirp)
                {
                    if (verb.ToLower() == "wield")
                    {
                        await mirp.OnItemWield(item, new Types.MIRPEventArgs() { Item = item, Player = actor.Character }, cancellationToken);
                    }
                    else
                    {
                        await mirp.OnItemWear(item, new Types.MIRPEventArgs() { Item = item, Player = actor.Character }, cancellationToken);
                    }
                }
            }
            else
            {
                // All locations are used, so we need to remove one thing before we wear this.
                if (removeIfEquipped)
                {
                    if (item.ItemFlags.Contains(ItemFlags.Cursed))
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"You can't remove {item.Name}!", cancellationToken);
                    }
                    else
                    {
                        var firstUsedLocation = usedLocations.First();

                        var wearDescription = firstUsedLocation.Key.ToWearDescription();

                        await this.communicator.SendToPlayer(actor.Connection, $"You remove {firstUsedLocation.Value.Name} {wearDescription?.RemoveAction}.", cancellationToken);
                        await this.communicator.SendToRoom(actor.Character, actor.Character.Location, $"{actor.Character.FirstName.FirstCharToUpper()} stops using {firstUsedLocation.Value.Name}.", cancellationToken);

                        actor.Character.Equipment.Remove(firstUsedLocation.Key);
                        actor.Character.Inventory.Add(firstUsedLocation.Value);

                        var program1 = this.communicator.MIRPProcessor.CreateProgramInstance(item);

                        if (program1 != null && program1 is MIRP mirp1)
                        {
                            if (verb.ToLower() == "wield")
                            {
                                await mirp1.OnItemRemove(item, new Types.MIRPEventArgs() { Item = item, Player = actor.Character }, cancellationToken);
                            }
                            else
                            {
                                await mirp1.OnItemRemove(item, new Types.MIRPEventArgs() { Item = item, Player = actor.Character }, cancellationToken);
                            }
                        }

                        // Add to the equipment.
                        actor.Character.Equipment.Add(firstUsedLocation.Key, item);
                        actor.Character.Inventory.Remove(item);

                        await this.communicator.SendToPlayer(actor.Connection, $"You {verb} {item.Name} {wearDescription?.WearAction}.", cancellationToken);
                        await this.communicator.SendToRoom(actor.Character, actor.Character.Location, $"{actor.Character.FirstName.FirstCharToUpper()} {verb}s {item.Name}.", cancellationToken);

                        var program2 = this.communicator.MIRPProcessor.CreateProgramInstance(item);

                        if (program2 != null && program2 is MIRP mirp2)
                        {
                            if (verb.ToLower() == "wield")
                            {
                                await mirp2.OnItemWield(item, new Types.MIRPEventArgs() { Item = item, Player = actor.Character }, cancellationToken);
                            }
                            else
                            {
                                await mirp2.OnItemWear(item, new Types.MIRPEventArgs() { Item = item, Player = actor.Character }, cancellationToken);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sacrifices a sacrificable item for some divine favor.
        /// </summary>
        /// <param name="user">The actor.</param>
        /// <param name="target">The item to sacrifice.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
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
                            await this.communicator.SendToPlayer(user.Connection, $"You sacrifice {item.Name} to {user.Character.Deity} for some divine favor.", cancellationToken);
                            await this.communicator.SendToRoom(user.Character, user.Character.Location, $"{user.Character.FirstName.FirstCharToUpper()} sacrifices {item.Name} to {user.Character.Deity}.", cancellationToken);
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
                    else if (item.IsPlayerCorpse)
                    {
                        await this.communicator.SendToPlayer(user.Connection, $"You can't sacrifice {item.Name}.", cancellationToken);
                        return;
                    }
                    else if (item.IsNPCCorpse)
                    {
                        this.ItemsFromCorpseToRoom(user.Character, item);
                        user.Character.DivineFavor += 1;
                        await this.communicator.SendToPlayer(user.Connection, $"You sacrifice {item.Name} to {user.Character.Deity} for some divine favor.", cancellationToken);
                        await this.communicator.SendToRoom(user.Character, user.Character.Location, $"{user.Character.FirstName.FirstCharToUpper()} sacrifices {item.Name} to {user.Character.Deity}.", cancellationToken);
                        itemsToRemove.Add(item);
                    }
                    else
                    {
                        user.Character.DivineFavor += 1;
                        await this.communicator.SendToPlayer(user.Connection, $"You sacrifice {item.Name} to {user.Character.Deity} for some divine favor.", cancellationToken);
                        await this.communicator.SendToRoom(user.Character, user.Character.Location, $"{user.Character.FirstName.FirstCharToUpper()} sacrifices {item.Name} to {user.Character.Deity}.", cancellationToken);
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

        private Tuple<IItem?, IItem?>? ItemFromRoom(UserData user, CommandArgs args, CancellationToken cancellationToken)
        {
            IItem? item = null;

            // Get all items that are in the room.
            var items = this.communicator.GetItemsInRoom(user.Character.Location);

            var itemName = args.Method;
            var containerName = args.Target;

            // Get an individual item. E.g. 'get sword', 'get 2.sword'
            if (!string.IsNullOrWhiteSpace(itemName) && string.IsNullOrWhiteSpace(containerName))
            {
                item = items?.ParseTargetName(args.Method);

                if (args.Index <= 1)
                {
                    return new Tuple<IItem?, IItem?>(item, null);
                }
                else
                {
                    // Getting an item by index (2.sword).
                    var matchingItems = items?.Where(i => i.ItemId == item?.ItemId).ToList();

                    // We do have more than 1, so get the item by index. If the index is out of bounds, just get the last one.
                    if (matchingItems != null && matchingItems.Count > 1)
                    {
                        var index = Math.Min(args.Index, matchingItems.Count - 1);
                        return new Tuple<IItem?, IItem?>(matchingItems[index], null);
                    }
                    else
                    {
                        // Null, or only had one, so just return that.
                        return new Tuple<IItem?, IItem?>(matchingItems?.FirstOrDefault(), null);
                    }
                }
            }
            else
            {
                // Trying to get an individual item from a container. See if we have a container. 'get sword chest'
                var container = items?.ParseTargetName(args.Target);

                if (container != null)
                {
                    if (container.IsClosed)
                    {
                        // May be trying to get an item from a closed container.
                        return new Tuple<IItem?, IItem?>(null, container);
                    }

                    if (container.Contains != null && container.Contains.Count > 0)
                    {
                        item = container.Contains?.ParseTargetName(args.Method);

                        if (item != null)
                        {
                            // Just return the first item we find.
                            if (args.Index <= 1)
                            {
                                return new Tuple<IItem?, IItem?>(item, container);
                            }
                            else
                            {
                                // If we have at least one item, get a list of all items with the same item Id.
                                if (item != null)
                                {
                                    var containerItems = container.Contains?.Where(i => i.ItemId == item.ItemId).ToList();

                                    // We have one more more items. Get either the given index, or the last one in the list.
                                    if (containerItems != null && containerItems.Count > 0)
                                    {
                                        var index = Math.Min(args.Index, containerItems.Count - 1);
                                        return new Tuple<IItem?, IItem?>(containerItems[index], container);
                                    }
                                    else
                                    {
                                        // Null, or only had one, so just return that.
                                        return new Tuple<IItem?, IItem?>(containerItems?.FirstOrDefault(), container);
                                    }
                                }
                                else
                                {
                                    return new Tuple<IItem?, IItem?>(null, container);
                                }
                            }
                        }
                        else
                        {
                            // Nothing like that in the container.
                            return new Tuple<IItem?, IItem?>(null, container);
                        }
                    }
                    else
                    {
                        // Container didn't contain anything matching.
                        return new Tuple<IItem?, IItem?>(null, container);
                    }
                }
                else
                {
                    // No container by that name.
                    return new Tuple<IItem?, IItem?>(null, null);
                }
            }
        }

        private async Task GetAllItemsInRoom(UserData actor, CancellationToken cancellationToken)
        {
            var itemsInRoom = this.communicator.GetItemsInRoom(actor.Character.Location);

            if (itemsInRoom != null && itemsInRoom.Count > 0)
            {
                List<Item> itemsToRemove = new ();

                foreach (var item in itemsInRoom)
                {
                    if (item.ItemType == ItemType.Currency)
                    {
                        actor.Character.Currency += item.Value;
                        await this.communicator.SendToPlayer(actor.Connection, $"You get {item.Name}.", cancellationToken);
                        itemsToRemove.Add(item);
                    }
                    else if (item.WearLocation.Contains(WearLocation.None))
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"You can't get {item.Name}.", cancellationToken);
                    }
                    else
                    {
                        if (ItemHelper.CanCarry(actor.Character, item))
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"You get {item.Name}.", cancellationToken);
                            await this.communicator.SendToRoom(actor.Character, actor.Character.Location, $"{actor.Character.FirstName.FirstCharToUpper()} gets {item.Name}.", cancellationToken);

                            actor.Character.Inventory.Add(item);
                            itemsToRemove.Add(item);

                            if (!string.IsNullOrWhiteSpace(item.Program))
                            {
                                var program = this.communicator.MIRPProcessor.CreateProgramInstance(item);

                                if (program != null && program is MIRP mirp)
                                {
                                    await mirp.OnItemGet(item, new Types.MIRPEventArgs() { Item = item, Player = actor.Character }, cancellationToken);
                                }
                            }
                        }
                        else
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"You can't carry that much weight.", cancellationToken);
                        }
                    }
                }

                var room = this.communicator.ResolveRoom(actor.Character.Location);

                if (room != null)
                {
                    room.Items.RemoveAll(i => itemsToRemove.Any(r => r.ItemId == i.ItemId));
                }
            }
            else
            {
                await this.communicator.SendToPlayer(actor.Connection, $"There's nothing here.", cancellationToken);
            }
        }

        private async Task GetAllItemsInContainer(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            var itemsInInventory = actor.Character.Inventory;
            var itemsInRoom = this.communicator.GetItemsInRoom(actor.Character.Location);
            Item? container = null;

            // Check inventory first.
            if (itemsInInventory != null && itemsInInventory.Count > 0)
            {
                container = itemsInInventory.ParseTargetName(args.Target);
            }

            // Check the items in the room.
            if (container == null && itemsInRoom != null && itemsInRoom.Count > 0)
            {
                container = itemsInRoom.ParseTargetName(args.Target);
            }

            await this.ItemsFromContainer(actor.Character, container, cancellationToken);
        }

        private async Task GetItemFromRoom(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            var room = this.communicator.ResolveRoom(actor.Character.Location);

            var itemsInRoom = this.communicator.GetItemsInRoom(actor.Character.Location);

            if (itemsInRoom != null)
            {
                List<Item> itemsToRemove = new ();

                var item = itemsInRoom.ParseTargetName(args.Method);

                if (item != null && room != null)
                {
                    var duplicateItems = itemsInRoom.Where(i => i.ItemId == item.ItemId).ToList();

                    if (duplicateItems.Count == 1)
                    {
                        // Only had 1, so use the original item.
                        if (item.WearLocation.Contains(WearLocation.None))
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"You can't get {item.Name}.", cancellationToken);
                        }
                        else
                        {
                            if (ItemHelper.CanCarry(actor.Character, item))
                            {
                                await this.communicator.SendToPlayer(actor.Connection, $"You get {item.Name}.", cancellationToken);
                                await this.communicator.SendToRoom(actor.Character.Location, actor.Character, null, $"{actor.Character.FirstName.FirstCharToUpper()} gets {item.Name}.", cancellationToken);

                                actor.Character.Inventory.Add(item);
                                room.Items.Remove(item);

                                if (!string.IsNullOrWhiteSpace(item.Program))
                                {
                                    var program = this.communicator.MIRPProcessor.CreateProgramInstance(item);

                                    if (program != null && program is MIRP mirp)
                                    {
                                        await mirp.OnItemGet(item, new Types.MIRPEventArgs() { Item = item, Player = actor.Character }, cancellationToken);
                                    }
                                }
                            }
                            else
                            {
                                await this.communicator.SendToPlayer(actor.Connection, $"You can't carry that much weight.", cancellationToken);
                            }
                        }
                    }
                    else
                    {
                        // Get the item by index.
                        var targetItem = duplicateItems[Math.Min(args.Index, duplicateItems.Count - 1)];

                        if (targetItem.WearLocation.Contains(WearLocation.None))
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"You can't get {targetItem.Name}.", cancellationToken);
                        }
                        else
                        {
                            if (ItemHelper.CanCarry(actor.Character, item))
                            {
                                await this.communicator.SendToPlayer(actor.Connection, $"You get {targetItem.Name}.", cancellationToken);
                                await this.communicator.SendToRoom(actor.Character.Location, actor.Character, null, $"{actor.Character.FirstName.FirstCharToUpper()} gets {targetItem.Name}.", cancellationToken);

                                actor.Character.Inventory.Add(targetItem);
                                room.Items.Remove(targetItem);

                                if (!string.IsNullOrWhiteSpace(item.Program))
                                {
                                    var program = this.communicator.MIRPProcessor.CreateProgramInstance(item);

                                    if (program != null && program is MIRP mirp)
                                    {
                                        await mirp.OnItemGet(item, new Types.MIRPEventArgs() { Item = item, Player = actor.Character }, cancellationToken);
                                    }
                                }
                            }
                            else
                            {
                                await this.communicator.SendToPlayer(actor.Connection, $"You can't carry that much weight.", cancellationToken);
                            }
                        }
                    }
                }
                else
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"You don't see that here.", cancellationToken);
                }
            }
            else
            {
                await this.communicator.SendToPlayer(actor.Connection, $"There's nothing here.", cancellationToken);
            }
        }

        private async Task GetItemFromItem(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            // Get the target. Could be in the room, or in inventory.
            var itemsInInventory = actor.Character.Inventory;
            var itemsInRoom = this.communicator.GetItemsInRoom(actor.Character.Location);
            Item? container = null;
            IItem? item = null;

            // Check inventory first.
            if (itemsInInventory != null && itemsInInventory.Count > 0)
            {
                container = itemsInInventory.ParseTargetName(args.Target);
            }

            // Check the items in the room.
            if (container == null && itemsInRoom != null && itemsInRoom.Count > 0)
            {
                container = itemsInRoom.ParseTargetName(args.Target);
            }

            if (container != null)
            {
                if (container.Contains != null && container.Contains.Count > 0)
                {
                    item = container.Contains.ParseTargetName(args.Method);

                    if (item != null)
                    {
                        var matchingItems = container.Contains.Where(c => c.ItemId == item.ItemId).ToList();

                        if (matchingItems.Count == 1)
                        {
                            if (ItemHelper.CanCarry(actor.Character, item))
                            {
                                // Only had 1, so use the original item.
                                await this.communicator.SendToPlayer(actor.Connection, $"You get {item.Name} from {container.Name}.", cancellationToken);
                                await this.communicator.SendToRoom(actor.Character.Location, actor.Character, null, $"{actor.Character.FirstName.FirstCharToUpper()} gets {item.Name} from {container.Name}.", cancellationToken);

                                var itemClone = (Item)item;
                                actor.Character.Inventory.Add(itemClone.DeepCopy());
                                container.Contains.Remove(item);

                                if (!string.IsNullOrWhiteSpace(itemClone.Program))
                                {
                                    var program = this.communicator.MIRPProcessor.CreateProgramInstance(itemClone);

                                    if (program != null && program is MIRP mirp)
                                    {
                                        await mirp.OnItemGet(itemClone, new Types.MIRPEventArgs() { Item = itemClone, Player = actor.Character }, cancellationToken);
                                    }
                                }
                            }
                            else
                            {
                                await this.communicator.SendToPlayer(actor.Connection, $"You can't carry that much weight.", cancellationToken);
                            }
                        }
                        else
                        {
                            if (ItemHelper.CanCarry(actor.Character, item))
                            {
                                // Get the item by index.
                                var targetItem = matchingItems[Math.Min(args.Index, matchingItems.Count - 1)];

                                await this.communicator.SendToPlayer(actor.Connection, $"You get {targetItem.Name} from {container.Name}.", cancellationToken);
                                await this.communicator.SendToRoom(actor.Character.Location, actor.Character, null, $"{actor.Character.FirstName.FirstCharToUpper()} gets {targetItem.Name} from {container.Name}.", cancellationToken);

                                var targetItemClone = (Item)targetItem;
                                actor.Character.Inventory.Add(targetItemClone.DeepCopy());
                                container.Contains.Remove(targetItemClone);

                                if (!string.IsNullOrWhiteSpace(targetItemClone.Program))
                                {
                                    var program = this.communicator.MIRPProcessor.CreateProgramInstance(targetItemClone);

                                    if (program != null && program is MIRP mirp)
                                    {
                                        await mirp.OnItemGet(targetItemClone, new Types.MIRPEventArgs() { Item = targetItemClone, Player = actor.Character }, cancellationToken);
                                    }
                                }
                            }
                            else
                            {
                                await this.communicator.SendToPlayer(actor.Connection, $"You can't carry that much weight.", cancellationToken);
                            }
                        }
                    }
                    else
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"You don't see that here.", cancellationToken);
                    }
                }
                else
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"There is nothing in {container.Name}.", cancellationToken);
                }
            }
            else
            {
                await this.communicator.SendToPlayer(actor.Connection, $"You don't see that here.", cancellationToken);
            }
        }

        private async Task GetItem(UserData actor, CommandArgs args, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Get what?", cancellationToken);
            }
            else if (args.Method.ToLower() == "all" && string.IsNullOrWhiteSpace(args.Target))
            {
                // get all
                await this.GetAllItemsInRoom(actor, cancellationToken);
            }
            else if (args.Method.ToLower() == "all" && !string.IsNullOrWhiteSpace(args.Target))
            {
                // get all chest
                await this.GetAllItemsInContainer(actor, args, cancellationToken);
            }
            else if (string.IsNullOrWhiteSpace(args.Target))
            {
                // get sword
                // get 2.sword
                await this.GetItemFromRoom(actor, args, cancellationToken);
            }
            else
            {
                // get sword chest
                // get sword backpack
                // get 2.sword chest
                // get 2.sword backpack
                await this.GetItemFromItem(actor, args, cancellationToken);
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
                    if (item.Food?.Current != null)
                    {
                        await this.communicator.SendToPlayer(user.Connection, $"You eat {item.Name}.", cancellationToken);
                        await this.communicator.SendToPlayer(user.Connection, $"You are no longer hungry.", cancellationToken);

                        // If a food value is 3, that equates to 3 meals. So when a player eats, each food value means 8 hours of food.
                        user.Character.Hunger.Current -= 8;

                        // Bugfix.
                        user.Character.Hunger.Max = 96;

                        await this.communicator.SendToRoom(user.Character, user.Character.Location, $"{user.Character.FirstName.FirstCharToUpper()} eats {item.Name}.", cancellationToken);

                        if (item.Food.Current <= 0)
                        {
                            await this.communicator.SendToPlayer(user.Connection, $"You have eaten all of {item.Name}.", cancellationToken);
                            user.Character.CarryWeight.Current -= (double)item.Weight;
                            user.Character.Inventory.Remove(item);
                        }
                        else
                        {
                            item.Food.Current -= 1;
                        }
                    }
                    else
                    {
                        await this.communicator.SendToPlayer(user.Connection, $"{item.Name.FirstCharToUpper()} has no apparent nutritional value.", cancellationToken);
                    }
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
                            if (!item.ItemFlags.Contains(ItemFlags.Cursed))
                            {
                                room.Items.Add(item.Clone());
                                await this.communicator.SendToPlayer(user.Connection, $"You drop {item.Name}.", cancellationToken);
                                await this.communicator.SendToRoom(user.Character, user.Character.Location, $"{user.Character.FirstName.FirstCharToUpper()} drops {item.Name}.", cancellationToken);
                                itemsToRemove.Add(item);

                                if (!string.IsNullOrWhiteSpace(item.Program))
                                {
                                    var program = this.communicator.MIRPProcessor.CreateProgramInstance(item);

                                    if (program != null && program is MIRP mirp)
                                    {
                                        await mirp.OnItemDrop(item, new Types.MIRPEventArgs() { Item = item, Player = user.Character }, cancellationToken);
                                    }
                                }
                            }
                            else
                            {
                                await this.communicator.SendToPlayer(user.Connection, $"You can't drop {item.Name}.", cancellationToken);
                            }
                        }
                    }

                    foreach (var itemToRemove in itemsToRemove)
                    {
                        user.Character.Inventory.Remove(itemToRemove);
                        user.Character.CarryWeight.Current -= (double)itemToRemove.Weight;
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
                        await this.communicator.SendToRoom(user.Character, user.Character.Location, $"{user.Character.FirstName.FirstCharToUpper()} drops {item.Name}.", cancellationToken);
                        itemsToRemove.Add(item);

                        if (!string.IsNullOrWhiteSpace(item.Program))
                        {
                            var program = this.communicator.MIRPProcessor.CreateProgramInstance(item);

                            if (program != null && program is MIRP mirp)
                            {
                                await mirp.OnItemDrop(item, new Types.MIRPEventArgs() { Item = item, Player = user.Character }, cancellationToken);
                            }
                        }
                    }
                    else
                    {
                        await this.communicator.SendToPlayer(user.Connection, $"You don't have that.", cancellationToken);
                    }

                    foreach (var itemToRemove in itemsToRemove)
                    {
                        user.Character.Inventory.Remove(itemToRemove);
                        user.Character.CarryWeight.Current -= (double)itemToRemove.Weight;
                    }
                }
            }
        }

        private async Task Tell(UserData user, string target, string message, CancellationToken cancellationToken = default)
        {
            if (user.Character.IsAffectedBy(nameof(Silence)))
            {
                await this.communicator.SendToPlayer(user.Connection, "You can't speak!", cancellationToken);
                return;
            }

            var targetUser = Communicator.Users?.FirstOrDefault(u => u.Value.Username == target);

            string senderName = user.Character.FirstName.FirstCharToUpper();

            var speakingLang = SkillHelper.ResolveSkill("Common", this.communicator, this.random, this.world, this.logger, this.combat);
            var speaking = user.Character.Speaking ?? speakingLang?.Name;
            var garbled = this.communicator.LanguageGenerator.BuildSentence(message);
            var skillRoll = this.random.Next(0, 99);

            if (targetUser != null && !PlayerHelper.CanPlayerSeePlayer(this.environment, this.communicator, targetUser.Value.Value.Character, user.Character))
            {
                senderName = "Someone";
            }

            if (targetUser != null)
            {
                var player = targetUser.Value.Value;

                if (!PlayerHelper.CanPlayerSeePlayer(this.environment, this.communicator, user.Character, player.Character))
                {
                    await this.communicator.SendToPlayer(user.Connection, $"Nobody around by that name.", cancellationToken);
                    return;
                }

                CommResult commResult;

                if (player.Character.HasSkill(speaking) && player.Character.GetSkillProficiency(speaking)?.Proficiency >= skillRoll)
                {
                    commResult = await this.communicator.SendToPlayer(user.Character.FirstName, target, $"{senderName} tells you (in {speaking}) \"<span class='tell'>{message}</span>\"", cancellationToken);
                }
                else
                {
                    commResult = await this.communicator.SendToPlayer(user.Character.FirstName, target, $"{senderName} tells you (in {speaking}) \"<span class='tell'><span class='{speaking?.Replace(" ", string.Empty)}'>{garbled}</span></span>\"", cancellationToken);
                }

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
                            await this.communicator.SendToPlayer(user.Character.FirstName, target, $"[NOTIFICATION]|../img/notifications/message.png|{user.Character.FirstName} has sent you a message.", cancellationToken);

                            await this.communicator.SendToPlayer(user.Connection, $"You tell {target} (in {speaking}) \"<span class='tell'>{message}</span>\"", cancellationToken);

                            // Check if the language improves each time it's used.
                            if (speakingLang != null)
                            {
                                await speakingLang.CheckImprove(user.Character, cancellationToken);
                            }

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
            else
            {
                await this.communicator.SendToPlayer(user.Connection, $"Nobody around by that name.", cancellationToken);
            }
        }

        private async Task ShowCharactersInArea(UserData actor, List<Character>? characters, List<Mobile>? mobiles, CancellationToken cancellationToken)
        {
            var sb = new StringBuilder();
            bool canSee = false;

            if (characters != null)
            {
                sb.Append($"Near you:<br/>");

                foreach (var character in characters)
                {
                    if (PlayerHelper.CanPlayerSeePlayer(this.environment, this.communicator, actor.Character, character))
                    {
                        canSee = true;

                        sb.Append($"<span class='scan'>{character.FirstName.FirstCharToUpper()} is in ");

                        var room = this.communicator.ResolveRoom(character.Location);

                        if (room != null)
                        {
                            sb.Append($"{room?.Name} [{room?.RoomId}]");
                        }

                        sb.Append("</span>");
                    }
                }
            }

            if (mobiles != null)
            {
                foreach (var mobile in mobiles)
                {
                    if (PlayerHelper.CanPlayerSeePlayer(this.environment, this.communicator, actor.Character, mobile))
                    {
                        canSee = true;

                        sb.Append($"<span class='scan'>{mobile.FirstName.FirstCharToUpper()} is in ");

                        var room = this.communicator.ResolveRoom(mobile.Location);

                        if (room != null)
                        {
                            sb.Append($"{room?.Name} [{room?.RoomId}]");
                        }

                        sb.Append("</span>");
                    }
                }
            }

            if (canSee)
            {
                await this.communicator.SendToPlayer(actor.Connection, sb.ToString(), cancellationToken);
            }
            else
            {
                await this.communicator.SendToPlayer(actor.Connection, "There are no players near you.", cancellationToken);
            }
        }

        private string ShowStatistics(Character actor)
        {
            Dictionary<WearLocation, Item> equipment = actor.Equipment;
            List<Effect> effects = actor.AffectedBy;
            var currencyTuple = actor.Currency.GetCurrency();

            int pierceTotal = equipment.Sum(a => a.Value?.Pierce ?? 0) + effects.Sum(a => a?.Pierce ?? 0);
            int slashTotal = equipment.Sum(a => a.Value?.Edged ?? 0) + effects.Sum(a => a?.Slash ?? 0);
            int bluntTotal = equipment.Sum(a => a.Value?.Blunt ?? 0) + effects.Sum(a => a?.Blunt ?? 0);
            int magicTotal = equipment.Sum(a => a.Value?.Magic ?? 0) + effects.Sum(a => a?.Magic ?? 0);

            var message = new ScoreMessage()
            {
                Message = new Legendary.Engine.Output.Message()
                {
                    Personal = new Personal()
                    {
                        Alignment = actor.Alignment.ToString(),
                        Ethos = actor.Ethos.ToString(),
                        Gender = actor.Gender.ToString(),
                        Hometown = actor.IsNPC ? "N/A" : "Griffonshire",
                        Name = actor.FirstName,
                        Race = actor.Race.ToString(),
                        Title = actor.Title,
                    },
                    Vitals = new Vitals()
                    {
                        Health = $"{actor.Health.Current}/{actor.Health.Max}",
                        Mana = $"{actor.Mana.Current}/{actor.Mana.Max}",
                        Movement = $"{actor.Movement.Current}/{actor.Movement.Max}",
                        Experience = actor.IsNPC ? "N/A" : actor.Experience.ToString(),
                        Carry = $"{(int)actor.CarryWeight.Current}/{(int)actor.CarryWeight.Max} lbs",
                        Level = actor.Level.ToString(),
                    },
                    Attributes = new Attributes()
                    {
                        Str = $"{actor.Str.Max} ({actor.Str.Current})",
                        Dex = $"{actor.Dex.Max} ({actor.Dex.Current})",
                        Wis = $"{actor.Wis.Max} ({actor.Wis.Current})",
                        Int = $"{actor.Int.Max} ({actor.Int.Current})",
                        Con = $"{actor.Con.Max} ({actor.Con.Current})",
                        Deity = actor.Deity.ToString(),
                    },
                    Armor = new Output.Armor()
                    {
                        Blunt = $"{bluntTotal}%",
                        Pierce = $"{pierceTotal}%",
                        Edged = $"{slashTotal}%",
                        Magic = $"{magicTotal}%",
                    },
                    Saves = new Saves()
                    {
                        Aff = $"{actor.SaveAfflictive}%",
                        Mal = $"{actor.SaveMaledictive}%",
                        Neg = $"{actor.SaveNegative}%",
                        Spell = $"{actor.SaveSpell}%",
                        Death = $"{actor.SaveDeath}%",
                        Learn = actor.Learns.ToString(),
                    },
                    Other = new Other()
                    {
                        Trains = actor.IsNPC ? "N/A" : actor.Trains.ToString(),
                        Pracs = actor.IsNPC ? "N/A" : actor.Practices.ToString(),
                        LastLogin = actor.IsNPC ? "N/A" : actor.Metrics.LastLogin.ToShortDateString(),
                        Gold = currencyTuple.Item1.ToString(),
                        Silver = currencyTuple.Item2.ToString(),
                        Copper = currencyTuple.Item3.ToString(),
                        HitDice = actor.HitDice.ToString(),
                        DamageDice = actor.DamageDice.ToString(),
                    },
                },
            };

            var objModel = JsonConvert.SerializeObject(message);

            return objModel;
        }

        private async Task ShowPlayerScore(UserData user, CancellationToken cancellationToken = default)
        {
            var message = this.ShowStatistics(user.Character);
            await this.communicator.SendToPlayer(user.Connection, message, cancellationToken);
        }

        private async Task GiveCurrency(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method) || string.IsNullOrEmpty(args.Target))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Give what to whom?", cancellationToken);
            }
            else
            {
                decimal amount = 0m;
                string type = args.Method.ToLower();

                switch (type)
                {
                    default:
                        return;
                    case "gold":
                        amount = (decimal)args.Index;
                        break;
                    case "silver":
                        amount = (decimal)args.Index / 10m;
                        break;
                    case "copper":
                        amount = (decimal)args.Index / 100m;
                        break;
                }

                if (actor.Character.Currency < amount)
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"You don't have that much {type} to give.", cancellationToken);
                }
                else
                {
                    // Giving to player or mob?
                    var targetPlayer = this.communicator.ResolveCharacter(args.Target);

                    if (targetPlayer != null)
                    {
                        targetPlayer.Character.Currency += amount;
                        actor.Character.Currency -= amount;
                        await this.communicator.SendToPlayer(actor.Connection, $"You give {args.Index} {type} to {targetPlayer.Character.FirstName}.", cancellationToken);
                        await this.communicator.SendToPlayer(targetPlayer.Connection, $"{actor.Character.FirstName.FirstCharToUpper()} gives you {args.Index} {type}.", cancellationToken);
                        await this.communicator.SendToRoom(actor.Character, actor.Character.Location, $"{actor.Character.FirstName.FirstCharToUpper()} gives some {type} to {targetPlayer.Character.FirstName}.", cancellationToken);
                    }
                    else
                    {
                        var targetMob = this.communicator.ResolveMobile(args.Target, actor.Character);

                        if (targetMob != null)
                        {
                            targetMob.Currency += amount;
                            actor.Character.Currency -= amount;
                            await this.communicator.SendToPlayer(actor.Connection, $"You give {args.Index} {args.Method.ToLower()} to {targetMob.FirstName}.", cancellationToken);
                            await this.communicator.SendToRoom(actor.Character, actor.Character.Location, $"{actor.Character.FirstName.FirstCharToUpper()} gives some {type} to {targetMob.FirstName}.", cancellationToken);
                        }
                        else
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"They aren't here.", cancellationToken);
                        }
                    }
                }
            }
        }
    }
}