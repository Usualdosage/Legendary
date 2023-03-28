// <copyright file="World.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Core.Types;
    using Legendary.Data.Contracts;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Extensions;
    using Legendary.Engine.Models.Skills;
    using Legendary.Engine.Models.Spells;
    using MongoDB.Driver;

    /// <summary>
    /// Represents an instance of a world.
    /// </summary>
    public class World : IWorld
    {
        private readonly IRandom random;
        private readonly IDataService dataService;
        private readonly ILogger logger;
        private readonly ICommunicator communicator;

        /// <summary>
        /// Initializes a new instance of the <see cref="World"/> class.
        /// </summary>
        /// <param name="dataService">The areas within the world.</param>
        /// <param name="random">The random generator.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="communicator">The communicator.</param>
        public World(IDataService dataService, IRandom random, ILogger logger, ICommunicator communicator)
        {
            this.dataService = dataService;
            this.logger = logger;
            this.random = random;
            this.communicator = communicator;
        }

        /// <inheritdoc/>
        public HashSet<Area> Areas { get; private set; } = new HashSet<Area>();

        /// <inheritdoc/>
        public HashSet<Item> Items { get; private set; } = new HashSet<Item>();

        /// <inheritdoc/>
        public HashSet<Mobile> Mobiles { get; private set; } = new HashSet<Mobile>();

        /// <inheritdoc/>
        public HashSet<Award> Awards { get; private set; } = new HashSet<Award>();

        /// <inheritdoc/>
        public GameMetrics? GameMetrics { get; internal set; } = null;

        /// <inheritdoc/>
        public async Task LoadWorld()
        {
            var areas = await this.dataService.Areas.Find(Builders<Area>.Filter.Empty).ToListAsync();
            var items = await this.dataService.Items.Find(Builders<Item>.Filter.Empty).ToListAsync();
            var mobiles = await this.dataService.Mobiles.Find(Builders<Mobile>.Filter.Empty).ToListAsync();
            var awards = await this.dataService.Awards.Find(Builders<Award>.Filter.Empty).ToListAsync();

            // Cache common lookups as hash sets for faster reads.
            this.Areas = new HashSet<Area>(areas);
            this.Items = new HashSet<Item>(items);
            this.Mobiles = new HashSet<Mobile>(mobiles);
            this.Awards = new HashSet<Award>(awards);
        }

        /// <inheritdoc/>
        public void RepopulateItems(Area area)
        {
            var resets = area.Rooms.SelectMany(r => r.ItemResets);

            var resetGroups = resets.GroupBy(g => g);

            foreach (var resetGroup in resetGroups)
            {
                var itemId = resetGroup.Key;
                var maxItems = resetGroup.Count();
                var currentItems = area.Rooms.Sum(r => r.Items?.Where(i => i.ItemId == itemId).Count());

                if (maxItems == currentItems)
                {
                    // All good, we are balanced.
                    continue;
                }
                else if (currentItems < maxItems)
                {
                    // We are short some mobs, so repop.
                    var diff = maxItems - currentItems;

                    this.logger.Info($"Repop: Area {area.AreaId} is under by {diff} items of ID {itemId}. Adding more.", this.communicator);

                    // Get all the possible rooms the item could repop in.
                    var repopRooms = area.Rooms.Where(r => r.ItemResets.Contains(itemId)).ToList();

                    for (int x = 0; x < diff; x++)
                    {
                        // Get one of the rooms it normally would populate in at random.
                        var room = repopRooms[this.random.Next(0, repopRooms.Count - 1)];

                        var item = this.Items.FirstOrDefault(i => i.ItemId == itemId);

                        // Get the item.
                        if (item != null)
                        {
                            // Create a copy.
                            var clone = item.DeepCopy();

                            clone.Location = new KeyValuePair<long, long>(room.AreaId, room.RoomId);

                            // Add the item to the room.
                            room.Items?.Add(clone);
                        }
                    }
                }
                else
                {
                    // We have too many mobs in this area.
                    var diff = currentItems - maxItems;

                    this.logger.Info($"Repop: Area {area.AreaId} is over by {diff} items of ID {itemId}. Removing excess.", this.communicator);

                    var removalRooms = area.Rooms.Where(r => r.Items.Any(i => i.ItemId == itemId)).ToList();

                    for (int x = 0; x < diff; x++)
                    {
                        var room = removalRooms[this.random.Next(0, removalRooms.Count - 1)];

                        // Get the first item in the room that matches our key.
                        var item = room.Items?.FirstOrDefault(r => r.ItemId == itemId);

                        if (item != null)
                        {
                            room.Items?.Remove(item);
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void RepopulateMobiles(Area area)
        {
            var resets = area.Rooms.SelectMany(r => r.MobileResets);

            var resetGroups = resets.GroupBy(g => g);

            foreach (var resetGroup in resetGroups)
            {
                var mobCharacterId = resetGroup.Key;
                var maxMobs = resetGroup.Count();
                var currentMobs = area.Rooms.Sum(r => r.Mobiles?.Where(m => m.CharacterId == mobCharacterId).Count());

                if (maxMobs == currentMobs)
                {
                    // All good, we are balanced.
                    continue;
                }
                else if (currentMobs < maxMobs)
                {
                    // We are short some mobs, so repop.
                    var diff = maxMobs - currentMobs;

                    this.logger.Info($"Repop: Area {area.AreaId} is under by {diff} mobiles of ID {mobCharacterId}. Adding more.", this.communicator);

                    // Get all the possible rooms they could repop in.
                    var repopRooms = area.Rooms.Where(r => r.MobileResets.Contains(mobCharacterId)).ToList();

                    for (int x = 0; x < diff; x++)
                    {
                        // Get one of the rooms they normally would populate in at random.
                        var room = repopRooms[this.random.Next(0, repopRooms.Count - 1)];

                        var mobile = this.Mobiles.FirstOrDefault(m => m.CharacterId == mobCharacterId);

                        // Get the mobile.
                        if (mobile != null)
                        {
                            // Create a copy.
                            var clone = mobile.DeepCopy();

                            clone.Location = new KeyValuePair<long, long>(room.AreaId, room.RoomId);

                            // Equip the mobile.
                            foreach (var itemReset in clone.EquipmentResets)
                            {
                                var item = this.Items.FirstOrDefault(i => i.ItemId == itemReset.ItemId);

                                if (item != null)
                                {
                                    var itemClone = item.DeepCopy();
                                    clone.Equipment.Add(itemClone);
                                }
                            }

                            // Apply skills.
                            this.ApplyMobileSkills(clone);

                            // Add the mobile to the room.
                            room.Mobiles?.Add(clone);
                        }
                    }
                }
                else
                {
                    // We have too many mobs in this area.
                    var diff = currentMobs - maxMobs;

                    this.logger.Info($"Repop: Area {area.AreaId} is over by {diff} mobs of ID {mobCharacterId}. Removing excess.", this.communicator);

                    var removalRooms = area.Rooms.Where(r => r.Mobiles.Any(m => m.CharacterId == mobCharacterId)).ToList();

                    for (int x = 0; x < diff; x++)
                    {
                        var room = removalRooms[this.random.Next(0, removalRooms.Count - 1)];

                        // Get the first mob in the room that matches our key and isn't fighting.
                        var mobile = room.Mobiles?.FirstOrDefault(r => r.CharacterId == mobCharacterId && !r.CharacterFlags.Contains(CharacterFlags.Fighting));

                        if (mobile != null)
                        {
                            room.Mobiles?.Remove(mobile);
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void Populate()
        {
            foreach (var area in this.Areas)
            {
                foreach (var room in area.Rooms)
                {
                    // Populate items from resets
                    foreach (var reset in room.ItemResets)
                    {
                        var item = this.Items.FirstOrDefault(i => i.ItemId == reset);

                        if (item != null)
                        {
                            var clone = item.DeepCopy();
                            clone.Location = new KeyValuePair<long, long>(area.AreaId, room.RoomId);

                            if (clone.ItemResets != null)
                            {
                                foreach (var itemReset in clone.ItemResets)
                                {
                                    var itemInItem = this.Items.FirstOrDefault(i => i.ItemId == itemReset);

                                    if (itemInItem != null)
                                    {
                                        var itemInItemClone = itemInItem.DeepCopy();

                                        if (clone.Contains == null)
                                        {
                                            clone.Contains = new List<IItem>();
                                        }

                                        clone.Contains.Add(itemInItemClone);
                                    }
                                }
                            }

                            room.Items?.Add(clone);
                        }
                    }

                    // Populate mobs from resets
                    foreach (var reset in room.MobileResets)
                    {
                        var mobile = this.Mobiles.FirstOrDefault(m => m.CharacterId == reset);

                        if (mobile != null)
                        {
                            var clone = mobile.DeepCopy();
                            clone.Location = new KeyValuePair<long, long>(area.AreaId, room.RoomId);

                            // Add items to mobs.
                            foreach (var itemReset in clone.EquipmentResets)
                            {
                                var item = this.Items.FirstOrDefault(i => i.ItemId == itemReset.ItemId);

                                if (item != null)
                                {
                                    var itemClone = item.DeepCopy();
                                    clone.Equipment.Add(itemClone);
                                }
                            }

                            this.ApplyMobileSkills(clone);

                            room.Mobiles?.Add(clone);
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        public async Task CleanupWorld(CancellationToken cancellationToken = default)
        {
            try
            {
                await Parallel.ForEachAsync(this.Areas, async (area, cancellationToken) =>
                {
                    await this.CleanupMobiles(area);
                    await this.CleanupItems(area);
                });
            }
            catch (Exception exc)
            {
                this.logger.Error(exc, null);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task CleanupMobiles(Area area)
        {
            await Parallel.ForEachAsync(area.Rooms, async (room, cancellationToken) =>
            {
                await Task.Run(
                    () =>
                    {
                        room.Mobiles?.RemoveAll(m => m.Location.Value != room.RoomId && !m.CharacterFlags.Contains(CharacterFlags.Fighting));
                    }, cancellationToken);
            });
        }

        /// <inheritdoc/>
        public async Task CleanupItems(Area area)
        {
            await Parallel.ForEachAsync(area.Rooms, async (room, cancellationToken) =>
            {
                await Task.Run(
                    () =>
                    {
                        room.Items?.RemoveAll(i => i.RotTimer == 0);
                    }, cancellationToken);
            });
        }

        /// <inheritdoc/>
        public async Task<GameMetrics> UpdateGameMetrics(Exception? lastException, DateTime? startup, CancellationToken cancellationToken)
        {
            try
            {
                var metrics = await this.dataService.GetGameMetrics();

                if (metrics == null)
                {
                    metrics = new GameMetrics();
                }

                // Update hour, day, month, and year.
                metrics.CurrentHour++;

                if (metrics.CurrentHour == 24)
                {
                    metrics.CurrentHour = 0;
                    metrics.CurrentDay++;

                    if (metrics.CurrentDay >= 31)
                    {
                        metrics.CurrentDay = 1;
                        metrics.CurrentMonth++;

                        if (metrics.CurrentMonth >= 13)
                        {
                            metrics.CurrentMonth = 1;
                            metrics.CurrentYear++;
                        }
                    }
                }

                metrics.HostURL = "https://legendary-web.azurewebsites.net";
                metrics.LastError = lastException;
                metrics.LastStartupDateTime = startup.HasValue ? startup : metrics.LastStartupDateTime;
                metrics.MaxPlayers = this.dataService.Characters.CountDocuments(c => c.CharacterId > 0, cancellationToken: cancellationToken);
                metrics.TotalAreas = this.Areas.Count;
                metrics.TotalMobiles = this.Mobiles.Count;
                metrics.TotalItems = this.Items.Count;
                metrics.TotalRooms = this.Areas.Sum(a => a.Rooms.Count);

                // Update the local cached version.
                this.GameMetrics = metrics;

                var killer = this.dataService.Characters.Find(c => c.Metrics != null && c.Metrics.PlayerKills > 0 && !c.IsNPC).SortByDescending(s => s.Metrics.PlayerKills).FirstOrDefault(cancellationToken: cancellationToken);

                if (killer != null)
                {
                    metrics.MostKills = killer.FirstName.ToLower().FirstCharToUpper();
                }
                else
                {
                    metrics.MostKills = "Nobody";
                }

                await this.dataService.SaveGameMetrics(metrics, cancellationToken);

                return metrics;
            }
            catch
            {
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task ProcessWorldChanges(CancellationToken cancellationToken = default)
        {
            try
            {
                foreach (var area in this.Areas)
                {
                    foreach (var room in area.Rooms)
                    {
                        // Decompose items.
                        var items = room.Items?.Where(i => i.RotTimer == 0);

                        var location = new KeyValuePair<long, long>(area.AreaId, room.RoomId);

                        if (items != null)
                        {
                            foreach (var item in items)
                            {
                                if (item.ItemId == Constants.ITEM_SPRING)
                                {
                                    await this.communicator.SendToRoom(location, $"{item.Name.FirstCharToUpper()} dries up.", cancellationToken);
                                }
                                else if (item.ItemId == Constants.ITEM_LIGHT)
                                {
                                    await this.communicator.SendToRoom(location, $"{item.Name.FirstCharToUpper()} flickers and fades into darkness.", cancellationToken);
                                }
                                else if (item.ItemId == Constants.ITEM_FOOD)
                                {
                                    await this.communicator.SendToRoom(location, $"{item.Name.FirstCharToUpper()} rots away.", cancellationToken);
                                }
                                else if (item.ItemId == Constants.ITEM_CORPSE)
                                {
                                    if (!item.IsNPCCorpse)
                                    {
                                        // TODO: Move PC inventory to a pit
                                    }

                                    await this.communicator.SendToRoom(location, $"{item.Name.FirstCharToUpper()} decomposes into dust.", cancellationToken);
                                }
                                else
                                {
                                    await this.communicator.SendToRoom(location, $"{item.Name.FirstCharToUpper()} disintegrates.", cancellationToken);
                                }
                            }
                        }

                        // Apply affects to mobiles.
                        await this.ProcessMobileAffects(room, cancellationToken);

                        // Move mobiles who wander.
                        await this.ProcessMobileWander(room, cancellationToken);
                    }
                }

                // Tidy everything up after movement, decomposition, etc.
                await this.CleanupWorld(cancellationToken);
            }
            catch (Exception exc)
            {
                this.logger.Error("An error occurred while processing world changes.", exc, this.communicator);
            }
       }

        /// <inheritdoc/>
        public async Task<Area?> FindArea(
            Expression<Func<Area, bool>> filter,
            FindOptions? options = null)
        {
            return await this.dataService.Areas.Find(filter, options)
                .FirstOrDefaultAsync();
        }

        /// <inheritdoc/>
        public async Task<Character?> FindCharacter(
            Expression<Func<Character, bool>> filter,
            FindOptions? options = null)
        {
            return await this.dataService.Characters.Find(filter, options)
                .FirstOrDefaultAsync();
        }

        /// <inheritdoc/>
        public async Task<Item?> FindItem(
            Expression<Func<Item, bool>> filter,
            FindOptions? options = null)
        {
            return await this.dataService.Items.Find(filter, options)
                .FirstOrDefaultAsync();
        }

        /// <inheritdoc/>
        public async Task<Mobile?> FindMobile(
            Expression<Func<Mobile, bool>> filter,
            FindOptions? options = null)
        {
            return await this.dataService.Mobiles.Find(filter, options)
                .FirstOrDefaultAsync();
        }

        /// <inheritdoc/>
        public List<Area> GetAllAreas()
        {
            return this.dataService.Areas.Find(a => true).ToList();
        }

        /// <inheritdoc/>
        public List<Character> GetAllCharacters()
        {
            return this.dataService.Characters.Find(c => true).ToList();
        }

        /// <inheritdoc/>
        public List<Item> GetAllItems()
        {
            return this.dataService.Items.Find(i => true).ToList();
        }

        /// <inheritdoc/>
        public List<Mobile> GetAllMobiles()
        {
            return this.dataService.Mobiles.Find(i => true).ToList();
        }

        /// <inheritdoc/>
        public void InsertOneArea(
            Area document,
            InsertOneOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            this.dataService.Areas?.InsertOne(document, options, cancellationToken);
        }

        /// <inheritdoc/>
        public void InsertOneCharacter(
            Character document,
            InsertOneOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            this.dataService.Characters?.InsertOne(document, options, cancellationToken);
        }

        /// <inheritdoc/>
        public void InsertOneItem(
            Item document,
            InsertOneOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            this.dataService.Items?.InsertOne(document, options, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<ReplaceOneResult> ReplaceOneAreaAsync(
            Expression<Func<Area, bool>> filter,
            Area replacement,
            ReplaceOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return await this.dataService.Areas.ReplaceOneAsync(
                filter,
                replacement,
                options,
                cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<ReplaceOneResult> ReplaceOneCharacterAsync(
            Expression<Func<Character, bool>> filter,
            Character replacement,
            ReplaceOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return await this.dataService.Characters.ReplaceOneAsync(
                filter,
                replacement,
                options,
                cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<ReplaceOneResult> ReplaceOneItemAsync(
            Expression<Func<Item, bool>> filter,
            Item replacement,
            ReplaceOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return await this.dataService.Items.ReplaceOneAsync(
                filter,
                replacement,
                options,
                cancellationToken);
        }

        private void ApplyMobileSkills(Mobile mobile)
        {
            // Apply skills to mobs based on their equipment.
            var wielded = mobile.Equipment.FirstOrDefault(c => c.WearLocation.Contains(WearLocation.Wielded));

            var proficiency = Math.Min(95, 50 + mobile.Level);

            if (wielded != null)
            {
                switch (wielded.DamageType)
                {
                    case DamageType.Blunt:
                        {
                            mobile.Skills.Add(new SkillProficiency("blunt weapons", proficiency));
                            mobile.Skills.Add(new SkillProficiency("parry", proficiency));
                            break;
                        }

                    case DamageType.Slash:
                        {
                            mobile.Skills.Add(new SkillProficiency("edged weapons", proficiency));
                            mobile.Skills.Add(new SkillProficiency("parry", proficiency));
                            break;
                        }

                    case DamageType.Pierce:
                        {
                            mobile.Skills.Add(new SkillProficiency("piercing weapons", proficiency));
                            mobile.Skills.Add(new SkillProficiency("parry", proficiency));
                            break;
                        }
                }
            }

            // All mobs have basic hand to hand
            mobile.Skills.Add(new SkillProficiency("hand to hand", proficiency));

            // All mobs have basic dodge
            mobile.Skills.Add(new SkillProficiency("dodge", proficiency));

            // Mobs with a dex of 16 or above get evasive
            if (mobile.Dex.Current >= 16)
            {
                mobile.Skills.Add(new SkillProficiency("evasive maneuvers", proficiency));
            }

            // Mobs >= level 20 have second attack
            if (mobile.Level >= 20)
            {
                mobile.Skills.Add(new SkillProficiency("second attack", proficiency));
            }

            // Mobs >= level 40 have third attack
            if (mobile.Level >= 40)
            {
                mobile.Skills.Add(new SkillProficiency("third attack", proficiency));
            }

            // Mobs >= level 60 have fourth attack
            if (mobile.Level >= 60)
            {
                mobile.Skills.Add(new SkillProficiency("fourth attack", proficiency));
            }

            // If mobile is wimpy, set to 25% of total HP.
            if (mobile.MobileFlags != null && mobile.MobileFlags.Contains(MobileFlags.Wimpy))
            {
                var wimpy = (double)mobile.Health.Max * .25d;
                mobile.Wimpy = (int)wimpy;
            }
        }

        private async Task ProcessMobileAffects(Room room, CancellationToken cancellationToken)
        {
            // Process effects on mobiles, and (maybe) move them if they are wandering.
            foreach (var mobile in room.Mobiles)
            {
                // Check effects.
                if (mobile.AffectedBy.Count > 0)
                {
                    foreach (var effect in mobile.AffectedBy)
                    {
                        if (effect != null)
                        {
                            effect.Duration -= 1;

                            if (effect.Effector != null && effect.Action != null)
                            {
                                await effect.Action.OnTick(mobile, effect);
                            }
                        }
                    }

                    mobile.AffectedBy.RemoveAll(e => e.Duration < 0);
                }
            }
        }

        private async Task ProcessMobileWander(Room room, CancellationToken cancellationToken)
        {
            try
            {
                await this.DoMobileWander(room, cancellationToken);

                await this.DoMobileScavenge(room, cancellationToken);
            }
            catch (Exception exc)
            {
                this.logger.Error($"Error in ProcessMobileWander: {exc.ToString()}", exc, this.communicator);
            }
        }

        private async Task DoMobileWander(Room room, CancellationToken cancellationToken)
        {
            List<Mobile> removeMobiles = new List<Mobile>();

            // This may not be the best option, but the mobs in the room need to be locked from other processes whil enumerating. Maybe check out SemaphoreSlim() at some point.
            lock (room.Mobiles)
            {
                // Process effects on mobiles, and (maybe) move them if they are wandering.
                foreach (var mobile in room.Mobiles)
                {
                    try
                    {
                        if (mobile.MobileFlags != null && mobile.MobileFlags.Any(a => a == MobileFlags.Wander))
                        {
                            if (!mobile.CharacterFlags.Contains(CharacterFlags.Fighting) && !mobile.CharacterFlags.Contains(CharacterFlags.Charmed) && !mobile.CharacterFlags.Contains(CharacterFlags.Sleeping))
                            {
                                // If the mob is fighting someone, we'll handle this in the tracker.
                                if (mobile.Fighting.HasValue)
                                {
                                    continue;
                                }
                                else if (mobile.PlayerTarget != null)
                                {
                                    if (this.communicator.IsInRoom(mobile.Location, mobile.PlayerTarget))
                                    {
                                        // If the mobile is engaged with a player, don't move it.
                                        continue;
                                    }
                                    else
                                    {
                                        // Player left, so reset the target.
                                        mobile.PlayerTarget = null;
                                    }
                                }
                                else
                                {
                                    // Mobiles have a 50% chance each tick to move around.
                                    var move = this.random.Next(0, 100);

                                    if (move <= 50)
                                    {
                                        var randomExitNumber = this.random.Next(0, room.Exits.Count);

                                        var exit = room.Exits[randomExitNumber];

                                        var newArea = this.FindArea(a => a.AreaId == exit.ToArea).Result;
                                        var newRoom = newArea?.Rooms?.FirstOrDefault(r => r.RoomId == exit.ToRoom);

                                        bool isGhost = mobile.CharacterFlags.Contains(CharacterFlags.Ghost) || mobile.IsAffectedBy(nameof(PassDoor));
                                        bool isFlying = mobile.Race == Race.Avian || mobile.Race == Race.Faerie || mobile.IsAffectedBy(nameof(Fly));

                                        if (newArea != null && newRoom != null && newRoom.Flags != null && !newRoom.Flags.Contains(RoomFlags.NoMobs))
                                        {
                                            if (newRoom.Terrain == Terrain.Air && !isFlying && !isGhost)
                                            {
                                                continue;
                                            }
                                            else if (newRoom.Flags.Contains(RoomFlags.NoMobs))
                                            {
                                                continue;
                                            }
                                            else if (newRoom.Terrain == Terrain.Water && !isFlying && !isGhost && mobile.Inventory.Any(i => i.ItemType == ItemType.Boat))
                                            {
                                                continue;
                                            }
                                            else if (mobile.Location.Key != newArea.AreaId)
                                            {
                                                // Don't let mobs leave their home area.
                                                continue;
                                            }
                                            else
                                            {
                                                string? dir = Enum.GetName(typeof(Direction), exit.Direction)?.ToLower();

                                                if (exit.IsDoor && exit.IsClosed && !exit.IsLocked)
                                                {
                                                    this.communicator.SendToRoom(mobile.Location, $"{mobile.FirstName.FirstCharToUpper()} opens the {dir} {exit.DoorName ?? "door"}.", cancellationToken);
                                                    exit.IsClosed = false;
                                                    continue;
                                                }
                                                else
                                                {
                                                    try
                                                    {
                                                        this.communicator.SendToRoom(mobile.Location, $"{mobile.FirstName.FirstCharToUpper()} leaves {dir}.", cancellationToken);

                                                        // Remove the mobile from the prior location.
                                                        var lastRoom = this.communicator.ResolveRoom(mobile.Location);

                                                        if (lastRoom != null)
                                                        {
                                                            removeMobiles.Add(mobile);
                                                        }

                                                        // Add the mobile to the new location.
                                                        var newLocation = new KeyValuePair<long, long>(exit.ToArea, exit.ToRoom);

                                                        var nextRoom = this.communicator.ResolveRoom(newLocation);

                                                        if (nextRoom != null)
                                                        {
                                                            // Clone the mob, we'll destroy the other one.
                                                            var mobileCopy = mobile.DeepCopy();

                                                            // Set the mobile's new location.
                                                            mobileCopy.Location = newLocation;

                                                            nextRoom.Mobiles.Add(mobileCopy);

                                                            if (!mobileCopy.IsAffectedBy(nameof(Sneak)))
                                                            {
                                                                this.communicator.SendToRoom(mobileCopy.Location, $"{mobileCopy.FirstName.FirstCharToUpper()} enters.", cancellationToken);
                                                            }
                                                        }
                                                    }
                                                    catch (Exception exc)
                                                    {
                                                        this.logger.Error("An error occurred at checkpoint WANDER.", exc, this.communicator);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        this.logger.Error("An error occurred while processing mobile wander.", exc, this.communicator);
                    }
                }

                try
                {
                    // Remove any mobiles from the room that moved.
                    room.Mobiles.RemoveAll(r => removeMobiles.Contains(r));
                }
                catch (Exception exc)
                {
                    this.logger.Error("An error occurred while processing mobile wander. Cleanup failed.", exc, this.communicator);
                }
            }
        }

        private async Task DoMobileScavenge(Room room, CancellationToken cancellationToken)
        {
            List<Item> itemsToRemove = new List<Item>();

            // Have wandering mobiles pick up stuff.
            foreach (var mobile in room.Mobiles)
            {
                try
                {
                    if (mobile.MobileFlags != null && mobile.MobileFlags.Any(a => a == MobileFlags.Scavenger) && mobile.Race != Race.Animal)
                    {
                        if (!mobile.CharacterFlags.Contains(CharacterFlags.Fighting) && !mobile.CharacterFlags.Contains(CharacterFlags.Charmed) && !mobile.CharacterFlags.Contains(CharacterFlags.Sleeping))
                        {
                            // Scavenging mobiles have a 50% chance each tick to pick something up.
                            var scavenge = this.random.Next(0, 100);
                            if (scavenge <= 50)
                            {
                                var items = room.Items;

                                if (items.Count > 0)
                                {
                                    try
                                    {
                                        var random = this.random.Next(0, items.Count - 1);
                                        var itemToGet = items[random];

                                        var clonedItem = itemToGet.DeepCopy();

                                        await this.communicator.SendToRoom(mobile.Location, $"{mobile.FirstName.FirstCharToUpper()} picks up {clonedItem.Name}.", cancellationToken);

                                        var targetWearLocation = clonedItem.WearLocation.FirstOrDefault(w => w != WearLocation.None);

                                        var equipped = mobile.Equipment.FirstOrDefault(i => i.WearLocation.Contains(targetWearLocation));

                                        // If they don't have anything equipped there, equip it.
                                        if (equipped == null && !clonedItem.WearLocation.Contains(WearLocation.InventoryOnly))
                                        {
                                            this.logger.Info($"{mobile.FirstName.FirstCharToUpper()} found a {clonedItem.ShortDescription} and is wearing it now.", this.communicator);
                                            var verb = clonedItem.ItemType == ItemType.Weapon ? "wields" : "wears";
                                            mobile.Equipment.Add(clonedItem);
                                            await this.communicator.SendToRoom(mobile.Location, $"{mobile.FirstName.FirstCharToUpper()} {verb} {clonedItem.Name}.", cancellationToken);
                                        }
                                        else
                                        {
                                            this.logger.Info($"{mobile.FirstName.FirstCharToUpper()} found a {clonedItem.ShortDescription} and added it to their inventory.", this.communicator);

                                            // Just add to inventory.
                                            mobile.Inventory.Add(clonedItem);
                                        }

                                        itemsToRemove.Add(itemToGet);
                                    }
                                    catch (Exception exc)
                                    {
                                        this.logger.Error("An error occurred at this checkpoint SCAVENGE.", exc, this.communicator);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception exc)
                {
                    this.logger.Error("An error occurred while processing mobile scavenge.", exc, this.communicator);
                }
            }

            // Remove any items the mobs picked up.
            room.Items.RemoveAll(i => itemsToRemove.Contains(i));
        }
    }
}
