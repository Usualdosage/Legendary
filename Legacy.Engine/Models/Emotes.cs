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
            { "agree", new Emote("You agree completely.", "{0} nods in complete agreement.") },
            { "blink", new Emote("You blink slowly.", "{0} blinks slowly.") },
            { "buff", new Emote("You buff your nails on your cloak.", "{0} buffs their nails on their cloak.") },
            { "cackle", new Emote("You cackle with glee!", "{0} cackles with glee!") },
            { "chortle", new Emote("You chortle mischieviously.", "{0} chortles mischieviously.") },
            { "chuckle", new Emote("You chuckle.", "{0} chuckles lightly.") },
            { "fart", new Emote("You fart loudly.", "{0} farts loudly. A brown cloud fills the room.") },
            { "frown", new Emote("You frown.", "{0} frowns disapprovingly.") },
            { "gag", new Emote("You gag, suppressing the urge to vomit.", "{0} gags audibly, and looks ill.") },
            { "gasp", new Emote("You gasp in shock.", "{0} gasps loudly.") },
            { "giggle", new Emote("You giggle.", "{0} giggles.") },
            { "grin", new Emote("You grin.", "{0} grins.") },
            { "groan", new Emote("You groan.", "{0} groans.") },
            { "grumble", new Emote("You grumble, annoyed.", "{0} grumbles loudly.") },
            { "hiccup", new Emote("*HIC*", "{0} hiccups.") },
            { "laugh", new Emote("You laugh.", "{0} laughs.") },
            { "lick", new Emote("You lick your lips.", "{0} licks their lips.") },
            { "lol", new Emote("You laugh out loud!", "{0} laughs out loud!") },
            { "moan", new Emote("You moan loudly.", "{0} moans loudly.") },
            { "mutter", new Emote("You mutter under your breath.", "{0} mutters under their breath.") },
            { "nod", new Emote("You nod.", "{0} nods.") },
            { "ponder", new Emote("You ponder the idea.", "{0} ponders deeply.") },
            { "rofl", new Emote("You laugh so hard you almost fall down.", "{0} laughs so hard they almost fall down.") },
            { "shrug", new Emote("You shrug.", "{0} shrugs.") },
            { "sigh", new Emote("You sigh.", "{0} sighs.") },
            { "smile", new Emote("You smile.", "{0} smiles.") },
            { "smirk", new Emote("You smirk.", "{0} smirks.") },
            { "snicker", new Emote("You snicker.", "{0} snickers wickedly.") },
            { "snore", new Emote("You let out a loud snore.", "{0} lets out a loud snore.") },
            { "snort", new Emote("You snort.", "{0} snorts derisively.") },
            { "tongue", new Emote("You stick out your tongue.", "{0} sticks out their tongue.") },
            { "twiddle", new Emote("You twiddle your thumbs.", "{0} twiddles their thumbs.") },
            { "whine", new Emote("You whine loudly to anyone who will liste.", "{0} whines pitifully to anyone who will listen.") },
            { "wince", new Emote("You wince painfully.", "{0} winces painfully.") },
            { "wink", new Emote("You wink.", "{0} winks.") },
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

        /// <summary>
        /// Gets an emote by index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>Emote.</returns>
        public static Emote? Get(int index)
        {
            var actionArray = Actions.ToArray();

            var emote = actionArray[index];

            return Actions[emote.Key];
        }
    }
}
