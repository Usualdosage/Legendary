// <copyright file="Spell.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Models
{
    using System;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Core.Types;
    using Legendary.Engine.Contracts;

    /// <summary>
    /// Abstract implementation of an ISpell contract.
    /// </summary>
    public abstract class Spell : IAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Spell"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        public Spell(ICommunicator communicator)
        {
            this.Communicator = communicator;
        }

        /// <summary>
        /// Gets the communicator.
        /// </summary>
        public ICommunicator Communicator { get; private set; }

        /// <inheritdoc/>
        public ActionType ActionType => ActionType.Spell;

        /// <inheritdoc/>
        public string? Name { get; set; }

        /// <inheritdoc/>
        public Action? PreAction { get; set; }

        /// <inheritdoc/>
        public Action? PostAction { get; set; }

        /// <inheritdoc/>
        public bool IsAffect { get; set; }

        /// <inheritdoc/>
        public int? AffectDuration { get; set; }

        /// <inheritdoc/>
        public abstract void Act(UserData actor, UserData? target);
    }
}
