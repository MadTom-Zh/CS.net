using System;

using System.Management;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MadTomDev.Resource
{
    public class DriveDetector : IDisposable
    {
        private DriveDetector() { }
        private static DriveDetector instance = null;
        public static DriveDetector GetInstance()
        {
            if (instance == null)
            {
                instance = new DriveDetector();
                instance.StartListening();
            }
            return instance;
        }

        private bool isListening = false;
        private bool notCancelFlag = true;
        ManagementEventWatcher watcher;
        private void StartListening()
        {
            Task.Run(() =>
            {
                isListening = true;
                WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2 or EventType = 3");

                watcher = new ManagementEventWatcher(query);
                watcher.EventArrived += Watcher_EventArrived;
                watcher.Start();

                while (notCancelFlag)
                {
                    Task.Delay(100).Wait();
                }
                watcher.EventArrived -= Watcher_EventArrived;
                isListening = false;
            });
        }

        public  delegate void DrivePlugedDelegate(char driveLetter,bool plugedInOrOut);
        public event DrivePlugedDelegate DrivePluged;
        private void Watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            //ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            //Dictionary<string, object> result = new Dictionary<string, object>();
            //foreach (var property in e.NewEvent.Properties)
            //{
            //    result.Add(property.Name, property.Value);
            //}
            DrivePluged?.Invoke(
                e.NewEvent.Properties["DriveName"].Value.ToString()[0],
                e.NewEvent.Properties["EventType"].Value.ToString() == "2");
        }


        public void Dispose()
        {
            notCancelFlag = false;
            while (isListening)
            {
                Task.Delay(10).Wait();
            }
            watcher?.Dispose();            
        }
    }
}
