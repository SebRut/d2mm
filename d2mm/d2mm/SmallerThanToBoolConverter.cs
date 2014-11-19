using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace de.sebastianrutofski.d2mm
{
    public class SmallerThanToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Int32.Parse(value.ToString()) < Int32.Parse(parameter.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}