using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Tests.Templating.LocalFunctions.NameConflict;

#pragma warning disable CS8321

[CompileTime]
class Aspect
{
    [TestTemplate]
    dynamic? Template()
    {
        object? TheLocalFunction()
        {
            return meta.Proceed();
        }

        return TheLocalFunction();
    }
}
    
class TargetCode
{
    int Method(int a)
    {
        void TheLocalFunction()
        {
            // Some conflict.
        }

        return a;
    }
}