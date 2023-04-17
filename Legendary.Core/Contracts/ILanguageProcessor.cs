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
    using System.Collections.Generic;
    using System.Threading;
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
        /// <param name="mobiles">The mobiles.</param>
        /// <param name="input">The message input.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result message.</returns>
        Task<(string[]?, Mobile?)> Process(Character character, List<Mobile> mobiles, string input, CancellationToken cancellationToken = default);

        /// <summary>
        /// If a mob doesn't perform a verbal response, it may execute an emote.
        /// </summary>
        /// <param name="character">The actor.</param>
        /// <param name="mobile">The target.</param>
        /// <param name="input">The input.</param>
        /// <returns>String.</returns>
        string? ProcessEmote(Character character, Mobile mobile, string input);

        /// <summary>
        /// Generates a DALL-E 2 image based on the character's long description.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <returns>The image URL.</returns>
        Task<string?> GenerateImage(Character character);
    }
}