// <copyright file="Maggie.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Programs.Mobiles
{
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Engine.Models;
    using Legendary.Engine.Processors;

    /// <summary>
    /// MIRP script for Maggie (mobile).
    /// </summary>
    public class Maggie : BaseMIRP
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Maggie"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        /// <param name="world">The world.</param>
        /// <param name="actionProcessor">The action processor.</param>
        /// <param name="combatProcessor">The combat processor.</param>
        /// <param name="awardProcessor">The award processor.</param>
        /// <param name="skillProcessor">The skill processor.</param>
        /// <param name="spellProcessor">The spell processor.</param>
        public Maggie(ICommunicator communicator, IWorld world, ActionProcessor actionProcessor, CombatProcessor combatProcessor, AwardProcessor awardProcessor, SkillProcessor skillProcessor, SpellProcessor spellProcessor)
            : base(communicator, world, actionProcessor, combatProcessor, awardProcessor, skillProcessor, spellProcessor)
        {
            this.PlayerEnter += this.Maggie_PlayerEnter;
            this.PlayerLeave += this.Maggie_PlayerLeave;
        }

        private void Maggie_PlayerEnter(object? sender, Types.MIRPEventArgs e)
        {
            if (sender is Mobile mobile)
            {
                if (e.Player != null)
                {
                    this.Communicator.SendToRoom(mobile.Location, $"Hello, {e.Player.FirstName}, welcome to the Red Dragon Inn!");
                }
            }
        }

        private void Maggie_PlayerLeave(object? sender, Types.MIRPEventArgs e)
        {
            if (sender is Mobile mobile)
            {
                if (e.Player != null)
                {
                    this.Communicator.SendToRoom(mobile.Location, $"Catch ya later, {e.Player.FirstName}!");
                }
            }
        }
    }
}
