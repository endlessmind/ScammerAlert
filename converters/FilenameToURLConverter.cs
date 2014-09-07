using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Net;

namespace ScammerAlert.converters
{
    class FilenameToURLConverter : IValueConverter
    {

        private String IP;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Console.WriteLine(value);
            string strVal = value.ToString();

            WebClient client = new WebClient();
            IP = client.DownloadString("http://www.scilor.com/groovemobile/getServerIP.php");

            return "http://" + IP + "/scammers/attachment/" + strVal;
        }



        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return int.Parse(value.ToString());
        }
    }
}
