// <copyright file="Hide.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Models.Skills
{
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Extensions;
    using Legendary.Engine.Processors;

    /// <summary>
    /// Allows a player to hide.
    /// </summary>
    public class Hide : Skill
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Hide"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        public Hide(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, CombatProcessor combat)
            : base(communicator, random, world, logger, combat)
        {
            this.Name = "Hide";
            this.ManaCost = 0;
            this.CanInvoke = true;
            this.IsAffect = true;
            this.AffectDuration = 0;
            this.DamageModifier = 0;
            this.HitDice = 0;
            this.DamageDice = 0;
        }

        /// <inheritdoc/>
        public override async Task Act(Character actor, Character? target, Item? targetItem, CancellationToken cancellationToken = default)
        {
            var room = this.Communicator.ResolveRoom(actor.Location);

            if (room != null)
            {
                if (room.Terrain == Core.Types.Terrain.Air || room.Terrain == Core.Types.Terrain.Water || room.Terrain == Core.Types.Terrain.Beach || room.Terrain == Core.Types.Terrain.Ethereal
                    || room.Terrain == Core.Types.Terrain.Desert || room.Terrain == Core.Types.Terrain.Snow || room.Terrain == Core.Types.Terrain.Shallows)
                {
                    await this.Communicator.SendToPlayer(actor, "You can't find a suitable place to hide.");
                }
                else
                {
                    await this.Communicator.SendToPlayer(actor, "You attempt to hide.");

                    // Roll percentiles again against their skill level.
                    var result = this.Random.Next(0, 100);

                    var skill = actor.GetSkillProficiency(this.Name);

                    if (result != 1 && skill != null && result < skill.Proficiency)
                    {
                        var effect = new Effect()
                        {
                            Effector = actor,
                            Action = this,
                            Name = this.Name,
                            Duration = actor.Level / 5,
                        };

                        actor.AffectedBy.AddIfNotAffected(effect);
                    }
                }
            }
        }
    }
}
