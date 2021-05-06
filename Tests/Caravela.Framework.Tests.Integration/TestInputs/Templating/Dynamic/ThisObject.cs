using System;
using System.Collections.Generic;
using Caravela.Framework.Project;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Dynamic.RunTimeMethod
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            target.This.MyMethod();
            target.This.Value = 5;
            return default;
        }
    }

    [TestOutput]
    class TargetCode
    {
        int Method(int a)
        {
            return a;
        }
    }
}