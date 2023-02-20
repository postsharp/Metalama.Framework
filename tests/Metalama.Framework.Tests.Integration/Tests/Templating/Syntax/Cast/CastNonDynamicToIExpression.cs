using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.Expressions;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Cast.CastNonDynamicToIExpression;

[CompileTime]
class Aspect
{
    [TestTemplate]
    dynamic Template()
    {
        var expression = (IExpression)new TargetCode();

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