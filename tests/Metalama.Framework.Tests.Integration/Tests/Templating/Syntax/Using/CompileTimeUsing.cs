#pragma warning disable CS0162

using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Using.CompileTimeUsing
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