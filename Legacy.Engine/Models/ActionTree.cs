// <copyright file="ActionTree.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using Legendary.Core.Contracts;
    using Legendary.Engine.Contracts;

    /// <summary>
    /// Abstract implementation of IActionTree contract.
    /// </summary>
    public abstract class ActionTree : IActionTree
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionTree"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        /// <param name="random">The random generator.</param>
        /// <param name="combat">The combat engine.</param>
        public ActionTree(ICommunicator communicator, IRandom random, Combat combat)
        {
            this.Communicator = communicator;
            this.Random = random;
            this.Combat = combat;
        }

        /// <inheritdoc/>
        public abstract string Name { get; }

        /// <inheritdoc/>
        public abstract Dictionary<IAction, int> Group1 { get; }

        /// <inheritdoc/>
        public abstract Dictionary<IAction, int> Group2 { get; }

        /// <inheritdoc/>
        public abstract Dictionary<IAction, int> Group3 { get; }

        /// <inheritdoc/>
        public abstract Dictionary<IAction, int> Group4 { get; }

        /// <inheritdoc/>
        public abstract Dictionary<IAction, int> Group5 { get; }

        /// <summary>
        /// Gets the communicator.
        /// </summary>
        protected ICommunicator Communicator { get; private set; }

        /// <summary>
        /// Gets the random generator.
        /// </summary>
        protected IRandom Random { get; private set; }

        /// <summary>
        /// Gets the combat engine.
        /// </summary>
        protected Combat Combat { get; private set; }

        /// <summary>
        /// When passed a list of proficiency names, gets a total count of matching items in the group.
        /// </summary>
        /// <param name="proficiencyNames">The list of proficiencies.</param>
        /// <returns>Int.</returns>
        public virtual int GetMatches(List<string> proficiencyNames)
        {
            int g1Total = this.Group1.Count(a => proficiencyNames.Any(s => a.Key.Name.ToLower() == s.ToLower()));
            int g2Total = this.Group1.Count(a => proficiencyNames.Any(s => a.Key.Name.ToLower() == s.ToLower()));
            int g3Total = this.Group1.Count(a => proficiencyNames.Any(s => a.Key.Name.ToLower() == s.ToLower()));
            int g4Total = this.Group1.Count(a => proficiencyNames.Any(s => a.Key.Name.ToLower() == s.ToLower()));
            int g5Total = this.Group1.Count(a => proficiencyNames.Any(s => a.Key.Name.ToLower() == s.ToLower()));

            return g1Total + g2Total + g3Total + g4Total + g5Total;
        }
    }
}