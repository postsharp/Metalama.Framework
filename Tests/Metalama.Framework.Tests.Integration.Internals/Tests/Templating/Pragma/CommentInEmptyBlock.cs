#pragma warning disable CS0162 // Unreachable code detected

using Metalama.Framework.Aspects;
using Metalama.TestFramework;

namespace Metalama.Framework.Tests.Integration.Templating.Pragma.CommentInEmptyBlock
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            if ( false )
            {
                meta.InsertComment("Oops 1");
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