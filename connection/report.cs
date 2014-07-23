using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ScammerAlert.connection
{
    public class report : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private int mID;
        private String mSteamID;
        private String mName;
        private String mAvatarURL;
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
                if (value != mName)
                {
                    mName = value;
                    OnPropertyChanged("Name");
                }
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
                if (value != mAvatarURL)
                {
                    mAvatarURL = value;
                    OnPropertyChanged("AvatarURL");
                }

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
