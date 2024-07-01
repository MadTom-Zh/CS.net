using System;

using SpeechLib;
using System.IO;
using System.Collections.Generic;

namespace MadTomDev.Resources.T2S_HCore
{
    public class BroadCaster : IDisposable
    {
        public static BroadCaster? instance;
        public static BroadCaster GetInstance()
        {
            if (instance == null)
            {
                instance = new BroadCaster();
            }
            return instance;
        }

        private SpVoice spVoice;
        public BroadCaster()
        {
            spVoice = new SpVoice();
            spVoice.StartStream += new _ISpeechVoiceEvents_StartStreamEventHandler(spVoice_StartStream);
            spVoice.EndStream += new _ISpeechVoiceEvents_EndStreamEventHandler(spVoice_EndStream);
            spVoice.Word += new _ISpeechVoiceEvents_WordEventHandler(spVoice_Word);
            spVoice.Sentence += new _ISpeechVoiceEvents_SentenceEventHandler(spVoice_Sentence);
            spVoice.Viseme += new _ISpeechVoiceEvents_VisemeEventHandler(spVoice_Viseme);

            defaultAudioOutputStream = spVoice.AudioOutputStream;

            ISpeechObjectTokens voices;
            voices = spVoice.GetVoices("", "");
            _iVoices = new List<SpObjectToken>();
            foreach (SpObjectToken voice in voices)
            {
                _iVoices.Add(voice);
            }
            _Voices = new List<Voice>();
            for (int i = 0; i < _iVoices.Count; i++)
            {
                _Voices.Add(new Voice(_iVoices[i].Id, _iVoices[i].GetDescription()));
            }
        }
        //private bool isDisposed = false;
        //public void Dispose() 
        //{
        //    if (isDisposed) return;

        //    spVoice = null;
        //    instance = null;
        //    GC.Collect();
        //    GC.f


        //    isDisposed = true;
        //}

        public void Dispose()
        {
            //spVoice = null;
            //instance = null;
            GC.SuppressFinalize(this);
        }

        internal bool HaveVoice(string voiceName)
        {
            Voice? foundVoice = Voices.Find(i => i.description == voiceName);
            return foundVoice != null;
        }

        public delegate void StreamStartDelegate(BroadCaster sender);
        public event StreamStartDelegate StreamStart;
        public class StreamEndAndJITListGendEventArgs : EventArgs
        {
            public StreamEndAndJITListGendEventArgs(List<JITEvent> completeJITEventList)
            {
                this.completeJITEventList = completeJITEventList;
                if (completeJITEventList.Count > 0)
                {
                    long basePosi = completeJITEventList[0].streamPosi;
                    for (int i = completeJITEventList.Count - 1; i >= 0; i--)
                    {
                        completeJITEventList[i].streamPosi -= basePosi;
                    }
                }
            }
            public List<JITEvent> completeJITEventList;
        }
        public event EventHandler<StreamEnd_Args> StreamEnd;
        public class StreamEnd_Args : EventArgs
        {
            public StreamEnd_Args(bool isUserStop)
            {
                _isUserStop = isUserStop;
            }
            private bool _isUserStop;
            public bool IsUserStop
            {
                get
                {
                    return _isUserStop;
                }
            }
        }
        public event EventHandler<StreamEndAndJITListGendEventArgs> StreamEndAndJITListGend;
        private List<JITEvent> _JITEventList = new();
        private bool _streamStarted = false;
        void spVoice_StartStream(int StreamNumber, object StreamPosition)
        {
            _Speeking_SentenceBoundary_readingSentenceIndex = 0;
            _streamStarted = true;
            _JITEventList = new List<JITEvent>();
            //throw new NotImplementedException();
            StreamStart?.Invoke(this);
        }
        void spVoice_EndStream(int StreamNumber, object StreamPosition)
        {
            _streamStarted = false;
            //throw new NotImplementedException();
            if (!_isUserStop && !_isRateUserChanged)
            {
                StreamEndAndJITListGend?.Invoke(this, new StreamEndAndJITListGendEventArgs(_JITEventList));
            }

            if (WorkingStateChanged != null)
            {
                WorkingStateChanged(
                    this,
                    new WorkingStateChanged_args()
                    {
                        stateOld = stateOld,
                        stateNew = States.Ready_Stoped
                    }
                    );
                stateOld = States.Ready_Stoped;
            }

            StreamEnd?.Invoke(this, new StreamEnd_Args(_isUserStop));
            _isUserStop = false;
        }

