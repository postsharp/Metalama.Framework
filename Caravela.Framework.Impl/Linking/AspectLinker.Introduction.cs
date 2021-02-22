using Caravela.Framework.Impl.Transformations;

namespace Caravela.Framework.Impl.Linking
{
    internal partial class AspectLinker
    {
        private record MemberIntroduction(
            IMemberIntroduction Introductor,
            IntroducedMember IntroducedMember
            );
    }
}
