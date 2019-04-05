using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace MyLib
{
    public class PostOCRTextProcessor
    {
        public static StringBuilder ScanText(string s)
        {
            if (s.Length < 20)
                return new StringBuilder();
            s = FixOCRConfusing1L(s);

            // Define a regular expression for repeated words.
            Regex rx = new Regex("[AB][0-9]+");

            // Find matches.
            MatchCollection matches = rx.Matches(s);

            // Report on each match.
            TTInfo[] vInfo = new TTInfo[matches.Count];
            int i = 0;
            foreach (Match match in matches)
            {
                GroupCollection groups = match.Groups;
                vInfo[i] = new TTInfo();
                vInfo[i].ID_idx = groups[0].Index;
                vInfo[i].ID = groups[0].Value;
                ++i;
            }

            string[] residues = new string[vInfo.Length];
            int a;
            for (i = 0; i < vInfo.Length - 1; ++i)
            {
                a = vInfo[i].ID_idx + vInfo[i].ID.Length;
                residues[i] = s.Substring(a, vInfo[i + 1].ID_idx - 1 - a);
            }
            a = vInfo.Length - 1;
            residues[a] = s.Substring(vInfo[a].ID_idx + vInfo[a].ID.Length);

            i = 0;
            foreach (string t in residues)
            {
                SearchDate("[0-9]+-[0-9]+-[0-9]+", t, ref vInfo[i]);
                if(vInfo[i].Birthday_idx < 0)
                {
                    SearchDate("[0-9]+/[0-9]+/[0-9]+", t, ref vInfo[i]);
                    if (vInfo[i].Birthday_idx < 0)
                    {
                        SearchDate("[0-9]+-[0-9]+", t, ref vInfo[i]);
                        if (vInfo[i].Birthday_idx < 0)
                        {
                            SearchDate("[0-9]+/[0-9]+", t, ref vInfo[i]);
                            if (vInfo[i].Birthday_idx < 0)
                                SearchDate("[0-9]+", t, ref vInfo[i]);
                        }
                    }
                }
                ++i;
            }

            i = 0;
            foreach (string t in residues)
            {
                if(-1 < vInfo[i].Birthday_idx)
                {
                    vInfo[i].Name = t.Substring(0, vInfo[i].Birthday_idx);
                    int birthPlaceX = vInfo[i].Birthday_idx + vInfo[i].Birthday.Length + 1;
                    if(birthPlaceX < t.Length)
                        vInfo[i].Birthplace = t.Substring(birthPlaceX);
                }
                else
                {
                    vInfo[i].Name = t;
                }
                ++i;
            }


            // Report the number of matches found.
            StringBuilder sb = new StringBuilder();
            foreach (TTInfo info in vInfo)
            {
                info.CleanUp();
                sb.Append(info.ToString() + "\r\n");
            }
            sb.Append(matches.Count + " found.");
            return sb;
        }

        static string FixOCRConfusing1L(string s)
        {
            // Define a regular expression for repeated words.
            Regex rx = new Regex("A[0-9l]+");

            // Find matches.
            var matches = rx.Matches(s);

            // Report
            int start = 0;
            StringBuilder sb = new StringBuilder();
            foreach(Match match in matches)
            {
                GroupCollection groups = match.Groups;
                sb.Append(s.Substring(start, groups[0].Index - start));//assume not substring(0, 0)
                start = groups[0].Index + groups[0].Value.Length;
                sb.Append(groups[0].Value.Replace('l', '1'));
            }
            if (start < s.Length)
                sb.Append(s.Substring(start));
            return sb.ToString();
        }

        static void SearchDate(string patt, string s, ref TTInfo info)
        {
            // Define a regular expression for repeated words.
            Regex rx = new Regex(patt);

            // Find matches.
            var match = rx.Match(s);

            // Report
            if(match.Success)
            {
                GroupCollection groups = match.Groups;
                info.Birthday = groups[0].Value;
                info.Birthday_idx = groups[0].Index;
            }
        }
    }
}
