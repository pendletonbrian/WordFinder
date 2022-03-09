using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WordFinder.Classes
{
    public static class Extensions
    {
        internal static bool Contains(this string sourceString, string term, StringComparison comparison)
        {
            return sourceString?.IndexOf(term, comparison) >= 0;
        }

        /// <summary>
        /// Gets a string representation of the given TimeSpan in a easily human
        /// readable format.
        /// </summary>
        /// <param name="timespan">
        /// </param>
        /// <returns>
        /// </returns>
        internal static string GetTimeFromTimeSpan(this TimeSpan timespan)
        {
            bool hasDays = false;
            bool hasHours = false;
            bool hasMinutes = false;

            StringBuilder s = new StringBuilder();

            if (0 < timespan.Days)
            {
                hasDays = true;

                if (1 == timespan.Days)
                {
                    _ = s.Append(timespan.Days + " day");
                }
                else
                {
                    _ = s.Append(timespan.Days + " days");
                }
            }

            if (0 < timespan.Hours)
            {
                hasHours = true;

                if (hasDays)
                {
                    _ = s.Append(", ");
                }

                if (1 == timespan.Hours)
                {
                    _ = s.Append(timespan.Hours + " hour");
                }
                else
                {
                    _ = s.Append(timespan.Hours + " hours");
                }
            }

            if (0 < timespan.Minutes)
            {
                hasMinutes = true;

                if (hasDays ||
                    hasHours)
                {
                    _ = s.Append(", ");
                }

                if (1 == timespan.Minutes)
                {
                    _ = s.Append(timespan.Minutes + " minute");
                }
                else
                {
                    _ = s.Append(timespan.Minutes + " minutes");
                }
            }

            if (0 < timespan.Seconds)
            {
                if (hasDays ||
                    hasHours ||
                    hasMinutes)
                {
                    _ = s.Append(", ");
                }

                if (0 < timespan.Milliseconds)
                {
                    _ = s.Append($"{timespan.Seconds}.{timespan.Milliseconds.ToString("###")} seconds");
                }
                else
                {
                    if (1 == timespan.Seconds)
                    {
                        _ = s.Append(timespan.Seconds + " second");
                    }
                    else
                    {
                        _ = s.Append(timespan.Seconds + " seconds");
                    }
                }
            }
            else if (0 < timespan.Milliseconds)
            {
                // No seconds.

                if (hasDays ||
                    hasHours ||
                    hasMinutes)
                {
                    _ = s.Append(".");
                }
                else
                {
                    _ = s.Append("0.");
                }

                _ = s.Append($"{timespan.Milliseconds.ToString("###")} seconds");
            }

            return s.ToString();
        }

        public static string GetDescription(this Enum val)
        {
            var attribute = (DescriptionAttribute)val
                .GetType()
                .GetField(val.ToString())
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .SingleOrDefault();

            return attribute == default(DescriptionAttribute) ? val.ToString() : attribute.Description;
        }

        public static T GetValueFromDescription<T>(this string description) where T : Enum
        {
            foreach (var field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    if (attribute.Description.Equals(description, StringComparison.OrdinalIgnoreCase))
                    {
                        return (T)field.GetValue(null);
                    }
                }
                else
                {
                    if (field.Name.Equals(description, StringComparison.OrdinalIgnoreCase))
                        return (T)field.GetValue(null);
                }
            }

            throw new ArgumentException("Not found.", nameof(description));
        }

    }
}
