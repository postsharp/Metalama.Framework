using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.Templating;

#pragma warning disable CS0169

namespace Metalama.Framework.Tests.Integration.Tests.Templating.Syntax.Misc.ConditionalExpression
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var extraField = meta.Target.Method.DeclaringType.Fields.OfName("extra").SingleOrDefault();

            // compile-time condition, compile-time result
            Console.WriteLine(extraField != null ? extraField.Name : "undefined");

            // run-time condition, compile-time result
            Console.WriteLine(meta.This.extra != null ? extraField!.Name : "undefined");

            // run-time condition, run-time result, inside compile-time condition, run-time result
            var extra = extraField != null ? meta.This.extra != null ? meta.This.extra.Value : 0 : 0;

            return meta.Proceed() + extra;
        }
    }

    class TargetCode
    {
        int? extra;

        int Method(int a)
        {
            return a;
        }
    }
}