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
    using Legendary.Engine.Contracts;

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
        /// Gets the equipment the actor is wearing.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <returns>String.</returns>
        public static string GetEquipment(Character actor)
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

                sb.Append("<tr>");
                var location = Enum.Parse<WearLocation>(wearLocation);
                sb.Append($"<td class='wear-table-location'>{description}</td><td class='wear-table-item'>{actor.Equipment.FirstOrDefault(a => a.WearLocation.Contains(location))?.Name ?? "nothing."}</td>");
                sb.Append("</tr>");
            }

            sb.Append("</table>");

            return sb.ToString();
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

                if (valueAttributes != null)
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
            catch (Exception exc)
            {
                return WearLocation.None.ToString();
            }
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