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
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Core.Types;
    using Legendary.Engine.Contracts;

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
        public virtual async Task<bool> IsSuccess(UserData actor, UserData? target, int proficiency, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                // Even with mastery, there is always a 1% chance of failure.
                var result = this.Random.Next(1, 99);
                return result < proficiency;
            });
        }

        /// <inheritdoc/>
        public virtual async Task PreAction(UserData actor, UserData? target, CancellationToken cancellationToken = default)
        {
            var spellWords = this.LanguageGenerator.BuildSentence(this.Name);

            if (target == null)
            {
                await this.Communicator.SendToPlayer(actor.Connection, $"You extend your hand and utter the word, '{spellWords}'.", cancellationToken);
                await this.Communicator.SendToRoom(actor.Character, actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} extends {actor.Character.Pronoun} hand and utters the words, '{spellWords}'.", cancellationToken);
            }
            else
            {
                await this.Communicator.SendToPlayer(actor.Connection, $"You extend your hand and utter the word, '{spellWords}' at {target?.Character.FirstName}.", cancellationToken);
                await this.Communicator.SendToRoom(actor.Character, actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName} extends {actor.Character.Pronoun} hand toward {target?.Character.FirstName} and utters the word, '{spellWords}'", cancellationToken);
            }
        }

        /// <inheritdoc/>
        public abstract Task Act(UserData actor, UserData? target, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public virtual Task PostAction(UserData actor, UserData? target, CancellationToken cancellationToken = default)
        {
            actor.Character.Mana.Current -= this.ManaCost;
            return Task.CompletedTask;
        }
    }
}
