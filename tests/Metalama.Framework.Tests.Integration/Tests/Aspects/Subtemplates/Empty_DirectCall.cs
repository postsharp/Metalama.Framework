using System;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.Linking.Inlining;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Subtemplates.Empty_DirectCall;

internal sealed class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        this.OverrideAsyncMethod();

        return meta.Proceed();
    }
}

// <target>
internal class TargetCode
{
    [Aspect]
    private void Method()
    {
    }
}