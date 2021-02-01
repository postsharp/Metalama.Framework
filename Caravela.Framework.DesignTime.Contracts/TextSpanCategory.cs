namespace Caravela.Framework.DesignTime.Contracts
{
    public enum TextSpanCategory
    {
        // Order of declaration (or at last enum value) matters. The higher value overwrites the lower.

        /// <summary>
        /// No category.
        /// </summary>
        Default,

        /// <summary>
        /// Run-time code in a template.
        /// </summary>
        RunTime,

        /// <summary>
        /// Compile-time in a template.
        /// </summary>
        CompileTime,

        /// <summary>
        /// COmment in a template.
        /// </summary>
        Comment,

        /// <summary>
        /// Dynamic member.
        /// </summary>
        Dynamic,

        /// <summary>
        /// Compile-time variable in a template.
        /// </summary>
        CompileTimeVariable,

        /// <summary>
        /// Keyword-like member in a template.
        /// </summary>
        TemplateKeyword,

        /// <summary>
        /// A text span has several categories.
        /// </summary>
        Conflict 
    }
}