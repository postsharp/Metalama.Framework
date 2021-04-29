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
        dynamic Template()
        {
            var list = compileTime(new List<string>());
            list.Add("a");
            list.Add("b");
            list.Add("c");
            list.Add("d");
            var x = runTime( list );
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