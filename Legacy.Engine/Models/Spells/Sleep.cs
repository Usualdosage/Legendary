// <copyright file="Sleep.cs" company="Legendary™">
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
    using Legendary.Core;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Extensions;

    /// <summary>
    /// Casts the sleep spell.
    /// </summary>
    public class Sleep : Spell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Sleep"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        public Sleep(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, Combat combat)
            : base(communicator, random, world, logger, combat)
        {
            this.Name = "Sleep";
            this.ManaCost = 45;
            this.CanInvoke = true;
            this.IsAffect = true;
            this.AffectDuration = 0;
        }

        /// <inheritdoc/>
        public override async Task Act(Character actor, Character? target, Item? itemTarget, CancellationToken cancellationToken)
        {
            var effect = new Effect()
            {
                Name = this.Name,
                Duration = actor.Level / 10,
            };

            if (target == null)
            {
                await this.Communicator.SendToPlayer(actor, $"Cast this spell on whom?", cancellationToken);
            }
            else
            {
                if (target.Location.Value != actor.Location.Value)
                {
                    await this.Communicator.SendToPlayer(actor, "They aren't here.", cancellationToken);
                }
                else
                {
                    if (target.IsAffectedBy(this) || (!target.IsNPC && target.CharacterFlags.Contains(Core.Types.CharacterFlags.Sleeping)))
                    {
                        await this.Communicator.SendToPlayer(actor, $"{target?.FirstName.FirstCharToUpper()} is already asleep.", cancellationToken);
                    }
                    else
                    {
                        if (this.Combat.DidSave(target, this))
                        {
                            await base.Act(actor, target, itemTarget, cancellationToken);
                            await this.Communicator.SendToPlayer(actor, $"{target.FirstName.FirstCharToUpper()} stays awake!", cancellationToken);
                            await this.Communicator.SendToPlayer(target, $"{actor.FirstName.FirstCharToUpper()} tried to put you to sleep!", cancellationToken);
                            await this.Combat.StartFighting(actor, target, cancellationToken);
                        }
                        else
                        {
                            await base.Act(actor, target, itemTarget, cancellationToken);
                            await this.Communicator.SendToPlayer(actor, $"{target.FirstName.FirstCharToUpper()} goes to sleep.", cancellationToken);
                            target.CharacterFlags.Add(Core.Types.CharacterFlags.Sleeping);
                            await this.Communicator.SendToPlayer(target, $"{actor.FirstName.FirstCharToUpper()} puts you to sleep!", cancellationToken);
                            await this.Communicator.SendToRoom(actor.Location, actor, target, $"{target.FirstName.FirstCharToUpper()} gets put to sleep by {actor.FirstName}.", cancellationToken);
                            target.AffectedBy.AddIfNotAffected(effect);
                        }
                    }
                }
            }
        }
    }
}
