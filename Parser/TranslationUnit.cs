namespace XliffTranslatorTool.Parser
{
    public class TranslationUnit
    {
        public string Identifier { get; set; }
        public string Meaning { get; set; }
        public string Description { get; set; }
        public string Source { get; set; }
        public string Target { get; set; }

        public override bool Equals(object obj)
        {
            return ((obj as TranslationUnit).Identifier == this.Identifier);
        }

        public override int GetHashCode()
        {
            return this.Identifier.GetHashCode();
        }
    }
}
