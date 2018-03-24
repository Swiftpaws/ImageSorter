using ImageSorter.Properties;
using System;
using System.IO;
using System.Windows;

namespace ImageSorter
{
    /// <summary>
    /// Interaction logic for PathSelector.xaml
    /// </summary>
    public partial class PathSelector : Window
    {
        private bool alreadyOnePath = false;
        private bool shouldExitOnX = true;

        public PathSelector()
        {
            InitializeComponent();

            tbLoad.Text = Settings.Default.LoadPath;
            tbSaveUp.Text = Settings.Default.SavePathUP;
            tbSaveDown.Text = Settings.Default.SavePathDown;

            CheckShouldEnableButton();
        }

        #region EventHandlers

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            var loadPath = "";
            try
            {
                loadPath = GetPathDialog().Replace("\"", string.Empty);
            }
            catch (Exception ex)
            {
                //dont care
            }
            if (loadPath != string.Empty)
            {
                Settings.Default.LoadPath = loadPath;
                tbLoad.Text = loadPath;

                CheckShouldEnableButton();
            }
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            var saveLocation = "";
            try
            {
                saveLocation = GetPathDialog().Replace("\"", string.Empty);
            }
            catch (Exception ex)
            {
            }

            if (saveLocation != string.Empty)
            {
                Settings.Default.SavePathUP = saveLocation;
                tbSaveUp.Text = saveLocation;

                CheckShouldEnableButton();
            }
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            var saveLocation = "";
            try
            {
                saveLocation = GetPathDialog().Replace("\"", string.Empty);
            }
            catch (Exception ex)
            {
            }

            if (saveLocation != string.Empty)
            {
                Settings.Default.SavePathDown = saveLocation;
                tbSaveDown.Text = saveLocation;

                CheckShouldEnableButton();
            }
        }
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (ValidatePaths())
            {
                shouldExitOnX = false;
                Settings.Default.Save();
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid Path detected");
                btnOK.IsEnabled = false;
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(shouldExitOnX)
                Environment.Exit(1);
        }
        #endregion EventHandlers

        #region Helpers

        private string GetPathDialog()
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog(this).GetValueOrDefault() && Directory.Exists(dialog.SelectedPath))
                return dialog.SelectedPath;

            return string.Empty;
        }

        private void CheckShouldEnableButton()
        {
            btnOK.IsEnabled = ValidatePaths();
        }

        private bool ValidatePaths()
        {
            if (Directory.Exists(tbLoad.Text))
            {
                if (Directory.Exists(tbSaveUp.Text) && !Directory.Exists(tbSaveDown.Text))
                {
                    Settings.Default.SavePathDown = tbSaveUp.Text;
                    return true;
                }
                else if (!Directory.Exists(tbSaveUp.Text) && Directory.Exists(tbSaveDown.Text))
                {
                    Settings.Default.SavePathUP = tbSaveDown.Text;
                    return true;
                }
                else if (Directory.Exists(tbSaveUp.Text) && Directory.Exists(tbSaveDown.Text))
                {
                    return true;
                }
            }

            return false;
        }


        #endregion Helpers
    }
}