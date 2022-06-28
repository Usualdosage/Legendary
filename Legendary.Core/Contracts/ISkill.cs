// <copyright file="ISkill.cs" company="Legendary™">
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
    /// Implementation contract for a skill.
    /// </summary>
    public interface ISkill
    {
        /// <summary>
        /// Gets or sets the name of the skill.
        /// </summary>
        string? Name { get; set; }

        /// <summary>
        /// Performs the skill action.
        /// </summary>
        /// <param name="actor">The one who performs the skill.</param>
        /// <param name="target">The target of the skill.</param>
        abstract void Act(UserData actor, UserData? target);
    }
}
