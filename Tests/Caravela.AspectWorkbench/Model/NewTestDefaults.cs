namespace Caravela.AspectWorkbench.Model
{
    internal static class NewTestDefaults
    {
        public const string TemplateSource = @"  
using System;
using System.Collections.Generic;

// TODO: Change the namespace
namespace Caravela.Framework.Tests.Integration.Templating.ChangeMe
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            return proceed();
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
";
    }
}
