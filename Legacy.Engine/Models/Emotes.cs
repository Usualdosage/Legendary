// <copyright file="Emotes.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
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
        private static readonly Dictionary<string, Emote> Actions = new ()
        {
            { "giggle", new Emote("You giggle.", "{0} giggles.") },
            { "grin", new Emote("You grin.", "{0} grins.") },
            { "laugh", new Emote("You laugh.", "{0} laughs.") },
            { "twiddle", new Emote("You twiddle your thumbs.", "{0} twiddles their thumbs.") },
            { "smile", new Emote("You smile.", "{0} smiles.") },
            { "wink", new Emote("You wink.", "{0} winks.") },
            { "smirk", new Emote("You smirk.", "{0} smirks.") },
            { "snicker", new Emote("You snicker.", "{0} snickers wickedly.") },
            { "snort", new Emote("You snort.", "{0} snorts derisively.") },
            { "yawn", new Emote("You yawn.", "{0} yawns.") },
            { "sigh", new Emote("You sigh.", "{0} sighs.") },
            { "shrug", new Emote("You shrug.", "{0} shrugs.") },
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
