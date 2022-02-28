using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml;
using WordFinder.Classes;

namespace WordFinder.ViewModel
{
    public class MainWindowViewModel : NotifyObject
    {
        #region Public Members

        internal static readonly string WORD_LIST_FILE_NAME = "word_finder_word_list.xml";

        public static RoutedCommand ReadFileCommand = new RoutedCommand();
        public static RoutedCommand ReadDirectoryCommand = new RoutedCommand();
        public static RoutedCommand GenerateListCommand = new RoutedCommand();

        #endregion Public Members

        #region Private Members

        private int m_WordLength = 5;

        private WordList m_WordList = new();

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

        public WordList WordList
        {
            get => m_WordList;
        }

        #endregion Public Properties

        #region constructor

        public MainWindowViewModel()
        { }

        #endregion constructor

        #region Public Methods

        internal async Task ReadWordListTxtFile(string fullyQualifiedFilepath)
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
                int skippedWords = 0;
                string line;

                await Task.Run(() =>
                {
                    using (StreamReader rdr = new(fullyQualifiedFilepath))
                    {
                        while ((line = rdr.ReadLine()) != null)
                        {
                            if (WordList.Contains(line, StringComparer.OrdinalIgnoreCase))
                            {
                                ++skippedWords;

                                continue;
                            }

                            ++lineCount;

                            WordList.Add(line);
                        }
                    }

                    WordList.Sort();
                });

                timer.Stop();

                var fileInfo = new FileInfo(fullyQualifiedFilepath);

                StatusLabelText = $"Added {lineCount:###,###,##0} words and skipped {skippedWords:###,###,##0} words from " +
                    $"\"{fileInfo.Name}\" in {timer.Elapsed} for a total of {WordList.Count:###,###,##0}.";

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

        internal async Task Save(string fullyQualifiedFilepath)
        {
            await WordList.Save(fullyQualifiedFilepath);
        }

        #endregion Public Methods

    }
}
