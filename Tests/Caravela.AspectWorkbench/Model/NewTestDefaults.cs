// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.AspectWorkbench.Model
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
