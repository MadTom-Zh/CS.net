using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Color = System.Windows.Media.Color;

namespace MadTomDev.App
{
    class Settings : Data.SettingsTxt
    {
        private Settings() { }
        private static Settings instance = null;
        public new static Settings GetInstance()
        {
            if (instance == null)
            {
                instance = new Settings();
                string dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "MadTomDev");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                instance.SettingsFileFullName = Path.Combine(dir, "QuickView.txt");
                instance.ReLoad();
            }
            return instance;
        }

        private double? _PlaySpeed = null;
        public double PlaySpeed
        {
            set
            {
                _PlaySpeed = value;
                base["PlaySpeed"] = value.ToString();
            }
            get
            {
                if (_PlaySpeed == null)
                {
                    if (double.TryParse(base["PlaySpeed"], out double v))
                    {
                        _PlaySpeed = v;
                    }
                    else
                    {
                        _PlaySpeed = 1.0;
                        base["PlaySpeed"] = "1.0";
                    }
                }
                return (double)_PlaySpeed;
            }
        }
        private double? _PlayVolume = null;
        public double PlayVolume
        {
            set
            {
                _PlayVolume = value;
                base["PlayVolume"] = value.ToString();
            }
            get
            {
                if (_PlayVolume == null)
                {
                    if (double.TryParse(base["PlayVolume"], out double v))
                    {
                        _PlayVolume = v;
                    }
                    else
                    {
                        _PlayVolume = 0.3;
                        base["PlayVolume"] = "0.3";
                    }
                }
                return (double)_PlayVolume;
            }
        }

        private bool? _CmnLoopSubs = null;
        public bool CmnLoopSubs
        {
            set
            {
                _CmnLoopSubs = value;
                base["CmnLoopSubs"] = value.ToString();
            }
            get
            {
                if (_CmnLoopSubs == null)
                {
                    if (bool.TryParse(base["CmnLoopSubs"], out bool v))
                    {
                        _CmnLoopSubs = v;
                    }
                    else
                    {
                        _CmnLoopSubs = true;
                        base["CmnLoopSubs"] = true.ToString();
                    }
                }
                return _CmnLoopSubs == true;
            }
        }
        private bool? _CmnErrorMsgBox = null;
        public bool CmnErrorMsgBox
        {
            set
            {
                _CmnErrorMsgBox = value;
                base["CmnErrorMsgBox"] = value.ToString();
            }
            get
            {
                if (_CmnErrorMsgBox == null)
                {
                    if (bool.TryParse(base["CmnErrorMsgBox"], out bool v))
                    {
                        _CmnErrorMsgBox = v;
                    }
                    else
                    {
                        _CmnErrorMsgBox = true;
                        base["CmnErrorMsgBox"] = true.ToString();
                    }
                }
                return _CmnErrorMsgBox == true;
            }
        }

        private Key? _ViewPrevKey;
        public Key ViewPrevKey
        {
            set
            {
                _ViewPrevKey = value;
                base["ViewPrevKey"] = value.ToString();
            }
            get
            {
                if (_ViewPrevKey == null)
                {
                    if (Key.TryParse(base["ViewPrevKey"], out Key v))
                    {
                        _ViewPrevKey = v;
                    }
                    else
                    {
                        _ViewPrevKey = Key.Left;
                        base["ViewPrevKey"] = _ViewPrevKey.ToString();
                    }
                }
                return (Key)_ViewPrevKey;
            }
        }
        private Key? _ViewNextKey;
        public Key ViewNextKey
        {
            set
            {
                _ViewNextKey = value;
                base["ViewNextKey"] = value.ToString();
            }
            get
            {
                if (_ViewNextKey == null)
                {
                    if (Key.TryParse(base["ViewNextKey"], out Key v))
                    {
                        _ViewNextKey = v;
                    }
                    else
                    {
                        _ViewNextKey = Key.Right;
                        base["ViewNextKey"] = _ViewNextKey.ToString();
                    }
                }
                return (Key)_ViewNextKey;
            }
        }
        private Key? _Sort0Key;
        public Key Sort0Key
        {
            set
            {
                _Sort0Key = value;
                base["Sort0Key"] = value.ToString();
            }
            get
            {
                if (_Sort0Key == null)
                {
                    if (Key.TryParse(base["Sort0Key"], out Key v))
                    {
                        _Sort0Key = v;
                    }
                    else
                    {
                        _Sort0Key = Key.Down;
                        base["Sort0Key"] = _Sort0Key.ToString();
                    }
                }
                return (Key)_Sort0Key;
            }
        }
        private Key? _Sort1Key;
        public Key Sort1Key
        {
            set
            {
                _Sort1Key = value;
                base["Sort1Key"] = value.ToString();
            }
            get
            {
                if (_Sort1Key == null)
                {
                    if (Key.TryParse(base["Sort1Key"], out Key v))
                    {
                        _Sort1Key = v;
                    }
                    else
                    {
                        _Sort1Key = Key.Up;
                        base["Sort1Key"] = _Sort1Key.ToString();
                    }
                }
                return (Key)_Sort1Key;
            }
        }

