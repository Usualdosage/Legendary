// <copyright file="TreeHelper.cs" company="Legendary™">
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
    using System.Reflection;
    using System.Text;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Extensions;
    using Legendary.Engine.Models;

    /// <summary>
    /// Helper class for handling skill and spell tree progression.
    /// </summary>
    public static class TreeHelper
    {
        /// <summary>
        /// Determines if the actor is able to learn this group (requires a prerequsite of the prior group).
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <param name="groupName">The group name.</param>
        /// <param name="groupNumeral">The group numeral.</param>
        /// <param name="teacher">The teacher.</param>
        /// <param name="communicator">The communicator.</param>
        /// <param name="random">The random generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat engine.</param>
        /// <returns>True if the player can learn the group.</returns>
        public static bool CanLearnGroup(UserData actor, string groupName, string groupNumeral, Mobile teacher, ICommunicator communicator, IRandom random, IWorld world, ILogger logger, Combat combat)
        {
            var skillTrees = GetSkillTrees(communicator, random, world, logger, combat);

            var targetSkillTree = skillTrees.FirstOrDefault(t => t.Name.ToLower().Contains(groupName.ToLower()));

            if (targetSkillTree != null)
            {
                if (teacher.SchoolType == Core.Types.SchoolType.General || teacher.SchoolType == targetSkillTree.SchoolType)
                {
                    var highestGroupCompleted = GetHighestSkillGroup(actor, targetSkillTree);
                    var groupToStudy = groupNumeral.FromRomanNumeral();

                    if ((groupToStudy - highestGroupCompleted) == 1)
                    {
                        return true;
                    }
                }
            }

            var spellTrees = GetSpellTrees(communicator, random, world, logger, combat);

            var targetSpellTree = spellTrees.FirstOrDefault(t => t.Name.ToLower().Contains(groupName.ToLower()));

            if (targetSpellTree != null)
            {
                if (teacher.SchoolType == Core.Types.SchoolType.General || teacher.SchoolType == targetSpellTree.SchoolType)
                {
                    var highestGroupCompleted = GetHighestSpellGroup(actor, targetSpellTree);
                    var groupToStudy = groupNumeral.FromRomanNumeral();

                    if ((groupToStudy - highestGroupCompleted) == 1)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the list of skills for a given group name and number.
        /// </summary>
        /// <param name="groupName">The group name.</param>
        /// <param name="groupNumeral">The group number.</param>
        /// <param name="communicator">The communicator.</param>
        /// <param name="random">The random generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat engine.</param>
        /// <returns>List of actions.</returns>
        public static List<IAction>? GetSkills(string groupName, string groupNumeral, ICommunicator communicator, IRandom random, IWorld world, ILogger logger, Combat combat)
        {
            var groupNumber = groupNumeral.FromRomanNumeral();
            var trees = GetSkillTrees(communicator, random, world, logger, combat);

            foreach (var tree in trees)
            {
                if (tree.Name.ToLower().Contains(groupName.ToLower()))
                {
                    switch (groupNumber)
                    {
                        case 1:
                            {
                                return tree.Group1;
                            }

                        case 2:
                            {
                                return tree.Group2;
                            }

                        case 3:
                            {
                                return tree.Group3;
                            }

                        case 4:
                            {
                                return tree.Group4;
                            }

                        case 5:
                            {
                                return tree.Group5;
                            }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the list of spells for a given group name and number.
        /// </summary>
        /// <param name="groupName">The group name.</param>
        /// <param name="groupNumeral">The group number.</param>
        /// <param name="communicator">The communicator.</param>
        /// <param name="random">The random generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat engine.</param>
        /// <returns>List of actions.</returns>
        public static List<IAction>? GetSpells(string groupName, string groupNumeral, ICommunicator communicator, IRandom random, IWorld world, ILogger logger, Combat combat)
        {
            var groupNumber = groupNumeral.FromRomanNumeral();
            var trees = GetSpellTrees(communicator, random, world, logger, combat);

            foreach (var tree in trees)
            {
                if (tree.Name.ToLower().Contains(groupName.ToLower()))
                {
                    switch (groupNumber)
                    {
                        case 1:
                            {
                                return tree.Group1;
                            }

                        case 2:
                            {
                                return tree.Group2;
                            }

                        case 3:
                            {
                                return tree.Group3;
                            }

                        case 4:
                            {
                                return tree.Group4;
                            }

                        case 5:
                            {
                                return tree.Group5;
                            }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Lists the skills for a given group name and number.
        /// </summary>
        /// <param name="groupName">The name of the group (e.g. martial).</param>
        /// <param name="groupNumeral">The group numeral (e.g. IV).</param>
        /// <param name="communicator">The communicator.</param>
        /// <param name="random">The random generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat engine.</param>
        /// <returns>string.</returns>
        public static string GetSkillsInGroup(string groupName, string groupNumeral, ICommunicator communicator, IRandom random, IWorld world, ILogger logger, Combat combat)
        {
            var groupNumber = groupNumeral.FromRomanNumeral();
            var trees = GetSkillTrees(communicator, random, world, logger, combat);

            foreach (var tree in trees)
            {
                if (tree.Name.ToLower().Contains(groupName.ToLower()))
                {
                    List<IAction>? skills = null;

                    switch (groupNumber)
                    {
                        case 1:
                            {
                                skills = tree.Group1;
                                break;
                            }

                        case 2:
                            {
                                skills = tree.Group2;
                                break;
                            }

                        case 3:
                            {
                                skills = tree.Group3;
                                break;
                            }

                        case 4:
                            {
                                skills = tree.Group4;
                                break;
                            }

                        case 5:
                            {
                                skills = tree.Group5;
                                break;
                            }
                    }

                    if (skills != null)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append($"<h6>The following skills are available in {groupName} group {groupNumeral}:</h6>");
                        sb.Append("<ul>");

                        foreach (var skill in skills)
                        {
                            sb.Append($"<li>{skill.Name}</li>");
                        }

                        sb.Append("</ul>");
                        return sb.ToString();
                    }
                    else
                    {
                        return $"That group number doesn't exist under the {groupName} group.";
                    }
                }
            }

            return "That group doesn't exist.";
        }

        /// <summary>
        /// Lists the spells for a given group name and number.
        /// </summary>
        /// <param name="groupName">The name of the group (e.g. conjuring).</param>
        /// <param name="groupNumeral">The group numeral (e.g. IV).</param>
        /// <param name="communicator">The communicator.</param>
        /// <param name="random">The random generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat engine.</param>
        /// <returns>string.</returns>
        public static string GetSpellsInGroup(string groupName, string groupNumeral, ICommunicator communicator, IRandom random, IWorld world, ILogger logger, Combat combat)
        {
            var groupNumber = groupNumeral.FromRomanNumeral();
            var trees = GetSpellTrees(communicator, random, world, logger, combat);

            foreach (var tree in trees)
            {
                if (tree.Name.ToLower().Contains(groupName.ToLower()))
                {
                    List<IAction>? spells = null;

                    switch (groupNumber)
                    {
                        case 1:
                            {
                                spells = tree.Group1;
                                break;
                            }

                        case 2:
                            {
                                spells = tree.Group2;
                                break;
                            }

                        case 3:
                            {
                                spells = tree.Group3;
                                break;
                            }

                        case 4:
                            {
                                spells = tree.Group4;
                                break;
                            }

                        case 5:
                            {
                                spells = tree.Group5;
                                break;
                            }
                    }

                    if (spells != null)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append($"<h6>The following spells are available in {groupName} group {groupNumeral}:</h6>");
                        sb.Append("<ul>");

                        foreach (var spell in spells)
                        {
                            sb.Append($"<li>{spell.Name}</li>");
                        }

                        sb.Append("</ul>");
                        return sb.ToString();
                    }
                    else
                    {
                        return $"That group number doesn't exist under the {groupName} group.";
                    }
                }
            }

            return "That group doesn't exist.";
        }

        /// <summary>
        /// Renders an HTML string of the possible skill trees with groups that are learnable by the actor.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <param name="skillTrees">The skill trees.</param>
        /// <param name="canLearn">True if there are skill groups available to learn.</param>
        /// <returns>string.</returns>
        public static string GetLearnableSkillTrees(UserData actor, List<ActionTree> skillTrees, out bool canLearn)
        {
            canLearn = false;
            StringBuilder sbSkills = new StringBuilder();

            sbSkills.Append("<h5>Available Skill Trees</h5>");
            sbSkills.Append("<ul>");

            foreach (var skillTree in skillTrees)
            {
                var maxGroup = GetHighestSkillGroup(actor, skillTree);

                if (maxGroup < 5)
                {
                    maxGroup += 1;
                    sbSkills.Append($"<li>{skillTree.Name} {maxGroup.ToRomanNumeral()}</li>");
                    canLearn = true;
                }
            }

            sbSkills.Append("</ul>");

            return sbSkills.ToString();
        }

        /// <summary>
        /// Renders an HTML string of the possible spell trees with groups that are learnable by the actor.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <param name="spellTrees">The spell trees.</param>
        /// <param name="canLearn">True if there are spell groups available to learn.</param>
        /// <returns>string.</returns>
        public static string GetLearnableSpellTrees(UserData actor, List<ActionTree> spellTrees, out bool canLearn)
        {
            canLearn = false;
            StringBuilder sbSpells = new StringBuilder();

            sbSpells.Append("<h5>Available Spell Trees</h5>");
            sbSpells.Append("<ul>");

            foreach (var spellTree in spellTrees)
            {
                var maxGroup = GetHighestSpellGroup(actor, spellTree);

                if (maxGroup < 5)
                {
                    maxGroup += 1;
                    sbSpells.Append($"<li>{spellTree.Name} {maxGroup.ToRomanNumeral()}</li>");
                    canLearn = true;
                }
            }

            sbSpells.Append("</ul>");

            return sbSpells.ToString();
        }

        /// <summary>
        /// Dynamically loads all skill trees from the assembly.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        /// <param name="random">The random generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat engine.</param>
        /// <returns>List of action trees.</returns>
        public static List<ActionTree> GetSkillTrees(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, Combat combat)
        {
            var engine = Assembly.Load("Legendary.Engine");

            var skillTreesRef = engine.GetTypes().Where(t => t.Namespace == "Legendary.Engine.Models.SkillTrees");

            List<ActionTree> skillTrees = new List<ActionTree>();

            foreach (var tree in skillTreesRef)
            {
                var treeInstance = Activator.CreateInstance(tree, communicator, random, world, logger, combat);

                if (treeInstance != null && treeInstance is ActionTree instance)
                {
                    skillTrees.Add(instance);
                }
            }

            return skillTrees;
        }

        /// <summary>
        /// Dynamically loads all spell trees from the assembly.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        /// <param name="random">The random generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="combat">The combat engine.</param>
        /// <returns>List of action trees.</returns>
        public static List<ActionTree> GetSpellTrees(ICommunicator communicator, IRandom random, IWorld world, ILogger logger, Combat combat)
        {
            var engine = Assembly.Load("Legendary.Engine");

            var spellTreesRef = engine.GetTypes().Where(t => t.Namespace == "Legendary.Engine.Models.SpellTrees");

            List<ActionTree> spellTrees = new List<ActionTree>();

            foreach (var tree in spellTreesRef)
            {
                var treeInstance = Activator.CreateInstance(tree, communicator, random, world, logger, combat);

                if (treeInstance != null && treeInstance is ActionTree instance)
                {
                    spellTrees.Add(instance);
                }
            }

            return spellTrees;
        }

        /// <summary>
        /// Gets the highest skill group within a tree that the player has learned.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <param name="skillTree">The skill tree.</param>
        /// <returns>int.</returns>
        public static int GetHighestSkillGroup(UserData actor, ActionTree skillTree)
        {
            // In order to study the next group, a player must have learned ALL skills in a group. If the group has no skills to learn, move on.
            var intersectGroup1 = skillTree.Group1.Select(g => g.Name.ToLower()).ToList().Intersect(actor.Character.Skills.Select(sk => sk.SkillName.ToLower()).ToList());
            var group1Count = intersectGroup1.Count();
            if (group1Count != skillTree.Group1.Count && skillTree.Group1.Count > 0)
            {
                return 0;
            }

            var intersectGroup2 = skillTree.Group2.Select(g => g.Name.ToLower()).ToList().Intersect(actor.Character.Skills.Select(sk => sk.SkillName.ToLower()).ToList());
            var group2Count = intersectGroup2.Count();
            if (group2Count != skillTree.Group2.Count && skillTree.Group2.Count > 0)
            {
                return 1;
            }

            var intersectGroup3 = skillTree.Group3.Select(g => g.Name.ToLower()).ToList().Intersect(actor.Character.Skills.Select(sk => sk.SkillName.ToLower()).ToList());
            var group3Count = intersectGroup3.Count();
            if (group3Count != skillTree.Group3.Count && skillTree.Group3.Count > 0)
            {
                return 2;
            }

            var intersectGroup4 = skillTree.Group4.Select(g => g.Name.ToLower()).ToList().Intersect(actor.Character.Skills.Select(sk => sk.SkillName.ToLower()).ToList());
            var group4Count = intersectGroup4.Count();
            if (group4Count != skillTree.Group4.Count && skillTree.Group4.Count > 0)
            {
                return 3;
            }

            var intersectGroup5 = skillTree.Group5.Select(g => g.Name.ToLower()).ToList().Intersect(actor.Character.Skills.Select(sk => sk.SkillName.ToLower()).ToList());
            var group5Count = intersectGroup5.Count();
            if (group5Count != skillTree.Group5.Count && skillTree.Group5.Count > 0)
            {
                return 4;
            }

            return 5;
        }

        /// <summary>
        /// Gets the highest spell group within a tree that the player has learned.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <param name="spellTree">The spell tree.</param>
        /// <returns>int.</returns>
        public static int GetHighestSpellGroup(UserData actor, ActionTree spellTree)
        {
            // In order to study the next group, a player must have learned ALL spells in a group. If the group has no spells to learn, move on.
            var intersectGroup1 = spellTree.Group1.Select(g => g.Name.ToLower()).ToList().Intersect(actor.Character.Spells.Select(sk => sk.SpellName.ToLower()).ToList());
            var group1Count = intersectGroup1.Count();
            if (group1Count != spellTree.Group1.Count && spellTree.Group1.Count > 0)
            {
                return 0;
            }

            var intersectGroup2 = spellTree.Group2.Select(g => g.Name.ToLower()).ToList().Intersect(actor.Character.Spells.Select(sk => sk.SpellName.ToLower()).ToList());
            var group2Count = intersectGroup2.Count();
            if (group2Count != spellTree.Group2.Count && spellTree.Group2.Count > 0)
            {
                return 1;
            }

            var intersectGroup3 = spellTree.Group3.Select(g => g.Name.ToLower()).ToList().Intersect(actor.Character.Spells.Select(sk => sk.SpellName.ToLower()).ToList());
            var group3Count = intersectGroup3.Count();
            if (group3Count != spellTree.Group3.Count && spellTree.Group3.Count > 0)
            {
                return 2;
            }

            var intersectGroup4 = spellTree.Group4.Select(g => g.Name.ToLower()).ToList().Intersect(actor.Character.Spells.Select(sk => sk.SpellName.ToLower()).ToList());
            var group4Count = intersectGroup4.Count();
            if (group4Count != spellTree.Group4.Count && spellTree.Group4.Count > 0)
            {
                return 3;
            }

            var intersectGroup5 = spellTree.Group5.Select(g => g.Name.ToLower()).ToList().Intersect(actor.Character.Spells.Select(sk => sk.SpellName.ToLower()).ToList());
            var group5Count = intersectGroup5.Count();
            if (group5Count != spellTree.Group5.Count && spellTree.Group5.Count > 0)
            {
                return 4;
            }

            return 5;
        }
    }
}