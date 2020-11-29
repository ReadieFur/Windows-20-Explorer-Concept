using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Windows_20_Explorer_Concept
{
    /// <summary>
    /// Interaction logic for ExplorerPage.xaml
    /// </summary>
    public partial class ExplorerPage : Page
    {
        private bool firstLoad = false;
        public int PageId;
        Timer checkForChange = new Timer();

        public ExplorerPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!firstLoad)
            {
                firstLoad = true;

                DataContext = new XAMLStyles { };
                checkForChange.Interval = 1000;
                checkForChange.Elapsed += (se, ea) => { try { if (Styles.themeChanged) { Dispatcher.Invoke(() => { DataContext = new XAMLStyles { }; }); } } catch { } };
                checkForChange.Start();

                IndexThisPC();
            }
        }

        //https://stackoverflow.com/questions/14488796/does-net-provide-an-easy-way-convert-bytes-to-kb-mb-gb-etc
        private string FormatBytes(long value, int decimalPlaces = 1)
        {
            string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

            if (value < 0) { return "-" + FormatBytes(-value); } //Input value is invalid

            int i = 0;
            decimal dValue = value;
            while (Math.Round(dValue, decimalPlaces) >= 1000)
            {
                dValue /= 1024;
                i++;
            }

            return $"{dValue.ToString().Split('.')[0]}.{decimalPlaces} {SizeSuffixes[i]}";
        }

        private void IndexThisPC()
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType == DriveType.Fixed)
                {
                    Grid DriveContainer = new Grid();
                    DriveContainer.Width = 250;
                    DriveContainer.Height = 60;
                    DriveContainer.Margin = new Thickness(10, 5, 5, 0);
                    DriveContainer.MouseDown += (se, ev) => { if (ev.ClickCount == 2) { IndexFiles(drive.Name.Replace("\\", "\\\\")); } };
                    DriveContainer.MouseEnter += (se, ev) => { DriveContainer.Background = Styles.bc("#3FFFFFFF"); };
                    DriveContainer.MouseLeave += (se, ev) => { DriveContainer.Background = null; };

                    Image DriveImage = new Image();
                    DriveImage.Source = new BitmapImage(new Uri(drive.Name.Contains("C:") ? $"Assets/WinDrive.ico" : $"Assets/Drive.ico", UriKind.Relative));
                    DriveImage.Width = 45;
                    DriveImage.Height = 45;
                    DriveImage.Margin = new Thickness(5, 0, 0, 5);
                    DriveImage.HorizontalAlignment = HorizontalAlignment.Left;

                    UniformGrid DriveDataContainer = new UniformGrid();
                    DriveDataContainer.VerticalAlignment = VerticalAlignment.Stretch;
                    DriveDataContainer.HorizontalAlignment = HorizontalAlignment.Stretch;
                    DriveDataContainer.Margin = new Thickness(55, 0, 10, 0);
                    DriveDataContainer.Rows = 3;

                    Label DriveName = new Label();
                    DriveName.Content = $"{(string.IsNullOrWhiteSpace(drive.VolumeLabel) ? "Local Disk" : drive.VolumeLabel)} ({drive.Name.Replace("\\", "")})";
                    DriveName.Height = 25;
                    DriveName.Margin = new Thickness(-2, -2, 0, 0);
                    DriveName.Style = FindResource("BindLabel") as Style;

                    Border SpaceBorder = new Border();
                    SpaceBorder.VerticalAlignment = VerticalAlignment.Stretch;
                    SpaceBorder.HorizontalAlignment = HorizontalAlignment.Stretch;
                    SpaceBorder.Style = FindResource("BorderForeground") as Style;

                    ProgressBar DriveSpace = new ProgressBar();
                    DriveSpace.VerticalAlignment = VerticalAlignment.Stretch;
                    DriveSpace.HorizontalAlignment = HorizontalAlignment.Stretch;
                    DriveSpace.BorderThickness = new Thickness(0);
                    DriveSpace.Style = FindResource("BindProgressBar") as Style;
                    DriveSpace.Value = 100 - Math.Truncate((double)drive.TotalFreeSpace / drive.TotalSize * 100);

                    Label DriveInfo = new Label();
                    DriveInfo.Content = $"{FormatBytes(drive.AvailableFreeSpace)} free of {FormatBytes(drive.TotalSize)}";
                    DriveInfo.Height = 25;
                    DriveInfo.Margin = new Thickness(-2, -4, 0, 0);
                    DriveInfo.Style = FindResource("BindLabel") as Style;

                    DriveDataContainer.Children.Add(DriveName);
                    SpaceBorder.Child = DriveSpace;
                    DriveDataContainer.Children.Add(SpaceBorder);
                    DriveDataContainer.Children.Add(DriveInfo);
                    DriveContainer.Children.Add(DriveImage);
                    DriveContainer.Children.Add(DriveDataContainer);
                    PageContentContainer.Children.Add(DriveContainer);
                }
            }
        }

        string[] InvalidChars = {  };
        List<string> VisitedDirectories = new List<string> {"This PC"};

        private void IndexFiles(string Dir)
        {
            string[] files = Directory.GetFiles(Dir);
        }

        private void PageDir_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (InvalidChars.All(PageDir.Text.Contains)) { MessageBox.Show("The specified path is invalid.", "Invalid Path"); } //No Regex match "new Regex(@"^[<>;*?|]$").IsMatch(PageDir.Text)"
                else
                {
                    try
                    {
                        VisitedDirectories.Add(PageDir.Text.Replace("\"", "").Replace("\\\\", "\\").Replace("\\", "\\\\"));
                        PageDir.Text = VisitedDirectories.Last().Replace("\\\\", " > ");
                        Keyboard.ClearFocus();
                        FocusManager.SetFocusedElement(FocusManager.GetFocusScope(PageDir), null);
                        IndexFiles(VisitedDirectories.Last());
                    }
                    catch (Exception ex) { MessageBox.Show(ex.ToString(), "Error indexing directory"); }
                }                
            }
        }

        private void PageDir_GotFocus(object sender, RoutedEventArgs e) { PageDir.Text = VisitedDirectories.Last().Replace("\\\\", "\\"); }

        private void Page_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (PageDir.IsFocused)
            {
                Keyboard.ClearFocus();
                FocusManager.SetFocusedElement(FocusManager.GetFocusScope(PageDir), null);
                PageDir.Text = VisitedDirectories.Last().Replace("\\\\", " > ");
            }
        }
    }
}
