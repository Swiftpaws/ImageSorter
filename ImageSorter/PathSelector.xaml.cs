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
        public PathSelector()
        {
            InitializeComponent();

            tbLoad.Text = Settings.Default.LoadPath;
            tbSaveUp.Text = Settings.Default.SavePathUP;
            tbSaveDown.Text = Settings.Default.SavePathDown;
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
            return (Directory.Exists(tbLoad.Text) && Directory.Exists(tbSaveUp.Text) && Directory.Exists(tbSaveDown.Text));
        }


        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (ValidatePaths())
            {
                Console.WriteLine("Valid Paths");
                Settings.Default.Save();
                this.Close();
            }
            else
            {
                MessageBox.Show("One or both paths are invalid");
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
            }
        }
    }
}
