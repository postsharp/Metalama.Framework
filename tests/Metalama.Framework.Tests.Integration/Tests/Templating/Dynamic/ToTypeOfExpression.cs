using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Tests.Templating.Dynamic.ToTypeOfExpression
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            meta.InsertComment("Should return typeof(int).");
            return meta.Target.Parameters[0].Type.ToTypeOfExpression();
        }
    }

    class TargetCode
    {
        Type Method(int a)
        {
            // Should return typeof(int).
            return typeof(void);
        }
    }
}