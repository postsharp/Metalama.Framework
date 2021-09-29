using System;
using System.Collections.Generic;
using System.Linq;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.Integration.TestInputs.Highlighting.IfStatements.CompileTimePatternMatching
{
    [CompileTimeOnly]
    enum SwitchEnum
    {
        one = 1,
        two = 2,
    }

    class Aspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
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
    
}
