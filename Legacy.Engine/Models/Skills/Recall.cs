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
    using Legendary.Core.Models;
    using Legendary.Engine.Contracts;

    /// <summary>
    /// Recalls the player to their hometown recall point.
    /// </summary>
    public class Recall : Skill
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Recall"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        public Recall(ICommunicator communicator)
            : base(communicator)
        {
            this.Name = "Recall";
        }

        /// <inheritdoc/>
        public override void Act(UserData actor, UserData? target)
        {
            this.PreAction = new System.Action(() =>
            {
                this.Communicator.SendToPlayer(actor.Connection, "You close your eyes and recall to your hometown.");
                this.Communicator.SendToRoom(actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} disappears in a puff of smoke.");
            });

            this.PostAction = new System.Action(() =>
            {
                this.Communicator.SendToServer(actor, "look");
            });

            this.PreAction?.Invoke();
            actor.Character.Location = actor.Character.Home ?? Room.Default;
            this.PostAction?.Invoke();
        }
    }
}
