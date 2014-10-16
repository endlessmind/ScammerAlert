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
using ScammerAlert.converters;
using System.Threading;

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
                        r.Name = callback.Name;
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

        public string[] getAvatarAndName(String id) {
            using (WebAPI.Interface steamFriedList = WebAPI.GetInterface("ISteamUser", "9DF293619722CA60815A3354C19DAB4F"))
            {
                Dictionary<string, string> MyArgs = new Dictionary<string, string>();
                MyArgs["steamids"] = "[" + Utils.GetCommunityID(id) + "]";
                KeyValue MyResult = steamFriedList.Call("GetPlayerSummaries", 2, MyArgs);
                string[] values = new string[2];
                values[0] = MyResult.Children[0].Children[0]["personaname"].Value;
                values[1] = MyResult.Children[0].Children[0]["avatarfull"].Value;
                return values;
            }
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

                    //main.GetFriendInfo(id); //Not used right now, because this requires the user to login and get the communication token.
                    //But if we want to check the friendlist without logging in, then we won't get the communication token, so we'll have to use and other function
                    //To get the current Nickname and profile pic of the "reporter" (see thread @ 181)



                    if (i < reports.Count)
                    {
                        reports[i].Attachment = sql.getAttachment(reports[i].ID, false);
                    }
                    else if (i == reports.Count)
                    {
                        reports[i].Attachment = sql.getAttachment(reports[i].ID, true);
                    }
                    Console.WriteLine(reports[i].ID);
                    Console.WriteLine(reports[i].Attachment.Count > 0 ? reports[i].Attachment[0].ReportID + "" : "null");
                    Console.WriteLine("//");
                }

                MotivationGrid.Visibility = System.Windows.Visibility.Visible;
                lbMotivation.ItemsSource = reports;


                Thread t1 = new Thread(new ThreadStart(delegate
                 {
                     foreach (report r in lbMotivation.ItemsSource)
                     {
                         string[] values = getAvatarAndName(r.SteamID);
                         r.Name = values[0];
                         r.AvatarURL = values[1]; 
                     }
                 }));
                t1.Start();
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            MotivationGrid.Visibility = System.Windows.Visibility.Collapsed;
        }

    }
}
