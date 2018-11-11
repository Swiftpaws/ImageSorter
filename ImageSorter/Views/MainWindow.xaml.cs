using ImageSorter.Helpers;
using ImageSorter.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;
using static ImageSorter.Helpers.CompareHelper;

namespace ImageSorter.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<FileInfo> filePaths;
        private string lastSavePath = string.Empty;
        private int selectedIndex = 0;
        private string initialPath = null;

        private Timer SelectionChangeTimer;

        public MainWindow()
        {
            InitializeComponent();
        }

        #region Helpers

        private void ChangeSelection()
        {
            if(filePaths.Count <= selectedIndex) return;
            Title = $"({selectedIndex + 1}/{filePaths.Count}) - {filePaths.ElementAt(selectedIndex)?.Name ?? "err"}";

            SelectionChangeTimer?.Change(Timeout.Infinite, Timeout.Infinite);

            SelectionChangeTimer = new Timer((obj) =>
            {
                Dispatcher.InvokeAsync(ChangeSelection_Work);
            }, null, 50, Timeout.Infinite);
        }

        private void ChangeSelection_Work()
        {
            snackNotify.IsActive = false;
            try
            {
                GC.Collect();

                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(filePaths.ElementAt(selectedIndex).FullName);
                image.EndInit();

                if (filePaths.ElementAt(selectedIndex).Extension == ".gif")
                {
                    imageHost.Source = null;
                    ImageBehavior.SetAnimatedSource(imageHost, image);
                }
                else
                {
                    ImageBehavior.SetAnimatedSource(imageHost, null);
                    imageHost.Source = image;
                }
            }
            catch (Exception)
            {
                Application.Current.Dispatcher.Invoke(() => imageHost.Source = null);
            }
        }

        private void SaveCurrentPicture(bool isUpArrow)
        {
            var fileName = filePaths.ElementAt(selectedIndex).Name;
            var newPath = $"{Settings.Default.SavePathDown}\\{fileName}";

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
                    var tries = 0;
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

            lastSavePath = newPath;

            //Notification
            snackNotify.IsActive = true;
            snackNotifyMessage.Content = "Saved to: " + newPath;
        }

        private void DeleteLoadFolder()
        {
            var dirInfo = new DirectoryInfo(Settings.Default.LoadPath);
            var files = dirInfo.GetFiles();
            var respFolder = MessageBox.Show("The selected folder contains no remaining relevant files\nDelete it?", dirInfo.FullName, MessageBoxButton.YesNo);
            if (respFolder == MessageBoxResult.Yes)
            {
                if (files.Length > 0)
                {
                    var filesText = files.Aggregate(string.Empty, (current, v) => current + (v + "\n"));
                    var responseFolderWithContent = MessageBox.Show("The folder still contains some non media files\n"+filesText, "Confirm deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (responseFolderWithContent == MessageBoxResult.Yes)
                    {
                        foreach (var f in files)
                        {
                            f.Delete();
                        }
                    }
                }

                files = dirInfo.GetFiles();
                if(files.Any())
                    return;
                
                dirInfo.Delete();
                ShowPathSelector();
            }
        }

        private void DeleteViewedFiles()
        {
            var countToDelete = selectedIndex + 1;
            var response = MessageBox.Show($"Confirm deletion of {countToDelete} file(s)", "Are you sure about that?",
                MessageBoxButton.YesNo);

            if (response == MessageBoxResult.No) return;

            imageHost.Source = null;
            ImageBehavior.SetAnimatedSource(imageHost, null);

            GC.Collect();
            GC.WaitForPendingFinalizers();

            var filesToDelete = filePaths.Take(countToDelete).ToList();
            var countDeleted = filesToDelete.DeleteAllFiles();

            filePaths = filePaths.Except(filesToDelete).ToList();
            selectedIndex = 0;
            ChangeSelection();

            if (countDeleted != filesToDelete.Count)
                MessageBox.Show($"Deleted {countDeleted} file(s) instead of {filesToDelete.Count}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            else
                MessageBox.Show($"Deleted {countDeleted} file(s)", "Finished", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    Close();
                }

                //this.WindowState = WindowState.Maximized;
                imageHost.Focus();

                filePaths = GetFiles(Settings.Default.LoadPath);

                menUp.Header = Settings.Default.SavePathUP.Split('\\').Last();
                menDown.Header = Settings.Default.SavePathDown.Split('\\').Last();
                menChange.Header = Settings.Default.LoadPath.Split('\\').Last();

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

        public static List<FileInfo> GetFiles(string path)
        {
            var supported = new[] { ".jpg", ".png", ".gif" };

            return new DirectoryInfo(path).GetFiles().Where(x => supported.Contains(Path.GetExtension(x.FullName)))
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

        private void SnackbarMessage_ActionClick(object sender, RoutedEventArgs e)
        {
            try
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                File.Delete(lastSavePath);
                snackNotify.IsActive = false;
            }
            catch (Exception)
            {
                MessageBox.Show("Undo failed");
            }
        }

        private void menChange_Click(object sender, RoutedEventArgs e)
        {
            PathSelector.ExitOnAbortOverride = true;
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

        private void MenDelete_OnClick(object sender, RoutedEventArgs e)
        {
            if(filePaths.Count > 0)
                DeleteViewedFiles();

            if (filePaths.Count == 0)
                DeleteLoadFolder();
        }



        #endregion MenuHandlers

        #region MainWindow Handlers

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ShowPathSelector();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Right:
                    {
                        if (selectedIndex != filePaths.Count - 1)
                        {
                            selectedIndex++;
                            ChangeSelection();
                        }

                        break;
                    }
                case Key.Left:
                    {
                        if (selectedIndex != 0)
                        {
                            selectedIndex--;
                            ChangeSelection();
                        }

                        break;
                    }
                case Key.Down:
                    SaveCurrentPicture(isUpArrow: false);
                    break;

                case Key.Up:
                    SaveCurrentPicture(isUpArrow: true);
                    break;

                case Key.Delete:
                    MenDelete_OnClick(null, null);
                    break;
            }
        }

        /// <summary>
        /// Hide the menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (filePaths == null) return;

            menU.Visibility = menU.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        #endregion MainWindow Handlers
    }
}