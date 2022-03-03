using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Data;
using System.Windows.Input;
using WordFinder.Classes;

namespace WordFinder.ViewModel
{
    public class MainWindowViewModel : NotifyObject, IDisposable
    {
        #region Public Members

        internal static readonly string BASE_TITLE_TEXT = " Word Finder";
        internal static readonly string WORD_LIST_FILE_NAME = "word_finder_word_list.xml";

        public static RoutedCommand ReadFileCommand = new RoutedCommand();
        public static RoutedCommand ReadDirectoryCommand = new RoutedCommand();
        public static RoutedCommand GenerateListCommand = new RoutedCommand();
        public static RoutedCommand SearchCommand = new RoutedCommand();

        #endregion Public Members

        #region Private Members

        private int m_TargetWordLength = 5;

        private int m_MaxWordLength = 0;

        private WordList m_WordList = new();

        private bool m_ShowProgressBar;

        private string m_StatusLabelText = string.Empty;

        private bool m_IsBusy;

        private readonly Timer m_StatusLabelTimer = new Timer(1000.0d);
        private const int TIMER_NUMBER_OF_SECONDS = 8;
        private int m_StatusLabelCount;

        private char[] m_IncludedChars;
        private char[] m_ExcludedChars;

        #endregion Private Members

        #region Public Properties

        /// <summary>
        /// Text in the, uh.. Title bar.
        /// </summary>
        public string TitleText
        {
            get => BASE_TITLE_TEXT;
        }

        /// <summary>
        /// Number of characters in the word, yo.
        /// </summary>
        public int TargetWordLength
        {
            get => m_TargetWordLength;

            set
            {
                if (m_TargetWordLength != value)
                {
                    m_TargetWordLength = value;

                    RaisePropertyChanged(nameof(TargetWordLength));
                }
            }

        }

        public int MaxWordLength
        {
            get => m_MaxWordLength;

            private set
            {
                if (m_MaxWordLength != value)
                {
                    m_MaxWordLength = value;

                    RaisePropertyChanged(nameof(MaxWordLength));
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

        /// <summary>
        /// Is the view model processing anything?
        /// </summary>
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

        /// <summary>
        /// Like, the list of all the words, man.
        /// </summary>
        public WordList WordList => m_WordList;

        public ObservableCollection<int> PossibleNumberOfChars { get; } = new ();

        /// <summary>
        /// Letters that are in the word, but not in any particular place.
        /// </summary>
        public string IncludedLetters { get; set; }

        /// <summary>
        /// Letters that are in the word, and in a particular place.
        /// </summary>
        public List<string> CorrectLetters { get; set; } = new(26);

        /// <summary>
        /// Letters that are not in the word at all.
        /// </summary>
        public string ExcludedLetters { get; set; }

        #endregion Public Properties

        #region constructor

        public MainWindowViewModel()
        {
            m_StatusLabelTimer.Elapsed += StatusLabelTimer_Elapsed;
        }

        #endregion constructor

        #region Private Methods

        private void StatusLabelTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (++m_StatusLabelCount >= TIMER_NUMBER_OF_SECONDS)
            {
                m_StatusLabelTimer.Enabled = false;

                StatusLabelText = string.Empty;
            }
        }

        private bool Search(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            string word = obj.ToString();

            if (word.Length != TargetWordLength)
            {
                return false;
            }

            if (word.IndexOfAny(m_ExcludedChars) >= 0)
            {
                return false;
            }

            if (m_IncludedChars.All(word.Contains) == false)
            {
                return false;
            }

            if (word.Contains("asty") == false)
            {
                return false;
            }

            return true;
        }

        #endregion Private Methods

        #region Public Methods

        internal async Task ReadWordListTxtFile(string fullyQualifiedFilepath)
        {
            try
            {
                if (File.Exists(fullyQualifiedFilepath) == false)
                {
                    throw new FileNotFoundException($"The word list file \"{fullyQualifiedFilepath}\" does not exist.");
                }

                FileInfo fi = new FileInfo(fullyQualifiedFilepath);

                SetStatusText($"Reading in \"{fi.Name}\".", false);

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
                            if (line.All(char.IsLetter) == false)
                            {
                                ++skippedWords;

                                continue;
                            }

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

                SetStatusText($"Added {lineCount:###,###,##0} words and skipped {skippedWords:###,###,##0} words from " +
                    $"\"{fileInfo.Name}\" in {timer.Elapsed} for a total of {WordList.Count:###,###,##0}.");

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

        internal async Task Load(string fullyQualifiedFilepath)
        {
            await WordList.Load(fullyQualifiedFilepath);

            MaxWordLength = WordList.MaxWordLength;

            PossibleNumberOfChars.Clear();

            for (int i = 3; i <= MaxWordLength; ++i)
            {
                PossibleNumberOfChars.Add(i);
            }

            RaisePropertyChanged(nameof(PossibleNumberOfChars));

            PerformWordSearch();
        }

        internal void SetStatusText(string text, bool autoRemove = true)
        {
            m_StatusLabelTimer.Enabled = false;

            StatusLabelText = text;

            if (autoRemove)
            {
                m_StatusLabelCount = 0;

                m_StatusLabelTimer.Enabled = true;
            }
        }

        public void Dispose()
        {
            if (m_StatusLabelTimer != null)
            {
                m_StatusLabelTimer.Enabled = false;

                m_StatusLabelTimer.Dispose();
            }
        }

        internal void PerformWordSearch()
        {
            char[] seperators = { ',', ' ' };

            string[] resultList;
            int index = 0;

            if (string.IsNullOrWhiteSpace(IncludedLetters) == false)
            {
                resultList = IncludedLetters.Split(seperators,
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                m_IncludedChars = new char[resultList.Length];

                foreach (var result in resultList)
                {
                    if (result.Length == 1)
                    {
                        m_IncludedChars[index++] = result[0];
                    }
                }

            }
            else
            {
                m_IncludedChars = Array.Empty<char>();
            }

            if (string.IsNullOrWhiteSpace(ExcludedLetters) == false)
            {
                resultList = ExcludedLetters.Split(seperators,
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                m_ExcludedChars = new char[resultList.Length];

                index = 0;

                foreach (var result in resultList)
                {
                    if (result.Length == 1)
                    {
                        m_ExcludedChars[index++] = result[0];
                    }
                }

            }
            else
            {
                m_ExcludedChars = Array.Empty<char>();
            }

            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(m_WordList);

            view.Filter = Search;

            SetStatusText($"There are { view.Count:###,###,##0} results.");
        }

        #endregion Public Methods

    }
}
