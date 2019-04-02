using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    public class TTInfo
    {
        public string ID;
        public int ID_idx;
        public string Name;
        public string Birthday;
        public int Birthday_idx;
        public string Birthplace;

        public TTInfo()
        {
            ID_idx = -1;
            Birthday_idx = -1;
            ID = "XX";
            Name = "No name";
            Birthday = "1900";
            Birthplace = "VN";
        }

        public void CleanUp()
        {
            CleanUp(ref Name);
            CleanUp(ref Birthday);
            CleanUp(ref Birthplace);
        }

        void CleanUp(ref string s)
        {
            s = s.Replace('\n', ' ');
            s = s.Replace('\t', ' ');
            s = s.Replace('|', ' ');
            s = s.Trim();
            while (-1 < s.IndexOf("  "))
                s = s.Replace("  ", " ");
        }

        override public string ToString()
        {
            return ID + "\t" + Name + "\t" + Birthday + "\t" + Birthplace;
        }
    }
}
