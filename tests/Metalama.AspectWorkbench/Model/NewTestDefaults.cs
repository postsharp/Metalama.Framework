// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.AspectWorkbench.Model
{
    internal static class NewTestDefaults
    {
        public const string TemplateSource =
            @"using System;
using System.Collections.Generic;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace $ns
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