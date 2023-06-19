using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Misc.ToTypeOf
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var t = meta.Target.Parameters[0].Type.ToTypeOf();

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