// <copyright file="IProcessor.cs" company="Legendary">
//  Copyright © 2021-2022 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Engine.Contracts
{
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
        /// <returns>Task.</returns>
        Task ProcessMessage(UserData user, string? input);

        /// <summary>
        /// Shows/updates the player information box.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Task.</returns>
        Task ShowPlayerInfo(UserData user);
    }
}



