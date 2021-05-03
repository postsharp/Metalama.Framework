using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Code;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.PatternMatching.PropertyPatternScopeMismatch
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
                  var rt = new object();

var a3 = rt is IParameter p3 && p3.DefaultValue.IsNull;

                    
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