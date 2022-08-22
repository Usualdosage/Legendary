// <copyright file="Parry.cs" company="Legendary™">
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

    /// <summary>
    /// Allows a player to parry.
    /// </summary>
    public class Parry : Skill
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Parry"/> class.
        /// </summary>
        /// <param name="communicator">ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="combat">The combat generator.</param>
        public Parry(ICommunicator communicator, IRandom random, Combat combat)
            : base(communicator, random, combat)
        {
            this.Name = "Parry";
            this.ManaCost = 0;
            this.CanInvoke = false;
            this.IsAffect = false;
            this.AffectDuration = 0;
            this.DamageModifier = 0;
            this.HitDice = 0;
            this.DamageDice = 0;
        }
    }
}
