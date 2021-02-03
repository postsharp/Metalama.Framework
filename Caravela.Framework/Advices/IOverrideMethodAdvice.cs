using Caravela.Framework.Code;

namespace Caravela.Framework.Advices
{
    /// <summary>
    /// Represents an advice that overrides the implementation of a method.
    /// </summary>
    public interface IOverrideMethodAdvice : IAdvice<IMethod>
    {        
        IMethod TemplateMethod { get; }
    }
}
