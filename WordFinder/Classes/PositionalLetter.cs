using System;

namespace WordFinder.Classes
{
    public class PositionalLetter :
        NotifyObject,
        IEquatable<PositionalLetter>,
        IComparable<PositionalLetter>
    {
        #region Private Members

        private string m_Text;

        private readonly int m_Position = -1;

        #endregion Private Members

        #region Public Properties

        public string Text
        {
            get => m_Text;

            set
            {
                if (string.IsNullOrWhiteSpace(m_Text) ||
                    m_Text.Equals(value, StringComparison.OrdinalIgnoreCase) == false)
                {
                    m_Text = value.ToUpperInvariant();

                    if (m_Text.Length > 1)
                    {
                        m_Text = m_Text.Substring(0, 1);
                    }

                    RaisePropertyChanged(nameof(Text));
                }

            }
        }

        /// <summary>
        /// Gets the zero-based position of the letter.
        /// </summary>
        public int Position
        {
            get => m_Position;
        }

        #endregion Public Properties

        #region constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position">The zero-based position of the letter.</param>
        public PositionalLetter(int position)
        {
            m_Position = position;
        }

        public PositionalLetter(int position, string text)
        {
            m_Position = position;
            Text = text;
        }

        #endregion constructors

        #region Public Methods

        /// <summary>
        ///
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        /// <remarks>Implements IComparable.</remarks>
        public int CompareTo(PositionalLetter other)
        {
            return Text.CompareTo(other.Text);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            PositionalLetter letter = obj as PositionalLetter;

            return letter is not null && Equals(letter);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        /// <remarks>Implements IEquatable.</remarks>
        public bool Equals(PositionalLetter other)
        {
            return GetHashCode().Equals(other.GetHashCode());
        }

        public override int GetHashCode()
        {
            // Assumes that the text was set via the property, which converts it
            //   to upper invariant.

            return Text.GetHashCode();
        }

        public override string ToString()
        {
            return Text;
        }

        #endregion Public Methods

    }
}
