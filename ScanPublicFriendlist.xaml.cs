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
using SteamKit2;
using ScammerAlert.connection;
using System.Threading;
using System.Globalization;
using ScammerAlert.converters;

namespace ScammerAlert
{
    /// <summary>
    /// Interaction logic for ScanPublicFriendlist.xaml
    /// </summary>
    public partial class ScanPublicFriendlist : Window
    {
        public delegate void UpdateTextCallback(String value);
        public delegate void getTextCallback();

        ScammerWindow scamWindow;
        List<Scammer> scammersInFriends;
        private MySQL sql;
        MainWindow main;
        Brush RedBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF781E1E"));
        Brush GreenBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF55B44C"));

        public ScanPublicFriendlist()
        {
            InitializeComponent();

           
        }

        public void setMainWindow(MainWindow window)
        {
            main = window;
            sql = main.getMySQLInstance();
        }

        private void UpdateLableText(Label lbl, String value)
        {
       

            lbl.Dispatcher.Invoke(
                new UpdateTextCallback( delegate(string s)  { lbl.Content = s; }),
                new object[] { value });
        }

        private void UpdateLableForeground(Label lbl, System.Windows.Media.Brush b)
        {
            lbl.Dispatcher.Invoke(new getTextCallback(delegate() { lbl.Foreground = b; }),
               null);
        }

        public String GetTextBoxText(TextBox lbl)
        {
            String text = "";
            lbl.Dispatcher.Invoke(new getTextCallback( delegate() { text = lbl.Text.ToString(); }),
                null);
           return text;
        }

        public void ChangeEnable(object o, Boolean state)
        {
            ((Control)o).Dispatcher.Invoke(new getTextCallback(delegate() { ((Control)o).IsEnabled = state; }),
                null);
        }



        private void checkPublicFriendList(string ID)
        {
            Thread t1 = new Thread(new ThreadStart(delegate
            {


         


                ChangeEnable(btnShowScammers, false);
                using (WebAPI.Interface steamFriedList = WebAPI.GetInterface("ISteamUser", "9DF293619722CA60815A3354C19DAB4F"))
                {
                    Dictionary<string, string> MyArgs = new Dictionary<string, string>();
                    MyArgs["steamids"] = "[" + ID + "]";
                    KeyValue MyResult = steamFriedList.Call("GetPlayerSummaries", 2, MyArgs);

                    String baseName = MyResult.Children[0].Children[0]["personaname"].Value;
                    Console.WriteLine(baseName.Substring(baseName.Length - 1));
                    String FormatedName = baseName.Substring(baseName.Length - 1).Equals("s") ? baseName : baseName + "'s";
                    UpdateLableText(lblSteamName, "Checking " + MyResult.Children[0].Children[0]["personaname"].Value + "'s friends");

                    Dictionary<string, string> friendArgs = new Dictionary<string, string>();
                    friendArgs["steamid"] = ID;
                    friendArgs["relationship"] = "friend";
                    KeyValue result = steamFriedList.Call("GetFriendList", 1, friendArgs);
                    Console.WriteLine(result.Children[0].Children.Count);
                    //    lblFriendCount.Content = result.Children[0].Children.Count;
                    UpdateLableText(lblFriendCount, result.Children[0].Children.Count + "");
                    List<Scammer> scammers = sql.getAllScammers();
                    scammersInFriends = new List<Scammer>();
                    int check = 0;
                    int sus = 0;
                    //Reset all labels
                    UpdateLableText(lblSuspiciousCount, sus + "");
                    UpdateLableText(lblCheckedCount, check + "");
                    UpdateLableText(lblStatus, sus > 1 ? "Unsafe" : "Safe");
                    UpdateLableForeground(lblStatus, sus > 1 ? RedBrush : GreenBrush);

                    foreach (KeyValue kv in result.Children[0].Children)
                    {
                        check++;
                        UpdateLableText(lblCheckedCount, check + "");
                        Scammer sc = new Scammer();

                        Dictionary<string, string> profileArgs = new Dictionary<string, string>();
                        profileArgs["steamids"] = "[" + kv["steamid"].Value + "]";

                        KeyValue profileResult = steamFriedList.Call("GetPlayerSummaries", 2, profileArgs);

                        sc.Name = profileResult.Children[0].Children[0]["personaname"].Value;
                        sc.SteamID = Utils.GetSteamID((Int64)kv["steamid"].AsLong());
                        sc.AvatarURL = profileResult.Children[0].Children[0]["avatarfull"].Value;
                        
                        
                        foreach (Scammer scam in scammers)
                        {
                            if (scam.SteamID.Equals(sc.SteamID))
                            {
                                sc.ID = scam.ID;
                                sc.Reported = scam.Reported;
                                scammersInFriends.Add(sc);
                                sus++;
                                UpdateLableText(lblStatus, sus > 1 ? "Unsafe" : "Safe");
                                UpdateLableForeground(lblStatus, sus > 1 ? RedBrush : GreenBrush);
                                UpdateLableText(lblSuspiciousCount, sus + "");

                            }
                        }

                        


                    }
                    ChangeEnable(txtSteamID, true);
                    ChangeEnable(btnCheck, true);
                    ChangeEnable(btnShowScammers,sus > 1 ? true : false);

                }

            }));
            t1.Start();
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
                else {
                    if (((String)value).Length == 17)
                    {
                        try
                        {
                            Int64.Parse(((String)value), NumberStyles.None);
                            return true;
                        }
                        catch { return false; }
                    }
                    return false;
                }
            }
            catch (Exception e1)
            {
                Console.WriteLine(e1.StackTrace);
                return false;
            }
        }

