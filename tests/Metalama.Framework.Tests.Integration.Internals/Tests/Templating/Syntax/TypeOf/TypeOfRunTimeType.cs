#pragma warning disable CS8600, CS8603
using System;
using Metalama.Framework.Code;
using Metalama.Testing.AspectTesting;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.CSharpSyntax.TypeOf.TypeOfRunTimeType
{
    [CompileTime]
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
        {
            
            var rt = meta.RunTime( typeof(MyClass1) );
            var ct = meta.CompileTime( typeof(MyClass1) );
            Console.WriteLine( "rt=" + rt );
            Console.WriteLine( "ct=" + ct );

            if (( (IParameter)meta.Target.Parameters[0] ).Type.Is( typeof(MyClass1) ))
            {
                Console.WriteLine( "Oops" );
            }

            // Use in a run-time class.
            Console.WriteLine( typeof(MyClass1) );
            Console.WriteLine( typeof(MyClass1).FullName );
            

            // Use in a compile-time class.
            _ = TypeFactory.GetType( typeof(MyClass1) );

            return meta.Proceed();
        }
    }

    public class MyClass1 { }

    internal class TargetCode
    {
        private string Method( MyClass1 a )
        {
            return "";
        }
    }
}