#pragma warning disable CS0162 // Unreachable code detected

using System;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Misc.Throw
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