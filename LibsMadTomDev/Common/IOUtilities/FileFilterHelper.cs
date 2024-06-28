using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MadTomDev.Data
{
    public class FileFilterHelper
    {
        public FileFilterHelper()
        {
        }
        /// <summary>
        /// 初始化带有默认过滤器的过滤帮助器实例
        /// </summary>
        /// <param name="fileterString">采用标准样式，例如"all files|*.*|text files|*.txt"</param>
        public FileFilterHelper(string fileterString)
        {
            if (string.IsNullOrWhiteSpace(fileterString))
                return;

            string[] parts = fileterString.Split('|');
            for (int i = 1, iv = parts.Length; i < iv; i += 2)
            {
                AppendFilter(parts[i - 1], parts[i]);
            }
        }

        public class FilterItem
        {
            public FilterItem()
            {
            }
            public FilterItem(string label, params string[] fileFilters)
            {
                this.label = label;
                foreach (string f in fileFilters)
                {
                    AddFileFilter(f);
                }
            }

            public string label;
            public string FullDescription
            {
                get => $"{label}({FilterDescription})";
            }
            public string FilterDescription
            {
                get
                {
                    if (fileFilterList.Count <= 0)
                    {
                        return "[none]";
                    }
                    StringBuilder result = new StringBuilder();
                    for (int i = 0, iv = fileFilterList.Count; i < iv; i++)
                    {
                        result.Append(fileFilterList[i]);
                        result.Append("|");
                    }
                    if (result.Length > 0)
                        result.Remove(result.Length - 1, 1);
                    return result.ToString();
                }
            }
            public string[] fileFilters
            {
                get => fileFilterList.ToArray();
            }
            private List<string> fileFilterList = new List<string>();

            public void AddFileFilter(string fileFilter)
            {
                fileFilter = fileFilter.Trim();
                if (fileFilterList.Contains(fileFilter))
                    return;
                fileFilterList.Add(fileFilter);
            }

            public override int GetHashCode()
            {
                return label.GetHashCode() * fileFilterList.GetHashCode();
            }
            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;
                if (!(obj is FilterItem))
                    return false;
                FilterItem objItem = (FilterItem)obj;
                if (objItem.label != label)
                    return false;
                if (objItem.fileFilterList.Count != fileFilterList.Count)
                    return false;
                foreach (string f in objItem.fileFilterList)
                {
                    if (fileFilterList.Find(i => i == f) == null)
                        return false;
                }
                return true;
            }
            public static bool operator ==(FilterItem a, FilterItem b)
            {
                if (a is null)
                {
                    return b is null;
                }
                return a.Equals(b);
            }
            public static bool operator !=(FilterItem a, FilterItem b)
            {
                return !a.Equals(b);
            }
        }
        public List<FilterItem> filters = new List<FilterItem>();
        public void AppendFilter(string label, params string[] fileFilters)
        {
            FilterItem newFilter = new FilterItem(label, fileFilters);
            FilterItem found = filters.Find(f => f == newFilter);
            if (found == null)
            {
                filters.Add(newFilter);
            }
        }

        Common.SimpleStringHelper.Checker_starNQues sqChecker = new Common.SimpleStringHelper.Checker_starNQues();
        private void ReSetCheckerFilters(string[] fileFilters)
        {
            sqChecker.ClearPatterns();
            foreach (string f in fileFilters)
            {
                if (f == "*.*")
                {
                    sqChecker.ClearPatterns();
                    sqChecker.AddPattern("*");
                    break;
                }
                else
                {
                    sqChecker.AddPattern(f);
                }
            }
        }

        public List<DirectoryInfo> Filter(int filterIndex, IEnumerable<DirectoryInfo> dirs)
        {
            return Filter(filters[filterIndex], dirs);
        }
        public List<DirectoryInfo> Filter(FilterItem filterItem, IEnumerable<DirectoryInfo> dirs)
        {
            ReSetCheckerFilters(filterItem.fileFilters);

            List<DirectoryInfo> result = new List<DirectoryInfo>();
            foreach (DirectoryInfo di in dirs)
            {
                if (sqChecker.Check(di.Name))
                    result.Add(di);
            }
            return result;
        }
        public List<FileInfo> Filter(int filterIndex, IEnumerable<FileInfo> files)
        {
            return Filter(filters[filterIndex], files);
        }
        public List<FileInfo> Filter(FilterItem filterItem, IEnumerable<FileInfo> files)
        {
            ReSetCheckerFilters(filterItem.fileFilters);

            List<FileInfo> result = new List<FileInfo>();
            foreach (FileInfo fi in files)
            {
                if (sqChecker.Check(fi.Name))
                    result.Add(fi);
            }
            return result;
        }
        public List<IOInfoShadow> Filter(int filterIndex, IEnumerable<IOInfoShadow> iois)
        {
            return Filter(filters[filterIndex], iois);
        }
        public List<IOInfoShadow> Filter(FilterItem filterItem, IEnumerable<IOInfoShadow> iois)
        {
            ReSetCheckerFilters(filterItem.fileFilters);

            List<IOInfoShadow> result = new List<IOInfoShadow>();
            foreach (IOInfoShadow i in iois)
            {
                if (sqChecker.Check(i.name))
                    result.Add(i);
            }
            return result;
        }
    }
}