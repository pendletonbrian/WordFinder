using System.Xml;

namespace WordFinder.Classes
{
    internal static class XmlHelper
    {
        /// <summary>
        /// Appends the given attribute name and value to the given node.
        /// </summary>
        /// <param name="doc">
        /// </param>
        /// <param name="node">
        /// </param>
        /// <param name="attrName">
        /// </param>
        /// <param name="attrValue">
        /// </param>
        internal static void AddAttr(XmlDocument doc, ref XmlElement node, string attrName, string attrValue)
        {
            XmlAttribute attr = doc.CreateAttribute(attrName);
            attr.Value = attrValue;
            _ = node.Attributes.Append(attr);
        }

        /// <summary>
        /// Attempts to get the given attribute's value. If it can't,
        /// string.Empty is returned.
        /// </summary>
        /// <param name="node">
        /// </param>
        /// <param name="attrName">
        /// </param>
        /// <returns>
        /// </returns>
        internal static string GetAttrValue(XmlNode node, string attrName)
        {
            XmlAttribute attr = node.Attributes[attrName];

            return attr is null ? string.Empty : attr.Value;
        }
    }
}
