﻿namespace Caravela.AspectWorkbench.Model
{
    internal static class NewTestDefaults
    {
        public const string TemplateSource = @"  
using System;
using System.Collections.Generic;
using Caravela.Framework.Project;
using static Caravela.Framework.Aspects.TemplateContext;


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
