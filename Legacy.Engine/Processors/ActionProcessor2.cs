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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Engine.Attributes;
    using Legendary.Engine.Extensions;
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

        [HelpText("<p>Command not yet available.</p>")]
        private async Task DoAwards(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            await this.communicator.SendToPlayer(actor.Connection, $"Not yet implemented.", cancellationToken);
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

        [HelpText("<p>Command not yet available.</p>")]
        private async Task DoPractice(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            await this.communicator.SendToPlayer(actor.Connection, $"Not yet implemented.", cancellationToken);
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

        [HelpText("<p>When accompanied by a trainer, use this skill to train up your vital attributes.<ul><li>train str</li><li>train hp</li></ul></p>")]
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