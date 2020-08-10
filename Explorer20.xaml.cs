using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Windows_20_Explorer_Concept
{
    public partial class MainWindow : Window
    {
        Timer winAero = new Timer();
        Timer checkForChange = new Timer();
        double previousWidth = 0;
        double previousHeight = 0;
        double previousTop = 0;
        double previousLeft = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BlurWindow.EnableBlur(this);
            winAero.Interval = 10;
            winAero.Elapsed += checkForAeroFC;
            winAero.Start();

            DataContext = new XAMLStyles { };
            checkForChange.Interval = 1000;
            checkForChange.Elapsed += (se, ea) => { try { if (Styles.themeChanged) { Dispatcher.Invoke(() => { DataContext = new XAMLStyles { }; }); } } catch { } };
            checkForChange.Start();
        }

        private void topBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (Width == SystemParameters.WorkArea.Width && Height == SystemParameters.WorkArea.Height)
                {
                    Top = System.Windows.Forms.Control.MousePosition.Y - 15;
                    Left = System.Windows.Forms.Control.MousePosition.X - 475;
                    Width = previousWidth;
                    Height = previousHeight;
                    resizebtn.Content = "\uE922";
                    DragMove();
                }
                else if (e.ClickCount == 2)
                {
                    Top = 0;
                    Left = 0;
                    Width = SystemParameters.WorkArea.Width;
                    Height = SystemParameters.WorkArea.Height;
                    resizebtn.Content = "\uE923";
                }
                else
                {
                    DragMove();
                    previousWidth = Width;
                    previousHeight = Height;
                    previousTop = Top;
                    previousLeft = Left;
                }
            }
        }

        private void closebtn_Click(object sender, RoutedEventArgs e) { Close(); }

        protected override void OnClosing(CancelEventArgs e)
        {
            winAero.Stop();
            checkForChange.Stop();
        }

        private void resizebtn_Click(object sender, RoutedEventArgs e)
        {
            if (Height != SystemParameters.WorkArea.Height && Width != SystemParameters.WorkArea.Width)
            {
                previousWidth = Width;
                previousHeight = Height;
                Top = 0;
                Left = 0;
                Height = SystemParameters.WorkArea.Height;
                Width = SystemParameters.WorkArea.Width;
                resizebtn.Content = "\uE923";
            }
            else
            {
                WindowState = WindowState.Normal;
                Width = previousWidth;
                Height = previousHeight;
                Top = previousTop;
                Left = previousLeft;
                resizebtn.Content = "\uE922";
            }
        }

        private void minimisebtn_Click(object sender, RoutedEventArgs e) { WindowState = WindowState.Minimized; }

        private void checkForAeroFC(object sender, ElapsedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (WindowState == WindowState.Maximized)
                    {
                        WindowState = WindowState.Normal;
                        Top = 0;
                        Left = 0;
                        Width = SystemParameters.WorkArea.Width;
                        Height = SystemParameters.WorkArea.Height;
                        resizebtn.Content = "\uE923";
                    }
                    else if (Width != SystemParameters.WorkArea.Width && Height != SystemParameters.WorkArea.Height)
                    {
                        resizebtn.Content = "\uE922";
                    }

                    if (Height > SystemParameters.WorkArea.Height) { Height = SystemParameters.WorkArea.Height; }
                });
            }
            catch { }
        }
    }
}
