using Caravela.Framework.Code;

namespace Caravela.Framework.Advices
{
    public enum IntroductionScope
    {
        Default,
        Instance,
        Static,
        Target
    }



    public interface IMethodIntroductionAdvice : IIntroduceAdvice<INamedType, IMethodBuilder>
    {
        
    }
}
