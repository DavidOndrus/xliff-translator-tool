using System.Collections.Generic;

namespace XliffTranslatorTool.Parser
{
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class TranslationUnit
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        public bool IsMarked { get; set; } = false;
        public bool IsVisible { get; set; } = true;

        public string Identifier { get; set; }
        public string Meaning { get; set; }
        public string Description { get; set; }
        public string Source { get; set; }
        public string Target { get; set; }
        public string DataType { get; set; }
        public string Purpose { get; set; }
        public string SourceFile { get; set; }
        public List<int> LineNumbers { get; set; } = new List<int>();

        public override bool Equals(object obj)
        {
            TranslationUnit other = (obj as TranslationUnit);
            return (other != null) && (other.Identifier == this.Identifier);
        }
    }
}
