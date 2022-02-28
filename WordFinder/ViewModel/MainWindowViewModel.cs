using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using WordFinder.Classes;

namespace WordFinder.ViewModel
{
    public class MainWindowViewModel : NotifyObject
    {
        #region Public Members

        public static RoutedCommand ReadFileCommand = new RoutedCommand();
        public static RoutedCommand ReadDirectoryCommand = new RoutedCommand();
        public static RoutedCommand GenerateListCommand = new RoutedCommand();

        #endregion Public Members

        #region Private Members

        private int m_WordLength = 5;

        private List<string> m_WordList = new();

        private bool m_ShowProgressBar;

        private string m_StatusLabelText = string.Empty;

        private bool m_IsBusy;

        #endregion Private Members

        #region Public Properties

        public int WordLength
        {
            get => m_WordLength;

            set
            {
                if (m_WordLength != value)
                {
                    m_WordLength = value;

                    RaisePropertyChanged(nameof(WordLength));
                }
            }

        }

        public bool ShowProgressBar
        {
            get => m_ShowProgressBar;

            set
            {
                if (m_ShowProgressBar != value)
                {
                    m_ShowProgressBar = value;

                    RaisePropertyChanged(nameof(ShowProgressBar));
                }

            }
        }

        public string StatusLabelText
        {
            get { return m_StatusLabelText; }

            set
            {
                if (string.IsNullOrWhiteSpace(m_StatusLabelText) ||
                    m_StatusLabelText.Equals(value, StringComparison.Ordinal) == false)
                {
                    m_StatusLabelText = value;

                    RaisePropertyChanged(nameof(StatusLabelText));
                }
            }
        }

        public bool IsBusy
        {
            get => m_IsBusy;

            private set
            {
                if (m_IsBusy != value)
                {
                    m_IsBusy = value;

                    RaisePropertyChanged(nameof(IsBusy));
                }
            }
        }

        public List<string> WordList
        {
            get => m_WordList;
        }

        #endregion Public Properties

        #region constructor

        public MainWindowViewModel()
        { }

        #endregion constructor

        #region Public Methods

        internal void ReadFile(string fullyQualifiedFilepath)
        {
            try
            {
                if (File.Exists(fullyQualifiedFilepath) == false)
                {
                    throw new FileNotFoundException($"The word list file \"{fullyQualifiedFilepath}\" does not exist.");
                }

                IsBusy = true;

                Stopwatch timer = Stopwatch.StartNew();

                int lineCount = 0;
                string line;

                using (StreamReader rdr = new(fullyQualifiedFilepath))
                {
                    while ((line = rdr.ReadLine()) != null)
                    {
                        if (m_WordList.Contains(line, StringComparer.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        ++lineCount;

                        m_WordList.Add(line);
                    }
                }

                timer.Stop();

                var fileInfo = new FileInfo(fullyQualifiedFilepath);

                StatusLabelText = $"Added {lineCount:###,###,##0} words from \"{fileInfo.Name}\" in {timer.Elapsed}.";

                RaisePropertyChanged(nameof(WordList));
            }
            catch
            {
                throw;
            }
            finally
            {
                IsBusy = false;
            }
        }

        #endregion Public Methods

    }
}
