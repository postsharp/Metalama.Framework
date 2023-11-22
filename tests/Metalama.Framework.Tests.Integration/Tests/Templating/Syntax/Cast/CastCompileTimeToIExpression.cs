using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Cast.CastCompileTimeToIExpression;

[CompileTime]
class Aspect
{
    [TestTemplate]
    dynamic Template()
    {
        var expression = (IExpression)meta.Target.Parameters[0];

        return expression.Value!.ToString();
    }
}

class TargetCode
{
    string Method(string a)
    {
        return a;
    }
}