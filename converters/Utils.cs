using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScammerAlert.converters
{
   public static class Utils
    {

        public static string GetSteamID(Int64 communityID)
        {
            communityID = communityID - 76561197960265728;
            Int64 authServer = communityID % 2;
            communityID = communityID - authServer;
            Int64 authID = communityID / 2;
            return string.Format("STEAM_0:{0}:{1}", authServer, authID);
        }

        public static string GetCommunityID(string steamID)
        {
            Int64 authServer = Convert.ToInt64(steamID.Substring(8, 1));
            Int64 authID = Convert.ToInt64(steamID.Substring(10));
            return (76561197960265728 + (authID * 2) + authServer).ToString();
        }

    }
}
