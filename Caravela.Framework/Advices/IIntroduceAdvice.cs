// unset

using Caravela.Framework.Code;

namespace Caravela.Framework.Advices
{
    public interface IIntroduceAdvice<out TTarget, out TBuilder> : IAdvice<TTarget>
        where TTarget : ICodeElement
        where TBuilder : ICodeElementBuilder
    {
        TBuilder Builder { get; }
    }
}