// <copyright file="BaseMIRP.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Models
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Engine.Processors;
    using Legendary.Engine.Types;

    /// <summary>
    /// Base class for implementing a Mob-Item-Room program.
    /// </summary>
    public abstract class BaseMIRP
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseMIRP"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        /// <param name="world">The world.</param>
        /// <param name="actionProcessor">The action processor.</param>
        /// <param name="combatProcessor">The combat processor.</param>
        /// <param name="awardProcessor">The award processor.</param>
        /// <param name="skillProcessor">The skill processor.</param>
        /// <param name="spellProcessor">The spell processor.</param>
        public BaseMIRP(ICommunicator communicator, IWorld world, ActionProcessor actionProcessor, CombatProcessor combatProcessor, AwardProcessor awardProcessor, SkillProcessor skillProcessor, SpellProcessor spellProcessor)
        {
            this.Communicator = communicator;
            this.World = world;
            this.ActionProcessor = actionProcessor;
            this.AwardProcessor = awardProcessor;
            this.CombatProcessor = combatProcessor;
            this.SkillProcessor = skillProcessor;
            this.SpellProcessor = spellProcessor;
        }

        /// <summary>
        /// Event handler for MIRP events.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        public delegate void MIRPEventHandler(object? sender, MIRPEventArgs e);

        /// <summary>
        /// Fires when a mob enters a room.
        /// </summary>
        public event MIRPEventHandler? MobEnter;

        /// <summary>
        /// Fires when a mob leaves a room.
        /// </summary>
        public event MIRPEventHandler? MobLeave;

        /// <summary>
        /// Fires when a player enters a room.
        /// </summary>
        public event MIRPEventHandler? PlayerEnter;

        /// <summary>
        /// Fires when a player leaves a room.
        /// </summary>
        public event MIRPEventHandler? PlayerLeave;

        /// <summary>
        /// Fires when a player communicates (say or yell).
        /// </summary>
        public event MIRPEventHandler? PlayerComm;

        /// <summary>
        /// Fires when an item is picked up.
        /// </summary>
        public event MIRPEventHandler? ItemGet;

        /// <summary>
        /// Fires when an item is dropped.
        /// </summary>
        public event MIRPEventHandler? ItemDrop;

        /// <summary>
        /// Fires when an item is is wielded.
        /// </summary>
        public event MIRPEventHandler? ItemWield;

        /// <summary>
        /// Fires when an item is worn.
        /// </summary>
        public event MIRPEventHandler? ItemWear;

        /// <summary>
        /// Fires each tick.
        /// </summary>
        public event MIRPEventHandler? Tick;

        /// <summary>
        /// Gets the communicator.
        /// </summary>
        public ICommunicator Communicator { get; private set; }

        /// <summary>
        /// Gets the world.
        /// </summary>
        public IWorld World { get; private set; }

        /// <summary>
        /// Gets the action processor.
        /// </summary>
        public ActionProcessor ActionProcessor { get; private set; }

        /// <summary>
        /// Gets the combat processor.
        /// </summary>
        public CombatProcessor CombatProcessor { get; private set; }

        /// <summary>
        /// Gets the award processor.
        /// </summary>
        public AwardProcessor AwardProcessor { get; private set; }

        /// <summary>
        /// Gets the skill processor.
        /// </summary>
        public SkillProcessor SkillProcessor { get; private set; }

        /// <summary>
        /// Gets the spell processor.
        /// </summary>
        public SpellProcessor SpellProcessor { get; private set; }

        /// <summary>
        /// Event handler for MobEnter.
        /// </summary>
        /// <param name="mobile">The mobile.</param>
        /// <param name="args">The args.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        protected virtual async Task OnMobEnter(Mobile mobile, MIRPEventArgs args, CancellationToken cancellationToken = default)
        {
            await Task.Run(() => this.MobEnter?.Invoke(mobile, args), cancellationToken);
        }

        /// <summary>
        /// Event handler for MobLeave.
        /// </summary>
        /// <param name="mobile">The mobile.</param>
        /// <param name="args">The args.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        protected virtual async Task OnMobLeave(Mobile mobile, MIRPEventArgs args, CancellationToken cancellationToken = default)
        {
            await Task.Run(() => this.MobLeave?.Invoke(mobile, args), cancellationToken);
        }

        /// <summary>
        /// Event handler for player enter.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="args">The event args.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        protected virtual async Task OnPlayerEnter(Character player, MIRPEventArgs args, CancellationToken cancellationToken = default)
        {
            await Task.Run(() => this.PlayerEnter?.Invoke(player, args), cancellationToken);
        }

        /// <summary>
        /// Event handler for player leave.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="args">The event args.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        protected virtual async Task OnPlayerLeave(Character player, MIRPEventArgs args, CancellationToken cancellationToken = default)
        {
            await Task.Run(() => this.PlayerLeave?.Invoke(player, args), cancellationToken);
        }

        /// <summary>
        /// Event handler for player comm (say/yell).
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="args">The event args.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        protected virtual async Task OnPlayerComm(Character player, MIRPEventArgs args, CancellationToken cancellationToken = default)
        {
            await Task.Run(() => this.PlayerComm?.Invoke(player, args), cancellationToken);
        }

        /// <summary>
        /// Event handler for item get.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="args">The event args.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        protected virtual async Task OnItemGet(Item item, MIRPEventArgs args, CancellationToken cancellationToken = default)
        {
            await Task.Run(() => this.ItemGet?.Invoke(item, args), cancellationToken);
        }

        /// <summary>
        /// Event handler for item drop.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="args">The event args.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        protected virtual async Task OnItemDrop(Item item, MIRPEventArgs args, CancellationToken cancellationToken = default)
        {
            await Task.Run(() => this.ItemDrop?.Invoke(item, args), cancellationToken);
        }

        /// <summary>
        /// Event handler for item wear.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="args">The event args.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        protected virtual async Task OnItemWear(Item item, MIRPEventArgs args, CancellationToken cancellationToken = default)
        {
            await Task.Run(() => this.ItemWear?.Invoke(item, args), cancellationToken);
        }

        /// <summary>
        /// Event handler for item wield.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="args">The event args.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        protected virtual async Task OnItemWield(Item item, MIRPEventArgs args, CancellationToken cancellationToken = default)
        {
            await Task.Run(() => this.ItemWield?.Invoke(item, args), cancellationToken);
        }

        /// <summary>
        /// Event handler for tick.
        /// </summary>
        /// <param name="sender">The sender (room, mob, item).</param>
        /// <param name="args">The event args.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        protected virtual async Task OnTick(object sender, MIRPEventArgs args, CancellationToken cancellationToken = default)
        {
            await Task.Run(() => this.Tick?.Invoke(sender, args), cancellationToken);
        }
    }
}
