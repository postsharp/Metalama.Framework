using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Subtemplates.Property;

internal class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        return P;
    }

    [Template]
    int P
    {
        get
        {
            Console.WriteLine("property subtemplate");
            return 42;
        }
    }
}

// <target>
internal class TargetCode
{
    [Aspect]
    private int Method()
    {
        return 0;
    }
}