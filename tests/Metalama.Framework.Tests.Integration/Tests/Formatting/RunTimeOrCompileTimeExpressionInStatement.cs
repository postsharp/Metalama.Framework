using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Tests.Formatting.RunTimeOrCompileTimeExpressionInStatement;

public class Aspect : OverrideMethodAspect
{
    public override dynamic OverrideMethod()
    {
        if (true)
        {
        }

        if ((true))
        {
        }

#if TESTRUNNER
        if (()meta.Proceed())
        {
        }
#endif

        foreach (var x in new[] { 42 })
        {
        }

        do
        {

        } while (false);

        while (true)
        {
        }
    }
}