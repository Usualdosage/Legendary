// <copyright file="IServer.cs" company="Legendary">
//  Copyright © 2021 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Networking.Contracts
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Implementation contract for a server.
    /// </summary>
    public interface IServer
    {
        /// <summary>
        /// Asynchronously invokes a websocket request. Removes and adds sockets to a dictionary.
        /// </summary>
        /// <param name="context">The HttpContext.</param>
        /// <returns>Task.</returns>
        Task Invoke(HttpContext context);
    }
}