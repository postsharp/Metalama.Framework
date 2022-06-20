using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.AppendParameter.UseExpression;
#pragma warning disable CS0067

public class MyAspect : ConstructorAspect
{
    public override void BuildAspect(IAspectBuilder<IConstructor> builder) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");

}

#pragma warning restore CS0067

public class C
{
    [MyAspect]
    public C(global::System.DateTime p = default(global::System.DateTime) ) { }

    public C(string s) : this(System.DateTime.Now ) { }
}

public class D : C
{


public D()
:base(System.DateTime.Now ){
}
}
