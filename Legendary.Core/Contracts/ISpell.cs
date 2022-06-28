// <copyright file="ISkill.cs" company="Legendary">
//  Copyright © 2021-2022 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Core.Contracts
{
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core.Models;
    using Legendary.Core.Types;

    public interface ISpell
	{
        /// <summary>
        /// Gets the name of the spell.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the damage noun of the spell.
        /// </summary>
        string DamageNoun { get; }

        /// <summary>
        /// Gets the mana cost of the spell.
        /// </summary>
        int ManaCost { get; }

        /// <summary>
        /// Gets the type of the spell.
        /// </summary>
        SpellType SpellType { get; }

        /// <summary>
        /// Gets the damage type of the spell.
        /// </summary>
        DamageType DamageType { get; }

        /// <summary>
        /// Casts the spell.
        /// </summary>
        /// <param name="actor">The caster.</param>
        /// <param name="target">The target.</param>
        abstract Task Cast(UserData actor, UserData? target, CancellationToken cancellationToken = default);

        /// <summary>
        /// Determines whether or not the spell can be cast (on the target).
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> CanCast(UserData actor, UserData? target, CancellationToken cancellationToken = default);
    }
}

