using Caravela.Framework.Code;

namespace Caravela.Framework.Advices
{
    public interface IAdvice 
    { 

    }

    public interface IAdvice<out T> : IAdvice where T : ICodeElement
    {
    }
}
