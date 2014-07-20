using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace ScammerAlert.converters
{


    public class SteamIDValidation : ValidationRule
    {
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
                                return new ValidationResult(true, null);
                            }
                            catch (Exception e) { Console.WriteLine(e.Message); Console.WriteLine(e.StackTrace); return new ValidationResult(false, "Inport not in correct format"); }



                        } else { return new ValidationResult(false, "Inport not in correct format"); }


                    } else { return new ValidationResult(false, "Inport not in correct format"); }
                } else { return new ValidationResult(true, null); }
            }
            catch (Exception e1)
            {
                Console.WriteLine(e1.StackTrace);
                return new ValidationResult(false, "Input is not a correct format.");
            }

        }
    }
}
