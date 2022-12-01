using System;
using System.Collections.Generic;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Switch.CompileTimePatternMatchingSwitch
{
    [CompileTime]
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
            switch (meta.Target.Parameters)
            {
                case null:
                    Console.WriteLine("1");
                    break;
                case IEnumerable<IParameter> enumerable when enumerable.Any():
                    meta.InsertComment(enumerable.Count().ToString());
                    break;
                case IEnumerable<IParameter> enumerable when !enumerable.Any():
                    meta.InsertComment("none");
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