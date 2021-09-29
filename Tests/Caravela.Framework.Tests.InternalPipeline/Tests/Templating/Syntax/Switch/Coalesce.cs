using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.InternalPipeline.Templating.Syntax.Switch.Coalesce
{
      class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            // Both (falls back to run-time)
            _ = default(int?) ?? 1;
            
            // Compile-time
            _ = meta.CompileTime(default(int?)) ?? 2;
            _ = default(int?) ?? meta.CompileTime( 3 );
           
            // Run-time
            _ = meta.RunTime( default(int?) ) ?? 4;
           _ = default(int?) ?? meta.RunTime( 5 );
            
            
            
            return default;
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