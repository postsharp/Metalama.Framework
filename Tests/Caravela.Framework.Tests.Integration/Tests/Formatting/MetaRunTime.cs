using System;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.Tests.Formatting.MetaRunTime
{
   
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            return meta.RunTime( DateTime.Now );
        }
    }
}