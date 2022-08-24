// <copyright file="ScoreMessage.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Output
{
    using Newtonsoft.Json;

    /// <summary>
    /// This message is sent to the client each time there is an update to the controls on the page.
    /// </summary>
    public class ScoreMessage
    {
        /// <summary>
        /// Gets or sets the message body.
        /// </summary>
        [JsonProperty("m")]
        public Message? Message { get; set; }

        /// <summary>
        /// Gets or sets the message type.
        /// </summary>
        [JsonProperty("type")]
        public string? Type { get; set; } = "score";
    }

    /// <summary>
    /// Main message body.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Compiled together in single JSON object.")]
    public class Message
    {
        /// <summary>
        /// Gets or sets the personal section.
        /// </summary>
        [JsonProperty("pe")]
        public Personal? Personal { get; set; }

        /// <summary>
        /// Gets or sets the vitals section.
        /// </summary>
        [JsonProperty("vi")]
        public Vitals? Vitals { get; set; }

        /// <summary>
        /// Gets or sets the attributes section.
        /// </summary>
        [JsonProperty("at")]
        public Attributes? Attributes { get; set; }

        /// <summary>
        /// Gets or sets the armor section.
        /// </summary>
        [JsonProperty("ar")]
        public Armor? Armor { get; set; }

        /// <summary>
        /// Gets or sets the saves section.
        /// </summary>
        [JsonProperty("sa")]
        public Saves? Saves { get; set; }

        /// <summary>
        /// Gets or sets the other section.
        /// </summary>
        [JsonProperty("ot")]
        public Other? Other { get; set; }
    }

    /// <summary>
    /// Messages that contains current personal stats.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Compiled together in single JSON object.")]
    public class Personal
    {
        /// <summary>
        /// Gets or sets name.
        /// </summary>
        [JsonProperty("na")]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets race.
        /// </summary>
        [JsonProperty("ra")]
        public string? Race { get; set; }

        /// <summary>
        /// Gets or sets alignment.
        /// </summary>
        [JsonProperty("al")]
        public string? Alignment { get; set; }

        /// <summary>
        /// Gets or sets ethos.
        /// </summary>
        [JsonProperty("et")]
        public string? Ethos { get; set; }

        /// <summary>
        /// Gets or sets hometown.
        /// </summary>
        [JsonProperty("ho")]
        public string? Hometown { get; set; }

        /// <summary>
        /// Gets or sets gender.
        /// </summary>
        [JsonProperty("ge")]
        public string? Gender { get; set; }

        /// <summary>
        /// Gets or sets title.
        /// </summary>
        [JsonProperty("ti")]
        public string? Title { get; set; }
    }

    /// <summary>
    /// Messages that contains current vital stats.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Compiled together in single JSON object.")]
    public class Vitals
    {
        /// <summary>
        /// Gets or sets health.
        /// </summary>
        [JsonProperty("he")]
        public string? Health { get; set; }

        /// <summary>
        /// Gets or sets mana.
        /// </summary>
        [JsonProperty("ma")]
        public string? Mana { get; set; }

        /// <summary>
        /// Gets or sets movement.
        /// </summary>
        [JsonProperty("mo")]
        public string? Movement { get; set; }

        /// <summary>
        /// Gets or sets experience.
        /// </summary>
        [JsonProperty("ex")]
        public string? Experience { get; set; }

        /// <summary>
        /// Gets or sets carry.
        /// </summary>
        [JsonProperty("ca")]
        public string? Carry { get; set; }

        /// <summary>
        /// Gets or sets level.
        /// </summary>
        [JsonProperty("le")]
        public string? Level { get; set; }
    }

    /// <summary>
    /// Messages that contains current attributes.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Compiled together in single JSON object.")]
    public class Attributes
    {
        /// <summary>
        /// Gets or sets str.
        /// </summary>
        [JsonProperty("st")]
        public string? Str { get; set; }

        /// <summary>
        /// Gets or sets int.
        /// </summary>
        [JsonProperty("in")]
        public string? Int { get; set; }

        /// <summary>
        /// Gets or sets wis.
        /// </summary>
        [JsonProperty("wi")]
        public string? Wis { get; set; }

        /// <summary>
        /// Gets or sets dex.
        /// </summary>
        [JsonProperty("de")]
        public string? Dex { get; set; }

        /// <summary>
        /// Gets or sets con.
        /// </summary>
        [JsonProperty("co")]
        public string? Con { get; set; }
    }

    /// <summary>
    /// Messages that contains current armor.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Compiled together in single JSON object.")]
    public class Armor
    {
        /// <summary>
        /// Gets or sets blunt.
        /// </summary>
        [JsonProperty("bl")]
        public string? Blunt { get; set; }

        /// <summary>
        /// Gets or sets pierce.
        /// </summary>
        [JsonProperty("pi")]
        public string? Pierce { get; set; }

        /// <summary>
        /// Gets or sets edged.
        /// </summary>
        [JsonProperty("ed")]
        public string? Edged { get; set; }

        /// <summary>
        /// Gets or sets magic.
        /// </summary>
        [JsonProperty("ma")]
        public string? Magic { get; set; }
    }

    /// <summary>
    /// Messages that contains current saves.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Compiled together in single JSON object.")]
    public class Saves
    {
        /// <summary>
        /// Gets or sets aff.
        /// </summary>
        [JsonProperty("af")]
        public string? Aff { get; set; }

        /// <summary>
        /// Gets or sets mal.
        /// </summary>
        [JsonProperty("ma")]
        public string? Mal { get; set; }

        /// <summary>
        /// Gets or sets spell.
        /// </summary>
        [JsonProperty("sp")]
        public string? Spell { get; set; }

        /// <summary>
        /// Gets or sets death.
        /// </summary>
        [JsonProperty("de")]
        public string? Death { get; set; }

        /// <summary>
        /// Gets or sets neg.
        /// </summary>
        [JsonProperty("ne")]
        public string? Neg { get; set; }

        /// <summary>
        /// Gets or sets learns.
        /// </summary>
        [JsonProperty("lr")]
        public string? Learn { get; set; }
    }

    /// <summary>
    /// Messages that contains other stuff.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Compiled together in single JSON object.")]
    public class Other
    {
        /// <summary>
        /// Gets or sets trains.
        /// </summary>
        [JsonProperty("tr")]
        public string? Trains { get; set; }

        /// <summary>
        /// Gets or sets practices.
        /// </summary>
        [JsonProperty("pr")]
        public string? Pracs { get; set; }

        /// <summary>
        /// Gets or sets gold.
        /// </summary>
        [JsonProperty("go")]
        public string? Gold { get; set; }

        /// <summary>
        /// Gets or sets silver.
        /// </summary>
        [JsonProperty("si")]
        public string? Silver { get; set; }

        /// <summary>
        /// Gets or sets copper.
        /// </summary>
        [JsonProperty("co")]
        public string? Copper { get; set; }

        /// <summary>
        /// Gets or sets last login.
        /// </summary>
        [JsonProperty("la")]
        public string? LastLogin { get; set; }

        /// <summary>
        /// Gets or sets hit dice.
        /// </summary>
        [JsonProperty("hd")]
        public string? HitDice { get; set; }

        /// <summary>
        /// Gets or sets the damage dice.
        /// </summary>
        [JsonProperty("dd")]
        public string? DamageDice { get; set; }
    }
}