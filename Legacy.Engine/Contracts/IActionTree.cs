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
        /// Gets the skills or spells available in group 1.
        /// </summary>
        public Dictionary<IAction, int> Group1 { get; }

        /// <summary>
        /// Gets the skills or spells available in group 2.
        /// </summary>
        public Dictionary<IAction, int> Group2 { get; }

        /// <summary>
        /// Gets the skills or spells available in group 3.
        /// </summary>
        public Dictionary<IAction, int> Group3 { get; }

        /// <summary>
        /// Gets the skills or spells available in group 4.
        /// </summary>
        public Dictionary<IAction, int> Group4 { get; }

        /// <summary>
        /// Gets the skills or spells available in group 5.
        /// </summary>
        public Dictionary<IAction, int> Group5 { get; }
    }
}
