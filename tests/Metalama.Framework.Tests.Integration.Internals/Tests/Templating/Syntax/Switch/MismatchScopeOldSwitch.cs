using System;
using Metalama.Testing.AspectTesting;
using Metalama.Framework.Aspects;

#pragma warning disable CS0162

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Switch.OldSwitchMismatchScope
{
    enum RunTimeEnum 
    {
        one = 1,
        two = 2,
    }

    [CompileTime]
    enum CompileTimeEnum
    {
        one = 1,
        two = 2,
    }

    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            
            switch ((int)CompileTimeEnum.one)
            {
                case (int)RunTimeEnum.one:
                    Console.WriteLine("1");
                    break;
                case (int)RunTimeEnum.two:
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
}