// <copyright file="Skill.cs" company="Legendary">
//  Copyright © 2021-2022 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Engine.Models
{
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;

    public abstract class Spell : ISpell
	{
        public abstract void Cast(UserData actor, UserData? target);

        public string? Name { get; set; }
    }
}

