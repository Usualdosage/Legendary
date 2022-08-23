// <copyright file="ActionHelper.cs" company="Legendary™">
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
    using System.Linq;
    using System.Text;
    using Legendary.Core.Attributes;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Core.Types;

    /// <summary>
    /// Helper for creating instances of skills and spells by reflection.
    /// </summary>
    public class ActionHelper
    {
        private readonly ICommunicator communicator;
        private readonly IRandom random;
        private readonly Combat combat;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionHelper"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="combat">The combat generator.</param>
        public ActionHelper(ICommunicator communicator, IRandom random, Combat combat)
        {
            this.communicator = communicator;
            this.random = random;
            this.combat = combat;
        }

        /// <summary>
        /// Gets the description of the wear location.
        /// </summary>
        /// <param name="wearLocation">The wear location.</param>
        /// <returns>String.</returns>
        public static string GetWearLocationDescription(string wearLocation)
        {
            try
            {
                var enumType = typeof(WearLocation);
                var memberInfos =
                enumType.GetMember(wearLocation);
                var enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == enumType);
                var valueAttributes = enumValueMemberInfo?.GetCustomAttributes(typeof(WearDescription), false);

                if (valueAttributes != null && valueAttributes.Count() > 0)
                {
                    var descAttribute = valueAttributes[0] as WearDescription;
                    if (descAttribute != null)
                    {
                        return descAttribute.Description;
                    }
                    else
                    {
                        return WearLocation.None.ToString();
                    }
                }
                else
                {
                    return WearLocation.None.ToString();
                }
            }
            catch
            {
                return WearLocation.None.ToString();
            }
        }

        /// <summary>
        /// Gets the description of the liquid.
        /// </summary>
        /// <param name="liquidType">The liquid type.</param>
        /// <returns>String.</returns>
        public static string GetLiquidDescription(LiquidType liquidType)
        {
            try
            {
                var enumType = typeof(LiquidType);
                var memberInfos = enumType.GetMember(liquidType.ToString());
                var enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == enumType);
                var valueAttributes = enumValueMemberInfo?.GetCustomAttributes(typeof(LiquidDescription), false);

                if (valueAttributes != null && valueAttributes.Count() > 0)
                {
                    var descAttribute = valueAttributes[0] as LiquidDescription;
                    if (descAttribute != null)
                    {
                        return descAttribute.Description;
                    }
                    else
                    {
                        return LiquidType.None.ToString();
                    }
                }
                else
                {
                    return LiquidType.None.ToString();
                }
            }
            catch
            {
                return LiquidType.None.ToString();
            }
        }

        /// <summary>
        /// Decorates the item with its flagged attributes.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="displayString">Defaults to name, but can be anything.</param>
        /// <returns>String.</returns>
        public static string? DecorateItem(Item? item, string? displayString)
        {
            if (item == null)
            {
                return null;
            }

            StringBuilder sbItem = new StringBuilder();

            if (item.ItemFlags.Contains(ItemFlags.Glowing))
            {
                sbItem.Append("(<span class='item_glow_yellow'>Glowing</span>) ");
            }

            if (item.ItemFlags.Contains(ItemFlags.Humming))
            {
                sbItem.Append("(<span class='item_hum'>Humming</span>) ");
            }

            if (item.ItemFlags.Contains(ItemFlags.GlowBlue))
            {
                sbItem.Append("(<span class='item_glow_blue'>Glowing</span>) ");
            }

            if (item.ItemFlags.Contains(ItemFlags.GlowDark))
            {
                sbItem.Append("(<span class='item_glow_dark'>Glowing</span>) ");
            }

            if (item.ItemFlags.Contains(ItemFlags.GlowGreen))
            {
                sbItem.Append("(<span class='item_glow_green'>Glowing</span>) ");
            }

            if (item.ItemFlags.Contains(ItemFlags.GlowOrange))
            {
                sbItem.Append("(<span class='item_glow_orange'>Glowing</span>) ");
            }

            if (item.ItemFlags.Contains(ItemFlags.GlowPurple))
            {
                sbItem.Append("(<span class='item_glow_purple'>Glowing</span>) ");
            }

            if (item.ItemFlags.Contains(ItemFlags.GlowRed))
            {
                sbItem.Append("(<span class='item_glow_red'>Glowing</span>) ");
            }

            if (item.ItemFlags.Contains(ItemFlags.GlowWhite))
            {
                sbItem.Append("(<span class='item_glow_white'>Glowing</span>) ");
            }

            sbItem.Append(displayString ?? item.Name);

            return sbItem.ToString();
        }

        /// <summary>
        /// Gets the equipment the actor is wearing.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <returns>String.</returns>
        public string GetEquipment(Character actor)
        {
            StringBuilder sb = new StringBuilder();

            // Worn items.
            var wearLocations = Enum.GetNames<WearLocation>();

            sb.Append("<table class='wear-table'>");

            foreach (var wearLocation in wearLocations)
            {
                var description = GetWearLocationDescription(wearLocation);

                if (description.ToLower() == "none")
                {
                    continue;
                }

                var location = Enum.Parse<WearLocation>(wearLocation);
                var item = actor.Equipment.FirstOrDefault(a => a.WearLocation.Contains(location));

                sb.Append("<tr>");
                sb.Append($"<td class='wear-table-location'>{description}</td><td class='wear-table-item'>{DecorateItem(item, null) ?? "nothing."}");

                if (item != null && item.ItemType == ItemType.Armor && item.Durability.Max != 0)
                {
                    sb.Append($"<span class='equipmentwear'><progress max='{item?.Durability.Max}' value='{item?.Durability.Current}'></progress></span>");
                }

                sb.Append("</td></tr>");
            }

            sb.Append("</table>");

            return sb.ToString();
        }

        /// <summary>
        /// Gets only the equipment the actor is wearing.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <returns>String.</returns>
        public string GetOnlyEquipment(Character actor)
        {
            StringBuilder sb = new StringBuilder();

            // Worn items.
            var wearLocations = Enum.GetNames<WearLocation>();

            sb.Append("<table class='wear-table'>");

            foreach (var wearLocation in wearLocations)
            {
                var description = GetWearLocationDescription(wearLocation);

                if (description.ToLower() == "none")
                {
                    continue;
                }

                var location = Enum.Parse<WearLocation>(wearLocation);
                var gear = DecorateItem(actor.Equipment.FirstOrDefault(a => a.WearLocation.Contains(location)), null);

                if (!string.IsNullOrWhiteSpace(gear))
                {
                    sb.Append("<tr>");
                    sb.Append($"<td class='wear-table-location'>{description}</td><td class='wear-table-item'>{gear}</td>");
                    sb.Append("</tr>");
                }
            }

            sb.Append("</table>");

            return sb.ToString();
        }

        /// <summary>
        /// Creates an instance of a skill or spell from the name.
        /// </summary>
        /// <param name="fullNamespace">The namespace of the action.</param>
        /// <param name="name">The name of the skill.</param>
        /// <typeparam name="T">The type (skill or spell).</typeparam>
        /// <returns>The IAction.</returns>
        public IAction? CreateActionInstance<T>(string fullNamespace, string name)
            where T : IAction
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }
            else
            {
                try
                {
                    Type? type = Type.GetType($"{fullNamespace}.{name}, Legendary.Engine");

                    if (type != null)
                    {
                        var instance = Activator.CreateInstance(type, this.communicator, this.random, this.combat);

                        if (instance is not null and T)
                        {
                            return (T)instance;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                catch
                {
                    throw;
                }
            }
        }
    }
}