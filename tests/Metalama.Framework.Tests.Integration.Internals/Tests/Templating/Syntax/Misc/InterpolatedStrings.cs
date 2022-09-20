using System;
using System.Linq;
using Metalama.TestFramework;
using Metalama.Framework.Aspects;

#pragma warning disable CS0169

namespace Metalama.Framework.Tests.Integration.Templating.InterpolatedString
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic? Template()
        {
            // Neutral
            var neutral = $"Zero={0,-5:x}";

            // Compile-time with formatting
            var ct = $"ParameterCount={meta.Target.Parameters.Count,-5:x}";

            // Dynamic.
            var dy = $"Value={meta.Target.Parameters[0].Value,-5:x}";

            // Run-time expression that would cause global::.
            var rt = $"Value={Environment.Version}";

            // Both
            var both = $"{meta.Target.Type.Fields.Single().Name}={meta.Target.Parameters[0].Value}";

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