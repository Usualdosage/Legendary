// <copyright file="Emotes.cs" company="Legendary">
//  Copyright © 2021 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Engine.Models
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Registers available "emotes" to users.
    /// </summary>
    public class Emotes
    {
        private static readonly Dictionary<string, Emote> Actions = new()
        {
            { "giggle", new Emote("You giggle.", "{0} giggles.") },
            { "grin", new Emote("You grin.", "{0} grins.") },
            { "laugh", new Emote("You laugh.", "{0} laughs.") },
            { "smile", new Emote("You smile.", "{0} smiles.") },
            { "smirk", new Emote("You smirk.", "{0} smirks.") },
            { "snicker", new Emote("You snicker.", "{0} snickers wickedly.") },
            { "snort", new Emote("You snort.", "{0} snorts derisively.") },
            { "yawn", new Emote("You yawn.", "{0} yawns.") },
        };

        /// <summary>
        /// Gets the emote associated with the action.
        /// </summary>
        /// <param name="action">The string.</param>
        /// <returns>Emote.</returns>
        public static Emote? Get(string action)
        {
            return Actions.FirstOrDefault(a => a.Key.ToLower() == action.ToLower()).Value;
        }
    }
}