        private bool is64bitID(String value)
        {
            if (((String)value).Length == 17)
            {
                try
                {
                    Int64.Parse(((String)value), NumberStyles.None);
                    return true;
                }
                catch { return false; }
            }
            return false;
        }

        public string getName(String id)
        {
            if (!Utils.is64bitID(id))
            {
                id = Utils.GetCommunityID(id);
            }

            using (WebAPI.Interface steamFriedList = WebAPI.GetInterface("ISteamUser", "9DF293619722CA60815A3354C19DAB4F"))
            {
                try
                {
                    Dictionary<string, string> MyArgs = new Dictionary<string, string>();
                    MyArgs["steamids"] = "[" + id + "]";
                    KeyValue MyResult = steamFriedList.Call("GetPlayerSummaries", 2, MyArgs);

                    return MyResult.Children[0].Children[0]["personaname"].Value;
                }
                catch (Exception e) { return null; }
            }
        }

        private void ValidateInput(string id)
        {
            Thread t1 = new Thread(new ThreadStart(delegate
            {
                string result = getName(id);
                if (result != null)
                {
                    UpdateLableText(lblSteamName, result);
                    ChangeEnable(btnCheck, true);
                }
                else { ChangeEnable(btnCheck, false); UpdateLableText(lblSteamName, "INVALID"); }


            }));
            t1.Start();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            

            if (IsValid(txtSteamID.Text))
            {
                String ID = "";
                if (is64bitID(txtSteamID.Text))
                {
                    ID = txtSteamID.Text;
                }
                else
                {
                    ID = Utils.GetCommunityID(txtSteamID.Text);
                }
                btnCheck.IsEnabled = false;
                txtSteamID.IsEnabled = false;
                checkPublicFriendList(ID);
            }


        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - (this.Width + 221);
            this.Top = desktopWorkingArea.Bottom - this.Height;
            UpdateLableText(lblFriendCount,"0");
            UpdateLableText(lblCheckedCount,  "0");
            UpdateLableText(lblSuspiciousCount, "0");
        }

        private void lblClose_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void txtSteamID_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtSteamID.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            if (IsValid(txtSteamID.Text))
            {
                ValidateInput(txtSteamID.Text);
            }
        }

        private void txtSteamID_KeyDown(object sender, KeyEventArgs e)
        {
            txtSteamID.GetBindingExpression(TextBox.TextProperty).UpdateSource();

            if (IsValid(txtSteamID.Text))
            {
                String ID = "";
                if (is64bitID(txtSteamID.Text))
                {
                    ID = txtSteamID.Text;
                }
                else
                {
                    ID = Utils.GetCommunityID(txtSteamID.Text);
                }
                btnCheck.IsEnabled = false;
                txtSteamID.IsEnabled = false;
                checkPublicFriendList(ID);
            }

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnShowScammers_Click(object sender, RoutedEventArgs e)
        {
            if (scamWindow == null || !scamWindow.IsVisible)
            {
                scamWindow = new ScammerWindow();

                scamWindow.Show();
                scamWindow.setMainWindow(main);
                scamWindow.ScammersInYourList(scammersInFriends);
            }
        }
    }
}
