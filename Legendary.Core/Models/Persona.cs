// <copyright file="Persona.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Core.Models
{
    using System.Collections.Generic;
    using System.IO;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a character persona that can be modified.
    /// </summary>
    public class Persona
    {
        /// <summary>
        /// Gets or sets the name of this persona.
        /// </summary>
        [JsonProperty("name")]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the prompt of this persona.
        /// </summary>
        [JsonProperty("prompt")]
        public string? Prompt { get; set; }

        /// <summary>
        /// Gets or sets the attitude of this persona.
        /// </summary>
        [JsonProperty("attitude")]
        public string? Attitude { get; set; }

        /// <summary>
        /// Gets or sets the age of this persona.
        /// </summary>
        [JsonProperty("age")]
        public string? Age { get; set; }

        /// <summary>
        /// Gets or sets the race of this persona.
        /// </summary>
        [JsonProperty("race")]
        public string? Race { get; set; }

        /// <summary>
        /// Gets or sets the class of this persona.
        /// </summary>
        [JsonProperty("class")]
        public string? Class { get; set; }

        /// <summary>
        /// Gets or sets information about this persona in sentences.
        /// </summary>
        [JsonProperty("background")]
        public List<string>? Background { get; set; }

        /// <summary>
        /// Loads this persona from a JSON file.
        /// </summary>
        /// <param name="mobile">The mobile to load the persona for.</param>
        /// <returns>Persona.</returns>
        public static Persona? Load(Mobile mobile)
        {
            try
            {
                return JsonConvert.DeserializeObject<Persona>(File.ReadAllText($@"Data/Personas/{mobile.PersonaFile}"));
            }
            catch
            {
                return LoadGenericPersona(mobile);
            }
        }

        private static Persona LoadGenericPersona(Mobile mobile)
        {
            return new Persona()
            {
                Age = mobile.Age.ToString(),
                Name = $"{mobile.FirstName} {mobile.LastName}",
                Race = mobile.Race.ToString(),
                Attitude = $"{mobile.Ethos} {mobile.Alignment}",
                Class = "non player character",
                Prompt = GenericPrompt(mobile),
                Background = new List<string>()
                    {
                        $"You are a {mobile.Gender} {mobile.Race}.",
                        "You live within the fantasy world of Mystra.",
                        "You will not mention AI language models.",
                        "You must stay in character at all times.",
                        "You have no specific persona, so you will answer generically in a medieval fantasy setting.",
                    },
            };
        }

        private static string GenericPrompt(Mobile mobile)
        {
            switch (mobile.Alignment)
            {
                case Types.Alignment.Good:
                    {
                        switch (mobile.Ethos)
                        {
                            case Types.Ethos.Lawful:
                                {
                                    return "May I help you?";
                                }

                            case Types.Ethos.Chaotic:
                                {
                                    return "Did you need something?. I'm a little busy.";
                                }

                            default:
                            case Types.Ethos.Neutral:
                                {
                                    return "What do you want?";
                                }
                        }
                    }

                case Types.Alignment.Evil:
                    {
                        switch (mobile.Ethos)
                        {
                            case Types.Ethos.Lawful:
                                {
                                    return "What the hell do you want?";
                                }

                            case Types.Ethos.Chaotic:
                                {
                                    return "Get the hell away from me.";
                                }

                            default:
                            case Types.Ethos.Neutral:
                                {
                                    return "What are you looking at?";
                                }
                        }
                    }

                default:
                case Types.Alignment.Neutral:
                    {
                        switch (mobile.Ethos)
                        {
                            case Types.Ethos.Lawful:
                                {
                                    return "Are you lost?";
                                }

                            case Types.Ethos.Chaotic:
                                {
                                    return "Did you need something?";
                                }

                            default:
                            case Types.Ethos.Neutral:
                                {
                                    return "Hm, yes?";
                                }
                        }
                    }
            }
        }
    }
}