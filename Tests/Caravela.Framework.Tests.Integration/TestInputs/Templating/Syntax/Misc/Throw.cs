#pragma warning disable CS0162 // Unreachable code detected

using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Misc.Throw
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
        
            try
            {
                throw new ArgumentNullException(target.Parameters[0].Name);
            }
            catch 
            {
                throw;
            }
            
            
            return proceed();
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