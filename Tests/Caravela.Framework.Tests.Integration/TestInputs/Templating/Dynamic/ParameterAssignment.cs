using System;
using System.Collections.Generic;
using Caravela.Framework.Project;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Dynamic.ParameterAssignment
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var result = proceed();
            target.Parameters[0].Value = 5;
            return result;
        }
    }

    [TestOutput]
    class TargetCode
    {
        int Method(out int a)
        {
            a = 1;
            return 1;
        }
    }
}