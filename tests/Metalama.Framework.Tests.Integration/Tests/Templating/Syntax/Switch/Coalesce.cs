using System;
using System.Collections.Generic;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.InternalPipeline.Templating.Syntax.Switch.Coalesce
{
      class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            // Both (evaluted at compile time into a run-time value)
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