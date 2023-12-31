#pragma warning disable CS8600, CS8603
using System;
using System.Linq;
using System.Collections.Generic;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.ForEachTests.ForEachRunTimeContainsProceed
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            IEnumerable<int> array = Enumerable.Range(1, 2);
            foreach (var i in array)
            {
                return meta.Proceed();
            }

            return null;
        }
    }

    class TargetCode
    {
        void Method(int a, int bb)
        {
            Console.WriteLine(a + bb);
        }
    }
}