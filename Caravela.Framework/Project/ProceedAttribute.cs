using System;

namespace Caravela.Framework.Project
{
    /// <summary>
    /// Marks the method as having <c>proceed</c> semantics.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method )]
    public class ProceedAttribute : TemplateKeywordAttribute
    {
    }
}