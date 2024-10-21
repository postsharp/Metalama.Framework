using System.Linq;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.OutParam_Expression;

public class TheAspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        var arg = ExpressionFactory.Parse( "_field" );
        meta.Target.Method.Invoke( arg );

        return meta.Proceed();
    }
}

// <target>
public class C
{
    private object _field = new();

    [TheAspect]
    public void Method( out object o )
    {
        o = new object();
    }
}