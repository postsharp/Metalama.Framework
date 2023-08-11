using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.RepeatedAnonymousTypeInTemplate_DifferentScope;

public class AspectAttribute : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Enumerable.Range(0, 10).Select(i => new { i });
        meta.CompileTime(Enumerable.Range(0, 10).Select(i => new { i }));
        return null;
    }
}

class Target
{
    // <target>
    [Aspect]
    void M() { }
}