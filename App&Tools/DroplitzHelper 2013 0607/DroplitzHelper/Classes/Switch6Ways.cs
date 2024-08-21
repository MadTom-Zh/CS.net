using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DroplitzHelper.Classes
{
    public class Switch6Ways
    {
        public UCs.UC_Switch UI;

        public SwitchPanelMgr.Point PosiInMatrix = null;

        public Switch6Ways()
        {
            _Init();
            MyType = Type.None;
        }
        public Switch6Ways(Type myType)
        {
            _Init();
            MyType = myType;
        }
        public void Dispose()
        {
        }
        private void _Init()
        {
            UI = new UCs.UC_Switch();
            UI.Switch6WData = this;
            this.ImChanged += Switch6Ways_ImChanged;
        }
        void Switch6Ways_ImChanged(object sender, EventArgs e)
        {
            UI.ReSetUI_Auto();
        }

        public event EventHandler ImChanged;
        public void TryBroadcastImChanged()
        {
            if (ImChanged != null)
            {
                ImChanged(this, new EventArgs());
            }
        }
        public enum Type
        {
            None,
            START,
            I,
            L,
            Y,
            U,
            X,
            END,
        }
        public int PosibleShapesCount
        {
            get
            {
                switch (_myType)
                {
                    case Type.None:
                        return 1;
                    case Type.START:
                        return 1;
                    case Type.END:
                        return 1;
                    case Type.I:
                        return 3;
                    case Type.L:
                        return 6;
                    case Type.Y:
                        return 2;
                    case Type.U:
                        return 6;
                    case Type.X:
                        return 3;
                }
                return 0;
            }
        }

        private Type _myType;
        public Type MyType
        {
            get
            {
                return _myType;
            }
            set
            {
                _myType = value;
                ResetWays();
                if (ImChanged != null)
                {
                    ImChanged(this, new EventArgs());
                }
            }
        }
        private void ResetWays()
        {
            switch (_myType)
            {
                case Type.None:
                    _ways[0]
                        = _ways[1]
                        = _ways[2]
                        = _ways[3]
                        = _ways[4]
                        = _ways[5]
                        = false;
                    break;
                case Type.START:
                    _ways[0]
                        = _ways[1]
                        = _ways[2]
                        = _ways[4]
                        = _ways[5]
                        = false;
                    _ways[3]
                        = true;
                    break;
                case Type.END:
                    _ways[1]
                        = _ways[2]
                        = _ways[3]
                        = _ways[4]
                        = _ways[5]
                        = false;
                    _ways[0]
                        = true;
                    break;
                case Type.I:
                    _ways[1]
                        = _ways[2]
                        = _ways[4]
                        = _ways[5]
                        = false;
                    _ways[0]
                        = _ways[3]
                        = true;
                    break;
                case Type.L:
                    _ways[1]
                        = _ways[3]
                        = _ways[4]
                        = _ways[5]
                        = false;
                    _ways[0]
                        = _ways[2]
                        = true;
                    break;
                case Type.Y:
                    _ways[1]
                        = _ways[3]
                        = _ways[5]
                        = false;
                    _ways[0]
                        = _ways[2]
                        = _ways[4]
                        = true;
                    break;
                case Type.U:
                    _ways[1]
                        = _ways[2]
                        = _ways[3]
                        = _ways[4]
                        = _ways[5]
                        = false;
                    _ways[0]
                        = true;
                    break;
                case Type.X:
                    _ways[2]
                        = _ways[5]
                        = false;
                    _ways[0]
                        = _ways[1]
                        = _ways[3]
                        = _ways[4]
                        = true;
                    break;
            }
        }
        public void TurnWaysRight(int turnTimes)
        {
            turnTimes %= 6;
            if (turnTimes == 0)
            {
                return;
            }
            else if (turnTimes == 3)
            {
                _TurnWays3Times();
            }
            else if (turnTimes > 3)
            {
                TurnWaysLeft(6 - turnTimes);
            }
            else
            {
                _TurnWays(true, (turnTimes == 1));
            }
            TryBroadcastImChanged();
        }
        public void TurnWaysLeft(int turnTimes)
        {
            turnTimes %= 6;
            if (turnTimes == 0)
            {
                return;
            }
            else if (turnTimes == 3)
            {
                _TurnWays3Times();
            }
            else if (turnTimes > 3)
            {
                TurnWaysRight(6 - turnTimes);
            }
            else
            {
                _TurnWays(false, (turnTimes == 1));
            } 
            TryBroadcastImChanged();
        }
        private void _TurnWays3Times()
        {
            bool tmp = _ways[0];
            _ways[0] = _ways[3];
            _ways[3] = tmp;
            tmp = _ways[1];
            _ways[1] = _ways[4];
            _ways[4] = tmp;
            tmp = _ways[2];
            _ways[2] = _ways[5];
            _ways[5] = tmp;
        }
        private void _TurnWays(bool isTurnRight, bool turnOneTimeOrTwo)
        {
            bool[] tmpList = new bool[6];
            for (int i = 5; i >= 0; i--)
            {
                tmpList[i] = _ways[i];
            }
            if (isTurnRight == true)
            {
                if (turnOneTimeOrTwo == true)
                {
                    _ways[0] = tmpList[5];
                    _ways[1] = tmpList[0];
                    _ways[2] = tmpList[1];
                    _ways[3] = tmpList[2];
                    _ways[4] = tmpList[3];
                    _ways[5] = tmpList[4];
                }
                else
                {
                    _ways[0] = tmpList[4];
                    _ways[1] = tmpList[5];
                    _ways[2] = tmpList[0];
                    _ways[3] = tmpList[1];
                    _ways[4] = tmpList[2];
                    _ways[5] = tmpList[3];
                }
            }
            else
            {
                if (turnOneTimeOrTwo == true)
                {
                    _ways[0] = tmpList[1];
                    _ways[1] = tmpList[2];
                    _ways[2] = tmpList[3];
                    _ways[3] = tmpList[4];
                    _ways[4] = tmpList[5];
                    _ways[5] = tmpList[0];
                }
                else
                {
                    _ways[0] = tmpList[2];
                    _ways[1] = tmpList[3];
                    _ways[2] = tmpList[4];
                    _ways[3] = tmpList[5];
                    _ways[4] = tmpList[0];
                    _ways[5] = tmpList[1];
                }
            }
        }

        private bool[] _ways = new bool[6];
        public bool[] Ways
        {
            get
            {
                return _ways;
            }
        }

    }
}
