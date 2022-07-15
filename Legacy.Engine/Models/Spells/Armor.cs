// <copyright file="Armor.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Models.Spells
{
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;

    /// <summary>
    /// Casts the armor spell.
    /// </summary>
    public class Armor : Spell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Armor"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="combat">The combat generator.</param>
        public Armor(ICommunicator communicator, IRandom random, Combat combat)
            : base(communicator, random, combat)
        {
            this.Name = "Armor";
            this.ManaCost = 10;
            this.CanInvoke = true;
            this.IsAffect = true;
            this.AffectDuration = 0;
        }

        /// <inheritdoc/>
        public override async Task Act(UserData actor, UserData? target, CancellationToken cancellationToken)
        {
            var effect = new Effect()
            {
                Name = this.Name,
                Duration = actor.Character.Level / 10,
                Pierce = actor.Character.Level / 10,
                Blunt = actor.Character.Level / 10,
                Magic = actor.Character.Level / 10,
                Slash = actor.Character.Level / 10,
            };

            if (target == null)
            {
                if (actor.Character.IsAffectedBy(this))
                {
                    await this.Communicator.SendToPlayer(actor.Connection, $"You are already armored.", cancellationToken);
                }
                else
                {
                    actor.Character.AffectedBy.Add(effect);
                    await this.Communicator.SendToPlayer(actor.Connection, $"You are protected by magical armor.", cancellationToken);
                    await this.Communicator.SendToRoom(actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} is protected by magical armor.", cancellationToken);
                }
            }
            else
            {
                if (actor.Character.IsAffectedBy(this))
                {
                    await this.Communicator.SendToPlayer(actor.Connection, $"{target?.Character.FirstName} is are already armored.", cancellationToken);
                }
                else
                {
                    target?.Character.AffectedBy.Add(effect);
                    await this.Communicator.SendToPlayer(actor.Connection, $"{target?.Character.FirstName} is protected by your magical armor.", cancellationToken);
                    await this.Communicator.SendToRoom(actor.Character.Location, actor.ConnectionId, $"{target?.Character.FirstName} is protected by {target?.Character.FirstName}'s armor.", cancellationToken);
                }
            }
        }
    }
}
