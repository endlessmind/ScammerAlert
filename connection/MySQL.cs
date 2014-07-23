using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using MySql.Data.MySqlClient;

namespace ScammerAlert.connection
{
    public class MySQL
    {
        string MyConString = "";
        public MySqlConnection connection;
        private MySqlCommand command;
        private MySqlDataReader reader;

        public MySQL(String connectionQuery)
        {
            MyConString = connectionQuery;
            OpenDB();
            command = connection.CreateCommand();
        }

        public void OpenConnection()
        {
            OpenDB();
        }

        private void CheckConnection()
        {
            if (connection == null ||connection.State != System.Data.ConnectionState.Open)
            {
                OpenDB();
                command = connection.CreateCommand();
            }
        }

        private void OpenDB()
        {
            Boolean bRetry = false;
            int iRetryCount = 0, iMaxRetry = 10;

            do
            {
                try
                {
                    connection = new MySqlConnection(MyConString);
                    connection.Open();
                    bRetry = false;
                }
                catch (MySqlException e)
                {
  
                    switch (e.Number)
                    {
                        case 0:
                            if (iRetryCount <= iMaxRetry)
                            {
                                bRetry = true;
                                iRetryCount++;
                            }
                            else
                            {
                                System.Windows.MessageBox.Show("Error connecting to scammer-database");
                            }
                            break;

                        default:
                            System.Windows.MessageBox.Show("Error connecting to scammer-database");
                            break;

                    }

                }

            } while (bRetry && iRetryCount < iMaxRetry);

        }

        public List<Scammer> getAllScammers()
        {
            CheckConnection();
            List<Scammer> scam = new List<Scammer>();

            command.CommandText = "SELECT * FROM scammers";
            reader = command.ExecuteReader();

            while (reader.Read())
            {
                Scammer sc = new Scammer();
                sc.ID = reader.GetInt16(0);
                sc.SteamID = reader.GetString(1);

                scam.Add(sc);
            }

            reader.Close();
            connection.Close();
            return scam;

        }

        public Scammer getScammer(String SteamID)
        {
            CheckConnection();
            command.CommandText = "SELECT * FROM scammers WHERE steam_id='" + SteamID + "'";
            reader = command.ExecuteReader();
            reader.Read();
            Scammer sc = new Scammer();
            sc.ID = reader.GetInt16(0);
            sc.SteamID = reader.GetString(1);

            reader.Close();
            connection.Close();
            return sc;
        }

        public void addScammer(Scammer s)
        {
            CheckConnection();
            String query = "INSERT INTO scammers(steam_id)VALUES('" + s.SteamID + "')";
            command = new MySqlCommand(query, connection);
            command.ExecuteNonQuery();
            connection.Close();
        }


        public ObservableCollection<report> getReports(int scammerID, string steamID = null)
        {
            CheckConnection();
            ObservableCollection<report> reports = new ObservableCollection<report>();

            if (steamID == null)
            {
                command.CommandText = "SELECT * FROM reports WHERE scammerID='" + scammerID + "'";
            }
            else
            {
                command.CommandText = "SELECT * FROM reports WHERE scammerID='" + scammerID + "' AND steamID='" + steamID + "'";
            }
            reader = command.ExecuteReader();

            while (reader.Read())
            {
                
                report r = new report();
                r.ID = reader.GetInt16(0);
                r.SteamID = reader.GetString(1);
                r.Name = reader.GetString(2);
                r.ScammerID = reader.GetInt16(3);
                r.Comment = reader.GetString(4);
                r.Time = DateTime.Parse(reader.GetDateTime(5).ToLongDateString());
                reports.Add(r);
            }
            
            reader.Close();
            connection.Close();
            
            return reports;

        }

        public void addReport(report report)
        {
            CheckConnection();
            String query = "INSERT INTO reports(steamID, nickname, scammerID, comment, time)VALUE('" + report.SteamID + "','" + report.Name + "','" + report.ScammerID + "','" + report.Comment + "','" + report.Time.ToString("yyyy-MM-dd H:mm:ss") + "')";
            command = new MySqlCommand(query, connection);
            command.ExecuteNonQuery();
            connection.Close();
        }

        public List<attachment> getAttachment(int reportID, bool closeConnection)
        {
            CheckConnection();
            List<attachment> attachments = new List<attachment>();
            command.CommandText = "SELECT * FROM attachment WHERE ReportID='" + reportID + "'";
            reader = command.ExecuteReader();

            while (reader.Read())
            {
                attachment at = new attachment();
                at.ID = reader.GetInt16(0);
                at.Filename = reader.GetString(1);
                at.ReportID = reader.GetInt16(2);
                attachments.Add(at);
            }
            reader.Close();
            if (closeConnection)
            {
                connection.Close();
            }
            return attachments;
        }


        public void addAttachment(attachment attach)
        {
            CheckConnection();
            String query = "INSERT INTO attachment(filename, reportID)VALUE('" + attach.Filename + "','" + attach.ReportID + "')";
            command = new MySqlCommand(query, connection);
            command.ExecuteNonQuery();
            connection.Close();
        }

    }
}
