// @FormatOutput

using System;
using System.Linq;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

#pragma warning disable CS0169

namespace Caravela.Framework.Tests.Integration.Templating.InterpolatedStringFormatted
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            // Neutral
            var neutral = $"Zero={0,-5:x}";
            
            // Compile-time with formatting
            var ct = $"ParameterCount={meta.Target.Parameters.Count,-5:x}";
            
            // Run-time
            var rt = $"Value={meta.Target.Parameters[0].Value,-5:x}";
            
            // Both
            var both = $"{meta.Target.Type.Fields.Single().Name}={meta.Target.Parameters[0].Value}";

            
            Console.WriteLine(ct);
            return meta.Proceed();
        }
    }

    class TargetCode
    {
        private int field;
        
        int Method(int a)
        {
            return a;
        }
    }
}