        public class Voice
        {
            private string _id;
            public string id
            {
                get { return _id; }
            }
            private string _description;
            public string description
            {
                get { return _description; }
            }
            public Voice(string id, string description)
            {
                _id = id;
                _description = description;
            }
        }
        private List<Voice> _Voices;
        public List<Voice> Voices
        {
            get
            {
                return _Voices;
            }
        }
        private List<SpObjectToken> _iVoices;
        private Voice? _voice;// = null;
        public Voice voice
        {
            get
            {
                if (_voice == null)
                {
                    for (int i = _Voices.Count - 1; i >= 0; i--)
                    {
                        if (spVoice.Voice.Id == _Voices[i].id)
                        {
                            _voice = _Voices[i];
                            break;
                        }
                    }
                }
                return _voice;
            }
            set
            {
                if (_voice != value)
                {
                    _voice = value;
                    for (int i = _Voices.Count - 1; i >= 0; i--)
                    {
                        if (_Voices[i].id == _voice.id)
                        {
                            spVoice.Voice = _iVoices[i];
                            return;
                        }
                    }
                    string exceptionStr = "No voice with id [" + _voice.id + "], description [" + _voice.description + "] exists!";
                    _voice = null;
                    throw new Exception(exceptionStr);
                }
            }
        }

        private bool _isRateUserChanged = false;
        public int Rate
        {
            get
            {
                return spVoice.Rate;
            }
            set
            {
                if (value < -10 || value > 10)
                {
                    throw new Exception("Rate should between -10 to 10.");
                }
                _isRateUserChanged = true;
                spVoice.Rate = value;
            }
        }

        public int Volume
        {
            get
            {
                return spVoice.Volume;
            }
            set
            {
                if (value < 0 || value > 100)
                {
                    throw new Exception("Volume should between 0 to 100.");
                }
                spVoice.Volume = value;
            }
        }

        private string _speekText;
        private int _speekText_length;
        //public event EventHandler SpeekBuffered;

        public void Speek(string text)
        {
            if (MultiVoice.TextHelper.Text_CheckIfOnlySymbols(text) == true)
            {
                return;
            }
            if (WorkingStateChanged != null)
            {
                WorkingStateChanged(
                    this,
                    new WorkingStateChanged_args()
                    {
                        stateOld = stateOld,
                        stateNew = States.Playing
                    }
                    );
                stateOld = States.Playing;
            }

            spVoice.AudioOutputStream = defaultAudioOutputStream;
            _Speeking_SentenceBoundary_readingSentenceIndex = -1;
            _speekText = text;
            _speekText_length = _speekText.Length;

            try
            {
                spVoice.Speak(_speekText, SpeechVoiceSpeakFlags.SVSFlagsAsync);
            }
            catch (Exception err)
            {
                throw err;
            }

        }
        public bool isPlaying
        {
            get
            {
                return (_streamStarted == true);
            }
        }


        private bool _isUserStop = false;
        public void Stop()
        {
            if (isPlaying == true)
            {
                _isUserStop = true;
            }
            _isRateUserChanged = false;
            spVoice.Speak(null, SpeechVoiceSpeakFlags.SVSFPurgeBeforeSpeak);
            _streamStarted = false;

            if (WorkingStateChanged != null)
            {
                WorkingStateChanged(
                    this,
                    new WorkingStateChanged_args()
                    {
                        stateOld = stateOld,
                        stateNew = States.Ready_Stoped
                    }
                    );
                stateOld = States.Ready_Stoped;
            }
        }

        #region voice Boundary
        public class Speeking_WordBoundaryEventArgs : EventArgs
        {
            public Speeking_WordBoundaryEventArgs(
                long steamPosition,
                int wordStartIndex,
                int wordLength,
                string word
                )
            {
                this.streamPosition = steamPosition;
                this.wordStartIndex = wordStartIndex;
                this.wordLength = wordLength;
                this.word = word;
            }
            public long streamPosition;
            public int wordStartIndex;
            public int wordLength;
            public string word;
        }
        public event EventHandler<Speeking_WordBoundaryEventArgs>? Speeking_WordBoundary;
        void spVoice_Word(int StreamNumber, object StreamPosition, int CharacterPosition, int Length)
        {
            //throw new NotImplementedException();
            long steamPosi = long.Parse(StreamPosition.ToString());
            _JITEventList.Add(new JITEvent(
                JITEvent.EType.Word,
                steamPosi,
                CharacterPosition,
                Length,
                0, 0, 0, 0));
            if (Speeking_WordBoundary != null)
            {
                // sometimes it just got wrong... so i correct it here
                if (CharacterPosition + Length > _speekText.Length)
                {
                    if (CharacterPosition < _speekText.Length)
                    {
                        Length = _speekText.Length - CharacterPosition;
                    }
                    else
                    {
                        CharacterPosition = _speekText.Length - 1;
                        Length = 1;
                    }
                }

                Speeking_WordBoundaryEventArgs swbEA
                    = new
                    (
                        steamPosi,
                        CharacterPosition,
                        Length,
                        _speekText.Substring(CharacterPosition, Length)
                    );
                Speeking_WordBoundary(this, swbEA);
            }
        }

