using System;
using System.Collections.Generic;
using Caravela.Framework.Project;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;
using meta = Caravela.Framework.Aspects.meta;

namespace Caravela.Framework.Tests.Integration.Templating.Dynamic.ParameterAssignment
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var result = meta.Proceed();
            meta.Parameters[0].Value = 5;
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