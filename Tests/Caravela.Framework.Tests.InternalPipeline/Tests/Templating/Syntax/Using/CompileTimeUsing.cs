#pragma warning disable CS0162

using System;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

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
        dynamic? Template()
        {
            using ( new DisposableClass() )
            {
                return meta.Proceed();
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