// <copyright file="Identify.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Models.Spells
{
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Extensions;

    /// <summary>
    /// Casts the identify spell.
    /// </summary>
    public class Identify : Spell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Identify"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        public Identify(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, Combat combat)
            : base(communicator, random, world, logger, combat)
        {
            this.Name = "Identify";
            this.ManaCost = 75;
            this.CanInvoke = true;
            this.IsAffect = false;
            this.AffectDuration = 0;
        }

        /// <inheritdoc/>
        public override async Task Act(Character actor, Character? target, Item? targetItem, CancellationToken cancellationToken)
        {
            if (targetItem == null)
            {
                await this.Communicator.SendToPlayer(actor, $"You don't have that item.", cancellationToken);
            }
            else
            {
                await base.Act(actor, target, targetItem, cancellationToken);

                await this.Communicator.SendToPlayer(actor, $"You glean all of the information you can from {targetItem.Name}.", cancellationToken);
                await this.Communicator.SendToRoom(actor, actor.Location, $"{actor.FirstName} peers intently at {targetItem.Name}.", cancellationToken);

                StringBuilder sb = new StringBuilder();

                sb.Append($"<h5>{targetItem.Name.FirstCharToUpper()}</h5>");
                sb.Append($"Item is of type {targetItem.ItemType}, and has an approximate value of {targetItem.Value} gold.<br/>");

                if (targetItem.ItemType == Core.Types.ItemType.Weapon)
                {
                    sb.Append($"This is a weapon of type {targetItem.WeaponType}.<br/>");
                    sb.Append($"This weapon has {targetItem.HitDice} hitdice, and {targetItem.DamageDice} damage dice.<br/>");
                    sb.Append($"It does damage of type {targetItem.DamageType}.<br/>");
                }

                if (targetItem.ItemType == Core.Types.ItemType.Armor)
                {
                    sb.Append($"Blunt protection is {targetItem.Blunt}%.<br/>");
                    sb.Append($"Edged protection is {targetItem.Edged}%.<br/>");
                    sb.Append($"Pierce protection is {targetItem.Pierce}.<br/>");
                    sb.Append($"Magic protection is {targetItem.Magic}.<br/>");
                }

                if (targetItem.ItemType == Core.Types.ItemType.Container)
                {
                    sb.Append($"The container is {(targetItem.IsLocked ? "locked" : "unlocked")}%.<br/>");
                    sb.Append($"The container can hold {targetItem.CarryWeight} pounds of stuff.<br/>");
                }

                if (targetItem.ItemType == Core.Types.ItemType.Food)
                {
                    sb.Append($"There are {targetItem.Food?.Current} of {targetItem.Food?.Max} meals left.<br/>");
                }

                if (targetItem.ItemType == Core.Types.ItemType.Drink)
                {
                    sb.Append($"The liquid in the container is {targetItem.LiquidType}.<br/>");
                    sb.Append($"There are {targetItem.Drinks?.Current} of {targetItem.Drinks?.Max} drinks left.<br/>");
                }

                if (targetItem.ItemType == Core.Types.ItemType.Potion)
                {
                    sb.Append($"The potion casts {targetItem.SpellName} at level {targetItem.CastLevel}.<br/>");
                }

                sb.Append($"This item weighs {targetItem.Weight} pounds.<br/>");

                await this.Communicator.SendToPlayer(actor, sb.ToString(), cancellationToken);
            }
        }
    }
}
