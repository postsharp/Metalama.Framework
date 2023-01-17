using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.AppendParameter.Target_Record_DoNotPull;

public class MyAspect : ConstructorAspect
{
    public override void BuildAspect( IAspectBuilder<IConstructor> builder )
    {
        builder.Advise.IntroduceParameter( builder.Target, "p", typeof(int), TypedConstant.Create( 15 ) );
    }
}

public record R
{
    [MyAspect]
    public R() { }

    public R( string s ) : this() { }
}

public record S : R { }