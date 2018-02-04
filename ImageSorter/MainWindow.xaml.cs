using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ImageSorter.Properties;
using MaterialDesignThemes.Wpf;

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


        public MainWindow()
        {
            InitializeComponent();
        }

        #region  Helpers

        #region SorterWindowsStyle

        public static int CompareNatural(string strA, string strB)
        {
            return CompareNatural(strA, strB, CultureInfo.CurrentCulture, CompareOptions.IgnoreCase);
        }

        public static int CompareNatural(string strA, string strB, CultureInfo culture, CompareOptions options)
        {
            CompareInfo cmp = culture.CompareInfo;
            int iA = 0;
            int iB = 0;
            int softResult = 0;
            int softResultWeight = 0;
            while (iA < strA.Length && iB < strB.Length)
            {
                bool isDigitA = Char.IsDigit(strA[iA]);
                bool isDigitB = Char.IsDigit(strB[iB]);
                if (isDigitA != isDigitB)
                {
                    return cmp.Compare(strA, iA, strB, iB, options);
                }
                else if (!isDigitA && !isDigitB)
                {
                    int jA = iA + 1;
                    int jB = iB + 1;
                    while (jA < strA.Length && !Char.IsDigit(strA[jA])) jA++;
                    while (jB < strB.Length && !Char.IsDigit(strB[jB])) jB++;
                    int cmpResult = cmp.Compare(strA, iA, jA - iA, strB, iB, jB - iB, options);
                    if (cmpResult != 0)
                    {
                        string sectionA = strA.Substring(iA, jA - iA);
                        string sectionB = strB.Substring(iB, jB - iB);
                        if (cmp.Compare(sectionA + "1", sectionB + "2", options) ==
                            cmp.Compare(sectionA + "2", sectionB + "1", options))
                        {
                            return cmp.Compare(strA, iA, strB, iB, options);
                        }
                        else if (softResultWeight < 1)
                        {
                            softResult = cmpResult;
                            softResultWeight = 1;
                        }
                    }
                    iA = jA;
                    iB = jB;
                }
                else
                {
                    char zeroA = (char)(strA[iA] - (int)Char.GetNumericValue(strA[iA]));
                    char zeroB = (char)(strB[iB] - (int)Char.GetNumericValue(strB[iB]));
                    int jA = iA;
                    int jB = iB;
                    while (jA < strA.Length && strA[jA] == zeroA) jA++;
                    while (jB < strB.Length && strB[jB] == zeroB) jB++;
                    int resultIfSameLength = 0;
                    do
                    {
                        isDigitA = jA < strA.Length && Char.IsDigit(strA[jA]);
                        isDigitB = jB < strB.Length && Char.IsDigit(strB[jB]);
                        int numA = isDigitA ? (int)Char.GetNumericValue(strA[jA]) : 0;
                        int numB = isDigitB ? (int)Char.GetNumericValue(strB[jB]) : 0;
                        if (isDigitA && (char)(strA[jA] - numA) != zeroA) isDigitA = false;
                        if (isDigitB && (char)(strB[jB] - numB) != zeroB) isDigitB = false;
                        if (isDigitA && isDigitB)
                        {
                            if (numA != numB && resultIfSameLength == 0)
                            {
                                resultIfSameLength = numA < numB ? -1 : 1;
                            }
                            jA++;
                            jB++;
                        }
                    }
                    while (isDigitA && isDigitB);
                    if (isDigitA != isDigitB)
                    {
                        return isDigitA ? 1 : -1;
                    }
                    else if (resultIfSameLength != 0)
                    {
                        return resultIfSameLength;
                    }
                    int lA = jA - iA;
                    int lB = jB - iB;
                    if (lA != lB)
                    {
                        return lA > lB ? -1 : 1;
                    }
                    else if (zeroA != zeroB && softResultWeight < 2)
                    {
                        softResult = cmp.Compare(strA, iA, 1, strB, iB, 1, options);
                        softResultWeight = 2;
                    }
                    iA = jA;
                    iB = jB;
                }
            }
            if (iA < strA.Length || iB < strB.Length)
            {
                return iA < strA.Length ? 1 : -1;
            }
            else if (softResult != 0)
            {
                return softResult;
            }
            return 0;
        }

        public class CustomComparer<T> : IComparer<T>
        {
            private Comparison<T> _comparison;

            public CustomComparer(Comparison<T> comparison)
            {
                _comparison = comparison;
            }

            public int Compare(T x, T y)
            {
                return _comparison(x, y);
            }
        }


        #endregion


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
                MessageBox.Show("file already exists");
            }


            var image = new BitmapImage(new Uri(filePaths.ElementAt(selectedIndex).FullName));
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));

            using (var fileStream = new System.IO.FileStream(newPath, System.IO.FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }

        private void LoadItemsFromPath()
        {
            if (string.IsNullOrEmpty(Settings.Default.LoadPath))
            {
                this.Close();
            }

            this.WindowState = WindowState.Maximized;

            var dinfo = new DirectoryInfo(@"" + Settings.Default.LoadPath);

            filePaths = dinfo.GetFiles().OrderBy(x => x.Name, new CustomComparer<string>(CompareNatural)).ToList();

            foreach (var VARIABLE in filePaths)
            {
                Console.WriteLine(VARIABLE);
            }

            if (filePaths.Count > 0)
            {
                imageHost.Source = new BitmapImage(new Uri(filePaths.First().FullName));
            }
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


        #endregion


        #region MenuHandlers

        private void menChange_Click(object sender, RoutedEventArgs e)
        {
            ShowPathSelector();
        }

        #endregion

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
                    imageHost.Source = new BitmapImage(new Uri(filePaths.ElementAt(selectedIndex).FullName));
                }

            }
            else if (e.Key == Key.Left)
            {
                if (selectedIndex != 0)
                {
                    selectedIndex--;
                    imageHost.Source = new BitmapImage(new Uri(filePaths.ElementAt(selectedIndex).FullName));
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


        #endregion

        
    }
}
