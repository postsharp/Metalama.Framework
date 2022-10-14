#if TEST_OPTIONS
// @FormatOutput
#endif

using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.TestFramework;

#pragma warning disable CS0169

namespace Metalama.Framework.Tests.Integration.Templating.InterpolatedStringFormatted
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic? Template()
        {
            // Neutral is compile-time.
            var neutral = $"Zero={0,-5:x}";

            // Compile-time with formatting
            var ct = $"ParameterCount={meta.Target.Parameters.Count,-5:x}";

            // Run-time
            var rt = $"Value={meta.Target.Parameters[0].Value,-5:x}";

            // Both
            var both = $"{meta.Target.Type.Fields.Single().Name}={meta.Target.Parameters[0].Value}";

            Console.WriteLine( ct );

            return meta.Proceed();
        }
    }

    internal class TargetCode
    {
        private int field;

        private int Method( int a )
        {
            return a;
        }
    }
}