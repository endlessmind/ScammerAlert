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
using System.Threading;
using System.Net;

namespace ScammerAlert
{
    /// <summary>
    /// Interaction logic for ReportWindow.xaml
    /// </summary>
    public partial class ReportWindow : Window
    {
        private List<file> files = new List<file>();
        MainWindow main;
        static string reportThisID, motivation;
        static SteamFriends steamFriends;
        Thread SenderThread;
        private MySQL sql;


        public delegate void UpdateTestCallback(String value);
        
        private delegate void CloseWindow();

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
            sql = main.getMySQLInstance();
        }


        public void setSteamFriend(SteamFriends f)
        {
            steamFriends = f;
        }

        private void CloseThis()
        {
            this.Close();
        }

        private void SteamNameText(String value)
        {
            lblSteamName.Dispatcher.Invoke(
                new UpdateTestCallback(setSteamName),
                new object[] { value });
        }

        private void CloseThisWindow()
        {
            this.Dispatcher.Invoke(new CloseWindow(CloseThis), null);
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

        private void runSender()
        {
            //Check if the SteamID is already reported
            Scammer s = new Scammer();
            s.SteamID = reportThisID;
            s.Reported = 1;
            bool exists = false;
            int scammerID;
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

            //If i don't exists, we'll add it. But if i dose, then we just need the ID
            if (!exists)
            {
                sql.addScammer(s);
                scammerID = sql.getScammer(s.SteamID).ID;
            }
            else
            {
                scammerID = foundScammer.ID;
            }


            //Now we need to create the report
            report r = new report();
            r.Comment = motivation;
            r.Name = main.getMyNickName();
            r.ScammerID = scammerID;
            r.SteamID = main.getMySteamID();
            r.Time = DateTime.Now;

            sql.addReport(r);

            //Now we need the report's id. It's needed for us to be able to link the attachments to the correct report
            int reportID = sql.getReports(r.ScammerID, r.SteamID)[0].ID;

            string AttachmentFolder = AppDomain.CurrentDomain.BaseDirectory + "\\attachment\\";
            foreach (file f in files)
            {
                WebClient client = new WebClient();
                client.Credentials = CredentialCache.DefaultCredentials;
                client.UploadFile(@"http://" + "89.160.119.29" + "/scammers/upload_attachment.php", "POST", AttachmentFolder + f.Hash);
                attachment a = new attachment();
                a.Filename = f.Hash;
                a.ReportID = reportID;
                sql.addAttachment(a);
            }
            CloseThisWindow();

            foreach (file f in files)
            {
                File.Delete(AttachmentFolder + f.Hash);
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

        private void btnAddImage_Click(object sender, RoutedEventArgs e)
        {
            string AttachmentFolder = AppDomain.CurrentDomain.BaseDirectory + "\\attachment\\";
            if (!Directory.Exists(AttachmentFolder))
            {
                DirectoryInfo d = Directory.CreateDirectory(AttachmentFolder);
            }




            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            System.Drawing.Imaging.ImageCodecInfo[] codecs = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();
            String sep = String.Empty;
            dlg.Filter = "";
            foreach (System.Drawing.Imaging.ImageCodecInfo c in codecs)
            {
                string CodecName = c.CodecName.Substring(8).Replace("Codec", "Files").Trim();
                dlg.Filter = String.Format("{0}{1}{2} ({3})|{3}", dlg.Filter, sep, CodecName, c.FilenameExtension);
                sep = "|";
            }
            dlg.FilterIndex = 2;
            
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
                String FileName = FilePath.Substring(FilePath.LastIndexOf("\\") + 1);
                String extention = FileName.Split('.')[FileName.Split('.').Length - 1];
                FileName = FileName.Substring(0, FileName.Length - 4);

                f.Name = FileName;
                f.Path = FilePath;
                f.Hash = uid + "." + extention;
                f.Extension = extention;
                info.CopyTo(AttachmentFolder + f.Hash);
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

        private void lbAttachment_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lbAttachment.SelectedIndex > -1)
            {
                file f = (file)lbAttachment.SelectedItem;
                if (MessageBox.Show("Do you want to remove " + f.Name + "." + f.Extension + " from the list?", "Remove?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    string AttachmentFolder = AppDomain.CurrentDomain.BaseDirectory + "\\attachment\\";
                    files.Remove(f);
                    lbAttachment.Items.Remove(f);
                    File.Delete(AttachmentFolder + f.Hash);
                }
            }
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            motivation = txtMotivation.Text;
            reportThisID = txtSteamID.Text;
            SenderThread = new Thread(new ThreadStart(runSender));
            SenderThread.Start();
            btnSend.IsEnabled = false;

        }
    }
}
