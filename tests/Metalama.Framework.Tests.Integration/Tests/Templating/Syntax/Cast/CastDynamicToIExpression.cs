using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Cast.CastDynamicToIExpression;

[CompileTime]
class Aspect
{
    [TestTemplate]
    dynamic Template()
    {
        var parameter = (IExpression)meta.Target.Parameters[0].Value!;

        return parameter;
    }
}

class TargetCode
{
    string Method(string a)
    {
        return a;
    }
}