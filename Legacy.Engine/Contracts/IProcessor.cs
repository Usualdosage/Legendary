// <copyright file="IProcessor.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Contracts
{
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Engine.Models;

    /// <summary>
    /// Implementation contract for a communication processor.
    /// </summary>
    public interface IProcessor
    {
        /// <summary>
        /// Handles the incoming message from the socket, and parses the arguments.
        /// </summary>
        /// <param name="user">The user data.</param>
        /// <param name="input">The input from the socket.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        Task ProcessMessage(UserData user, string? input, CancellationToken cancellationToken = default);

        /// <summary>
        /// Shows/updates the player information box.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        Task ShowPlayerInfo(UserData user, CancellationToken cancellationToken = default);
    }
}
