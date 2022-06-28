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
        public Action(ICommunicator communicator, IRandom random)
        {
            this.Communicator = communicator;
            this.Random = random;
            this.LanguageGenerator = new LanguageGenerator(random);
        }

        /// <inheritdoc/>
        public string? Name { get; set; }

        /// <inheritdoc/>
        public bool IsAffect { get; set; }

        /// <inheritdoc/>
        public int? AffectDuration { get; set; }

        /// <inheritdoc/>
        public abstract ActionType ActionType { get; }

        /// <inheritdoc/>
        public DamageType DamageType { get; set; }

        /// <inheritdoc/>
        public int ManaCost { get; set; }

        /// <summary>
        /// Gets the communicator.
        /// </summary>
        public ICommunicator Communicator { get; private set; }

        /// <summary>
        /// Gets the random number generator.
        /// </summary>
        public IRandom Random { get; private set; }

        /// <summary>
        /// Gets the language generator.
        /// </summary>
        public LanguageGenerator LanguageGenerator { get; private set; }

        /// <inheritdoc/>
        public abstract Task PreAction(UserData actor, UserData? target, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract Task Act(UserData actor, UserData? target, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract Task PostAction(UserData actor, UserData? target, CancellationToken cancellationToken = default);
    }
}
