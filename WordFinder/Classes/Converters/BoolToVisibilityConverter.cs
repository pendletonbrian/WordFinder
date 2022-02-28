using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WordFinder.Classes.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            var isVisible = (bool)value;

            if (ConverterMethods.IsVisibilityInverted(parameter))
            {
                isVisible = !isVisible;
            }

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            var isVisible = (Visibility)value == Visibility.Visible;

            // If visibility is inverted by the converter parameter, then invert
            // our value
            if (ConverterMethods.IsVisibilityInverted(parameter))
            {
                isVisible = !isVisible;
            }

            return isVisible;
        }
    }
}
