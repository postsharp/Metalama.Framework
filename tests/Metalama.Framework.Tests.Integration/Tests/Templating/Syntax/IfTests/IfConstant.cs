using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;
using System;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.IfTests.IfConstant;

[CompileTime]
class Aspect
{
    [TestTemplate]
    void Template()
    {
        if (true)
        {
            Console.WriteLine("true");
        }

        const bool c = true;
        if (c)
        {
            Console.WriteLine("c");
        }

        bool b = true;
        if (b)
        {
            Console.WriteLine("b");
        }
    }
}

class TargetCode
{
    void Method() { }
}