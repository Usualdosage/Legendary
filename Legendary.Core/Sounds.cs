// <copyright file="Sounds.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Core
{
    using System;

    /// <summary>
    /// Sound reference.
    /// </summary>
    public static class Sounds
    {
        /// <summary>
        /// Needs to be appended to requests to access file share.
        /// </summary>
        public const string SAS_TOKEN = "?sv=2021-06-08&ss=f&srt=sco&sp=r&se=2027-07-27T07:17:16Z&st=2022-07-26T23:17:16Z&spr=https&sig=H7cbZXl63g6mhIHYC00wri8cTRiob6XrWdf1F4D0F%2Bo%3D";

        /// <summary>
        /// Armor MP3.
        /// </summary>
        public const string ARMOR = "https://legendary.file.core.windows.net/audio/soundfx/armor.mp3" + SAS_TOKEN;

        /// <summary>
        /// Fireball MP3.
        /// </summary>
        public const string FIREBALL = "https://legendary.file.core.windows.net/audio/soundfx/fireball.mp3" + SAS_TOKEN;

        /// <summary>
        /// Lightning bolt MP3.
        /// </summary>
        public const string LIGHTNINGBOLT = "https://legendary.file.core.windows.net/audio/soundfx/lightningbolt.mp3" + SAS_TOKEN;

        /// <summary>
        /// Recall MP3.
        /// </summary>
        public const string RECALL = "https://legendary.file.core.windows.net/audio/soundfx/recall.mp3" + SAS_TOKEN;

        /// <summary>
        /// Slash MP3.
        /// </summary>
        public const string SLASH = "https://legendary.file.core.windows.net/audio/soundfx/slash.mp3" + SAS_TOKEN;

        /// <summary>
        /// Punch MP3.
        /// </summary>
        public const string PUNCH = "https://legendary.file.core.windows.net/audio/soundfx/punch.mp3" + SAS_TOKEN;

        /// <summary>
        /// Armor MP3.
        /// </summary>
        public const string DEATH = "https://legendary.file.core.windows.net/audio/soundfx/death.mp3" + SAS_TOKEN;

        /// <summary>
        /// Level up MP3.
        /// </summary>
        public const string LEVELUP = "https://legendary.file.core.windows.net/audio/soundfx/levelup.mp3" + SAS_TOKEN;

        /// <summary>
        /// Cure MP3.
        /// </summary>
        public const string CURELIGHT = "https://legendary.file.core.windows.net/audio/soundfx/curelight.mp3" + SAS_TOKEN;

        /// <summary>
        /// Walk MP3.
        /// </summary>
        public const string WALK = "https://legendary.file.core.windows.net/audio/soundfx/walk.mp3" + SAS_TOKEN;

        /// <summary>
        /// Rain MP3.
        /// </summary>
        public const string RAIN = "https://legendary.file.core.windows.net/audio/weather/rain.mp3" + SAS_TOKEN;

        /// <summary>
        /// Storm MP3.
        /// </summary>
        public const string STORM = "https://legendary.file.core.windows.net/audio/weather/storm.mp3" + SAS_TOKEN;

        /// <summary>
        /// Space MP3.
        /// </summary>
        public const string SPACE = "https://legendary.file.core.windows.net/audio/weather/space.mp3" + SAS_TOKEN;

        /// <summary>
        /// Wind MP3.
        /// </summary>
        public const string WIND = "https://legendary.file.core.windows.net/audio/weather/wind.mp3" + SAS_TOKEN;

        /// <summary>
        /// Forest MP3.
        /// </summary>
        public const string FOREST = "https://legendary.file.core.windows.net/audio/weather/forest.mp3" + SAS_TOKEN;

        /// <summary>
        /// Fountain MP3.
        /// </summary>
        public const string CITY_FOUNTAIN = "https://legendary.file.core.windows.net/audio/weather/city_fountain.mp3" + SAS_TOKEN;

        /// <summary>
        /// City MP3.
        /// </summary>
        public const string CITY = "https://legendary.file.core.windows.net/audio/weather/city.mp3" + SAS_TOKEN;

        /// <summary>
        /// Open Door MP3.
        /// </summary>
        public const string OPENDOOR = "https://legendary.file.core.windows.net/audio/soundfx/door-open.mp3" + SAS_TOKEN;

        /// <summary>
        /// Close Door MP3.
        /// </summary>
        public const string CLOSEDOOR = "https://legendary.file.core.windows.net/audio/soundfx/door-close.mp3" + SAS_TOKEN;

        /// <summary>
        /// Lock Door MP3.
        /// </summary>
        public const string LOCKDOOR = "https://legendary.file.core.windows.net/audio/soundfx/door-open.mp3" + SAS_TOKEN;

        /// <summary>
        /// Unlock Door MP3.
        /// </summary>
        public const string UNLOCKDOOR = "https://legendary.file.core.windows.net/audio/soundfx/door-close.mp3" + SAS_TOKEN;
    }
}