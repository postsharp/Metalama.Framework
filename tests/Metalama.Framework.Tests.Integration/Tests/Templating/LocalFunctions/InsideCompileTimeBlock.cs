using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;
using System;

namespace Metalama.Framework.Tests.Integration.Tests.Templating.LocalFunctions.InsideCompileTimeBlock;

[CompileTime]
class Aspect
{
    [TestTemplate]
    dynamic? Template()
    {
        if (true)
        {
            {
                var local = (Action<TargetCode>)Local;

                void Local(TargetCode x) { }
            }
        }

        {
            var local = (Action<TargetCode>)Local;

            void Local(TargetCode x) { }
        }

        return default;
    }
}
    
class TargetCode
{
    int Method(int a)
    {
        return a;
    }
}