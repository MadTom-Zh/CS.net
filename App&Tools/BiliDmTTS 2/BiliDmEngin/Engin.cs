using System;

using MadTomDev.Common;
using BilibiliDM_PluginFramework;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq;

namespace MadTomDev.App.BiliDmEngin
{
    public class Engin : IDisposable
    {
        private Logger logger;
        BiliDMLib.DanmakuLoader dmLoader;
        public delegate void NewBulletDelegate(Engin sender, Bullet newBullet);
        public event NewBulletDelegate NewBullet;
        public delegate void ConnectedDelegate(Engin sender);
        public event ConnectedDelegate Connected;
        public delegate void DisconnectedDelegate(Engin sender, Exception err);
        public event DisconnectedDelegate Disconnected;
        public Engin(Logger logger)
        {
            this.logger = logger;
            #region dmLoader init with events handlers
            dmLoader = new BiliDMLib.DanmakuLoader();
            dmLoader.Connected += (s,e) =>
            {
                Connected?.Invoke(this);
                logger.Log("Connected: " + e.ToString());
                logger.NewLine();
                NewBullet?.Invoke(this, new Bullet(Bullet.BulletType.Sys)
                {
                    sysMsg = "bililive_dm 已连接到房间:" + e.ToString(),
                });
            };
            dmLoader.Disconnected += (s1, e1) =>
            {
                Disconnected?.Invoke(this, e1.Error);
                logger.Log("Disconnected: " + (e1.Error == null ? "" : e1.Error.ToString()));
                logger.NewLine();
                NewBullet?.Invoke(this, new Bullet(Bullet.BulletType.Sys)
                {
                    sysMsg = "bililive_dm 已经断开，原因是:" + (e1.Error == null ? "正常断开" : e1.Error.Message),
                });
            };
            dmLoader.LogMessage += (s2, e2) =>
                {
                    logger.Log("LogMessage: " + e2.message);
                    logger.NewLine();
                    NewBullet?.Invoke(this, new Bullet(Bullet.BulletType.Sys)
                    {
                        sysMsg = e2.message,
                    });
                };
            dmLoader.ReceivedRoomCount += (s3, e3) =>
                    {
                        logger.Log("ReceivedRoomCount: " + e3.UserCount);
                        logger.NewLine();
                        NewBullet?.Invoke(this, new Bullet(Bullet.BulletType.Sys)
                        {
                            sysMsg = "ReceivedRoomCount: " + e3.UserCount,
                        });

                        // too ashamed to see

                    };
            dmLoader.ReceivedDanmaku += (s4, e4) =>
            {
                logger.Log("ReceivedDanmaku: " + e4.Danmaku.RawData);
                logger.NewLine();
                Bullet newBullet = null;
                switch (e4.Danmaku.MsgType)
                {
                    case MsgTypeEnum.LiveStart:
                        newBullet = new Bullet(Bullet.BulletType.Sys)
                        { sysMsg = "Live started at " + DateTime.Now.ToLongTimeString(), };
                        break;
                    case MsgTypeEnum.LiveEnd:
                        newBullet = new Bullet(Bullet.BulletType.Sys)
                        { sysMsg = "Live end at " + DateTime.Now.ToLongTimeString(), };
                        break;
                    case MsgTypeEnum.Welcome:
                        newBullet = new Bullet(Bullet.BulletType.UserEnter)
                        {
                            userID = e4.Danmaku.UserID.ToString(),
                            userName = e4.Danmaku.UserName,
                            userTitle = GetUserTitle(e4.Danmaku) + "老爷",
                        };
                        break;
                    case MsgTypeEnum.WelcomeGuard:
                        newBullet = new Bullet(Bullet.BulletType.UserEnter)
                        {
                            userID = e4.Danmaku.UserID.ToString(),
                            userName = e4.Danmaku.UserName,
                            userTitle = GetUserTitle(e4.Danmaku),
                        };
                        break;
                    case MsgTypeEnum.SuperChat:
                        newBullet = new Bullet(Bullet.BulletType.Sys)
                        {
                            sysMsg = GetUserTitle(e4.Danmaku)
                                + $"{e4.Danmaku.UserName}({e4.Danmaku.UserID})"
                                + e4.Danmaku.CommentText
                                + "，价值￥" + e4.Danmaku.Price.ToString("N2")
                                + (e4.Danmaku.SCKeepTime > 0 ? $"，将保留{e4.Danmaku.SCKeepTime}秒" : null),
                        };
                        break;
                    case MsgTypeEnum.Comment:
                        {
                            if (string.IsNullOrWhiteSpace(e4.Danmaku.CommentText))
                                break;
                            newBullet = new Bullet(Bullet.BulletType.Msg)
                            {
                                userID = e4.Danmaku.UserID.ToString(),
                                userName = e4.Danmaku.UserName,
                                userMsg = e4.Danmaku.CommentText,
                                userTitle = GetUserTitle(e4.Danmaku),
                            };
                        }
                        break;
                    case MsgTypeEnum.GiftSend:
                        newBullet = new Bullet(Bullet.BulletType.Gft)
                        {
                            userID = e4.Danmaku.UserID.ToString(),
                            userName = e4.Danmaku.UserName,
                            userTitle = GetUserTitle(e4.Danmaku),
                            giftName = e4.Danmaku.GiftName,
                            giftCount = e4.Danmaku.GiftCount,
                        };
                        break;
                    case MsgTypeEnum.GiftTop:
                        newBullet = new Bullet(Bullet.BulletType.Sys)
                        {
                            sysMsg = GetUserTitle(e4.Danmaku)
                                + e4.Danmaku.UserName
                                + "(" + e4.Danmaku.UserID + ")"
                                + "礼物排名提升。"
                        };
                        break;
                    case MsgTypeEnum.GuardBuy:
                        newBullet = new Bullet(Bullet.BulletType.Sys)
                        {
                            sysMsg = GetUserTitle(e4.Danmaku)
                                + e4.Danmaku.UserName
                                + "(" + e4.Danmaku.UserID + ")"
                                + $"使用{e4.Danmaku.GiftCount}{e4.Danmaku.GiftName}"
                                + "购买船票，成功上船。"
                        };
                        break;
                    case MsgTypeEnum.Interact: // user enter room
                        {
                            switch (e4.Danmaku.InteractType)
                            {
                                case InteractTypeEnum.Enter:
                                    newBullet = new Bullet(Bullet.BulletType.UserEnter)
                                    {
                                        userID = e4.Danmaku.UserID.ToString(),
                                        userName = e4.Danmaku.UserName,
                                        userTitle = GetUserTitle(e4.Danmaku),
                                    };
                                    break;
                                case InteractTypeEnum.Follow:
                                    newBullet = new Bullet(Bullet.BulletType.Sys)
                                    {
                                        sysMsg = GetUserTitle(e4.Danmaku)
                                            + e4.Danmaku.UserName
                                            + $"({e4.Danmaku.UserID})"
                                            + "关注了直播间",
                                    };
                                    break;
                                case InteractTypeEnum.Share:
                                    newBullet = new Bullet(Bullet.BulletType.Sys)
                                    {
                                        sysMsg = GetUserTitle(e4.Danmaku)
                                            + e4.Danmaku.UserName
                                            + $"({e4.Danmaku.UserID})"
                                            + "分享了直播间",
                                    };
                                    break;
                                case InteractTypeEnum.SpecialFollow:
                                    newBullet = new Bullet(Bullet.BulletType.Sys)
                                    {
                                        sysMsg = GetUserTitle(e4.Danmaku)
                                            + e4.Danmaku.UserName
                                            + $"({e4.Danmaku.UserID})"
                                            + "特别关注了直播间",
                                    };
                                    break;
                                case InteractTypeEnum.MutualFollow:
                                    newBullet = new Bullet(Bullet.BulletType.Sys)
                                    {
                                        sysMsg = GetUserTitle(e4.Danmaku)
                                            + e4.Danmaku.UserName
                                            + $"({e4.Danmaku.UserID})"
                                            + "相互关注了直播间",
                                    };
                                    break;
                                default:
                                    newBullet = new Bullet(Bullet.BulletType.Sys)
                                    {
                                        sysMsg = GetUserTitle(e4.Danmaku)
                                            + e4.Danmaku.UserName
                                            + $"({e4.Danmaku.UserID})"
                                            + "执行了未知的互动操作",
                                    };
                                    break;
                            }
                        }
                        break;
                    case MsgTypeEnum.Unknown:
                        newBullet = GetBullet_fromUnknowMsg(e4.Danmaku.RawDataJToken);
                        break;
                }
                if (newBullet != null)
                    NewBullet?.Invoke(this, newBullet);
            };
            #endregion

        }
        private string GetUserTitle(DanmakuModel dm)
        {
            StringBuilder result = new StringBuilder();
            if (dm.isVIP)
                result.Append("贵宾");
            if (dm.isAdmin)
                result.Append("管理员");
            switch (dm.UserGuardLevel)
            {
                default:
                case 0:
                    break;
                case 1:
                    result.Append("总督");
                    break;
                case 2:
                    result.Append("提督");
                    break;
                case 3:
                    result.Append("舰长");
                    break;
            }
            if (result.Length > 0)
                return result.ToString();
            else
                return null;
        }
        private Bullet GetBullet_fromUnknowMsg(JToken rawDataJToken)
        {
            object testValue = rawDataJToken["cmd"];
            string testStr;
            if (testValue != null)
            {
                testStr = testValue.ToString();
                switch (testStr)
                {
                    case "ENTRY_EFFECT": // big dady enter room
                        testStr = rawDataJToken["data"]["copy_writing"].ToString();
                        testStr = HttpUtility.HtmlDecode(testStr);
                        if (!string.IsNullOrWhiteSpace(testStr))
                        {
                            return new Bullet(Bullet.BulletType.Sys)
                            { sysMsg = testStr.Replace("<%", " ").Replace("%>", " "), };
                        }
                        break;
                    case "ONLINERANK":
                    case "ROOM_REAL_TIME_MESSAGE_UPDATE":
                    case "ACTIVITY_BANNER_UPDATE_V2":
                    case "PANEL": // "note": "娱乐 第45名",
                    case "ROOM_RANK": // "rank_desc": "娱乐小时榜 53",
                    case "ROOM_BANNER": // "season_name": "大乱斗S1赛季",
                    case "ANCHOR_LOT_CHECKSTATUS":
                    case "ANCHOR_LOT_END":
                    case "NOTICE_MSG": // 猫同学 投喂 猫大人 1个小电视飞船，点击前往TA的房间吧！ // 猫大人の白开水 在 猫大人 的房间开通了总督

                        // ???
                        return new Bullet(Bullet.BulletType.Other)
                        {
                            sysMsg = testStr,
                        };
                    default:
                        logger.Log("Un-processed data above.");
                        logger.NewLine();
                        return new Bullet(Bullet.BulletType.Other)
                        {
                            sysMsg = testStr,
                        };
                }
            }
            return null;
        }

        private int _RoomID = 668931;
        public int RoomID
        {
            get => _RoomID;
            set
            {
                _RoomID = value;
            }
        }
        public async void Connect()
        {
            if (RoomID >= 0)
            {
                logger.Log($"Connecting...");
                logger.NewLine();
                bool result = await dmLoader.ConnectAsync(RoomID);
                if (!result)
                    Disconnected?.Invoke(this, new Exception("尝试连接失败。"));
            }
            else
            {
                logger.Log("RoomID is not correct.");
                logger.NewLine();
            }
        }
        public void Disconnect()
        {
            if (dmLoader.IsConnected)
                dmLoader.Disconnect();
        }

        public void Dispose()
        {
            Disconnect();
        }
    }

}
