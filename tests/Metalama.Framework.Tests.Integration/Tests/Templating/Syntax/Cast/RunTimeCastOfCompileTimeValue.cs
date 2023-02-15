using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Cast.RunTimeCastOfCompileTimeValue;

[CompileTime]
class Aspect
{
    [TestTemplate]
    dynamic? Template()
    {
        var parameter = (TargetCode)meta.Target.Parameters[0];

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