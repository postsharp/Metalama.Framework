using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.Linking
{
    internal class LinkerIntroductionNameProvider : IntroductionNameProvider
    {
        internal override string GetOverrideName( AspectPartId aspectPart, IMethod overriddenDeclaration )
        {
            return
                aspectPart.PartName != null
                    ? $"__{overriddenDeclaration.Name}__{aspectPart.AspectType}__{aspectPart.PartName}"
                    : $"__{overriddenDeclaration.Name}__{aspectPart.AspectType}";
        }
    }
}
