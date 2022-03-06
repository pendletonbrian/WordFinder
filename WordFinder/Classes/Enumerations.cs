using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace WordFinder.Classes
{
    public static class Enumerations
    {
        public enum Characters
        {
            [Description("None")]
            None = 0,

            [Description("1")]
            _1,

            [Description("2")]
            _2,

            [Description("3")]
            _3,

            [Description("4")]
            _4,

            [Description("5")]
            _5,

            [Description("6")]
            _6,

            [Description("7")]
            _7,

            [Description("8")]
            _9,

            [Description("10")]
            _10
        }

        public enum Letters
        {
            [Description("None")]
            None = 0,

            [Description("A")]
            A,

            [Description("B")]
            B,

            [Description("C")]
            C,

            [Description("D")]
            D,

            [Description("E")]
            E,

            [Description("F")]
            F,

            [Description("G")]
            G,

            [Description("H")]
            H,

            [Description("I")]
            I,

            [Description("J")]
            J,

            [Description("K")]
            K,

            [Description("L")]
            L,

            [Description("M")]
            M,

            [Description("N")]
            N,

            [Description("O")]
            O,

            [Description("P")]
            P,

            [Description("Q")]
            Q,

            [Description("R")]
            R, 
            
            [Description("S")]
            S,

            [Description("T")]
            T,

            [Description("U")]
            U,

            [Description("V")]
            V,

            [Description("W")]
            W,

            [Description("X")]
            X,

            [Description("Y")]
            Y,

            [Description("Z")]
            Z
        }

        /// <summary>
        /// Method to get a list of values and descriptions for generating a
        /// list of controls.
        /// </summary>
        /// <param name="enumType">
        /// </param>
        /// <returns>
        /// </returns>
        public static List<KeyValuePair<string, string>> GetEnumValueDescriptionPairs(Type enumType)
        {
            return Enum.GetValues(enumType)
                .Cast<Enum>()
                .Select(e => new KeyValuePair<string, string>(e.ToString(), e.GetDescription()))
                .ToList();
        }
    }
}
