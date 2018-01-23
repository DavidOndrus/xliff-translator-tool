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
            TranslationUnit other = (obj as TranslationUnit);
            return (other != null) && (other.Identifier == this.Identifier);
        }

        public override int GetHashCode()
        {
            return this.Identifier?.GetHashCode() ?? string.Empty.GetHashCode();
        }
    }
}
