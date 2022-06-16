using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.AppendParameter.UseExpression;

public class MyAspect : ConstructorAspect
{
    public override void BuildAspect(IAspectBuilder<IConstructor> builder)
    {
        builder.Advice.IntroduceParameter(builder.Target, "p", typeof(DateTime), TypedConstant.Default<DateTime>(), ( parameter, constructor ) => PullAction.UseExpression( ExpressionFactory.Parse( "System.DateTime.Now" ) ) );
    }
}

public class C
{
    [MyAspect]
    public C() { }

    public C(string s) : this() { }
}

public class D : C
{

}