
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using BitmapConverter = OpenCvSharp.Extensions.BitmapConverter;

namespace FishTrapTimer.Core
{
    public class TaskManager : Singleton<TaskManager>
    {
        private const double V = 0.01;
        private CancellationTokenSource cts;

        public List<FindTemplate> findTemplates;

        public delegate void SetTimerText(string str);
        public event SetTimerText SetTimer;


        public TaskManager() { findTemplates = new List<FindTemplate>(); }

        public async Task CaptureAuto(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (GlobalManager.SelectedProcess != null)
                    {
                        using (Bitmap bit = ImageCapture.PrintWindow(GlobalManager.SelectedProcess.MainWindowHandle))
                        using (Mat capture = BitmapConverter.ToMat(bit).Threshold(160, 255, ThresholdTypes.Tozero))
                        {
                            foreach (FindTemplate find in findTemplates)// findTemplates 실행부
                            {
                                RECT rect = find.BorderRatio;
                                double x = (GlobalManager.RealBorder.Width * rect.Left * V) + GlobalManager.Offset.Left;
                                double y = (GlobalManager.RealBorder.Height * rect.Top * V) + GlobalManager.Offset.Top;
                                double width = (GlobalManager.RealBorder.Width * rect.Width * V) + GlobalManager.Offset.Right;
                                double height = (GlobalManager.RealBorder.Height * rect.Height * V) + GlobalManager.Offset.Bottom;

                                using (Mat crop = capture[new OpenCvSharp.Rect((int)x, (int)y, (int)width, (int)height)])
                                using (Mat resize = crop.Resize(new OpenCvSharp.Size(200, 36)))
                                {
                                    find.Find = ImageFinder.ImageFind(resize, find.FindMat, find.Threshold);
                                    //SetImg(resize);
                                    if (find.Find == true && find.Switching == false)
                                    {
                                        find.Function();
                                        find.Switching = true;
                                    }
                                    else if (find.Find == false)
                                    {
                                        find.Switching = false;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                catch
                {
                    TaskStop();
                    break;
                }
                await Task.Delay(750);
            }
        }

        public Task TaskStart()
        {

            cts = new CancellationTokenSource();
            //return Task.Run(() => GetTimer(cts.Token));

            return Task.WhenAny(GetTimer(cts.Token), CaptureAuto(cts.Token));

        }
        public void TaskStop()
        {
            cts.Cancel();
        }
        public async Task GetTimer(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                GlobalManager.ElaspedTime = new TimeSpan(DateTime.Now.Ticks - GlobalManager.StartTime.Ticks);
                var TimeLeft = GlobalManager.SettingTime - GlobalManager.ElaspedTime;
                if (TimeLeft.Ticks <= 0)
                {
                    TaskStop();
                    Listener.Instance.BeginSend("통발에 문제가 생긴듯?");
                    MessageBox.Show("통발에 문제가 생긴듯?");
                    break;
                }
                else
                {
                    SetTimer(TimeLeft.ToString("hh\\:mm\\:ss\\.ff"));
                }
                await Task.Delay(10);
            }
        }

    }
}
