using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;

namespace Caravela.Framework.Impl
{
    internal abstract class IntroductionNameProvider
    {
        internal abstract string GetOverrideName( AspectPartId advice, IMethod overriddenDeclaration );
    }
}