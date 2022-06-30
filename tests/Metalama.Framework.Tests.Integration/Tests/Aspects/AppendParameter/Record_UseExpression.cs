using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.AppendParameter.Record_UseExpression;

public class MyAspect : ConstructorAspect
{
    public override void BuildAspect(IAspectBuilder<IConstructor> builder)
    {
        builder.Advice.IntroduceParameter(builder.Target, "p", typeof(DateTime), TypedConstant.Default<DateTime>(),
            ( parameter, constructor ) => PullAction.UseExpression( ExpressionFactory.Parse( "System.DateTime.Now" ) ) );
    }
}

public record R
{
    [MyAspect]
    public R() { }

    public R(string s) : this() { }
}

public record S : R
{

}