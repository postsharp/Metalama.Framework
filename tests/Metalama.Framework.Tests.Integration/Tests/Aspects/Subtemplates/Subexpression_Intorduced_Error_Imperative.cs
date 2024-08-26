using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Subtemplates.Subexpression_Intorduced_Error_Imperative;

internal class Aspect : TypeAspect
{
    [Introduce]
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