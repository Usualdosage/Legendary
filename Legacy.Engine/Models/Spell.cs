﻿// <copyright file="Spell.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Models
{
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core.Contracts;
    using Legendary.Core.Extensions;
    using Legendary.Core.Models;
    using Legendary.Core.Types;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Extensions;
    using Legendary.Engine.Helpers;
    using Legendary.Engine.Processors;

    /// <summary>
    /// Abstract implementation of an ISkill contract.
    /// </summary>
    public abstract class Spell : Action
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Spell"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        protected Spell(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, CombatProcessor combat)
            : base(communicator, random, world, logger, combat)
        {
        }

        /// <inheritdoc/>
        public override ActionType ActionType => ActionType.Spell;

        /// <inheritdoc/>
        public override async Task Act(Character actor, Character? target, Item? targetItem, CancellationToken cancellationToken = default)
        {
            var spellWords = $"<span class='spellWords'>{this.LanguageGenerator.BuildSentence(this.Name)}</span>";

            if (target == null)
            {
                await this.Communicator.SendToPlayer(actor, $"You extend your hand and utter the word, '{spellWords}'.", cancellationToken);
                await this.Communicator.SendToRoom(actor.Location, actor, target, $"{actor.FirstName.FirstCharToUpper()} extends {actor.Pronoun} hand and utters the words, '{spellWords}'.", cancellationToken);
            }
            else
            {
                await this.Communicator.SendToPlayer(actor, $"You extend your hand and utter the word, '{spellWords}' at {target?.FirstName}.", cancellationToken);
                await this.Communicator.SendToRoom(actor.Location, actor, target, $"{actor.FirstName.FirstCharToUpper()} extends {actor.Pronoun} hand toward {target?.FirstName} and utters the word, '{spellWords}'", cancellationToken);
            }

            await this.CheckImprove(actor, cancellationToken);
        }

        /// <summary>
        /// Invokes damage on a single target.
        /// </summary>
        /// <param name="actor">The caster.</param>
        /// <param name="target">The target.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public virtual async Task DamageToTarget(Character actor, Character target, CancellationToken cancellationToken)
        {
            if (target.Fighting == null)
            {
                await this.Communicator.SendToArea(actor.Location, string.Empty, $"{target.FirstName.FirstCharToUpper()} yells \"<span class='yell'>Die, {actor.FirstName}, you sorcerous dog!</span>\"", cancellationToken);
            }

            await this.CombatProcessor.StartFighting(actor, target, cancellationToken);
            await this.CombatProcessor.DoDamage(actor, target, this, false, cancellationToken);
        }

        /// <summary>
        /// Invokes damage for everything in the room.
        /// </summary>
        /// <param name="actor">The caster.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public virtual async Task DamageToRoom(Character actor, CancellationToken cancellationToken)
        {
            if (Legendary.Engine.Communicator.Users != null)
            {
                foreach (var user in Legendary.Engine.Communicator.Users)
                {
                    if (user.Value.Character.Location.InSamePlace(actor.Location) && user.Value.Character.FirstName != actor.FirstName)
                    {
                        if (!GroupHelper.IsGroupedWith(actor.CharacterId, user.Value.Character.CharacterId))
                        {
                            await this.CombatProcessor.StartFighting(actor, user.Value.Character, cancellationToken);
                            await this.CombatProcessor.DoDamage(actor, user.Value.Character, this, false, cancellationToken);
                        }
                    }
                }
            }

            // Do damage to all mobiles in the room.
            var mobiles = this.Communicator.GetMobilesInRoom(actor.Location);

            if (mobiles != null)
            {
                foreach (var mobile in mobiles)
                {
                    await this.CombatProcessor.StartFighting(actor, mobile, cancellationToken);
                    await this.CombatProcessor.DoDamage(actor, mobile, this, false, cancellationToken);
                }
            }
        }
    }
}
