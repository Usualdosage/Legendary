// <copyright file="Spell.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Models
{
<<<<<<< HEAD
    using System;
=======
    using System.Threading;
    using System.Threading.Tasks;
>>>>>>> 4e33d3b (Checkpoint.)
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Core.Types;
    using Legendary.Engine.Contracts;

<<<<<<< HEAD
    /// <summary>
    /// Abstract implementation of an ISpell contract.
    /// </summary>
    public abstract class Spell : IAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Spell"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        public Spell(ICommunicator communicator)
        {
            this.Communicator = communicator;
        }

        /// <summary>
        /// Gets the communicator.
        /// </summary>
        public ICommunicator Communicator { get; private set; }

        /// <inheritdoc/>
        public ActionType ActionType => ActionType.Spell;

        /// <inheritdoc/>
        public string? Name { get; set; }

        /// <inheritdoc/>
        public Action? PreAction { get; set; }

        /// <inheritdoc/>
        public Action? PostAction { get; set; }

        /// <inheritdoc/>
        public bool IsAffect { get; set; }

        /// <inheritdoc/>
        public int? AffectDuration { get; set; }

        /// <inheritdoc/>
        public abstract void Act(UserData actor, UserData? target);
=======
    public abstract class Spell : ISpell
	{
        protected readonly ICommunicator communicator;
        protected readonly IRandom random;
        protected readonly LanguageGenerator languageGenerator;

        public abstract Task Cast(UserData actor, UserData? target, CancellationToken cancellationToken = default);

        public string Name { get; private set; }
        public string DamageNoun { get; private set; }
        public int ManaCost { get; private set; }
        public SpellType SpellType { get; private set; }
        public DamageType DamageType { get; private set; }

        public Spell(ICommunicator communicator, IRandom random, string name, string damageNoun, int manaCost, SpellType spellType, DamageType damageType)
        {
            this.communicator = communicator;
            this.random = random;
            this.languageGenerator = new LanguageGenerator(this.random);

            this.Name = name;
            this.DamageNoun = damageNoun;
            this.ManaCost = manaCost;
            this.SpellType = spellType;
            this.DamageType = damageType;
        }

        public async Task PreMessage(UserData actor, CancellationToken cancellationToken = default)
        {
            // Display the casting message to the players and room.
            string spellMessage = this.languageGenerator.BuildSentence(this.Name ?? string.Empty);
            await communicator.SendToPlayer(actor.Connection, $"You utter the incantation, {spellMessage}'.", cancellationToken);
            await communicator.SendToRoom(actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} utters the incantation, {spellMessage}'.", cancellationToken);

        }

        public async Task PostMessage(UserData actor, UserData target, CancellationToken cancellationToken = default)
        {
            // Display damage message
            await communicator.SendToPlayer(actor.Connection, $"Your {this.DamageNoun} MUTILATES {target.Character.FirstName}!", cancellationToken);
            await communicator.SendToPlayer(target.Connection, $"{actor.Character.FirstName}'s {this.DamageNoun} MUTILATES you!", cancellationToken);
            await communicator.SendToRoom(actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName}'s {this.DamageNoun} MUTILATES {target.Character.FirstName}!", cancellationToken);
        }

        public virtual async Task<bool> CanCast(UserData actor, UserData? target, CancellationToken cancellationToken = default)
        {
            var canCast = actor.Character.Mana.Current >= this.ManaCost;
            if (canCast)
            {
                return true;
            }
            else
            {
                await communicator.SendToPlayer(actor.Connection, "You don't have enough mana", cancellationToken);
                return false;
            }
        }
>>>>>>> 4e33d3b (Checkpoint.)
    }
}
