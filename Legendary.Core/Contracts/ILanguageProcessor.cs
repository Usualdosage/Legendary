// <copyright file="ILanguageProcessor.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Core.Contracts
{
    using System.Threading.Tasks;
    using Legendary.Core.Models;

    /// <summary>
    /// Processes character speech and returns responses from mobiles.
    /// </summary>
    public interface ILanguageProcessor
    {
        /// <summary>
        /// Gets a message from the bot.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="mobile">The mobile.</param>
        /// <param name="input">The message input.</param>
        /// <param name="situation">The situation to train the AI on.</param>
        /// <returns>The result message.</returns>
        Task<string?> Process(Character character, Mobile mobile, string input, string situation);

        /// <summary>
        /// If a mob doesn't perform a verbal response, it may execute an emote.
        /// </summary>
        /// <param name="character">The actor.</param>
        /// <param name="mobile">The target.</param>
        /// <param name="input">The input.</param>
        /// <returns>String.</returns>
        string? ProcessEmote(Character character, Mobile mobile, string input);
    }
}