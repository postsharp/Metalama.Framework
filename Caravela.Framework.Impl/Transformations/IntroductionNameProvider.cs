using Caravela.Framework.Code;

namespace Caravela.Framework.Impl
{
    internal abstract class IntroductionNameProvider
    {
        internal abstract string GetOverrideName( AspectLayerId advice, IMethod overriddenDeclaration );
    }
}