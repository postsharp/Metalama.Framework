using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug33484;


public class TestAspect : MethodAspect
{
    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        //builder.Advice.Override(builder.Target, nameof(Template), args: new { T = typeof(int), a = 42 });
    }

    [Template]
    X Template([CompileTime] X a)
    {
        return a;
    }
}

public struct X
{
}