// <copyright file="IAction.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Core.Contracts
{
    using System;
    using Legendary.Core.Models;
    using Legendary.Core.Types;

    /// <summary>
    /// Defines a user action as it relates to using a skill or casting a spell.
    /// </summary>
    public interface IAction
    {
        /// <summary>
        /// Gets or sets the name of the skill.
        /// </summary>
        string? Name { get; set; }

        /// <summary>
        /// Gets the action type.
        /// </summary>
        ActionType ActionType { get; }

        /// <summary>
        /// Gets a value indicating whether this action is an affect or not.
        /// </summary>
        bool IsAffect { get; }

        /// <summary>
        /// Gets or sets the duration the player is affected by this action.
        /// </summary>
        int? AffectDuration { get; set; }

        /// <summary>
        /// Gets or sets the action to perform when before Act() is triggered.
        /// </summary>
        Action? PreAction { get; set; }

        /// <summary>
        /// Gets or sets the action to perform when after Act() is triggered.
        /// </summary>
        Action? PostAction { get; set; }

        /// <summary>
        /// Performs the skill action.
        /// </summary>
        /// <param name="actor">The one who performs the skill.</param>
        /// <param name="target">The target of the skill.</param>
        abstract void Act(UserData actor, UserData? target);
    }
}
