using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadTomDev.Common
{
    public class TextFileDecodeHelper
    {
        public struct FileCodeInfo
        {
            public Encoding Encoding;
            public string EncodingName;
            public double Confidence;
            public override string ToString()
            {
                StringBuilder strBdr = new StringBuilder();
                strBdr.AppendLine("Encoding: " + EncodingName);
                strBdr.Append("Confidence: " + Confidence.ToString("P2"));
                return strBdr.ToString();
            }
        }

        public static FileCodeInfo AnalyzeFile(string fileFullName)
        {
            return _GetCodeInfo(_GetCD(fileFullName));
        }
        private static Ude.CharsetDetector _GetCD(string fileFullName)
        {
            Ude.CharsetDetector cdet = new Ude.CharsetDetector();
            using (FileStream fs = File.OpenRead(fileFullName))
            {
                cdet.Feed(fs);
                cdet.DataEnd();
            }
            return cdet;
        }
        private static Ude.CharsetDetector _GetCD(Stream stream)
        {
            Ude.CharsetDetector cdet = new Ude.CharsetDetector();
            cdet.Feed(stream);
            cdet.DataEnd();

            return cdet;
        }

        public static string ReadAllTextInUTF8(string fileFullName, out FileCodeInfo codeInfo)
        {
            codeInfo = AnalyzeFile(fileFullName);
            if (codeInfo.Encoding == null)
                return File.ReadAllText(fileFullName, Encoding.UTF8);
            else
                return File.ReadAllText(fileFullName, codeInfo.Encoding);
        }
        public static StreamReader ReaderInUTF8(Stream inStream, out FileCodeInfo codeInfo)
        {
            codeInfo = _GetCodeInfo(_GetCD(inStream));
            if (codeInfo.Encoding != null)
                return new StreamReader(inStream, codeInfo.Encoding);
            else
                return new StreamReader(inStream, Encoding.UTF8);
        }
        private static FileCodeInfo _GetCodeInfo(Ude.CharsetDetector detail)
        {
            if (detail == null)
                return new FileCodeInfo() { EncodingName = "unknow", Confidence = 0, };
            else
                return new FileCodeInfo()
                {
                    Encoding = Encoding.GetEncoding(detail.Charset),
                    EncodingName = detail.Charset,
                    Confidence = detail.Confidence,
                };
        }

    }
}
