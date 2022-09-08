// <copyright file="Gnome.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Models.Skills
{
    using Legendary.Core.Contracts;
    using Legendary.Engine.Contracts;

    /// <summary>
    /// Allows a player to speak Gnome.
    /// </summary>
    public class Gnome : Skill
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Gnome"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat generator.</param>
        public Gnome(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, Combat combat)
            : base(communicator, random, world, logger, combat)
        {
            this.Name = "Gnome";
            this.ManaCost = 0;
        }
    }
}
