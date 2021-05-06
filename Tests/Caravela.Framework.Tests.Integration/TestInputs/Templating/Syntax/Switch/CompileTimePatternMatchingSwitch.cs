using System;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using Caravela.Framework.Code;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Switch.CompileTimePatternMatchingSwitch
{
    [CompileTimeOnly]
    enum SwitchEnum
    {
        one = 1,
        two = 2,
    }

    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            

            switch (meta.Parameters)
            {
                case null:
                    Console.WriteLine("1");
                    break;
                case IEnumerable<IParameter> enumerable when enumerable.Any():
                    meta.Comment(enumerable.Count().ToString());
                    break;
                case IEnumerable<IParameter> enumerable when !enumerable.Any():
                    meta.Comment("none");
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