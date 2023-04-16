// <copyright file="Gate.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Models.Spells
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Extensions;
    using Legendary.Engine.Processors;

    /// <summary>
    /// Casts the Gate spell.
    /// </summary>
    public class Gate : Spell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Gate"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        public Gate(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, CombatProcessor combat)
            : base(communicator, random, world, logger, combat)
        {
            this.Name = "Gate";
            this.ManaCost = 80;
            this.CanInvoke = true;
            this.IsAffect = true;
            this.AffectDuration = 0;
        }

        /// <inheritdoc/>
        public override async Task Act(Character actor, Character? target, Item? itemTarget, CancellationToken cancellationToken)
        {
            if (target != null)
            {
                if (actor.Fighting.HasValue)
                {
                    await this.Communicator.SendToPlayer(actor, $"You can't create a gate while you're fighting!", cancellationToken);
                    return;
                }

                await this.Communicator.PlaySound(actor, Core.Types.AudioChannel.Spell, Sounds.GATE, cancellationToken);
                await base.Act(actor, target, itemTarget, cancellationToken);
                await this.Communicator.SendToPlayer(actor, $"Your open a gate, step through, and vanish.", cancellationToken);
                await this.Communicator.SendToRoom(actor.Location, actor, target, $"{actor.FirstName.FirstCharToUpper()} opens an ethereal gate, steps through, and vanishes.", cancellationToken);
                await this.Communicator.PlaySoundToRoom(actor, target, Sounds.GATE, cancellationToken);

                actor.Location = target.Location;

                await this.Communicator.ShowRoomToPlayer(actor, cancellationToken);

                await this.Communicator.SendToRoom(actor.Location, actor, target, $"{actor.FirstName.FirstCharToUpper()} arrives through an ethereal gate.", cancellationToken);
            }
            else
            {
                await this.Communicator.SendToPlayer(actor, $"Whom would you like to create a gate to?", cancellationToken);
            }
        }
    }
}
