using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.AppendParameter.Record_UseExpression;
#pragma warning disable CS0067

public class MyAspect : ConstructorAspect
{
    public override void BuildAspect(IAspectBuilder<IConstructor> builder) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");

}

#pragma warning restore CS0067

public class R
{
    [MyAspect]
    public R(global::System.DateTime p = default(global::System.DateTime) ) { }

    public R(string s) : this(System.DateTime.Now ) { }
}

public class S : R
{


public S()
:base(System.DateTime.Now ){
}
}
