using System.Xml;

namespace XliffTranslatorTool
{
    public class XliffParser
    {
        private XmlDocument XmlDocument { get; } = new XmlDocument();

        public XliffParser(string filePath)
        {
            XmlDocument.Load(filePath);
        }
    }
}
