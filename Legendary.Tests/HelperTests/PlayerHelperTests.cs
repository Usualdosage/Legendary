// <copyright file="PlayerHelperTests.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Tests.HelperTests
{
    using System;
    using Legendary.Core.Models;
    using Legendary.Engine.Helpers;

    /// <summary>
    /// Tests for the Player Helper class.
    /// </summary>
    public class PlayerHelperTests
    {
        /// <summary>
        /// Tests to calculate experience from one level to another.
        /// </summary>
        [Fact]
        public void CalculateExperienceToLevelTests()
        {
            var test = PlayerHelper.GetTotalExperienceRequired(1, 2, 0);

            // Level 1 to 2 Human should require 1500 experience points
            Assert.True(test == 1500);

            // Level 1 to 10 Human should require 57665 total experience points.
            var test2 = PlayerHelper.GetTotalExperienceRequired(1, 10, 0);

            Assert.True(test2 == 54204);

            // Level 1 to 50 Human should require 150,091,072 total experience points
            var test3 = PlayerHelper.GetTotalExperienceRequired(1, 50, 0);

            Assert.True(test3 == 150091072);

            // Level 1 to 100 Human should require 1,538,508,434,532 total experience points
            var test4 = PlayerHelper.GetTotalExperienceRequired(1, 100, 0);

            Assert.True(test4 == 1538508434532);

            // Level 1 to 2 Avian should require 2,000 total experience points
            var test5 = PlayerHelper.GetTotalExperienceRequired(1, 2, 500);

            Assert.True(test5 == 2000);

            // Level 1 to 10 Avian should require 58,704 total experience points
            var test6 = PlayerHelper.GetTotalExperienceRequired(1, 10, 500);

            Assert.True(test6 == 58704);

            // Level 1 to 50 Avian should require 150,115,572 total experience points
            var test7 = PlayerHelper.GetTotalExperienceRequired(1, 50, 500);

            Assert.True(test7 == 150115572);
        }
    }
}