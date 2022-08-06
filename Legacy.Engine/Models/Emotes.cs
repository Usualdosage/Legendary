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
        /// <summary>
        /// All available "built in" emotes.
        /// </summary>
        public static readonly Dictionary<string, Emote> Actions = new ()
        {
            { "giggle", new Emote("You giggle.", "{0} giggles.") },
            { "wince", new Emote("You wince painfully.", "{0} winces painfully.") },
            { "blink", new Emote("You blink slowly.", "{0} blinks slowly.") },
            { "chuckle", new Emote("You chuckle.", "{0} chuckles lightly.") },
            { "grin", new Emote("You grin.", "{0} grins.") },
            { "groan", new Emote("You groan.", "{0} groans.") },
            { "moan", new Emote("You moan loudly.", "{0} moans loudly.") },
            { "hiccup", new Emote("*HIC*", "{0} hiccups.") },
            { "laugh", new Emote("You laugh.", "{0} laughs.") },
            { "twiddle", new Emote("You twiddle your thumbs.", "{0} twiddles their thumbs.") },
            { "smile", new Emote("You smile.", "{0} smiles.") },
            { "wink", new Emote("You wink.", "{0} winks.") },
            { "smirk", new Emote("You smirk.", "{0} smirks.") },
            { "chortle", new Emote("You chortle mischieviously.", "{0} chortles mischieviously.") },
            { "snicker", new Emote("You snicker.", "{0} snickers wickedly.") },
            { "tongue", new Emote("You stick out your tongue.", "{0} sticks out their tongue.") },
            { "snort", new Emote("You snort.", "{0} snorts derisively.") },
            { "snore", new Emote("You let out a loud snore.", "{0} lets out a loud snore.") },
            { "yawn", new Emote("You yawn.", "{0} yawns.") },
            { "sigh", new Emote("You sigh.", "{0} sighs.") },
            { "fart", new Emote("You fart loudly.", "{0} farts loudly. A brown cloud fills the room.") },
            { "shrug", new Emote("You shrug.", "{0} shrugs.") },
            { "nod", new Emote("You nod.", "{0} nods.") },
            { "frown", new Emote("You frown.", "{0} frowns disapprovingly.") },
            { "mutter", new Emote("You mutter under your breath.", "{0} mutters under their breath.") },
            { "cackle", new Emote("You cackle with glee!", "{0} cackles with glee!") },
            { "lol", new Emote("You laugh out loud!", "{0} laughs out loud!") },
            { "rofl", new Emote("You laugh so hard you almost fall down.", "{0} laughs so hard they almost fall down.") },
            { "lick", new Emote("You lick your lips.", "{0} licks their lips.") },
            { "ponder", new Emote("You ponder the idea.", "{0} ponders deeply.") },
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