        private string _Sort0DirName;
        public string Sort0DirName
        {
            set
            {
                _Sort0DirName = value;
                base["Sort0DirName"] = value;
            }
            get
            {
                if (_Sort0DirName == null)
                {
                    _Sort0DirName = "0";
                    base["Sort0DirName"] = _Sort0DirName;
                }
                return _Sort0DirName;
            }
        }
        private string _Sort1DirName;
        public string Sort1DirName
        {
            set
            {
                _Sort1DirName = value;
                base["Sort1DirName"] = value;
            }
            get
            {
                if (_Sort1DirName == null)
                {
                    _Sort1DirName = "1";
                    base["Sort1DirName"] = _Sort1DirName;
                }
                return _Sort1DirName;
            }
        }

        private int _ViewThumbnailCount = -1;
        public int ViewThumbnailCount
        {
            set
            {
                _ViewThumbnailCount = value;
                base["ViewThumbnailCount"] = value.ToString();
            }
            get
            {
                if (_ViewThumbnailCount < 0)
                {
                    if (int.TryParse(base["ViewThumbnailCount"], out int v))
                    {
                        _ViewThumbnailCount = v;
                    }
                    else
                    {
                        _ViewThumbnailCount = 8;
                        base["ViewThumbnailCount"] = _ViewThumbnailCount.ToString();
                    }
                }
                return _ViewThumbnailCount;
            }
        }

        private int _ViewHistoryCount = -1;
        public int ViewHistoryCount
        {
            set
            {
                _ViewHistoryCount = value;
                base["ViewHistoryCount"] = value.ToString();
            }
            get
            {
                if (_ViewHistoryCount < 0)
                {
                    if (int.TryParse(base["ViewHistoryCount"], out int v))
                    {
                        _ViewHistoryCount = v;
                    }
                    else
                    {
                        _ViewHistoryCount = 10;
                        base["ViewHistoryCount"] = _ViewHistoryCount.ToString();
                    }
                }
                return _ViewHistoryCount;
            }
        }

        public int _StillWaitMin = -1;
        public int StillWaitMin
        {
            set
            {
                _StillWaitMin = value;
                base["StillWaitMin"] = value.ToString();
            }
            get
            {
                if (_StillWaitMin < 0)
                {
                    if (int.TryParse(base["StillWaitMin"], out int v))
                    {
                        _StillWaitMin = v;
                    }
                    else
                    {
                        _StillWaitMin = 5;
                        base["StillWaitMin"] = _StillWaitMin.ToString();
                    }
                }
                return _StillWaitMin;
            }
        }

        private Color _BG = Colors.Transparent;
        public Color BG
        {
            set
            {
                _BG = value;
                base["BG"] = value.A + "," + value.R + "," + value.G + "," + value.B;
            }
            get
            {
                bool vOk = false;
                string raw = base["BG"];
                if (!string.IsNullOrWhiteSpace(raw))
                {
                    try
                    {
                        string[] parts = raw.Split(',');
                        if (parts.Length == 4)
                        {
                            _BG = Color.FromArgb(
                                byte.Parse(parts[0]),
                                byte.Parse(parts[1]),
                                byte.Parse(parts[2]),
                                byte.Parse(parts[3]));
                            vOk = true;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                if (!vOk)
                {
                    System.Drawing.Color c = SystemColors.Window;
                    _BG = Color.FromArgb(c.A, c.R, c.G, c.B);
                    base["BG"] = _BG.A + "," + _BG.R + "," + _BG.G + "," + _BG.B;
                }
                return _BG;
            }
        }


        public string BlockFiles
        {
            get => base["BlockFiles"];
            set
            {
                base["BlockFiles"] = value;
            }
        }
        public List<string> GetBlockFileList()
        {
            List<string> result = new List<string>();
            if (base["BlockFiles"] != null)
            {
                string[] files = base["BlockFiles"].Split('|');
                foreach (string f in files)
                {
                    if (string.IsNullOrWhiteSpace(f))
                    {
                        continue;
                    }
                    result.Add(f.Trim().ToLower());
                }
            }
            return result;
        }

        public float _LimitMinFileSize = -1;
        public float LimitMinFileSize
        {
            set
            {
                _LimitMinFileSize = (value < 0 ? 0 : value);
                base["LimitMinFileSize"] = _LimitMinFileSize.ToString();
            }
            get
            {
                if (_LimitMinFileSize < 0)
                {
                    if (float.TryParse(base["LimitMinFileSize"], out float v))
                    {
                        _LimitMinFileSize = (v < 0 ? 0 : v);
                    }
                    else
                    {
                        _LimitMinFileSize = 0;
                        base["LimitMinFileSize"] = _LimitMinFileSize.ToString();
                    }
                }
                return _LimitMinFileSize;
            }
        }
    }
}
