using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using XliffTranslatorTool.Parser;

namespace XliffTranslatorTool
{
    public partial class MainWindow : Window
    {
        private string OpenedFileName { get; set; }
        private XliffParser XliffParser { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            ImportFileMenuOption.IsEnabled = false;
            SaveAsMenuOption.IsEnabled = false;
            MainDataGrid.Visibility = Visibility.Hidden;
        }

        private void OpenFileMenuOption_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = CreateOpenFileDialog();
            bool? result = openFileDialog.ShowDialog();

            string filePath = openFileDialog.FileName;
            OpenedFileName = openFileDialog.SafeFileName;

            if (result == true)
            {
                XliffParser = new XliffParser(filePath);
                IList<TranslationUnit> translationUnits = XliffParser.GetTranslationUnits();
                if (translationUnits == null)
                {
                    MessageBox.Show($"XLIFF version was not recognized. Supported versions are: {String.Join(", ", Constants.XLIFF_VERSION_V12, Constants.XLIFF_VERSION_V20)}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (translationUnits.Count == 0)
                {
                    MessageBox.Show("0 translations loaded.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MainDataGrid.ItemsSource = translationUnits;
                    MainDataGrid.Visibility = Visibility.Visible;
                }
            }
        }

        private void ImportFileMenuOption_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = CreateOpenFileDialog();
            bool? result = openFileDialog.ShowDialog();

            string filePath = openFileDialog.FileName;

            if (result == true)
            {
                //send filepath to parser
            }
        }

        private void SaveAsMenuOption_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = OpenedFileName,
                DefaultExt = Constants.FILE_DIALOG_DEFAULT_EXT,
                Filter = Constants.FILE_DIALOG_FILTER,
                CheckFileExists = true,
                CheckPathExists = true,
                OverwritePrompt = true,
                AddExtension = true
            };

            bool? result = saveFileDialog.ShowDialog();

            if (result == true)
            {
                using (StreamWriter sw = new StreamWriter(saveFileDialog.FileName))
                {
                    //write xml to file
                }
            }
        }

        private static OpenFileDialog CreateOpenFileDialog()
        {
            return new OpenFileDialog
            {
                DefaultExt = Constants.FILE_DIALOG_DEFAULT_EXT,
                Filter = Constants.FILE_DIALOG_FILTER,
                Multiselect = false
            };
        }
    }
}
