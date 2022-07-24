// <copyright file="Effect.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Core.Models
{
    using Legendary.Core.Contracts;

    /// <summary>
    /// Represents the effect someone can be affected by.
    /// </summary>
    public class Effect
    {
        /// <summary>
        /// Gets or sets the action of the effect.
        /// </summary>
        public IAction? Action { get; set; }

        /// <summary>
        /// Gets or sets the source of the effect.
        /// </summary>
        public Character? Effector { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the duration.
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Gets or sets the hit dice effect.
        /// </summary>
        public int? HitDice { get; set; }

        /// <summary>
        /// Gets or sets the damage dice effect.
        /// </summary>
        public int? DamageDice { get; set; }

        /// <summary>
        /// Gets or sets the pierce effect.
        /// </summary>
        public int? Pierce { get; set; }

        /// <summary>
        /// Gets or sets the blunt effect.
        /// </summary>
        public int? Blunt { get; set; }

        /// <summary>
        /// Gets or sets the slash effect.
        /// </summary>
        public int? Slash { get; set; }

        /// <summary>
        /// Gets or sets the magic effect.
        /// </summary>
        public int? Magic { get; set; }

        /// <summary>
        /// Gets or sets the spell effect.
        /// </summary>
        public int? Spell { get; set; }

        /// <summary>
        /// Gets or sets the maledictive effect.
        /// </summary>
        public int? Maledictive { get; set; }

        /// <summary>
        /// Gets or sets the negative effect.
        /// </summary>
        public int? Negative { get; set; }

        /// <summary>
        /// Gets or sets the death effect.
        /// </summary>
        public int? Death { get; set; }

        /// <summary>
        /// Gets or sets the afflictive effect.
        /// </summary>
        public int? Afflictive { get; set; }

        /// <summary>
        /// Gets or sets the health effect.
        /// </summary>
        public int? Health { get; set; }

        /// <summary>
        /// Gets or sets the mana effect.
        /// </summary>
        public int? Mana { get; set; }

        /// <summary>
        /// Gets or sets the movement effect.
        /// </summary>
        public int? Movement { get; set; }

        /// <summary>
        /// Gets or sets the str effect.
        /// </summary>
        public int? Str { get; set; }

        /// <summary>
        /// Gets or sets the int effect.
        /// </summary>
        public int? Int { get; set; }

        /// <summary>
        /// Gets or sets the dex effect.
        /// </summary>
        public int? Dex { get; set; }

        /// <summary>
        /// Gets or sets the wis effect.
        /// </summary>
        public int? Wis { get; set; }

        /// <summary>
        /// Gets or sets the con effect.
        /// </summary>
        public int? Con { get; set; }
    }
}