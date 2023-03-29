using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Switch.RunTimeOrCompileTimeOldSwitch;

[RunTimeOrCompileTime]
enum SwitchEnum
{
    one = 1,
    two = 2,
}

class Aspect
{
    [TestTemplate]
    dynamic? Template()
    {
        var i = meta.CompileTime(SwitchEnum.one);

        switch (i)
        {
            case SwitchEnum.one:
                Console.WriteLine("1");
                break;
            case SwitchEnum.two:
                Console.WriteLine("2");
                break;
            default:
                break;
        }
        
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