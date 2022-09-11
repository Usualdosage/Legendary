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
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Generators;
    using Legendary.Engine.Processors;

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
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        public Action(ICommunicator communicator, IRandom random, IWorld world, ILogger logger,  Combat combat)
        {
            this.Communicator = communicator;
            this.Random = random;
            this.LanguageGenerator = new LanguageGenerator(random);
            this.Combat = combat;
            this.World = world;
            this.AwardProcessor = new AwardProcessor(communicator, world, logger, random, combat);
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
        /// Gets the award processor.
        /// </summary>
        public AwardProcessor AwardProcessor { get; private set; }

        /// <summary>
        /// Gets the combat generator.
        /// </summary>
        public Combat Combat { get; private set; }

        /// <summary>
        /// Gets the random number generator.
        /// </summary>
        public IRandom Random { get; private set; }

        /// <summary>
        /// Gets the world.
        /// </summary>
        public IWorld World { get; private set; }

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
            int maxImprove = (int)Math.Max(10, actor.Int.Current);

            if (this.ActionType == ActionType.Skill)
            {
                var skillProficiency = actor.GetSkillProficiency(this.Name);

                if (skillProficiency != null)
                {
                    if (skillProficiency.Proficiency == 100)
                    {
                        return;
                    }

                    skillProficiency.Progress += this.Random.Next(0, maxImprove);

                    if (skillProficiency.Progress >= 100)
                    {
                        skillProficiency.Proficiency += 1;
                        skillProficiency.Progress = 0;

                        if (skillProficiency.Proficiency == 100)
                        {
                            await this.Communicator.SendToPlayer(actor, $"You have now mastered [{this.Name}]!", cancellationToken);
                            actor.Experience += this.Random.Next(1000, 2000);
                            await this.AwardProcessor.GrantAward(8, actor, $"mastered {this.Name}", cancellationToken);
                        }
                        else
                        {
                            await this.Communicator.SendToPlayer(actor, $"You have become better at {this.Name}!", cancellationToken);
                            actor.Experience += this.Random.Next(100, 200);
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

                    spellProficiency.Progress += this.Random.Next(0, maxImprove);

                    if (spellProficiency.Progress >= 100)
                    {
                        spellProficiency.Proficiency += 1;
                        spellProficiency.Progress = 0;

                        if (spellProficiency.Proficiency == 100)
                        {
                            await this.Communicator.SendToPlayer(actor, $"You have now mastered [{this.Name}]!", cancellationToken);
                            actor.Experience += this.Random.Next(1000, 2000);
                            await this.AwardProcessor.GrantAward(9, actor, $"mastered {this.Name}", cancellationToken);
                        }
                        else
                        {
                            await this.Communicator.SendToPlayer(actor, $"You have become better at {this.Name}!", cancellationToken);
                            actor.Experience += this.Random.Next(100, 200);
                        }

                        await this.Communicator.SaveCharacter(actor);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public virtual Task PreAction(Character actor, Character? target, Item? targetItem, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual Task Act(Character actor, Character? target, Item? targetItem, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual async Task PostAction(Character actor, Character? target, Item? targetItem, CancellationToken cancellationToken = default)
        {
            await this.Communicator.SendGameUpdate(actor, null, null, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual Task OnTick(Character actor, Effect effect, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual Task OnVioTick(Character actor, Effect effect, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
