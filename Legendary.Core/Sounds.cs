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
    /// <summary>
    /// Sound reference.
    /// </summary>
    public static class Sounds
    {
        /// <summary>
        /// Needs to be appended to requests to access file share.
        /// </summary>
        public const string SAS_TOKEN = "?sv=2021-12-02&ss=bfqt&srt=sco&sp=rwdlacupiytfx&se=2025-03-02T01:01:15Z&st=2023-03-10T17:01:15Z&spr=https&sig=nNpMARshWaVt834sDpwGXLp5%2BfAQtnrMcSQmWqf8o%2Fk%3D";

        /// <summary>
        /// Armor MP3.
        /// </summary>
        public const string ARMOR = "https://legendaryweb.file.core.windows.net/audio/soundfx/armor.mp3" + SAS_TOKEN;

        /// <summary>
        /// Acid Blast MP3.
        /// </summary>
        public const string ACIDBLAST = "https://legendaryweb.file.core.windows.net/audio/soundfx/acidblast.mp3" + SAS_TOKEN;

        /// <summary>
        /// Fireball MP3.
        /// </summary>
        public const string FIREBALL = "https://legendaryweb.file.core.windows.net/audio/soundfx/fireball.mp3" + SAS_TOKEN;

        /// <summary>
        /// Lightning bolt MP3.
        /// </summary>
        public const string LIGHTNINGBOLT = "https://legendaryweb.file.core.windows.net/audio/soundfx/lightningbolt.mp3" + SAS_TOKEN;

        /// <summary>
        /// Recall MP3.
        /// </summary>
        public const string RECALL = "https://legendaryweb.file.core.windows.net/audio/soundfx/recall.mp3" + SAS_TOKEN;

        /// <summary>
        /// Slash MP3.
        /// </summary>
        public const string SLASH = "https://legendaryweb.file.core.windows.net/audio/soundfx/slash.mp3" + SAS_TOKEN;

        /// <summary>
        /// Punch MP3.
        /// </summary>
        public const string PUNCH = "https://legendaryweb.file.core.windows.net/audio/soundfx/punch.mp3" + SAS_TOKEN;

        /// <summary>
        /// Blunt MP3.
        /// </summary>
        public const string BLUNT = "https://legendaryweb.file.core.windows.net/audio/soundfx/blunt.mp3" + SAS_TOKEN;

        /// <summary>
        /// Pierce MP3.
        /// </summary>
        public const string PIERCE = "https://legendaryweb.file.core.windows.net/audio/soundfx/pierce.mp3" + SAS_TOKEN;

        /// <summary>
        /// Armor MP3.
        /// </summary>
        public const string DEATH = "https://legendaryweb.file.core.windows.net/audio/soundfx/death.mp3" + SAS_TOKEN;

        /// <summary>
        /// Level up MP3.
        /// </summary>
        public const string LEVELUP = "https://legendaryweb.file.core.windows.net/audio/soundfx/levelup.mp3" + SAS_TOKEN;

        /// <summary>
        /// Cure MP3.
        /// </summary>
        public const string CURELIGHT = "https://legendaryweb.file.core.windows.net/audio/soundfx/curelight.mp3" + SAS_TOKEN;

        /// <summary>
        /// Walk MP3.
        /// </summary>
        public const string WALK = "https://legendaryweb.file.core.windows.net/audio/soundfx/walk.mp3" + SAS_TOKEN;

        /// <summary>
        /// Rain MP3.
        /// </summary>
        public const string RAIN = "https://legendaryweb.file.core.windows.net/audio/weather/rain.mp3" + SAS_TOKEN;

        /// <summary>
        /// Storm MP3.
        /// </summary>
        public const string STORM = "https://legendaryweb.file.core.windows.net/audio/weather/storm.mp3" + SAS_TOKEN;

        /// <summary>
        /// Space MP3.
        /// </summary>
        public const string SPACE = "https://legendaryweb.file.core.windows.net/audio/weather/space.mp3" + SAS_TOKEN;

        /// <summary>
        /// Wind MP3.
        /// </summary>
        public const string WIND = "https://legendaryweb.file.core.windows.net/audio/weather/wind.mp3" + SAS_TOKEN;

        /// <summary>
        /// Forest MP3.
        /// </summary>
        public const string FOREST = "https://legendaryweb.file.core.windows.net/audio/weather/forest.mp3" + SAS_TOKEN;

        /// <summary>
        /// Fountain MP3.
        /// </summary>
        public const string CITY_FOUNTAIN = "https://legendaryweb.file.core.windows.net/audio/weather/city_fountain.mp3" + SAS_TOKEN;

        /// <summary>
        /// City MP3.
        /// </summary>
        public const string CITY = "https://legendaryweb.file.core.windows.net/audio/weather/city.mp3" + SAS_TOKEN;

        /// <summary>
        /// Open Door MP3.
        /// </summary>
        public const string OPENDOOR = "https://legendaryweb.file.core.windows.net/audio/soundfx/door-open.mp3" + SAS_TOKEN;

        /// <summary>
        /// Close Door MP3.
        /// </summary>
        public const string CLOSEDOOR = "https://legendaryweb.file.core.windows.net/audio/soundfx/door-close.mp3" + SAS_TOKEN;

        /// <summary>
        /// Lock Door MP3.
        /// </summary>
        public const string LOCKDOOR = "https://legendaryweb.file.core.windows.net/audio/soundfx/lock.mp3" + SAS_TOKEN;

        /// <summary>
        /// Unlock Door MP3.
        /// </summary>
        public const string UNLOCKDOOR = "https://legendaryweb.file.core.windows.net/audio/soundfx/unlock.mp3" + SAS_TOKEN;

        /// <summary>
        /// Thunder MP3.
        /// </summary>
        public const string THUNDER = "https://legendaryweb.file.core.windows.net/audio/weather/thunder.mp3" + SAS_TOKEN;

        /// <summary>
        /// Light rain MP3.
        /// </summary>
        public const string LIGHTRAIN = "https://legendaryweb.file.core.windows.net/audio/weather/lightrain.mp3" + SAS_TOKEN;

        /// <summary>
        /// Heavy wind MP3.
        /// </summary>
        public const string HEAVYWIND = "https://legendaryweb.file.core.windows.net/audio/weather/heavywind.mp3" + SAS_TOKEN;

        /// <summary>
        /// Heavy thunder MP3.
        /// </summary>
        public const string HEAVYTHUNDER = "https://legendaryweb.file.core.windows.net/audio/weather/heavythunder.mp3" + SAS_TOKEN;

        /// <summary>
        /// Heavy rain MP3.
        /// </summary>
        public const string HEAVYRAIN = "https://legendaryweb.file.core.windows.net/audio/weather/heavyrain.mp3" + SAS_TOKEN;

        /// <summary>
        /// Thunder fade MP3.
        /// </summary>
        public const string THUNDERFADE = "https://legendaryweb.file.core.windows.net/audio/weather/thunderfade.mp3" + SAS_TOKEN;

        /// <summary>
        /// Wind fade MP3.
        /// </summary>
        public const string WINDFADE = "https://legendaryweb.file.core.windows.net/audio/weather/windfade.mp3" + SAS_TOKEN;

        /// <summary>
        /// Selling sound.
        /// </summary>
        public const string COINS_SELL = "https://legendaryweb.file.core.windows.net/audio/soundfx/coins_buy.mp3" + SAS_TOKEN;

        /// <summary>
        /// Buying sound.
        /// </summary>
        public const string COINS_BUY = "https://legendaryweb.file.core.windows.net/audio/soundfx/coins_buy.mp3" + SAS_TOKEN;

        /// <summary>
        /// Harm MP3.
        /// </summary>
        public const string HARM = "https://legendaryweb.file.core.windows.net/audio/soundfx/harm.mp3" + SAS_TOKEN;

        /// <summary>
        /// Sanctuary MP3.
        /// </summary>
        public const string SANCTUARY = "https://legendaryweb.file.core.windows.net/audio/soundfx/sanctuary.mp3" + SAS_TOKEN;

        /// <summary>
        /// Summon MP3.
        /// </summary>
        public const string SUMMON = "https://legendaryweb.file.core.windows.net/audio/soundfx/summon.mp3" + SAS_TOKEN;

        /// <summary>
        /// Turn Undead MP3.
        /// </summary>
        public const string TURNUNDEAD = "https://legendaryweb.file.core.windows.net/audio/soundfx/turnundead.mp3" + SAS_TOKEN;

        /// <summary>
        /// Teleport MP3.
        /// </summary>
        public const string TELEPORT = "https://legendaryweb.file.core.windows.net/audio/soundfx/teleport.mp3" + SAS_TOKEN;

        /// <summary>
        /// Heal MP3.
        /// </summary>
        public const string HEAL = "https://legendaryweb.file.core.windows.net/audio/soundfx/heal.mp3" + SAS_TOKEN;
    }
}