using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScammerAlert.connection
{
    class attachment
    {
        private int mID;
        private String mFilename;
        private int mReportID;

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

        public String Filename
        {
            get
            {
                return mFilename;
            }

            set
            {
                mFilename = value;
            }
        }

        public int ReportID
        {
            get
            {
                return mReportID;
            }

            set
            {
                mReportID = value;
            }
        }


    }
}