        private int _Speeking_SentenceBoundary_readingSentenceIndex = 0;
        public class Speeking_SentenceBoundaryEventArgs : EventArgs
        {
            public Speeking_SentenceBoundaryEventArgs(
                long streamPosition,
                int readingSentenceIndex,
                string sentence,
                int sentenceStartIndex,
                int sentenceLength
                )
            {
                this.streamPosition = streamPosition;
                this.readingSentenceIndex = readingSentenceIndex;
                this.sentence = sentence;
                this.sentenceStartIndex = sentenceStartIndex;
                this.sentenceLength = sentenceLength;
            }
            public long streamPosition;
            public int readingSentenceIndex;
            public string sentence;
            public int sentenceStartIndex;
            public int sentenceLength;
        }
        public event EventHandler<Speeking_SentenceBoundaryEventArgs>? Speeking_SentenceBoundary;
        void spVoice_Sentence(int StreamNumber, object StreamPosition, int CharacterPosition, int Length)
        {
            //throw new NotImplementedException();
            long streamPosi = long.Parse(StreamPosition.ToString());
            if (_speekText_length < (CharacterPosition + Length))
            {
                Length = _speekText_length - CharacterPosition;
            }
            //string speakingText = _speekText.Substring(CharacterPosition, Length).Trim();
            //Length = speakingText.Length;
            _JITEventList.Add(new JITEvent(
                JITEvent.EType.Sentence,
                streamPosi,
                CharacterPosition,
                Length,
                0, 0, 0, 0));
            if (Speeking_SentenceBoundary != null)
            {
                Speeking_SentenceBoundaryEventArgs ssbEA
                    = new(
                        streamPosi,
                        _Speeking_SentenceBoundary_readingSentenceIndex,
                        _speekText.Substring(CharacterPosition, Length),  //speakingText
                        CharacterPosition,
                        Length
                        );
                Speeking_SentenceBoundary(this, ssbEA);
            }
            _Speeking_SentenceBoundary_readingSentenceIndex++;
        }

        public class Speeking_VisemeEventArgs : EventArgs
        {
            public Speeking_VisemeEventArgs(
                long streamPosition,
                int duration,
                int id,
                int nextId,
                int feature
                )
            {
                this.streamPosition = streamPosition;
                this.duration = duration;
                this.id = id;
                this.nextId = nextId;
                this.feature = feature;
            }
            public long streamPosition;
            public int duration;
            public int id;
            public int nextId;
            public int feature;
        }
        public event EventHandler<Speeking_VisemeEventArgs> Speeking_Viseme;
        void spVoice_Viseme(int StreamNumber,
            object StreamPosition,
            int Duration,
            SpeechVisemeType NextVisemeId,
            SpeechVisemeFeature Feature,
            SpeechVisemeType CurrentVisemeId)
        {
            //throw new NotImplementedException();
            long steamPosi = long.Parse(StreamPosition.ToString());
            int currentVisemeId = (int)CurrentVisemeId;
            int nextVisemeId = (int)NextVisemeId;
            int feature = (int)Feature;
            _JITEventList.Add(
                new JITEvent(
                    JITEvent.EType.Viseme,
                    steamPosi,
                    0,
                    0,
                    currentVisemeId,
                    feature,
                    nextVisemeId,
                    Duration));
            if (Speeking_Viseme != null)
            {
                Speeking_VisemeEventArgs svEA
                    = new(
                        steamPosi,
                        Duration,
                        currentVisemeId,
                        nextVisemeId,
                        feature
                        );
                Speeking_Viseme(this, svEA);
            }
        }
        #endregion


        private ISpeechBaseStream defaultAudioOutputStream;
        //public void SaveWave(string text, string fileFullName)
        //{
        //    defaultAudioOutputStream = spVoice.AudioOutputStream;

