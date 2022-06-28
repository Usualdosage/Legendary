// <copyright file="Skill.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Models
{
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Engine.Contracts;

    /// <summary>
    /// Abstract implementation of an ISkill contract.
    /// </summary>
    public abstract class Skill : ISkill
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Skill"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        public Skill(ICommunicator communicator)
        {
            this.Communicator = communicator;
        }

        /// <summary>
        /// Gets the communicator.
        /// </summary>
        public ICommunicator Communicator { get; private set; }

        /// <inheritdoc/>
        public string? Name { get; set; }

        /// <inheritdoc/>
        public abstract void Act(UserData actor, UserData? target);
    }
}
