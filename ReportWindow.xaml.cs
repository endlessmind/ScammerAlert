using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.IO;
using ScammerAlert.connection;
using SteamKit2;

namespace ScammerAlert
{
    /// <summary>
    /// Interaction logic for ReportWindow.xaml
    /// </summary>
    public partial class ReportWindow : Window
    {
        private List<file> files = new List<file>();
        MainWindow main;
        static string reportThisID;
        static SteamFriends steamFriends;


        public delegate void UpdateTestCallback(String value);

        public ReportWindow()
        {
            InitializeComponent();
        }

        private void lblClose_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        public void setMainWindow(MainWindow window)
        {
            main = window;
        }


        public void setSteamFriend(SteamFriends f)
        {
            steamFriends = f;
        }

        private void SteamNameText(String value)
        {
            lblSteamName.Dispatcher.Invoke(
                new UpdateTestCallback(setSteamName),
                new object[] { value });
        }

        private void setSteamName(String value)
        {
            lblSteamName.Content = value;
        }

        public void OnPersonaState(SteamFriends.PersonaStateCallback callback)
        {
            if (callback.FriendID.Render().Equals(reportThisID))
            {
                SteamNameText(callback.Name);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - (this.Width + 221);
            this.Top = desktopWorkingArea.Bottom - this.Height;

            
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

        private void btnAddImage_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".jpg";
            dlg.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                FileInfo info = new FileInfo(dlg.FileName);
                if (info.Length > (1024 * 1024 * 10))
                {
                    MessageBox.Show("Please only upload files under 10Mb.");
                    return;
                }
                String uid = System.Guid.NewGuid().ToString();
                file f = new file();
                String FilePath = dlg.FileName;
                String extention = FilePath.Substring(FilePath.Length - 3, 3);
                String FileName = FilePath.Substring(FilePath.LastIndexOf("\\") + 1);
                FileName = FileName.Substring(0, FileName.Length - 4);

                f.Name = FileName;
                f.Path = FilePath;
                f.Hash = uid + "." + extention;
                f.Extension = extention;

                files.Add(f);

                lbAttachment.Items.Add(f);
                
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtMotivation.Text.Length > 0 && IsValid(txtSteamID.Text))
            {
                btnSend.IsEnabled = true;
            }
            else
            {
                btnSend.IsEnabled = false;
            }
        }

        private void txtSteamID_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtMotivation.Text.Length > 0 && IsValid(txtSteamID.Text))
            {
                btnSend.IsEnabled = true;
            }
            else
            {
                btnSend.IsEnabled = false;
            }
        }

        private void txtSteamID_KeyDown(object sender, KeyEventArgs e)
        {
            BindingExpression expression = txtSteamID.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty);
            expression.UpdateSource();
            btnSend.IsEnabled = IsValid(txtSteamID.Text);

            if (e.Key == Key.Enter)
            {

                if (IsValid(txtSteamID.Text))
                {
                    SteamID id = new SteamID();
                    id.SetFromString(txtSteamID.Text, EUniverse.Public);

                    steamFriends.RequestFriendInfo(id);
                    ////steamFriends.RequestProfileInfo(id);
                    reportThisID = txtSteamID.Text;


                }
            }

        }
    }
}
