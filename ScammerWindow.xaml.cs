using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        ObservableCollection<report> reports;
        static SteamFriends steamFriends;

        private MySQL sql;

        private delegate void UpdateListBox(object list);
        private delegate void UpdateReportCollection(SteamFriends.PersonaStateCallback list);

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

            foreach (Scammer sc in scammer)
            {
                sc.Reported = sql.getReports(sc.ID).Count;
            }

            lbScammers.ItemsSource = scammer;
        }

        public void setMainWindow(MainWindow window)
        {
            main = window;
            sql = main.getMySQLInstance();
        }

        public void setSteamFriend(SteamFriends f)
        {
            steamFriends = f;
        }

        private void UpdateAvatars(SteamFriends.PersonaStateCallback callback)
        {
            try
            {


                foreach (report r in reports)
                {
                    if (r.SteamID.Equals(callback.FriendID.Render()))
                    {
                        r.AvatarURL = CreateAvatarURL(callback.AvatarHash);
                    }

                }
            }
            catch (Exception e) { }
        }


        private void setReportList(object list)
        {
            ObservableCollection<report> items = (ObservableCollection<report>)list;
            lbMotivation.ItemsSource = items;
        }

        public static string CreateAvatarURL(byte[] input)
        {
            String hash = input.Aggregate(new StringBuilder(),
                                   (sb, v) => sb.Append(v.ToString("x2"))
                                  ).ToString();

            String baseURL = "http://cdn.akamai.steamstatic.com/steamcommunity/public/images/avatars/";

            String extendedUrL = hash.Substring(0, 2) + "/" + hash + "_full.jpg";
            if (hash.Substring(0, 6) == "000000")
            {
                //The default avatar, black background with a with questionmark.
                return "http://cdn.akamai.steamstatic.com/steamcommunity/public/images/avatars/fe/fef49e7fa7e1997310d705b2a6158ff8dc1cdfeb_full.jpg";
            }
            return baseURL + extendedUrL; ;
        }

        public void OnPersonaState(SteamFriends.PersonaStateCallback callback)
        {
            //Oh no no no. When using ObserableCollections, we need to perform all and any changes on UI Thread!
            Dispatcher.Invoke(new UpdateReportCollection(UpdateAvatars), new object[] { callback });
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

        private void lbScammers_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lbScammers.SelectedIndex > -1)
            {


                Scammer scammer = (Scammer)lbScammers.SelectedItem;
                reports = sql.getReports(scammer.ID);

                for (int i = 0; i < reports.Count; i++)
                {
                    SteamID id = new SteamID();
                    id.SetFromString(reports[i].SteamID, EUniverse.Public);
                    main.GetFriendInfo(id);
                    if (i < reports.Count)
                    {
                        reports[i].Attachment = sql.getAttachment(reports[i].ID, false);
                    }
                    else if (i == reports.Count)
                    {
                        reports[i].Attachment = sql.getAttachment(reports[i].ID, true);
                    }
                }

                MotivationGrid.Visibility = System.Windows.Visibility.Visible;
                lbMotivation.ItemsSource = reports;
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            MotivationGrid.Visibility = System.Windows.Visibility.Collapsed;
        }

    }
}
