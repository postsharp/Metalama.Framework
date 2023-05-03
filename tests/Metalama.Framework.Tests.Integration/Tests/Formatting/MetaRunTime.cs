using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Templating;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.Tests.Integration.Tests.Formatting.MetaRunTime
{   
    [RunTimeOrCompileTime]
    class Aspect
    {
        [TestTemplate]
        void Template()
        {
            var metalamaRelease = meta.RunTime( meta.CompileTime( new DateTime( 2023, 5, 3 ) ) );
            var now = meta.RunTime( DateTime.Now );
        }
    }
}