// <copyright file="MobHelper.cs" company="Legendary™">
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection.PortableExecutable;
    using Legendary.Core;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Core.Types;
    using Legendary.Engine.Extensions;
    using Legendary.Engine.Models.Skills;
    using Legendary.Engine.Models.Spells;

    /// <summary>
    /// Helper for mobs.
    /// </summary>
    public class MobHelper
    {
        /// <summary>
        /// Autospawns a mobile in a room based on the level of the character.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <param name="room">The room.</param>
        /// <param name="random">The random number generator.</param>
        /// <returns>Mobile.</returns>
        public static Mobile? Autospawn(Character? actor, Room room, IRandom random)
        {
            var mobile = new Mobile();

            // Randomize the gender.
            mobile.Gender = mobile.Gender.RandomEnum();

            // We are only doing humans for now, to keep it simple.
            mobile.Race = Race.Human;

            // All of these autospawns will be neutral align, random ethos.
            mobile.Alignment = Alignment.Neutral;
            mobile.Ethos = mobile.Ethos.RandomEnum();

            if (actor != null)
            {
                mobile.Level = random.Next(Math.Min(1, actor.Level - 5), actor.Level + 5);
            }
            else
            {
                mobile.Level = random.Next(10, 40);
            }

            // Randomize the stat rolls.
            var str = random.Next(12, 18);
            var intg = random.Next(12, 18);
            var wis = random.Next(12, 18);
            var dex = random.Next(12, 18);
            var con = random.Next(12, 18);

            mobile.Str = new MaxCurrent(str, str);
            mobile.Int = new MaxCurrent(intg, intg);
            mobile.Wis = new MaxCurrent(wis, wis);
            mobile.Dex = new MaxCurrent(dex, dex);
            mobile.Con = new MaxCurrent(con, con);

            // Randomize the vitals.
            var health = mobile.Level * random.Next(12, (int)mobile.Con.Current);
            var mana = mobile.Level * random.Next(12, (int)mobile.Wis.Current);
            var move = mobile.Level * random.Next(12, (int)mobile.Dex.Current);

            mobile.Health = new MaxCurrent(health, health);
            mobile.Mana = new MaxCurrent(mana, mana);
            mobile.Movement = new MaxCurrent(move, move);

            // Set the mob to wander, scavenge, and flee.
            mobile.MobileFlags = new List<MobileFlags> { MobileFlags.Wander, MobileFlags.Scavenger, MobileFlags.Wimpy };

            // Set the mob to the room location.
            mobile.Location = new KeyValuePair<long, long>(room.AreaId, room.RoomId);

            // Give them some money.
            mobile.Currency = random.Next(0.15m, 10m);

            mobile.CarryWeight = new MaxCurrent(120, 120);

            mobile.HitDice = 0;
            mobile.DamageDice = 0;

            // Apply some random saves.
            mobile.SaveAfflictive = random.Next(2, mobile.Level / 2);
            mobile.SaveDeath = random.Next(2, mobile.Level / 2);
            mobile.SaveMaledictive = random.Next(2, mobile.Level / 2);
            mobile.SaveNegative = random.Next(2, mobile.Level / 2);
            mobile.SaveSpell = random.Next(2, mobile.Level / 2);

            // Generate some random names
            string? name = string.Empty;
            string? longDesc = string.Empty;
            string? shortDesc = string.Empty;
            var rand = random.Next(0, 4);

            switch (room.Terrain)
            {
                case Terrain.City:
                    {
                        switch (rand)
                        {
                            default:
                            case 0:
                            case 1:
                                name = "a city commoner";
                                shortDesc = $"{name.FirstCharToUpper()} walks quietly long a city path here.";
                                longDesc = $"{name.FirstCharToUpper()} is dressed in plain, humble clothing. {mobile.PronounSubjective} keeps {mobile.Pronoun} eyes averted as you pass, daring not to look directly at you, as {mobile.PronounSubjective} is well below your station.";
                                break;
                            case 2:
                            case 3:
                                name = "a city youth";
                                shortDesc = $"{name.FirstCharToUpper()} walks haughtily along the streets here.";
                                longDesc = $"{name.FirstCharToUpper()} is dressed in popular youth clothing. {mobile.PronounSubjective} is walking along arrogantly, without a care in the world. {mobile.PronounSubjective} is enjoying all of the benefits of being young and free.";
                                break;
                        }

                        break;
                    }
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                mobile.CharacterId = Constants.RANDOM_MOBILE + rand;
                mobile.FirstName = name;
                mobile.ShortDescription = shortDesc;
                mobile.LongDescription = longDesc;
                return mobile;
            }
            else
            {
                return null;
            }
        }
    }
}