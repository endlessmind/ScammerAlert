using System;
using System.Collections.Generic;
using System.Text;

namespace ScammerAlert.connection
{
    class Scammer
    {

        private int mID;
        private String mSteamID;
        private String mName;
        private int mReported;
        private String mAvatarURL;

        public int ID
        {
            get
            {
                return mID;
            }

            set
            {
                mID = value;
            }
        }

        public String SteamID
        {
            get
            {
                return mSteamID;
            }

            set
            {
                mSteamID = value;
            }
        }

        public String Name
        {
            get
            {
                return mName;
            }

            set
            {
                mName = value;
            }
        }

        public int Reported
        {
            get
            {
                return mReported;
            }

            set
            {
                mReported = value;
            }
        }

        public String AvatarURL
        {
            get
            {
                return mAvatarURL;
            }

            set
            {
                mAvatarURL = value;
            }
        }


    }
}
