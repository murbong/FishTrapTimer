using FishTrapTimer.Core;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace FishTrapTimer
{
    /// <summary>
    /// TrapWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TrapWindow : Window
    {
        private const double V = 0.01;
        public IntPtr windowHandle;
        public TrapWindow()
        {
            InitializeComponent();

            this.Topmost = true;


        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            windowHandle = new WindowInteropHelper(this).Handle;
        }
        public void SetWindowRect(RECT rect)
        {
            try
            {
                var _Left = rect.Left - Rect_Outline.StrokeThickness;
                var _Top = rect.Top - Rect_Outline.StrokeThickness;
                var _Width = rect.Right - rect.Left + Rect_Outline.StrokeThickness;
                var _Height = rect.Bottom - rect.Top + Rect_Outline.StrokeThickness;

                if (_Width > 0 && _Height > 0)
                {
                    Left = _Left;
                    Top = _Top;
                    Width = _Width;
                    Height = _Height;
                }


            }
            catch { }
        }

        public void ClearCanvas()
        {
            Dispatcher.Invoke(() =>
            {
                canvas.Children.Clear();
            });
        }

        public void DrawRectByRatio(RECT rect)
        {

            double x = GlobalManager.RealBorder.Width * rect.Left * V;
            double y = GlobalManager.RealBorder.Height * rect.Top * V;
            double Width = GlobalManager.RealBorder.Width * rect.Width * V;
            double Height = GlobalManager.RealBorder.Height * rect.Height * V;
            Dispatcher.Invoke(() =>
            {
                if (Width > 0 && Height > 0)
                {
                    Rectangle rec = new Rectangle()
                    {
                        Width = Width,
                        Height = Height,
                        //Fill = Brushes.Green,
                        Stroke = Brushes.Red,
                        StrokeThickness = 2,
                    };
                    // Add to a canvas 
                    canvas.Children.Add(rec);
                    Canvas.SetLeft(rec, x);
                    Canvas.SetTop(rec, y);
                }
            });
        }

        public void SetRectOutlineStrokeThickness(int thickness)
        {
            Rect_Outline.StrokeThickness = thickness;
        }
        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //DragMove();
        }
        private void Window_Deactivated(object sender, EventArgs e)
        {

        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ClearCanvas();
            foreach (var k in
            TaskManager.Instance.findTemplates)
            {
                DrawRectByRatio(k.BorderRatio);
            }
        }
    }
}
