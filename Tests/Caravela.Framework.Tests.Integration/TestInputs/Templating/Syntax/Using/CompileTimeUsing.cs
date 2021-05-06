#pragma warning disable CS0162

using System;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Using.CompileTimeUsing
{
    [CompileTimeOnly]
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
            
            using ( DisposableClass c = null )
            {
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