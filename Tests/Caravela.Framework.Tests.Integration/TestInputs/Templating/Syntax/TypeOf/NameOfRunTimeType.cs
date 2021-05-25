#pragma warning disable CS8600, CS8603
using System;
using Caravela.Framework.Code;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.CSharpSyntax.TypeOf.NameOfRunTimeType
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var rt = meta.RunTime(nameof(MyClass1));
            var ct = nameof(MyClass1);
            Console.WriteLine("rt=" + rt);
            Console.WriteLine("ct=" + ct);

            if (meta.Parameters[0].ParameterType is INamedType { Name: nameof(MyClass1) } )
            {
                Console.WriteLine("Oops");
            }

            Console.WriteLine(nameof(MyClass1));

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