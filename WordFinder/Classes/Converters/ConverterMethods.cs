using System;
using System.Windows;

namespace WordFinder.Classes.Converters
{
    internal class ConverterMethods
    {
        internal static Visibility GetVisibilityMode(object parameter)
        {
            Visibility mode = Visibility.Visible;

            if (parameter == null)
            {
                return mode;
            }

            if (parameter is Visibility)
            {
                mode = (Visibility)parameter;
            }
            else
            {
                // Let's try to parse the parameter as a Visibility value,
                // throwing an exception when the parsing fails
                try
                {
                    mode = (Visibility)Enum.Parse(typeof(Visibility), parameter.ToString(), true);
                }
                catch (FormatException e)
                {
                    throw new FormatException("Invalid Visibility specified as the ConverterParameter.  Use Visible or Collapsed.", e);
                }
            }

            // Return the detected mode
            return mode;
        }

        internal static bool IsVisibilityInverted(object parameter)
        {
            return (GetVisibilityMode(parameter) == Visibility.Collapsed);
        }
    }
}
