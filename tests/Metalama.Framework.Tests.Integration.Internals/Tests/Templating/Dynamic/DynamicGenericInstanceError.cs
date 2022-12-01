using System;
using System.Collections.Generic;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.Templating;

#pragma warning disable CS0168

namespace Metalama.Framework.Tests.Integration.Templating.Dynamic.DynamicGenericInstanceError
{
    [CompileTime]
    internal class Aspect
    {
        [TestTemplate]
        private dynamic? Template()
        {
            List<dynamic> forbidden1;
            var forbidden2 = new Dictionary<dynamic,string>();

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