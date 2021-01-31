namespace Caravela.Framework.DesignTime.Contracts
{
    public enum TextSpanCategory
    {
        // Order of declaration (or at last enum value) matters. The higher value overwrites the lower.
        Default,
        CompileTime,
        Dynamic,
        TemplateVariable,
        TemplateKeyword,
        Conflict // A text span has several categories.
    }
}