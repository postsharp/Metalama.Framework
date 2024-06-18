using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Cast.CastNonDynamicToIExpression;

[CompileTime]
internal class Aspect
{
    [TestTemplate]
    private dynamic Template()
    {
        var expression = (IExpression)new TargetCode();

        return expression.Value!.ToString();
    }
}

internal class TargetCode
{
    private string Method( string a )
    {
        return a;
    }
}