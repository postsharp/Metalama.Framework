using System.Collections.Generic;
using Metalama.Testing.AspectTesting;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;
using System;

namespace Metalama.Framework.Tests.Integration.Tests.Formatting.SwitchStatements;

class Aspect : IAspect
{
    [Template]
    dynamic? Template()
    {
        switch (0)
        {
            case 0:
                Console.WriteLine("zero");
                break;
        }

        switch (meta.CompileTime(0))
        {
            case 0:
                Console.WriteLine("zero");
                break;
        }

        switch (meta.RunTime(0))
        {
            case 0:
                Console.WriteLine("zero");
                break;
        }

        return meta.Proceed();
    }
}
