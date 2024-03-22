using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.TestInputs.Highlighting.Declarations.CompileTimeCodeInRuntimeCode
{
    class Aspect : IAspect
    {
        [Template]
        dynamic? Template()
        {
            Console.WriteLine( $"Invoking {meta.Target.Method.ToDisplayString()}" );

            return meta.Proceed();
        }
    }
}
