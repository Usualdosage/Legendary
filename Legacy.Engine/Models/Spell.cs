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
        /// <param name="spellName">The name of the spell.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public virtual async Task DamageToTarget(UserData actor, UserData target, string spellName, CancellationToken cancellationToken)
        {
            var damage = this.Combat.CalculateDamage(actor.Character, target.Character, this);
            var damageVerb = this.Combat.CalculateDamageVerb(damage);

            // Do damage directly to the target.
            await this.Communicator.SendToPlayer(actor.Connection, $"Your {spellName} {damageVerb} {target?.Character.FirstName}!", cancellationToken);
            await this.Communicator.SendToRoom(actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName}'s {spellName} {damageVerb} {target?.Character.FirstName}!", cancellationToken);
        }

        /// <summary>
        /// Invokes damage for everything in the room.
        /// </summary>
        /// <param name="actor">The caster.</param>
        /// <param name="spellName">The name of the spell.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public virtual async Task DamageToRoom(UserData actor, string spellName, CancellationToken cancellationToken)
        {
            if (Legendary.Engine.Communicator.Users != null)
            {
                foreach (var user in Legendary.Engine.Communicator.Users)
                {
                    // Do damage to everything in the room that isn't the player, or in the player's group.
                    if (user.Value.Character.Location.RoomId == actor.Character.Location.RoomId && user.Value.Character.FirstName != actor.Character.FirstName)
                    {
                        var damage = this.Combat.CalculateDamage(actor.Character, user.Value.Character, this);
                        var damageVerb = this.Combat.CalculateDamageVerb(damage);

                        await this.Communicator.SendToPlayer(actor.Connection, $"Your {spellName} {damageVerb} {user.Value.Character.FirstName}!", cancellationToken);
                        await this.Communicator.SendToRoom(actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName}'s {spellName} {damageVerb} {user.Value.Character.FirstName}!", cancellationToken);

                        this.Combat.ApplyDamage(user.Value.Character, damage);
                    }
                }
            }

            // Do damage to all mobiles in the room.
            var mobiles = this.Communicator.GetMobilesInRoom(actor.Character.Location);

            if (mobiles != null)
            {
                foreach (var mobile in mobiles)
                {
                    var damage = this.Combat.CalculateDamage(actor.Character, mobile, this);
                    var damageVerb = this.Combat.CalculateDamageVerb(damage);

                    await this.Communicator.SendToPlayer(actor.Connection, $"Your fireball {damageVerb} {mobile.FirstName}!", cancellationToken);
                    await this.Communicator.SendToRoom(actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName}'s spell {damageVerb} {mobile.FirstName}!", cancellationToken);

                    this.Combat.ApplyDamage(mobile, damage);
                }
            }
        }
    }
}
