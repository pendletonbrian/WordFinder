using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Microsoft.Win32;
using WordFinder.Classes;
using WordFinder.ViewModel;

namespace WordFinder.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private Members

        private MainWindowViewModel m_ViewModel;

        #endregion Private Members

        #region constructor

        public MainWindow()
        {
            InitializeComponent();

            m_ViewModel = new MainWindowViewModel();

            DataContext = m_ViewModel;
        }

        #endregion constructor

        #region Public Methods

        internal void ShowErrorMessage(string msg)
        {
            _ = MessageBox.Show(this, msg, MainWindowViewModel.BASE_TITLE_TEXT, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #endregion Public Methods

        #region Private Methods

        private void ExcludedLetterButton_CheckedChanged(object sender, RoutedEventArgs e)
        {
            var btn = sender as ToggleButton;

            if (btn is null)
            {
                return;
            }

            var isChecked = btn.IsChecked;

            if (isChecked.HasValue == false)
            {
                return;
            }

            var enumName = btn.Content.ToString();

            if (string.IsNullOrWhiteSpace(enumName))
            {
                return;
            }

            var enumValue = enumName.GetValueFromDescription<Enumerations.Letters>();

            if (btn.IsChecked.Value)
            {
                m_ViewModel.AddExcludedLetter(enumValue);
            }
            else
            {
                m_ViewModel.RemoveExcludedLetter(enumValue);
            }

        }

        private void IncludedLetterButton_CheckedChanged(object sender, RoutedEventArgs e)
        {
            var btn = sender as ToggleButton;

            if (btn is null)
            {
                return;
            }

            var isChecked = btn.IsChecked;

            if (isChecked.HasValue == false)
            {
                return;
            }

            var enumName = btn.Content.ToString();

            if (string.IsNullOrWhiteSpace(enumName))
            {
                return;
            }

            var enumValue = enumName.GetValueFromDescription<Enumerations.Letters>();

            if (btn.IsChecked.Value)
            {
                m_ViewModel.AddIncludedLetter(enumValue);
            }
            else
            {
                m_ViewModel.RemoveIncludedLetter(enumValue);
            }

        }

        private void SearchCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                m_ViewModel.PerformWordSearch();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());

                ShowErrorMessage($"Exception searching list: \"{ex.Message}\".");
            }
            finally
            {
                m_ViewModel.ShowProgressBar = false;
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var dataDir = AppDomain.CurrentDomain.BaseDirectory;

                dataDir = Path.Combine(dataDir, "data");

                if (Directory.Exists(dataDir) == false)
                {
                    throw new DirectoryNotFoundException($"Data directory at \"{dataDir}\" does not exist.");
                }

                var wordListFile = Path.Combine(dataDir, MainWindowViewModel.WORD_LIST_FILE_NAME);

                if (File.Exists(wordListFile) == false)
                {
                    m_ViewModel.SetStatusText($"Word list does not exist at \"{wordListFile}\".", false);

                    return;
                }

                m_ViewModel.ShowProgressBar = true;

                m_ViewModel.SetStatusText($"Loading \"{wordListFile}\".");

                Stopwatch timer = Stopwatch.StartNew();

                await m_ViewModel.LoadAsync(wordListFile);

                timer.Stop();

                m_ViewModel.SetStatusText($"Loaded {m_ViewModel.WordList.Count:###,###,##0} entries in {timer.Elapsed}.");

                m_ViewModel.PerformWordSearch();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());

                ShowErrorMessage($"Exception during load: \"{ex.Message}\".");
            }
            finally
            {
                m_ViewModel.ShowProgressBar = false;
            }
        }

        private void ReadFileCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                if (m_ViewModel is null)
                {
                    e.CanExecute = false;
                }
                else
                {
                    e.CanExecute = m_ViewModel.IsBusy == false;
                }

                e.Handled = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());

                ShowErrorMessage($"Exception: \"{ex.Message}\".");
            }
            finally
            {
            }
        }

        private async void ReadFileCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var diag = new OpenFileDialog
                {
                    InitialDirectory = AppDomain.CurrentDomain.BaseDirectory,
                    Multiselect = false,
                    CheckPathExists = true
                };

                var result = diag.ShowDialog();

                if (result.HasValue == false ||
                    result.Value == false)
                {
                    return;
                }

                m_ViewModel.ShowProgressBar = true;

                await m_ViewModel.ReadWordListTxtFileAsync(diag.FileName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());

                ShowErrorMessage($"Exception reading file: \"{ex.Message}\".");
            }
            finally
            {
                m_ViewModel.ShowProgressBar = false;
            }
        }

        private void ReadDirectoryCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                if (m_ViewModel is null)
                {
                    e.CanExecute = false;
                }
                else
                {
                    e.CanExecute = m_ViewModel.IsBusy == false;
                }

                e.Handled = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());

                ShowErrorMessage($"Exception: \"{ex.Message}\".");
            }
            finally
            {
            }
        }

        private async void ReadDirectoryCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                string directoryPath = null;

                string startPath = AppDomain.CurrentDomain.BaseDirectory;

                using (System.Windows.Forms.FolderBrowserDialog diag = new())
                {
                    diag.SelectedPath = startPath;
                    diag.Description = "Select the directory of word list(s) to append to the current list";

                    if (diag.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        directoryPath = diag.SelectedPath;
                    }

                }

                if (string.IsNullOrWhiteSpace(directoryPath))
                {
                    return;
                }

                var dataDirDI = new DirectoryInfo(directoryPath);

                var wordListFiles = dataDirDI.GetFiles("*.txt");

                m_ViewModel.ShowProgressBar = true;

                Stopwatch timer = Stopwatch.StartNew();

                foreach (FileInfo wordListFile in wordListFiles)
                {
                    m_ViewModel.SetStatusText($"Reading in \"{wordListFile.Name}\".", false);

                    await m_ViewModel.ReadWordListTxtFileAsync(wordListFile.FullName);
                }

                timer.Stop();

                m_ViewModel.SetStatusText($"Parsed {wordListFiles.Length:###,###,##0} " +
                    $"files in {timer.Elapsed}, with {m_ViewModel.WordList.Count:###,###,##0} entries");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());

                ShowErrorMessage($"Exception reading files: \"{ex.Message}\".");
            }
            finally
            {
                m_ViewModel.ShowProgressBar = false;
            }
        }

        private void GenerateListCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                if (m_ViewModel is null)
                {
                    e.CanExecute = false;
                }
                else
                {
                    e.CanExecute = m_ViewModel.IsBusy == false && 
                                   m_ViewModel.WordList.Count > 0;
                }

                e.Handled = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());

                ShowErrorMessage($"Exception: \"{ex.Message}\".");
            }
            finally
            {
            }
        }

        private async void GenerateListCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                DateTime now = DateTime.Now;

                var diag = new SaveFileDialog
                {
                    Title = "SaveAsync file as...",
                    AddExtension = true,
                    DefaultExt = "xml",
                    FileName = MainWindowViewModel.WORD_LIST_FILE_NAME
                };

                var result = diag.ShowDialog();

                if (result.HasValue == false ||
                    result.Value == false)
                {
                    return;
                }

                await m_ViewModel.SaveAsync(diag.FileName);

                m_ViewModel.SetStatusText($"Saved file to \"{diag.FileName}\".");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());

                ShowErrorMessage($"Exception saving the word list: \"{ex.Message}\".");
            }
            finally
            {
                m_ViewModel.ShowProgressBar = false;
            }
        }

        #endregion Private Methods

    }
}
