using System;
using System.Collections.Generic;
using System.Windows;
using System.Xml;

namespace XliffTranslatorTool.Parser
{
    public class XliffParser
    {
        private XmlNamespaceManager XmlNamespaceManager { get; } 
        private XmlDocument XmlDocument { get; } = new XmlDocument();
        private const string NAMESPACE_PREFIX = "ns";
        private enum XliffVersion
        {
            V12, V20, UNKNOWN
        }

        public XliffParser(string filePath)
        {
            XmlDocument.Load(filePath);
            XmlNamespaceManager = new XmlNamespaceManager(XmlDocument.NameTable);
        }

        private XliffVersion GetXliffVersion()
        {
            switch (XmlDocument.DocumentElement.GetAttribute(Constants.XML_ATTRIBUTE_VERSION))
            {
                case Constants.XLIFF_VERSION_V12:   return XliffVersion.V12;
                case Constants.XLIFF_VERSION_V20:   return XliffVersion.V20;
                default:                            return XliffVersion.UNKNOWN;
            }
        }

        public IList<TranslationUnit> GetTranslationUnits()
        {
            XmlNamespaceManager.AddNamespace(NAMESPACE_PREFIX, GetNamespace());
            switch (GetXliffVersion())
            {
                case XliffVersion.V12:  return GetTranslationUnitsV12();
                case XliffVersion.V20:  return GetTranslationUnitsV20();
                case XliffVersion.UNKNOWN:
                default:
                    {
                        MessageBox.Show($"XLIFF version was not recognized. Supported versions are: {String.Join(", ", Constants.XLIFF_VERSION_V12, Constants.XLIFF_VERSION_V20)}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return null;
                    }
            }
        }

        private string GetNamespace()
        {
            return XmlDocument.DocumentElement.NamespaceURI;
        }

        private IList<TranslationUnit> GetTranslationUnitsV12()
        {
            XmlNodeList translationUnitNodes = XmlDocument.DocumentElement.SelectNodes($"//{NAMESPACE_PREFIX}:{Constants.XML_NODE_TRANSLATION_UNIT_V12}", XmlNamespaceManager);

            IList<TranslationUnit> translationUnits = new List<TranslationUnit>();
            for (int translationUnitNodeIndex = 0; translationUnitNodeIndex < translationUnitNodes.Count; translationUnitNodeIndex++)
            {
                XmlNode translationUnitNode = translationUnitNodes.Item(translationUnitNodeIndex);
                string meaning = string.Empty;
                string description = string.Empty;
                string identifier = translationUnitNode.Attributes.GetNamedItem(Constants.XML_ATTRIBUTE_IDENTIFIER)?.Value ?? string.Empty;
                string source = translationUnitNode.SelectSingleNode($"{NAMESPACE_PREFIX}:{Constants.XML_NODE_SOURCE}", XmlNamespaceManager)?.InnerText ?? string.Empty;
                string target = translationUnitNode.SelectSingleNode($"{NAMESPACE_PREFIX}:{Constants.XML_NODE_TARGET}", XmlNamespaceManager)?.InnerText ?? string.Empty;

                XmlNodeList noteNodes = translationUnitNode.SelectNodes($"{NAMESPACE_PREFIX}:{Constants.XML_NODE_NOTE}", XmlNamespaceManager);
                for (int noteNodeIndex = 0; noteNodeIndex < noteNodes.Count; noteNodeIndex++)
                {
                    XmlNode noteNode = noteNodes.Item(noteNodeIndex);
                    string from = noteNode.Attributes.GetNamedItem(Constants.XML_ATTRIBUTE_EXTRA_DATA_V12)?.Value ?? string.Empty;
                    string value = noteNode.Attributes.GetNamedItem(Constants.XML_ATTRIBUTE_EXTRA_DATA_V12)?.InnerText ?? string.Empty;

                    switch (from)
                    {
                        case Constants.XML_ATTRIBUTE_VALUE_DESCRIPTION:
                            {
                                description = value;
                                break;
                            }
                        case Constants.XML_ATTRIBUTE_VALUE_MEANING:
                            {
                                meaning = value;
                                break;
                            }
                        default: continue;
                    }
                }

                translationUnits.Add(new TranslationUnit()
                {
                    Identifier = identifier,
                    Source = source,
                    Target = target,
                    Meaning = meaning,
                    Description = description
                });
            }

            return translationUnits;
        }

        private IList<TranslationUnit> GetTranslationUnitsV20()
        {
            XmlNodeList translationUnitNodes = XmlDocument.DocumentElement.SelectNodes($"//{NAMESPACE_PREFIX}:{Constants.XML_NODE_TRANSLATION_UNIT_V20}", XmlNamespaceManager);

            IList<TranslationUnit> translationUnits = new List<TranslationUnit>();
            for (int translationUnitNodeIndex = 0; translationUnitNodeIndex < translationUnitNodes.Count; translationUnitNodeIndex++)
            {
                XmlNode translationUnitNode = translationUnitNodes.Item(translationUnitNodeIndex);
                string meaning = string.Empty;
                string description = string.Empty;
                string identifier = translationUnitNode.Attributes.GetNamedItem(Constants.XML_ATTRIBUTE_IDENTIFIER)?.Value ?? string.Empty;

                XmlNode segmentNode = translationUnitNode.SelectSingleNode($"{NAMESPACE_PREFIX}:{Constants.XML_NODE_SEGMENT_V20}");
                string source = segmentNode?.SelectSingleNode($"{NAMESPACE_PREFIX}:{Constants.XML_NODE_SOURCE}")?.InnerText ?? string.Empty;
                string target = segmentNode?.SelectSingleNode($"{NAMESPACE_PREFIX}:{Constants.XML_NODE_TARGET}")?.InnerText ?? string.Empty;

                XmlNode notesNode = translationUnitNode.SelectSingleNode($"{NAMESPACE_PREFIX}:{Constants.XML_NODE_NOTES_V20}");
                if (notesNode != null)
                {
                    XmlNodeList noteNodes = notesNode.SelectNodes($"{NAMESPACE_PREFIX}:{Constants.XML_NODE_NOTE}");
                    for (int noteNodeIndex = 0; noteNodeIndex < noteNodes.Count; noteNodeIndex++)
                    {
                        XmlNode noteNode = noteNodes.Item(noteNodeIndex);
                        string category = noteNode.Attributes.GetNamedItem(Constants.XML_ATTRIBUTE_EXTRA_DATA_V20)?.Value ?? string.Empty;
                        string value = noteNode.Attributes.GetNamedItem(Constants.XML_ATTRIBUTE_EXTRA_DATA_V20)?.InnerText ?? string.Empty;

                        switch (category)
                        {
                            case Constants.XML_ATTRIBUTE_VALUE_DESCRIPTION:
                                {
                                    description = value;
                                    break;
                                }
                            case Constants.XML_ATTRIBUTE_VALUE_MEANING:
                                {
                                    meaning = value;
                                    break;
                                }
                            default: continue;
                        }
                    }
                }

                translationUnits.Add(new TranslationUnit()
                {
                    Identifier = identifier,
                    Source = source,
                    Target = target,
                    Meaning = meaning,
                    Description = description
                });
            }

            return translationUnits;
        }
    }
}
