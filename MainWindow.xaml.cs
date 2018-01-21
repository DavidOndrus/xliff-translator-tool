using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace XliffTranslatorTool
{
    public partial class MainWindow : Window
    {
        private string OpenedFileName { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            ImportFileMenuOption.IsEnabled = false;
            SaveAsMenuOption.IsEnabled = false;
        }

        private void OpenFileMenuOption_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = CreateOpenFileDialog();
            bool? result = openFileDialog.ShowDialog();

            string filePath = openFileDialog.FileName;
            OpenedFileName = openFileDialog.SafeFileName;

            if (result == true)
            {
                //send filepath to parser
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
