#pragma warning disable CS8600, CS8603
using System;
using Caravela.Framework.Code;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.CSharpSyntax.TypeOf.TypeOfCompileTimeType
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

            if (meta.Parameters[0].ParameterType.Is(typeof(string)))
            {
            
            }


            return meta.Proceed();
        }
    }

    [CompileTimeOnly]
    public class MyClass1 { }

    class TargetCode
    {
        string Method(string a)
        {
            return a;
        }
    }
}