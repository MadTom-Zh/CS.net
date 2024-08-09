using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MadTomDev.App.BiliDmEnginInterface
{

    public interface EnginInterface : IDisposable
    {
        public delegate void NewBulletDelegate(EnginInterface sender, Bullet newBullet);
        public event NewBulletDelegate NewBullet;
        public delegate void ConnectedDelegate(EnginInterface sender);
        public event ConnectedDelegate Connected;
        public delegate void DisconnectedDelegate(EnginInterface sender, Exception? err);
        public event DisconnectedDelegate Disconnected;

        public void Connect() { }
        public void Disconnect() { }
    }    
}
