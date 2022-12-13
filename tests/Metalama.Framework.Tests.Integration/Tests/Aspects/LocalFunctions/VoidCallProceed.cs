using System.Threading;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplateTypeParameter.LocalFunction_TypeParameter_Error;

public class TheAspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        void LocalFunction()
        {
            // Invalid code generated here.
            meta.Proceed();
        }

        LocalFunction();
        return default;

    }
}

// <target>
internal class C
{
    [TheAspect]
    private int M() => 5;
}