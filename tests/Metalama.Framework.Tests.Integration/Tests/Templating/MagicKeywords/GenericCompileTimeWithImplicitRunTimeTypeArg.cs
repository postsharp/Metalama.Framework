using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.TestInputs.MagicKeywords.GenericCompileTimeWithImplicitRunTimeTypeArg;

[CompileTime]
class Aspect
{
    [TestTemplate]
    dynamic? Template()
    {
        _ = meta.CompileTime(new TargetCode());

        return meta.Proceed();
    }
}

class TargetCode
{
    int Method(int a)
    {
        return a;
    }
}