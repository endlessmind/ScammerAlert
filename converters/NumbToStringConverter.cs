using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace ScammerAlert.converters
{
    public class NumbToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Console.WriteLine(value);
            string strVal = value.ToString();
            
            if (string.IsNullOrEmpty(strVal))
                return 0;

            else
                return strVal + " reports";
        }



        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return int.Parse(value.ToString());
        }
    }
}
