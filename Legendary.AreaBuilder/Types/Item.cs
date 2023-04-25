// <copyright file="Item.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.AreaBuilder.Types
{
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;
    using Legendary.Core.Contracts;
    using Legendary.Core.Types;
    using MongoDB.Bson.Serialization.Attributes;

    /// <summary>
    /// Item class for the UI editor.
    /// </summary>
    [BsonIgnoreExtraElements]
    public class Item : IItem
    {
        /// <inheritdoc/>
        [BsonElement("_id")]
        public long ItemId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [Category("Description")]
        [Description("The name of the item.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the level.
        /// </summary>
        [Category("General")]
        [Description("The level of the item.")]
        public int Level { get; set; } = 1;

        /// <summary>
        /// Gets or sets the short description.
        /// </summary>
        [Category("Description")]
        [Description("A brief description of the item.")]
        public string? ShortDescription { get; set; }

        /// <summary>
        /// Gets or sets the long description.
        /// </summary>
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        [Category("Description")]
        [Description("The long description of the item.")]
        public string? LongDescription { get; set; }

        /// <summary>
        /// Gets or sets the weight of the item.
        /// </summary>
        [Category("Physical")]
        [Description("The weight, in pounds, of the item.")]
        public decimal Weight { get; set; } = 0m;

        /// <summary>
        /// Gets or sets the durability of the armor.
        /// </summary>
        [Editor(typeof(MaxCurrentEditor), typeof(UITypeEditor))]
        [Category("Physical")]
        [Description("The durability (max and current) of the item.")]
        public MaxCurrent Durability { get; set; } = new MaxCurrent(0, 0);

        /// <summary>
        /// Gets or sets the percent chance to block piercing attacks.
        /// </summary>
        [Category("Armor")]
        [Description("Percentage defense against pierce.")]
        public int Pierce { get; set; } = 0;

        /// <summary>
        /// Gets or sets the percent chance to block blunt attacks.
        /// </summary>
        [Category("Armor")]
        [Description("Percentage defense against blunt.")]
        public int Blunt { get; set; } = 0;

        /// <summary>
        /// Gets or sets the percent chance to block edged attacks.
        /// </summary>
        [Category("Armor")]
        [Description("Percentage defense against edged.")]
        public int Edged { get; set; } = 0;

        /// <summary>
        /// Gets or sets the percent chance to block magic attacks.
        /// </summary>
        [Category("Armor")]
        [Description("Percentage defense against magic.")]
        public int Magic { get; set; } = 0;

        /// <summary>
        /// Gets or sets the item type.
        /// </summary>
        [Category("General")]
        [Description("The type of the item.")]
        public ItemType ItemType { get; set; } = ItemType.Weapon;

        /// <summary>
        /// Gets or sets the item kind.
        /// </summary>
        [Category("General")]
        [Description("The kind of the item.")]
        public ItemKind ItemKind { get; set; } = ItemKind.Common;

        /// <summary>
        /// Gets or sets the hit dice.
        /// </summary>
        [Category("Weapon")]
        [Description("Number of hit dice to roll.")]
        public int HitDice { get; set; } = 0;

        /// <summary>
        /// Gets or sets the damage dice.
        /// </summary>
        [Category("Weapon")]
        [Description("Number of damage dice to roll.")]
        public int DamageDice { get; set; } = 0;

        /// <summary>
        /// Gets or sets the damage modifier.
        /// </summary>
        [Category("Weapon")]
        [Description("Additional damage modifier.")]
        public int Modifier { get; set; } = 0;

        /// <summary>
        /// Gets or sets the damage type.
        /// </summary>
        [Category("Weapon")]
        [Description("The damage type.")]
        public DamageType DamageType { get; set; } = DamageType.None;

        /// <summary>
        /// Gets or sets the armor flags.
        /// </summary>
        [Category("General")]
        [Description("The item flags.")]
        public IList<ItemFlags> ItemFlags { get; set; } = new List<ItemFlags>();

        /// <summary>
        /// Gets or sets the wear location(s) of this equipment.
        /// </summary>
        [Category("General")]
        [Description("The wear location of the item.")]
        public IList<WearLocation> WearLocation { get; set; } = new List<WearLocation>();

        /// <summary>
        /// Gets or sets the value of the item.
        /// </summary>
        [Category("Value")]
        [Description("Gold/silver/copper value of item.")]
        public decimal Value { get; set; } = 0m;

        /// <summary>
        /// Gets or sets the item image.
        /// </summary>
        [Category("Description")]
        [Description("The image of the item.")]
        public string? Image { get; set; }

        /// <summary>
        /// Gets or sets the liquid type if this is a spring or a drink.
        /// </summary>
        [Category("Food and Drink")]
        [Description("The liquid type in the item.")]
        public LiquidType LiquidType { get; set; } = LiquidType.None;

        /// <summary>
        ///  Gets or sets the number of draughts if this is a drink.
        /// </summary>
        [Editor(typeof(MaxCurrentEditor), typeof(UITypeEditor))]
        [Category("Food and Drink")]
        [Description("The number of drinks in the item.")]
        public MaxCurrent? Drinks { get; set; } = new MaxCurrent(0, 0);

        /// <summary>
        ///  Gets or sets the food value if this is a food item.
        /// </summary>
        [Editor(typeof(MaxCurrentEditor), typeof(UITypeEditor))]
        [Category("Food and Drink")]
        [Description("The number of meals in the item.")]
        public MaxCurrent? Food { get; set; } = new MaxCurrent(0, 0);

        /// <summary>
        ///  Gets or sets the carry weight if this is a container.
        /// </summary>
        [Editor(typeof(MaxCurrentEditor), typeof(UITypeEditor))]
        [Category("Containers")]
        [Description("The weight of items this container can carry.")]
        public MaxCurrent? CarryWeight { get; set; } = new MaxCurrent(0, 0);

        /// <inheritdoc/>
        [Category("Containers")]
        [Description("Indicates if the container is closed.")]
        public bool IsClosed { get; set; } = false;

        /// <inheritdoc/>
        [Category("Containers")]
        [Description("Indicates if the container is locked.")]
        public bool IsLocked { get; set; } = false;

        /// <inheritdoc/>
        [Category("Containers")]
        [Description("The id of the key that unlocks this container.")]
        public long? KeyId { get; set; }

        /// <summary>
        /// Gets or sets the items reset inside the container (if it's a container).
        /// </summary>
        [Category("Containers")]
        [Description("Items reset inside the container.")]
        public List<long>? ItemResets { get; set; } = new List<long>();

        /// <summary>
        /// Gets or sets what the item contains. Not browsable.
        /// </summary>
        [Browsable(false)]
        public List<IItem>? Contains { get; set; }

        /// <summary>
        /// Gets or sets the name of the spell the item casts.
        /// </summary>
        [Category("Magic")]
        [Description("The spell this item will cast.")]
        public string? SpellName { get; set; }

        /// <summary>
        /// Gets or sets the level the spell is cast.
        /// </summary>
        [Category("Magic")]
        [Description("The level of spell this item will cast.")]
        public int? CastLevel { get; set; } = 0;

        /// <summary>
        /// Gets or sets the cast frequence in ticks.
        /// </summary>
        [Category("Magic")]
        [Description("The frequency, in ticks, this item will cast the spell. Use 0 for permanent effects.")]
        public int? CastFrequency { get; set; } = 0;

        /// <summary>
        /// Gets or sets the weapon type.
        /// </summary>
        [Category("Weapon")]
        [Description("The weapon type.")]
        public WeaponType WeaponType { get; set; } = WeaponType.Sword;

        /// <summary>
        /// Gets or sets the save modifiers.
        /// </summary>
        [Category("Save Modifiers")]
        [Description("Bonuses to spell saving throws.")]
        public int? SaveSpell { get; set; } = 0;

        /// <summary>
        /// Gets or sets the save modifiers.
        /// </summary>
        [Category("Save Modifiers")]
        [Description("Bonuses to negative saving throws.")]
        public int? SaveNegative { get; set; } = 0;

        /// <summary>
        /// Gets or sets the save modifiers.
        /// </summary>
        [Category("Save Modifiers")]
        [Description("Bonuses to maledictive saving throws.")]
        public int? SaveMaledictive { get; set; } = 0;

        /// <summary>
        /// Gets or sets the save modifiers.
        /// </summary>
        [Category("Save Modifiers")]
        [Description("Bonuses to afflictive saving throws.")]
        public int? SaveAfflictive { get; set; } = 0;

        /// <summary>
        /// Gets or sets the save modifiers.
        /// </summary>
        [Category("Save Modifiers")]
        [Description("Bonuses to death saving throws.")]
        public int? SaveDeath { get; set; } = 0;

        /// <inheritdoc/>
        [Browsable(false)]
        public Guid UniqueId => Guid.NewGuid();

        /// <inheritdoc/>
        [Browsable(false)]
        public KeyValuePair<long, long> Location { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <inheritdoc/>
        [Category("General")]
        [Description("The amount of time before this item disintegrates/rots.")]
        public int RotTimer { get; set; }

        /// <inheritdoc/>
        [Category("General")]
        [Description("The item program file.")]
        public string? Program { get; set; } = string.Empty;

        /// <inheritdoc/>
        [Category("Containers")]
        [Description("Indicates if the container is trapped.")]
        public bool IsTrapped { get; set; } = false;
    }
}
