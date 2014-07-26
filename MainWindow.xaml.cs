using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ScammerAlert.connection;
using ScammerAlert.converters;
using System.Windows.Forms;
using System.Drawing;
using SteamKit2;
using System.Threading;
using System.IO;
using System.Linq;

namespace ScammerAlert
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ScammerWindow scamWindow;
        ReportWindow reportWindow;
        NotifyIcon icon = new NotifyIcon();
        Thread CallbackThread;
        System.Windows.Forms.Timer ScammerUpdater;

        static SteamClient steamClient;
        static SteamUser steamUser;
        static CallbackManager manager;
        static SteamFriends steamFriends;

        static SteamID idTest;


        static bool isRunning;
        static bool isConnected = false;
        static bool isGuardNeeded;
        static bool isWarningOpen = false;
        static bool isLoggedOut = false;

        ReadOnlyCollection<SteamFriends.FriendsListCallback.Friend> friendList;
        List<Scammer> scammersInFriends;
        List<String> foundScammersID = new List<String>();

        static string user = "", pass = "";
        static string authCode;
        static string reportThisID;
        static string myNickeName;
        private MySQL sql;
        private String connectInfo = "SERVER=89.160.119.29;" + "DATABASE=csgo_scammer;" + "UID=goscammer;" + "PASSWORD=z87qfbtYzSpMXUBz";
        public delegate void GuradVisibleCallback(bool value);
        public delegate void UpdateTestCallback(String value);
        private delegate void UpdateScammersList(object list);

        private System.Windows.Forms.ContextMenuStrip m_contextMenu;

        string cd = System.IO.Path.GetTempPath();
        string home = Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");

        public MainWindow()
        {
            InitializeComponent();
            m_contextMenu = new System.Windows.Forms.ContextMenuStrip();
            

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {


            cd += "AccXtract";
            LoginButtonEnableState(false);
            LoginButtonText("Connecting..");

            txtUsername.Focus();


            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width;
            this.Top = desktopWorkingArea.Bottom - this.Height;

            System.Windows.Forms.ToolStripMenuItem mI1 = new System.Windows.Forms.ToolStripMenuItem();
            mI1.Text = "Close";

            mI1.MouseUp += new System.Windows.Forms.MouseEventHandler(CloseMenuItem_Click);

            m_contextMenu.Items.Add(mI1);
            
            steamClient = new SteamClient();
            steamFriends = steamClient.GetHandler<SteamFriends>();

            

            steamUser = steamClient.GetHandler<SteamUser>();
            isRunning = true;

            

            icon.Icon = new Icon(@"icon.ico");
            icon.Visible = true;
            icon.Text = "Game Item Scammer Alerter";
            icon.Click += new EventHandler(iconClicked);
            icon.ContextMenuStrip = m_contextMenu;

            

            manager = new CallbackManager(steamClient);
            Console.WriteLine("Connecting to Steam...");

            new Callback<SteamUser.UpdateMachineAuthCallback>(OnMachineAuth, manager);
            new Callback<SteamUser.LoggedOnCallback>( OnLoggedOn, manager );
            new Callback<SteamFriends.ProfileInfoCallback>(OnProfileInfo, manager);
           // new Callback<SteamFriends.FriendsListCallback>(OnFriendsList, manager);
            new Callback<SteamFriends.PersonaStateCallback>(OnPersonaState, manager);


            new Callback<SteamClient.ConnectedCallback>(OnConnected, manager);
            new Callback<SteamClient.DisconnectedCallback>(OnDisconnected, manager);

           // new Callback<SteamUser.LoggedOnCallback>(OnLoggedOn, manager);
            new Callback<SteamUser.LoggedOffCallback>(OnLoggedOff, manager);

            // we use the following callbacks for friends related activities
            new Callback<SteamUser.AccountInfoCallback>(OnAccountInfo, manager);
            new Callback<SteamFriends.FriendsListCallback>(OnFriendsList, manager);
            new Callback<SteamFriends.PersonaStateCallback>(OnPersonaState, manager);
            new Callback<SteamFriends.FriendAddedCallback>(OnFriendAdded, manager);

            // initiate the connection
            steamClient.Connect();


            sql = new MySQL(connectInfo);
           // sql.OpenConnection();

            ScammerUpdater = new System.Windows.Forms.Timer();
            ScammerUpdater.Interval =3000;
            ScammerUpdater.Tick += new EventHandler(ScammerUpdater_Tick);
            ScammerUpdater.Start();
            

            CallbackThread = new Thread(new ThreadStart(runCallbacks));
            CallbackThread.Start();

            
            
        }

        public String getMySteamID()
        {
            if (steamUser.SteamID != null)
            {
                return steamUser.SteamID.Render();
            }
            else
            {
                return "STEAM_0:0:00000000";
            }
        }

        public String getMyNickName()
        {
            if (steamUser.SteamID != null)
            {
                return myNickeName;
            }
            else
            {
                return "Unknown";
            }
        }

        public MySQL getMySQLInstance()
        {
            return sql;
        }

        void iconClicked(object sender, EventArgs e)
        {
            
            this.WindowState = System.Windows.WindowState.Normal;
            this.ShowInTaskbar = true;

            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width;
            this.Top = desktopWorkingArea.Bottom - this.Height;
        }

        public void GetFriendInfo(SteamID id)
        {
            steamFriends.RequestFriendInfo(id);
        }

        private void reconnect()
        {
            isRunning = true;
            steamClient.Connect();

            CallbackThread = new Thread(new ThreadStart(runCallbacks));
            CallbackThread.Start();
        }

        private void runCallbacks()
        {
            while (isRunning)
            {

                try
                {
                    manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
                }
                catch (Exception e) { }

            }

        }

        private void ScammerUpdater_Tick(Object myObject, EventArgs myEventArgs)
        {

            try
            {

              //  Console.WriteLine("Tick");
                List<Scammer> scammers = sql.getAllScammers();
                if (scammersInFriends == null)
                {
                    scammersInFriends = new List<Scammer>();
                }
                else
                {
                    scammersInFriends.Clear();
                    foundScammersID.Clear();
                }

                foreach (Scammer s in scammers)
                {
                    s.Reported = sql.getReports(s.ID).Count;
                    foreach (SteamFriends.FriendsListCallback.Friend f in this.friendList)
                    {
                        if (s.SteamID.Equals(f.SteamID.Render()) && s.Reported > 1)
                        {
                          //  Console.WriteLine("Scammer found");
                            foundScammersID.Add(f.SteamID.Render());
                            SteamID id = new SteamID();
                            id.SetFromString(f.SteamID.Render(), EUniverse.Public);
                            steamFriends.RequestFriendInfo(id);

                            System.Windows.Media.Brush b = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF781E1E")); //Green: #FF55B44C
                            lblStatus.Foreground = b;
                            lblStatus.Content = "Unsafe";
                            lblShowScammers.Visibility = System.Windows.Visibility.Visible;
                            tbInfo.Text = "There seems to be some scammers in your friendlist, watch out!";

                        }
                    }
                }

                if (foundScammersID.Count < 1)
                {
                    System.Windows.Media.Brush b = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF55B44C")); //Red: #FF781E1E
                    lblStatus.Foreground = b;
                    lblStatus.Content = "Safe";
                    lblShowScammers.Visibility = System.Windows.Visibility.Collapsed;
                    tbInfo.Text = "There is currently no scammers identifyed in any active conversations or in your friends list";
                }


               

            }
            catch (Exception e) { /*Console.WriteLine(e.Message); Console.WriteLine(e.StackTrace);*/ }
        }

        private void isGuardEnable(bool value)
        {
            if (value)
            {
                SteamGuardGrid.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                SteamGuardGrid.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void isLoginVisible(bool value)
        {
            if (value)
            {
                LoginOverlay.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                LoginOverlay.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void isLoginButtonEnable(bool value)
        {
            btnLogin.IsEnabled = value;
        }

        private void setScammerIDName(String value) {
            lblIDName.Content = value;
        }

        private void setLoginButtonText(String value)
        {
            btnLogin.Content = value;
        }

        private void setScammersList(object list)
        {
            List<Scammer> items = (List<Scammer>)list;
            lbScammers.ItemsSource = items;
        }

        private void LoginButtonEnableState(bool value)
        {
            btnLogin.Dispatcher.Invoke(
                new GuradVisibleCallback(isLoginButtonEnable),
                new object[] { value });
        }

        private void LoginButtonText(String value)
        {
            btnLogin.Dispatcher.Invoke(
                new UpdateTestCallback(setLoginButtonText),
                new object[] { value });
        }

        private void LoginVisible(bool value)
        {
            SteamGuardGrid.Dispatcher.Invoke(
                new GuradVisibleCallback(isLoginVisible),
                new object[] { value });
        }

        private void SteamGuardvisible(bool value)
        {
            LoginOverlay.Dispatcher.Invoke(
                new GuradVisibleCallback(isGuardEnable),
                new object[] { value });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            user = txtUsername.Text;
            pass = txtPassword.Password;

          

            if (isConnected)
            {
                if (txtUsername.Text.Length > 0 && txtPassword.Password.Length > 0)
                {
                    //Detta är random data som skickas av steam som senare används för att identifiera datorn.
                    //Om denna fil inte finns, så antar steam att det är en ny enhet du loggat in på.
                    byte[] sentryHash = null;
                    string guard = null;
                    if (txtSteamGuard.Text.Length > 1)
                    {
                        guard = txtSteamGuard.Text;
                    }
                    if (File.Exists(user + ".bin"))
                    {
                        // if we have a saved sentry file, read and sha-1 hash it
                        byte[] sentryFile = File.ReadAllBytes(user + ".bin");
                        sentryHash = CryptoHelper.SHAHash(sentryFile);
                    }



                    steamUser.LogOn(new SteamUser.LogOnDetails
                    {
                        Username = txtUsername.Text,
                        Password = txtPassword.Password,
                        AuthCode = guard,
                        SentryFileHash = sentryHash,
                    });
                }


            
            }
            else
            {
                Console.WriteLine("Not connected");
            }
    

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;

            this.WindowState = System.Windows.WindowState.Minimized;
            this.ShowInTaskbar = false;


            steamUser.LogOff();
        }
#region callbacks

        private void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                if (callback.Result == EResult.AccountLogonDenied)
                {
                    // if we recieve AccountLogonDenied or one of it's flavors (AccountLogonDeniedNoMailSent, etc)
                    // then the account we're logging into is SteamGuard protected
                    // see sample 6 for how SteamGuard can be handled

                    Console.WriteLine("Unable to logon to Steam: This account is SteamGuard protected.");

                    SteamGuardvisible(true);
                    isGuardNeeded = true;
                    // isRunning = false;


                    return;
                }

                Console.WriteLine("Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult);

                isRunning = false;
                return;
            }
            Console.WriteLine("Successfully logged on!");



            LoginVisible(false);
            isGuardNeeded = false;

        }

        private void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            Console.WriteLine("Logged off of Steam: {0}", callback.Result);
            isLoggedOut = true;
            LoginVisible(true);
            isGuardNeeded = false;
            steamClient.Disconnect();
        }



        static void OnMachineAuth(SteamUser.UpdateMachineAuthCallback callback)
        {
            Console.WriteLine("Updating sentryfile...");

            byte[] sentryHash = CryptoHelper.SHAHash(callback.Data);
            

            // write out our sentry file
            // ideally we'd want to write to the filename specified in the callback
            // but then this sample would require more code to find the correct sentry file to read during logon
            // for the sake of simplicity, we'll just use "sentry.bin"
            File.WriteAllBytes(user + ".bin", callback.Data);

            // inform the steam servers that we're accepting this sentry file
            steamUser.SendMachineAuthResponse(new SteamUser.MachineAuthDetails
            {
                JobID = callback.JobID,
                FileName = callback.FileName,
                BytesWritten = callback.BytesToWrite,
                FileSize = callback.Data.Length,
                Offset = callback.Offset,
                Result = EResult.OK,
                LastError = 0,
                OneTimePassword = callback.OneTimePassword,
                SentryFileHash = sentryHash,
            });

            Console.WriteLine("Done!");
        }

        private void OnProfileInfo(SteamFriends.ProfileInfoCallback callback)
        {
            Console.WriteLine("ID: " + callback.SteamID.Render());
            Console.WriteLine("id: " + reportThisID);
            Console.WriteLine("Name: " + callback.RealName);


            if (callback.SteamID.Render().Equals(reportThisID))
            {
                Console.WriteLine("ID belongs to: " + callback.RealName);
            }

        }

        public static string CreateAvatarURL(byte[] input)
        {
            String hash = input.Aggregate(new StringBuilder(),
                       (sb, v) => sb.Append(v.ToString("x2"))
                      ).ToString();

            String baseURL = "http://cdn.akamai.steamstatic.com/steamcommunity/public/images/avatars/";

            String extendedUrL = hash.Substring(0,2) + "/" + hash + "_full.jpg";
            if (hash.Substring(0, 6) == "000000")
            {
                return "http://cdn.akamai.steamstatic.com/steamcommunity/public/images/avatars/fe/fef49e7fa7e1997310d705b2a6158ff8dc1cdfeb_full.jpg";
            }
            return baseURL + extendedUrL; ;
        }


        private void OnPersonaState(SteamFriends.PersonaStateCallback callback)
        {
            //Look at all the found scammers
            foreach (String s in foundScammersID)
            {
                //If we get a callback for that scammer
                if (s == callback.FriendID.Render())
                {
                    
                    //Check if we already added it to the list
                    bool exists = false;
                    foreach (Scammer sc in scammersInFriends)
                    {
                        if (sc.SteamID.Equals(s))
                        {
                            exists = true;
                            return;
                        }
                    }
                    //If not, then add it
                    if (!exists)
                    {
                        Scammer scam = new Scammer();
                        scam.SteamID = s;
                        scam.Name = callback.Name;
                        scam.AvatarURL = CreateAvatarURL(callback.AvatarHash);
                        scammersInFriends.Add(scam);
                    }
                }
            }

            if (scamWindow != null && scamWindow.IsVisible)
            {
                scamWindow.OnPersonaState(callback);
            }

            if (reportWindow != null && reportWindow.IsVisible)
            {
                reportWindow.OnPersonaState(callback);
            }

            if (foundScammersID.Count == scammersInFriends.Count && foundScammersID.Count > 0)
            {
                lbScammers.Dispatcher.Invoke(new UpdateScammersList(setScammersList), new object[] { scammersInFriends });
            }

            if (callback.FriendID.Render().Equals(steamUser.SteamID.Render()))
            {
                myNickeName = callback.Name;
            }

            if (callback.FriendID.Render().Equals(reportThisID))
            {
              //  Console.WriteLine("ID belongs to: " + callback.Name);

                lblIDName.Dispatcher.Invoke(
            new UpdateTestCallback(setScammerIDName),
            new object[] { callback.Name });
            }
        }

        

        static void OnFriendAdded(SteamFriends.FriendAddedCallback callback)
        {

            // someone accepted our friend request, or we accepted one
            Console.WriteLine("{0} is now a friend", callback.PersonaName);
        }

        private void OnFriendsList(SteamFriends.FriendsListCallback callback)
        {
            //int friendCount = steamFriends.GetFriendCount();
            ScammerUpdater.Start();
            friendList = callback.FriendList;
        }

        static void OnAccountInfo(SteamUser.AccountInfoCallback callback)
        {
            steamFriends.SetPersonaState(EPersonaState.Online);
        }

        private void OnConnected(SteamClient.ConnectedCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                Console.WriteLine("Unable to connect to Steam: {0}", callback.Result);
                LoginButtonEnableState(false);
                LoginButtonText("Connecting..");
                isRunning = false;
                return;
            }

            isConnected = true;
            Console.WriteLine("Connected to Steam!");
            LoginButtonEnableState(true);
            LoginButtonText("Sign in");

        }


        private void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            Console.WriteLine("Disconnected from Steam");
            isConnected = false;
            isRunning = false;
            LoginButtonEnableState(false);
            LoginButtonText("Connecting..");
                reconnect();
            
        }

#endregion

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (reportWindow == null || !reportWindow.IsVisible)
            {
                reportWindow = new ReportWindow();
                reportWindow.Show();
                

            }
            reportWindow.setMainWindow(this);
            reportWindow.setSteamFriend(steamFriends);
          //  ReportGrid.Visibility = System.Windows.Visibility.Visible;
            ScammersListGrid.Visibility = System.Windows.Visibility.Collapsed;
            //txtSteamID.Focus();
        }

        private void CloseMenuItem_Click(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {


                icon.Visible = false;
                this.Close();
                System.Windows.Application.Current.Shutdown();
                System.Environment.Exit(0);
            }

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            steamUser.LogOff();
            isLoggedOut = true;
            isGuardNeeded = false;
            LoginVisible(true);
            SteamGuardvisible(isGuardNeeded);
            txtPassword.Password = "";
            txtUsername.Text = "";
            txtSteamGuard.Text = "";
            txtUsername.Focus();
        }

        private void txtSteamID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

            BindingExpression expression = txtSteamID.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty);
            expression.UpdateSource();
            btnReport.IsEnabled = IsValid(txtSteamID.Text);


                if (e.Key == Key.Enter)
                {

                        if (IsValid(txtSteamID.Text))
                        {
                            SteamID id = new SteamID();
                            id.SetFromString(txtSteamID.Text, EUniverse.Public);
                            steamFriends.RequestFriendInfo(id);
                            //steamFriends.RequestProfileInfo(id);
                            reportThisID = txtSteamID.Text;

                            if (IsSteamIDReported(reportThisID))
                            {
                                lblIsReported.Content = "This user is reported";
                            }
                        }
                }
            
        }

        private bool IsValid(string value)
            {
                try
                {
                    if (((String)value).Length == 18)
                    {
                        if (((String)value).Substring(0, 6).ToUpper().Equals("STEAM_"))
                        {

                            if (((String)value).Split(':').Length == 3)
                            {

                                try
                                {
                                    int firstNumber = int.Parse(((String)value).Split(':')[0].Substring(6), NumberStyles.Any);

                                    int numberTwo = int.Parse(((String)value).Split(':')[1], NumberStyles.Any);

                                    int longID = int.Parse(((String)value).Split(':')[2], NumberStyles.Any);
                                    return true;
                                }
                                catch (Exception e) { Console.WriteLine(e.Message); Console.WriteLine(e.StackTrace); return false; }



                            }
                            else { return false; }


                        }
                        else { return false; }
                    }
                    else { return false; }
                }
                catch (Exception e1)
                {
                    Console.WriteLine(e1.StackTrace);
                    return false;
                }
            }


        private void btnReport_Click(object sender, RoutedEventArgs e)
        {
            ReportGrid.Visibility = System.Windows.Visibility.Collapsed;
            Scammer s = new Scammer();
            s.SteamID = txtSteamID.Text;
            bool exists = false;
            List<Scammer> scammers = sql.getAllScammers();
            Scammer foundScammer = null;
            foreach (Scammer scam in scammers)
            {
                if (scam.SteamID.Equals(s.SteamID))
                {
                    foundScammer = scam;
                    exists = true;
                }
            }
            //We don't want duplicates. Not sure what that would do to the system :P
            if (!exists)
            {
                sql.addScammer(s);
            }
            else
            {
            }

            txtSteamID.Text = "";
            lblIDName.Content = "";
            btnReport.IsEnabled = false;
        }

        private void txtSteamID_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void txtSteamID_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            BindingExpression expression = txtSteamID.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty);
            expression.UpdateSource();
            btnReport.IsEnabled = IsValid(txtSteamID.Text);

            if (IsValid(txtSteamID.Text))
            {
                SteamID id = new SteamID();
                id.SetFromString(txtSteamID.Text, EUniverse.Public);
                steamFriends.RequestFriendInfo(id);
                //steamFriends.RequestProfileInfo(id);
                reportThisID = txtSteamID.Text;

                if (IsSteamIDReported(reportThisID))
                {
                    lblIsReported.Content = "This user is reported";
                }

            }
        }

        private void txtSteamID_LayoutUpdated(object sender, EventArgs e)
        {
            BindingExpression expression = txtSteamID.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty);
            expression.UpdateSource();
            btnReport.IsEnabled = IsValid(txtSteamID.Text);

            if (IsValid(txtSteamID.Text))
            {
                SteamID id = new SteamID();
                id.SetFromString(txtSteamID.Text, EUniverse.Public);
                steamFriends.RequestFriendInfo(id);
                //steamFriends.RequestProfileInfo(id);
                reportThisID = txtSteamID.Text;

                if (IsSteamIDReported(reportThisID))
                {
                    lblIsReported.Content= "This user is reported";
                }
            }
        }

        public bool IsSteamIDReported(string reportThisID)
        {
            //Is SteamID reported?
            bool exists = false;
            List<Scammer> scammers = sql.getAllScammers();
            Scammer foundScammer = null;
            foreach (Scammer scam in scammers)
            {
                if (scam.SteamID.Equals(reportThisID))
                {
                    foundScammer = scam;
                    exists = true;
                }
            }

            return exists;
        }

        private void btnCloseReport_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ReportGrid.Visibility = System.Windows.Visibility.Collapsed;

            txtSteamID.Text = "";
            lblIDName.Content = "";
            btnReport.IsEnabled = false;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {

            ScammersListGrid.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void Label_MouseUp(object sender, MouseButtonEventArgs e)
        {
          //  ScammersListGrid.Visibility = System.Windows.Visibility.Visible;
            List<Scammer> scammers = sql.getAllScammers();
            foreach (Scammer s in scammers) {

                foreach (Scammer scam in scammersInFriends)
                {
                    if (s.SteamID.Equals(scam.SteamID))
                    {
                        scam.ID = s.ID;
                    }
                }

            }
            if (scamWindow == null || !scamWindow.IsVisible)
            {
                scamWindow = new ScammerWindow();
                
                scamWindow.Show();
                scamWindow.setMainWindow(this);
                scamWindow.ScammersInYourList(scammersInFriends);
            }

        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width;
            this.Top = desktopWorkingArea.Bottom - this.Height;
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (reportWindow == null || !reportWindow.IsVisible)
            {
                reportWindow = new ReportWindow();
                reportWindow.Show();

            }
                        reportWindow.setMainWindow(this);
            reportWindow.setSteamFriend(steamFriends);
          //  ReportGrid.Visibility = System.Windows.Visibility.Visible;
            ScammersListGrid.Visibility = System.Windows.Visibility.Collapsed;
           // txtSteamID.Focus();
        }

        private void lblClose_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
            this.ShowInTaskbar = false;


            steamUser.LogOff();
        }

        private void Binding_SourceUpdated(object sender, DataTransferEventArgs e)
        {

        }

    }
}
