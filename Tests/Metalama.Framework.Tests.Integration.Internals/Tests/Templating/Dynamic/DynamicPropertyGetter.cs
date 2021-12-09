using System;
using Metalama.Framework.Aspects;
using Metalama.TestFramework;


namespace Metalama.Framework.Tests.Integration.Templating.Dynamic.DynamicPropertyMember
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            // Dynamic property as method argument.
            Console.WriteLine(meta.This);
            Console.WriteLine(meta.Target.Parameters[0].Value);
            
            // Dynamic property in assignment;
            object o;
            o = meta.This;
            
            // Dynamic property in variable initialization/
            object x = meta.This;
            
            return default;
        }
    }

    // <target>
    class TargetCode
    {
        int Method(int a)
        {
            return a;
        }
    }
}