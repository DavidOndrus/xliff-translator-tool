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
            XmlNamespaceManager.AddNamespace("ns", GetNamespace());
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
            string xPath = $"//ns:{Constants.XML_NODE_TRANSLATION_UNIT_V12}";
            XmlNodeList translationUnitNodes = XmlDocument.DocumentElement.SelectNodes(xPath, XmlNamespaceManager);
            throw new NotImplementedException();
        }

        private IList<TranslationUnit> GetTranslationUnitsV20()
        {
            throw new NotImplementedException();
        }
    }
}
