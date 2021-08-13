#pragma warning disable CS8600, CS8603
using System;
using Caravela.Framework.Code;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.CSharpSyntax.TypeOf.TypeOfRunTimeType
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var rt = meta.RunTime(typeof(MyClass1));
            var ct = typeof(MyClass1);
            Console.WriteLine("rt=" + rt);
            Console.WriteLine("ct=" + ct);

            if (meta.Target.Parameters[0].ParameterType.Is(typeof(MyClass1)))
            {
                Console.WriteLine("Oops");
            }

            Console.WriteLine(typeof(MyClass1));
            Console.WriteLine(typeof(MyClass1).FullName);

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