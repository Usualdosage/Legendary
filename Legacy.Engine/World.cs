﻿// <copyright file="World.cs" company="Legendary™">
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
        private readonly ICacheService cache;
        private readonly SemaphoreSlim semaphore = new (1, 1);
        private DateTime? lastMemoryDate;

        /// <summary>
        /// Initializes a new instance of the <see cref="World"/> class.
        /// </summary>
        /// <param name="dataService">The areas within the world.</param>
        /// <param name="random">The random generator.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="communicator">The communicator.</param>
        /// <param name="cache">The cache.</param>
        public World(IDataService dataService, IRandom random, ILogger logger, ICommunicator communicator, ICacheService cache)
        {
            this.dataService = dataService;
            this.logger = logger;
            this.random = random;
            this.communicator = communicator;
            this.cache = cache;
        }

        /// <inheritdoc/>
        public HashSet<Area> Areas { get; private set; } = new HashSet<Area>();

        /// <inheritdoc/>
        public HashSet<Item> Items { get; private set; } = new HashSet<Item>();

        /// <inheritdoc/>
        public HashSet<Award> Awards { get; private set; } = new HashSet<Award>();

        /// <inheritdoc/>
        public HashSet<Persona> Personas { get; private set; } = new HashSet<Persona>();

        /// <inheritdoc/>
        public HashSet<Memory> Memories { get; set; } = new HashSet<Memory>();

        /// <inheritdoc/>
        public HashSet<Mobile> Mobiles { get; set; } = new HashSet<Mobile>();

        /// <inheritdoc/>
        public GameMetrics? GameMetrics { get; internal set; } = null;

        /// <summary>
        /// Adds all of the default skills to the mob.
        /// </summary>
        /// <param name="mobile">The mobile.</param>
        public static void ApplyMobileSkills(Mobile mobile)
        {
            // Apply skills to mobs based on their equipment.
            var wielded = mobile.Equipment.FirstOrDefault(c => c.Key == WearLocation.Wielded).Value;

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

        /// <inheritdoc/>
        public async Task LoadWorld()
        {
            var areas = await this.dataService.Areas.Find(Builders<Area>.Filter.Empty).ToListAsync();
            var items = await this.dataService.Items.Find(Builders<Item>.Filter.Empty).ToListAsync();
            var awards = await this.dataService.Awards.Find(Builders<Award>.Filter.Empty).ToListAsync();
            var personas = await this.dataService.Personas.Find(Builders<Persona>.Filter.Empty).ToListAsync();
            var memories = await this.dataService.Memories.Find(Builders<Memory>.Filter.Empty).ToListAsync();
            var mobiles = await this.dataService.Mobiles.Find(Builders<Mobile>.Filter.Empty).ToListAsync();

            // Cache common lookups as hash sets for faster reads.
            this.Areas = new HashSet<Area>(areas);
            this.Items = new HashSet<Item>(items);
            this.Awards = new HashSet<Award>(awards);
            this.Personas = new HashSet<Persona>(personas);
            this.Memories = new HashSet<Memory>(memories);
            this.Mobiles = new HashSet<Mobile>(mobiles);

            // Set the last memory date. Any new memories that come in will be serialized each tick.
            this.lastMemoryDate = memories.Max(m => m.LastInteraction);
        }

        /// <inheritdoc/>
        public async Task SaveMemories(CancellationToken cancellationToken)
        {
            if (this.lastMemoryDate.HasValue)
            {
                var newMemories = this.Memories.Where(m => m.LastInteraction >= this.lastMemoryDate).ToList();

                if (newMemories != null && newMemories.Any())
                {
                    this.lastMemoryDate = DateTime.UtcNow;

                    foreach (var memory in newMemories)
                    {
                        // It may not exist yet. If not, insert it.
                        var existingCursor = await this.dataService.Memories.FindAsync(m => m.CharacterId == memory.CharacterId && m.MobileId == memory.MobileId, cancellationToken: cancellationToken);
                        var existing = await existingCursor.FirstOrDefaultAsync(cancellationToken: cancellationToken);

                        if (existing != null)
                        {
                            existing.Memories = new List<string>(memory.Memories);
                            try
                            {
                                await this.dataService.Memories.ReplaceOneAsync(e => e.CharacterId == existing.CharacterId && e.MobileId == existing.MobileId, existing, cancellationToken: cancellationToken);
                            }
                            catch (Exception exc)
                            {
                                this.logger.Error(exc, this.communicator);
                            }
                        }
                        else
                        {
                            await this.dataService.Memories.InsertOneAsync(memory, new InsertOneOptions(), cancellationToken);
                        }
                    }
                }
            }
            else
            {
                this.lastMemoryDate = DateTime.UtcNow;
            }
        }

        /// <inheritdoc/>
        public void RepopulateItems(Area area)
        {
            var resets = area.Rooms?.SelectMany(r => r.ItemResets);

            var resetGroups = resets?.GroupBy(g => g);

            if (resetGroups != null)
            {
                foreach (var resetGroup in resetGroups)
                {
                    var itemId = resetGroup.Key;
                    var maxItems = resetGroup.Count();
                    var currentItems = area.Rooms?.Sum(r => r.Items?.Where(i => i.ItemId == itemId).Count());
                    var mobsInArea = this.communicator.GetMobilesInArea(area.AreaId);

                    // Scavenging mobs will pickup items. We don't want an eternal repop, so include those in the counts.
                    if (mobsInArea != null)
                    {
                        var mobInventoryItems = mobsInArea.Sum(m => m.Inventory.Where(i => i.ItemId == itemId).Count());
                        var mobEquipmentItems = mobsInArea.Sum(m => m.Equipment.Where(e => e.Value.ItemId == itemId).Count());
                        currentItems = currentItems + mobInventoryItems + mobEquipmentItems;
                    }

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
                        var repopRooms = area.Rooms?.Where(r => r.ItemResets.Contains(itemId)).ToList();

                        for (int x = 0; x < diff; x++)
                        {
                            // Get one of the rooms it normally would populate in at random.
                            var room = repopRooms?[this.random.Next(0, repopRooms.Count - 1)];

                            var item = this.Items.FirstOrDefault(i => i.ItemId == itemId);

                            // Get the item.
                            if (item != null && room != null)
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

                        var removalRooms = area.Rooms?.Where(r => r.Items.Any(i => i.ItemId == itemId)).ToList();

                        for (int x = 0; x < diff; x++)
                        {
                            var room = removalRooms?[this.random.Next(0, removalRooms.Count - 1)];

                            // Get the first item in the room that matches our key.
                            var item = room?.Items?.FirstOrDefault(r => r.ItemId == itemId);

                            if (item != null)
                            {
                                room?.Items?.Remove(item);
                            }
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void AddMemory(Character character, Mobile mobile, string memory)
        {
            var memoryObject = this.Memories.FirstOrDefault(m => m.CharacterId == character.CharacterId && m.MobileId == mobile.CharacterId);

            if (memoryObject == null)
            {
                memoryObject = new Memory(character.CharacterId, mobile.CharacterId)
                {
                    LastInteraction = DateTime.UtcNow,
                    Memories = new List<string>() { memory },
                };

                this.Memories.Add(memoryObject);
            }
            else
            {
                var newMemory = new Memory(memoryObject.CharacterId, memoryObject.MobileId)
                {
                    LastInteraction = DateTime.UtcNow,
                };

                newMemory.Memories.AddRange(memoryObject.Memories);
                newMemory.Memories.Add(memory);

                if (newMemory.Memories.Count > 30)
                {
                    newMemory.Memories.RemoveAt(0);
                }

                this.Memories.Remove(memoryObject);
                this.Memories.Add(newMemory);
            }
        }

        /// <inheritdoc/>
        public List<string>? GetMemories(Character character, Mobile mobile)
        {
            var memoryObject = this.Memories.FirstOrDefault(m => m.CharacterId == character.CharacterId && m.MobileId == mobile.CharacterId);
            return memoryObject?.Memories.Take(20).ToList() ?? null;
        }

        /// <inheritdoc/>
        public async void RepopulateMobiles(Area area)
        {
            var resets = area.Rooms?.SelectMany(r => r.MobileResets);

            var resetGroups = resets?.GroupBy(g => g);

            if (resetGroups != null)
            {
                foreach (var resetGroup in resetGroups)
                {
                    var mobCharacterId = resetGroup.Key;
                    var maxMobs = resetGroup.Count();
                    var currentMobs = area.Rooms?.Sum(r => r.Mobiles?.Where(m => m.CharacterId == mobCharacterId).Count());

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
                        var repopRooms = area.Rooms?.Where(r => r.MobileResets.Contains(mobCharacterId)).ToList();

                        for (int x = 0; x < diff; x++)
                        {
                            // Get one of the rooms they normally would populate in at random.
                            var room = repopRooms?[this.random.Next(0, repopRooms.Count - 1)];

                            var mobile = this.Mobiles.FirstOrDefault(m => m.CharacterId == mobCharacterId);

                            // Get the mobile.
                            if (mobile != null && room != null)
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

                                        await this.EquipMob(clone, itemClone, false);
                                    }
                                }

                                // Apply skills.
                                ApplyMobileSkills(clone);

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

                        var removalRooms = area.Rooms?.Where(r => r.Mobiles.Any(m => m.CharacterId == mobCharacterId)).ToList();

                        for (int x = 0; x < diff; x++)
                        {
                            var room = removalRooms?[this.random.Next(0, removalRooms.Count - 1)];

                            // Get the first mob in the room that matches our key and isn't fighting.
                            var mobile = room?.Mobiles?.FirstOrDefault(r => r.CharacterId == mobCharacterId && !r.CharacterFlags.Contains(CharacterFlags.Fighting));

                            if (mobile != null && room != null)
                            {
                                room.Mobiles?.Remove(mobile);
                            }
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        public async void Populate()
        {
            foreach (var area in this.Areas)
            {
                if (area.Rooms != null)
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
                                        await this.EquipMob(clone, itemClone, false);
                                    }
                                }

                                ApplyMobileSkills(clone);

                                room.Mobiles?.Add(clone);
                            }
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
            if (area.Rooms != null)
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
        }

        /// <inheritdoc/>
        public async Task CleanupItems(Area area)
        {
            if (area.Rooms != null)
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
        }

        /// <inheritdoc/>
        public async Task<GameMetrics> UpdateGameMetrics(Exception? lastException, DateTime? startup, CancellationToken cancellationToken)
        {
            try
            {
                var metrics = await this.dataService.GetGameMetrics();

                metrics ??= new GameMetrics();

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
                metrics.TotalRooms = this.Areas.Sum(a => a.Rooms != null ? a.Rooms.Count : 0);

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

                var questors = await this.dataService.Characters.Find(c => c.Awards != null && c.Awards.Count > 0 && !c.IsNPC).ToListAsync();

                if (questors != null)
                {
                    var questor = questors.OrderByDescending(o => o.Awards.Count).FirstOrDefault();

                    if (questor != null)
                    {
                        var questorName = questor.FirstName.ToLower().FirstCharToUpper();

                        // Check if we have a new hero.
                        if (!string.IsNullOrWhiteSpace(metrics.MasterQuestor) && metrics.MasterQuestor != questorName)
                        {
                            await this.communicator.SendGlobal($"{questorName} is now the <span class='say'>Master Questor</span> of Mystra!", cancellationToken);
                        }

                        metrics.MasterQuestor = questor.FirstName.ToLower().FirstCharToUpper();
                    }
                    else
                    {
                        metrics.MasterQuestor = "Nobody";
                    }
                }
                else
                {
                    metrics.MasterQuestor = "Nobody";
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
                    if (area.Rooms != null)
                    {
                        foreach (var room in area.Rooms)
                        {
                            // Decrement the rot timer for anything that can rot.
                            room.Items?.Where(i => i.RotTimer > -1).ToList().ForEach(r => r.RotTimer -= 1);
                            
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
                            await ProcessMobileAffects(room, cancellationToken);

                            // Move mobiles who wander.
                            await this.ProcessMobileWander(room, cancellationToken);
                        }
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

        /// <summary>
        /// Equips an item to a mob, as long as the mob isn't already wearing an item there.
        /// </summary>
        /// <param name="mobile">The mobile.</param>
        /// <param name="item">The item.</param>
        /// <param name="broadcast">If true, will broadcast this event to the room and logs.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task EquipMob(Mobile mobile, Item item, bool broadcast, CancellationToken cancellationToken = default)
        {
            // Get the first open wear location in a character's equipment that matches a wear location of the item.
            var openSlot = mobile.Equipment.FirstOrDefault(e => item.WearLocation.Contains(e.Key));

            if (openSlot.Value == null && !item.WearLocation.Contains(WearLocation.InventoryOnly) && !item.WearLocation.Contains(WearLocation.None))
            {
                if (item.WearLocation.Count > 0)
                {
                    mobile.Equipment.Add(item.WearLocation.First(), item);

                    if (broadcast)
                    {
                        if (item.ItemType == ItemType.Weapon)
                        {
                            await this.communicator.SendToRoom(mobile, mobile.Location, $"{mobile.FirstName.FirstCharToUpper()} wields {item.Name}.", cancellationToken);
                        }
                        else
                        {
                            await this.communicator.SendToRoom(mobile, mobile.Location, $"{mobile.FirstName.FirstCharToUpper()} wears {item.Name}.", cancellationToken);
                        }

                        this.logger.Debug($"{mobile.FirstName.FirstCharToUpper()} found a {item.Name} and is wearing it now.", this.communicator);
                    }
                }
            }
            else
            {
                if (broadcast)
                {
                    await this.communicator.SendToRoom(mobile, mobile.Location, $"{mobile.FirstName.FirstCharToUpper()} picks up {item.Name}.", cancellationToken);

                    this.logger.Debug($"{mobile.FirstName.FirstCharToUpper()} found a {item.Name} and added it to their inventory.", this.communicator);

                    // Just add to inventory.
                    mobile.Inventory.Add(item);
                }
            }
        }

        /// <inheritdoc/>
        public async Task ClearCache()
        {
            // await this.cache.ClearCache("Characters");
            await this.cache.ClearCache("Mobiles");
        }

        private static async Task ProcessMobileAffects(Room room, CancellationToken cancellationToken)
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
                                await effect.Action.OnTick(mobile, effect, cancellationToken);
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
            List<Mobile> removeMobiles = new ();

            await this.semaphore.WaitAsync(cancellationToken);

            try
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
                                                    await this.communicator.SendToRoom(mobile.Location, $"{mobile.FirstName.FirstCharToUpper()} opens the {dir} {exit.DoorName ?? "door"}.", cancellationToken);
                                                    exit.IsClosed = false;
                                                    continue;
                                                }
                                                else
                                                {
                                                    try
                                                    {
                                                        await this.communicator.SendToRoom(mobile.Location, $"{mobile.FirstName.FirstCharToUpper()} leaves {dir}.", cancellationToken);

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
                                                                await this.communicator.SendToRoom(mobileCopy.Location, $"{mobileCopy.FirstName.FirstCharToUpper()} enters.", cancellationToken);
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
            finally
            {
                this.semaphore.Release();
            }
        }

        private async Task DoMobileScavenge(Room room, CancellationToken cancellationToken)
        {
            List<Item> itemsToRemove = new ();

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

                                        if (itemToGet.WearLocation.Contains(WearLocation.None))
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            var clonedItem = itemToGet.DeepCopy();

                                            await this.EquipMob(mobile, clonedItem, true, cancellationToken);

                                            itemsToRemove.Add(itemToGet);
                                        }
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
