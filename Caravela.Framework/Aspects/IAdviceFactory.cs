using Caravela.Framework.Advices;
using Caravela.Framework.Code;

namespace Caravela.Framework.Aspects
{
    public interface IAdviceFactory
    {
        IOverrideMethodAdvice OverrideMethod( IMethod method, string defaultTemplate );
    }
}