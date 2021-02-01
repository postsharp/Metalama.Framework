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
        IntroductionScope? Scope { get; set; }
        string? Name { get; set; }
        bool? IsStatic { get; set; }
        bool? IsVirtual { get; set; }
        Visibility? Visibility { get; set; }
    }
}
