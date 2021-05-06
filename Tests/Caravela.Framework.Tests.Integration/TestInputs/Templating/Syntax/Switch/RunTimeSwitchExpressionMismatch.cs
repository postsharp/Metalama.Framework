using System;
using System.Linq;
using System.Collections.Generic;
using Caravela.Framework.Code;
using Caravela.Framework;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.RunTimeSwitchExpressionMismatch
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
             
            object o = new ();
            
            var y = o switch 
            {
                IParameter p => 1,
                _ => 0
            };
            
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