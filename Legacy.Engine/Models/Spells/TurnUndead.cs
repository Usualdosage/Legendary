// <copyright file="TurnUndead.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Models.Spells
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Processors;

    /// <summary>
    /// Casts the turn undead spell.
    /// </summary>
    public class TurnUndead : Spell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TurnUndead"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        public TurnUndead(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, CombatProcessor combat)
            : base(communicator, random, world, logger, combat)
        {
            this.Name = "Turn Undead";
            this.ManaCost = 100;
            this.CanInvoke = true;
            this.DamageType = Core.Types.DamageType.Holy;
            this.IsAffect = false;
            this.AffectDuration = 0;
            this.HitDice = 10;
            this.DamageDice = 20;
            this.DamageModifier = 200;
            this.DamageNoun = "holy fury";
        }

        /// <inheritdoc/>
        public override async Task Act(Character actor, Character? target, Item? itemTarget, CancellationToken cancellationToken)
        {
            var holySymbol = actor.Equipment.FirstOrDefault(e => e.Value.ItemFlags.Contains(Core.Types.ItemFlags.Holy)).Value;

            if (holySymbol == null)
            {
                holySymbol = actor.Inventory.FirstOrDefault(i => i.ItemFlags.Contains(Core.Types.ItemFlags.Holy));
            }

            if (holySymbol == null)
            {
                await this.Communicator.SendToPlayer(actor, "You are not carrying a holy symbol.", cancellationToken);
            }
            else
            {
                await this.Communicator.PlaySound(actor, Core.Types.AudioChannel.Spell, Sounds.TURNUNDEAD, cancellationToken);

                await base.Act(actor, target, itemTarget, cancellationToken);

                var mobs = this.Communicator.GetMobilesInRoom(actor.Location);

                if (mobs != null)
                {
                    var undead = mobs.Where(m => m.Race == Core.Types.Race.Undead).ToList();

                    if (undead.Count > 0)
                    {
                        foreach (var critter in undead)
                        {
                            await this.DamageToTarget(actor, critter, cancellationToken);
                        }
                    }
                }
            }
        }
    }
}
