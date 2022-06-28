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
<<<<<<< HEAD
    using System;
=======
    using System.Threading;
    using System.Threading.Tasks;
>>>>>>> 4e33d3b (Checkpoint.)
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Core.Types;
    using Legendary.Engine.Contracts;

    /// <summary>
    /// Abstract implementation of an ISkill contract.
    /// </summary>
    public abstract class Skill : IAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Skill"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        public Skill(ICommunicator communicator)
        {
            this.Communicator = communicator;
        }

<<<<<<< HEAD
        /// <summary>
        /// Gets the communicator.
        /// </summary>
        public ICommunicator Communicator { get; private set; }
=======
        public abstract Task Act(UserData actor, UserData? target, CancellationToken cancellationToken = default);
>>>>>>> 4e33d3b (Checkpoint.)

        /// <inheritdoc/>
        public string? Name { get; set; }

        /// <inheritdoc/>
        public bool IsAffect { get; set; }

        /// <inheritdoc/>
        public int? AffectDuration { get; set; }

        /// <inheritdoc/>
        public ActionType ActionType => ActionType.Skill;

        /// <inheritdoc/>
        public Action? PreAction { get; set; }

        /// <inheritdoc/>
        public Action? PostAction { get; set; }

        /// <inheritdoc/>
        public abstract void Act(UserData actor, UserData? target);
    }
}
