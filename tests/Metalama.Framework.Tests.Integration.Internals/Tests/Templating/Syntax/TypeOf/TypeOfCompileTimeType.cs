#pragma warning disable CS8600, CS8603
using System;
using Metalama.Framework.Code;
using Metalama.Testing.AspectTesting;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.CSharpSyntax.TypeOf.TypeOfCompileTimeType
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var rt = meta.RunTime(typeof(string));
            var ct = typeof(string);
            Console.WriteLine("rt=" + rt);
            Console.WriteLine("ct=" + ct);

            if (( (IParameter)meta.Target.Parameters[0] ).Type.Is(typeof(string)))
            {
            
            }


            return meta.Proceed();
        }
    }

    [CompileTime]
    public class MyClass1 { }

    class TargetCode
    {
        string Method(string a)
        {
            return a;
        }
    }
}