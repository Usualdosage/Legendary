// <copyright file="ILanguageGenerator.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Core.Contracts
{
    /// <summary>
    /// Implementation contract for a language generator.
    /// </summary>
    public interface ILanguageGenerator
    {
        /// <summary>
        /// Builds a random sentence from a random sentence.
        /// </summary>
        /// <param name="sentence">The source sentence.</param>
        /// <returns>New sentence.</returns>
        string BuildSentence(string sentence);
    }
}