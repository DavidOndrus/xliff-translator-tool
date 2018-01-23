using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using XliffTranslatorTool.Parser;

namespace XliffTranslatorTool
{
    public partial class MainWindow : Window
    {
        private enum State
        {
            Loaded,
            FileOpened
        }

        private string OpenedFileName { get; set; }
        private XliffParser XliffParser { get; set; } = new XliffParser();
        private State _currentState;
        private State CurrentState
        {
            get => _currentState;
            set
            {
                _currentState = value;
                OnStateChanged();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetState(State.Loaded);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (MainDataGrid.HasItems)
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Save as new file ?", "Question", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                switch (messageBoxResult)
                {
                    case MessageBoxResult.None:
                    case MessageBoxResult.Cancel:
                        {
                            e.Cancel = true;
                            break;
                        }
                    case MessageBoxResult.Yes:
                        {
                            SaveAs();
                            break;
                        }
                    case MessageBoxResult.No:
                        {
                            e.Cancel = false;
                            break;
                        }
                }
            }
        }

        private void OnStateChanged()
        {
            switch (CurrentState)
            {
                case State.Loaded:
                    {
                        ImportFileMenuOption.IsEnabled = false;
                        SaveAsMenuOption.IsEnabled = false;
                        MainDataGrid.Visibility = Visibility.Hidden;
                        break;
                    }
                case State.FileOpened:
                    {
                        ImportFileMenuOption.IsEnabled = true;
                        SaveAsMenuOption.IsEnabled = true;
                        MainDataGrid.Visibility = Visibility.Visible;
                        break;
                    }
                default: throw new NotImplementedException($"WindowState '{CurrentState.ToString()}' not implemented");
            }
        }

        private void SetState(State state)
        {
            CurrentState = state;
        }

        private void OpenFileMenuOption_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentState == State.FileOpened)
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Opening a new file will overwrite your current data and your changes will be lost.\nContinue ?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (messageBoxResult == MessageBoxResult.No)
                {
                    return;
                }
            }

            OpenFile();
        }

        private void OpenFile()
        {
            OpenFileDialog openFileDialog = CreateOpenFileDialog();
            bool? result = openFileDialog.ShowDialog();

            string filePath = openFileDialog.FileName;
            OpenedFileName = openFileDialog.SafeFileName;

            if (result == true)
            {
                IList<TranslationUnit> translationUnits = XliffParser.GetTranslationUnitsFromFile(filePath);
                if (translationUnits == null)
                {
                    MessageBox.Show($"XLIFF version was not recognized. Supported versions are: {String.Join(", ", Constants.XLIFF_VERSION_V12, Constants.XLIFF_VERSION_V20)}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (translationUnits.Count == 0)
                {
                    MessageBox.Show("0 translations loaded", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MainDataGrid.ItemsSource = translationUnits;
                    SetState(State.FileOpened);
                }
            }
        }

        private void ImportFileMenuOption_Click(object sender, RoutedEventArgs e)
        {
            ImportFile();
        }

        private static void ImportFile()
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
            SaveAs();
        }

        private void SaveAs()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = OpenedFileName,
                DefaultExt = Constants.FILE_DIALOG_DEFAULT_EXT,
                Filter = Constants.FILE_DIALOG_FILTER,
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
