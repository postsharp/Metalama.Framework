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



    public interface IIntroduceMethodAdvice : IIntroduceAdvice<INamedType, IMethodBuilder>
    {
        
    }
}
