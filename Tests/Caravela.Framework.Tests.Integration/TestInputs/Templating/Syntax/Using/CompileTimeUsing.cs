using System;
using System.Collections.Generic;
using Caravela.Framework.Project;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Using.CompileTimeUsing
{
    [CompileTime]
    class DisposableClass : IDisposable
    {
        public void Dispose()
        {
           
        }
    }
    
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            using ( new DisposableClass() )
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