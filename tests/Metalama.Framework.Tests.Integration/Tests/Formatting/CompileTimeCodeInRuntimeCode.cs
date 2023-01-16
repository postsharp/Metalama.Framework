﻿using System;
using Metalama.Testing.AspectTesting;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.TestInputs.Highlighting.Declarations.CompileTimeCodeInRuntimeCode
{
    [RunTimeOrCompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            Console.WriteLine( $"Invoking {meta.Target.Method.ToDisplayString()}" );

            return meta.Proceed();
        }
    }
}
