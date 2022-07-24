// <copyright file="TitleGenerator.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Generators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Engine.Models.SkillTrees;
    using Legendary.Engine.Models.SpellTrees;

    /// <summary>
    /// Generates custom titles for characters based on their levels and skill/spell trees.
    /// </summary>
    public class TitleGenerator
    {
        private readonly IRandom random;
        private readonly ICommunicator communicator;
        private readonly Combat combat;

        private readonly Dictionary<int, string> genericTitles = new Dictionary<int, string>()
        {
            { 1, "Adventurer" },
            { 2, "Explorer" },
            { 3, "Seeker" },
            { 4, "Wanderer" },
            { 5, "Voyager" },
        };

        private readonly Dictionary<int, string> warriorTitles = new Dictionary<int, string>()
        {
            { 6, "Combatant" },
            { 7, "Brawler" },
            { 8, "Beater" },
            { 9, "Crusher" },
            { 10, "Warrior" },
            { 11, "Enraged" },
            { 12, "Powerful" },
            { 13, "Violent" },
            { 14, "Myrmidon" },
            { 15, "Great Warrior" },
        };

        private readonly Dictionary<int, string> mageTitles = new Dictionary<int, string>()
        {
            { 6, "Dilettante" },
            { 7, "Apprentice" },
            { 8, "Spellcaster" },
            { 9, "Adept,Adept of Magic" },
            { 10, "Mage,Magus" },
            { 11, "Acolyte,Acolyte of Magic" },
            { 12, "Conjurer" },
            { 13, "Summoner" },
            { 14, "Invoker" },
            { 15, "Great Magus" },
        };

        private readonly Dictionary<int, string> clericTitles = new Dictionary<int, string>()
        {
            { 6, "Faithful" },
            { 7, "Diligent" },
            { 8, "Pious" },
            { 9, "Peaceful" },
            { 10, "Cleric" },
            { 11, "Gentle" },
            { 12, "Healer" },
            { 13, "Curer" },
            { 14, "Studious" },
            { 15, "Great Cleric" },
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="TitleGenerator"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="combat">The combat engine.</param>
        public TitleGenerator(ICommunicator communicator, IRandom random, Combat combat)
        {
            this.random = random;
            this.communicator = communicator;
            this.combat = combat;
        }

        private enum ClassBasis
        {
            Warrior = 0, // Warrior, Berserker, Cavalier
            Mage = 1, // Mage, Necromancer, Invoker
            Rogue = 3, // Thief
            Cleric = 4, // Cleric, Healer
            WarriorMage = 5, // Battlemage, Dark Knight
            WarriorRogue = 7, // Ranger, Pirate
            WarriorCleric = 8, // Paladin
            MageRogue = 10, // Illusionist
            MageCleric = 11, // Wizard
            ClericRogue = 12, // Druid
        }

        /// <summary>
        /// Generate the custom title.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <returns>String.</returns>
        public string? Generate(Character character)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("the ");

                if (character.Level <= 5)
                {
                    sb.Append(this.genericTitles[character.Level]);
                }
                else
                {
                    sb.Append(this.CalculateTitle(character));
                }

                return sb.ToString();
            }
            catch
            {
                return null;
            }
        }

        private string CalculateTitle(Character character)
        {
            // Use this to see which class basic the character is leaning toward.
            Dictionary<ClassBasis, int> classBalance = new Dictionary<ClassBasis, int>()
            {
                { ClassBasis.Warrior, 0 },
                { ClassBasis.Mage, 0 },
                { ClassBasis.Rogue, 0 },
                { ClassBasis.Cleric, 0 },
                { ClassBasis.WarriorMage, 0 },
                { ClassBasis.WarriorRogue, 0 },
                { ClassBasis.WarriorCleric, 0 },
                { ClassBasis.MageRogue, 0 },
                { ClassBasis.MageCleric, 0 },
                { ClassBasis.ClericRogue, 0 },
            };

            // Get the player's highest skill proficiency that isn't recall.
            var highestSkillProf = character.Skills.Where(sk => sk.SkillName.ToLower() != "recall").OrderByDescending(sk => sk.Proficiency).FirstOrDefault();

            // Get the player's highest spell proficiency.
            var highestSpellProf = character.Spells.OrderByDescending(sp => sp.Proficiency).FirstOrDefault();

            // Get the count of both skills and spells.
            var skillCount = character.Skills.Count;
            var spellCount = character.Spells.Count;

            // More spells than skills, lean mage/cleric.
            if (skillCount < spellCount)
            {
                classBalance[ClassBasis.Mage] += 1;
                classBalance[ClassBasis.Cleric] += 1;
            }
            else
            {
                // Lean warrior/rogue
                classBalance[ClassBasis.Warrior] += 1;
                classBalance[ClassBasis.Rogue] += 1;
            }

            // Now check skill and spell trees
            this.CalculateSkillTrees(character, classBalance);
            this.CalculateSpellTrees(character, classBalance);

            // Get the highest 3 scores
            var results = classBalance.OrderByDescending(b => b.Key).Take(3).ToList();

            // Randomize a pick.
            var randomPick = results[this.random.Next(0, 2)];

            // Build the title based on the pick.
            return this.SelectTitle(character, randomPick.Key);
        }

        private string SelectTitle(Character character, ClassBasis basis)
        {
            switch (basis)
            {
                case ClassBasis.Warrior:
                    {
                        var titles = this.warriorTitles[character.Level].Split(',');
                        return titles[this.random.Next(0, titles.Length - 1)];
                    }

                case ClassBasis.Cleric:
                    {
                        var titles = this.clericTitles[character.Level].Split(',');
                        return titles[this.random.Next(0, titles.Length - 1)];
                    }

                case ClassBasis.Mage:
                    {
                        var titles = this.mageTitles[character.Level].Split(',');
                        return titles[this.random.Next(0, titles.Length - 1)];
                    }

                case ClassBasis.Rogue:
                    break;
                case ClassBasis.ClericRogue:
                    break;
                case ClassBasis.MageCleric:
                    break;
                case ClassBasis.MageRogue:
                    break;
                case ClassBasis.WarriorCleric:
                    break;
                case ClassBasis.WarriorMage:
                    break;
                case ClassBasis.WarriorRogue:
                    break;
            }

            return string.Empty;
        }

        private void CalculateSkillTrees(Character character, Dictionary<ClassBasis, int> classBalance)
        {
            List<string> skillList = character.Skills.Where(sk => sk.Proficiency > 0).Select(sk => sk.SkillName.ToLower()).ToList();

            // Warrior.
            var martialCount = new MartialGroup(this.communicator, this.random, this.combat).GetMatches(skillList);

            classBalance[ClassBasis.Warrior] += martialCount;
            classBalance[ClassBasis.WarriorCleric] += martialCount;
            classBalance[ClassBasis.WarriorMage] += martialCount;
            classBalance[ClassBasis.WarriorRogue] += martialCount;

            // Warrior.
            var weaponCount = new WeaponGroup(this.communicator, this.random, this.combat).GetMatches(skillList);

            classBalance[ClassBasis.Warrior] += weaponCount;
            classBalance[ClassBasis.WarriorCleric] += weaponCount;
            classBalance[ClassBasis.WarriorMage] += weaponCount;
            classBalance[ClassBasis.WarriorRogue] += weaponCount;
        }

        private void CalculateSpellTrees(Character character, Dictionary<ClassBasis, int> classBalance)
        {
            List<string> spellList = character.Spells.Where(sp => sp.Proficiency > 0).Select(sp => sp.SpellName.ToLower()).ToList();

            // Mage.
            var airCount = new AirGroup(this.communicator, this.random, this.combat).GetMatches(spellList);

            classBalance[ClassBasis.Mage] += airCount;
            classBalance[ClassBasis.WarriorMage] += airCount;
            classBalance[ClassBasis.MageCleric] += airCount;

            // Mage.
            var fireCount = new FireGroup(this.communicator, this.random, this.combat).GetMatches(spellList);

            classBalance[ClassBasis.Mage] += fireCount;
            classBalance[ClassBasis.WarriorMage] += fireCount;
            classBalance[ClassBasis.MageCleric] += fireCount;

            // Cleric.
            var healingCount = new HealingGroup(this.communicator, this.random, this.combat).GetMatches(spellList);

            classBalance[ClassBasis.Cleric] += healingCount;
            classBalance[ClassBasis.ClericRogue] += healingCount;
            classBalance[ClassBasis.MageCleric] += healingCount;
            classBalance[ClassBasis.WarriorCleric] += healingCount;
        }
    }
}