using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tool
{
    enum TestType
    {
        EN_A,
        EN_B,
        EN_C,
        IT_A,
        IT_B,
        CNTT_CB,
        CNTT_NC
    }
    enum DataName
    {
        BASE_TEST_TYPE,
        ID,
        NAME1,
        BIRTHDATE,
        BIRTHPLACE
    }
    class TTakerFactory
    {
        int BaseTestType;
        int ID, Name1, Name2, Birthdate, Birthplace;
        public string TestDate;
        Dictionary<DataName, int> DatFmt = null;
        public TTakerFactory()
        {
            DatFmt = new Dictionary<DataName, int>();
        }
        public void ParseDataFormat(string file)
        {
            if (!File.Exists(file))
                throw new Exception("Data Format File not found.");
            Console.Write("A Key should only be either ");
            foreach (string k in Enum.GetNames(typeof(DataName)))
                Console.Write(k + " ");
            Console.WriteLine(" folllowed by [TAB] and the column index.");
            string[] map = File.ReadAllLines(file);
            int line_i = 0;
            foreach (string s in map)
            {
                string[] key_val = s.Split('\t');
                DataName name;
                if (key_val.Length == 2 && Enum.TryParse(key_val[0], out name))
                    DatFmt.Add(name, int.Parse(key_val[1]));
                else
                    throw new Exception("Wrong line " + line_i);
                ++line_i;
            }
            foreach(DataName k in Enum.GetValues(typeof(DataName)))
                if(!DatFmt.ContainsKey(k))
                throw new Exception("Format missing " + k);
            if (Enum.IsDefined(typeof(TestType), DatFmt[DataName.BASE_TEST_TYPE]))
                BaseTestType = (int)DatFmt[DataName.BASE_TEST_TYPE];
            else
                throw new Exception("Wrong test type");
            DatFmt.Remove(DataName.BASE_TEST_TYPE);
            ID = DatFmt[DataName.ID];
            Name1 = DatFmt[DataName.NAME1];
            Name2 = Name1 + 1;
            Birthdate = DatFmt[DataName.BIRTHDATE];
            Birthplace = DatFmt[DataName.BIRTHPLACE];
        }

        bool ParseTestDate(string line)
        {
            DateTime dt;
            if (15 < line.Length && DateTime.TryParseExact(line.Substring(8, 8), "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None, out dt))
            {
                TestDate = dt.ToString("yyyy-MM-dd");
                Console.WriteLine(TestDate);
                return true;
            }

            return false;
        }
        public List<TTaker> ParseTTaker(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);
            List<TTaker> takers = new List<TTaker>();
            int line_i = 0;
            foreach (string line in lines)
            {
                if (ParseTestDate(line))
                    continue;
                TTaker t = new TTaker(TestDate);
                string[] attr = line.Split('\t');
                if (!t.ParseID(attr[ID], BaseTestType))
                {
                    Console.WriteLine("invalid birthdate line " + line_i);
                    continue;
                }
                if (!t.ParseBirthdate(attr[Birthdate]))
                {
                    Console.WriteLine("invalid birthdate line " + line_i);
                    continue;
                }
                t.Name = attr[Name1];
                t.birthplace = RemoveDoubleSpace(MapString(attr[Birthplace]));
                takers.Add(t);
            }
            return takers;
        }

        public SortedDictionary<string, string> StringMap;
        public void ReadMap(string file)
        {
            StringMap = new SortedDictionary<string, string>();
            if (!File.Exists(file))
                return;
            string[] map_file = File.ReadAllLines(file);
            foreach (string s in map_file)
            {
                Char delim = '\t';
                string[] kvp = s.Split(delim);
                if (kvp.Length == 2)
                    StringMap.Add(kvp[0], kvp[1]);
            }
        }
        public string MapString(string originS)
        {
            string s = originS;
            foreach (KeyValuePair<string, string> kvp in StringMap)
            {
                int pos = s.IndexOf(kvp.Key, StringComparison.CurrentCultureIgnoreCase);
                if (-1 < pos)
                {
                    if (0 < pos)
                        s = s.Substring(0, pos) + kvp.Value +
                            s.Substring(pos + kvp.Key.Length, s.Length - pos - kvp.Key.Length);
                    else
                        s = kvp.Value +
                            s.Substring(pos + kvp.Key.Length, s.Length - pos - kvp.Key.Length);
                }
            }
            return s;
        }
        public string RemoveDoubleSpace(string s)
        {
            while (s.IndexOf("  ") != -1)
                s = s.Replace("  ", " ");
            return s;
        }
    }

    class TTaker
    {
        int testType;
        string testDate;
        int weakID;
        public string Name;
        DateTime birthdate;
        public string birthplace;
        int passed;
        public TTaker(string test_date) { testDate = test_date; passed = 0; }
        public bool ParseID(string id, int baseTestType)
        {
            char testTypeChar = id.ToCharArray()[0];
            if (testTypeChar == 'A' || testTypeChar == 'B' || testTypeChar == 'C')
            {
                testType = baseTestType + testTypeChar - 'A';
            }
            else
            {
                Console.WriteLine("Test type error ABC");
                return false;
            }
            if (!int.TryParse(id.Substring(1, id.Length - 1), out weakID))
            {
                Console.WriteLine("Test type error weak ID");
                return false;
            }
            return true;
        }
        public bool ParseBirthdate(string s)
        {
            s = s.Trim();
            s = s.Replace("\"", "");
            s = s.Replace("//", "");
            s = s.Replace(",", "");
            s = s.Replace("-", "/");
            bool parsed = false;
            int y;
            if (int.TryParse(s, out y) && 1930 < y && y < 2010)
            {
                birthdate = DateTime.ParseExact(y.ToString() + "/01/01", "yyyy/MM/dd", CultureInfo.CurrentCulture, DateTimeStyles.None);
                return true;
            }
            if (!parsed && DateTime.TryParseExact(s, "M/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out birthdate))
                parsed = true;
            if (!parsed && !DateTime.TryParseExact(s, "d/M/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out birthdate))
                return false;
            return true;
        }
        override public string ToString()
        {
            return testType + "\t" + testDate + "\t" + weakID + "\t" + Name + "\t" + birthdate.ToString("yyyy-MM-dd") +
                "\t" + birthplace + "\t" + passed;
        }
    }
    class Program
    {
        public static void Main(string[] args)
        {
            string fmt = "yyMMdd";
            if (args.Length == 1)
                fmt = args[0];
            TTakerFactory takerFactory = new TTakerFactory();
            Console.WriteLine("Reading " + Directory.GetCurrentDirectory() + "\\dataFormat.txt");
            takerFactory.ParseDataFormat(Directory.GetCurrentDirectory() + "\\dataFormat.txt");
            Console.WriteLine("Reading " + Directory.GetCurrentDirectory() + "\\mapString.txt");
            takerFactory.ReadMap(Directory.GetCurrentDirectory() + "\\mapString.txt");
            foreach (string path in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.txt"))
            {
                string filePath = Path.GetFileName(path);
                if (fmt.Length < filePath.Length)
                {
                    string d = filePath.Substring(0, fmt.Length);
                    DateTime dt;
                    if (DateTime.TryParseExact(d, "yyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None, out dt))
                    {
                        takerFactory.TestDate = dt.ToString("yyyy-MM-dd");
                        List<TTaker> takers = takerFactory.ParseTTaker(filePath);
                        StringBuilder sb = new StringBuilder();
                        Console.WriteLine("{0} takers {1}", filePath + ".txt", takers.Count);
                        foreach (TTaker t in takers)
                            sb.Append(t.ToString() + "\n");
                        File.WriteAllText(filePath + ".txt", sb.ToString());
                    }
                }
            }
        }
    }
}
