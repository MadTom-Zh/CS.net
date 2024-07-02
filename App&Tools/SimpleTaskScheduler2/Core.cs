using MadTomDev.Common;
using MadTomDev.Common.Variables;
using MadTomDev.UI.Class;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MadTomDev.App
{
    internal class Core
    {
        private Core()
        {
            data = new Data();
            logger = new Logger()
            {
                BaseDir = Path.Combine(IOPath.LogDir),
                BaseFileNamePre = "STS2",
                IncrementType = Logger.IncrementTypeEnums.Size5M,
            };
        }
        private static Core instance;
        public static Core GetInstance()
        {
            if (instance == null)
            {
                instance = new Core();
            }
            return instance;
        }

        public MainWindow mainWindow;
        public Data data;
        public Logger logger;
        public void ReInit(MainWindow mainWindow)
        {
            data.ReloadSchelduleList();
            data.ReloadSettings();

            this.mainWindow = mainWindow;

            timer = new Timer(new TimerCallback(timerTick));
            ChangeTimerInterval(data.settingTimerIntervalSecs, false);

            // load settings
            mainWindow.settingTaskListDays_userSetting = true;
            mainWindow.settingTimerInterval_userSetting = true;
            mainWindow.tb_settingTaskListDays.Text = data.settingTaskScoutDays.ToString("0");
            mainWindow.sld_settingTaskListDays.Value = data.settingTaskScoutDays;
            mainWindow.tb_settingTimerInterval.Text = data.settingTimerIntervalSecs.ToString("0");
            mainWindow.sld_settingTimerInterval.Value = data.settingTimerIntervalSecs;
            mainWindow.settingTaskListDays_userSetting = false;
            mainWindow.settingTimerInterval_userSetting = false;


            // load data grid schedule
            ReLoadAllSchedules();

            // alert of dated tasks            
            DateTime now = DateTime.Now;
            if (data.settingLastTaskStartTime < now)
            {
                StringBuilder strBdr = new StringBuilder();
                int tCount;
                foreach (Data.VMDataSchedule vm in GetAllSchedules())
                {
                    tCount = vm.scheduleData.GetTriggerPoints(data.settingLastTaskStartTime, now).Count;
                    if (tCount > 0)
                    {
                        strBdr.AppendLine($"Missing {tCount} times, {vm.Title}, {vm.scheduleData.GetCycleDescription(false)}.");
                    }
                }
                if (strBdr.Length > 0)
                {
                    MessageBox.Show(mainWindow, strBdr.ToString(), "When app is down", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    data.settingLastTaskStartTime = now;
                    data.SaveSettings();
                }
            }

            // re-gen task list
            GenerateTasks(true);
        }
        public void ReLoadAllSchedules()
        {
            mainWindow.dg_scheOneTime_data.Clear();
            mainWindow.dg_scheEveryDay_data.Clear();
            mainWindow.dg_scheEveryWeek_data.Clear();
            mainWindow.dg_scheEveryMonth_data.Clear();
            mainWindow.dg_scheOtherInterval_data.Clear();
            List<Data.VMDataSchedule> allSchelList = data.ReloadSchelduleList();
            int noO = 1, noEd = 1, noEw = 1, noEm = 1, noOi = 1;
            foreach (Data.VMDataSchedule vm in allSchelList)
            {
                switch (vm.scheduleData.scheduleType)
                {
                    case ScheduleData.ScheduleTypes.Once:
                        vm.NoTx = (noO++).ToString();
                        mainWindow.dg_scheOneTime_data.Add(vm);
                        break;
                    case ScheduleData.ScheduleTypes.EveryDay:
                        vm.NoTx = (noEd++).ToString();
                        mainWindow.dg_scheEveryDay_data.Add(vm);
                        break;
                    case ScheduleData.ScheduleTypes.EveryWeek:
                        vm.NoTx = (noEw++).ToString();
                        mainWindow.dg_scheEveryWeek_data.Add(vm);
                        break;
                    case ScheduleData.ScheduleTypes.EveryMonth:
                        vm.NoTx = (noEm++).ToString();
                        mainWindow.dg_scheEveryMonth_data.Add(vm);
                        break;
                    case ScheduleData.ScheduleTypes.OtherInterval:
                        vm.NoTx = (noOi++).ToString();
                        mainWindow.dg_scheOtherInterval_data.Add(vm);
                        break;
                }
            }
        }
        internal void SaveAllSchedules()
        {
            data.SaveSchelduleLists(new ObservableCollection<Data.VMDataSchedule>[]
                {
                    mainWindow.dg_scheEveryDay_data,
                    mainWindow.dg_scheEveryMonth_data,
                    mainWindow.dg_scheEveryWeek_data,
                    mainWindow.dg_scheOneTime_data,
                    mainWindow.dg_scheOtherInterval_data,
                });
        }
        private List<Data.VMDataSchedule> GetAllSchedules()
        {
            List<Data.VMDataSchedule> result = new List<Data.VMDataSchedule>();
            result.AddRange(mainWindow.dg_scheEveryDay_data);
            result.AddRange(mainWindow.dg_scheEveryMonth_data);
            result.AddRange(mainWindow.dg_scheEveryWeek_data);
            result.AddRange(mainWindow.dg_scheOneTime_data);
            result.AddRange(mainWindow.dg_scheOtherInterval_data);
            return result;
        }
        private bool HaveSchedule()
        {
            return mainWindow.dg_scheEveryDay_data.Count > 0
                || mainWindow.dg_scheEveryMonth_data.Count > 0
                || mainWindow.dg_scheEveryWeek_data.Count > 0
                || mainWindow.dg_scheOneTime_data.Count > 0
                || mainWindow.dg_scheOtherInterval_data.Count > 0;
        }
        public bool GenerateTasks(bool clearRegen = false)
        {
            bool result = false;
            mainWindow.Dispatcher.Invoke(() =>
            {
                DateTime taskSFrom = DateTime.Now;
                DateTime taskSBefore = DateTime.Now.AddDays(data.settingTaskScoutDays);
                if (clearRegen)
                {
                    mainWindow.dg_taskList_data.Clear();
                }
                else
                {
                    if (mainWindow.dg_taskList_data.Count > 0)
                        taskSFrom = mainWindow.dg_taskList_data[mainWindow.dg_taskList_data.Count - 1].StartTime.AddSeconds(1);
                }

                List<Data.VMDataTask> allTasks = new List<Data.VMDataTask>();
                List<DateTime> tPoints;
                foreach (Data.VMDataSchedule vm in GetAllSchedules())
                {
                    tPoints = vm.scheduleData.GetTriggerPoints(taskSFrom, taskSBefore);
                    foreach (DateTime tp in tPoints)
                    {
                        allTasks.Add(new Data.VMDataTask()
                        {
                            VMSchelduleData = vm,
                            StartTime = tp,
                            // CountDownTx =
                        });
                    }
                }
                allTasks.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
                for (int i = 0, iv = allTasks.Count; i < iv; ++i)
                {
                    mainWindow.dg_taskList_data.Add(allTasks[i]);
                }
                result = allTasks.Count > 0;
            });
            return result;
        }


        #region timer

        private Timer timer;
        private DateTime nextStartTime = DateTime.MaxValue;
        public int timer_selfCountdown = 0;
        private void timerTick(object s)
        {
            if (timer_selfCountdown > 0)
            {
                --timer_selfCountdown;
                return;
            }
            if (mainWindow.dg_taskList_data.Count <= 0)
            {
                if (!HaveSchedule())
                    return;

                // gen tasks
                if (!GenerateTasks())
                {
                    timer_selfCountdown = 10;
                }
            }
            else
            {
                mainWindow.UpdateTasksCountDown();

                // try start task
                if (nextStartTime == DateTime.MaxValue)
                {
                    nextStartTime = mainWindow.dg_taskList_data[0].StartTime;
                }
                else
                {
                    DateTime now = DateTime.Now;
                    if (nextStartTime <= now)
                    {
                        nextStartTime = DateTime.MaxValue;
                        Data.VMDataTask taskData = mainWindow.dg_taskList_data[0];

                        // start
                        Task.Run(() =>
                        {
                            using (Process p = new Process())
                            {
                                //启动cmd，并设置好相关属性。
                                logger.Log(" Start Cmd");
                                p.StartInfo = new ProcessStartInfo()
                                {
                                    FileName = "cmd.exe",
                                    //Arguments = "/c " + command,
                                    UseShellExecute = false,
                                    RedirectStandardInput = true,
                                    RedirectStandardOutput = true,
                                    RedirectStandardError = true,
                                    CreateNoWindow = true,
                                };
                                p.Start();

                                string[] cmdLines = taskData.VMSchelduleData.Cmd.Replace("\r\n", "\r").Replace("\n", "\r").Split('\r');

                                foreach (string l in cmdLines)
                                {
                                    p.StandardInput.WriteLine(l);
                                }
                                p.StandardInput.Flush();
                                p.StandardInput.Close();

                                string outPut = p.StandardOutput.ReadToEnd();
                                logger.Log(outPut);

                                p.WaitForExit();
                                p.Close();
                            }
                        });

                        data.settingLastTaskStartTime = now.AddMilliseconds(1);
                        data.SaveSettings();

                        // remove first task, gen more
                        mainWindow.Dispatcher.Invoke(() =>
                        {
                            if (mainWindow.dg_taskList_data.Count > 0)
                            {
                                mainWindow.dg_taskList_data.RemoveAt(0);
                            }
                            GenerateTasks();
                        });

                        if (mainWindow.dg_taskList_data.Count <= 0)
                        {
                            timer_selfCountdown = 10;
                        }
                    }
                }
            }
        }

        #endregion

        internal void ChangeTimerInterval(int vSec, bool saveSetting = true)
        {
            int timerItv = vSec * 1000;
            timer.Change(timerItv, timerItv);

            if (saveSetting)
            {
                data.settingTimerIntervalSecs = vSec;
                data.SaveSettings();
            }
        }

    }
}
