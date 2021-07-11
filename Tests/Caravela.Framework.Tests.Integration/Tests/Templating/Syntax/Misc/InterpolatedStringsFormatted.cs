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
            var ct = $"ParameterCount={meta.Parameters.Count,-5:x}";
            
            // Run-time
            var rt = $"Value={meta.Parameters[0].Value,-5:x}";
            
            // Both
            var both = $"{meta.Type.Fields.Single().Name}={meta.Parameters[0].Value}";

            
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