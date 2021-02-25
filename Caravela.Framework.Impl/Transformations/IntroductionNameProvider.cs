using Caravela.Framework.Code;

namespace Caravela.Framework.Impl
{
    internal abstract class IntroductionNameProvider
    {
        internal abstract string GetOverrideName( AspectPartId advice, IMethod overriddenDeclaration );
    }
}