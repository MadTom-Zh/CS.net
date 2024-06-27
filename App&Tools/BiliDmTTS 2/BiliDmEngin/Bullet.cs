using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadTomDev.App.BiliDmEngin
{

    public class Bullet
    {
        public Bullet(BulletType type)
        {
            this.type = type;
        }

        public enum BulletType
        {
            Msg,
            Gft,
            UserEnter,
            Sys,
            Other,
        }
        public BulletType type;

        public string userID;
        public string userName;
        public string userTitle;
        public string userMsg;

        public string giftName;
        public int giftCount;

        public string sysMsg = null;

        public string CompleteText
        {
            get
            {
                switch (type)
                {
                    case BulletType.Gft:
                        return "收到 " + userName + " 的" + giftCount + "个 " + giftName;
                    case BulletType.Msg:
                        return (string.IsNullOrWhiteSpace(userTitle) ? "" : (userTitle + " "))
                            + userName + $"({userID}):" + userMsg;
                    case BulletType.UserEnter:
                        return (string.IsNullOrWhiteSpace(userTitle) ? "" : (userTitle + " "))
                            + userName + " 进入直播间";
                    default:
                    case BulletType.Other:
                    case BulletType.Sys:
                        return sysMsg;
                }
            }
        }
    }
}
