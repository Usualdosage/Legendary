// <copyright file="CreateFood.cs" company="Legendary™">
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

    /// <summary>
    /// Casts the create food spell.
    /// </summary>
    public class CreateFood : Spell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateFood"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="combat">The combat generator.</param>
        public CreateFood(ICommunicator communicator, IRandom random, Combat combat)
            : base(communicator, random, combat)
        {
            this.Name = "Create Food";
            this.ManaCost = 10;
            this.CanInvoke = true;
            this.IsAffect = false;
            this.AffectDuration = 0;
        }

        /// <inheritdoc/>
        public override async Task Act(Character actor, Character? target, CancellationToken cancellationToken)
        {
            var item = this.CreateFoodItem();

            await this.Communicator.SendToPlayer(actor, $"You close your eyes and {item.Name} suddenly appears.", cancellationToken);
            await this.Communicator.SendToRoom(actor.Location, actor, null, $"{actor.FirstName} closes {actor.Pronoun} eyes and {item.Name} suddenly appears.", cancellationToken);

            var room = this.Communicator.ResolveRoom(actor.Location);
            room.Items.Add(item);
        }

        private Item CreateFoodItem()
        {
            List<string> adjectives = new List<string>()
            {
                "warm",
                "delicious",
                "tempting",
                "fresh",
                "steaming",
                "grilled",
                "smoked",
                string.Empty,
            };

            List<string> foodNouns = new List<string>()
            {
                "loaf of bread",
                "biscuit",
                "chicken breast",
                "leg of lamb",
                "side of beef",
                "apple pie",
                "pot pie",
                "turkey leg",
                "rabbit stew",
                "cottage pie",
                "shepherd's pie",
                "filet of fish",
                "mushroom stew",
            };

            var adj = adjectives[this.Random.Next(0, adjectives.Count - 1)];

            var food = foodNouns[this.Random.Next(0, foodNouns.Count - 1)];

            string title = $"a {adj} {food}";
            string shortDesc = $"A {adj} {food} is here.";

            var item = new Item()
            {
                ItemType = Core.Types.ItemType.Food,
                Name = title,
                ShortDescription = shortDesc,
                LongDescription = shortDesc,
                RotTimer = this.Random.Next(8, 36),
                Weight = this.Random.Next(1, 4),
                Value = this.Random.Next(4, 24),
            };

            return item;
        }
    }
}
