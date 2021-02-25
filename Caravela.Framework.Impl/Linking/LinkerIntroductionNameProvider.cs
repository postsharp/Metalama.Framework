using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.Linking
{
    internal class LinkerIntroductionNameProvider : IntroductionNameProvider
    {
        internal override string GetOverrideName( AspectLayerId aspectLayer, IMethod overriddenDeclaration )
        {
            return
                aspectLayer.LayerName != null
                    ? $"__{overriddenDeclaration.Name}__{aspectLayer.AspectName}__{aspectLayer.LayerName}"
                    : $"__{overriddenDeclaration.Name}__{aspectLayer.AspectName}";
        }
    }
}
