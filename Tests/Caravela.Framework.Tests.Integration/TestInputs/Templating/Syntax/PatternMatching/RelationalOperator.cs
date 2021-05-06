using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

// TODO: Change the namespace
namespace Caravela.Framework.Tests.Integration.PatternMatching.RelationalOperator
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
          // Compile time
          var a1 = meta.Parameters.Count is >= 0 and < 5;
          meta.Comment("a1 = " + a1 );  
          
          // Run-time
          var a2 = meta.Parameters[0].Value is >= 0 and < 5;
                    
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