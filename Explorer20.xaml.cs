using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
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
using Image = System.Windows.Controls.Image;

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
            Styles.checkForChange();
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

            CreateTab(null, null);
        }

        private void TopBar_MouseDown(object sender, MouseButtonEventArgs e)
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

        //
        private int PagesCreated = 0;
        public static Dictionary<int, WindowInfo> Windows = new Dictionary<int, WindowInfo>();

        public class WindowInfo
        {
            public int TabIndex = Windows.Count + 1;
            public ExplorerPage explorerPage;
            public Grid Container;
            public Image Icon;
            public Label Path;
            public Button Close;
            public bool Active = true;
        }

        private void CreateTab(object sender, RoutedEventArgs e)
        {
            foreach (KeyValuePair<int, WindowInfo> wi in Windows)
            {
                wi.Value.Container.Style = FindResource("TabButton") as Style;
                wi.Value.Close.Visibility = Visibility.Hidden;
                wi.Value.Active = false;
            }

            //Currently limited to 4 tabs, ONLY TMP
            if (Windows.Count != 4)
            {
                int PageId = PagesCreated += 1;

                ExplorerPage explorerPage = new ExplorerPage();
                explorerPage.PageId = PageId;

                Grid Container = new Grid();
                Container.Style = FindResource("TabButtonActive") as Style;
                Container.MouseDown += (se, ea) =>
                {
                    foreach (KeyValuePair<int, WindowInfo> wi in Windows)
                    {
                        if (wi.Key == PageId)
                        {
                            wi.Value.Container.Style = FindResource("TabButtonActive") as Style;
                            wi.Value.Close.Visibility = Windows.Count == 1 ? Visibility.Hidden : Visibility.Visible;
                            wi.Value.Active = true;
                            ExplorerWindow.Content = wi.Value.explorerPage;
                        }
                        else
                        {
                            wi.Value.Container.Style = FindResource("TabButton") as Style;
                            wi.Value.Close.Visibility = Visibility.Hidden;
                            wi.Value.Active = false;
                        }                        
                    }
                };

                Image Icon = new Image();
                Icon.Source = new BitmapImage(new Uri($"Assets/ThisPC.ico", UriKind.Relative));
                Icon.Width = 20;
                Icon.Height = 20;
                Icon.HorizontalAlignment = HorizontalAlignment.Left;
                Icon.VerticalAlignment = VerticalAlignment.Center;
                Icon.Margin = new Thickness(10, 0, 0, 0);

                Label Path = new Label();
                Path.Style = FindResource("BindLabel") as Style;
                Path.FontWeight = FontWeights.Bold;
                Path.FontSize = 14;
                Path.Tag = "This PC";
                Path.Content = "This PC";
                Path.HorizontalAlignment = HorizontalAlignment.Left;
                Path.VerticalAlignment = VerticalAlignment.Center;
                Path.Margin = new Thickness(35, 0, 0, 0);

                Button Close = new Button();
                Close.Style = FindResource("ClearButton") as Style;
                Close.Width = 27;
                Close.Height = 27;
                Close.Content = "\uE8BB";
                Close.FontFamily = new System.Windows.Media.FontFamily("Segoe MDL2 Assets");
                Close.HorizontalAlignment = HorizontalAlignment.Right;
                Close.VerticalAlignment = VerticalAlignment.Center;
                Close.Margin = new Thickness(10, 0, 0, 0);
                Close.Visibility = Windows.Count + 1 == 1 ? Visibility.Hidden : Visibility.Visible;
                Close.Click += (se, ev) =>
                {
                    Tabs.Children.Remove(Container);
                    Windows.Remove(PageId);

                    WindowInfo wi = Windows.First().Value;
                    wi.Container.Style = FindResource("TabButtonActive") as Style;
                    wi.Close.Visibility = Windows.Count == 1 ? Visibility.Hidden : Visibility.Visible;
                    wi.Active = true;
                    ExplorerWindow.Content = wi.explorerPage;

                    if (Windows.Count == 4) { CreateTabBtn.Visibility = Visibility.Hidden; }
                    else { CreateTabBtn.Visibility = Visibility.Visible; }
                };

                Windows.Add(PageId, new WindowInfo
                {
                    explorerPage = explorerPage,
                    Container = Container,
                    Icon = Icon,
                    Path = Path,
                    Close = Close
                });

                Container.Children.Add(Icon);
                Container.Children.Add(Path);
                Container.Children.Add(Close);
                Tabs.Children.Insert(Tabs.Children.Count - 1, Container);
                ExplorerWindow.Content = Windows[PageId].explorerPage;

                //TMP
                if (Windows.Count == 4) { CreateTabBtn.Visibility = Visibility.Hidden; }
                else { CreateTabBtn.Visibility = Visibility.Visible; }
            }
        }
    }
}
