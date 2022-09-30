#pragma warning disable CS8600, CS8603
using System;
using Metalama.Framework.Code;
using Metalama.TestFramework;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.CSharpSyntax.TypeOf.TypeOfRunTimeType
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var rt = meta.RunTime(typeof(MyClass1));
            var ct = meta.CompileTime(typeof(MyClass1));
            Console.WriteLine("rt=" + rt);
            Console.WriteLine("ct=" + ct);

            if (( (IParameter)meta.Target.Parameters[0] ).Type.Is(typeof(MyClass1)))
            {
                Console.WriteLine("Oops");
            }

            Console.WriteLine(typeof(MyClass1));
            Console.WriteLine(meta.CompileTime(typeof(MyClass1).FullName));

            return meta.Proceed();
        }
    }

    public class MyClass1 { }

    class TargetCode
    {
        string Method(MyClass1 a)
        {
            return "";
        }
    }
}