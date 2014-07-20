using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScammerAlert.connection
{
    class file
    {
        private String mName;
        private String mPath;
        private String mHash;
        private String mExtension;

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

        public String Path
        {
            get
            {
                return mPath;
            }

            set
            {
                mPath = value;
            }
        }

        public String Hash
        {
            get
            {
                return mHash;
            }

            set
            {
                mHash = value;
            }
        }

        public String Extension
        {
            get
            {
                return mExtension;
            }

            set
            {
                mExtension = value;
            }
        }

    }
}
