using System.IO;
using System;
using System.Text.RegularExpressions;




namespace VxiIds
{
    internal class Program
    {
        const String VppFile = @"VPP9.txt";
        const String VppFileLine = @"(?<name>.*)\s*(?<key>[A-Z][A-Z])";
        const RegexOptions VppFileOptions = RegexOptions.RightToLeft;

        const String vxiOfficial = @"VXI Mfg ID # 2010-8-24.txt";
        const String vxiOfficialLine = @"(?<key>\d*),(?<name>.*)";
        const RegexOptions vxiOfficialOptions = RegexOptions.None;

        const String vxiKeysightFile = @"VXI.txt";
        const String vxiKeysightLine = @"(?<key>\d+)\s+(?<name>.*)";
        const RegexOptions vxiKeysightOptions = RegexOptions.None;


        const String Root = "../../../data/";


        static void Main(string[] args)
        {
            var vpp = LoadVpp(VppFile, VppFileLine, VppFileOptions);
            var vxiOffical = LoadVpp(vxiOfficial, vxiOfficialLine, vxiOfficialOptions);
            var vxiKeysight = LoadVpp(vxiKeysightFile, vxiKeysightLine, vxiKeysightOptions);

            var vxiTables = new Dictionary<String, String>[] { vxiOffical, vxiKeysight };


            // produce a list of vppIDs that also have VXI ID's
            var VppVxiMap = new Dictionary<String, String>();

            foreach (KeyValuePair<string, string> vppEntry in vpp)
            {
                foreach (var vxi in vxiTables)
                {
                    foreach (KeyValuePair<string, string> vxiEntry in vxi)
                    {
                        //Console.WriteLine("Checking: {0}={1} and {2}", vppEntry.Key, vppEntry.Value, vxiEntry.Value);
                        if (Similar(vppEntry.Value, vxiEntry.Value))
                        {

                            if (VppVxiMap.ContainsKey(vppEntry.Key))
                            {
                                Console.WriteLine("Duplicate match: {0}={1} and {2}", vppEntry.Key, vppEntry.Value, vxiEntry.Value);
                            }
                            else
                            {
                                VppVxiMap[vppEntry.Key] = vxiEntry.Key;
                            }
                        }
                    }
                }


            }

            foreach (var VppId in VppVxiMap.Keys)
            {
                Console.WriteLine("[{1}] =\"{0}\",   // '{2}'", VppId.ToLower(), VppVxiMap[VppId], vpp[VppId]);
            }
            Console.WriteLine("Total entries:{0}", VppVxiMap.Count);

        }

        public static Boolean Similar(String s1, String s2)
        {
            // strict equality:
            //return s1.Equals(s2); 
            return EssenceOf(s1).Equals(EssenceOf(s2)); 
        }

        static String[] Decorations = {  // vareful about order here
            "corporation",
            "incorported",       // this misspelling is in the VXI vendor list
            "incorporated",
            "corp",
            "co",
            "inc",
            "gmbh",
            "limited",
            "ltd",
            "bv",
            "b v",
            "n v",
            "and",
        };

        public static String EssenceOf(String s) {
            s = s.ToLower();
            s = s + " ";                                // delimit the decorations
            s = Regex.Replace(s, "[,.'\"-;\\t&]", " "); // punctuation
            foreach (var decoration in Decorations)
                s = Regex.Replace(s, " " + decoration + " ", " ");


            s = Regex.Replace(s, " ", "");
            return s;
        }


        static Dictionary<String, String> LoadVpp(String Filename, String LineEx, RegexOptions Options)
        {
            var result = new Dictionary<String, String>();
            using (StreamReader streamReader = File.OpenText(Root+Filename))
            {
                String? line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    
                    var matches = Regex.Matches(line, LineEx, Options);
                    if (matches.Count == 1)
                    {
                        var groups = matches[0].Groups;
                        //Console.WriteLine("abbr:{0} name:{1} ", groups[2].Value, groups[1].Value);
                        var Key = groups[1].Value;
                        var Name = groups[2].Value;
                        if (groups[2].Name == "key")
                        {
                            Key = groups[2].Value;
                            Name = groups[1].Value;
                        }
                        result[Key] = Name;
                    }
                    else
                        throw new Exception("Parse error in " + Filename);
                }
            }

            return result;



        }
    }
}