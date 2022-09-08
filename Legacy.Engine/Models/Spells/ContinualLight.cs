// <copyright file="ContinualLight.cs" company="Legendary™">
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

    /// <summary>
    /// Casts the continual light spell.
    /// </summary>
    public class ContinualLight : Spell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContinualLight"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        public ContinualLight(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, Combat combat)
            : base(communicator, random, world, logger, combat)
        {
            this.Name = "Continual Light";
            this.ManaCost = 10;
            this.CanInvoke = true;
            this.IsAffect = false;
            this.AffectDuration = 0;
        }

        /// <inheritdoc/>
        public override async Task Act(Character actor, Character? target, Item? itemTarget, CancellationToken cancellationToken)
        {
            await base.Act(actor, target, itemTarget, cancellationToken);

            var item = this.CreateFoodItem();

            await this.Communicator.SendToPlayer(actor, $"You twiddle your thumbs and {item.Name} suddenly appears.", cancellationToken);
            await this.Communicator.SendToRoom(actor.Location, actor, null, $"{actor.FirstName.FirstCharToUpper()} twiddles {actor.Pronoun} thumbs and {item.Name} suddenly appears.", cancellationToken);

            var room = this.Communicator.ResolveRoom(actor.Location);

            if (room != null)
            {
                room.Items?.Add(item);
            }
        }

        private Item CreateFoodItem()
        {
            List<string> adjectives = new List<string>()
            {
                "glowing",
                "bright",
                "shiny",
                "yellow",
                "brilliant",
                "luminous",
                string.Empty,
            };

            List<string> lightNouns = new List<string>()
            {
                "candle",
                "lantern",
                "lamp",
                "ball of light",
                "sphere of light",
                "orb",
            };

            var adj = adjectives[this.Random.Next(0, adjectives.Count - 1)];

            var light = lightNouns[this.Random.Next(0, lightNouns.Count - 1)];

            string title = $"a {adj} {light}";
            string shortDesc = $"A {adj} {light} is here.";

            var item = new Item()
            {
                ItemType = ItemType.Light,
                WearLocation = new List<WearLocation>() { WearLocation.Light },
                Name = title,
                ShortDescription = shortDesc,
                LongDescription = shortDesc,
                RotTimer = this.Random.Next(72, 144),
                Weight = this.Random.Next(1, 2),
                Value = .05m,
                ItemId = Constants.ITEM_LIGHT,
                ItemFlags = new List<ItemFlags>() { ItemFlags.Glowing },
            };

            return item;
        }
    }
}
