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
            { "agree", new Emote("You agree completely.", "{0} nods in complete agreement.", "{0} nods in agreement at {1}.", "You nod in agreement at {1}.") },
            { "beckon", new Emote("You beckon to follow.", "{0} beckons for you to follow.", "{0} beckons {1} to follow.", "You beckon {1} to follow.") },
            { "blink", new Emote("You blink slowly.", "{0} blinks slowly.", "{0} blinks slowly at {1}.", "You blink slowly at {1}.") },
            { "buff", new Emote("You buff your nails on your cloak.", "{0} buffs their nails on their cloak.", "{0} buffs their nails on their cloak, grinning at {1}.", "You buff your nails on your cloak, grinning at {1}.") },
            { "cackle", new Emote("You cackle with glee!", "{0} cackles with glee!", "{0} cackles delightfully at {1}.", "You cackle gleefully at {1}.") },
            { "chortle", new Emote("You chortle mischieviously.", "{0} chortles mischieviously.", "{0} chortles mischieviously at {1}.", "You chortle mischieviously at {1}.") },
            { "chuckle", new Emote("You chuckle.", "{0} chuckles lightly.", "{0} chuckles lightly at {1}.", "You chuckle lightly at {1}.") },
            { "dance", new Emote("You dance with yourself.", "{0} does some unusual dance moves.", "{0} grabs {1} and dances with them.", "You grab {1} and do a little dance with them.") },
            { "eyebrow", new Emote("You arch your eyebrow.", "{0} arches their eyebrow.", "{0} arches their eyebrow at {1}.", "You arch your eyebrow at {1}.") },
            { "fart", new Emote("You fart loudly.", "{0} farts loudly. A brown cloud fills the room.", "{0} farts on {1}. Gross!", "You are a disgusting person.") },
            { "frown", new Emote("You frown.", "{0} frowns disapprovingly.", "{0} frowns at {1}.", "You frown at {1}.") },
            { "gag", new Emote("You gag, suppressing the urge to vomit.", "{0} gags audibly, and looks ill.", "{0} gags when looking at {1}.", "You gag while looking at {1}.") },
            { "gasp", new Emote("You gasp in shock.", "{0} gasps loudly.", "{0} gasps loudly at {1}.", "You gasp loudly at {1}.") },
            { "giggle", new Emote("You giggle.", "{0} giggles.", "{0} giggles adorably at {1}.", "You giggle adorably at {1}.") },
            { "grin", new Emote("You grin.", "{0} grins.", "{0} grins at {1}.", "You grin at {1}.") },
            { "groan", new Emote("You groan.", "{0} groans.", "{0} groans at {1}.", "You groan loudly at {1}.") },
            { "grumble", new Emote("You grumble, annoyed.", "{0} grumbles loudly.", "{0} grumbles loudly at {1}.", "You grumble, annoyed, at {1}.") },
            { "hiccup", new Emote("*HIC*", "{0} hiccups.", "{0} looks at {1} and hiccups.", "You look at {1} and hiccup cutely.") },
            { "hug", new Emote("You hug yourself.", "{0} hugs themself tightly.", "{0} hugs {1}.", "You hug {1}.") },
            { "kiss", new Emote("You blow a kiss.", "{0} blows a kiss.", "{0} kisses {1}.", "You kiss {1}.") },
            { "laugh", new Emote("You laugh.", "{0} laughs.", "{0} laughs straight at {1}.", "You laugh directly at {1}.") },
            { "lick", new Emote("You lick your lips.", "{0} licks their lips.", "{0} licks their lips at {1}.", "You lick your lips at {1}.") },
            { "lol", new Emote("You laugh out loud!", "{0} laughs out loud!", "{0} laughs out loud at {1}.", "You laugh out loud at {1}.") },
            { "moan", new Emote("You moan loudly.", "{0} moans loudly.", "{0} moans loudly to {1}.", "You moan loudly in {1}'s ear.") },
            { "mutter", new Emote("You mutter under your breath.", "{0} mutters under their breath.", "{0} mutters under their breath at {1}.", "You mutter under your breath at {1}.") },
            { "nod", new Emote("You nod.", "{0} nods.", "{0} nods at {1}.", "You nod at {1}.") },
            { "ponder", new Emote("You ponder the idea.", "{0} ponders deeply.", "{0} ponders deeply while looking at {1}.", "You ponder deeply while looking at {1}.") },
            { "rofl", new Emote("You laugh so hard you almost fall down.", "{0} laughs so hard they almost fall down.", "{0} laughs so hard at {1} they almost fall down.", "You laugh so hard at {1} you almost fall down.") },
            { "shrug", new Emote("You shrug.", "{0} shrugs.", "{0} shrugs at {1}.", "You shrug at {1}.") },
            { "sigh", new Emote("You sigh.", "{0} sighs.", "{0} sighs wistfully at {1}.", "You sigh wistfully at {1}.") },
            { "slap", new Emote("You slap...nothing", "{0} slaps themself.", "{0} slaps {1} across the face.", "You slap {1} across the face.") },
            { "smile", new Emote("You smile.", "{0} smiles.", "{0} smiles at {1}.", "You smile at {1}.") },
            { "smirk", new Emote("You smirk.", "{0} smirks.", "{0} smirks at {1}.", "You smirk at {1}.") },
            { "sneer", new Emote("You sneer.", "{0} sneers.", "{0} sneers at {1}.", "You sneer at {1}.") },
            { "snicker", new Emote("You snicker.", "{0} snickers wickedly.", "{0} snickers wickedly at {1}.", "You snicker wickedly at {1}.") },
            { "snore", new Emote("You let out a loud snore.", "{0} lets out a loud snore.", "{0} snores obviously at {1}.", "You snore loudly at {1}.") },
            { "snort", new Emote("You snort.", "{0} snorts derisively.", "{0} snorts derisively at {1}.", "You snort derisively at {1}.") },
            { "tongue", new Emote("You stick out your tongue.", "{0} sticks out their tongue.", "{0} sticks their tongue out at {1}.", "You stick your tongue out at {1}.") },
            { "twiddle", new Emote("You twiddle your thumbs.", "{0} twiddles their thumbs.", "{0} twiddles their thumbs at {1}.", "You twiddle your thumbs at {1}.") },
            { "wave", new Emote("You wave.", "{0} waves.", "{0} waves at {1}.", "You wave at {1}.") },
            { "whine", new Emote("You whine loudly to anyone who will liste.", "{0} whines pitifully to anyone who will listen.", "{0} whines pitifully to {1}.", "You whine pitifully to {1}.") },
            { "wince", new Emote("You wince painfully.", "{0} winces painfully.", "{0} winces painfully at {1}.", "You wince painfully at {1}.") },
            { "wink", new Emote("You wink.", "{0} winks.", "{0} winks at {1}.", "You wink suggestively at {1}.") },
            { "yawn", new Emote("You yawn.", "{0} yawns.", "{0} yawns while listening to {1}.", "You yawn while listening to {1} drone on.") },
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
