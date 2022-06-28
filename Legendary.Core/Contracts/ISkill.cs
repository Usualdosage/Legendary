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

    public interface ISkill
	{
        /// <summary>
        /// Performs the skill action.
        /// </summary>
        /// <param name="actor">The one who performs the skill.</param>
        /// <param name="target">The target of the skill.</param>
        abstract Task Act(UserData actor, UserData? target, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets or sets the name of the skill.
        /// </summary>
        string? Name { get; set; }
    }
}

