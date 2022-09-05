// <copyright file="ActionProcessor2.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Processors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core;
    using Legendary.Core.Attributes;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Core.Types;
    using Legendary.Engine.Attributes;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Extensions;
    using Legendary.Engine.Helpers;
    using Legendary.Engine.Models;

    /// <summary>
    /// Additional action processing commands.
    /// </summary>
    public partial class ActionProcessor
    {
        [HelpText("<p>Command not yet available.</p>")]
        private async Task DoAdvance(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            await this.communicator.SendToPlayer(actor.Connection, $"Not yet implemented.", cancellationToken);
        }

        [HelpText("<p>List all of the areas, their information, and the author.</p>")]
        private async Task DoAreas(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var area in this.world.Areas)
            {
                sb.Append($"<div class='player-section'>{area.Name}</div>");
                sb.Append($"<span>{area.Description}</span><br/>");
                sb.Append($"<span><em>Created by {area.Author} ({area.Rooms.Count} rooms)</em></span><br/>");
                sb.Append($"<br/>");
            }

            var explored = (double)((double)actor.Character.Metrics.RoomsExplored.Sum(s => s.Value.Count) / (double)this.world.Areas.Sum(a => a.Rooms.Count)) * 100;

            sb.Append($"There are {this.world.Areas.Count} areas in Legendary, with a total of {this.world.Areas.Sum(a => a.Rooms.Count)} rooms. You have explored {(int)explored}% of the entire world.");

            await this.communicator.SendToPlayer(actor.Connection, sb.ToString(), cancellationToken);
        }

        [HelpText("<p>Information about the current area your are in.</p>")]
        private async Task DoArea(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            StringBuilder sb = new StringBuilder();

            var area = this.communicator.ResolveArea(actor.Character.Location);

            if (area != null)
            {
                var totalVisited = actor.Character.Metrics.RoomsExplored.Where(a => a.Key == area.AreaId).Sum(r => r.Value.Count());
                var total = area.Rooms.Count();
                var explorationPct = (double)((double)totalVisited / (double)total) * 100;

                sb.Append($"<div class='player-section'>{area.Name}</div>");
                sb.Append($"<span>{area.Description}</span><br/>");
                sb.Append($"<span><em>Created by {area.Author} ({area.Rooms.Count} rooms)</em></span><br/>");
                sb.Append($"<span>You have explored {(int)explorationPct}% of this area.</span><br/>");
                sb.Append($"<br/>");

                await this.communicator.SendToPlayer(actor.Connection, sb.ToString(), cancellationToken);
            }
        }

        [HelpText("<p>Toggles automatic looting. When active, corpses will be automatically looted when killed.</p>")]
        private async Task DoAutoloot(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (actor.Character.CharacterFlags.Contains(CharacterFlags.Autoloot))
            {
                actor.Character.CharacterFlags.Remove(CharacterFlags.Autoloot);
                await this.communicator.SendToPlayer(actor.Connection, "You will no longer automatically loot corpses.", cancellationToken);
            }
            else
            {
                actor.Character.CharacterFlags.Add(CharacterFlags.Autoloot);
                await this.communicator.SendToPlayer(actor.Connection, "You will now automatically loot corpses.", cancellationToken);
            }
        }

        [HelpText("<p>Toggles automatic sacrificing. When active, corpses will be automatically sacrificed when killed.</p>")]
        private async Task DoAutosac(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (actor.Character.CharacterFlags.Contains(CharacterFlags.Autosac))
            {
                actor.Character.CharacterFlags.Remove(CharacterFlags.Autosac);
                await this.communicator.SendToPlayer(actor.Connection, "You will no longer automatically sacrifice corpses.", cancellationToken);
            }
            else
            {
                actor.Character.CharacterFlags.Add(CharacterFlags.Autosac);
                await this.communicator.SendToPlayer(actor.Connection, "You will now automatically sacrifice corpses.", cancellationToken);
            }
        }

        [HelpText("<p>Lists all awards and accomplishments you have received.</p>")]
        private async Task DoAwards(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (actor.Character.Awards.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<div class='award-container'>");

                foreach (var award in actor.Character.Awards)
                {
                    string color = "bronze";
                    string level = "I";

                    switch (award.AwardLevel)
                    {
                        default:
                        case 0:
                            color = "bronze";
                            level = "I";
                            break;
                        case 1:
                            color = "silver";
                            level = "II";
                            break;
                        case 2:
                            color = "gold";
                            level = "III";
                            break;
                        case 3:
                            color = "platinum";
                            level = "IV";
                            break;
                        case 4:
                            color = "topaz";
                            level = "V";
                            break;
                        case 5:
                            color = "amethyst";
                            level = "VI";
                            break;
                        case 6:
                            color = "emerald";
                            level = "VII";
                            break;
                        case 7:
                            color = "sapphire";
                            level = "VIII";
                            break;
                        case 8:
                            color = "ruby";
                            level = "IX";
                            break;
                        case 9:
                            color = "diamond";
                            level = "X";
                            break;
                    }

                    sb.Append("<div class='award'>");
                    if (award.AwardLevel <= 3)
                    {
                        sb.Append($"<div class='quiz-medal'><div class='quiz-medal-circle quiz-medal-circle-{color}'><span><i class='fa-solid {award.Image}'></i></span></div><div class='quiz-medal-ribbon quiz-medal-ribbon-left'></div><div class='quiz-medal-ribbon quiz-medal-ribbon-right'></div></div>");
                    }
                    else
                    {
                        sb.Append($"<div class='gem-award' id='gem-{color}'><div><i class='fa-solid {award.Image}'></i></div></div>");
                    }

                    sb.Append($"<div class='ribbon-row'><div class='ribbon-block'><h1><span>{award.Name} {level}</span></h1></div></div>");
                    sb.Append("</div>");
                }

                sb.Append("</div>");

                await this.communicator.SendToPlayer(actor.Connection, sb.ToString(), cancellationToken);
            }
            else
            {
                await this.communicator.SendToPlayer(actor.Connection, $"You do not currently have any awards.", cancellationToken);
            }
        }

        [SightRequired]
        [HelpText("<p>Buys an item from a merchant. See also: HELP LIST, HELP SELL<ul><li>buy item</li><ul></p>")]
        private async Task DoBuy(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Buy what?", cancellationToken);
            }
            else
            {
                var mobs = this.communicator.GetMobilesInRoom(actor.Character.Location);
                if (mobs != null)
                {
                    var merchant = mobs.FirstOrDefault(m => m.MobileFlags != null && m.MobileFlags.Contains(Core.Types.MobileFlags.Shopkeeper));
                    if (merchant == null)
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"There isn't a merchant here.", cancellationToken);
                    }
                    else
                    {
                        if (merchant.Equipment != null && merchant.Equipment.Count > 0)
                        {
                            var item = merchant.Equipment.ParseTargetName(args.Method);

                            if (item != null)
                            {
                                var price = item.Value.AdjustSellPrice(actor.Character, merchant);

                                if (actor.Character.Currency >= price)
                                {
                                    await this.communicator.SendToPlayer(actor.Connection, $"You purchase {item.Name} from {merchant.FirstName.FirstCharToUpper()} for {item.Value.ToMerchantSellPrice(actor.Character, merchant)}.", cancellationToken);
                                    await this.communicator.SendToRoom(actor.Character.Location, actor.Character, merchant, $"{actor.Character.FirstName.FirstCharToUpper()} purchases {item.Name} from {merchant.FirstName.FirstCharToUpper()}.", cancellationToken);
                                    actor.Character.Currency -= price;
                                    var newItem = item.DeepCopy();
                                    actor.Character.Inventory.Add(newItem);

                                    await this.communicator.PlaySound(actor.Character, Core.Types.AudioChannel.BackgroundSFX2, Sounds.COINS_BUY, cancellationToken);
                                }
                                else
                                {
                                    await this.communicator.SendToPlayer(actor.Connection, $"You lack the funds to purchase that.", cancellationToken);
                                }
                            }
                            else
                            {
                                await this.communicator.SendToPlayer(actor.Connection, $"{merchant.FirstName.FirstCharToUpper()} does not have that for sale.", cancellationToken);
                            }
                        }
                    }
                }
                else
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"There isn't a merchant here.", cancellationToken);
                }
            }
        }

        [SightRequired]
        [HelpText("<p>Considers a target before attacking it. Among consideration are the target's level, strength, and power.<ul><li>consider <em>target</em></li></ul></p>")]
        private async Task DoConsider(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Consider whom?", cancellationToken);
            }
            else
            {
                var targetName = args.Method.ToLower();

                if (targetName == actor.Character.FirstName || targetName == "self")
                {
                    await this.communicator.SendToPlayer(actor.Connection, "You consider yourself...yourself.", cancellationToken);
                }
                else
                {
                    var target = Communicator.Users?.FirstOrDefault(u => u.Value.Character.FirstName?.ToLower() == targetName.ToLower());

                    if (target == null || target.Value.Value == null)
                    {
                        var mobiles = this.communicator.GetMobilesInRoom(actor.Character.Location);

                        if (mobiles != null)
                        {
                            var mobile = mobiles.ParseTargetName(targetName);

                            if (mobile != null)
                            {
                                await this.communicator.SendToPlayer(actor.Connection, $"You carefully size up {mobile.FirstName}.", cancellationToken);
                                await this.communicator.SendToRoom(actor.Character, actor.Character.Location, $"{actor.Character.FirstName.FirstCharToUpper()} carefully sizes up {mobile.FirstName}.", cancellationToken);

                                var characterPower = (actor.Character.Level * (actor.Character.HitDice + actor.Character.DamageDice)) + actor.Character.Health.Current;
                                var mobilePower = (mobile.Level * (mobile.HitDice + mobile.DamageDice)) + mobile.Health.Current;

                                var powerVariance = characterPower - mobilePower;

                                var message = powerVariance switch
                                {
                                    >= -10000 and <= -101 => $"You may as well try to kill a God.",
                                    >= -100 and <= -51 => $"{mobile.FirstName} would most likely annihilate you.",
                                    >= -50 and <= -21 => "You would need a LOT of luck!",
                                    >= -20 and <= -6 => "You would need a little luck.",
                                    >= -5 and <= 5 => $"{mobile.FirstName} seems like a perfect match.",
                                    >= 6 and <= 20 => $"You're pretty sure you could take {mobile.FirstName}.",
                                    >= 21 and <= 50 => $"You have little doubt you could kill {mobile.FirstName}.",
                                    >= 51 and <= 100 => $"{mobile.FirstName} looks like an easy kill.",
                                    >= 101 and <= 10000 => $"Killing {mobile.FirstName} would be like killing an infant.",
                                    _ => $"You have no idea if you could take {mobile.FirstName} or not."
                                };

                                await this.communicator.SendToPlayer(actor.Connection, message, cancellationToken);
                            }
                            else
                            {
                                await this.communicator.SendToPlayer(actor.Connection, "They're not here.", cancellationToken);
                            }
                        }
                        else
                        {
                            await this.communicator.SendToPlayer(actor.Connection, "They're not here.", cancellationToken);
                        }
                    }
                    else
                    {
                        var targetPlayer = target.Value.Value.Character;

                        await this.communicator.SendToPlayer(actor.Connection, $"You carefully size up {targetPlayer.FirstName}.", cancellationToken);
                        await this.communicator.SendToRoom(actor.Character, actor.Character.Location, $"{actor.Character.FirstName.FirstCharToUpper()} carefully sizes up {targetPlayer.FirstName}.", cancellationToken);

                        var characterPower = (actor.Character.Level * (actor.Character.HitDice + actor.Character.DamageDice)) + actor.Character.Health.Current;
                        var targetPower = (targetPlayer.Level * (targetPlayer.HitDice + targetPlayer.DamageDice)) + targetPlayer.Health.Current;

                        var powerVariance = characterPower - targetPower;

                        var message = powerVariance switch
                        {
                            >= -10000 and <= -101 => $"You may as well try to kill a God.",
                            >= -100 and <= -51 => $"{targetPlayer.FirstName} would most likely annihilate you.",
                            >= -50 and <= -21 => "You would need a LOT of luck!",
                            >= -20 and <= -6 => "You would need a little luck.",
                            >= -5 and <= 5 => $"{targetPlayer.FirstName} seems like a perfect match.",
                            >= 6 and <= 20 => $"You're pretty sure you could take {targetPlayer.FirstName}.",
                            >= 21 and <= 50 => $"You have little doubt you could kill {targetPlayer.FirstName}.",
                            >= 51 and <= 100 => $"{targetPlayer.FirstName} looks like an easy kill.",
                            >= 101 and <= 10000 => $"Killing {targetPlayer.FirstName} would be like killing an infant.",
                            _ => $"You have no idea if you could take {targetPlayer.FirstName} or not."
                        };

                        await this.communicator.SendToPlayer(actor.Connection, message, cancellationToken);
                    }
                }
            }
        }

        [SightRequired]
        [HelpText("<p>When accompanied by a teacher, use this command to learn new skill or spell trees. See also: HELP PRACTICE, HELP TRAIN<ul><li>learn <em>tree</em></li></ul></p>")]
        private async Task DoLearn(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            var mobs = this.communicator.GetMobilesInRoom(actor.Character.Location);
            if (mobs != null)
            {
                var teacher = mobs.FirstOrDefault(m => m.MobileFlags != null && m.MobileFlags.Contains(Core.Types.MobileFlags.Teacher));
                if (teacher == null)
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"There isn't a teacher here.", cancellationToken);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(args.Method) && string.IsNullOrWhiteSpace(args.Target))
                    {
                        await this.ShowAvailableTrees(actor, teacher, cancellationToken);
                    }
                    else
                    {
                        if (actor.Character.Learns <= 0)
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"You don't have any learning sessions available.", cancellationToken);
                        }
                        else
                        {
                            // [learn] [martial] group [II]
                            var action = args.Action;
                            var groupName = args.Method ?? string.Empty;
                            var groupNum = args.Target ?? string.Empty;

                            // Check if this is a) available to learn and b) has been learned already.
                            if (!TreeHelper.CanLearnGroup(actor, groupName, groupNum, teacher, this.communicator, this.random, this.world, this.logger, this.combat))
                            {
                                await this.communicator.SendToPlayer(actor.Connection, $"You are not able to learn that group at this time and place.", cancellationToken);
                            }
                            else
                            {
                                // Add the skill or spell group abilities to the player's proficiencies, all starting at 1%
                                var skillsToAdd = TreeHelper.GetSkills(groupName, groupNum, this.communicator, this.random, this.world, this.logger, this.combat);
                                var spellsToAdd = TreeHelper.GetSpells(groupName, groupNum, this.communicator, this.random, this.world, this.logger, this.combat);

                                StringBuilder sb = new StringBuilder();

                                // We will teach skills or spells, but not both.
                                if (skillsToAdd != null && skillsToAdd.Count > 0)
                                {
                                    // Deduct a learning session
                                    actor.Character.Learns -= 1;

                                    foreach (var skill in skillsToAdd)
                                    {
                                        sb.Append($"{teacher.FirstName.FirstCharToUpper()} instructs you on the various methods of '{skill.Name}'.<br/>");
                                        actor.Character.Skills.Add(new SkillProficiency(skill.Name, 1));
                                    }
                                }
                                else if (spellsToAdd != null && spellsToAdd.Count > 0)
                                {
                                    // Deduct a learning session
                                    actor.Character.Learns -= 1;

                                    foreach (var spell in spellsToAdd)
                                    {
                                        sb.Append($"{teacher.FirstName.FirstCharToUpper()} instructs you on the uses of the spell '{spell.Name}'.<br/>");
                                        actor.Character.Spells.Add(new SpellProficiency(spell.Name, 1));
                                    }
                                }
                                else
                                {
                                    sb.Append($"There was nothing to learn in that group.<br/>");
                                }

                                await this.communicator.SendToPlayer(actor.Connection, sb.ToString(), cancellationToken);
                                await this.communicator.SaveCharacter(actor);
                            }
                        }
                    }
                }
            }
            else
            {
                await this.communicator.SendToPlayer(actor.Connection, $"There isn't a teacher here.", cancellationToken);
            }
        }

        [SightRequired]
        [HelpText("<p>Lists all items for sale by a merchant. See also: HELP SELL, HELP BUY</p>")]
        private async Task DoList(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            var mobs = this.communicator.GetMobilesInRoom(actor.Character.Location);
            if (mobs != null)
            {
                var merchant = mobs.FirstOrDefault(m => m.MobileFlags != null && m.MobileFlags.Contains(Core.Types.MobileFlags.Shopkeeper));
                if (merchant == null)
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"There isn't a merchant here.", cancellationToken);
                }
                else
                {
                    StringBuilder sb = new StringBuilder();

                    if (merchant.Equipment != null && merchant.Equipment.Count > 0)
                    {
                        sb.Append($"{merchant.FirstName.FirstCharToUpper()} offers the following items for sale:<br/><ul>");

                        var equipment = merchant.Equipment.GroupBy(g => g.ItemId);

                        foreach (var group in equipment)
                        {
                            var item = group.First();

                            if (item.WearLocation != null && !item.WearLocation.Contains(Core.Types.WearLocation.None))
                            {
                                switch (item.ItemType)
                                {
                                    case Core.Types.ItemType.Currency:
                                    case Core.Types.ItemType.Spring:
                                        break;
                                    default:
                                        {
                                            if (item.Value > 0)
                                            {
                                                var price = item.Value.ToMerchantSellPrice(actor.Character, merchant);
                                                sb.Append($"<li>{item.Name}, for {price}.</li>");
                                            }

                                            break;
                                        }
                                }
                            }
                        }

                        sb.Append($"</ul>");

                        await this.communicator.SendToPlayer(actor.Connection, sb.ToString(), cancellationToken);
                    }
                    else
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"{merchant.FirstName.FirstCharToUpper()} is not currently offering anything for sale.", cancellationToken);
                    }
                }
            }
            else
            {
                await this.communicator.SendToPlayer(actor.Connection, $"There isn't a merchant here.", cancellationToken);
            }
        }

        [SightRequired]
        [HelpText("<p>Command not yet available.</p>")]
        private async Task DoGain(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            await this.communicator.SendToPlayer(actor.Connection, $"Not yet implemented.", cancellationToken);
        }

        [SightRequired]
        [HelpText("<p>Adds a member to your group. Groups can communicate privately with GTELL See HELP GTELL.<p><ul><li>group <em>player</em></li></ul>")]
        private async Task DoGroup(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.ShowGroupToPlayer(actor, cancellationToken);
            }
            else
            {
                var targetPlayer = this.communicator.ResolveCharacter(args.Method);

                if (targetPlayer != null)
                {
                    // The target needs to be in the same room.
                    if (targetPlayer.Character.Location.Value != actor.Character.Location.Value)
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"They aren't here.", cancellationToken);
                    }
                    else if (targetPlayer.Character.Following != actor.Character.CharacterId)
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"{targetPlayer.Character.FirstName} isn't following you.", cancellationToken);
                    }
                    else
                    {
                        // Ensure target player isn't already in a group.
                        var isTargetInGroup = GroupHelper.IsInGroup(targetPlayer.Character.CharacterId);

                        if (isTargetInGroup)
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"{targetPlayer.Character.FirstName} is already in a group.", cancellationToken);
                        }
                        else
                        {
                            // Is the actor currently in a group?
                            var isActorInGroup = GroupHelper.IsInGroup(actor.Character.CharacterId);

                            if (isActorInGroup && actor.Character.GroupId.HasValue)
                            {
                                // Actor is in a group, see if they own the group.
                                if (GroupHelper.IsGroupOwner(actor.Character.CharacterId))
                                {
                                    GroupHelper.AddToGroup(actor.Character.CharacterId, targetPlayer.Character.CharacterId);
                                    await this.communicator.SendToPlayer(actor.Connection, $"You add {targetPlayer.Character.FirstName} to your group.", cancellationToken);
                                    await this.communicator.SendToPlayer(targetPlayer.Connection, $"{actor.Character.FirstName} adds you to {actor.Character.Pronoun} group.", cancellationToken);
                                    targetPlayer.Character.GroupId = actor.Character.CharacterId;
                                    await this.communicator.SaveCharacter(targetPlayer);
                                }
                                else
                                {
                                    await this.communicator.SendToPlayer(actor.Connection, $"You can't add {targetPlayer.Character.FirstName} to your group, because you're not the group leader.", cancellationToken);
                                }
                            }
                            else
                            {
                                // Actor is not in a group, so create one.
                                if (GroupHelper.AddToGroup(actor.Character.CharacterId, targetPlayer.Character.CharacterId))
                                {
                                    await this.communicator.SendToPlayer(actor.Connection, $"You add {targetPlayer.Character.FirstName} to your group.", cancellationToken);
                                    await this.communicator.SendToPlayer(targetPlayer.Connection, $"{actor.Character.FirstName} adds you to {actor.Character.Pronoun} group.", cancellationToken);
                                    actor.Character.GroupId = actor.Character.CharacterId;
                                    targetPlayer.Character.GroupId = actor.Character.CharacterId;
                                    await this.communicator.SaveCharacter(actor);
                                    await this.communicator.SaveCharacter(targetPlayer);
                                }
                                else
                                {
                                    // This would be caused by a bug.
                                    await this.communicator.SendToPlayer(actor.Connection, $"You were unable to add {targetPlayer.Character.FirstName} to your group.", cancellationToken);
                                }
                            }
                        }
                    }
                }
                else
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"They aren't here.", cancellationToken);
                }
            }
        }

        [HelpText("<p>Sends a message to everyone in your group.</p><ul><li>gtell <em>message</em></li></ul>")]
        private async Task DoGTell(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            var sentence = args.Method;
            if (!string.IsNullOrWhiteSpace(sentence))
            {
                if (GroupHelper.IsInGroup(actor.Character.CharacterId) && actor.Character.GroupId != null)
                {
                    var group = GroupHelper.GetGroup(actor.Character.GroupId.Value);

                    if (group != null && group.Value.Value.Count > 0)
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"You tell the group \"<span class='gtell'>{sentence}</span>\"", cancellationToken);

                        // If the player isn't the group owner, send a message to the actual owner.
                        if (!GroupHelper.IsGroupOwner(actor.Character.CharacterId))
                        {
                            await this.communicator.SendToPlayer(group.Value.Key, $"{actor.Character.FirstName.FirstCharToUpper()} tells the group \"<span class='gtell'>{sentence}</span>\"", cancellationToken);
                        }

                        // Now send a message to everyone else in the group.
                        foreach (var target in group.Value.Value)
                        {
                            if (target != actor.Character.CharacterId)
                            {
                                await this.communicator.SendToPlayer(target, $"{actor.Character.FirstName.FirstCharToUpper()} tells the group \"<span class='gtell'>{sentence}</span>\"", cancellationToken);
                            }
                        }
                    }
                    else
                    {
                        // Shouldn't happen, but if there are no group members, just remove the group.
                        GroupHelper.RemoveGroup(actor.Character.GroupId.Value);
                        actor.Character.GroupId = null;
                        await this.communicator.SendToPlayer(actor.Connection, $"You aren't in a group.", cancellationToken);
                    }
                }
                else
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"You aren't in a group.", cancellationToken);
                }
            }
            else
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Tell your group what?", cancellationToken);
            }
        }

        [HelpText("<p>Outfits your player with a very basic set of travel equipment. Useful if you find yourself naked.</p><ul><li>outfit</li></ul>")]
        private async Task DoOutfit(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (actor.Character.DivineFavor < 0)
            {
                await this.communicator.SendToPlayer(actor.Connection, $"The Gods do not deem you worthy of their attention. Perhaps do something to garner their favor.", cancellationToken);
            }
            else
            {
                var head = actor.Character.Equipment.FirstOrDefault(f => f.WearLocation.Contains(WearLocation.Head));

                List<Item> itemsToEquip = new List<Item>();

                if (head == null)
                {
                    itemsToEquip.Add(ItemHelper.CreatePracticeGear(this.random, WearLocation.Head));
                }

                var torso = actor.Character.Equipment.FirstOrDefault(f => f.WearLocation.Contains(WearLocation.Torso));

                if (torso == null)
                {
                    itemsToEquip.Add(ItemHelper.CreatePracticeGear(this.random, WearLocation.Torso));
                }

                var feet = actor.Character.Equipment.FirstOrDefault(f => f.WearLocation.Contains(WearLocation.Feet));

                if (feet == null)
                {
                    itemsToEquip.Add(ItemHelper.CreatePracticeGear(this.random, WearLocation.Feet));
                }

                var arms = actor.Character.Equipment.FirstOrDefault(f => f.WearLocation.Contains(WearLocation.Arms));

                if (torso == null)
                {
                    itemsToEquip.Add(ItemHelper.CreatePracticeGear(this.random, WearLocation.Arms));
                }

                var hands = actor.Character.Equipment.FirstOrDefault(f => f.WearLocation.Contains(WearLocation.Hands));

                if (hands == null)
                {
                    itemsToEquip.Add(ItemHelper.CreatePracticeGear(this.random, WearLocation.Hands));
                }

                var weapon = actor.Character.Equipment.FirstOrDefault(f => f.WearLocation.Contains(WearLocation.Wielded));

                if (weapon == null)
                {
                    itemsToEquip.Add(ItemHelper.CreatePracticeWeapon(this.random));
                }

                var light = actor.Character.Equipment.FirstOrDefault(f => f.WearLocation.Contains(WearLocation.Light));

                if (light == null)
                {
                    itemsToEquip.Add(ItemHelper.CreateLight(this.random));
                }

                if (itemsToEquip.Count > 0)
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"The Gods have taken pity on you, and you have been re-equipped by them. You have lost some divine favor.", cancellationToken);
                    actor.Character.DivineFavor -= itemsToEquip.Count;

                    foreach (var item in itemsToEquip)
                    {
                        actor.Character.Inventory.Add(item);
                        await this.communicator.SendToPlayer(actor.Connection, $"You have received {item.Name}.", cancellationToken);
                    }
                }
                else
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"The Gods are insulted, and do not feel you presently need their handouts. You have lost some divine favor.", cancellationToken);
                    actor.Character.DivineFavor -= 10;
                }
            }
        }

        [SightRequired]
        [HelpText("<p>When accompanied by a guild master, use this command to train up your skills. See also: HELP LEARN, HELP TRAIN<ul><li>practice skill</li><li>practice spell</li></ul></p>")]
        private async Task DoPractice(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            var mobs = this.communicator.GetMobilesInRoom(actor.Character.Location);
            if (mobs != null)
            {
                var gm = mobs.FirstOrDefault(m => m.MobileFlags != null && m.MobileFlags.Contains(Core.Types.MobileFlags.Guildmaster));
                if (gm == null)
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"There isn't a guildmaster here.", cancellationToken);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(args.Method))
                    {
                        StringBuilder sb = new StringBuilder();

                        if (actor.Character.Practices > 0)
                        {
                            bool canPractice = false;

                            sb.Append("<div class='skillgroups'>");
                            var engine = Assembly.Load("Legendary.Engine");

                            var skillTrees = engine.GetTypes().Where(t => t.Namespace == "Legendary.Engine.Models.SkillTrees");

                            foreach (var tree in skillTrees)
                            {
                                var treeInstance = Activator.CreateInstance(tree, this.communicator, this.random, this.world, this.logger, this.combat);

                                if (treeInstance != null && treeInstance is IActionTree instance)
                                {
                                    var groupProps = tree.GetProperties();

                                    StringBuilder sbTree = new StringBuilder();

                                    bool hasSkillInGroup = false;

                                    for (var x = 1; x <= 5; x++)
                                    {
                                        var spellGroup = groupProps.FirstOrDefault(g => g.Name == $"Group{x}");

                                        if (spellGroup != null)
                                        {
                                            var obj = spellGroup.GetValue(treeInstance);

                                            if (obj != null)
                                            {
                                                var group = (List<IAction>)obj;

                                                foreach (var action in group)
                                                {
                                                    if (actor.Character.HasSkill(action.Name.ToLower()))
                                                    {
                                                        var proficiency = actor.Character.GetSkillProficiency(action.Name.ToLower());
                                                        if (proficiency != null && proficiency.Proficiency < 75)
                                                        {
                                                            sbTree.Append($"<span class='skillinfo'>{proficiency.SkillName} {proficiency.Proficiency}% <progress class='skillprogress' max='100' value='{proficiency.Progress}'>{proficiency.Progress}%</progress></span>");
                                                            hasSkillInGroup = true;
                                                            canPractice = true;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    if (hasSkillInGroup)
                                    {
                                        sb.Append($"<div><span class='skillgroup'>{instance.Name}</span>");
                                        sb.Append(sbTree.ToString());
                                    }
                                }

                                sb.Append("</div>");
                            }

                            var spellTrees = engine.GetTypes().Where(t => t.Namespace == "Legendary.Engine.Models.SpellTrees");

                            foreach (var tree in spellTrees)
                            {
                                var treeInstance = Activator.CreateInstance(tree, this.communicator, this.random, this.world, this.logger, this.combat);

                                if (treeInstance != null && treeInstance is IActionTree instance)
                                {
                                    StringBuilder sbTree = new StringBuilder();

                                    var groupProps = tree.GetProperties();

                                    bool hasSpellInGroup = false;

                                    for (var x = 1; x <= 5; x++)
                                    {
                                        var spellGroup = groupProps.FirstOrDefault(g => g.Name == $"Group{x}");

                                        if (spellGroup != null)
                                        {
                                            var obj = spellGroup.GetValue(treeInstance);

                                            if (obj != null)
                                            {
                                                var group = (List<IAction>)obj;

                                                foreach (var action in group)
                                                {
                                                    if (actor.Character.HasSpell(action.Name.ToLower()))
                                                    {
                                                        var proficiency = actor.Character.GetSpellProficiency(action.Name.ToLower());
                                                        if (proficiency != null && proficiency.Proficiency < 75)
                                                        {
                                                            sbTree.Append($"<span class='spellinfo'>{proficiency.SpellName} {proficiency.Proficiency}% <progress class='spellprogress' max='100' value='{proficiency.Progress}'>{proficiency.Progress}%</progress></span>");
                                                            hasSpellInGroup = true;
                                                            canPractice = true;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    if (hasSpellInGroup)
                                    {
                                        sb.Append($"<div><span class='skillgroup'>{instance.Name}</span>");
                                        sb.Append(sbTree.ToString());
                                    }
                                }

                                sb.Append("</div>");
                            }

                            sb.Append("</div>");

                            if (canPractice)
                            {
                                await this.communicator.SendToPlayer(actor.Character, $"{gm.FirstName.FirstCharToUpper()} says \"<span class='say'>Sure, {actor.Character.FirstName.FirstCharToUpper()}, you have {actor.Character.Practices} practice sessions, and here's what we can practice.</span>\"", cancellationToken);
                                await this.communicator.SendToPlayer(actor.Connection, sb.ToString(), cancellationToken);
                            }
                            else
                            {
                                await this.communicator.SendToPlayer(actor.Character, $"{gm.FirstName.FirstCharToUpper()} says \"<span class='say'>Sorry, {actor.Character.FirstName.FirstCharToUpper()}, looks like you'll have to practice the rest on your own.</span>\"", cancellationToken);
                            }
                        }
                        else
                        {
                            await this.communicator.SendToPlayer(actor.Character, $"{gm.FirstName.FirstCharToUpper()} says \"<span class='say'>I'm sorry, {actor.Character.FirstName.FirstCharToUpper()}, but you have no practice sessions available.</span>\"", cancellationToken);
                        }
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();

                        if (actor.Character.Practices > 0)
                        {
                            if (actor.Character.HasSkill(args.Method))
                            {
                                var skillProf = actor.Character.GetSkillProficiency(args.Method);

                                if (skillProf != null)
                                {
                                    if (skillProf.Proficiency >= 75)
                                    {
                                        await this.communicator.SendToPlayer(actor.Character, $"{gm.FirstName.FirstCharToUpper()} says \"<span class='say'>I'm sorry, {actor.Character.FirstName.FirstCharToUpper()}, but I have helped you all I can on skill.</span>\"", cancellationToken);
                                    }
                                    else
                                    {
                                        int advance = ((int)actor.Character.Wis.Current * 2) + this.random.Next(1, (int)actor.Character.Dex.Current);
                                        skillProf.Proficiency = Math.Min(75, skillProf.Proficiency + advance);
                                        skillProf.Progress = 0;
                                        actor.Character.Practices -= 1;

                                        await this.communicator.SendToPlayer(actor.Character, $"{gm.FirstName.FirstCharToUpper()} helps you practice {skillProf.SkillName.FirstCharToUpper()}, and your proficiency increases!", cancellationToken);
                                        await this.awardProcessor.GrantAward(4, actor.Character, "met a guildmaster", cancellationToken);
                                    }
                                }
                                else
                                {
                                    await this.communicator.SendToPlayer(actor.Character, "You are not proficient in that.", cancellationToken);
                                }
                            }
                            else if (actor.Character.HasSpell(args.Method))
                            {
                                var spellProf = actor.Character.GetSpellProficiency(args.Method);

                                if (spellProf != null)
                                {
                                    if (spellProf.Proficiency >= 75)
                                    {
                                        await this.communicator.SendToPlayer(actor.Character, $"{gm.FirstName.FirstCharToUpper()} says \"<span class='say'>I'm sorry, {actor.Character.FirstName.FirstCharToUpper()}, but I have helped you all I can on that spell.</span>\"", cancellationToken);
                                    }
                                    else
                                    {
                                        int advance = ((int)actor.Character.Wis.Current * 2) + this.random.Next(1, (int)actor.Character.Int.Current);
                                        spellProf.Proficiency = Math.Min(75, spellProf.Proficiency + advance);
                                        spellProf.Progress = 0;
                                        actor.Character.Practices -= 1;

                                        await this.communicator.SendToPlayer(actor.Character, $"{gm.FirstName.FirstCharToUpper()} helps you practice {spellProf.SpellName.FirstCharToUpper()}, and your proficiency increases!", cancellationToken);
                                        await this.awardProcessor.GrantAward(4, actor.Character, "met a guildmaster", cancellationToken);
                                    }
                                }
                                else
                                {
                                    await this.communicator.SendToPlayer(actor.Character, "You are not proficient in that.", cancellationToken);
                                }
                            }
                            else
                            {
                                await this.communicator.SendToPlayer(actor.Character, $"{gm.FirstName.FirstCharToUpper()} says \"<span class='say'>I'm sorry, {actor.Character.FirstName.FirstCharToUpper()}, I can't help you practice that.</span>\"", cancellationToken);
                            }
                        }
                        else
                        {
                            await this.communicator.SendToPlayer(actor.Character, $"{gm.FirstName.FirstCharToUpper()} says \"<span class='say'>I'm sorry, {actor.Character.FirstName.FirstCharToUpper()}, but you have no practice sessions available.</span>\"", cancellationToken);
                        }

                        await this.communicator.SendToPlayer(actor.Connection, sb.ToString(), cancellationToken);

                        await this.communicator.SaveCharacter(actor);
                    }
                }
            }
        }

        [SightRequired]
        [HelpText("<p>Puts an item or items from your inventory into another item.</p><ul><li>put <em>item</em> <em>target</em></li><li>put all <em>target</em></li></ul>")]
        private async Task DoPut(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method) || string.IsNullOrEmpty(args.Target))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Put what where?", cancellationToken);
            }
            else
            {
                // See if it's an item in the room.
                var items = this.communicator.GetItemsInRoom(actor.Character.Location);
                var item = items?.ParseTargetName(args.Target);

                if (item == null)
                {
                    // See if it's an item the player is carrying.
                    item = actor.Character.Inventory.ParseTargetName(args.Target);
                }

                if (item != null)
                {
                    if (item.IsClosed)
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"{item.Name.FirstCharToUpper()} is closed.", cancellationToken);
                    }
                    else if (item.ItemType == Core.Types.ItemType.Container)
                    {
                        if (item.Contains == null)
                        {
                            item.Contains = new List<IItem>();
                        }

                        if (args.Method == "all")
                        {
                            var itemsToRemove = new List<Item>();

                            foreach (var target in actor.Character.Inventory)
                            {
                                if (target != item)
                                {
                                    itemsToRemove.Add(target);
                                    item.Contains.Add(target);

                                    await this.communicator.SendToPlayer(actor.Connection, $"You put {target.Name} in {item.Name}.", cancellationToken);
                                    await this.communicator.SendToRoom(actor.Character, actor.Character.Location, $"{actor.Character.FirstName.FirstCharToUpper()} puts {target.Name} in {item.Name}.", cancellationToken);
                                }
                            }

                            foreach (var itemToRemove in itemsToRemove)
                            {
                                actor.Character.Inventory.Remove(itemToRemove);
                            }
                        }
                        else
                        {
                            var target = actor.Character.Inventory.ParseTargetName(args.Method);

                            if (target != null && target != item)
                            {
                                actor.Character.Inventory.Remove(target);
                                item.Contains.Add(target);

                                await this.communicator.SendToPlayer(actor.Connection, $"You put {target.Name} in {item.Name}.", cancellationToken);
                                await this.communicator.SendToRoom(actor.Character, actor.Character.Location, $"{actor.Character.FirstName.FirstCharToUpper()} puts {target.Name} in {item.Name}.", cancellationToken);
                            }
                            else if (target == item)
                            {
                                await this.communicator.SendToPlayer(actor.Connection, $"You can't put {item.Name.FirstCharToUpper()} inside of itself.", cancellationToken);
                            }
                            else
                            {
                                await this.communicator.SendToPlayer(actor.Connection, $"You don't have that item.", cancellationToken);
                            }
                        }
                    }
                    else
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"{item.Name.FirstCharToUpper()} is not a container.", cancellationToken);
                    }
                }
                else
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"You don't see that here.", cancellationToken);
                }
            }
        }

        [SightRequired]
        [HelpText("<p>Quaffs an item of type potion.</p><ul><li>quaff <em>potion</em></li></ul>")]
        private async Task DoQuaff(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            // See if they have a potion in their inventory.
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Quaff what?", cancellationToken);
            }
            else
            {
                // See if it's an item they are carrying.
                var item = actor.Character.Inventory.ParseTargetName(args.Method);

                if (item != null)
                {
                    if (item.ItemType == ItemType.Potion)
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"You toss your head back and quaff {item.Name}.", cancellationToken);
                        await this.communicator.SendToRoom(actor.Character.Location, actor.Character, null, $"{actor.Character.FirstName.FirstCharToUpper()} throws {actor.Character.Pronoun} head back and quaffs {item.Name}.", cancellationToken);

                        if (!string.IsNullOrWhiteSpace(item.SpellName))
                        {
                            var spell = SpellHelper.ResolveSpell(item.SpellName, this.communicator, this.random, this.world, this.logger, this.combat);

                            if (spell != null)
                            {
                                // Cache the actor's level.
                                var level = actor.Character.Level;

                                // Set the actor's level to the cast level, temporarily.
                                actor.Character.Level = item.CastLevel ?? actor.Character.Level;

                                // Cast the spell as the actor, with a null target (targets self).
                                await spell.Act(actor.Character, null, cancellationToken);

                                // Restore the actor's actual level.
                                actor.Character.Level = level;
                            }
                        }

                        actor.Character.Inventory.Remove(item);
                    }
                }
                else
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"That's not a potion.", cancellationToken);
                }
            }
        }

        [HelpText("<p>Reports your current health, mana, and movement levels out loud.</p>")]
        private async Task DoReport(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            StringBuilder sb = new StringBuilder();

            var healthPct = actor.Character.Health.GetPercentage();

            if (healthPct >= 95)
            {
                sb.Append("I'm in excellent health, ");
            }
            else if (healthPct >= 75)
            {
                sb.Append("I'm a little banged up, but ok, ");
            }
            else if (healthPct >= 50)
            {
                sb.Append("I'm over half dead, ");
            }
            else if (healthPct >= 25)
            {
                sb.Append("I'm extremely beat up, ");
            }
            else
            {
                sb.Append("I'm practically on my death bed, ");
            }

            var manaPct = actor.Character.Mana.GetPercentage();

            if (manaPct >= 95)
            {
                sb.Append("my mind is crystal clear, ");
            }
            else if (manaPct >= 75)
            {
                sb.Append("my head is just a little cloud, ");
            }
            else if (manaPct >= 50)
            {
                sb.Append("I'm having a hard time focusing, ");
            }
            else if (manaPct >= 25)
            {
                sb.Append("I can hardly focus, ");
            }
            else
            {
                sb.Append("I'm absolutely mentally drained, ");
            }

            var movePct = actor.Character.Movement.GetPercentage();

            if (movePct >= 95)
            {
                sb.Append("and I'm fully refreshed.");
            }
            else if (movePct >= 75)
            {
                sb.Append("but my legs are only a little bit sore.");
            }
            else if (movePct >= 50)
            {
                sb.Append("and I'm a pretty tired from moving around.");
            }
            else if (movePct >= 25)
            {
                sb.Append("and I should probably rest my legs soon.");
            }
            else
            {
                sb.Append("and I can hardly stand up.");
            }

            await this.communicator.SendToPlayer(actor.Connection, $"You report, \"<span class='say'>{sb.ToString()}</span>\".", cancellationToken);
            await this.communicator.SendToRoom(actor.Character, actor.Character.Location, $"{actor.Character.FirstName.FirstCharToUpper()} reports, \"<span class='say'>{sb.ToString()}</span>\".", cancellationToken);
        }

        [SightRequired]
        [HelpText("<p>Sells an item to a merchant. See also: HELP LIST, HELP BUY<ul><li>sell item</li><ul></p>")]
        private async Task DoSell(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Sell what?", cancellationToken);
            }
            else
            {
                var mobs = this.communicator.GetMobilesInRoom(actor.Character.Location);

                if (mobs != null)
                {
                    var merchant = mobs.FirstOrDefault(m => m.MobileFlags != null && m.MobileFlags.Contains(Core.Types.MobileFlags.Shopkeeper));
                    if (merchant == null)
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"There isn't a merchant here.", cancellationToken);
                    }
                    else
                    {
                        if (actor.Character.Inventory != null && actor.Character.Inventory.Count > 0)
                        {
                            var item = actor.Character.Inventory.ParseTargetName(args.Method);

                            if (item != null)
                            {
                                if (item.Value == 0)
                                {
                                    await this.communicator.SendToPlayer(actor.Connection, $"{merchant.FirstName.FirstCharToUpper()} looks to have no interest in {item.Name}.", cancellationToken);
                                }
                                else
                                {
                                    var price = item.Value.AdjustBuyPrice(actor.Character, merchant);

                                    if (merchant.Currency >= price)
                                    {
                                        await this.communicator.SendToPlayer(actor.Connection, $"You sell {item.Name} to {merchant.FirstName.FirstCharToUpper()} for {item.Value.ToMerchantBuyPrice(actor.Character, merchant)}.", cancellationToken);
                                        await this.communicator.SendToRoom(actor.Character.Location, actor.Character, merchant, $"{actor.Character.FirstName.FirstCharToUpper()} sells {item.Name} to {merchant.FirstName.FirstCharToUpper()}.", cancellationToken);
                                        actor.Character.Currency += price;
                                        var newItem = item.DeepCopy();
                                        merchant.Equipment.Add(newItem);
                                        merchant.Currency -= price;
                                        actor.Character.Inventory.Remove(item);

                                        await this.communicator.PlaySound(actor.Character, Core.Types.AudioChannel.BackgroundSFX2, Sounds.COINS_SELL, cancellationToken);
                                    }
                                    else
                                    {
                                        await this.communicator.SendToPlayer(actor.Connection, $"{merchant.FirstName.FirstCharToUpper()} likes {item.Name}, but lacks the funds to purchase it at that price.", cancellationToken);
                                    }
                                }
                            }
                            else
                            {
                                await this.communicator.SendToPlayer(actor.Connection, $"You do not have that for sale.", cancellationToken);
                            }
                        }
                    }
                }
                else
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"There isn't a merchant here.", cancellationToken);
                }
            }
        }

        [SightRequired]
        [HelpText("<p>When accompanied by a trainer, use this command to train up your vital attributes. See also: HELP PRACTICE, HELP LEARN<ul><li>train str</li><li>train hp</li></ul></p>")]
        private async Task DoTrain(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            var mobs = this.communicator.GetMobilesInRoom(actor.Character.Location);
            if (mobs != null)
            {
                var trainer = mobs.FirstOrDefault(m => m.MobileFlags != null && m.MobileFlags.Contains(Core.Types.MobileFlags.Trainer));
                if (trainer == null)
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"There isn't a trainer here.", cancellationToken);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(args.Method))
                    {
                        StringBuilder sb = new StringBuilder();

                        if (actor.Character.Trains > 0)
                        {
                            await this.communicator.SendToPlayer(actor.Character, $"{trainer.FirstName.FirstCharToUpper()} says \"<span class='say'>Sure, {actor.Character.FirstName.FirstCharToUpper()}, I can help you with that.</span>\"", cancellationToken);

                            sb.Append($"You have {actor.Character.Trains} training sessions, and you can train the following:<br/>");

                            if (actor.Character.Str.Max < 18)
                            {
                                sb.Append("Strength (STR), ");
                            }

                            if (actor.Character.Int.Max < 18)
                            {
                                sb.Append("Intelligence (INT), ");
                            }

                            if (actor.Character.Wis.Max < 18)
                            {
                                sb.Append("Wisdom (WIS), ");
                            }

                            if (actor.Character.Dex.Max < 18)
                            {
                                sb.Append("Dexterity (DEX), ");
                            }

                            if (actor.Character.Con.Max < 18)
                            {
                                sb.Append("Constitution (CON), ");
                            }

                            sb.Append("Health (HP), ");
                            sb.Append("Mana (MANA), ");
                            sb.Append("Movement (MOVE). ");
                        }
                        else
                        {
                            await this.communicator.SendToPlayer(actor.Character, $"{trainer.FirstName.FirstCharToUpper()} says \"<span class='say'>I'm sorry, {actor.Character.FirstName.FirstCharToUpper()}, but you have no training sessions available.</span>\"", cancellationToken);
                        }

                        await this.communicator.SendToPlayer(actor.Connection, sb.ToString(), cancellationToken);
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();

                        if (actor.Character.Trains > 0)
                        {
                            switch (args.Method.ToLower())
                            {
                                default:
                                    sb.Append("You can't train that.");
                                    break;
                                case "str":
                                    if (actor.Character.Str.Max >= 18)
                                    {
                                        sb.Append("Your strength is already at maximum.");
                                    }
                                    else
                                    {
                                        actor.Character.Str = new Core.Types.MaxCurrent(actor.Character.Str.Max + 1, actor.Character.Str.Current + 1);
                                        sb.Append("You train your strength, and feel much stronger!");
                                        actor.Character.Trains -= 1;
                                    }

                                    break;
                                case "int":
                                    if (actor.Character.Int.Max >= 18)
                                    {
                                        sb.Append("Your intelligence is already at maximum.");
                                    }
                                    else
                                    {
                                        actor.Character.Int = new Core.Types.MaxCurrent(actor.Character.Int.Max + 1, actor.Character.Int.Current + 1);
                                        sb.Append("You train your intelligence, and feel much smarter!");
                                        actor.Character.Trains -= 1;
                                    }

                                    break;
                                case "wis":
                                    if (actor.Character.Wis.Max >= 18)
                                    {
                                        sb.Append("Your wisdom is already at maximum.");
                                    }
                                    else
                                    {
                                        actor.Character.Wis = new Core.Types.MaxCurrent(actor.Character.Wis.Max + 1, actor.Character.Wis.Current + 1);
                                        sb.Append("You train your wisdom, and feel much wiser!");
                                        actor.Character.Trains -= 1;
                                    }

                                    break;
                                case "dex":
                                    if (actor.Character.Dex.Max >= 18)
                                    {
                                        sb.Append("Your dexterity is already at maximum.");
                                    }
                                    else
                                    {
                                        actor.Character.Dex = new Core.Types.MaxCurrent(actor.Character.Dex.Max + 1, actor.Character.Dex.Current + 1);
                                        sb.Append("You train your dexterity, and feel much more agile!");
                                        actor.Character.Trains -= 1;
                                    }

                                    break;
                                case "con":
                                    if (actor.Character.Con.Max >= 18)
                                    {
                                        sb.Append("Your constitution is already at maximum.");
                                    }
                                    else
                                    {
                                        actor.Character.Con = new Core.Types.MaxCurrent(actor.Character.Con.Max + 1, actor.Character.Con.Current + 1);
                                        sb.Append("You train your constitution, and feel much healthier!");
                                        actor.Character.Trains -= 1;
                                    }

                                    break;
                                case "hp":
                                    actor.Character.Health.Max += 10;
                                    sb.Append("You train your health, and feel much more physically powerful!");
                                    actor.Character.Trains -= 1;
                                    break;
                                case "mana":
                                    actor.Character.Mana.Max += 10;
                                    sb.Append("You train your mana, and feel much more mentally powerful!");
                                    actor.Character.Trains -= 1;
                                    break;
                                case "move":
                                    actor.Character.Movement.Max += 10;
                                    sb.Append("You train your movement, and feel much more endurance!");
                                    actor.Character.Trains -= 1;
                                    break;
                            }

                            await this.awardProcessor.GrantAward(4, actor.Character, "met a trainer", cancellationToken);
                        }
                        else
                        {
                            await this.communicator.SendToPlayer(actor.Character, $"{trainer.FirstName.FirstCharToUpper()} says \"<span class='say'>I'm sorry, {actor.Character.FirstName.FirstCharToUpper()}, but you have no training sessions available.</span>\"", cancellationToken);
                        }

                        await this.communicator.SendToPlayer(actor.Connection, sb.ToString(), cancellationToken);

                        await this.communicator.SaveCharacter(actor);
                    }
                }
            }
        }

        [SightRequired]
        [HelpText("<p>Remove yourself or a member from the group. Groups can communicate privately with GTELL See HELP GTELL.<p><ul><li>group <em>player</em></li></ul>")]
        private async Task DoUngroup(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                // Remove actor from any group they are in, or, if it's their group, disband it.
                if (actor.Character.GroupId.HasValue)
                {
                    var group = GroupHelper.GetGroup(actor.Character.GroupId.Value);

                    if (group != null)
                    {
                        // If the group belongs to the player, disband the whole group.
                        if (GroupHelper.IsGroupOwner(actor.Character.CharacterId))
                        {
                            List<long> playersToRemove = new List<long>();

                            // Send a disbanding message to each member of the group, then remove them and save.
                            foreach (var characterId in group.Value.Value)
                            {
                                if (characterId != actor.Character.CharacterId)
                                {
                                    var member = this.communicator.ResolveCharacter(characterId);
                                    if (member != null)
                                    {
                                        await this.communicator.SendToPlayer(member.Connection, $"{actor.Character.FirstName} has disbanded the group.", cancellationToken);
                                        playersToRemove.Add(member.Character.CharacterId);
                                        member.Character.GroupId = null;
                                        await this.communicator.SaveCharacter(member);
                                    }
                                }
                            }

                            GroupHelper.RemoveFromGroup(group.Value.Key, playersToRemove);

                            // All members removed, remove the group entirely.
                            GroupHelper.RemoveGroup(group.Value.Key);

                            await this.communicator.SendToPlayer(actor.Connection, "You have disbanded the group.", cancellationToken);
                        }
                        else
                        {
                            // Leaving a group owned by another player.
                            var owner = this.communicator.ResolveCharacter(group.Value.Key);
                            if (owner != null)
                            {
                                await this.communicator.SendToPlayer(actor.Connection, $"You have left {owner.Character.FirstName}'s group.", cancellationToken);
                                await this.communicator.SendToPlayer(owner.Connection, $"{actor.Character.FirstName} has left your group.", cancellationToken);
                            }

                            // If there are zero members left, remove the group.
                            if (group.Value.Value.Count == 0)
                            {
                                await this.communicator.SendToPlayer(actor.Connection, "Your group has disbanded.", cancellationToken);
                                GroupHelper.RemoveGroup(group.Value.Key);
                            }
                            else
                            {
                                // Send a departure message to the rest of the group.
                                foreach (var characterId in group.Value.Value)
                                {
                                    if (characterId != actor.Character.CharacterId)
                                    {
                                        var member = this.communicator.ResolveCharacter(characterId);
                                        if (member != null)
                                        {
                                            await this.communicator.SendToPlayer(member.Connection, $"{actor.Character.FirstName} has left the group.", cancellationToken);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Regardless of any messaging, remove the player from any groups.
                    GroupHelper.RemoveFromAllGroups(actor.Character.CharacterId);
                    actor.Character.GroupId = null;

                    // Save.
                    await this.communicator.SaveCharacter(actor);
                }
                else
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"You aren't in a group.", cancellationToken);
                }
            }
            else
            {
                // Ungroup a specific player. This can be done from anywhere in the world.
                var targetPlayer = this.communicator.ResolveCharacter(args.Method);

                if (targetPlayer != null)
                {
                    if (actor.Character.GroupId.HasValue)
                    {
                        var group = GroupHelper.GetGroup(actor.Character.GroupId.Value);

                        if (group != null)
                        {
                            // If the group belongs to the player, they can remove this player.
                            if (GroupHelper.IsGroupOwner(actor.Character.CharacterId))
                            {
                                await this.communicator.SendToPlayer(actor.Connection, $"You remove {targetPlayer.Character.FirstName} from your group.", cancellationToken);

                                // Send a departure message to the rest of the group.
                                foreach (var characterId in group.Value.Value)
                                {
                                    if (characterId != actor.Character.CharacterId && characterId != targetPlayer.Character.CharacterId)
                                    {
                                        var member = this.communicator.ResolveCharacter(characterId);
                                        if (member != null)
                                        {
                                            await this.communicator.SendToPlayer(member.Connection, $"{actor.Character.FirstName} has removed {targetPlayer.Character.FirstName} from the group.", cancellationToken);
                                        }
                                    }
                                }

                                // Remove the target player from any groups.
                                await this.communicator.SendToPlayer(targetPlayer.Connection, $"{actor.Character.FirstName} has removed you from the group.", cancellationToken);
                                GroupHelper.RemoveFromAllGroups(targetPlayer.Character.CharacterId);
                                targetPlayer.Character.GroupId = null;
                                await this.communicator.SaveCharacter(targetPlayer);

                                // If there are zero members left, remove the group.
                                if (group.Value.Value.Count == 0)
                                {
                                    await this.communicator.SendToPlayer(actor.Connection, "Your group has disbanded.", cancellationToken);
                                    GroupHelper.RemoveGroup(group.Value.Key);
                                }
                            }
                            else
                            {
                                await this.communicator.SendToPlayer(actor.Connection, $"You can't make that decision. It's not your group.", cancellationToken);
                            }
                        }
                        else
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"You couldn't find the group.", cancellationToken);
                        }
                    }
                    else
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"You aren't in a group.", cancellationToken);
                    }
                }
                else
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"They aren't here.", cancellationToken);
                }
            }
        }

        [SightRequired]
        [HelpText("<p>Gets the appraised value of an item from a merchant. See also: HELP LIST, HELP BUY<ul><li>sell item</li><ul></p>")]
        private async Task DoValue(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Get the value of what?", cancellationToken);
            }
            else
            {
                var mobs = this.communicator.GetMobilesInRoom(actor.Character.Location);

                if (mobs != null)
                {
                    var merchant = mobs.FirstOrDefault(m => m.MobileFlags != null && m.MobileFlags.Contains(Core.Types.MobileFlags.Shopkeeper));
                    if (merchant == null)
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"There isn't a merchant here.", cancellationToken);
                    }
                    else
                    {
                        if (actor.Character.Inventory != null && actor.Character.Inventory.Count > 0)
                        {
                            var item = actor.Character.Inventory.ParseTargetName(args.Method);

                            if (item != null)
                            {
                                if (item.Value == 0)
                                {
                                    await this.communicator.SendToPlayer(actor.Connection, $"{merchant.FirstName.FirstCharToUpper()} tells you \"<span class='tell'>{item.Name.FirstCharToUpper()} is useless to me. I have no interest in it.</span>\"", cancellationToken);
                                }
                                else
                                {
                                    await this.communicator.SendToPlayer(actor.Connection, $"{merchant.FirstName.FirstCharToUpper()} tells you \"<span class='tell'>I'd be willing to give you {item.Value.ToMerchantBuyPrice(actor.Character, merchant)} for {item.Name}.</span>\"", cancellationToken);
                                }
                            }
                            else
                            {
                                await this.communicator.SendToPlayer(actor.Connection, $"You do not have that to appraise.", cancellationToken);
                            }
                        }
                    }
                }
                else
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"There isn't a merchant here.", cancellationToken);
                }
            }
        }

        [SightRequired]
        [HelpText("<p>Watches a brief scene as mentioned in the room description.<ul><li>watch scene</li></ul></p>")]
        private async Task DoWatch(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Watch what?", cancellationToken);
            }
            else
            {
                var room = this.communicator.ResolveRoom(actor.Character.Location);

                if (room != null)
                {
                    if (!string.IsNullOrWhiteSpace(room.Video) && room.WatchKeyword?.ToLower() == args.Method.ToLower())
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"<video controls autoplay><source src=\"{room.Video}\" type=\"video/mp4\"></video>", cancellationToken);
                    }
                    else
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"There's really nothing here to watch.", cancellationToken);
                    }
                }
                else
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"There's really nothing here to watch.", cancellationToken);
                }
            }
        }

        [HelpText("<p>Sets your wimpy to a percentage of your health. If you fall below that percentage, you will flee automatically.</p>")]
        private async Task DoWimpy(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            StringBuilder sb = new StringBuilder();

            if (string.IsNullOrEmpty(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Your wimpy value should be a number that represents a percentage of your hit points.", cancellationToken);
            }
            else
            {
                if (int.TryParse(args.Method, out int wimpy))
                {
                    var hp = ((double)wimpy / 100d) * (double)actor.Character.Health.Max;
                    await this.communicator.SendToPlayer(actor.Connection, $"Wimpy set to {wimpy}% of your hit points ({(int)hp}hp).", cancellationToken);
                }
                else
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"Your wimpy value should be a number that represents a percentage of your hit points.", cancellationToken);
                }
            }
        }

        [SightRequired]
        [HelpText("<p>Shows your current worth in gold, silver, and copper.</p>")]
        private async Task DoWorth(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            StringBuilder sb = new StringBuilder();

            var currency = actor.Character.Currency.GetCurrency();

            sb.Append($"<span class='worth'>You currently have <span class='gold'>{currency.Item1}</span> gold, <span class='silver'>{currency.Item2}</span> silver, and <span class='copper'>{currency.Item3}</span> copper.</span>");

            await this.communicator.SendToPlayer(actor.Connection, sb.ToString(), cancellationToken);
        }

        /// <summary>
        /// Display the available learning trees (if applicable) to the character, that the teacher can teach them.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <param name="teacher">The teacher.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        private async Task ShowAvailableTrees(UserData actor, Mobile teacher, CancellationToken cancellationToken)
        {
            // Get all available skill and spell trees.
            List<ActionTree> skillTrees = TreeHelper.GetSkillTrees(this.communicator, this.random, this.world, this.logger, this.combat);
            List<ActionTree> spellTrees = TreeHelper.GetSpellTrees(this.communicator, this.random, this.world, this.logger, this.combat);

            // Filter the trees down depending on the type of teacher.
            if (teacher.SchoolType != SchoolType.General)
            {
                skillTrees = skillTrees.Where(sk => sk.SchoolType == teacher.SchoolType).ToList();
                spellTrees = spellTrees.Where(sp => sp.SchoolType == teacher.SchoolType).ToList();
            }

            bool canLearnSkills = false;
            bool canLearnSpells = false;
            string availableSkills = TreeHelper.GetLearnableSkillTrees(actor, skillTrees, out canLearnSkills);
            string availableSpells = TreeHelper.GetLearnableSpellTrees(actor, spellTrees, out canLearnSpells);

            StringBuilder sb = new StringBuilder();
            sb.Append($"You have {actor.Character.Learns} learning sessions available.<br/>");

            if (canLearnSkills)
            {
                sb.Append(availableSkills);
            }

            if (canLearnSpells)
            {
                sb.Append(availableSpells);
            }

            if (!canLearnSpells && !canLearnSkills)
            {
                sb.Append($"{teacher.FirstName.FirstCharToUpper()} says \"<span class='say'>I'm sorry, {actor.Character.FirstName}, but there isn't anything I can teach you here.</span>\"");
            }
            else
            {
                sb.Append($"{teacher.FirstName.FirstCharToUpper()} says \"<span class='say'>To learn a skill or spell group, just enter 'learn skill/spell [group name]'.</span>\"<br/>");
                sb.Append($"{teacher.FirstName.FirstCharToUpper()} says \"<span class='say'>For example 'learn <em>martial group II</em>'.</span>\"<br/>");
                sb.Append($"{teacher.FirstName.FirstCharToUpper()} says \"<span class='say'>To see what skills or spells are available in a group, enter 'skills/spells [group name]'.</span>\"<br/>");
                sb.Append($"{teacher.FirstName.FirstCharToUpper()} says \"<span class='say'>For example 'spells <em>conjuring group III</em>'.</span>\"<br/>");
            }

            await this.communicator.SendToPlayer(actor.Character, sb.ToString(), cancellationToken);
        }
    }
}