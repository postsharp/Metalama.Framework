using System;
using System.Collections.Generic;
using Caravela.Framework.Project;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;
using meta = Caravela.Framework.Aspects.meta;

namespace Caravela.Framework.Tests.Integration.Templating.Dynamic.DynamicPropertyMember
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            // Dynamic property as method argument.
            Console.WriteLine(meta.This);
            Console.WriteLine(meta.Parameters[0].Value);
            
            // Dynamic property in assignment;
            object o;
            o = meta.This;
            
            // Dynamic property in variable initialization/
            object x = meta.This;
            
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