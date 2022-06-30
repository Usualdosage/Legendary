// <copyright file="ActionHelper.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Helpers
{
    using System;
    using Legendary.Core.Contracts;
    using Legendary.Engine.Contracts;

    /// <summary>
    /// Helper for creating instances of skills and spells by reflection.
    /// </summary>
    public class ActionHelper
    {
        private readonly ICommunicator communicator;
        private readonly IRandom random;
        private readonly Combat combat;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionHelper"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="combat">The combat generator.</param>
        public ActionHelper(ICommunicator communicator, IRandom random, Combat combat)
        {
            this.communicator = communicator;
            this.random = random;
            this.combat = combat;
        }

        /// <summary>
        /// Creates an instance of a skill or spell from the name.
        /// </summary>
        /// <param name="fullNamespace">The namespace of the action.</param>
        /// <param name="name">The name of the skill.</param>
        /// <typeparam name="T">The type (skill or spell).</typeparam>
        /// <returns>The IAction.</returns>
        public IAction? CreateActionInstance<T>(string fullNamespace, string name)
            where T : IAction
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }
            else
            {
                try
                {
                    Type? type = Type.GetType($"{fullNamespace}.{name}, Legendary.Engine");

                    if (type != null)
                    {
                        var instance = Activator.CreateInstance(type, this.communicator, this.random, this.combat);

                        if (instance is not null and T)
                        {
                            return (T)instance;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                catch
                {
                    throw;
                }
            }
        }
    }
}