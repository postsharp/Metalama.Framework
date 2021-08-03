// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.AspectWorkbench.Model
{
    internal static class NewTestDefaults
    {
        public const string TemplateSource =
            @"using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

// TODO: Change the namespace
namespace Caravela.Framework.Tests.Integration.Templating.ChangeMe
{
    class Aspect : Attribute
    {
    }

    class TargetCode
    {
        [Aspect]
        int Method(int a)
        {
            return a;
        }
    }
}";
    }
}