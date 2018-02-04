using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ImageSorter.Properties;

namespace ImageSorter
{
    /// <summary>
    /// Interaction logic for PathSelector.xaml
    /// </summary>
    public partial class PathSelector : Window
    {
        private bool alreadyOnePath = false;

        public PathSelector()
        {
            InitializeComponent();

            tbLoad.Text = Settings.Default.LoadPath;
            tbSaveUp.Text = Settings.Default.SavePathUP;
            tbSaveDown.Text = Settings.Default.SavePathDown;


            CheckShouldEnableButton();
        }

        private void CheckShouldEnableButton()
        {
            if (ValidatePaths())
                btnClose.IsEnabled = true;
        }


        private string GetPathDialog()
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog(this).GetValueOrDefault())
            {
                if (Directory.Exists(dialog.SelectedPath))
                {
                    return dialog.SelectedPath;
                }
            }

            return null;
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


        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (ValidatePaths())
            {
                Settings.Default.Save();
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid Path detected");
                btnClose.IsEnabled = false;
            }
            
        }

        private void tbLoad_GotFocus(object sender, RoutedEventArgs e)
        {
            var loadPath = "";

            try
            {
                loadPath = GetPathDialog().Replace("\"", string.Empty);
            }
            catch (Exception ex)
            {
                tbLoad.Text = "Invalid Path";
            }

            if (Directory.Exists(loadPath))
            {
                Settings.Default.LoadPath = loadPath;
                tbLoad.Text = loadPath;

                CheckShouldEnableButton();
            }
        }

        private void tbSave_GotFocus(object sender, RoutedEventArgs e)
        {
            var saveLocation = "";

            try
            {
                saveLocation = GetPathDialog().Replace("\"", string.Empty);
            }
            catch (Exception ex)
            {
                tbSaveUp.Text = "Invalid Path";
            }

            if (Directory.Exists(saveLocation))
            {
                Settings.Default.SavePathUP = saveLocation;
                tbSaveUp.Text = saveLocation;

                CheckShouldEnableButton();
            }

        }

        private void tbSaveDown_GotFocus(object sender, RoutedEventArgs e)
        {
            var saveLocation = "";

            try
            {
                saveLocation = GetPathDialog().Replace("\"", string.Empty);
            }
            catch (Exception ex)
            {
                tbSaveDown.Text = "Invalid Path";
            }

            if (Directory.Exists(saveLocation))
            {
                Settings.Default.SavePathDown = saveLocation;
                tbSaveDown.Text = saveLocation;

                CheckShouldEnableButton();
            }
        }
    }
}
