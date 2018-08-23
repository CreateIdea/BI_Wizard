using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Markup;

namespace BI_Wizard.Helper
{

    //public enum AgeRange
    //{
    //    [Description("0 - 18 years")]
    //    Youth,
    //    [Description("18 - 65 years")]
    //    Adult,
    //    [Description("65+ years")]
    //    Senior,
    //}

    public class EnumToItemsSource : MarkupExtension
    {
        private readonly Type _type;

        public EnumToItemsSource(Type type)
        {
            _type = type;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Enum.GetValues(_type)
                .Cast<object>()
                .Select(e => new { Value = e.ToString(), DisplayName = EnumHelper.ToDescriptionString((Enum)e) });
        }
    }

    public static class EnumHelper
    {
        public static string ToDescriptionString(this Enum val)
        {
            var attribute =
                (DescriptionAttribute)
                val.GetType().GetField(val.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false).
                    SingleOrDefault();
            return attribute == default(DescriptionAttribute) ? val.ToString() : attribute.Description;
        }

        public static List<KeyValuePair<string, string>> GetEnumValueDescriptionPairs(Type enumType)
        {
            return Enum.GetValues(enumType)
                .Cast<Enum>()
                .Select(e => new KeyValuePair<string, string>(e.ToString(), e.ToDescriptionString()))
                .ToList();
        }

        /// <summary>
        /// Gets the description of a specific enum value.
        /// </summary>
        public static string Description(this Enum eValue)
        {
            var nAttributes = eValue.GetType().GetField(eValue.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);

            // If no description is found, best guess is to generate it by replacing underscores with spaces
            if (!nAttributes.Any())
            {
                TextInfo oTI = CultureInfo.CurrentCulture.TextInfo;
                return oTI.ToTitleCase(oTI.ToLower(eValue.ToString().Replace("_", " ")));
            }

            return (nAttributes.First() as DescriptionAttribute).Description;
        }

        /// <summary>
        /// Returns an enumerable collection of all values and descriptions for an enum type.
        /// </summary>
        public static IEnumerable<ValueDescription> GetAllValuesAndDescriptions<TEnum>() where TEnum : struct, IConvertible, IComparable, IFormattable
        {
            if (!typeof(TEnum).IsEnum)
                throw new ArgumentException("TEnum must be an Enumeration type");

            return Enum.GetValues(typeof(TEnum)).Cast<Enum>().Select((e) => new ValueDescription() { Value = e, Description = e.Description() }).ToList();
        }


        public class ValueDescription
        {
            public object Value;
            public string Description;
        }
    }
}
