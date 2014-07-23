using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace ScammerAlert.converters
{
    public class FilenameToURLConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string strVal = value.ToString();

            return "http://89.160.119.29/scammers/attachment/" + strVal;
        }



        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return int.Parse(value.ToString());
        }
    }
}
