using System;
using System.Collections.Generic;
using Caravela.Framework.Project;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

// TODO: Change the namespace
namespace Caravela.Framework.Tests.Integration.Templating.Dynamic.DynamicCallAsStatement
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            target.Method.Invoke( target.This, 0 );
            _ = target.Method.Invoke( target.This, 1 );
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