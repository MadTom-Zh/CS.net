using System;

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace MadTomDev.Resources.T2S_HCore
{
    public class MultiVoice
    {
        public enum LanguageType
        {
            General,
            EN,
            ZH,
            JA,
            DE,
            RU,
        }
        public class VoiceLanguageLib
        {
            public static string FileFullName_VLLib
                = Path.Combine(Common.Variables.IOPath.SettingDir, "TTS_VLLib.txt");
            public List<VLLItem> lib = new List<VLLItem>();
            public List<VLLItem> LoadLib()
            {
                lib.Clear();
                if (File.Exists(FileFullName_VLLib))
                {
                    foreach (string line in File.ReadAllLines(FileFullName_VLLib))
                    {
                        if (line != "")
                        {
                            lib.Add(new VLLItem(line));
                        }
                    }
                }
                return lib;
            }


            private bool SaveLib_saved = false;
            public void SaveLib()
            {
                if (SaveLib_saved)
                    return;

                if (File.Exists(FileFullName_VLLib))
                {
                    File.Delete(FileFullName_VLLib);
                }
                using (StreamWriter sr = File.CreateText(FileFullName_VLLib))
                {
                    int endIdx = lib.Count - 1;
                    for (int i = 0; i <= endIdx; i++)
                    {
                        sr.WriteLine(lib[i].IOContent);
                    }
                }
                SaveLib_saved = true;
            }


            /// <summary>
            /// add a Item, if it has the same VoiceName, the item in Lib will be updated
            /// </summary>
            /// <param name="vllItem"></param>
            public void AddUpdateLibItem(VLLItem vllItem)
            {
                VLLItem libItem;
                for (int i = lib.Count - 1; i >= 0; i--)
                {
                    libItem = lib[i];
                    if (libItem.VoiceName == vllItem.VoiceName)
                    {
                        lib[i] = vllItem;
                        SaveLib_saved = false;
                        return;
                    }
                }
                lib.Add(vllItem);
                SaveLib_saved = false;
            }
            /// <summary>
            /// delete items by matching VoiceName
            /// </summary>
            /// <param name="vllItem"></param>
            /// <returns>row affected</returns>
            public int RemoveLibItem(VLLItem vllItem)
            {
                return RemoveLibItem(vllItem.VoiceName);
            }
            public int RemoveLibItem(string voiceName)
            {
                int rowAftd = 0;
                VLLItem libItem;
                for (int i = lib.Count - 1; i >= 0; i--)
                {
                    libItem = lib[i];
                    if (libItem.VoiceName == voiceName)
                    {
                        lib.RemoveAt(i);
                        rowAftd++;
                    }
                }
                SaveLib_saved = false;
                return rowAftd;
            }

            internal bool AddIdle()
            {
                BroadCaster bc = BroadCaster.GetInstance();
                VLLItem? foundV;
                foreach (BroadCaster.Voice v in bc.Voices)
                {
                    foundV = lib.Find(i => i.VoiceName == v.description);
                    if (foundV == null)
                    {
                        lib.Add(new VLLItem(v.description, LanguageType.General));
                        SaveLib_saved = false;
                        return true;
                    }
                }
                return false;
            }

            #region voice move up/down

            /// <summary>
            /// make the item index lesser
            /// </summary>
            /// <param name="voiceName"></param>
            public bool MoveItemUp(string voiceName)
            {
                return MoveItem(voiceName, true);
            }
            public bool MoveItemDown(string voiceName)
            {
                return MoveItem(voiceName, false);
            }
            private bool MoveItem(string voiceName, bool isToZero)
            {
                VLLItem? item = null;
                int idx;
                bool notFound = true;
                for (idx = lib.Count - 1; idx >= 0; idx--)
                {
                    item = lib[idx];
                    if (voiceName == item.VoiceName)
                    {
                        notFound = false;
                        break;
                    }
                }
                if (notFound == true
                    || idx < 0
                    || (isToZero == true && idx == 0)
                    || (isToZero == false && idx == lib.Count - 1)
                    )
                {
                    return false;
                }
                lib.RemoveAt(idx);
                if (isToZero == true)
                {
                    lib.Insert(idx - 1, item);
                }
                else
                {
                    lib.Insert(idx + 1, item);
                }
                SaveLib_saved = false;
                return true;
            }

            #endregion

            public class VLLItem
            {
                public string VoiceName;
                public LanguageType Language;
                public int CustomSpeed = 0;
                public int CustomVolume = 100;

                public VLLItem(string voiceName, LanguageType language)
                {
                    VoiceName = voiceName;
                    Language = language;
                }
                public VLLItem(string ioContent)
                {
                    this.IOContent = ioContent;
                }

                public string IOContent
                {
                    get
                    {
                        string content = VoiceName + "\t";
                        content += Language.ToString() + "\t";
                        content += CustomSpeed.ToString() + "\t";
                        content += CustomVolume.ToString() + "";
                        return content;
                    }
                    set
                    {
                        string[] parts = value.Split('\t');
                        VoiceName = parts[0];
                        Language = (LanguageType)Enum.Parse(typeof(LanguageType), parts[1]);
                        CustomSpeed = int.Parse(parts[2]);
                        CustomVolume = int.Parse(parts[3]);
                    }
                }

                private bool? _Avaliable = null;
                public bool Avaliable
                {
                    get
                    {
                        if (_Avaliable == null)
                            _Avaliable = BroadCaster.GetInstance().HaveVoice(VoiceName);
                        return _Avaliable == true;
                    }
                }
            }
        }
        public class TextMultiFragments
        {
            public class Fragment
            {
                public string? fgValue;
                public LanguageType fgLang;
            }
            public static List<Fragment> Detach(string text)
            {
                List<Fragment> result = new List<Fragment>();
                Fragment fg;
                LanguageType LT;
                int lastIdx;

                //Detach_RemoveOdds(ref text);
                foreach (char chr in text)
                {
                    LT = GetCharLanguage(chr);
                    lastIdx = result.Count - 1;
                    if (lastIdx < 0)
                    {
                        result.Add(new Fragment() { fgLang = LT, fgValue = chr + "" });
                    }
                    else
                    {
                        fg = result[lastIdx];
                        if (fg.fgLang == LT)
                        {
                            fg.fgValue += chr;
                            result[lastIdx] = fg;
                        }
                        else
                        {
                            result.Add(new Fragment() { fgLang = LT, fgValue = chr + "" });
                        }
                    }
                }

                Detach_GroupJoin(ref result);

                return result;
            }

            private void Detach_RemoveOdds(ref string value)
            {
                while (value.Contains("　"))
                {
                    value = value.Replace("　", " ");
                }
                while (value.Contains("  "))
                {
                    value = value.Replace("  ", " ");
                }
                while (value.Contains("，"))
                {
                    value = value.Replace("，", ",");
                }
                while (value.Contains(",,"))
                {
                    value = value.Replace(",,", ",");
                }
                while (value.Contains("。。"))
                {
                    value = value.Replace("。。", "。");
                }
                while (value.Contains(".."))
                {
                    value = value.Replace("..", ".");
                }
            }
            private static void Detach_GroupJoin(ref List<Fragment> frags)
            {
                Fragment curFrag;

                // combine ZH into JP
                Fragment nextFrag;
                for (int i = frags.Count - 2; i >= 0; i--)
                {
                    curFrag = frags[i];
                    nextFrag = frags[i + 1];
                    if ((curFrag.fgLang == LanguageType.JA && nextFrag.fgLang == LanguageType.ZH)
                        || (curFrag.fgLang == LanguageType.ZH && nextFrag.fgLang == LanguageType.JA)
                        || (curFrag.fgLang == LanguageType.JA && nextFrag.fgLang == LanguageType.JA)
                        )
                    {
                        curFrag.fgLang = LanguageType.JA;
                        curFrag.fgValue += nextFrag.fgValue;
                        frags[i] = curFrag;
                        frags.RemoveAt(i + 1);
                    }
                }

                // combine general into neighbour   // 2018 first join to EN, then JA, last ZH
                for (int i = frags.Count - 2; i >= 0; i--)
                {
                    curFrag = frags[i];
                    nextFrag = frags[i + 1];
                    if ((curFrag.fgLang == LanguageType.EN && nextFrag.fgLang == LanguageType.General)
                        || (curFrag.fgLang == LanguageType.General && nextFrag.fgLang == LanguageType.EN)
                        || (curFrag.fgLang == LanguageType.EN && nextFrag.fgLang == LanguageType.EN)
                        )
                    {
                        curFrag.fgLang = LanguageType.EN;
                        curFrag.fgValue += nextFrag.fgValue;
                        frags[i] = curFrag;
                        frags.RemoveAt(i + 1);
                    }
                }
                for (int i = frags.Count - 2; i >= 0; i--)
                {
                    curFrag = frags[i];
                    nextFrag = frags[i + 1];
                    if ((curFrag.fgLang == LanguageType.JA && nextFrag.fgLang == LanguageType.General)
                        || (curFrag.fgLang == LanguageType.General && nextFrag.fgLang == LanguageType.JA)
                        || (curFrag.fgLang == LanguageType.JA && nextFrag.fgLang == LanguageType.JA)
                        )
                    {
                        curFrag.fgLang = LanguageType.JA;
                        curFrag.fgValue += nextFrag.fgValue;
                        frags[i] = curFrag;
                        frags.RemoveAt(i + 1);
                    }
                }
                for (int i = frags.Count - 2; i >= 0; i--)
                {
                    curFrag = frags[i];
                    nextFrag = frags[i + 1];
                    if ((curFrag.fgLang == LanguageType.ZH && nextFrag.fgLang == LanguageType.General)
                        || (curFrag.fgLang == LanguageType.General && nextFrag.fgLang == LanguageType.ZH)
                        || (curFrag.fgLang == LanguageType.ZH && nextFrag.fgLang == LanguageType.ZH)
                        )
                    {
                        curFrag.fgLang = LanguageType.ZH;
                        curFrag.fgValue += nextFrag.fgValue;
                        frags[i] = curFrag;
                        frags.RemoveAt(i + 1);
                    }
                }


                // trim, and remove empty frags
                for (int i = frags.Count - 1; i >= 0; i--)
                {
                    curFrag = frags[i];
                    curFrag.fgValue = curFrag.fgValue.Trim();
                    if (curFrag.fgValue == "")
                    {
                        frags.RemoveAt(i);
                    }
                }
            }
            public static string Assembly(List<Fragment> fragments)
            {
                string? result = null;
                int count = fragments.Count;
                for (int i = 0; i < count; i++)
                {
                    result += fragments[i].fgValue;
                }
                return result;
            }

            public static Regex regexJA = new Regex("^[\u3040-\u309F\u30A0-\u30FF]+$");
            public static Regex regexZH = new Regex("^[\u4e00-\u9fa5]+$");
            public static LanguageType GetCharLanguage(char chr)
            {
                if ((chr >= 'a' && chr <= 'z')
                    || (chr >= 'A' && chr <= 'Z')
                    )
                {
                    return LanguageType.EN;
                }
                if ((chr >= '0' && chr <= '9'))
                {
                    return LanguageType.General;
                }
                if (regexZH.IsMatch(chr + "") == true)
                {
                    return LanguageType.ZH;
                }
                if (regexJA.IsMatch(chr + "") == true)
                {
                    return LanguageType.JA;
                }


                return LanguageType.General;
            }
        }

        public VoiceLanguageLib voiceLangLib
            = new VoiceLanguageLib();
        public static MultiVoice instance;
        public static MultiVoice GetInstance()
        {
            if (instance == null)
            {
                instance = new MultiVoice();
            }
            return instance;
        }
        public MultiVoice()
        {
            voiceLangLib.LoadLib();
            broadCaster.StreamEnd += bc_StreamEnd;
            broadCaster.SaveWave_FStream_Complete += bc_SaveWave_FStream_Complete;
            broadCaster.Speeking_WordBoundary += bc_Speeking_WordBoundary;
            broadCaster.Speeking_SentenceBoundary += bc_Speeking_SentenceBoundary;
        }

        private DateTime timeStamp;
        private long speeking_base_streamPosition = 0;
        private long speeking_base_streamPosition_tmp = 0;
        private int speeking_base_startWordIndex = 0;
        private int speeking_base_startWordIndex_lastEnd = 0;
        private int speeking_base_startWordIndex_tmp = 0;
        private int speeking_base_readingSentenceIndex = 0;
        private int speeking_base_readingSentenceIndex_tmp = 0;
        private int speeking_base_startSentenceIndex = 0;
        private int speeking_base_startSentenceIndex_lastEnd = 0;
        private int speeking_base_startSentenceIndex_tmp = 0;
        private int speeking_base_countReline = 0;
        private int speeking_base_countReline_lastEnd = 0;
        private int speeking_base_countReline_tmp = 0;
        private void ClearSpeekingBaseVals()
        {
            speeking_base_streamPosition = 0;
            speeking_base_streamPosition_tmp = 0;
            speeking_base_startWordIndex = 0;
            speeking_base_startWordIndex_tmp = 0;
            speeking_base_readingSentenceIndex = 0;
            speeking_base_readingSentenceIndex_tmp = 0;
            speeking_base_startSentenceIndex = 0;
            speeking_base_startSentenceIndex_tmp = 0;
        }
        public event EventHandler<BroadCaster.Speeking_WordBoundaryEventArgs> Speeking_WordBoundaryEvent;
        public event EventHandler<BroadCaster.Speeking_SentenceBoundaryEventArgs> Speeking_SentenceBoundaryEvent;
        private void bc_Speeking_WordBoundary(object sender, BroadCaster.Speeking_WordBoundaryEventArgs e)
        {
            //throw new NotImplementedException();
            timeStamp = DateTime.Now;
            speeking_base_streamPosition_tmp = speeking_base_streamPosition + e.streamPosition;
            speeking_base_startWordIndex_lastEnd = e.wordStartIndex + e.wordLength;
            speeking_base_startWordIndex_tmp = speeking_base_startWordIndex + speeking_base_startWordIndex_lastEnd;


            if (Speeking_WordBoundaryEvent != null)
            {
                Speeking_WordBoundaryEvent
                (
                    this,
                    new BroadCaster.Speeking_WordBoundaryEventArgs
                    (
                        speeking_base_streamPosition + e.streamPosition,
                        speeking_base_startWordIndex + e.wordStartIndex,
                        e.wordLength,
                        e.word
                    )
                );
            }
        }
        private void bc_Speeking_SentenceBoundary(object sender, BroadCaster.Speeking_SentenceBoundaryEventArgs e)
        {
            //throw new NotImplementedException();
            timeStamp = DateTime.Now;
            speeking_base_streamPosition_tmp = speeking_base_streamPosition + e.streamPosition;
            speeking_base_readingSentenceIndex_tmp = speeking_base_readingSentenceIndex + e.readingSentenceIndex + 1;
            speeking_base_startSentenceIndex_lastEnd = e.sentenceStartIndex + e.sentenceLength;
            speeking_base_startSentenceIndex_tmp = speeking_base_startSentenceIndex + speeking_base_startSentenceIndex_lastEnd;

            if (Speeking_SentenceBoundaryEvent != null)
            {
                Speeking_SentenceBoundaryEvent
                (
                    this,
                    new BroadCaster.Speeking_SentenceBoundaryEventArgs
                    (
                        speeking_base_streamPosition + e.streamPosition,
                        speeking_base_readingSentenceIndex + e.readingSentenceIndex,
                        e.sentence,
                        speeking_base_startSentenceIndex + e.sentenceStartIndex,
                        e.sentenceLength
                    )
                );
            }
        }

        public void Dispose()
        {
            broadCaster.Dispose();
        }

        void bc_SaveWave_FStream_Complete(object sender, EventArgs e)
        {
            SpeekNextFrag(false);
        }
        void bc_StreamEnd(object sender, BroadCaster.StreamEnd_Args e)
        {
            TimeSpan timeSpan = DateTime.Now - timeStamp;
            speeking_base_streamPosition = speeking_base_streamPosition_tmp + BroadCaster.Calculate_StreamLength(timeSpan);

            int fragLength = 0;
            if (textFragments.Count > 0)
            {
                fragLength = textFragments[0].fgValue.Length;
            }

            speeking_base_startWordIndex = speeking_base_startWordIndex_tmp + (fragLength - speeking_base_startWordIndex_lastEnd);
            speeking_base_readingSentenceIndex = speeking_base_readingSentenceIndex_tmp;
            speeking_base_startSentenceIndex = speeking_base_startSentenceIndex_tmp + (fragLength - speeking_base_startSentenceIndex_lastEnd);
            speeking_base_countReline = speeking_base_countReline_tmp - speeking_base_countReline_lastEnd;

            if (textFragments != null && textFragments.Count > 0)
            {
                textFragments.RemoveAt(0);
            }
            SpeekNextFrag(e.IsUserStop);
        }
        private void SpeekNextFrag(bool isUserStop)
        {
            // when using  Save2C , bc_StreamEnd not trigering...
            if (isSave2C && textFragments.Count > 0) textFragments.RemoveAt(0);

            if (textFragments != null && textFragments.Count > 0)
            {
                if (isSaveMode == true)
                {
                    saveToWav_index++;
                    SaveFragWav(textFragments[0]);
                }
                else
                {
                    SpeekFrag(textFragments[0]);
                }
            }
            else
            {
                isSave2C = false;

                ClearSpeekingBaseVals();
                if (textFragments != null)
                {
                    textFragments.Clear();
                }
                _isPlaying = false;
                if (MVSpeekEnd != null)
                {
                    MVSpeekEnd(this, new BroadCaster.StreamEnd_Args(isUserStop));
                }
            }
        }
        public event EventHandler MVSpeekStart;
        public event EventHandler<BroadCaster.StreamEnd_Args> MVSpeekEnd;


        public BroadCaster broadCaster = BroadCaster.GetInstance();
        private List<TextMultiFragments.Fragment> textFragments = new List<TextMultiFragments.Fragment>();

        public void Speek(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            isSaveMode = false;
            textFragments = TextMultiFragments.Detach(text);
            _isPlaying = true;
            MVSpeekStart?.Invoke(this, new EventArgs());

            SpeekFrag(textFragments[0]);
        }
        public void Speek(List<TextMultiFragments.Fragment> textFragments)
        {
            isSaveMode = false;
            this.textFragments = textFragments;
            _isPlaying = true;
            MVSpeekStart?.Invoke(this, new EventArgs());

            SpeekFrag(this.textFragments[0]);
        }

        bool isSave2C = false;
        /// <summary>
        /// this function will save several wav files, suffix like "000#.wav"
        /// </summary>
        /// <param name="baseFileFullName">like @"C:\test_", watch it, no ".wav" behind</param>
        public void Speek2WaveMultiFiles(string text, string baseFileFullName)
        {
            isSave2C = true;

            isSaveMode = true;
            saveToWav_baseFileFullName = baseFileFullName;
            saveToWav_index = 0;
            broadCaster.Stop();
            textFragments = TextMultiFragments.Detach(text);
            _isPlaying = true;
            MVSpeekStart?.Invoke(this, new EventArgs());

            SaveFragWav(textFragments[0]);
        }
        public void Speek2WaveMultiFiles(List<TextMultiFragments.Fragment> textFragments, string baseFileFullName)
        {
            isSaveMode = true;
            saveToWav_baseFileFullName = baseFileFullName;
            saveToWav_index = 0;
            broadCaster.Stop();
            this.textFragments = textFragments;
            _isPlaying = true;
            MVSpeekStart?.Invoke(this, new EventArgs());

            SaveFragWav(this.textFragments[0]);
        }
        private void SpeekFrag(TextMultiFragments.Fragment frag)
        {
            SpeekFrag_SetLanguage(frag.fgLang);
            broadCaster.Speek(frag.fgValue);
        }
        public bool isSaveMode = false;
        public string saveToWav_baseFileFullName;
        public int saveToWav_index;
        private void SaveFragWav(TextMultiFragments.Fragment frag)
        {
            SpeekFrag_SetLanguage(frag.fgLang);
            broadCaster.SaveWave_FStream(frag.fgValue, saveToWav_baseFileFullName + saveToWav_index.ToString("000#") + ".wav");
        }
        private void SpeekFrag_SetLanguage(LanguageType lang)
        {
            VoiceLanguageLib.VLLItem? vllItem = null;
            for (int i = 0, iv = voiceLangLib.lib.Count; i < iv; i++)
            {
                vllItem = voiceLangLib.lib[i];
                if (lang == vllItem.Language)
                {
                    break;
                }
            }
            if (vllItem == null)
            {
                throw new Exception("No Language(" + lang.ToString() + ") found in VoiceLangLib!");
            }
            BroadCaster.Voice? bcVoice = null;
            foreach (BroadCaster.Voice v in broadCaster.Voices)
            {
                if (v.description == vllItem.VoiceName)
                {
                    bcVoice = v;
                    break;
                }
            }
            if (bcVoice == null)
            {
                throw new Exception("No Voice(" + vllItem.VoiceName + ") found in BroadCaster!");
            }

            broadCaster.voice = bcVoice;
            broadCaster.Rate = vllItem.CustomSpeed;
            broadCaster.Volume = vllItem.CustomVolume;
        }

        private bool _isPlaying = false;
        public bool isPlaying
        {
            get => _isPlaying;
        }
        public void Stop()
        {
            textFragments.Clear();
            broadCaster.Stop();
            ClearSpeekingBaseVals();
            _isPlaying = false;
            if (MVSpeekEnd != null)
            {
                MVSpeekEnd(this, new BroadCaster.StreamEnd_Args(true));
            }
        }

        public class TextHelper
        {
            public static bool Text_CheckIfOnlySymbols(string text)
            {
                return Text_RemoveAllSymbols(text).Length == 0;
            }
            public static string[] Symbols
                = new string[] { ",", ".", ":", "/", "?" , "!", "@", "#", "$", "%" , "^", "&", "*", "(", ")", "-",
                "+","_","=","，","。","？","、","！","·","~",
                    //" ",
                    //"🐶","❌","⭕️","✈","🔥","💩","👻","😂"
                };
            public static string Text_RemoveAllSymbols(string text)
            {
                if (text == null) return null;

                string testing = text;
                foreach (string s in Symbols)
                {
                    testing = testing.Replace(s, "");
                }

                return testing;
            }

            public static string Text_numberToChinese(string numText)
            {
                string integral = "", fractional = "";
                if (numText.Contains("."))
                {
                    while (numText.EndsWith("0"))
                    {
                        numText = numText.Substring(0, numText.Length - 1);
                    }
                    int pointIdx = numText.IndexOf(".");
                    if (pointIdx != numText.LastIndexOf("."))
                    {
                        throw new Exception("It's NOT a legal number. " + numText);
                    }
                    integral = numText.Substring(0, pointIdx);
                    fractional = numText.Substring(pointIdx + 1);
                }
                else integral = numText;

                #region integral
                string result = "";
                int level = 0, levelInner, levelBig = 0;
                int levelMax = integral.Length - 1;
                char c;
                string part = "";
                bool midZero = false, allZero = false;
                for (int i = integral.Length - 1; i >= 0; i--)
                {
                    c = integral[i];
                    levelInner = level % 4;
                    levelBig = level / 4;

                    if (levelBig > 0 && levelInner == 0)
                    {
                        if (allZero == true)
                        {
                            allZero = false;
                        }
                        else if (midZero == true)
                        {
                            if (level < levelMax)
                            {
                                part = "零" + part;
                            }
                            midZero = false;
                        }
                        result = _Text_numberToChinese_bigUnit(levelBig) + part + result;
                        part = "";
                    }

                    switch (levelInner)
                    {
                        case 0:
                            if (c != '0')
                            {
                                part = Text_numberToChinese_sample(c) + part;
                                allZero = false;
                            }
                            break;
                        case 1:
                            if (c != '0')
                            {
                                if (levelMax == levelInner && c == '1') part = "十" + part;
                                else part = Text_numberToChinese_sample(c) + "十" + part;
                                allZero = false;
                            }
                            break;
                        case 2:
                            if (c != '0')
                            {
                                part = Text_numberToChinese_sample(c) + "百" + part;
                                allZero = false;
                            }
                            break;
                        case 3:
                            if (c == '0')
                            {
                                midZero = true;
                            }
                            else
                            {
                                part = Text_numberToChinese_sample(c) + "千" + part;
                                allZero = false;
                            }
                            break;
                    }

                    level++;
                }

                result = part + result;
                #endregion

                #region fractional
                if (fractional.Length > 1)
                {
                    result += "点" + Text_numberToChinese_sample(fractional);
                }
                #endregion

                return result;
            }

            /// <summary>
            /// 阿拉伯数字转中文大写数字 *采用中法，万进系统
            /// </summary>
            /// <param name="num"></param>
            /// <returns></returns>
            public static string Text_numberToChinese(double num)
            {
                return Text_numberToChinese_integral(num) + Text_numberToChinese_fractional(num);
            }

            public static string Text_numberToChinese_integral(double num)
            {
                string result = "", resultPiece = "";
                bool isNagtive = num < 0;

                double tmp = Math.Abs(num);
                int tmp2, tmp3, bigUnit = 0;
                bool midZero, meetZero = false, meetZero_all;

                while (tmp >= 1)
                {
                    midZero = false;
                    meetZero_all = true;
                    resultPiece = "";
                    tmp2 = (int)(tmp % 10000);
                    if (tmp2 >= 0)
                    {
                        tmp3 = tmp2 % 10;
                        if (tmp3 == 0)
                        {
                        }
                        else
                        {
                            meetZero = meetZero_all = false;
                            resultPiece = _Text_numberToChinese_single(tmp3) + resultPiece;
                        }
                    }
                    if (tmp2 >= 10)
                    {
                        tmp3 = (tmp2 / 10) % 10;
                        if (tmp3 == 0)
                        {
                            meetZero = true;
                        }
                        else
                        {
                            meetZero = meetZero_all = false;
                            if (tmp < 20) resultPiece = _Text_numberToChinese_smallUnit(1) + resultPiece;
                            else resultPiece = _Text_numberToChinese_single(tmp3) + "十" + resultPiece;
                        }
                    }
                    else if (tmp2 < 100) midZero = true;
                    if (tmp2 >= 100)
                    {
                        tmp3 = (tmp2 / 100) % 10;
                        if (tmp3 == 0)
                        {
                            if (meetZero == false && bigUnit > 0)
                            {
                                resultPiece = _Text_numberToChinese_single(tmp3) + resultPiece;
                            }
                            else { }
                            meetZero = true;
                        }
                        else
                        {
                            meetZero = meetZero_all = false;
                            resultPiece = _Text_numberToChinese_single(tmp3) + "百" + resultPiece;
                        }
                    }
                    else if (tmp2 < 1000) midZero = true;
                    if (tmp2 >= 1000)
                    {
                        tmp3 = (tmp2 / 1000) % 10;
                        if (tmp3 == 0)
                        {
                            if (meetZero == false && bigUnit > 0)
                            {
                                resultPiece = _Text_numberToChinese_single(tmp3) + resultPiece;
                            }
                            else { }
                            meetZero = true;
                        }
                        else
                        {
                            meetZero = meetZero_all = false;
                            resultPiece = _Text_numberToChinese_single(tmp3) + "千" + resultPiece;
                        }
                    }
                    else if (tmp2 < 10000) midZero = true;



                    if (meetZero_all == true)
                    {
                    }
                    else if (midZero == true && tmp >= 10000)
                    {
                        if (bigUnit > 0) result = "零" + resultPiece + _Text_numberToChinese_bigUnit(bigUnit) + result;
                        else result = "零" + resultPiece + result;
                    }
                    else
                    {
                        if (bigUnit > 0) result = resultPiece + _Text_numberToChinese_bigUnit(bigUnit) + result;
                        else result = resultPiece + result;
                    }
                    bigUnit++;

                    tmp /= 10000;
                }

                if (isNagtive == true)
                {
                    result = "负" + result;
                }

                return result;


            }

            public static string Text_numberToChinese_fractional(double num)
            {
                string fractionalText = num + "";
                if (fractionalText.Contains("."))
                {
                    fractionalText = fractionalText.Substring(fractionalText.IndexOf('.') + 1);
                    if (fractionalText.Contains("E"))
                    {
                        fractionalText = fractionalText.Substring(0, fractionalText.IndexOf('E'));
                        if (fractionalText.Contains("00"))
                        {
                            fractionalText = fractionalText.Substring(0, fractionalText.LastIndexOf("00"));
                            while (fractionalText.EndsWith("0"))
                            {
                                fractionalText = fractionalText.Substring(0, fractionalText.Length - 1);
                            }
                        }
                    }
                }
                return "点" + Text_numberToChinese_sample(fractionalText);
            }
            public static string Text_numberToChinese_sample(char numChar)
            {
                switch (numChar)
                {
                    case '.': return "点";
                    case '0': return "零";
                    case '1': return "一";
                    case '2': return "二";
                    case '3': return "三";
                    case '4': return "四";
                    case '5': return "五";
                    case '6': return "六";
                    case '7': return "七";
                    case '8': return "八";
                    case '9': return "九";
                }
                return numChar + "";
            }
            public static string Text_numberToChinese_sample(string numText)
            {
                numText = numText.Replace(".", "点");
                numText = numText.Replace("0", "零");
                numText = numText.Replace("1", "一");
                numText = numText.Replace("2", "二");
                numText = numText.Replace("3", "三");
                numText = numText.Replace("4", "四");
                numText = numText.Replace("5", "五");
                numText = numText.Replace("6", "六");
                numText = numText.Replace("7", "七");
                numText = numText.Replace("8", "八");
                numText = numText.Replace("9", "九");
                return numText;
            }
            private static string _Text_numberToChinese_single(int num)
            {
                if (num > 9) throw new Exception(num + " is too big for a single char");

                switch (num)
                {
                    case 0: return "零";
                    case 1: return "一";
                    case 2: return "二";
                    case 3: return "三";
                    case 4: return "四";
                    case 5: return "五";
                    case 6: return "六";
                    case 7: return "七";
                    case 8: return "八";
                    case 9: return "九";
                }
                return null;
            }
            private static string _Text_numberToChinese_smallUnit(int unit)
            {
                if (unit <= 0) throw new Exception(unit + " is too small for smallUnit");
                else if (unit > 3) throw new Exception(unit + " is too big for smallUnit");
                switch (unit)
                {
                    case 1: return "十";
                    case 2: return "百";
                    case 3: return "千";
                }
                return null;
            }
            private static string _Text_numberToChinese_bigUnit(int unit4)
            {
                if (unit4 <= 0) throw new Exception(unit4 + " is too small for obtaining a bigUnit");

                switch (unit4)
                {
                    case 1: return "萬"; //10^4
                    case 2: return "億"; //10^8
                    case 3: return "兆"; //10^12
                    case 4: return "京"; //10^16
                    case 5: return "垓"; //10^20
                    case 6: return "杼"; //10^24
                    case 7: return "穰"; //10^28
                    case 8: return "溝"; //10^32
                    case 9: return "澗"; //10^36
                    case 10: return "正"; //10^40
                    case 11: return "載"; //10^44
                    case 12: return "極"; //10^48
                    case 13: return "恆河沙"; //10^52
                    case 14: return "阿僧祇"; //10^56
                    case 15: return "那由他"; //10^60
                    case 16: return "不可思議"; //10^64
                    case 17: return "無量"; //10^68
                    case 18: return "大數"; //10^72
                }

                throw new Exception("TOO BIG!! It's over 10^72 ! " + unit4 + "*4");
            }


            public class ReplaceNumToChinese_pair
            {
                public string text;
                public bool? isNum;
            }
            public static string ReplaceNumToChinese(string source)
            {
                bool numStart = false;
                char c;
                List<ReplaceNumToChinese_pair> pairList = new List<ReplaceNumToChinese_pair>();
                ReplaceNumToChinese_pair? pair = null;
                for (int i = 0; i < source.Length; i++)
                {
                    c = source[i];
                    if (i == 0)
                    {
                        pair = new ReplaceNumToChinese_pair()
                        {
                            text = c + "",
                        };
                        pairList.Add(pair);
                    }

                    if (c >= '0' && c <= '9')
                    {
                        if (numStart == false)
                        {
                            if (i != 0)
                            {
                                pair = new ReplaceNumToChinese_pair
                                {
                                    text = c + "",
                                };
                                pairList.Add(pair);
                            }
                        }
                        else
                        {
                            ((ReplaceNumToChinese_pair)pair).text += c + "";
                        }
                        numStart = true;
                    }
                    else if (c == '.')
                    {
                        if (numStart == true)
                        {
                            pair.text += c + "";
                        }
                        else
                        {
                            if (i != 0)
                            {
                                pair = new ReplaceNumToChinese_pair
                                {
                                    text = c + "",
                                };
                                pairList.Add(pair);
                            }
                        }
                    }
                    else
                    {
                        if (numStart == true)
                        {
                            pair = new ReplaceNumToChinese_pair
                            {
                                text = c + "",
                            };
                            pairList.Add(pair);
                        }
                        else
                        {
                            if (i != 0)
                            {
                                ((ReplaceNumToChinese_pair)pair).text += c + "";
                            }
                        }
                        numStart = false;
                    }
                }

                for (int i = pairList.Count - 1; i >= 0; i--)
                {
                    pair = pairList[i];
                    c = pair.text[0];
                    if (c >= '0' && c <= '9')
                    {
                        if (pair.text.Contains("."))
                        {
                            if (pair.text.IndexOf('.') != pair.text.LastIndexOf('.'))
                            {
                                pair.isNum = null;
                            }
                            else
                            {
                                if (pair.text.IndexOf('.') == pair.text.Length - 1)
                                {
                                    pair.text = pair.text.Substring(0, pair.text.Length - 1);
                                }
                                pair.isNum = true;
                            }
                        }
                        else pair.isNum = true;
                    }
                    else pair.isNum = false;
                }

                // 将片段合并， 数字的 转化 成汉字，不是数字的直接合并
                string result = "";
                for (int i = 0; i < pairList.Count; i++)
                {
                    pair = pairList[i];
                    if (pair.isNum == null)
                    {
                        result += MultiVoice.TextHelper.Text_numberToChinese_sample(pair.text);
                    }
                    else if (pair.isNum == true)
                    {
                        result += MultiVoice.TextHelper.Text_numberToChinese(pair.text);
                    }
                    else result += pair.text;
                }
                return result;
            }
        }
    }
}
