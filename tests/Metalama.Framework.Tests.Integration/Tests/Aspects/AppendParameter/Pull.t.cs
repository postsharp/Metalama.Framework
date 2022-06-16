using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.AppendParameter.Pull;
#pragma warning disable CS0067

public class MyAspect : ConstructorAspect
{
    public override void BuildAspect(IAspectBuilder<IConstructor> builder) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");

}

#pragma warning restore CS0067

public class C
{
    [MyAspect]
    public C(global::System.Int32 p = 15 ) { }

    public C(string s,global::System.Int32 p = 20 ) : this(p ) { }
}

public class D : C
{


public D(global::System.Int32 p = 20 )
:base(p ){
}
}
