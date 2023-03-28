// <copyright file="WizActions.cs" company="Legendary™">
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
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core.Contracts;
    using Legendary.Core.Extensions;
    using Legendary.Core.Models;
    using Legendary.Core.Types;
    using Legendary.Engine.Attributes;
    using Legendary.Engine.Extensions;
    using Legendary.Engine.Helpers;
    using Legendary.Engine.Models;

    /// <summary>
    /// Contains methods that are specific to wizard (immortal) actions.
    /// </summary>
    public partial class ActionProcessor
    {
        /// <summary>
        /// Configures all of the wizard (immortal) actions. Actions from this list cannot be accessed by mortal players.
        /// </summary>
        private void ConfigureWizActions()
        {
            this.wizActions.Add("goto", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoGoTo)));
            this.wizActions.Add("grant", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoGrant)));
            this.wizActions.Add("peace", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoPeace)));
            this.wizActions.Add("reload", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoReload)));
            this.wizActions.Add("repop", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoRepop)));
            this.wizActions.Add("restore", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoRestore)));
            this.wizActions.Add("slay", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoSlay)));
            this.wizActions.Add("snoop", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoSnoop)));
            this.wizActions.Add("stat", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoStat)));
            this.wizActions.Add("switch", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(3, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoSwitch)));
            this.wizActions.Add("title", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoTitle)));
            this.wizActions.Add("transfer", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(2, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoTransfer)));
            this.wizActions.Add("wiznet", new KeyValuePair<int, Func<UserData, CommandArgs, CancellationToken, Task>>(1, new Func<UserData, CommandArgs, CancellationToken, Task>(this.DoWiznet)));
        }

        [MinimumLevel(90)]
        private async Task DoGoTo(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Goto where?", cancellationToken);
            }
            else
            {
                if (long.TryParse(args.Method, out long result))
                {
                    await this.GotoRoom(actor, result, cancellationToken);
                }
                else
                {
                    var player = this.communicator.ResolveCharacter(args.Method);

                    if (player != null)
                    {
                        var room = this.communicator.ResolveRoom(player.Character.Location);

                        if (room != null)
                        {
                            await this.GotoRoom(actor, player.Character.Location.Value, cancellationToken);
                        }
                    }
                    else
                    {
                        var location = this.communicator.ResolveMobileLocation(args.Method);

                        if (location != null)
                        {
                            await this.GotoRoom(actor, location.Value.Value, cancellationToken);
                        }
                        else
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"There's nobody here by that name.", cancellationToken);
                        }
                    }
                }
            }
        }

        [MinimumLevel(95)]
        private async Task DoGrant(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method) || string.IsNullOrWhiteSpace(args.Target))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Grant what to whom?", cancellationToken);
            }
            else
            {
                var player = this.communicator.ResolveCharacter(args.Target);
                var spell = SpellHelper.ResolveSpell(args.Method, this.communicator, this.random, this.world, this.logger, this.combat);
                var skill = SkillHelper.ResolveSkill(args.Method, this.communicator, this.random, this.world, this.logger, this.combat);

                if (player != null)
                {
                    if (args.Method.ToLower() == "all")
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"You grant every skill and spell available to {player.Character.FirstName}.", cancellationToken);
                        await this.communicator.SendToPlayer(player.Connection, $"{actor.Character.FirstName} has granted you every skill and spell available.", cancellationToken);
                        this.ApplySkillsAndSpells(player);
                    }
                    else if (skill == null && spell == null)
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"There aren't any skills or spells by that name.", cancellationToken);
                    }
                    else if (spell != null)
                    {
                        if (player.Character.HasSpell(spell.Name))
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"{player.Character.FirstName} already has that spell.", cancellationToken);
                        }
                        else
                        {
                            player.Character.Spells.Add(new SpellProficiency(spell.Name, 75));
                            await this.communicator.SendToPlayer(actor.Connection, $"You grant the {spell.Name} spell to {player.Character.FirstName}.", cancellationToken);
                            await this.communicator.SendToPlayer(player.Connection, $"{actor.Character.FirstName} has granted you the {spell.Name} spell.", cancellationToken);
                            await this.communicator.SaveCharacter(player);
                        }
                    }
                    else if (skill != null)
                    {
                        if (player.Character.HasSkill(skill.Name))
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"{player.Character.FirstName} already has that skill.", cancellationToken);
                        }
                        else
                        {
                            player.Character.Skills.Add(new SkillProficiency(skill.Name, 75));
                            await this.communicator.SendToPlayer(actor.Connection, $"You grant the {skill.Name} skill to {player.Character.FirstName}.", cancellationToken);
                            await this.communicator.SendToPlayer(player.Connection, $"{actor.Character.FirstName} has granted you the {skill.Name} skill.", cancellationToken);
                            await this.communicator.SaveCharacter(player);
                        }
                    }
                }
                else
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"That person isn't here.", cancellationToken);
                }
            }
        }

        [MinimumLevel(90)]
        private async Task DoPeace(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (Communicator.Users != null)
            {
                var users = Communicator.Users.Where(u => u.Value.Character.Location.InSamePlace(actor.Character.Location));

                // Stop all the users from fighting
                foreach (var user in users)
                {
                    user.Value.Character.CharacterFlags?.RemoveIfExists(CharacterFlags.Fighting);
                    user.Value.Character.Fighting = null;
                }

                // Stop all the mobiles from fighting
                var mobiles = this.communicator.GetMobilesInRoom(actor.Character.Location);
                if (mobiles != null)
                {
                    foreach (var mob in mobiles)
                    {
                        mob.CharacterFlags?.RemoveIfExists(CharacterFlags.Fighting);
                        mob.MobileFlags?.RemoveIfExists(MobileFlags.Aggressive);
                        mob.Fighting = null;
                    }
                }
            }

            await this.communicator.SendToPlayer(actor.Connection, "You stop all fighting in the room.", cancellationToken);
            await this.communicator.SendToRoom(actor.Character, actor.Character.Location, $"{actor.Character.FirstName} stops all fighting in the room.", cancellationToken);
        }

        [MinimumLevel(100)]
        private async Task DoReload(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            await this.communicator.SendToPlayer(actor.Connection, "Reloading the world...", cancellationToken);
            await this.world.LoadWorld();
            await this.world.CleanupWorld(cancellationToken);
            this.world.Populate();
            this.communicator.RestartGameLoop();
            await this.communicator.SendToPlayer(actor.Connection, "You have reloaded the area, room, mobiles, and items, and repopulated the world.", cancellationToken);
            this.logger.Warn($"{actor.Character.FirstName.FirstCharToUpper()} has reloaded all of the game data and repopulated the world.", this.communicator);
        }

        [MinimumLevel(90)]
        private async Task DoRepop(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            var area = this.communicator.ResolveArea(actor.Character.Location);

            if (area != null)
            {
                await this.communicator.SendToPlayer(actor.Connection, "Repopulating this area with mobiles and items...", cancellationToken);
                this.world.RepopulateMobiles(area);
                this.world.RepopulateItems(area);
                await this.communicator.SendToPlayer(actor.Connection, "You have repopulated this area.", cancellationToken);
                this.logger.Warn($"{actor.Character.FirstName.FirstCharToUpper()} has repopulated {area.Name}.", this.communicator);
            }
            else
            {
                await this.communicator.SendToPlayer(actor.Connection, "This area cannot be repopulated.", cancellationToken);
            }
        }

        [MinimumLevel(100)]
        private async Task DoRestore(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                actor.Character.Mana.Current = actor.Character.Mana.Max;
                actor.Character.Movement.Current = actor.Character.Movement.Max;
                actor.Character.Health.Current = actor.Character.Health.Max;
                await this.communicator.SendToPlayer(actor.Connection, "You have restored yourself.", cancellationToken);
            }
            else
            {
                var target = args.Method;

                var player = this.communicator.ResolveCharacter(target);

                if (player != null)
                {
                    player.Character.Mana.Current = player.Character.Mana.Max;
                    player.Character.Movement.Current = player.Character.Movement.Max;
                    player.Character.Health.Current = player.Character.Health.Max;
                    await this.communicator.SendToPlayer(actor.Connection, $"You have restored {player.Character.FirstName.FirstCharToUpper()}.", cancellationToken);
                    await this.communicator.SendToPlayer(player.Connection, $"{actor.Character.FirstName.FirstCharToUpper()} has restored you.", cancellationToken);
                }
                else
                {
                    await this.communicator.SendToPlayer(actor.Connection, "The aren't here.", cancellationToken);
                }
            }
        }

        [MinimumLevel(100)]
        private async Task DoSlay(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Slay whom?", cancellationToken);
            }
            else
            {
                var target = args.Method;

                var player = this.communicator.ResolveCharacter(target);

                if (player != null)
                {
                    await this.communicator.SendToPlayer(actor.Connection, $"You SLAY {player.Character.FirstName} in cold blood!", cancellationToken);
                    await this.communicator.SendToRoom(actor.Character.Location, actor.Character, player.Character, $"{actor.Character.FirstName.FirstCharToUpper()} SLAYS {player.Character.FirstName} in cold blood!", cancellationToken);
                    await this.communicator.SendToPlayer(player.Connection, $"{actor.Character} has SLAIN you!", cancellationToken);
                    await this.combat.KillPlayer(player.Character, actor.Character, cancellationToken);
                }
                else
                {
                    var mobile = this.communicator.ResolveMobile(target, actor.Character);

                    if (mobile != null)
                    {
                        if (this.communicator.IsInRoom(mobile.Location, actor.Character))
                        {
                            await this.communicator.SendToPlayer(actor.Connection, $"You SLAY {mobile.FirstName} in cold blood!", cancellationToken);
                            await this.communicator.SendToRoom(actor.Character.Location, actor.Character, null, $"{actor.Character.FirstName.FirstCharToUpper()} SLAYS {mobile.FirstName} in cold blood!", cancellationToken);
                            await this.combat.KillMobile(mobile, actor.Character, cancellationToken);
                        }
                    }
                    else
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"They aren't here.", cancellationToken);
                    }
                }
            }
        }

        [MinimumLevel(100)]
        private async Task DoSnoop(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            // TODO
            await this.communicator.SendToPlayer(actor.Connection, $"Not yet implemented.", cancellationToken);
        }

        [MinimumLevel(90)]
        private async Task DoStat(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Stat who or what?", cancellationToken);
            }
            else
            {
                var target = args.Method;

                var player = this.communicator.ResolveCharacter(target);

                if (player != null)
                {
                    var message = this.ShowStatistics(player.Character);
                    await this.communicator.SendToPlayer(actor.Connection, message, cancellationToken);
                }
                else
                {
                    var mobile = this.communicator.ResolveMobile(target, actor.Character);

                    if (mobile != null)
                    {
                        var message = this.ShowStatistics(mobile);
                        await this.communicator.SendToPlayer(actor.Connection, message, cancellationToken);
                    }
                    else
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"They aren't here.", cancellationToken);
                    }
                }
            }
        }

        [MinimumLevel(100)]
        private async Task DoSwitch(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            // TODO
            await this.communicator.SendToPlayer(actor.Connection, $"Not yet implemented.", cancellationToken);
        }

        [MinimumLevel(95)]
        private async Task DoTitle(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method) && string.IsNullOrWhiteSpace(args.Target))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Set title to what? (title [\"title\"] [player (optional)])", cancellationToken);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(args.Target))
                {
                    var player = this.communicator.ResolveCharacter(args.Target);

                    if (player != null)
                    {
                        player.Character.Title = args.Method;
                        await this.communicator.SaveCharacter(player.Character);
                        await this.communicator.SendToPlayer(actor.Connection, $"{player.Character.FirstName.FirstCharToUpper()}'s title set to \"{args.Method}\".", cancellationToken);
                    }
                    else
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"They aren't here.", cancellationToken);
                    }
                }
                else
                {
                    actor.Character.Title = args.Method;
                    await this.communicator.SaveCharacter(actor.Character);
                    await this.communicator.SendToPlayer(actor.Connection, $"Title set to \"{args.Method}\".", cancellationToken);
                }
            }
        }

        [MinimumLevel(90)]
        private async Task DoTransfer(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(args.Method))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Transfer whom?", cancellationToken);
            }
            else
            {
                var target = args.Method;

                var player = this.communicator.ResolveCharacter(target);

                if (player != null)
                {
                    player.Character.Location = actor.Character.Location;

                    // Track exploration for award purposes.
                    if (player.Character.Metrics.RoomsExplored.ContainsKey(actor.Character.Location.Key))
                    {
                        var roomList = player.Character.Metrics.RoomsExplored[actor.Character.Location.Key];

                        if (!roomList.Contains(actor.Character.Location.Value))
                        {
                            player.Character.Metrics.RoomsExplored[actor.Character.Location.Key].Add(actor.Character.Location.Value);
                            await this.awardProcessor.CheckVoyagerAward(actor.Character.Location.Key, player.Character, cancellationToken);
                        }
                    }
                    else
                    {
                        player.Character.Metrics.RoomsExplored.Add(actor.Character.Location.Key, new List<long>() { actor.Character.Location.Value });
                        await this.awardProcessor.CheckVoyagerAward(actor.Character.Location.Key, player.Character, cancellationToken);
                    }

                    await this.communicator.SendToPlayer(actor.Connection, $"You have transferred {player.Character.FirstName} here.", cancellationToken);
                    await this.communicator.SendToRoom(actor.Character.Location, actor.Character, player.Character, $"{player.Character.FirstName.FirstCharToUpper()} arrives in a puff of smoke.", cancellationToken);
                    await this.communicator.SendToPlayer(player.Connection, $"{actor.Character.FirstName.FirstCharToUpper()} has summoned you!", cancellationToken);
                    await this.communicator.SendToRoom(player.Character.Location, actor.Character, player.Character, $"{player.Character.FirstName.FirstCharToUpper()} vanishes in a flash of light.", cancellationToken);
                }
                else
                {
                    var mobile = this.communicator.ResolveMobile(target, actor.Character);

                    if (mobile != null)
                    {
                        var oldRoom = this.communicator.ResolveRoom(mobile.Location);
                        var newRoom = this.communicator.ResolveRoom(actor.Character.Location);

                        var oldMob = oldRoom != null ? oldRoom.Mobiles.FirstOrDefault(m => m.CharacterId == mobile.CharacterId) : null;

                        if (oldMob != null)
                        {
                            oldRoom?.Mobiles.Remove(oldMob);
                        }

                        newRoom?.Mobiles.Add(mobile);

                        mobile.Location = actor.Character.Location;

                        await this.communicator.SendToPlayer(actor.Connection, $"You have transferred {mobile.FirstName} here.", cancellationToken);
                        await this.communicator.SendToRoom(actor.Character.Location, actor.Character, null, $"{mobile.FirstName.FirstCharToUpper()} arrives in a puff of smoke.", cancellationToken);
                        await this.communicator.SendToRoom(mobile.Location, actor.Character, null, $"{mobile.FirstName.FirstCharToUpper()} vanishes in a flash of light.", cancellationToken);
                    }
                    else
                    {
                        await this.communicator.SendToPlayer(actor.Connection, $"They aren't here.", cancellationToken);
                    }
                }
            }
        }

        [MinimumLevel(90)]
        private async Task DoWiznet(UserData actor, CommandArgs args, CancellationToken cancellationToken)
        {
            // Sub/unsub to wiznet channel
            if (this.communicator.IsSubscribed("wiznet", actor.ConnectionId, actor))
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Unsubscribed from WIZNET. You will no longer see all logs.", cancellationToken);
                this.communicator.RemoveFromChannel("wiznet", actor.ConnectionId, actor);
            }
            else
            {
                await this.communicator.SendToPlayer(actor.Connection, $"Welcome to WIZNET! You will now see all logs.", cancellationToken);
                this.communicator.AddToChannel("wiznet", actor.ConnectionId, actor);
            }
        }

        private async Task GotoRoom(UserData user, long room, CancellationToken cancellationToken = default)
        {
            foreach (var area in this.world.Areas)
            {
                var targetRoom = area.Rooms.FirstOrDefault(r => r.RoomId == room);
                if (targetRoom == null)
                {
                    continue;
                }
                else
                {
                    await this.communicator.SendToPlayer(user.Connection, $"You suddenly teleport to {targetRoom.Name}.", cancellationToken);
                    await this.communicator.SendToRoom(user.Character, user.Character.Location, $"{user.Character.FirstName.FirstCharToUpper()} vanishes.", cancellationToken);
                    user.Character.Location = new KeyValuePair<long, long>(targetRoom.AreaId, targetRoom.RoomId);
                    await this.communicator.ShowRoomToPlayer(user.Character, cancellationToken);

                    // Track exploration for award purposes.
                    if (user.Character.Metrics.RoomsExplored.ContainsKey(user.Character.Location.Key))
                    {
                        var roomList = user.Character.Metrics.RoomsExplored[user.Character.Location.Key];

                        if (!roomList.Contains(user.Character.Location.Value))
                        {
                            user.Character.Metrics.RoomsExplored[user.Character.Location.Key].Add(user.Character.Location.Value);
                            await this.awardProcessor.CheckVoyagerAward(user.Character.Location.Key, user.Character, cancellationToken);
                        }
                    }
                    else
                    {
                        user.Character.Metrics.RoomsExplored.Add(user.Character.Location.Key, new List<long>() { user.Character.Location.Value });
                        await this.awardProcessor.CheckVoyagerAward(user.Character.Location.Key, user.Character, cancellationToken);
                    }

                    await this.awardProcessor.CheckVoyagerAward(user.Character.Location.Key, user.Character, cancellationToken);

                    return;
                }
            }

            await this.communicator.SendToPlayer(user.Connection, $"You were unable to teleport there.", cancellationToken);
        }

        /// <summary>
        /// Applies all skills and spells to a character. This is just for testing.
        /// </summary>
        /// <param name="userData">The user.</param>
        private void ApplySkillsAndSpells(UserData userData)
        {
            var engine = Assembly.Load("Legendary.Engine");

            var spellTrees = engine.GetTypes().Where(t => t.Namespace == "Legendary.Engine.Models.SpellTrees");

            foreach (var tree in spellTrees)
            {
                var treeInstance = Activator.CreateInstance(tree, this.communicator, this.random, this.world, this.logger, this.combat);

                var groupProps = tree.GetProperties();

                for (var x = 1; x <= 5; x++)
                {
                    var spellGroup = groupProps.FirstOrDefault(g => g.Name == $"Group{x}");

                    if (spellGroup != null)
                    {
                        var obj = spellGroup.GetValue(treeInstance);
                        if (obj != null)
                        {
                            var group = (List<IAction>)obj;

                            foreach (var kvp in group)
                            {
                                if (!userData.Character.HasSpell(kvp.Name.ToLower()))
                                {
                                    userData.Character.Spells.Add(new SpellProficiency(kvp.Name, 100));
                                }
                            }
                        }
                    }
                }
            }

            var skillTrees = engine.GetTypes().Where(t => t.Namespace == "Legendary.Engine.Models.SkillTrees");

            foreach (var tree in skillTrees)
            {
                var treeInstance = Activator.CreateInstance(tree, this.communicator, this.random, this.world, this.logger, this.combat);

                var groupProps = tree.GetProperties();

                for (var x = 1; x <= 5; x++)
                {
                    var spellGroup = groupProps.FirstOrDefault(g => g.Name == $"Group{x}");

                    if (spellGroup != null)
                    {
                        var obj = spellGroup.GetValue(treeInstance);
                        if (obj != null)
                        {
                            var group = (List<IAction>)obj;

                            foreach (var kvp in group)
                            {
                                if (!userData.Character.HasSkill(kvp.Name.ToLower()))
                                {
                                    userData.Character.Skills.Add(new SkillProficiency(kvp.Name, 75));
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}