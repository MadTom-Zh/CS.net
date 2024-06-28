using System;
using System.Threading;

namespace MadTomDev.Common
{

    public class MillisecondTimer : IDisposable
    {
        private static MillisecondTimer instance = null;
        public static MillisecondTimer GetInstance()
        {
            if (instance == null)
            {
                instance = new MillisecondTimer();
            }
            return instance;
        }
        public static MillisecondTimer New()
        {
            MillisecondTimer newTimer = new MillisecondTimer();
            return newTimer;
        }

        public void Dispose()
        {
            flagStop = true;
        }

        public delegate void TickDelegate(MillisecondTimer sender, DateTime tickTime, bool is500ms, bool is1000ms);
        public event TickDelegate Tick;

        public enum LoopTypes
        { RealTime, Wait1ms, Wait2ms, Wait4ms, Wait8ms, }

        private int Loop500msERate = 1;
        private LoopTypes _LoopType = LoopTypes.Wait2ms;
        public LoopTypes LoopType
        {
            set
            {
                _LoopType = value;
                switch (value)
                {
                    case LoopTypes.RealTime: Loop500msERate = 0; break;
                    case LoopTypes.Wait1ms: Loop500msERate = 0; break;
                    case LoopTypes.Wait2ms: Loop500msERate = 1; break;
                    case LoopTypes.Wait4ms: Loop500msERate = 2; break;
                    case LoopTypes.Wait8ms: Loop500msERate = 4; break;
                }
            }
            get => _LoopType;
        }

        public enum Intervals
        { Stoped, ms10, ms100, ms500, ms1000 }
        private Intervals _Interval = Intervals.Stoped;
        public Intervals Interval
        {
            get => _Interval;
            set
            {
                _Interval = value;
                switch (value)
                {
                    case Intervals.Stoped:
                        intInterval = -1; break;
                    case Intervals.ms10:
                        intInterval = 10; break;
                    case Intervals.ms100:
                        intInterval = 100; break;
                    case Intervals.ms500:
                        intInterval = 500; break;
                    case Intervals.ms1000:
                        intInterval = 1000; break;
                }
                DateTime now = DateTime.Now;
                preS = now.Second;
                preMs = now.Millisecond;
                preMsItv = preMs / ((intInterval == 0) ? 10 : intInterval);
            }
        }

        private DateTime tmpTime1 = DateTime.Now, now;
        private int curMs, preMs, curMsItv, preMsItv, intInterval = -1;
        private int curS, preS;

        private bool flagStop = false;
        private bool flagTick_500ms_1kms = false;
        private MillisecondTimer()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback((s) =>
            {
                bool notTicked;
                while (true)
                {
                    if (flagStop)
                        break;
                    if (intInterval <= 0)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    switch (LoopType)
                    {
                        case LoopTypes.Wait1ms:
                            Thread.Sleep(1); break;
                        case LoopTypes.Wait2ms:
                            Thread.Sleep(2); break;
                        case LoopTypes.Wait4ms:
                            Thread.Sleep(4); break;
                        case LoopTypes.Wait8ms:
                            Thread.Sleep(8); break;
                    }

                    now = DateTime.Now;
                    if (intInterval < 1000)
                    {
                        curMs = now.Millisecond;
                        curMsItv = curMs / intInterval;
                        if (preMs != curMs)
                        {
                            if (curMsItv != preMsItv)
                            {
                                notTicked = true;
                                if (flagTick_500ms_1kms)
                                {
                                    if (1000 - Loop500msERate <= curMs || 0 <= curMs )
                                    {
                                        flagTick_500ms_1kms = false;
                                        Tick?.Invoke(this, now, false, true);
                                        notTicked = false;
                                    }
                                }
                                else
                                {
                                    if (curMs - Loop500msERate <= curMs)
                                    {
                                        flagTick_500ms_1kms = true;
                                        Tick?.Invoke(this, now, true, false);
                                        notTicked = false;
                                    }
                                }
                                if (notTicked)
                                {
                                    Tick?.Invoke(this, now, false, false);
                                }
                            }
                        }

                        preMs = now.Millisecond;
                        preMsItv = curMsItv;
                    }
                    else // ms1000
                    {
                        curS = now.Second;
                        if (preS != curS)
                        {
                            Tick?.Invoke(this, now, true, true);
                        }
                        preS = curS;
                    }
                }
            }));
        }
    }
}