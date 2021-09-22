#pragma warning disable CS0162 // Unreachable code detected

using Caravela.Framework.Aspects;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.Templating.Pragma.CommentInEmptyBlock
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