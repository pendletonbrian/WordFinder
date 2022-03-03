using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace WordFinder.Classes
{
    /// <summary>
    /// Most of the word lists came from https://stackoverflow.com/questions/4456446/dictionary-text-file
    /// I also used WordList and Project Gutenberg.
    /// </summary>
    public class WordList : List<string>
    {
        #region Public Properties

        public DateTimeOffset LastUpdatedDate { get; set; }

        public int MaxWordLength { get; set; }

        #endregion Public Properties

        internal async Task Save(string fullyQualifiedFilepath)
        {
            if (string.IsNullOrWhiteSpace(fullyQualifiedFilepath))
            {
                throw new ArgumentException("Filepath can't be null/empty.", nameof(fullyQualifiedFilepath));
            }

            Sort(StringComparer.OrdinalIgnoreCase);

            var doc = new XmlDocument();
            var rootNode = doc.CreateElement(XmlDefinitions.Dictionary.E_Root);

            XmlHelper.AddAttr(doc, ref rootNode,
                XmlDefinitions.Dictionary.A_Count,
                Count.ToString());

            XmlHelper.AddAttr(doc, ref rootNode,
                XmlDefinitions.Dictionary.A_LastUpdatedDate,
                DateTimeOffset.Now.ToString());

            XmlElement wordNode;

            await Task.Run(() =>
            {
                foreach (string word in this)
                {
                    if (string.IsNullOrWhiteSpace(word))
                    {
                        continue;
                    }

                    wordNode = doc.CreateElement(XmlDefinitions.Word.E_Word);

                    XmlHelper.AddAttr(doc, ref wordNode,
                                      XmlDefinitions.Word.A_Text,
                                      word.Trim().ToLower());

                    _ = rootNode.AppendChild(wordNode);
                }

                _ = doc.AppendChild(rootNode);

                doc.Save(fullyQualifiedFilepath);
            });
        }

        internal async Task Load(string fullyQualifiedFilepath)
        {
            if (string.IsNullOrWhiteSpace(fullyQualifiedFilepath))
            {
                throw new ArgumentException("Filepath can't be null/empty.", nameof(fullyQualifiedFilepath));
            }

            try
            {
                Clear();

                int maxLength = 0;

                var doc = new XmlDocument();

                doc.Load(fullyQualifiedFilepath);

                var rootNode = doc.ChildNodes[0];

                XmlAttribute attr = rootNode.Attributes[XmlDefinitions.Dictionary.A_Count];

                // Does the attribute exist?
                if (attr is null)
                {
                    throw new XmlException($"The collection attribute " +
                        $"\"{XmlDefinitions.Dictionary.A_Count}\" DNE.");
                }

                // Is it correctly formatted?
                if (int.TryParse(attr.Value, out int documentCount) == false)
                {
                    throw new FormatException($"The attribute value \"{attr.Value}\" did not parse into an integer.");
                }

                // Get the last updated attribute.
                attr = rootNode.Attributes[XmlDefinitions.Dictionary.A_LastUpdatedDate];

                // Does it exist?
                if (attr is null)
                {
                    throw new XmlException($"The collection attribute " +
                        $"\"{XmlDefinitions.Dictionary.A_LastUpdatedDate}\" DNE.");
                }

                // Is it correctly formatted?
                if (DateTimeOffset.TryParse(attr.Value, out var lastUpdateDate) == false)
                {
                    throw new FormatException($"The attribute value \"{attr.Value}\" did not parse into a DateTimeOffset.");
                }

                LastUpdatedDate = lastUpdateDate;

                await Task.Run(() =>
                {
                    var wordNodeList = doc.SelectNodes($"//{XmlDefinitions.Word.E_Word}");

                    string text;
                    int length;

                    foreach (XmlNode node in wordNodeList)
                    {
                        text = XmlHelper.GetAttrValue(node, XmlDefinitions.Word.A_Text);

                        if (string.IsNullOrWhiteSpace(text) == false)
                        {
                            length = text.Length;

                            if (length > maxLength)
                            {
                                maxLength = length;
                            }

                            Add(text);
                        }
                    }

                    MaxWordLength = maxLength;

                    Sort(StringComparer.OrdinalIgnoreCase);
                });
            }
            catch
            {
                throw;
            }
        }

    }
}
