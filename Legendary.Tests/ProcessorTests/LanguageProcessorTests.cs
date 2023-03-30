// <copyright file="LanguageProcessorTests.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Tests.ProcessorTests
{
    using System;
    using Legendary.Core.Models;
    using Legendary.Engine;
    using Legendary.Engine.Processors;

    /// <summary>
    /// Tests of the LanguageProcessor class.
    /// </summary>
    public class LanguageProcessorTests
    {
        /// <summary>
        /// Test of the CleanOutput method.
        /// </summary>
        /// <param name="aiResponseMessage">The response message from the AI.</param>
        /// <param name="expectedMessageCount">The number of expected messages returned.</param>
        [Theory]
        [InlineData("Testmobile: That is an interesting comment.", 1)]
        [InlineData("Testmobile: This is a test comment *smiles sweetly*.", 2)]
        [InlineData("Testmobile: This is a test comment *smiles sweetly*. You're in for some real excitement!", 3)]
        [InlineData("Testmobile: This is a test comment *smiles sweetly*. You're in for some real excitement! *cackles gleefully*", 4)]
        [InlineData("This is a test comment *smiles sweetly*. You're in for some real excitement!", 3)]
        [InlineData("This is a test comment *smiles sweetly*. You're in for some real excitement! *cackles gleefully*", 4)]
        [InlineData("Testmobile: *throws back his head and laughs*.", 1)]
        [InlineData("EMOTE: smiles at you", 1)]
        [InlineData("*giggles*", 1)]
        [InlineData("EMOTE: *smiles at you*", 1)]
        [InlineData("That is an interesting comment.", 1)]
        public void CleanOutputTest(string aiResponseMessage, int expectedMessageCount)
        {
            var persona = new Persona() { Name = "Testmobile" };
            var mobile = new Mobile() { FirstName = "Testmobile", LastName = "Testerson" };

            var messages = LanguageProcessor.CleanOutput(aiResponseMessage, persona, mobile);

            Assert.NotNull(messages);
            Assert.NotEmpty(messages);
            Assert.True(messages.Length == expectedMessageCount);
        }
    }
}