        //    SpMemoryStream spMemStream = new SpMemoryStream();
        //    spVoice.AudioOutputStream = spMemStream;

        //    SpAudioFormat audioFormat = new SpAudioFormat();
        //    audioFormat.Type = SpeechAudioFormatType.SAFT48kHz16BitStereo;
        //    spMemStream.Format = audioFormat;

        //    spVoice.Speak(text, SpeechVoiceSpeakFlags.SVSFlagsAsync);
        //    spVoice.WaitUntilDone(-1);

        //    spMemStream.Seek(0, SpeechStreamSeekPositionType.SSSPTRelativeToStart);
        //    //spMemStream.
        //    object buf = spMemStream.GetData();
        //    byte[] b = (byte[])buf;
        //    File.WriteAllBytes(fileFullName, b);


        //    //MemoryStream memoryStream = new MemoryStream();
        //    //memoryStream.Write(b, 0, b.Length);
        //    //memoryStream.g

        //    spVoice.AudioOutputStream = defaultAudioOutputStream;

        //}

        private bool _isSavingWave = false;
        public bool isSavingWave
        {
            get
            {
                return _isSavingWave;
            }
        }
        public event EventHandler SaveWave_FStream_Complete;
        public void SaveWave_FStream(string text, string fileFullName)
        {
            if (MultiVoice.TextHelper.Text_CheckIfOnlySymbols(text) == true)
            {
                return;
            }
            //_isBusy = true;
            //if (WorkingStateChanged != null)
            //{
            //    WorkingStateChanged(
            //        this,
            //        new WorkingStateChanged_args()
            //        {
            //            stateOld = stateOld,
            //            stateNew = States.SavingFile
            //        }
            //        );
            //    stateOld = States.SavingFile;
            //}
            //defaultAudioOutputStream = spVoice.AudioOutputStream;

            _isSavingWave = true;
            SpFileStream spFileStream = new();
            spFileStream.Format.Type = SpeechAudioFormatType.SAFT48kHz16BitStereo;
            spFileStream.Open(fileFullName, SpeechStreamFileMode.SSFMCreateForWrite, false);

            spVoice.AudioOutputStream = spFileStream;
            //spVoice.AudioOutputStream.Format.Type = SpeechAudioFormatType.SAFT48kHz16BitMono;

            //SpAudioFormat audioFormat = new SpAudioFormat();
            //audioFormat.GetWaveFormatEx();
            //audioFormat.Type = SpeechAudioFormatType.SAFT48kHz16BitMono;
            //spFileStream.Format = audioFormat;

            // default: 22KHz, 16bit, PCM 352kbps

            spVoice.Speak(text, SpeechVoiceSpeakFlags.SVSFlagsAsync);
            spVoice.WaitUntilDone(-1);

            spVoice.AudioOutputStream = defaultAudioOutputStream;

            //spFileStream.Seek(0, SpeechStreamSeekPositionType.SSSPTRelativeToStart);
            spFileStream.Close();

            _isSavingWave = false;

            //_isBusy = false;
            //if (SaveWave_FStream_Complete != null)
            //{
            //    SaveWave_FStream_Complete(this, null);
            //}
            //if (WorkingStateChanged != null)
            //{
            //    WorkingStateChanged(
            //        this,
            //        new WorkingStateChanged_args()
            //        {
            //            stateOld = stateOld,
            //            stateNew = States.Ready_Stoped
            //        }
            //        );
            //    stateOld = States.Ready_Stoped;
            //}

            SaveWave_FStream_Complete?.Invoke(this, new EventArgs());
        }

