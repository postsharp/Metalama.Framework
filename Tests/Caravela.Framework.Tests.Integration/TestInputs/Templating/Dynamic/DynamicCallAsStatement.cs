using System;
using System.Collections.Generic;
using Caravela.Framework.Project;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;
using meta = Caravela.Framework.Aspects.meta;

namespace Caravela.Framework.Tests.Integration.Templating.Dynamic.DynamicCallAsStatement
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            // Expression statement
            meta.Method.Invoke( meta.This, 0 );
            
            // Assignment
            _ = meta.Method.Invoke( meta.This, 1 );
            
            return default;
        }
    }

    [TestOutput]
    class TargetCode
    {
        int Method(int a)
        {
            return a;
        }
    }
}