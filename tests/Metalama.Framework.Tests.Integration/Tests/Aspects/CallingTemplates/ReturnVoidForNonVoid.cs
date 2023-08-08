using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.CallingTemplates.ReturnVoidForNonVoid;

internal class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        CalledTemplate();
        return default;
    }

    [Template]
    private void CalledTemplate()
    {
        return;
    }
}

internal class TargetCode
{
    // <target>
    [Aspect]
    private int Method()
    {
        return 42;
    }
}