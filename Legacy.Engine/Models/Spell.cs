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
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core.Contracts;
    using Legendary.Core.Extensions;
    using Legendary.Core.Models;
    using Legendary.Core.Types;

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
        /// <param name="combat">The combat generator.</param>
        protected Spell(ICommunicator communicator, IRandom random, Combat combat)
            : base(communicator, random, combat)
        {
        }

        /// <inheritdoc/>
        public override ActionType ActionType => ActionType.Spell;

        /// <summary>
        /// Invokes damage on a single target.
        /// </summary>
        /// <param name="actor">The caster.</param>
        /// <param name="target">The target.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public virtual async Task DamageToTarget(Character actor, Character target, CancellationToken cancellationToken)
        {
            Combat.StartFighting(actor, target);
            await this.Communicator.SendToArea(actor.Location, string.Empty, $"{target.FirstName} yells \"<span class='yell'>{actor.FirstName}, you sorcerous dog!</span>\"", cancellationToken);
            await this.Combat.DoDamage(actor, target, this, cancellationToken);
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
                    // TODO: Do damage to everything in the room that isn't the player, or in the player's group.
                    if (user.Value.Character.Location.InSamePlace(actor.Location) && user.Value.Character.FirstName != actor.FirstName)
                    {
                        Combat.StartFighting(actor, user.Value.Character);
                        await this.Combat.DoDamage(actor, user.Value.Character, this, cancellationToken);
                    }
                }
            }

            // Do damage to all mobiles in the room.
            var mobiles = this.Communicator.GetMobilesInRoom(actor.Location);

            if (mobiles != null)
            {
                foreach (var mobile in mobiles)
                {
                    Combat.StartFighting(actor, mobile);
                    await this.Combat.DoDamage(actor, mobile, this, cancellationToken);
                }
            }
        }
    }
}
