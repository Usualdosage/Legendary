// <copyright file="Recall.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Models.Skills
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;

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
        /// <param name="combat">The combat generator.</param>
        public Recall(ICommunicator communicator, IRandom random, Combat combat)
            : base(communicator, random, combat)
        {
            this.Name = "Recall";
            this.ManaCost = 5;
            this.CanInvoke = true;
            this.DamageType = Core.Types.DamageType.None;
            this.IsAffect = false;
            this.AffectDuration = 0;
        }

        /// <inheritdoc/>
        public override async Task PreAction(UserData actor, UserData? target, CancellationToken cancellationToken = default)
        {
            await this.Communicator.SendToPlayer(actor.Connection, "You close your eyes and recall to your hometown.", cancellationToken);
            await this.Communicator.SendToRoom(null, actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} disappears in a puff of smoke.", cancellationToken);
        }

        /// <inheritdoc/>
        public override async Task Act(UserData actor, UserData? target, CancellationToken cancellationToken)
        {
            await Task.Run(
                () =>
                {
                    this.Communicator.PlaySound(actor.Character, Core.Types.AudioChannel.Spell, "../audio/soundfx/recall.mp3", cancellationToken);
                    actor.Character.Location = actor.Character.Home;
                }, cancellationToken);
        }

        /// <inheritdoc/>
        public override async Task PostAction(UserData actor, UserData? target, CancellationToken cancellationToken = default)
        {
            await this.Communicator.ShowRoomToPlayer(actor, cancellationToken);
        }
    }
}
