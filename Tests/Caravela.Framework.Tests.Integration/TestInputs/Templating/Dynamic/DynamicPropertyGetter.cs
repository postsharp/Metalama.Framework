using System;
using System.Collections.Generic;
using Caravela.Framework.Project;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Dynamic.DynamicPropertyMember
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            // Dynamic property as method argument.
            Console.WriteLine(target.This);
            Console.WriteLine(target.Parameters[0].Value);
            
            // Dynamic property in assignment;
            object o;
            o = target.This;
            
            // Dynamic property in variable initialization/
            object x = target.This;
            
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