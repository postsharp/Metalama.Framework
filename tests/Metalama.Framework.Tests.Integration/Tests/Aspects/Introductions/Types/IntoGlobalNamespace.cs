

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.IntoGlobalNamespace
{
    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.With( builder.Target.Compilation.GlobalNamespace ).IntroduceClass( "TestType" );
            builder.With( builder.Target.Compilation.GlobalNamespace ).IntroduceClass( "TestType", buildType: t => t.AddTypeParameter( "T" ) );
        }
    }

    // <target>
    [IntroductionAttribute]
    public class TargetType { }
}