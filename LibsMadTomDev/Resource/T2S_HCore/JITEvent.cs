using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadTomDev.Resources.T2S_HCore
{
    public class JITEvent
    {
        public enum EType
        {
            Sentence = 0,
            Word = 1,
            Viseme = 2
        }
        public EType eType;
        public long streamPosi;
        public long selectedTextStartPosi;
        public long selectedTextLength;
        public int visemeId;
        public int visemeFeature;
        public int visemeNextId;
        public int visemeDuration;

        public JITEvent(string ioContent)
        {
            IOContent = ioContent;
        }
        public JITEvent(EType eType,
            long streamPosi,
            long selectedTextStartPosi,
            long selectedTextLength,
            int visemeId,
            int visemeFeature,
            int visemeNextId,
            int visemeDuration)
        {
            this.eType = eType;
            this.streamPosi = streamPosi;
            this.selectedTextStartPosi = selectedTextStartPosi;
            this.selectedTextLength = selectedTextLength;

            this.visemeId = visemeId;
            this.visemeFeature = visemeFeature;
            this.visemeNextId = visemeNextId;
            this.visemeDuration = visemeDuration;
        }

        public string IOContent
        {
            set
            {
                string[] parts = value.Split('\t');
                streamPosi = long.Parse(parts[0]);
                eType = (parts[1] == EType.Sentence.ToString()) ? EType.Sentence : ((parts[1] == EType.Word.ToString()) ? EType.Word : EType.Viseme);
                selectedTextStartPosi = long.Parse(parts[2]);
                selectedTextLength = long.Parse(parts[3]);

                visemeId = int.Parse(parts[4]);
                visemeFeature = int.Parse(parts[5]);
                visemeNextId = int.Parse(parts[6]);
                visemeDuration = int.Parse(parts[7]);
            }
            get
            {
                string result = "";
                result += streamPosi + "\t";
                result += eType.ToString() + "\t";
                result += selectedTextStartPosi + "\t";
                result += selectedTextLength + "\t";
                result += visemeId + "\t";
                result += visemeFeature + "\t";
                result += visemeNextId + "\t";
                result += visemeDuration;
                return result;
            }
        }
    }
}
