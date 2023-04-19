// <copyright file="ICompanionProcessor.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Web.Contracts
{
    using System.Threading.Tasks;

    /// <summary>
    /// Implementastion of a Companion Processor.
    /// </summary>
    public interface ICompanionProcessor
    {
        /// <summary>
        /// Processes an input message using ChatGPT.
        /// </summary>
        /// <param name="userName">The username.</param>
        /// <param name="persona">The persona.</param>
        /// <param name="message">The input.</param>
        /// <returns>Output from AI.</returns>
        Task<string> ProcessChat(string userName, string persona, string message);
    }
}
