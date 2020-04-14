using System;
using System.Globalization;
using System.Windows.Data;

namespace ZenovaLauncher
{
    [ValueConversion(typeof(bool), typeof(object))]
    public class BooleanValueConverter : IValueConverter
    {
        public object FalseValue { get; set; }
        public object TrueValue { get; set; }

        #region IValueConverter Members

        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            return (bool)value ? this.TrueValue : this.FalseValue;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            return object.Equals(this.TrueValue, value) ? true : false;
        }

        #endregion
    }
}
