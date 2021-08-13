using System;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Misc.ElementAccess
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