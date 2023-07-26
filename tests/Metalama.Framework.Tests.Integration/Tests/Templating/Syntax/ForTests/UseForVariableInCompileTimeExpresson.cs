using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.ForTests.UseForVariableInCompileTimeExpresson
{
    [CompileTime]
    internal class Aspect
    {
        [TestTemplate]
        private dynamic? Template()
        {
            for (var i = 0; i < meta.Target.Parameters.Count; i++)
            {
                Console.WriteLine( meta.Target.Parameters[i].Name );
            }

            var result = meta.Proceed();

            return result;
        }
    }

    internal class TargetCode
    {
        private int Method( int a, int b )
        {
            return a + b;
        }
    }
}