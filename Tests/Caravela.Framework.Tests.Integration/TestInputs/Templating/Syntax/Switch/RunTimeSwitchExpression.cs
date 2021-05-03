#pragma warning disable CS8509

using System;
using System.Linq;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.RunTimeSwitchExpression
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var x = DateTime.Now.DayOfWeek switch
            {
             DayOfWeek.Monday => "Spaghetti",
             DayOfWeek.Tuesday => "Salad",
             _ => "McDonald"
            };
            
            object o = new ();
            
            var y = o switch 
            {
                IEnumerable<object> enumerable when enumerable.Count() > target.Parameters.Count => -1,
                IEnumerable<object> enumerable2 => enumerable2.Count()
            };
            
            return proceed();
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