        public void SaveWave_FStream_MultiLangInOne
            (List<MultiVoice.TextMultiFragments.Fragment> textFragments,
            MultiVoice.VoiceLanguageLib voiceLangLib,
            string singleFileFullName
            )
        {
            MultiVoice.TextMultiFragments.Fragment frag;
            for (int i = textFragments.Count - 1; i >= 0; i--)
            {
                frag = textFragments[i];
                if (MultiVoice.TextHelper.Text_CheckIfOnlySymbols(frag.fgValue) == true)
                {
                    textFragments.RemoveAt(i);
                }
            }
            if (textFragments.Count <= 0)
            {
                return;
            }

            _isSavingWave = true;
            SpFileStream spFileStream = new();
            spFileStream.Format.Type = SpeechAudioFormatType.SAFT48kHz16BitStereo;
            if (File.Exists(singleFileFullName)) File.Delete(singleFileFullName);
            spFileStream.Open(singleFileFullName, SpeechStreamFileMode.SSFMCreateForWrite, false);

            spVoice.AudioOutputStream = spFileStream;
            Speach_Black(spVoice.AudioOutputStream, 500);

            Voice? targetVoice = null;
            MultiVoice.VoiceLanguageLib.VLLItem? vllItem = null;
            for (int i = 0; i < textFragments.Count; i++)
            {
                frag = textFragments[i];
                vllItem = FindVLLItem(frag.fgLang, voiceLangLib, out targetVoice);
                if (targetVoice != null)
                {
                    voice = targetVoice;
                }
                if (vllItem != null)
                {
                    Rate = vllItem.CustomSpeed;
                    Volume = vllItem.CustomVolume;
                }
                spVoice.Speak(frag.fgValue, SpeechVoiceSpeakFlags.SVSFlagsAsync);
                spVoice.WaitUntilDone(-1);
            }

            Speach_Black(spVoice.AudioOutputStream, 500);

            spVoice.AudioOutputStream = defaultAudioOutputStream;

            spFileStream.Close();

            _isSavingWave = false;

            if (SaveWave_FStream_Complete != null)
            {
                SaveWave_FStream_Complete(this, new EventArgs());
            }
        }
        //public static bool Text_CheckIfContainsChinese(string text)
        //{
        //    return Regex.IsMatch(text, @"^.*[\u4e00-\u9fa5].*$"); //存在中文
        //}
        private void Speach_Black(ISpeechBaseStream stream, long miliseconds)
        {
            object buf = new byte[Stream_BytePreMilisec * miliseconds];
            stream.Write(buf);
        }
        private void Speach_Black(ISpeechBaseStream stream, TimeSpan timeSpan)
        {
            Speach_Black(stream, (long)(timeSpan.TotalMilliseconds + 0.5));
        }
        // for SpeechAudioFormatType.SAFT48kHz16BitStereo;
        // it's not accurate
        private static int stream_BytePreMilisec = 210;
        public static int Stream_BytePreMilisec
        {
            set
            {
                stream_BytePreMilisec = value;
            }
            get
            {
                return stream_BytePreMilisec;
            }
        }
        public static long Calculate_StreamLength(TimeSpan timeSpan)
        {
            long result = (long)Stream_BytePreMilisec;
            result *= (long)(timeSpan.TotalMilliseconds + 0.5);
            return result;
        }

        public class OnExceptionArgs : EventArgs
        {
            public Exception err;
        }
        public event EventHandler<OnExceptionArgs> OnException;
        private MultiVoice.VoiceLanguageLib.VLLItem FindVLLItem
            (
            MultiVoice.LanguageType langType,
            MultiVoice.VoiceLanguageLib inVoiceLangLib,
            out Voice? targetBCVoice
            )
        {
            MultiVoice.VoiceLanguageLib.VLLItem? result = null;
            targetBCVoice = null;
            if (langType.ToString() == "General")
            {
                result = inVoiceLangLib.lib[0];
            }
            else
            {
                foreach (MultiVoice.VoiceLanguageLib.VLLItem item in inVoiceLangLib.lib)
                {
                    if (item.Language == langType)
                    {
                        result = item;
                        break;
                    }
                }
            }
            if (result == null)
            {
                if (OnException != null)
                {
                    OnException
                        (
                        this,
                        new OnExceptionArgs() { err = new Exception("No Language(" + langType + ") found in VoiceLangLib!") }
                        );
                }
                return null;
            }
            foreach (Voice v in Voices)
            {
                if (v.description == result.VoiceName)
                {
                    targetBCVoice = v;
                    break;
                }
            }
            if (targetBCVoice == null)
            {
                if (OnException != null)
                {
                    OnException
                        (
                        this,
                        new OnExceptionArgs() { err = new Exception("No Voice found in BroadCaster!") }
                        );
                }
                return null;
            }
            return result;
        }

        public bool IsSpeaking
        {
            get
            {
                return (spVoice.Status.RunningState == SpeechRunState.SRSEIsSpeaking);
            }
        }
        public enum States
        {
            Playing,
            //Paused,
            Ready_Stoped,
            SavingFile,
        }
        private States stateOld = States.Ready_Stoped;
        public class WorkingStateChanged_args : EventArgs
        {
            public States stateOld;
            public States stateNew;
        }
        public event EventHandler<WorkingStateChanged_args> WorkingStateChanged;
    }
}
