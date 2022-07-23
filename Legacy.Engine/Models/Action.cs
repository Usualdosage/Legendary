// <copyright file="Action.cs" company="Legendary™">
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
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Core.Types;
    using Legendary.Engine.Generators;

    /// <summary>
    /// Abstract implementation of an action contract.
    /// </summary>
    public abstract class Action : IAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Action"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="combat">The combat generator.</param>
        public Action(ICommunicator communicator, IRandom random, Combat combat)
        {
            this.Communicator = communicator;
            this.Random = random;
            this.LanguageGenerator = new LanguageGenerator(random);
            this.Combat = combat;
        }

        /// <inheritdoc/>
        public string Name { get; set; } = string.Empty;

        /// <inheritdoc/>
        public string? DamageNoun { get; set; }

        /// <inheritdoc/>
        public bool IsAffect { get; set; }

        /// <inheritdoc/>
        public int? AffectDuration { get; set; }

        /// <inheritdoc/>
        public bool CanInvoke { get; set; }

        /// <inheritdoc/>
        public abstract ActionType ActionType { get; }

        /// <inheritdoc/>
        public DamageType DamageType { get; set; }

        /// <inheritdoc/>
        public int ManaCost { get; set; }

        /// <inheritdoc/>
        public int HitDice { get; set; }

        /// <inheritdoc/>
        public int DamageDice { get; set; }

        /// <inheritdoc/>
        public int DamageModifier { get; set; }

        /// <summary>
        /// Gets the communicator.
        /// </summary>
        public ICommunicator Communicator { get; private set; }

        /// <summary>
        /// Gets the combat generator.
        /// </summary>
        public Combat Combat { get; private set; }

        /// <summary>
        /// Gets the random number generator.
        /// </summary>
        public IRandom Random { get; private set; }

        /// <summary>
        /// Gets the language generator.
        /// </summary>
        public LanguageGenerator LanguageGenerator { get; private set; }

        /// <inheritdoc/>
        public virtual async Task<bool> IsSuccess(int proficiency, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                // Even with mastery, there is always a 1% chance of failure.
                var result = this.Random.Next(1, 99);
                return result < proficiency;
            });
        }

        /// <inheritdoc/>
        public virtual async Task CheckImprove(Character actor, CancellationToken cancellationToken = default)
        {
            int maxImprove = (int)Math.Max(2, actor.Int.Current / 4);

            if (this.ActionType == ActionType.Skill)
            {
                var skillProficiency = actor.GetSkillProficiency(this.Name);

                if (skillProficiency != null)
                {
                    if (skillProficiency.Proficiency == 100)
                    {
                        return;
                    }

                    // The lower the skill percentage, the higher the modifier. So at 25%, the bonus is 25. At 50% it's 10, etc.
                    int modifier = (int)(1 / (skillProficiency.Proficiency / 5) * 100);

                    skillProficiency.Progress += this.Random.Next(0, maxImprove) + modifier;

                    if (skillProficiency.Progress >= 100)
                    {
                        skillProficiency.Proficiency += 1;
                        skillProficiency.Progress = 0;

                        if (skillProficiency.Proficiency == 100)
                        {
                            await this.Communicator.SendToPlayer(actor, $"You have now mastered [{this.Name}]!", cancellationToken);
                        }
                        else
                        {
                            await this.Communicator.SendToPlayer(actor, $"You have become better at {this.Name}!", cancellationToken);
                        }

                        await this.Communicator.SaveCharacter(actor);
                    }
                }
            }

            if (this.ActionType == ActionType.Spell)
            {
                var spellProficiency = actor.GetSpellProficiency(this.Name);

                if (spellProficiency != null)
                {
                    if (spellProficiency.Proficiency == 100)
                    {
                        return;
                    }

                    // The lower the spell percentage, the higher the modifier. So at 25%, the bonus is 25. At 50% it's 10, etc.
                    int modifier = (int)(1 / (spellProficiency.Proficiency / 5) * 100);

                    spellProficiency.Progress += this.Random.Next(0, maxImprove) + modifier;

                    if (spellProficiency.Progress >= 100)
                    {
                        spellProficiency.Proficiency += 1;
                        spellProficiency.Progress = 0;

                        if (spellProficiency.Proficiency == 100)
                        {
                            await this.Communicator.SendToPlayer(actor, $"You have now mastered [{this.Name}]!", cancellationToken);
                        }
                        else
                        {
                            await this.Communicator.SendToPlayer(actor, $"You have become better at {this.Name}!", cancellationToken);
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        public virtual Task PreAction(Character actor, Character? target, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual Task Act(Character actor, Character? target, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual Task PostAction(Character actor, Character? target, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
