using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Xml;
using XliffTranslatorTool.Parser;
using System.Linq;
using System.Runtime.CompilerServices;

namespace XliffTranslatorTool
{
    public partial class MainWindow : Window
    {
        private string _OpenedFilePath;

        private XliffParser _XliffParser = new XliffParser();
        private ObservableCollection<TranslationUnit> _TranslationUnits;

        private bool _ItemsDirty = false;
        private Visibility _IsShowMetaData = Visibility.Hidden;
        private bool _IsShowDublicatesOnly = false;
        private ToolStates CurrentState = ToolStates.None;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetToolState(ToolStates.Loaded);
            UpdateTitle();
            UpdateColumnVisibility();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_ItemsDirty)
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Save changes?", "Question", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                switch (messageBoxResult)
                {
                    case MessageBoxResult.Yes:
                        Save(_XliffParser.CreateXliffDocument(_XliffParser.GetLastFileXliffVersion(), MainDataGrid.ItemsSource), _OpenedFilePath);
                        break;
                    case MessageBoxResult.Cancel:
                            e.Cancel = true;
                            break;
                    case MessageBoxResult.No:
                            e.Cancel = false;
                            break;
                    default:
                        break;
                }
            }
        }

        private void UpdateTitle()
        {
            string dirtySuffix = "";
            if (_ItemsDirty)
                dirtySuffix = "*";

            if (String.IsNullOrEmpty(_OpenedFilePath))
                Title = "XLIFF Translator Tool";
            else
                Title = "XLIFF Translator Tool  (" + _OpenedFilePath + ")" + dirtySuffix;
        }

        private void SetToolState(ToolStates newState)
        {
            CurrentState = newState;
            switch (CurrentState)
            {
                case ToolStates.Loaded:
                    {
                        ImportFileMenuOption.IsEnabled = false;
                        SaveAsMenuOption.IsEnabled = false;
                        SaveMenuOption.IsEnabled = false;
                        MainDataGrid.Visibility = Visibility.Hidden;
                        TrimOption.IsEnabled = false;
                        ShowDublicatesOnlyOption.IsEnabled = false;
                        break;
                    }
                case ToolStates.FileOpened:
                    {
                        ImportFileMenuOption.IsEnabled = true;
                        SaveAsMenuOption.IsEnabled = true;
                        SaveMenuOption.IsEnabled = true;
                        MainDataGrid.Visibility = Visibility.Visible;
                        TrimOption.IsEnabled = true;
                        ShowDublicatesOnlyOption.IsEnabled = true;
                        break;
                    }
                default:
                    break;
            }
        }

        private void OpenFileMenuOption_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentState == ToolStates.FileOpened)
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Opening a new file will overwrite your current data and your changes will be lost.\nContinue ?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (messageBoxResult == MessageBoxResult.No)
                    return;
            }

            OpenFile();
        }

        private void OpenFile()
        {
            OpenFileDialog openFileDialog = CreateOpenFileDialog();
            bool? result = openFileDialog.ShowDialog();

            _OpenedFilePath = openFileDialog.FileName;
            if (result == true)
            {
                _TranslationUnits = _XliffParser.GetTranslationUnitsFromFile(_OpenedFilePath);
                if (AreTranslationUnitsValid(_TranslationUnits))
                {
                    UpdateUnitListDisplay();
                    SetToolState(ToolStates.FileOpened);
                    UpdateTitle();
                }
            }
        }

        private void ImportFileMenuOption_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("YES - update existing records if ID of translation unit matches\nNO - import only new translation units", "Import mode", MessageBoxButton.YesNo, MessageBoxImage.Question);
            switch (messageBoxResult)
            {
                case MessageBoxResult.Yes:
                    {
                        ImportFile(true);
                        break;
                    }
                case MessageBoxResult.No:
                    {
                        ImportFile(false);
                        break;
                    }
                case MessageBoxResult.None:
                    return;
                default:
                    throw new NotImplementedException($"Not implemented MessageBoxResult '{messageBoxResult.ToString()}'");
            }
        }

        private void ImportFile(bool updateExisting)
        {
            OpenFileDialog openFileDialog = CreateOpenFileDialog();
            openFileDialog.Title = "Import";
            bool? result = openFileDialog.ShowDialog();

            string filePath = openFileDialog.FileName;

            if (result == true)
            {
                UpdateTitle();

                ObservableCollection<TranslationUnit> newTranslationUnits = _XliffParser.GetTranslationUnitsFromFile(filePath);
                if (AreTranslationUnitsValid(newTranslationUnits))
                {
                    ObservableCollection<TranslationUnit> list = (ObservableCollection<TranslationUnit>)MainDataGrid.ItemsSource;
                    foreach (TranslationUnit translationUnit in newTranslationUnits)
                    {
                        if (!list.Contains(translationUnit))
                        {
                            list.Add(translationUnit);
                        }
                        else
                        {
                            if (updateExisting)
                            {
                                TranslationUnit originalTranslationUnit = list.Where(otu => otu.Identifier == translationUnit.Identifier).FirstOrDefault();
                                if (originalTranslationUnit != null)
                                {
                                    originalTranslationUnit.Description = translationUnit.Description;
                                    originalTranslationUnit.Meaning = translationUnit.Meaning;
                                    originalTranslationUnit.Source = translationUnit.Source;
                                    originalTranslationUnit.Target = translationUnit.Target;
                                }
                            }
                        }
                    }
                    MainDataGrid.Items.Refresh();
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

        private bool AreTranslationUnitsValid(IList<TranslationUnit> translationUnits)
        {
            if (translationUnits == null)
            {
                MessageBox.Show($"XLIFF version was not recognized. Supported versions are: {String.Join(", ", Constants.XLIFF_VERSION_V12, Constants.XLIFF_VERSION_V20)}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (translationUnits.Count == 0)
            {
                MessageBox.Show("0 translations found", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                return true;
            }

            return false;
        }

        private void SaveMenuOption_Click(object sender, RoutedEventArgs e)
        {
            Save(_XliffParser.CreateXliffDocument(_XliffParser.GetLastFileXliffVersion(), MainDataGrid.ItemsSource), _OpenedFilePath);
        }

        private void SaveAsMenuOption_Click(object sender, RoutedEventArgs e)
        {
            SaveAs();
        }

        private void SaveAs()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = Path.GetFileName(_OpenedFilePath),
                DefaultExt = Constants.FILE_DIALOG_DEFAULT_EXT,
                Filter = Constants.FILE_DIALOG_FILTER,
                CheckPathExists = true,
                OverwritePrompt = true,
                AddExtension = true
            };

            bool? result = saveFileDialog.ShowDialog();

            if (result == true)
            {
                XmlDocument xmlDocument = null;
                MessageBoxResult messageBoxResult = MessageBox.Show("YES - XLIFF version 1.2 (default)\nNO - XLIFF version 2.0", "XLIFF version", MessageBoxButton.YesNo, MessageBoxImage.Question);
                switch (messageBoxResult)
                {
                    case MessageBoxResult.Yes:
                        {
                            xmlDocument = _XliffParser.CreateXliffDocument(XliffParser.XliffVersion.V12, MainDataGrid.ItemsSource);
                            break;
                        }
                    case MessageBoxResult.No:
                        {
                            xmlDocument = _XliffParser.CreateXliffDocument(XliffParser.XliffVersion.V20, MainDataGrid.ItemsSource);
                            break;
                        }
                    case MessageBoxResult.None:
                        return;
                    default:
                        throw new NotImplementedException($"Not implemented MessageBoxResult '{messageBoxResult.ToString()}'");
                }

                Save(xmlDocument, saveFileDialog.FileName);
            }
        }

        private void Save(XmlDocument xmlDocument, string filePath)
        {
            WriteToFile(xmlDocument, filePath);
            SetDirty(false);
            MessageBox.Show("Saved", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void WriteToFile(XmlDocument xmlDocument, string filePath)
        {
            StringWriter stringWriter = new StringWriter();
            xmlDocument.Save(stringWriter);
            string indented = stringWriter.ToString().Replace("_AMP;_", "&").Replace("_LT;_", "<").Replace("_GT;_", ">");
            using (StreamWriter streamWriter = new StreamWriter(filePath, false))
            {
                streamWriter.Write(indented);
            }
        }


        private void ShowMetaDataToggle_Click(object sender, RoutedEventArgs e)
        {
            if (_IsShowMetaData == Visibility.Hidden)
            {
                _IsShowMetaData = Visibility.Visible;
                ShowMetaDataOption.IsChecked = true;
            }
            else
            {
                _IsShowMetaData = Visibility.Hidden;
                ShowMetaDataOption.IsChecked = false;
            }

            UpdateColumnVisibility();
        }

        private void UpdateColumnVisibility()
        {
            IdColumn.Visibility = _IsShowMetaData;
            MeanColumn.Visibility = _IsShowMetaData;
            DescColumn.Visibility = _IsShowMetaData;
        }

        private void TrimBlanks_Click(object sender, RoutedEventArgs e)
        {
            if (_TranslationUnits != null)
            {
                foreach (var translationUnit in _TranslationUnits)
                {
                    translationUnit.Target = translationUnit.Target.TrimStart(' ');
                    translationUnit.Target = translationUnit.Target.TrimEnd(' ');
                }

                UpdateUnitListDisplay();
            }
        }


        private void MainDataGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            SetDirty(true);
            UpdateUnitListDisplay();
        }

        private void SetDirty(bool newDirty)
        {
            _ItemsDirty = newDirty;
            UpdateTitle();
        }


        private void ShowDublicatesOnlyToggle_Click(object sender, RoutedEventArgs e)
        {
            _IsShowDublicatesOnly = !_IsShowDublicatesOnly;
            UpdateUnitListDisplay();
        }

        private void UpdateUnitListDisplay()
        {
            //Pre-Update
            UpdateDublicates();

            //Update
            MainDataGrid.ItemsSource = _TranslationUnits;
            MainDataGrid.Items.Refresh();

            //Post-Update
            if (_IsShowDublicatesOnly)
                SortAscendingSource();
        }

        private void UpdateDublicates()
        {
            foreach (var translationUnit in _TranslationUnits)
            {
                translationUnit.IsVisible = !_IsShowDublicatesOnly;
                translationUnit.IsMarked = false;
            }

            if (_IsShowDublicatesOnly)
            {
                var duplicates = _TranslationUnits.GroupBy(x => x.Source)
                                                        .Where(g => g.Count() > 1)
                                                        .ToList();

                foreach (var duplicateGroup in duplicates)
                {
                    var referenceUnit = duplicateGroup.ElementAt(0);
                    foreach (var dublicate in duplicateGroup)
                    {
                        dublicate.IsVisible = true;
                        if (dublicate.Target.CompareTo(referenceUnit.Target) != 0)
                        {
                            referenceUnit.IsMarked = true;
                            dublicate.IsMarked = true;
                        }
                    }
                }
            }
        }

        private void SortAscendingSource()
        {
            MainDataGrid.Items.SortDescriptions.Clear();
            MainDataGrid.Items.SortDescriptions.Add(new SortDescription("Source", ListSortDirection.Ascending));
            MainDataGrid.Items.Refresh();
        }

    }
}
