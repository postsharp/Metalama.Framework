using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.CSharp12.PrimaryConstructor_CompileTime;

public class TheAspect(int x) : MethodAspect
{
    private int _x = x;

    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        base.BuildAspect(builder);

        builder.Advice.Override(builder.Target, nameof(M));
    }

    [Template]
    public void M()
    {
        Console.WriteLine(this._x);
        meta.Proceed();
    }
}

// <target>
public class C
{
    [TheAspect(42)]
    public void M()
    {
    }
}