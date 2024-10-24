using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.While.RunTimeInCompileTimeWhile
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic? Template()
        {
            var i = meta.CompileTime( 0 );

            while (i < meta.Target.Method.Name.Length)
            {
                i++;

                Console.WriteLine( i );
            }

            Console.WriteLine( "Test result = " + i );

            var result = meta.Proceed();

            return result;
        }
    }

    internal class TargetCode
    {
        private int Method( int a )
        {
            return a;
        }
    }
}