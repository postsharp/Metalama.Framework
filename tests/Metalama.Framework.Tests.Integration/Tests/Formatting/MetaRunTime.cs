using System;
using Metalama.Framework.Aspects;
using Metalama.Testing.Framework;

namespace Metalama.Framework.Tests.Integration.Tests.Formatting.MetaRunTime
{
   
    [RunTimeOrCompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            return meta.RunTime( DateTime.Now );
        }
    }
}