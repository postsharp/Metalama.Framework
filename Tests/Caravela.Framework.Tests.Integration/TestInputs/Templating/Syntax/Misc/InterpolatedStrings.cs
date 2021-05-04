using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

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
            var ct = $"ParameterCount={target.Parameters.Count,-5:x}";
            
            // Run-time
            var rt = $"Value={target.Parameters[0].Value,-5:x}";
            
            Console.WriteLine(ct);
            return proceed();
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