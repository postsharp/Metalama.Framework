using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.Templating;

#pragma warning disable CS0169

namespace Metalama.Framework.Tests.Integration.Tests.Templating.Syntax.Misc.ConditionalExpressionError
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var extraField = meta.Target.Method.DeclaringType.Fields.OfName("extra").SingleOrDefault();

            // compile-time condition, mix of run-time and compile-time results
            Console.WriteLine(extraField != null ? extraField.Name : meta.RunTime("undefined"));

            return null;
        }
    }

    class TargetCode
    {
        void Method()
        {
        }
    }
}