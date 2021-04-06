using System;
using System.Collections.Generic;
using Caravela.Framework.Project;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Switch.PatternMatchingWithWhenSwitchCompileTime
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            dynamic result;
            object obj = compileTime(1);
            switch (obj)
            {
                case int i when i > 0: 
                    result = i;
                    break;
                case string s when s.Length < target.Parameters.Count:
                    result = s.Length;
                    break;
                default:
                    result = null;
                    break;
            }
            
            return result;
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