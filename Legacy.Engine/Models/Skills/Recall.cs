﻿// <copyright file="Recall.cs" company="Legendary™">
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
    using Legendary.Core;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Extensions;
    using Legendary.Engine.Processors;

    /// <summary>
    /// Recalls the player to their hometown recall point.
    /// </summary>
    public class Recall : Skill
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Recall"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        public Recall(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, CombatProcessor combat)
            : base(communicator, random, world, logger, combat)
        {
            this.Name = "Recall";
            this.ManaCost = 0;
            this.CanInvoke = true;
            this.DamageType = Core.Types.DamageType.None;
            this.IsAffect = false;
            this.AffectDuration = 0;
        }

        /// <inheritdoc/>
        public override async Task Act(Character actor, Character? target, Item? itemTarget, CancellationToken cancellationToken)
        {
            if (actor.Level > 10)
            {
                await this.Communicator.SendToPlayer(actor, "Only those level 10 and below may use recall.", cancellationToken);
            }
            else
            {
                await this.Communicator.SendToPlayer(actor, "You close your eyes and recall to your hometown.", cancellationToken);
                await this.Communicator.SendToRoom(actor.Location, actor, target, $"{actor.FirstName.FirstCharToUpper()} disappears in a puff of smoke.", cancellationToken);
                await this.Communicator.PlaySound(actor, Core.Types.AudioChannel.Spell, Sounds.RECALL, cancellationToken);
                actor.Location = actor.Home;
            }
        }

        /// <inheritdoc/>
        public override async Task PostAction(Character actor, Character? target, Item? itemTarget, CancellationToken cancellationToken = default)
        {
            await this.Communicator.ShowRoomToPlayer(actor, cancellationToken);
            await this.CheckImprove(actor, cancellationToken);
        }
    }
}
