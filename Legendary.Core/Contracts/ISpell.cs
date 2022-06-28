// <copyright file="ISpell.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Core.Contracts
{
    using Legendary.Core.Models;

    /// <summary>
    /// Implementation contract for a spell.
    /// </summary>
    public interface ISpell
    {
        /// <summary>
        /// Gets or sets the name of the spell.
        /// </summary>
        string? Name { get; set; }

        /// <summary>
        /// Casts the spell.
        /// </summary>
        /// <param name="actor">The caster.</param>
        /// <param name="target">The target.</param>
        abstract void Act(UserData actor, UserData? target);
    }
}
