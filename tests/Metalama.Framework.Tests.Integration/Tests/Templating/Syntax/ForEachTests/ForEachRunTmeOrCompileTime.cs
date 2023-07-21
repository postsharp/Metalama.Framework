using System;
using System.Collections.Generic;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.ForEachTests.ForEachRunTmeOrCompileTime;

[CompileTime]
class Aspect
{
    [TestTemplate]
    void Template()
    {
        foreach (var i in new[] { 42 })
        {
            Console.WriteLine(i);
        }
    }
}

class TargetCode
{
    void Method()
    {
    }
}