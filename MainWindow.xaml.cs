using FishTrapTimer.Core;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace FishTrapTimer
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        private TrapWindow trapWindow;
        private BattleWindow battleWindow;
        public bool isRunning;
        public double Setting;
        readonly List<Process> ProcessList;
        private static string path = @".\config.ini";

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        private string ReadINI(string section, string key, string path)
        {
            StringBuilder sb = new StringBuilder(255);

            GetPrivateProfileString(section, key, "", sb, sb.Capacity, path);

            return sb.ToString();
        }
        private void WriteINI(string section, string key, string value, string path)
        {
            WritePrivateProfileString(section, key, value, path);
        }

        private void InitINI()
        {
            var left = ReadINI("Offset", "Left", path);
            var top = ReadINI("Offset", "Top", path);
            var bottom = ReadINI("Offset", "Bottom", path);
            var right = ReadINI("Offset", "Right", path);
            Setting = Convert.ToDouble( ReadINI("Time", "Init", path));

            GlobalManager.Offset = new RECT(Convert.ToInt32(left), Convert.ToInt32(top), Convert.ToInt32(right), Convert.ToInt32(bottom));

            txt_Left.Text = left;
            txt_Right.Text = right;
            txt_Top.Text = top;
            txt_Bot.Text = bottom;

        }
        private void SaveINI()
        {
            WriteINI("Offset", "Left", GlobalManager.Offset.Left.ToString(), path);
            WriteINI("Offset", "Top", GlobalManager.Offset.Top.ToString(), path);
            WriteINI("Offset", "Bottom", GlobalManager.Offset.Bottom.ToString(), path);
            WriteINI("Offset", "Right", GlobalManager.Offset.Right.ToString(), path);

        }


        public MainWindow()
        {
            InitializeComponent();
            //Icon = new BitmapImage(new Uri("pack://application:,,,/FishTrapTimer;component/icon.ico"));
            InitINI();

            ProcessList = new List<Process>();
            GlobalManager.StartTime = DateTime.Now;
            GlobalManager.SettingTime = TimeSpan.FromMinutes(Setting);
            GlobalManager.Count = 0;
            Task.Run(() => TrapWindowMove());
            TaskManager.Instance.SetTimer += new TaskManager.SetTimerText(SetContent); // event 구독

            Listener.Instance.StartServer(52323);

            isRunning = false;
            lbl_Timer.Content = GlobalManager.SettingTime.ToString(@"hh\:mm\:ss\.ff");
            trapWindow = new TrapWindow();

            battleWindow = new BattleWindow();


            var find1 = new FindTemplate("반복", @".\images\수정됨_반복전투.png", 0.7, new RECT(35, 45, 65, 55));
            find1.SetAction(() =>
            {
                GlobalManager.Count++;
                GlobalManager.StartTime = DateTime.Now;
                var str = "통발 " + GlobalManager.Count + "번 건져냈다리~ (소요시간 : " + GlobalManager.ElaspedTime.ToString(@"mm\:ss") + " )\n";
                Listener.Instance.BeginSend(str);
                battleWindow?.AppendTextBox(str);

                if (GlobalManager.Count > 1)
                {
                    GlobalManager.SettingTime = GlobalManager.ElaspedTime + TimeSpan.FromMinutes(5);
                }

                Console.WriteLine("I Found !");
            });

            TaskManager.Instance.findTemplates.Add(find1);

            RefreshProcess();
        }

        private void SetContent(string str)
        {
            Dispatcher.Invoke(() =>
            lbl_Timer.Content = str);
        }

        #region Event

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GlobalManager.StartTime = DateTime.Now;
            if (GlobalManager.SelectedProcess?.MainWindowHandle != null && GlobalManager.RealBorder.Width>0 && GlobalManager.RealBorder.Height>0)
            {
                isRunning = true ^ isRunning;
                if (isRunning)
                {
                    btn.Content = "정지";
                    TaskManager.Instance.TaskStart().ContinueWith((arg) => Dispatcher.Invoke(() =>
                    {
                        btn.Content = "실행";
                        isRunning = false;
                        GC.Collect();
                        GlobalManager.SettingTime = TimeSpan.FromMinutes(Setting);
                        GlobalManager.Count = 0;
                    }));
                }
                else
                {
                    TaskManager.Instance.TaskStop();
                    Console.WriteLine("정지");
                }
            }
            else
            {
                MessageBox.Show("프로세스를 찾을 수 없습니다.");
            }
        }
        private void RefreshProcess()
        {
            ProcessList.Clear();
            try
            {
                foreach (Process p in Process.GetProcesses())
                {
                    if (!string.IsNullOrEmpty(p.MainWindowTitle))
                    {
                        ProcessList.Add(p);
                    }
                }
                cmb_Process.ItemsSource = ProcessList;
                cmb_Process.DisplayMemberPath = "MainWindowTitle";
                cmb_Process.Items.Refresh();
            }
            catch
            {

            }
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            RefreshProcess();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            trapWindow?.Close();
            battleWindow?.Close();
            Listener.Instance.StopServer();
            SaveINI();
        }

        private void Cmb_Process_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            GlobalManager.SelectedProcess = comboBox.SelectedItem as Process;
        }

        private async void TrapWindowMove()
        {
            while (true)
            {
                if (GlobalManager.SelectedProcess != null && trapWindow?.windowHandle != null)
                {
                    ImageCapture.GetWindowRect(GlobalManager.SelectedProcess.MainWindowHandle, out GlobalManager.ProcessBorder);
                    Dispatcher.Invoke(() =>
                    {
                        if (GlobalManager.SelectedProcess?.HasExited == false && GlobalManager.SelectedProcess.MainWindowHandle != Process.GetCurrentProcess().MainWindowHandle)
                            trapWindow.SetWindowRect(GlobalManager.RealBorder);
                        //trapWindow.Rect_Outline.Stroke
                    }
                    );
                    //await Task.Delay(10);
                }
                else
                {
                    await Task.Delay(1000);
                }
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            //Regex regex = new Regex("[^0-9]+");
            Regex regex = new Regex(@"^[-]?\d*$");
            e.Handled = !regex.IsMatch(e.Text);
            if (e.Text == "-" && (!string.IsNullOrEmpty((sender as TextBox).Text.Trim()) || (sender as TextBox).Text.IndexOf('-') > -1)) e.Handled = true;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = sender as TextBox;
            int offset = 0;
            if (!string.IsNullOrEmpty(textbox.Text))
            {
                try
                {
                    offset = Convert.ToInt32(textbox.Text);
                }
                catch
                {
                    offset = 0;
                }
            }

            switch (textbox.Name)
            {
                case "txt_Left":
                    GlobalManager.Offset.Left = offset;
                    break;
                case "txt_Top":
                    GlobalManager.Offset.Top = offset;
                    break;
                case "txt_Bot":
                    GlobalManager.Offset.Bottom = offset;
                    break;
                case "txt_Right":
                    GlobalManager.Offset.Right = offset;
                    break;

            }
        }

        #endregion

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            trapWindow.Show();
            battleWindow.Show();

            battleWindow?.AppendTextBox("서버 시작 52323포트.\n");

        }
    }
}
