using ImageSorter.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfAnimatedGif;
using static ImageSorter.Helpers.CompareHelper;

namespace ImageSorter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<FileInfo> filePaths;
        private int selectedIndex = 0;
        private bool isMenuHidden = false;
        private string initialPath = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        #region Helpers

        private void ChangeSelection()
        {
            snackNotify.IsActive = false;
            try
            {
                this.Title = $"({selectedIndex + 1}/{filePaths.Count}) - {filePaths.ElementAt(selectedIndex)?.Name ?? "err"}";

                var task = Task.Run(() => LoadImage(filePaths.ElementAt(selectedIndex).FullName));
     

            }
            catch (Exception e)
            {
                Application.Current.Dispatcher.Invoke(() => imageHost.Source = null);
            }
        }

        private void LoadImage(string path)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(path);
            image.EndInit();

            if (filePaths.ElementAt(selectedIndex).Extension == ".gif")
            {
                Application.Current.Dispatcher.Invoke(() => imageHost.Source = null);
                Application.Current.Dispatcher.Invoke(() => ImageBehavior.SetAnimatedSource(imageHost, image));
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() => ImageBehavior.SetAnimatedSource(imageHost, null));
                Application.Current.Dispatcher.Invoke(() => imageHost.Source = new BitmapImage(new Uri(filePaths.ElementAt(selectedIndex).FullName)));
            }
        }

        private void SaveCurrentPicture(bool isUpArrow)
        {
            var fileName = filePaths.ElementAt(selectedIndex).Name;
            var newPath = Settings.Default.SavePathDown + "\\" + fileName;

            if (isUpArrow)
            {
                newPath = Settings.Default.SavePathUP + "\\" + fileName;
            }

            if (File.Exists(newPath))
            {
                var existingFile = new FileInfo(newPath);

                //Is it the same file - Calculate a new name
                if (filePaths.ElementAt(selectedIndex).Length != existingFile.Length)
                {
                    int tries = 0;
                    //When the file already exists
                    while (File.Exists(newPath))
                    {
                        var split = fileName.Split('.');
                        var newName = split.First() + tries + "." + split.Last();

                        if (isUpArrow)
                        {
                            newPath = Settings.Default.SavePathUP + "\\" + newName;
                        }
                        else
                        {
                            newPath = Settings.Default.SavePathDown + "\\" + newName;
                        }

                        tries++;
                        if (tries > 1000)
                        {
                            MessageBox.Show("There are too many files with this name: " + fileName);
                            break;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("The file already exists in this location");
                    return;
                }
            }

            //Save the image
            File.Copy(filePaths.ElementAt(selectedIndex).FullName, newPath);

            //Notification
            snackNotify.IsActive = true;
            System.Threading.Timer timer = null;
            timer = new System.Threading.Timer((obj) =>
                {
                    Application.Current.Dispatcher.BeginInvoke(
                        DispatcherPriority.Background,
                        new Action(() => snackNotify.IsActive = false));
                    timer.Dispose();
                },
                null, 2000, System.Threading.Timeout.Infinite);
        }

        private void LoadItemsFromPath()
        {
            try
            {
                if (initialPath == null)
                {
                    initialPath = Settings.Default.LoadPath;
                    selectedIndex = 0;
                }
                else if (initialPath != Settings.Default.LoadPath)
                {
                    selectedIndex = 0;
                }

                if (string.IsNullOrEmpty(Settings.Default.LoadPath))
                {
                    this.Close();
                }

                this.WindowState = WindowState.Maximized;

                var dinfo = new DirectoryInfo(@"" + Settings.Default.LoadPath);

                // filePaths = dinfo.GetFiles().OrderBy(x => x.Name, new CustomComparer<string>(CompareNatural)).ToList();
                filePaths = GetFiles(dinfo);

                menUp.Header = Settings.Default.SavePathUP.Split('\\').Last();
                menDown.Header = Settings.Default.SavePathDown.Split('\\').Last();

                if (filePaths.Count > 0)
                {
                    ChangeSelection();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static List<FileInfo> GetFiles(DirectoryInfo dir)
        {
            var supported = new[] { ".jpg", ".png", ".gif" };

            return dir.GetFiles().Where(x => supported.Contains(Path.GetExtension(x.FullName)))
                .OrderBy(x => x.Name, new CustomComparer<string>(CompareNatural)).ToList();
        }

        private void ShowPathSelector()
        {
            var selector = new PathSelector
            {
                Owner = this
            };
            selector.Closing += (o, args) => LoadItemsFromPath();
            selector.Show();
        }

        #endregion Helpers

        #region MenuHandlers

        private void menChange_Click(object sender, RoutedEventArgs e)
        {
            ShowPathSelector();
        }

        private void menUp_Click(object sender, RoutedEventArgs e)
        {
            SaveCurrentPicture(true);
        }

        private void menDown_Click(object sender, RoutedEventArgs e)
        {
            SaveCurrentPicture(false);
        }

        #endregion MenuHandlers

        #region MainWindow Handlers

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ShowPathSelector();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right)
            {
                if (selectedIndex != filePaths.Count - 1)
                {
                    selectedIndex++;
                    ChangeSelection();
                }
            }
            else if (e.Key == Key.Left)
            {
                if (selectedIndex != 0)
                {
                    selectedIndex--;
                    ChangeSelection();
                }
            }
            else if (e.Key == Key.Down)
            {
                SaveCurrentPicture(false);
            }
            else if (e.Key == Key.Up)
            {
                SaveCurrentPicture(true);
            }
        }

        /// <summary>
        /// Hide the menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!isMenuHidden && filePaths != null)
            {
                rowMenu.Height = new GridLength(0);
                isMenuHidden = true;
            }
            else
            {
                rowMenu.Height = new GridLength(35);
                isMenuHidden = false;
            }
        }

        #endregion MainWindow Handlers
    }
}