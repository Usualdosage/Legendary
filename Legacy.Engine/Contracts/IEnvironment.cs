// <copyright file="IEnvironment.cs" company="Legendary">
//  Copyright © 2022 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Engine.Contracts
{	
    using System.Threading.Tasks;

    public interface IEnvironment
	{
        /// <summary>
        /// Processes changes to the connected user's environment each game hour.
        /// </summary>
        /// <param name="gameTicks"></param>
        /// <param name="gameHour"></param>
        /// <returns></returns>
        Task ProcessEnvironmentChanges(int gameTicks, int gameHour);
    }
}

