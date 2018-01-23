namespace XliffTranslatorTool.Parser
{
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class TranslationUnit
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
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
    }
}
