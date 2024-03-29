﻿// <copyright file="CreateSpring.cs" company="Legendary™">
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
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Core.Types;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Extensions;
    using Legendary.Engine.Processors;

    /// <summary>
    /// Casts the cause light spell.
    /// </summary>
    public class CreateSpring : Spell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateSpring"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        public CreateSpring(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, CombatProcessor combat)
            : base(communicator, random, world, logger, combat)
        {
            this.Name = "Create Spring";
            this.ManaCost = 20;
            this.CanInvoke = true;
            this.IsAffect = false;
            this.AffectDuration = 0;
        }

        /// <inheritdoc/>
        public override async Task Act(Character actor, Character? target, Item? itemTarget, CancellationToken cancellationToken)
        {
            await base.Act(actor, target, itemTarget, cancellationToken);

            var item = this.CreateSpringItem();

            await this.Communicator.SendToPlayer(actor, $"You close your eyes and a bubbling spring suddenly appears.", cancellationToken);
            await this.Communicator.SendToRoom(actor.Location, actor, null, $"{actor.FirstName.FirstCharToUpper()} closes {actor.Pronoun} eyes and a bubbling spring suddenly appears.", cancellationToken);

            var room = this.Communicator.ResolveRoom(actor.Location);

            if (room != null)
            {
                room.Items?.Add(item);
            }
        }

        private Item CreateSpringItem()
        {
            string title = $"a bubbling spring";
            string shortDesc = $"A crystal clear spring bubbles from the ground here.";

            var item = new Item()
            {
                ItemType = ItemType.Spring,
                ItemId = Constants.ITEM_SPRING,
                Name = title,
                ShortDescription = shortDesc,
                LongDescription = shortDesc,
                RotTimer = 48,
                ItemFlags = new List<ItemFlags>() { ItemFlags.Glowing },
                WearLocation = new List<WearLocation>() { WearLocation.None },
                LiquidType = LiquidType.Water,
                Drinks = new MaxCurrent(100, 100),
            };

            return item;
        }
    }
}
