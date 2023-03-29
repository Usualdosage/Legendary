// <copyright file="EnumExtensions.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine.Extensions
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Legendary.Core.Attributes;
    using Legendary.Core.Types;

    /// <summary>
    ///  Extension methods for enums.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Gets a WearDescription for a given wear location.
        /// </summary>
        /// <param name="wearLocation">The wear location.</param>
        /// <returns>WearDescription.</returns>
        public static WearDescription? ToWearDescription(this WearLocation wearLocation)
        {
            try
            {
                var enumType = typeof(WearLocation);
                var memberInfos = enumType.GetMember(wearLocation.ToString());
                var enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == enumType);
                var valueAttributes = enumValueMemberInfo?.GetCustomAttributes(typeof(WearDescription), false);

                if (valueAttributes != null && valueAttributes.Length > 0)
                {
                    var descAttribute = valueAttributes[0] as WearDescription;
                    if (descAttribute != null)
                    {
                        return descAttribute;
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
                return null;
            }
        }

        /// <summary>
        /// Gets a random enumeration value.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="en">The enum.</param>
        /// <returns>The value.</returns>
        /// <exception cref="Exception">Exception if not an enum.</exception>
        public static T RandomEnum<T>(this T en)
            where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new Exception("Type is not an enum.");
            }

            var random = new Random();
            var values = Enum.GetValues(typeof(T));
            if (values != null && values.Length > 0)
            {
                object? result = values.GetValue(random.Next(values.Length));

                if (result != null && result is T)
                {
                    return (T)result;
                }
            }

            return default(T);
        }
    }
}