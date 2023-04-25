// <copyright file="ItemExtensions.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.AreaBuilder.Extensions
{
    /// <summary>
    /// Extensions for items.
    /// </summary>
    public static class ItemExtensions
    {
        /// <summary>
        /// Converts an AreaBuilder type to a core domain model.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>Core.Models.Item.</returns>
        public static Core.Models.Item ToDomainModel(this Legendary.AreaBuilder.Types.Item item)
        {
            return new Core.Models.Item() 
            { 
                SaveAfflictive = item.SaveAfflictive,
                Blunt = item.Blunt,
                CarryWeight = item.CarryWeight,
                CastFrequency = item.CastFrequency,
                CastLevel = item.CastLevel,
                Contains = item.Contains,
                DamageDice = item.DamageDice,
                DamageType = item.DamageType,
                Drinks = item.Drinks,
                Durability = item.Durability,
                Edged = item.Edged,
                Food = item.Food,
                HitDice = item.HitDice,
                Image = item.Image,
                IsClosed = item.IsClosed,
                IsLocked = item.IsLocked,
                IsNPCCorpse = false,
                IsPlayerCorpse = false,
                IsTrapped = item.IsTrapped,
                ItemFlags = item.ItemFlags,
                ItemId = item.ItemId,
                SaveDeath = item.SaveDeath,
                SaveMaledictive = item.SaveMaledictive,
                SaveNegative = item.SaveNegative,
                SaveSpell = item.SaveSpell,
                ShortDescription = item.ShortDescription,
                SpellName = item.SpellName,
                ItemKind = item.ItemKind,
                ItemResets = item.ItemResets,
                ItemType = item.ItemType,
                KeyId = item.KeyId,
                Level = item.Level,
                LiquidType = item.LiquidType,
                LongDescription = item.LongDescription,
                Magic = item.Magic,
                Modifier = item.Modifier,
                Name = item.Name,
                Pierce = item.Pierce,
                Program = item.Program,
                RotTimer = item.RotTimer,
                Value = item.Value,
                WeaponType = item.WeaponType,
                WearLocation = item.WearLocation,
                Weight = item.Weight,
            };
        }
    }
}
