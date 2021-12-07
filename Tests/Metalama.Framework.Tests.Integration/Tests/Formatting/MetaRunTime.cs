using System;
using Metalama.Framework.Aspects;
using Metalama.TestFramework;

namespace Metalama.Framework.Tests.Integration.Tests.Formatting.MetaRunTime
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