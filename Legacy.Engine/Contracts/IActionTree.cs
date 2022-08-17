// <copyright file="IActionTree.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Contracts
{
    using System;
    using System.Collections.Generic;
    using Legendary.Core.Contracts;
    using Legendary.Core.Types;

    /// <summary>
    /// Implementation contract for a skill or a spell tree.
    /// </summary>
    public interface IActionTree
    {
        /// <summary>
        /// Gets the name of the tree.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the type of school this skill or spell tree can be learned in.
        /// </summary>
        public SchoolType SchoolType { get; }

        /// <summary>
        /// Gets the skills or spells available in group 1.
        /// </summary>
        public List<IAction> Group1 { get; }

        /// <summary>
        /// Gets the skills or spells available in group 2.
        /// </summary>
        public List<IAction> Group2 { get; }

        /// <summary>
        /// Gets the skills or spells available in group 3.
        /// </summary>
        public List<IAction> Group3 { get; }

        /// <summary>
        /// Gets the skills or spells available in group 4.
        /// </summary>
        public List<IAction> Group4 { get; }

        /// <summary>
        /// Gets the skills or spells available in group 5.
        /// </summary>
        public List<IAction> Group5 { get; }
    }
}
