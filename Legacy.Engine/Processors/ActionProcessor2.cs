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

            await this.communicator.SendToPlayer(actor.Connection, sb.ToString(), cancellationToken);
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
                            color = "amethyst";
                            level = "V";
                            break;
                        case 5:
                            color = "emerald";
                            level = "VI";
                            break;
                        case 6:
                            color = "sapphire";
                            level = "VII";
                            break;
                        case 7:
                            color = "ruby";
                            level = "VIII";
                            break;
                        case 8:
                            color = "diamond";
                            level = "IX";
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

        [HelpText("<p>When accompanied by a teacher, use this command to learn new skill or spell trees. See also: HELP PRACTICE, HELP TRAIN<ul><<li>learn tree</li></ul></p></p></p>")]
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
                    var engine = Assembly.Load("Legendary.Engine");

                    var skillTrees = engine.GetTypes().Where(t => t.Namespace == "Legendary.Engine.Models.SkillTrees");
                    var spellTrees = engine.GetTypes().Where(t => t.Namespace == "Legendary.Engine.Models.SpellTrees");

                    foreach (var tree in skillTrees)
                    {
                        var treeInstance = Activator.CreateInstance(tree, this.communicator, this.random, this.combat);

                        if (treeInstance != null && treeInstance is IActionTree instance)
                        {
                            var groupProps = tree.GetProperties();
                        }
                    }

                    switch (teacher.SchoolType)
                    {
                        default:
                        case Core.Types.SchoolType.General:
                            break;
                        case Core.Types.SchoolType.War:
                            break;
                        case Core.Types.SchoolType.Magic:
                            break;
                        case Core.Types.SchoolType.Divinity:
                            break;
                    }

                    await this.communicator.SendToPlayer(actor.Character, $"{teacher.FirstName.FirstCharToUpper()} says \"<span class='say'>I'm sorry, {actor.Character.FirstName.FirstCharToUpper()}, I'm afraid I'm not able to teach you anything right now.</span>\"", cancellationToken);
                }
            }
            else
            {
                await this.communicator.SendToPlayer(actor.Connection, $"There isn't a teacher here.", cancellationToken);
            }
        }

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

                        foreach (var item in merchant.Equipment)
                        {
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

        [HelpText("<p>Command not yet available.</p>")]
        private async Task DoGain(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            await this.communicator.SendToPlayer(actor.Connection, $"Not yet implemented.", cancellationToken);
        }

        [HelpText("<p>Adds a member to your group. Groups can communicate privately with GTELL See HELP GTELL.<p><ul><li>group <em>player</em></li></ul>")]
        private async Task DoGroup(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                // TODO How to get the group for everyone?
                // Show players in group
                if (Communicator.Groups.ContainsKey(actor.Character.CharacterId))
                {
                    var group = Communicator.Groups[actor.Character.CharacterId];

                    StringBuilder sb = new StringBuilder();
                    sb.Append("<span class='group-info'>");

                    foreach (var characterId in group)
                    {
                        var player = this.communicator.ResolveCharacter(characterId);

                        if (player != null)
                        {
                            sb.Append($"<span class='group-member'>{player.Character.FirstName}");
                            sb.Append($"<progress class='group-health' max='100' value='{player.Character.Health.GetPercentage()}'></progress>");
                            sb.Append($"<progress class='group-mana' max='100' value='{player.Character.Mana.GetPercentage()}'></progress>");
                            sb.Append($"<progress class='group-movement' max='100' value='{player.Character.Movement.GetPercentage()}'></progress>");
                            sb.Append($"</span>");
                        }
                    }

                    sb.Append("</span>");

                    await this.communicator.SendToPlayer(actor.Connection, sb.ToString(), cancellationToken);
                }
                else
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"There is nobody in your group.", cancellationToken);
                }
            }
            else
            {
                var targetPlayer = this.communicator.ResolveCharacter(args.Method);

                if (targetPlayer != null)
                {
                    if (targetPlayer.Character.Following != actor.Character.CharacterId)
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"{targetPlayer.Character.FirstName} isn't following you.", cancellationToken);
                    }
                    else
                    {
                        if (Communicator.Groups.ContainsKey(actor.Character.CharacterId))
                        {
                            Communicator.Groups[actor.Character.CharacterId].Add(targetPlayer.Character.CharacterId);
                        }
                        else
                        {
                            Communicator.Groups.TryAdd(actor.Character.CharacterId, new List<long>() { targetPlayer.Character.CharacterId });
                        }

                        await this.communicator.SendToPlayer(actor.Connection, $"You add {targetPlayer.Character.FirstName} to your group.", cancellationToken);
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
                                var treeInstance = Activator.CreateInstance(tree, this.communicator, this.random, this.combat);

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
                                var treeInstance = Activator.CreateInstance(tree, this.communicator, this.random, this.combat);

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
                                    int advance = ((int)actor.Character.Wis.Current * 2) + this.random.Next(1, (int)actor.Character.Dex.Current);
                                    skillProf.Proficiency = Math.Min(75, skillProf.Proficiency + advance);
                                    skillProf.Progress = 0;
                                    actor.Character.Practices -= 1;

                                    await this.communicator.SendToPlayer(actor.Character, $"{gm.FirstName.FirstCharToUpper()} helps you practice {skillProf.SkillName.FirstCharToUpper()}, and your proficiency increases!", cancellationToken);
                                    await this.awardProcessor.GrantAward(4, actor.Character, "met a guildmaster", cancellationToken);
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
                                    int advance = ((int)actor.Character.Wis.Current * 2) + this.random.Next(1, (int)actor.Character.Int.Current);
                                    spellProf.Proficiency = Math.Min(75, spellProf.Proficiency + advance);
                                    spellProf.Progress = 0;
                                    actor.Character.Practices -= 1;

                                    await this.communicator.SendToPlayer(actor.Character, $"{gm.FirstName.FirstCharToUpper()} helps you practice {spellProf.SpellName.FirstCharToUpper()}, and your proficiency increases!", cancellationToken);
                                    await this.awardProcessor.GrantAward(4, actor.Character, "met a guildmaster", cancellationToken);
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
                                    await this.communicator.SendToRoom(actor.Character, actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName.FirstCharToUpper()} puts {target.Name} in {item.Name}.", cancellationToken);
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
                                await this.communicator.SendToRoom(actor.Character, actor.Character.Location, actor.ConnectionId, $"{actor.Character.FirstName.FirstCharToUpper()} puts {target.Name} in {item.Name}.", cancellationToken);
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

        [HelpText("<p>Removes a member from your group. Groups can communicate privately with GTELL See HELP GTELL.<p><ul><li>group <em>player</em></li></ul>")]
        private async Task DoUngroup(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Who do you want to remove from your group?", cancellationToken);
            }
            else
            {
                // Giving to player or mob?
                var targetPlayer = this.communicator.ResolveCharacter(args.Method);

                if (targetPlayer != null)
                {

                }
                else
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"They aren't here.", cancellationToken);
                }
            }
        }

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
    }
}