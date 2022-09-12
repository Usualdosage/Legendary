// <copyright file="Steal.cs" company="Legendary™">
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
    /// Allows a player to steal from another player.
    /// </summary>
    public class Steal : Skill
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Steal"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        public Steal(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, Combat combat)
            : base(communicator, random, world, logger, combat)
        {
            this.Name = "Steal";
            this.CanInvoke = true;
            this.IsAffect = false;
        }

        /// <inheritdoc/>
        public override async Task Act(Character actor, Character? target, Item? itemTarget, CancellationToken cancellationToken)
        {
            if (target == null)
            {
                await this.Communicator.SendToPlayer(actor, "Steal from whom?", cancellationToken);
            }
            else
            {
                // Roll percentiles again against their skill level.
                var result = this.Random.Next(0, 100);

                var skill = actor.GetSkillProficiency(this.Name);

                if (result != 1 && skill != null && result < skill.Proficiency)
                {
                    await this.Communicator.SendToPlayer(actor, $"You silently rummage around in {target.FirstName.FirstCharToUpper()}'s inventory, and pluck out something.", cancellationToken);

                    // Get a random item from their inventory.
                    if (target.Inventory.Count > 0)
                    {
                        var equipment = target.Inventory[this.Random.Next(0, target.Equipment.Count)];

                        if (equipment != null)
                        {
                            // Add to player's inventory.
                            target.Inventory.Remove(equipment);
                            actor.Inventory.Add(equipment.DeepCopy());
                            await this.Communicator.SendToPlayer(actor, $"You snag {equipment.Name} and stuff it in your inventory.", cancellationToken);
                        }
                    }
                    else
                    {
                        // They don't have jack, so steal some money.
                        var currency = target.Currency / 10;

                        var randomAmount = Math.Max(0, this.Random.Next(0, currency));

                        if (randomAmount == 0)
                        {
                            await this.Communicator.SendToPlayer(actor, $"{target.FirstName.FirstCharToUpper()} is totally broke. You can't find a single copper.", cancellationToken);
                        }
                        else
                        {
                            target.Currency -= randomAmount;
                            actor.Currency += randomAmount;
                            await this.Communicator.SendToPlayer(actor, $"You filch {randomAmount.CurrenyToWords()} from {target.FirstName}.", cancellationToken);
                        }
                    }
                }
                else
                {
                    // Done got busted.
                    await this.Communicator.SendToPlayer(actor, $"You attempt to steal from {target.FirstName.FirstCharToUpper()} but you get BUSTED!", cancellationToken);

                    // Target will yell.
                    await this.Communicator.SendToArea(actor.Location, string.Empty, $"{target.FirstName.FirstCharToUpper()} yells \"<span class='yell'>{actor.FirstName.FirstCharToUpper()}, just tried to steal from me!</span>\"", cancellationToken);

                    // And start fighting.
                    await this.Combat.StartFighting(actor, target, cancellationToken);
                }
            }
        }
    }
}
