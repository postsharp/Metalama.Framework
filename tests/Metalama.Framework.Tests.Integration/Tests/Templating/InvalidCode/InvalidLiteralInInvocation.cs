using Metalama.Framework.Aspects;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Templating.InvalidCode.InvalidLiteralInInvocation
{

public class EnrichExceptionAttribute : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        var methodSignatureBuilder = new InterpolatedStringBuilder();

#if TESTRUNNER
        // The next line has an intentional syntax error.
        methodSignatureBuilder.AddText(""(');
#endif
        
        return meta.Proceed();
    }
}
}