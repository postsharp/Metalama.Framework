  
using System;
using System.Collections.Generic;
using Caravela.Framework.Project;
using static Caravela.Framework.Aspects.TemplateContext;


// TODO: Change the namespace
namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Lock.CompileTimeLock
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            lock ( target.Compilation )
            {
                return proceed();
            }
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
