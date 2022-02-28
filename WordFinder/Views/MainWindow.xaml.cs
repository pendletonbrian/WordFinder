using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Microsoft.Win32;
using WordFinder.ViewModel;

namespace WordFinder.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private Members

        private static readonly string BASE_TITLE_TEXT = " Word Finder";

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
            _ = MessageBox.Show(this, msg, BASE_TITLE_TEXT, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #endregion Public Methods

        #region Private Methods

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //var dataDir = AppDomain.CurrentDomain.BaseDirectory;

                //dataDir = System.IO.Path.Combine(dataDir, "data");

                //if (Directory.Exists(dataDir) == false)
                //{
                //    throw new DirectoryNotFoundException($"Data directory at \"{dataDir}\" does not exist.");
                //}

                //var dataDirDI = new DirectoryInfo(dataDir);

                //var wordListFiles = dataDirDI.GetFiles("*.txt");

                //m_ViewModel.ShowProgressBar = true;

                //foreach (FileInfo wordListFile in wordListFiles)
                //{
                //    m_ViewModel.StatusLabelText = $"Reading in \"{wordListFile.Name}\".";

                //    m_ViewModel.ReadFile(wordListFile.FullName);
                //}
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

        private void ReadFileCommand_Executed(object sender, ExecutedRoutedEventArgs e)
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

                FileInfo fi = new(diag.FileName);

                m_ViewModel.StatusLabelText = $"Reading in \"{fi.Name}\".";

                m_ViewModel.ReadFile(diag.FileName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());

                ShowErrorMessage($"Exception: \"{ex.Message}\".");
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

        private void ReadDirectoryCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                string directoryPath = null;

                string startPath = AppDomain.CurrentDomain.BaseDirectory;

                using (System.Windows.Forms.FolderBrowserDialog diag = new())
                {
                    diag.SelectedPath = startPath;
                    diag.Description = "Select the word list to append to the current dictionary";

                    if (diag.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        directoryPath = diag.SelectedPath;
                    }

                    if (string.IsNullOrWhiteSpace(directoryPath))
                    {
                        return;
                    }

                    var dataDirDI = new DirectoryInfo(directoryPath);

                    var wordListFiles = dataDirDI.GetFiles("*.txt");

                    m_ViewModel.ShowProgressBar = true;

                    foreach (FileInfo wordListFile in wordListFiles)
                    {
                        m_ViewModel.StatusLabelText = $"Reading in \"{wordListFile.Name}\".";

                        m_ViewModel.ReadFile(wordListFile.FullName);
                    }

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());

                ShowErrorMessage($"Exception: \"{ex.Message}\".");
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

        private void GenerateListCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                DateTime now = DateTime.Now;

                var diag = new SaveFileDialog
                {
                    Title = "Save file as...",
                    AddExtension = true,
                    DefaultExt = "xml",
                    FileName = $"{DateTime.Now:yyyy_MMM_dd}_word_finder_dictionary"
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());

                ShowErrorMessage($"Exception: \"{ex.Message}\".");
            }
            finally
            {
                m_ViewModel.ShowProgressBar = false;
            }
        }

        #endregion Private Methods
    }
}
