using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Code;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

// TODO: Change the namespace
namespace Caravela.Framework.Tests.Integration.PatternMatching.PropertyPattern
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
          // Compile time
          var ct = compileTime(new object());
          var a1 = ct is IParameter { Index: var index } p && p.DefaultValue.IsNull && index > 0;
          pragma.Comment("a1 = " + a1 );  
          
          // Run-time
          var a2 = target.Parameters[0].Value is >= 0 and < 5;
                    
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