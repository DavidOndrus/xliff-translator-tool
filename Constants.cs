namespace XliffTranslatorTool
{
    public static class Constants
    {
        #region XLIFF CONSTANTS
        public const string XLIFF_VERSION_V12 = "1.2";
        public const string XLIFF_VERSION_V20 = "2.0";

        public const string XLIFF_NAMESPACE_V12 = "urn:oasis:names:tc:xliff:document:1.2";
        public const string XLIFF_NAMESPACE_V20 = "urn:oasis:names:tc:xliff:document:2.0";
        #endregion

        #region FILE DIALOGS
        public const string FILE_DIALOG_DEFAULT_EXT = ".xlf";
        public const string FILE_DIALOG_FILTER = "XLIFF documents (*.xlf;*.xliff)|*.xlf;*.xliff";
        #endregion

        #region XML NODES & ATTRIBUTES
        public const string XML_NODE_ROOT = "xliff";
        public const string XML_NODE_FILE = "file";
        public const string XML_NODE_BODY_V12 = "body";
        public const string XML_NODE_TRANSLATION_UNIT_V12 = "trans-unit";
        public const string XML_NODE_TRANSLATION_UNIT_V20 = "unit";
        public const string XML_NODE_SOURCE = "source";
        public const string XML_NODE_TARGET = "target";
        public const string XML_NODE_NOTE = "note";
        public const string XML_NODE_NOTES_V20 = "notes";
        public const string XML_NODE_SEGMENT_V20 = "segment";
        public const string XML_NODE_CONTEXT_GROUP_V12 = "context-group";
        public const string XML_NODE_CONTEXT_V12 = "context";

        public const string XML_ATTRIBUTE_VERSION = "version";
        public const string XML_ATTRIBUTE_NAMESPACE = "xmlns";
        public const string XML_ATTRIBUTE_IDENTIFIER = "id";
        public const string XML_ATTRIBUTE_EXTRA_DATA_V12 = "from";
        public const string XML_ATTRIBUTE_EXTRA_DATA_V20 = "category";
        public const string XML_ATTRIBUTE_SOURCE_LANGUAGE_V12 = "source-language";
        public const string XML_ATTRIBUTE_SOURCE_LANGUAGE_V20 = "srcLang";

        public const string XML_ATTRIBUTE_VALUE_DESCRIPTION = "description";
        public const string XML_ATTRIBUTE_VALUE_MEANING = "meaning";
        #endregion
    }
}
