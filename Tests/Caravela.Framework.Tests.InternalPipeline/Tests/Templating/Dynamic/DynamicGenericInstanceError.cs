using System;
using System.Collections.Generic;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code.SyntaxBuilders;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.Templating.Dynamic.DynamicGenericInstanceError
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