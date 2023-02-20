using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Tests.Templating.LocalFunctions.NameConflict_GenericParameter;

#pragma warning disable CS8321

[CompileTime]
class Aspect
{
    [TestTemplate]
    dynamic? Template()
    {
        int TheLocalFunction<TParameter>()
        {
            return meta.Proceed();
        }

        return TheLocalFunction<int>();
    }
}
    
class TargetCode
{
    int Method(int a)
    {
        void TParameter()
        {
            // Some conflict.
        }

        return a;
    }
}