using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

// TODO: Change the namespace
namespace Caravela.Framework.Tests.Integration.Templating.ChangeMe
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            // Neutral
            var neutral = $"Zero={0,-5:x}";
            
            // Compile-time
            var ct = $"ParameterCount={meta.Parameters.Count,-5:x}";
            
            // Run-time
            var rt = $"Value={meta.Parameters[0].Value,-5:x}";
            
            Console.WriteLine(ct);
            return meta.Proceed();
        }
    }

    class TargetCode
    {
        int Method(int a)
        {
            return a;
        }
    }
}