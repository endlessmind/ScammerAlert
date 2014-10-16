using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using SteamKit2;
using ScammerAlert.converters;

namespace ScammerAlert.converters
{


    public class SteamIDValidation : ValidationRule
    {

        public string getAvatarAndName(String id)
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

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
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

                                string result = getAvatarAndName((String)value);
                                if (result != null)
                                {
                                    return new ValidationResult(true, null);
                                }
                                else
                                {
                                    return new ValidationResult(false, "SteamID not connected with any userprofile");
                                }

                            }
                            catch (Exception e) { Console.WriteLine(e.Message); Console.WriteLine(e.StackTrace); return new ValidationResult(false, "Input not in correct format"); }



                        }
                        else { return new ValidationResult(false, "Input not in correct format"); }


                    }
                    else { return new ValidationResult(false, "Input not in correct format"); }
                }
                else
                {
                    if (((String)value).Length == 17)
                    {
                        try
                        {
                            Int64.Parse(((String)value));
                            string result = getAvatarAndName((String)value);
                            if (result != null)
                            {
                                return new ValidationResult(true, null);
                            }
                            else
                            {
                                return new ValidationResult(false, "SteamID not connected with any userprofile");
                            }
                        }
                        catch { return new ValidationResult(false, "Input not in correct format"); }
                    }
                    return new ValidationResult(false, "Input not in correct format");
                }
            }
            catch (Exception e1)
            {
                Console.WriteLine(e1.StackTrace);
                return new ValidationResult(false, "Input not in correct format");
            }






        }
    }
}
