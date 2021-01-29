using Caravela.Framework.Aspects;
using System;

namespace Caravela.Framework.Advices
{
    /// <summary>
    /// Custom attributes that marks the target method as a template for <see cref="IOverrideMethodAdvice"/>.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method, Inherited = true )]
    public class OverrideMethodTemplateAttribute : TemplateAttribute
    {
        
    }
}
