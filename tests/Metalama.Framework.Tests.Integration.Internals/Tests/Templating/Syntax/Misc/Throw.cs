#pragma warning disable CS0162 // Unreachable code detected

using System;
using Metalama.Testing.AspectTesting;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.Misc.Throw
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
        
            try
            {
                throw new ArgumentNullException(meta.Target.Parameters[0].Name);
            }
            catch 
            {
                throw;
            }
            
            
            return meta.Proceed();
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