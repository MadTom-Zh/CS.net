using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadTomDev.Common
{
    public class CSVHelper
    {
        private CSVHelper() { }
        public class Reader : IDisposable
        {
            Stream fs;
            StreamReader fsr;
            public Reader(string csvFileFullName)
            {
                fs = File.OpenRead(csvFileFullName);

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                TextFileDecodeHelper.FileCodeInfo detail = TextFileDecodeHelper.AnalyzeFile(csvFileFullName);
                fsr = new StreamReader(fs, detail.Confidence > 0.8 ? detail.Encoding : Encoding.GetEncoding("GB2312"));
            }
            public Reader(Stream inputStream, Encoding encoding)
            {
                fs = inputStream;
                fsr = new StreamReader(fs, encoding);
            }

            public bool IsEoF { private set; get; } = false;
            public string[] ReadRow()
            {
                List<string> result = new List<string>();
                StringBuilder fieldBdr = new StringBuilder();

                string line = fsr.ReadLine();
                if (line == null)
                {
                    IsEoF = true;
                    return null;
                }
                bool qStart = false;
                char c, c2;
                while (line != null)
                {
                    for (int i = 0, iv = line.Length; i < iv; i++)
                    {
                        c = line[i];
                        if (c == '"')
                        {
                            if (i + 1 < iv)
                            {
                                c2 = line[i + 1];
                                if (c2 == '"')
                                {
                                    fieldBdr.Append('"');
                                    i++;
                                    continue;
                                }
                            }
                            if (qStart)
                            {
                                result.Add(fieldBdr.ToString());
                                fieldBdr.Clear();
                                i++;
                            }
                            qStart = !qStart;
                            continue;
                        }

                        if (qStart)
                        {
                            fieldBdr.Append(c);
                        }
                        else
                        {
                            if (c == ',')
                            {
                                result.Add(fieldBdr.ToString());
                                fieldBdr.Clear();
                            }
                            else
                            {
                                fieldBdr.Append(c);
                            }
                        }
                    }

                    if (qStart)
                    {
                        fieldBdr.Append(Environment.NewLine);
                        line = fsr.ReadLine();
                    }
                    else
                    {
                        result.Add(fieldBdr.ToString());
                        fieldBdr.Clear();
                        break;
                    }
                }

                if (qStart)
                {
                    result.Add(fieldBdr.ToString());
                    fieldBdr.Clear();
                }

                return result.ToArray();
            }
            public void Dispose()
            {
                fsr.Dispose();
                fs.Dispose();
            }
        }
        public class Writer : IDisposable
        {
            private Stream fs;
            private StreamWriter fsw;
            public Writer(string csvFileFullName)
            {
                fs = File.OpenWrite(csvFileFullName);
                fsw = new StreamWriter(fs, Encoding.UTF8);
            }
            public void WriteRow(string[] fields)
            {
                fsw.WriteLine(CSVHelper.RowToString(fields));
            }
            public void Flush()
            {
                fsw.Flush();
            }

            public void Dispose()
            {
                fsw.Dispose();
                fs.Dispose();
            }
        }


        public static string RowToString(params string[] fields)
        {
            if (fields == null)
                return null;
            StringBuilder result = new StringBuilder();
            string raw;
            foreach (string fd in fields)
            {
                if (fd != null)
                {
                    if (fd.Contains("\""))
                        raw = fd.Replace("\"", "\"\"");
                    else
                        raw = fd;

                    if (raw.Contains("\r") || raw.Contains("\n")
                        || raw.Contains(",") || raw.Contains("\""))
                    {
                        result.Append("\"");
                        result.Append(raw);
                        result.Append("\"");
                    }
                    else
                    {
                        result.Append(raw);
                    }
                }
                result.Append(",");
            }
            if (result.Length > 0)
                result.Remove(result.Length - 1, 1);
            return result.ToString();
        }
        public static string[] StringToRow(string rawRowString)
        {
            List<string> result = new List<string>();
            bool qtStart = false;
            StringBuilder field = new StringBuilder();
            char curC = '\0', nextC;
            bool isDoubleQt;
            for (int i = 0, iv = rawRowString.Length; i < iv; i++)
            {
                curC = rawRowString[i];
                if (qtStart)
                {
                    if (curC == '\"')
                    {
                        isDoubleQt = false;
                        if (i < iv - 1)
                        {
                            nextC = rawRowString[i + 1];
                            if (nextC == '\"')
                            {
                                isDoubleQt = true;
                            }
                        }
                        if (isDoubleQt)
                        {
                            field.Append("\"");
                        }
                        else
                        {
                            result.Add(field.ToString());
                            field.Clear();
                            qtStart = false;
                        }
                        i++;
                    }
                    else
                    {
                        field.Append(curC);
                    }
                }
                else  //  qtStart == false
                {
                    if (curC == '\"')
                    {
                        qtStart = true;
                    }
                    else if (curC == ',')
                    {
                        result.Add(field.ToString());
                        field.Clear();
                    }
                    else
                    {
                        field.Append(curC);
                    }
                }
            }

            if (field.Length > 0)
                result.Add(field.ToString());
            if (curC == ',')
                result.Add(field.ToString());
            return result.ToArray();
        }
    }
}
