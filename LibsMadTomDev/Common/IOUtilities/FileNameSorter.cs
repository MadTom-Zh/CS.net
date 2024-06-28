using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace MadTomDev.Data
{
    public class FileNameSorter
    {
        public static void Sort(ref string[] fileList, bool isAscOrDesc)
        {
            List<string> helper = new List<string>();
            helper.AddRange(fileList);
            Sort(ref helper, isAscOrDesc);
            for (int i = 0, iv = helper.Count; i < iv; ++i)
            {
                fileList[i] = helper[i];
            }
        }
        public static void Sort(ref List<string> fileList, bool isAscOrDesc)
        {
            // load list into 2-levels list
            Dictionary<string, List<string>> helper = new Dictionary<string, List<string>>();
            List<string> dirNameList = new List<string>();
            List<string> subNameList;
            string dirName;
            foreach (string fullName in fileList)
            {
                dirName = Path.GetDirectoryName(fullName);
                if (helper.ContainsKey(dirName))
                {
                    subNameList = helper[dirName];
                }
                else
                {
                    dirNameList.Add(dirName);
                    subNameList = new List<string>();
                    helper.Add(dirName, subNameList);
                }
                subNameList.Add(fullName.Substring(dirName.Length + 1));
            }

            // sort dir list
            dirNameList.Sort(FileNameCompare);

            // sort subname list
            foreach (string key in helper.Keys)
            {
                subNameList = helper[key];
                subNameList.Sort(FileNameCompare);
            }

            // refill fileList
            fileList.Clear();
            if (isAscOrDesc)
            {
                for (int i = 0, iv = dirNameList.Count, j, jv; i < iv; ++i)
                {
                    dirName = dirNameList[i];
                    subNameList = helper[dirName];
                    for (j = 0, jv = subNameList.Count; j < jv; ++j)
                    {
                        fileList.Add(Path.Combine(dirName, subNameList[j]));
                    }
                }
            }
            else
            {
                for (int i = dirNameList.Count - 1, j; i >= 0; --i)
                {
                    dirName = dirNameList[i];
                    subNameList = helper[dirName];
                    for (j = subNameList.Count - 1; j >= 0; --j)
                    {
                        fileList.Add(Path.Combine(dirName, subNameList[j]));
                    }
                }
            }
        }
        private static int FileNameCompare(string a, string b)
        {
            double? an = GetPrefixNum(a);
            if (an == null)
            {
                return a.CompareTo(b);
            }
            else
            {
                double? bn = GetPrefixNum(b);
                if (bn == null)
                {
                    return a.CompareTo(b);
                }
                else
                {
                    return ((double)an).CompareTo((double)bn);
                }
            }
        }

        public static double? GetPrefixNum(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            name = name.Trim();
            StringBuilder numBdr = new StringBuilder();
            int i = 0, iv = name.Length;
            char c;
            while (i < iv)
            {
                c = name[i++];
                if (c == '.' || (c >= '0' && c <= '9'))
                    numBdr.Append(c);
                else
                {
                    if (c >= 'A')
                        return null;
                    break;
                }
            }
            if (numBdr.Length == 0)
                return null;
            double.TryParse(numBdr.ToString(), out double result);
            return result;
        }
    }
}
