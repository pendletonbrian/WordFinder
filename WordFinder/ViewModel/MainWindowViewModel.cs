using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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

        /// <summary>
        /// The length of the target word.
        /// </summary>
        private int m_TargetWordLength;

        /// <summary>
        /// The maximum possible length of a word, due to the dictionary's
        /// constraints.
        /// </summary>
        private int m_MaxWordLength;

        /// <summary>
        /// ALL the words, yo!
        /// </summary>
        private WordList m_WordList = new();

        /// <summary>
        /// Whether or not to show and start the progress bar.
        /// </summary>
        private bool m_ShowProgressBar;

        /// <summary>
        /// The text to display in the status bar.
        /// </summary>
        private string m_StatusLabelText = string.Empty;

        /// <summary>
        /// Some of the actions (generating the list) can take a long time.
        /// This is used by the GUI to enable/disable commands if the long-running
        /// actions are not done yet.
        /// </summary>
        private bool m_IsBusy;

        /// <summary>
        /// Timer declaration and its interval.
        /// </summary>
        private readonly Timer m_StatusLabelTimer = new(1000.0d);

        /// <summary>
        /// Maximum number of seconds to show a message.
        /// </summary>
        private const int TIMER_NUMBER_OF_SECONDS = 8;

        /// <summary>
        /// The current number of seconds that a message has been shown. Gets
        /// reset every time a new message is shown.
        /// </summary>
        private int m_StatusLabelCount;

        /// <summary>
        /// Used in the filtering method.
        /// </summary>
        private char[] m_IncludedChars = Array.Empty<char>();

        /// <summary>
        /// Used in the filtering method.
        /// </summary>
        private char[] m_ExcludedChars = Array.Empty<char>();

        /// <summary>
        /// Used in the filtering method to find words with letters in an exact position.
        /// </summary>
        private Regex m_RegEx;

        /// <summary>
        /// Used in the GUI to select included/excluded letters.
        /// </summary>
        private readonly List<KeyValuePair<string, string>> m_SelectedLettersKeyValueList = new(26);

        /// <summary>
        /// The letters that the user selects to include words in the search.
        /// </summary>
        private List<Enumerations.Letters> m_IncludedLetters = new(26);

        /// <summary>
        /// The letters that the user selectes to exclude words from the search.
        /// </summary>
        private List<Enumerations.Letters> m_ExcludedLetters = new(26);

        private ObservableCollection<string> m_ExactLetters = new ObservableCollection<string>();

        #endregion Private Members

        #region Public Properties

        public List<KeyValuePair<string, string>> SelectedLettersKeyValueList => m_SelectedLettersKeyValueList;

        public ObservableCollection<string> ExactPositionLettersList 
        {
            get => m_ExactLetters;


            set
            {
                m_ExactLetters = value;

                RaisePropertyChanged(nameof(ExactPositionLettersList));
            }
        }

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

                    int diff = TargetWordLength - ExactPositionLettersList.Count;

                    if (diff == 0)
                    {
                        return;
                    }

                    if (diff > 0)
                    {
                        char A = 'A';
                        char c;
                        for (int i = 0; i < diff; ++i)
                        {
                            c = (char)(A + i);

                            ExactPositionLettersList.Add($"{c}");
                        }
                    }
                    else if (diff < 0)
                    {
                        for (int i = diff - 1; i > TargetWordLength; ++i)
                        {
                            ExactPositionLettersList.RemoveAt(i);
                        }
                    }

                    RaisePropertyChanged(nameof(ExactPositionLettersList));
                }
            }

        }

        /// <summary>
        /// The character count of the longest word in the list.
        /// </summary>
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

        /// <summary>
        /// Whether or not to show (and start) the status bar.
        /// </summary>
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

        /// <summary>
        /// The text to display in the status label.
        /// </summary>
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

        /// <summary>
        /// The list of word lengths from which the user can select thier target
        /// word's length.
        /// </summary>
        public ObservableCollection<int> PossibleNumberOfChars { get; } = new ();

        #endregion Public Properties

        #region constructor

        public MainWindowViewModel()
        {
            m_StatusLabelTimer.Elapsed += StatusLabelTimer_Elapsed;

            m_SelectedLettersKeyValueList = Enumerations.GetEnumValueDescriptionPairs(typeof(Enumerations.Letters));
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

            if (m_ExcludedChars.Length > 0 &&
                word.IndexOfAny(m_ExcludedChars) >= 0)
            {
                return false;
            }

            if (m_IncludedChars.Length > 0 &&
                m_IncludedChars.All(word.Contains) == false)
            {
                return false;
            }

            if (m_RegEx is null)
            {
                return true;
            }

            return m_RegEx.IsMatch(word);
        }

        #endregion Private Methods

        #region Public Methods

        internal async Task ReadWordListTxtFileAsync(string fullyQualifiedFilepath)
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

        internal async Task SaveAsync(string fullyQualifiedFilepath)
        {
            await WordList.SaveAsync(fullyQualifiedFilepath);
        }

        internal async Task LoadAsync(string fullyQualifiedFilepath)
        {
            await WordList.LoadAsync(fullyQualifiedFilepath);

            MaxWordLength = WordList.MaxWordLength;

            PossibleNumberOfChars.Clear();

            for (int i = 3; i <= MaxWordLength; ++i)
            {
                PossibleNumberOfChars.Add(i);
            }

            RaisePropertyChanged(nameof(PossibleNumberOfChars));

            TargetWordLength = 5;
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
            int index = 0;

            if (m_IncludedLetters.Count > 0)
            {
                m_IncludedChars = new char[m_IncludedLetters.Count];

                foreach (var enumValue in m_IncludedLetters)
                {
                    var description = enumValue.GetDescription();

                    if (description.Length == 1)
                    {
                        m_IncludedChars[index++] = description.ToLower()[0];
                    }
                }

            }
            else
            {
                m_IncludedChars = Array.Empty<char>();
            }

            if (m_ExcludedLetters.Count > 0)
            {
                m_ExcludedChars = new char[m_ExcludedLetters.Count];

                index = 0;

                foreach (var enumValue in m_ExcludedLetters)
                {
                    var description = enumValue.GetDescription();

                    if (description.Length == 1)
                    {
                        m_ExcludedChars[index++] = description.ToLower()[0];
                    }
                }

            }
            else
            {
                m_ExcludedChars = Array.Empty<char>();
            }

            bool errorFound = false;
            bool characterFound = false;
            string regex = string.Empty;

            if (ExactPositionLettersList.Count == 0)
            {
                m_RegEx = null;
            }
            else
            {
                foreach (string letter in ExactPositionLettersList)
                {
                    if (string.IsNullOrWhiteSpace(letter))
                    {
                        continue;
                    }

                    char c = letter.ToLower()[0];

                    if (c.Equals('*') ||
                        char.IsWhiteSpace(c))
                    {
                        // Look for any letter.

                        regex += "[a-zA-Z]";
                    }
                    else
                    {
                        if (char.IsLetter(c))
                        {
                            // Look for that specific letter.
                            regex += $"[{c}]";

                            characterFound = true;
                        }
                        else
                        {
                            SetStatusText($"The character \"{c}\" is not a letter.");

                            errorFound = true;

                            break;
                        }

                    }
                }

                m_RegEx = new Regex(regex, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }

            if (errorFound)
            {
                regex = null;

                return;
            }

            if (characterFound == false)
            {
                regex = null;
            }

            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(m_WordList);

            view.Filter = Search;

            SetStatusText($"There are { view.Count:###,###,##0} results.");
        }

        internal void AddExcludedLetter(Enumerations.Letters letter)
        {
            if (m_ExcludedLetters.Contains(letter))
            {
                return;
            }

            m_ExcludedLetters.Add(letter);
        }

        internal void RemoveExcludedLetter(Enumerations.Letters letter)
        {
            if (m_ExcludedLetters.Contains(letter) == false)
            {
                return;
            }

            m_ExcludedLetters.Remove(letter);
        }

        internal void AddIncludedLetter(Enumerations.Letters letter)
        {
            if (m_IncludedLetters.Contains(letter))
            {
                return;
            }

            m_IncludedLetters.Add(letter);
        }

        internal void RemoveIncludedLetter(Enumerations.Letters letter)
        {
            if (m_IncludedLetters.Contains(letter) == false)
            {
                return;
            }

            m_IncludedLetters.Remove(letter);
        }

        #endregion Public Methods

    }
}
