using System;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.Tests.Templating.Syntax.Misc.ThisError
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            // Must be an error.
            meta.Cast( meta.NamedType, this );

            return 0;
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