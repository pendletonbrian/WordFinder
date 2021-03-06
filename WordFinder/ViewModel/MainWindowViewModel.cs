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

        public static RoutedCommand AddWordCommand = new();
        public static RoutedCommand CopyCommand = new();
        public static RoutedCommand GenerateListCommand = new();
        public static RoutedCommand ReadDirectoryCommand = new();
        public static RoutedCommand ReadFileCommand = new();
        public static RoutedCommand RemoveWordCommand = new();
        public static RoutedCommand SearchCommand = new();
        internal static readonly string BASE_TITLE_TEXT = " Word Finder";
        internal static readonly string WORD_LIST_FILE_NAME = "word_finder_word_list.xml";

        #endregion Public Members

        #region Private Members

        /// <summary>
        /// Maximum number of seconds to show a message.
        /// </summary>
        private const int TIMER_NUMBER_OF_SECONDS = 8;

        /// <summary>
        /// Used in the GUI to select included/excluded letters.
        /// </summary>
        private readonly List<KeyValuePair<string, string>> m_SelectedLettersKeyValueList = new(26);

        /// <summary>
        /// Timer declaration and its interval.
        /// </summary>
        private readonly Timer m_StatusLabelTimer = new(1000.0d);

        private ExactLetterCollection m_ExactIncludedLetters = new();

        /// <summary>
        /// Used in the filtering method.
        /// </summary>
        private char[] m_ExcludedChars = Array.Empty<char>();

        /// <summary>
        /// The letters that the user selectes to exclude words from the search.
        /// </summary>
        private List<Enumerations.Letters> m_ExcludedLetters = new(26);

        /// <summary>
        /// Used in the filtering method.
        /// </summary>
        private char[] m_IncludedChars = Array.Empty<char>();

        /// <summary>
        /// The letters that the user selects to include words in the search.
        /// </summary>
        private List<Enumerations.Letters> m_IncludedLetters = new(26);

        /// <summary>
        /// Some of the actions (generating the list) can take a long time.
        /// This is used by the GUI to enable/disable commands if the long-running
        /// actions are not done yet.
        /// </summary>
        private bool m_IsBusy;

        /// <summary>
        /// The maximum possible length of a word, due to the dictionary's
        /// constraints.
        /// </summary>
        private int m_MaxWordLength;

        /// <summary>
        /// Used in the filtering method to find words with letters in an exact position.
        /// </summary>
        private Regex m_RegEx;

        private List<string> m_SelectedWords = new();

        /// <summary>
        /// Whether or not to show and start the progress bar.
        /// </summary>
        private bool m_ShowProgressBar;

        /// <summary>
        /// The current number of seconds that a message has been shown. Gets
        /// reset every time a new message is shown.
        /// </summary>
        private int m_StatusLabelCount;

        /// <summary>
        /// The text to display in the status bar.
        /// </summary>
        private string m_StatusLabelText = string.Empty;

        /// <summary>
        /// The length of the target word.
        /// </summary>
        private int m_TargetWordLength;

        /// <summary>
        /// ALL the words, yo!
        /// </summary>
        private WordList m_WordList = new();

        #endregion Private Members

        #region Public Properties

        public ExactLetterCollection ExactIncludedPositionLettersList
        {
            get => m_ExactIncludedLetters;

            set
            {
                m_ExactIncludedLetters = value;

                RaisePropertyChanged(nameof(ExactIncludedPositionLettersList));
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
        /// The list of word lengths from which the user can select thier target
        /// word's length.
        /// </summary>
        public ObservableCollection<int> PossibleNumberOfChars { get; } = new();

        public List<KeyValuePair<string, string>> SelectedLettersKeyValueList => m_SelectedLettersKeyValueList;

        public List<string> SelectedWords
        {
            get => m_SelectedWords;
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

                    int diff = TargetWordLength - ExactIncludedPositionLettersList.Count;

                    if (diff == 0)
                    {
                        return;
                    }

                    if (diff > 0)
                    {
                        for (int i = 0; i < diff; ++i)
                        {
                            ExactIncludedPositionLettersList.Add(new PositionalLetter(i));
                        }
                    }
                    else if (diff < 0)
                    {
                        for (int i = diff - 1; i > TargetWordLength; ++i)
                        {
                            ExactIncludedPositionLettersList.RemoveAt(i);
                        }
                    }

                    RaisePropertyChanged(nameof(ExactIncludedPositionLettersList));

                }
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
        /// Like, the list of all the words, man.
        /// </summary>
        public WordList WordList => m_WordList;

        #endregion Public Properties

        #region constructor

        public MainWindowViewModel()
        {
            m_StatusLabelTimer.Elapsed += StatusLabelTimer_Elapsed;

            m_SelectedLettersKeyValueList = Enumerations.GetEnumValueDescriptionPairs(typeof(Enumerations.Letters));
        }

        #endregion constructor

        #region Private Methods

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

        public void Dispose()
        {
            if (m_StatusLabelTimer != null)
            {
                m_StatusLabelTimer.Enabled = false;

                m_StatusLabelTimer.Dispose();
            }
        }

        internal void AddExcludedLetter(Enumerations.Letters letter)
        {
            if (m_ExcludedLetters.Contains(letter))
            {
                return;
            }

            m_ExcludedLetters.Add(letter);
        }

        internal void AddIncludedLetter(Enumerations.Letters letter)
        {
            if (m_IncludedLetters.Contains(letter))
            {
                return;
            }

            m_IncludedLetters.Add(letter);
        }

        internal void AddSelectedWord(string wordToAdd)
        {
            if (string.IsNullOrWhiteSpace(wordToAdd))
            {
                Debug.WriteLine("The word to add is null/empty.");

                return;
            }

            if (SelectedWords.Contains(wordToAdd, StringComparer.OrdinalIgnoreCase))
            {
                Debug.WriteLine($"The word to add \"{wordToAdd}\" already exists in the list.");

                return;
            }

            SelectedWords.Add(wordToAdd);

            SelectedWords.Sort(StringComparer.OrdinalIgnoreCase);

            RaisePropertyChanged(nameof(SelectedWords));
        }

        internal bool HasSelectedItems()
        {
            return SelectedWords.Count > 0;
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

            if (ExactIncludedPositionLettersList.Count == 0)
            {
                m_RegEx = null;
            }
            else
            {
                foreach (var letter in ExactIncludedPositionLettersList)
                {
                    if (string.IsNullOrWhiteSpace(letter.Text))
                    {
                        regex += "[a-zA-Z]";
                    }
                    else
                    {
                        char c = letter.Text.ToLower()[0];

                        if (c.Equals('*'))
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

                    } // Text is not null

                }// foreach letter in ExactPositionLettersList

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

                /*
                 * Search for 3 of the same letter consecutively.
                 * 
                 * NOTE: This only works for lower case.
                */
                Regex consecutiveLettersRegex = new Regex(@"([a-z])\1\1");

                await Task.Run(() =>
                {
                    using (StreamReader rdr = new(fullyQualifiedFilepath))
                    {
                        while ((line = rdr.ReadLine()) != null)
                        {
                            // Skip numbers, spaces, apostrophes, etc.
                            if (line.All(char.IsLetter) == false)
                            {
                                ++skippedWords;

                                continue;
                            }

                            line = line.ToLowerInvariant();

                            if (consecutiveLettersRegex.IsMatch(line))
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

        internal void RemoveExcludedLetter(Enumerations.Letters letter)
        {
            if (m_ExcludedLetters.Contains(letter) == false)
            {
                return;
            }

            m_ExcludedLetters.Remove(letter);
        }

        internal void RemoveIncludedLetter(Enumerations.Letters letter)
        {
            if (m_IncludedLetters.Contains(letter) == false)
            {
                return;
            }

            m_IncludedLetters.Remove(letter);
        }

        internal void RemoveSelectedWord(string wordToRemove)
        {
            if (string.IsNullOrWhiteSpace(wordToRemove))
            {
                Debug.WriteLine("The word to remove is null/empty.");

                return;
            }

            if (SelectedWords.Remove(wordToRemove) == false)
            {
                Debug.WriteLine($"The word to remove \"{wordToRemove}\" doesn't exist in the list.");

                return;
            }

            SelectedWords.Sort(StringComparer.OrdinalIgnoreCase);

            RaisePropertyChanged(nameof(SelectedWords));
        }

        internal async Task SaveAsync(string fullyQualifiedFilepath)
        {
            await WordList.SaveAsync(fullyQualifiedFilepath);
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
