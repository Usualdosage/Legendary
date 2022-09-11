﻿// <copyright file="Races.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Core.Models
{
    using System;
    using System.Collections.Generic;
    using Legendary.Core.Contracts;
    using Legendary.Core.Types;

    /// <summary>
    /// Static information about races, like exp penalty, maximums and minimums.
    /// </summary>
    public static class Races
    {
        /// <summary>
        /// Definition of race data (lookup table).
        /// </summary>
        public static Dictionary<Race, RaceStats> RaceData = new Dictionary<Race, RaceStats>()
        {
            {
                Race.Avian, new RaceStats()
                {
                    ConMax = 18,
                    DexMax = 23,
                    IntMax = 18,
                    WisMax = 18,
                    StrMax = 18,
                    ExperiencePenalty = 500,
                    Size = Size.Medium,
                    Alignments = new List<Alignment>() { Alignment.Good, Alignment.Neutral, Alignment.Evil },
                    Abilities = new List<string>() { "Avian" },
                    BaseCarryWeight = 120,
                }
            },
            {
                Race.Drow, new RaceStats()
                {
                    ConMax = 18,
                    DexMax = 24,
                    IntMax = 20,
                    WisMax = 18,
                    StrMax = 18,
                    ExperiencePenalty = 750,
                    Size = Size.Small,
                    Alignments = new List<Alignment>() { Alignment.Evil },
                    Abilities = new List<string>() { "Drow", "Sneak" },
                    BaseCarryWeight = 80,
                }
            },
            {
                Race.Duergar, new RaceStats()
                {
                    ConMax = 22,
                    DexMax = 18,
                    IntMax = 18,
                    WisMax = 20,
                    StrMax = 18,
                    ExperiencePenalty = 500,
                    Size = Size.Small,
                    Alignments = new List<Alignment>() { Alignment.Evil },
                    Abilities = new List<string>() { "Duergar" },
                    BaseCarryWeight = 130,
                }
            },
            {
                Race.Dwarf, new RaceStats()
                {
                    ConMax = 22,
                    DexMax = 18,
                    IntMax = 18,
                    WisMax = 18,
                    StrMax = 20,
                    ExperiencePenalty = 550,
                    Size = Size.Small,
                    Alignments = new List<Alignment>() { Alignment.Good, Alignment.Neutral },
                    Abilities = new List<string>() { "Dwarf", "EdgedWeapons" },
                    BaseCarryWeight = 130,
                }
            },
            {
                Race.Elf, new RaceStats()
                {
                    ConMax = 18,
                    DexMax = 22,
                    IntMax = 20,
                    WisMax = 20,
                    StrMax = 18,
                    ExperiencePenalty = 550,
                    Size = Size.Medium,
                    Alignments = new List<Alignment>() { Alignment.Good },
                    Abilities = new List<string>() { "Elf", "Sneak" },
                    BaseCarryWeight = 120,
                }
            },
            {
                Race.Faerie, new RaceStats()
                {
                    ConMax = 16,
                    DexMax = 24,
                    IntMax = 18,
                    WisMax = 18,
                    StrMax = 14,
                    ExperiencePenalty = 350,
                    Size = Size.Tiny,
                    Alignments = new List<Alignment>() { Alignment.Good, Alignment.Neutral, Alignment.Evil },
                    Abilities = new List<string>() { "Faerie" },
                    BaseCarryWeight = 40,
                }
            },
            {
                Race.FireGiant, new RaceStats()
                {
                    ConMax = 16,
                    DexMax = 16,
                    IntMax = 16,
                    WisMax = 18,
                    StrMax = 25,
                    ExperiencePenalty = 400,
                    Size = Size.Giant,
                    Alignments = new List<Alignment>() { Alignment.Evil },
                    Abilities = new List<string>() { "FireGiant" },
                    BaseCarryWeight = 250,
                }
            },
            {
                Race.Gnome, new RaceStats()
                {
                    ConMax = 16,
                    DexMax = 16,
                    IntMax = 25,
                    WisMax = 22,
                    StrMax = 16,
                    ExperiencePenalty = 500,
                    Size = Size.Small,
                    Alignments = new List<Alignment>() { Alignment.Good, Alignment.Neutral },
                    Abilities = new List<string>() { "Gnome" },
                    BaseCarryWeight = 50,
                }
            },
            {
                Race.HalfElf, new RaceStats()
                {
                    ConMax = 18,
                    DexMax = 19,
                    IntMax = 18,
                    WisMax = 18,
                    StrMax = 18,
                    ExperiencePenalty = 100,
                    Size = Size.Medium,
                    Alignments = new List<Alignment>() { Alignment.Good, Alignment.Neutral },
                    Abilities = new List<string>() { "HalfElf" },
                    BaseCarryWeight = 120,
                }
            },
            {
                Race.Halfling, new RaceStats()
                {
                    ConMax = 18,
                    DexMax = 24,
                    IntMax = 18,
                    WisMax = 20,
                    StrMax = 18,
                    ExperiencePenalty = 250,
                    Size = Size.Small,
                    Alignments = new List<Alignment>() { Alignment.Good, Alignment.Neutral },
                    Abilities = new List<string>() { "Halfling" },
                    BaseCarryWeight = 110,
                }
            },
            {
                Race.HalfOrc, new RaceStats()
                {
                    ConMax = 17,
                    DexMax = 19,
                    IntMax = 17,
                    WisMax = 16,
                    StrMax = 23,
                    ExperiencePenalty = 450,
                    Size = Size.Large,
                    Alignments = new List<Alignment>() { Alignment.Neutral },
                    Abilities = new List<string>() { "HalfOrc" },
                    BaseCarryWeight = 240,
                }
            },
            {
                Race.Human, new RaceStats()
                {
                    ConMax = 18,
                    DexMax = 18,
                    IntMax = 18,
                    WisMax = 18,
                    StrMax = 18,
                    ExperiencePenalty = 0,
                    Size = Size.Medium,
                    Alignments = new List<Alignment>() { Alignment.Good, Alignment.Neutral, Alignment.Evil },
                    Abilities = new List<string>() { "Human" },
                    BaseCarryWeight = 120,
                }
            },
            {
                Race.StoneGiant, new RaceStats()
                {
                    ConMax = 17,
                    DexMax = 16,
                    IntMax = 15,
                    WisMax = 17,
                    StrMax = 24,
                    ExperiencePenalty = 400,
                    Size = Size.Giant,
                    Alignments = new List<Alignment>() { Alignment.Neutral },
                    Abilities = new List<string>() { "StoneGiant" },
                    BaseCarryWeight = 250,
                }
            },
            {
                Race.StormGiant, new RaceStats()
                {
                    ConMax = 18,
                    DexMax = 17,
                    IntMax = 16,
                    WisMax = 17,
                    StrMax = 23,
                    ExperiencePenalty = 650,
                    Size = Size.Giant,
                    Alignments = new List<Alignment>() { Alignment.Good },
                    Abilities = new List<string>() { "StormGiant" },
                    BaseCarryWeight = 250,
                }
            },
        };
    }
}