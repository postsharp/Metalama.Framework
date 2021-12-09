using System;
using Metalama.TestFramework;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.Misc.ElementAccess
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
           Console.WriteLine( (string?) meta.Tags["TestKey"] );
           Console.WriteLine( meta.This[0] );
           Console.WriteLine( meta.Target.Parameters[0].Value );

           return 0;
        }
    }

    class TargetCode
    {
        int Method(int a)
        {
            return a;
        }
    }
}