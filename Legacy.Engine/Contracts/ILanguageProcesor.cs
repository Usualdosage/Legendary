﻿// <copyright file="ILanguageProcesor.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Contracts
{
    using System.Threading.Tasks;
    using Legendary.Core.Models;

    /// <summary>
    /// Processes character speech and returns responses from mobiles.
    /// </summary>
    public interface ILanguageProcesor
    {
        /// <summary>
        /// Gets a message from the personality forge bot.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="mobile">The mobile.</param>
        /// <param name="message">The message input.</param>
        /// <returns>The result message.</returns>
        string? Process(Character character, Mobile mobile, string message);
    }
}