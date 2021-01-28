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
    public enum Visibility
    {
        Private,
        PrivateInternal,
        Protected,
        ProtectedInternal,
        Internal,
        Public
    }

    public interface IIntroductionAdvice : IAdvice<INamedType> 
    {
        //IntroductionScope Scope { get; }
        //string Name { get; }
        //bool IsStatic { get; }
        //bool IsVirtual { get; }
        //Visibility Visibility { get; }
    }
}
