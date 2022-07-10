﻿// <copyright file="Fireball.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Models.Spells
{
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;

    /// <summary>
    /// Casts the fireball spell.
    /// </summary>
    public class Fireball : Spell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Fireball"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="combat">The combat generator.</param>
        public Fireball(ICommunicator communicator, IRandom random, Combat combat)
            : base(communicator, random, combat)
        {
            this.Name = "Fireball";
            this.ManaCost = 50;
            this.CanInvoke = true;
            this.DamageType = Core.Types.DamageType.Fire;
            this.IsAffect = false;
            this.AffectDuration = 0;
            this.HitDice = 3;
            this.DamageDice = 6;
            this.DamageModifier = 200;
        }

        /// <inheritdoc/>
        public override async Task PreAction(UserData actor, UserData? target, CancellationToken cancellationToken = default)
        {
            var spellWords = this.LanguageGenerator.BuildSentence("fireball");

            if (target == null)
            {
                await this.Communicator.SendToPlayer(actor.Connection, $"You extend your hand and utter the word, '{spellWords}'", cancellationToken);
                await this.Communicator.SendToRoom(actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} extends {actor.Character.Pronoun} hand and utters the words, '{spellWords}'.", cancellationToken);
            }
            else
            {
                await this.Communicator.SendToPlayer(actor.Connection, $"You extend your hand and utter the word, '{spellWords}' at {target?.Character.FirstName}.", cancellationToken);
                await this.Communicator.SendToRoom(actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} extends {actor.Character.Pronoun} hand toward {target?.Character.FirstName} and utters the word, '{spellWords}'", cancellationToken);
            }
        }

        /// <inheritdoc/>
        public override async Task Act(UserData actor, UserData? target, CancellationToken cancellationToken)
        {
            if (target == null)
            {
                await this.DamageToRoom(actor, "fireball", cancellationToken);
            }
            else
            {
                await this.DamageToTarget(actor, target, "fireball", cancellationToken);
            }
        }

        /// <inheritdoc/>
        public override Task PostAction(UserData actor, UserData? target, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
