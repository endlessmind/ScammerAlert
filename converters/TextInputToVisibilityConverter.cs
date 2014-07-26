using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace ScammerAlert.converters
{
    public class TextInputToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Always test MultiValueConverter inputs for non-null
            // (to avoid crash bugs for views in the designer)
            
            bool hasText = false;
            if (values[0] is bool && values[1] is bool)
            {
                hasText = !(bool)values[0];
                bool hasFocus = (bool)values[1];


            }
            else if (values[0] is Int32)
            {
                hasText = (bool)values[1];
            }

            if (hasText)
                return Visibility.Collapsed;

            return Visibility.Visible;
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
