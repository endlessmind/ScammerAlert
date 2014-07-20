using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScammerAlert.connection
{
    class report
    {
        private int mID;
        private String mSteamID;
        private String mName;
        private int mScammerID;
        private String mComment;
        private DateTime mTime;
        private List<attachment> mAttachments;

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

        public int ScammerID
        {
            get
            {
                return mScammerID;
            }

            set
            {
                mScammerID = value;
            }
        }

        public String Comment
        {
            get
            {
                return mComment;
            }

            set
            {
                mComment = value;
            }
        }

        public DateTime Time
        {
            get
            {
                return mTime;
            }

            set
            {
                mTime = value;
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

        public List<attachment> Attachment
        {
            get
            {
                return mAttachments;
            }

            set
            {
                mAttachments = value;
            }
        }


    }
}
