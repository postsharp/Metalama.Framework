using System;
using System.Collections.Generic;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.Tests.Integration.Aspects.Misc.AdvancedInterpolatedStringBuilder
{
    class GenerateMethodAttribute : TypeAspect
    {
        [Introduce]
        public string GeneratedMethod()
        {
            // Build an interpolated string that contains all parameters.
            var stringBuilder = new InterpolatedStringBuilder();

            stringBuilder.AddExpression(0);
            stringBuilder.AddExpression("1", -5);
            stringBuilder.AddExpression(2, format: "x");
            stringBuilder.AddExpression(3, 10, "g");

            stringBuilder.AddText(" a");
            stringBuilder.AddExpression("b");
            stringBuilder.AddText("c");
            stringBuilder.AddExpression("d", 3);
            stringBuilder.AddText("e");
            stringBuilder.AddExpression("f", format: "s");
            stringBuilder.AddExpression("f", format: null);

            return stringBuilder.ToValue();
        }
    }

    // <target>
    [GenerateMethod]
    class Program { }
}