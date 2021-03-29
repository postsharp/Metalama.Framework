  
using System;
using System.Collections.Generic;
using Caravela.Framework.Project;
using static Caravela.Framework.Aspects.TemplateContext;


// TODO: Change the namespace
namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Using.RunTimeUsing
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            using ( target.This )
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
