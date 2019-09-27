using FishTrapTimer.Core;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using Window = System.Windows.Window;

namespace FishTrapTimer
{
    /// <summary>
    /// BattleWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BattleWindow : Window
    {
        public BattleWindow()
        {
            InitializeComponent();
            Listener.Instance.Append += new Listener.AppendText(AppendTextBox);
        }

        public void AppendTextBox(string str)
        {
            Dispatcher.Invoke(() =>
            {
                txtBox.AppendText(DateTime.Now.ToString(@"yyyy\-MM\-dd hh\:mm\:ss \: "));
                txtBox.AppendText(str);
                txtBox.Select(txtBox.Text.Length, 0);
                txtBox.ScrollToEnd();
            }
            );
        }

    }
}
