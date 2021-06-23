using System;
using System.Linq;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

#pragma warning disable CS0169

// TODO: Change the namespace
namespace Caravela.Framework.Tests.Integration.Templating.InterpolatedString
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            // Neutral
            var neutral = $"Zero={0,-5:x}";
            
            // Compile-time with formatting
            var ct = $"ParameterCount={meta.Parameters.Count,-5:x}";
            
            // Run-time
            var rt = $"Value={meta.Parameters[0].Value,-5:x}";
            
            // Both
            var both = $"{meta.NamedType.Fields.Single().Name}={meta.Parameters[0].Value}";

            
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