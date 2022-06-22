// <copyright file="Recall.cs" company="Legendary">
//  Copyright © 2021-2022 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Engine.Models.Skills
{
    using Legendary.Core.Models;
    using Legendary.Engine.Contracts;

    public class Recall : Skill
    {
        public Recall(ICommunicator communicator) : base(communicator)
        {
            this.Name = "Recall";
        }

        public override void Act(UserData actor, UserData? target)
        {
            this.communicator.SendToPlayer(actor.Connection, "You close your eyes and recall to your hometown.");
            this.communicator.SendToRoom(actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} disappears in a puff of smoke.");

            actor.Character.Location = actor.Character.Home ?? Room.Default;

            this.communicator.SendToServer(actor, "look");
        }
    }
}

