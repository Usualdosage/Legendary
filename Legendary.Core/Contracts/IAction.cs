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
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core.Models;
    using Legendary.Core.Types;

    /// <summary>
    /// Defines a user action as it relates to using a skill or casting a spell.
    /// </summary>
    public interface IAction
    {
        /// <summary>
        /// Gets or sets the name of the action.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets the action type.
        /// </summary>
        ActionType ActionType { get; }

        /// <summary>
        /// Gets or sets the damage type.
        /// </summary>
        DamageType DamageType { get; set; }

        /// <summary>
        /// Gets or sets the damage noun (e.g. "blast", "punch", etc).
        /// </summary>
        string? DamageNoun { get; set; }

        /// <summary>
        /// Gets or sets the mana cost to use the action.
        /// </summary>
        int ManaCost { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this action can be invoked, or is automatic.
        /// </summary>
        bool CanInvoke { get; set; }

        /// <summary>
        /// Gets or sets the number of dice to roll.
        /// </summary>
        int HitDice { get; set; }

        /// <summary>
        /// Gets or sets the type of dice to roll.
        /// </summary>
        int DamageDice { get; set; }

        /// <summary>
        /// Gets or sets the damage modifier (bonus added to damage roll).
        /// </summary>
        int DamageModifier { get; set; }

        /// <summary>
        /// Gets a value indicating whether this action is an affect or not.
        /// </summary>
        bool IsAffect { get; }

        /// <summary>
        /// Gets or sets the duration the player is affected by this action.
        /// </summary>
        int? AffectDuration { get; set; }

        /// <summary>
        /// Determines if the action was executed successfully. This should be called BEFORE PreAction.
        /// </summary>
        /// <param name="proficiency">The action proficiency.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        Task<bool> IsSuccess(int proficiency, CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs the skill action.
        /// </summary>
        /// <param name="actor">The one who performs the skill.</param>
        /// <param name="target">The target of the skill.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        abstract Task Act(Character actor, Character? target, CancellationToken cancellationToken = default);

        /// <summary>
        /// Each time the action is used, checks the improvement of the action.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        Task CheckImprove(Character actor, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets or sets the action to execute before the main action effect.
        /// </summary>
        /// <param name="actor">The one who performs the skill.</param>
        /// <param name="target">The target of the skill.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        Task PreAction(Character actor, Character? target, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets or sets the action to execute after the main action effect.
        /// </summary>
        /// <param name="actor">The one who performs the skill.</param>
        /// <param name="target">The target of the skill.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        Task PostAction(Character actor, Character? target, CancellationToken cancellationToken = default);

        /// <summary>
        /// Processes an effect each tick.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <param name="effect">The effect to process.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        Task OnTick(Character actor, Effect effect, CancellationToken cancellationToken = default);

        /// <summary>
        /// Processes an effect each viotick.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <param name="effect">The effect to process.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        Task OnVioTick(Character actor, Effect effect, CancellationToken cancellationToken = default);
    }
}
