using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Cast.CompileTimeCastOfRunTimeValue;

[CompileTime]
class Aspect
{
    [TestTemplate]
    dynamic? Template()
    {
        var parameter = (IParameter)meta.Target.Parameters[0].Value!;

        return null;
    }
}

class TargetCode
{
    string Method(string a)
    {
        return a;
    }
}