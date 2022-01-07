﻿using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Xml;

namespace XliffTranslatorTool.Parser
{
    public class XliffParser
    {
        private const string NAMESPACE_PREFIX = "ns";

        private XmlNamespaceManager XmlNamespaceManager { get; set; } 
        private XmlDocument XmlDocument { get; } = new XmlDocument();
        private string SourceLanguage { get; set; }
        private XliffVersion LastFileXliffVersion { get; set; }
        private string LastFilePath { get; set; }
        public enum XliffVersion
        {
            V12,
            V20,
            UNKNOWN
        }

        public XliffParser() { }

        private XliffVersion GetXliffVersion()
        {
            switch (XmlDocument.DocumentElement.GetAttribute(Constants.XML_ATTRIBUTE_VERSION))
            {
                case Constants.XLIFF_VERSION_V12:
                    this.LastFileXliffVersion = XliffVersion.V12;
                    return XliffVersion.V12;
                case Constants.XLIFF_VERSION_V20:
                    this.LastFileXliffVersion = XliffVersion.V20;
                    return XliffVersion.V20;
                default:
                    this.LastFileXliffVersion = XliffVersion.UNKNOWN;
                    return XliffVersion.UNKNOWN;
            }
        }

        public ObservableCollection<TranslationUnit> GetTranslationUnitsFromFile(string filePath)
        {
            this.LastFilePath = filePath;

            string text = string.Empty;
            int originalTextSize = -1, escapedTextLength = -1;
            using (StreamReader streamReader = new StreamReader(filePath))
            {
                text = streamReader.ReadToEnd();
                originalTextSize = text.Length;
            }
            text = text.Replace("&", "_AMP;_");
            escapedTextLength = text.Length;

            //if (originalTextSize != escapedTextLength)
            //{
            //    MessageBox.Show("There were some invalid characters in the file.\nSince default XML parser doesn't work with invalid XML format file, these characters were replaced.\n& = &amp;", "Invalid file", MessageBoxButton.OK, MessageBoxImage.Information);
            //}

            try
            {
                XmlDocument.LoadXml(text);
            }
            catch (XmlException ex)
            {
                MessageBox.Show("Error occured while reading file. Please, create new issue with this report on GitHub:\n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            XmlNamespaceManager = new XmlNamespaceManager(XmlDocument.NameTable);
            XmlNamespaceManager.AddNamespace(NAMESPACE_PREFIX, GetNamespace());
            XliffVersion xliffVersion = GetXliffVersion();
            SaveSourceLanguage(xliffVersion);
            switch (xliffVersion)
            {
                case XliffVersion.V12:
                    return GetTranslationUnitsV12();
                case XliffVersion.V20:
                    return GetTranslationUnitsV20();
                default:
                    return null;
            }
        }

        private string GetNamespace()
        {
            return XmlDocument.DocumentElement.NamespaceURI;
        }

        private void SaveSourceLanguage(XliffVersion xliffVersion)
        {
            switch (xliffVersion)
            {
                case XliffVersion.V12:
                    {
                        SourceLanguage = XmlDocument.DocumentElement.SelectSingleNode($"{NAMESPACE_PREFIX}:{Constants.XML_NODE_FILE}", XmlNamespaceManager)?.Attributes.GetNamedItem(Constants.XML_ATTRIBUTE_SOURCE_LANGUAGE_V12)?.Value ?? string.Empty;
                        break;
                    }
                case XliffVersion.V20:
                    {
                        SourceLanguage = XmlDocument.DocumentElement?.Attributes.GetNamedItem(Constants.XML_ATTRIBUTE_SOURCE_LANGUAGE_V20)?.Value ?? string.Empty;
                        break;
                    }
                default:
                    throw new NotImplementedException("Not implemented XliffVersion");
            }
        }

        private ObservableCollection<TranslationUnit> GetTranslationUnitsV12()
        {
            XmlNodeList translationUnitNodes = XmlDocument.DocumentElement.SelectNodes($"//{NAMESPACE_PREFIX}:{Constants.XML_NODE_TRANSLATION_UNIT_V12}", XmlNamespaceManager);

            ObservableCollection<TranslationUnit> translationUnits = new ObservableCollection<TranslationUnit>();
            for (int translationUnitNodeIndex = 0; translationUnitNodeIndex < translationUnitNodes.Count; translationUnitNodeIndex++)
            {
                XmlNode translationUnitNode = translationUnitNodes.Item(translationUnitNodeIndex);
                string meaning = string.Empty;
                string description = string.Empty;
                string identifier = translationUnitNode.Attributes.GetNamedItem(Constants.XML_ATTRIBUTE_IDENTIFIER)?.Value ?? string.Empty;
                string source = EncodeAndCleanValue(translationUnitNode.SelectSingleNode($"{NAMESPACE_PREFIX}:{Constants.XML_NODE_SOURCE}", XmlNamespaceManager)?.InnerXml ?? string.Empty, XliffVersion.V12);
                string target = EncodeAndCleanValue(translationUnitNode.SelectSingleNode($"{NAMESPACE_PREFIX}:{Constants.XML_NODE_TARGET}", XmlNamespaceManager)?.InnerXml ?? string.Empty, XliffVersion.V12);

                XmlNodeList noteNodes = translationUnitNode.SelectNodes($"{NAMESPACE_PREFIX}:{Constants.XML_NODE_NOTE}", XmlNamespaceManager);
                for (int noteNodeIndex = 0; noteNodeIndex < noteNodes.Count; noteNodeIndex++)
                {
                    XmlNode noteNode = noteNodes.Item(noteNodeIndex);
                    string from = noteNode.Attributes.GetNamedItem(Constants.XML_ATTRIBUTE_EXTRA_DATA_V12)?.Value ?? string.Empty;
                    string value = noteNode.InnerText ?? string.Empty;

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
                        default:
                            continue;
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

        private string EncodeAndCleanValue(string str, XliffVersion xliffVersion)
        {
            switch (xliffVersion)
            {
                case XliffVersion.V12:
                    {
                        str = str.Replace(" xmlns=\"urn:oasis:names:tc:xliff:document:1.2\" ", string.Empty);
                        break;
                    }
                case XliffVersion.V20:
                    {
                        str = str.Replace(" xmlns=\"urn:oasis:names:tc:xliff:document:2.0\" ", string.Empty);
                        break;
                    }
                default:
                    throw new NotImplementedException("Not implemented XliffVersion");
            }

            return str.Replace("<", "_LT;_").Replace(">", "_GT;_");
        }

        private ObservableCollection<TranslationUnit> GetTranslationUnitsV20()
        {
            XmlNodeList translationUnitNodes = XmlDocument.DocumentElement.SelectNodes($"//{NAMESPACE_PREFIX}:{Constants.XML_NODE_TRANSLATION_UNIT_V20}", XmlNamespaceManager);

            ObservableCollection<TranslationUnit> translationUnits = new ObservableCollection<TranslationUnit>();
            for (int translationUnitNodeIndex = 0; translationUnitNodeIndex < translationUnitNodes.Count; translationUnitNodeIndex++)
            {
                XmlNode translationUnitNode = translationUnitNodes.Item(translationUnitNodeIndex);
                string meaning = string.Empty;
                string description = string.Empty;
                string identifier = translationUnitNode.Attributes.GetNamedItem(Constants.XML_ATTRIBUTE_IDENTIFIER)?.Value ?? string.Empty;

                XmlNode segmentNode = translationUnitNode.SelectSingleNode($"{NAMESPACE_PREFIX}:{Constants.XML_NODE_SEGMENT_V20}", XmlNamespaceManager);
                string source = EncodeAndCleanValue(segmentNode?.SelectSingleNode($"{NAMESPACE_PREFIX}:{Constants.XML_NODE_SOURCE}", XmlNamespaceManager)?.InnerXml ?? string.Empty, XliffVersion.V20);
                string target = EncodeAndCleanValue(segmentNode?.SelectSingleNode($"{NAMESPACE_PREFIX}:{Constants.XML_NODE_TARGET}", XmlNamespaceManager)?.InnerXml ?? string.Empty, XliffVersion.V20);

                XmlNode notesNode = translationUnitNode.SelectSingleNode($"{NAMESPACE_PREFIX}:{Constants.XML_NODE_NOTES_V20}", XmlNamespaceManager);
                if (notesNode != null)
                {
                    XmlNodeList noteNodes = notesNode.SelectNodes($"{NAMESPACE_PREFIX}:{Constants.XML_NODE_NOTE}", XmlNamespaceManager);
                    for (int noteNodeIndex = 0; noteNodeIndex < noteNodes.Count; noteNodeIndex++)
                    {
                        XmlNode noteNode = noteNodes.Item(noteNodeIndex);
                        string category = noteNode.Attributes.GetNamedItem(Constants.XML_ATTRIBUTE_EXTRA_DATA_V20)?.Value ?? string.Empty;
                        string value = noteNode.InnerText ?? string.Empty;

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
                            default:
                                continue;
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

        public XmlDocument CreateXliffDocument(XliffVersion xliffVersion, IEnumerable translationUnitsCollection)
        {
            XmlDocument xmlDocument = new XmlDocument();

            switch (xliffVersion)
            {
                case XliffVersion.V12:
                    return CreateXliffDocumentV12(xmlDocument, translationUnitsCollection);
                case XliffVersion.V20:
                    return CreateXliffDocumentV20(xmlDocument, translationUnitsCollection);
                default:
                    throw new NotImplementedException("Not implemented XliffVersion");
            }
        }

        private XmlDocument CreateXliffDocumentV12(XmlDocument xmlDocument, IEnumerable translationUnits)
        {
            XmlNode rootNode = xmlDocument.CreateElement(Constants.XML_NODE_ROOT, Constants.XLIFF_NAMESPACE_V12);

            XmlAttribute versionAttribute = xmlDocument.CreateAttribute(Constants.XML_ATTRIBUTE_VERSION);
            versionAttribute.Value = Constants.XLIFF_VERSION_V12;
            rootNode.Attributes.Append(versionAttribute);

            XmlAttribute namespaceAttribute = xmlDocument.CreateAttribute(Constants.XML_ATTRIBUTE_NAMESPACE);
            namespaceAttribute.Value = Constants.XLIFF_NAMESPACE_V12;
            rootNode.Attributes.Append(namespaceAttribute);

            XmlNode fileNode = xmlDocument.CreateElement(Constants.XML_NODE_FILE, Constants.XLIFF_NAMESPACE_V12);

            if (!String.IsNullOrEmpty(SourceLanguage))
            {
                XmlAttribute sourceLanguageAttribute = xmlDocument.CreateAttribute(Constants.XML_ATTRIBUTE_SOURCE_LANGUAGE_V12);
                sourceLanguageAttribute.Value = SourceLanguage;
                fileNode.Attributes.Append(sourceLanguageAttribute);
            }

            XmlNode bodyNode = xmlDocument.CreateElement(Constants.XML_NODE_BODY_V12, Constants.XLIFF_NAMESPACE_V12);

            foreach (TranslationUnit translationUnit in translationUnits)
            {
                if (String.IsNullOrEmpty(translationUnit.Identifier)) continue;

                XmlNode translationUnitNode = xmlDocument.CreateElement(Constants.XML_NODE_TRANSLATION_UNIT_V12, Constants.XLIFF_NAMESPACE_V12);

                XmlAttribute identifierAttribute = xmlDocument.CreateAttribute(Constants.XML_ATTRIBUTE_IDENTIFIER);
                identifierAttribute.Value = translationUnit.Identifier;
                translationUnitNode.Attributes.Append(identifierAttribute);

                XmlNode sourceNode = xmlDocument.CreateElement(Constants.XML_NODE_SOURCE, Constants.XLIFF_NAMESPACE_V12);
                sourceNode.InnerText = translationUnit.Source;
                translationUnitNode.AppendChild(sourceNode);

                XmlNode targetNode = xmlDocument.CreateElement(Constants.XML_NODE_TARGET, Constants.XLIFF_NAMESPACE_V12);
                targetNode.InnerText = translationUnit.Target;
                translationUnitNode.AppendChild(targetNode);

                if (!String.IsNullOrEmpty(translationUnit.Description))
                {
                    XmlNode descriptionNode = xmlDocument.CreateElement(Constants.XML_NODE_NOTE, Constants.XLIFF_NAMESPACE_V12);

                    XmlAttribute fromAttribute = xmlDocument.CreateAttribute(Constants.XML_ATTRIBUTE_EXTRA_DATA_V12);
                    fromAttribute.Value = Constants.XML_ATTRIBUTE_VALUE_DESCRIPTION;
                    descriptionNode.Attributes.Append(fromAttribute);
                    descriptionNode.InnerText = translationUnit.Description;
                    translationUnitNode.AppendChild(descriptionNode);
                }

                if (!String.IsNullOrEmpty(translationUnit.Meaning))
                {
                    XmlNode meaningNode = xmlDocument.CreateElement(Constants.XML_NODE_NOTE, Constants.XLIFF_NAMESPACE_V12);

                    XmlAttribute fromAttribute = xmlDocument.CreateAttribute(Constants.XML_ATTRIBUTE_EXTRA_DATA_V12);
                    fromAttribute.Value = Constants.XML_ATTRIBUTE_VALUE_MEANING;
                    meaningNode.Attributes.Append(fromAttribute);
                    meaningNode.InnerText = translationUnit.Meaning;
                    translationUnitNode.AppendChild(meaningNode);
                }

                bodyNode.AppendChild(translationUnitNode);
            }

            fileNode.AppendChild(bodyNode);
            rootNode.AppendChild(fileNode);
            xmlDocument.AppendChild(rootNode);

            return xmlDocument;
        }

        private XmlDocument CreateXliffDocumentV20(XmlDocument xmlDocument, IEnumerable translationUnits)
        {
            XmlNode rootNode = xmlDocument.CreateElement(Constants.XML_NODE_ROOT, Constants.XLIFF_NAMESPACE_V20);

            XmlAttribute versionAttribute = xmlDocument.CreateAttribute(Constants.XML_ATTRIBUTE_VERSION);
            versionAttribute.Value = Constants.XLIFF_VERSION_V20;
            rootNode.Attributes.Append(versionAttribute);

            XmlAttribute namespaceAttribute = xmlDocument.CreateAttribute(Constants.XML_ATTRIBUTE_NAMESPACE);
            namespaceAttribute.Value = Constants.XLIFF_NAMESPACE_V20;
            rootNode.Attributes.Append(namespaceAttribute);

            if (!String.IsNullOrEmpty(SourceLanguage))
            {
                XmlAttribute sourceLanguageAttribute = xmlDocument.CreateAttribute(Constants.XML_ATTRIBUTE_SOURCE_LANGUAGE_V20);
                sourceLanguageAttribute.Value = SourceLanguage;
                rootNode.Attributes.Append(sourceLanguageAttribute);
            }

            XmlNode fileNode = xmlDocument.CreateElement(Constants.XML_NODE_FILE, Constants.XLIFF_NAMESPACE_V20);

            foreach (TranslationUnit translationUnit in translationUnits)
            {
                if (String.IsNullOrEmpty(translationUnit.Identifier)) continue;

                XmlNode translationUnitNode = xmlDocument.CreateElement(Constants.XML_NODE_TRANSLATION_UNIT_V20, Constants.XLIFF_NAMESPACE_V20);

                XmlAttribute identifierAttribute = xmlDocument.CreateAttribute(Constants.XML_ATTRIBUTE_IDENTIFIER);
                identifierAttribute.Value = translationUnit.Identifier;
                translationUnitNode.Attributes.Append(identifierAttribute);

                XmlNode segmentNode = xmlDocument.CreateElement(Constants.XML_NODE_SEGMENT_V20, Constants.XLIFF_NAMESPACE_V20);

                XmlNode sourceNode = xmlDocument.CreateElement(Constants.XML_NODE_SOURCE, Constants.XLIFF_NAMESPACE_V20);
                sourceNode.InnerText = translationUnit.Source;
                segmentNode.AppendChild(sourceNode);

                XmlNode targetNode = xmlDocument.CreateElement(Constants.XML_NODE_TARGET, Constants.XLIFF_NAMESPACE_V20);
                targetNode.InnerText = translationUnit.Target;
                segmentNode.AppendChild(targetNode);
                translationUnitNode.AppendChild(segmentNode);

                bool isDescriptionNullOrEmpty = String.IsNullOrEmpty(translationUnit.Description);
                bool isMeaningNullOrEmpty = String.IsNullOrEmpty(translationUnit.Meaning);
                if (!isDescriptionNullOrEmpty || !isMeaningNullOrEmpty)
                {
                    XmlNode notesNode = xmlDocument.CreateElement(Constants.XML_NODE_NOTES_V20, Constants.XLIFF_NAMESPACE_V20);

                    if (!isDescriptionNullOrEmpty)
                    {
                        XmlNode descriptionNode = xmlDocument.CreateElement(Constants.XML_NODE_NOTE, Constants.XLIFF_NAMESPACE_V20);

                        XmlAttribute categoryAttribute = xmlDocument.CreateAttribute(Constants.XML_ATTRIBUTE_EXTRA_DATA_V20);
                        categoryAttribute.Value = Constants.XML_ATTRIBUTE_VALUE_DESCRIPTION;
                        descriptionNode.Attributes.Append(categoryAttribute);
                        descriptionNode.InnerText = translationUnit.Description;
                        notesNode.AppendChild(descriptionNode);
                    }

                    if (!isMeaningNullOrEmpty)
                    {
                        XmlNode meaningNode = xmlDocument.CreateElement(Constants.XML_NODE_NOTE, Constants.XLIFF_NAMESPACE_V20);

                        XmlAttribute categoryAttribute = xmlDocument.CreateAttribute(Constants.XML_ATTRIBUTE_EXTRA_DATA_V20);
                        categoryAttribute.Value = Constants.XML_ATTRIBUTE_VALUE_MEANING;
                        meaningNode.Attributes.Append(categoryAttribute);
                        meaningNode.InnerText = translationUnit.Meaning;
                        notesNode.AppendChild(meaningNode);
                    }

                    translationUnitNode.AppendChild(notesNode);
                }
                
                fileNode.AppendChild(translationUnitNode);
            }

            rootNode.AppendChild(fileNode);
            xmlDocument.AppendChild(rootNode);

            return xmlDocument;
        }

        public XliffVersion GetLastFileXliffVersion()
        {
            return this.LastFileXliffVersion;
        }

        public string GetLastFilePath()
        {
            return this.LastFilePath;
        }
    }
}
