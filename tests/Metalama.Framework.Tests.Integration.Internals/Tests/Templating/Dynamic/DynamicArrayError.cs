using System;
using System.Collections.Generic;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Testing.AspectTesting;

#pragma warning disable CS0168

namespace Metalama.Framework.Tests.Integration.Templating.Dynamic.DynamicArrayError
{
    [CompileTime]
    internal class Aspect
    {
        [TestTemplate]
        private dynamic? Template()
        {
            dynamic[] forbidden1;
            var forbidden2 = new dynamic[5];

            return default;
        }
    }

    // <target>
    internal class TargetCode
    {
        private int Method( int a, string c, DateTime dt )
        {
            return a;
        }
    }
}