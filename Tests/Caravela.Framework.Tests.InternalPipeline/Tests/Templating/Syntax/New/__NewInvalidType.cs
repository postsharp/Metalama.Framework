// @AcceptInvalidInput

using System;
using System.Text;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.New.NewInvalidType
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var o = new NonExistingType();
            
            return meta.Proceed();
        }
    }

    // <target>
    class TargetCode
    {
        int Method(int a)
        {
            return a;
        }
    }
}