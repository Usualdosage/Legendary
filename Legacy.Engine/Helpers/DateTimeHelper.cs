﻿// <copyright file="DateTimeHelper.cs" company="Legendary™">
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

    /// <summary>
    /// Methods and such for calculating the local "game" date and time.
    /// </summary>
    public class DateTimeHelper
    {
        /// <summary>
        /// Gets the days of the week.
        /// </summary>
        public static readonly string[] DaysOfWeek = new string[6] { "Khoday", "Ashday", "Meriday", "Vashday", "Fireday", "Sauraday" };

        /// <summary>
        /// Gets the months of the year.
        /// </summary>
        public static readonly string[] MonthsOfYear = new string[12] { "Khodarus", "Ashuran", "Faustan", "Indiran", "Meriman", "Chiran", "Trajan", "Vashuran", "Atrinan", "Khodan", "Sauran", "Rathan" };

        /// <summary>
        /// Gets all of the date and time information.
        /// </summary>
        /// <param name="gameDay">The day.</param>
        /// <param name="gameMonth">The month.</param>
        /// <param name="gameYear">The year.</param>
        /// <param name="gameHour">The hour.</param>
        /// <param name="gameMinute">The minute.</param>
        /// <param name="gameSecond">The second.</param>
        /// <returns>String.</returns>
        public static string GetDate(int gameDay, int gameMonth, int gameYear, int gameHour, int gameMinute, int gameSecond)
        {
            var displayText =
            "It is " +
            FormatSeason(gameMonth) +
            " on " +
            DaysOfWeek[gameDay % 6] +
            ", the " +
            FormatNumber(gameDay) +
            " day of " +
            MonthsOfYear[gameMonth] +
            ", in the year " +
            gameYear +
            ". The time is " +
            FormatTime(gameHour, gameMinute, gameSecond) +
            ". " +
            GetMoon(gameDay).Item1 +
            " " +
            GetHolidays(gameMonth, gameDay);

            return displayText;
        }

        private static Tuple<string, string> GetMoon(int day)
        {
            var moonMessage = string.Empty;
            var moonImage = string.Empty;
            var moonDay = day % 24;

            switch (moonDay)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                    moonMessage = "There is a waxing crescent moon tonight.";
                    moonImage =
                        "https://img.icons8.com/emoji/48/undefined/waxing-crescent-moon.png";
                    break;
                case 6:
                case 18:
                    moonMessage = "There is a half moon tonight.";
                    moonImage =
                        "https://img.icons8.com/emoji/48/undefined/first-quarter-moon.png";
                    break;
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                    moonMessage = "There is a waxing gibbous moon tonight.";
                    moonImage =
                        "https://img.icons8.com/emoji/48/undefined/waxing-gibbous-moon.png";
                    break;
                case 12:
                    moonMessage = "There is a full moon tonight.";
                    moonImage =
                        "https://img.icons8.com/emoji/48/undefined/full-moon-emoji.png";
                    break;
                case 13:
                case 14:
                case 15:
                case 16:
                case 17:
                    moonMessage = "There is a waning gibbous moon tonight.";
                    moonImage =
                        "https://img.icons8.com/emoji/48/undefined/waning-gibbous-moon.png";
                    break;
                case 19:
                case 20:
                case 21:
                case 22:
                case 23:
                    moonMessage = "There is a waning crescent moon tonight.";
                    moonImage =
                        "https://img.icons8.com/emoji/48/undefined/waning-crescent-moon.png";
                    break;
                case 0:
                case 24:
                    moonMessage = "There is a new moon tonight.";
                    moonImage =
                        "https://img.icons8.com/emoji/48/undefined/new-moon-emoji.png";
                    break;
            }

            return new Tuple<string, string>(moonMessage, moonImage);
        }

        private static string FormatNumber(int gameDay)
        {
            return gameDay switch
            {
                1 or 21 => gameDay + "st",
                2 or 22 => gameDay + "nd",
                3 or 23 => gameDay + "rd",
                _ => gameDay + "th",
            };
        }

        private static string FormatSeason(int gameMonth)
        {
            return gameMonth switch
            {
                4 or 5 or 6 => "summer",
                7 or 8 or 9 => "autumn",
                10 or 11 or 12 => "winter",
                _ => "spring",
            };
        }

        private static string FormatTime(int hour, int minute, int second)
        {
            var ampm = "AM";
            var displayHour = hour;
            string displayMinute = minute.ToString();
            string displaySecond = second.ToString();

            if (hour >= 12)
            {
                displayHour = hour - 12;
                ampm = "PM";
            }

            if (minute < 10)
            {
                displayMinute = "0" + minute;
            }

            if (second < 10)
            {
                displaySecond = "0" + second;
            }

            return displayHour + ":" + displayMinute + ":" + displaySecond + " " + ampm;
        }

        private static string GetHolidays(int month, int day)
        {
            switch (month)
            {
                default:
                    return "There are no holidays today.";
                case 1:
                    {
                        return day switch
                        {
                            1 => "Today is New Year's Day.",
                            30 => "Today is The Celebration of the Two Chalices.",
                            _ => "There are no holidays today.",
                        };
                    }

                case 2:
                    {
                        return day switch
                        {
                            // 31
                            1 => "Today is the Day of the Dead.",

                            // 39
                            9 => "Today is Shayl Nag.",

                            // 56
                            26 => "Today is The Bonfire.",
                            _ => "There are no holidays today.",
                        };
                    }

                case 3:
                    {
                        return day switch
                        {
                            // 64
                            4 => "Today is Darul Tine.",
                            _ => "There are no holidays today.",
                        };
                    }

                case 4:
                    {
                        return day switch
                        {
                            // 101
                            11 => "Today is Nature Day.",
                            _ => "There are no holidays today.",
                        };
                    }

                case 5:
                    {
                        return day switch
                        {
                            // 133
                            13 => "Today is Mother's Day.",

                            // 150
                            30 => "Today is The Baptism.",
                            _ => "There are no holidays today.",
                        };
                    }

                case 6:
                    {
                        return day switch
                        {
                            // 158
                            8 => "Today is The Mystran Joust.",

                            // 173
                            23 => "Today is The Day of Remembrance.",
                            _ => "There are no holidays today.",
                        };
                    }

                case 7:
                    {
                        return day switch
                        {
                            // 195
                            15 => "Today is The Day of Purgation.",
                            _ => "There are no holidays today.",
                        };
                    }

                case 8:
                    {
                        return day switch
                        {
                            // 238
                            28 => "Today is The Illumination.",
                            _ => "There are no holidays today.",
                        };
                    }

                case 9:
                    {
                        return day switch
                        {
                            // 241
                            1 => "Today is The Day of Peace.",

                            // 255
                            15 => "Today is The Sailfin Regatta.",

                            // 260
                            20 => "Today is Veteran's Day.",
                            _ => "There are no holidays today.",
                        };
                    }

                case 10:
                    {
                        return day switch
                        {
                            // 273
                            3 => "Today is Labor Day.",
                            _ => "There are no holidays today.",
                        };
                    }

                case 11:
                    {
                        return day switch
                        {
                            // 301
                            1 => "Today is The Day of the Shadows.",

                            // 315
                            15 => "Today is The Burning of the Lamps.",

                            // 350
                            28 => "Today is The Call to Power.",
                            _ => "There are no holidays today.",
                        };
                    }

                case 12:
                    {
                        return day switch
                        {
                            7 => "Today is the Dark Dance.",
                            20 => "Today is Winter's End.",
                            30 => "Today is Cayl Tine.",
                            _ => "There are no holidays today.",
                        };
                    }
            }
        }
    }
}