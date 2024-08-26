using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Subtemplates.Subexpression_Intorduced_Error_Programmatic;

internal class Aspect : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        base.BuildAspect(builder);

        builder.IntroduceMethod(nameof(this.IntroducedMethod));
    }

    [Template]
    public void IntroducedMethod()
    {
        LocalFunction();
        
        [Template]
        void LocalFunction()
        {
        }
    }
}

// <target>
[Aspect]
internal class TargetCode
{
}