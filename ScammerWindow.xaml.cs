using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ScammerAlert.connection;
using SteamKit2;

namespace ScammerAlert
{
    /// <summary>
    /// Interaction logic for ScammerWindow.xaml
    /// </summary>
    public partial class ScammerWindow : Window
    {

        List<Scammer> scammer;
        MainWindow main;
        List<report> reports;
        static SteamFriends steamFriends;

        public ScammerWindow()
        {
            InitializeComponent();
        }


        public void ScammersInYourList(object scammers)
        {
            List<Scammer> list = (List<Scammer>)scammers;
            Scammer[] s = new Scammer[list.Count];
            list.CopyTo(s);
            scammer = s.ToList();
            lbScammers.ItemsSource = scammer;
        }

        public void setMainWindow(MainWindow window)
        {
            main = window;
        }

        public void setSteamFriend(SteamFriends f)
        {
            steamFriends = f;
        }

        public void setReports(object r)
        {
            List<report> list = (List<report>)r;
            report[] reportArray = new report[list.Count];
            list.CopyTo(reportArray);
            reports = reportArray.ToList();
        }

        public void OnPersonaState(SteamFriends.PersonaStateCallback callback)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - (this.Width + 221);
            this.Top = desktopWorkingArea.Bottom - this.Height;
        }


        private void lblClose_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void lbScammers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Scammer scammer = (Scammer)lbScammers.SelectedItem;
            Console.WriteLine(lbScammers.SelectedItem.ToString());
            main.FetchReports(scammer.ID);
        }

    }
}
