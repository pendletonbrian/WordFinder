using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using WordFinder.Classes;

namespace WordFinder.ViewModel
{
    public class MainWindowViewModel : NotifyObject
    {
        #region Public Members

        internal static readonly string BASE_TITLE_TEXT = " Word Finder";
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

        private readonly Timer m_StatusLabelTimer = new Timer(1000.0d);
        private const int TIMER_NUMBER_OF_SECONDS = 8;
        private int m_StatusLabelCount;

        #endregion Private Members

        #region Public Properties

        public string TitleText
        {
            get => BASE_TITLE_TEXT;
        }

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

        #endregion Public Methods

    }
}
