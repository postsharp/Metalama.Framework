﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Testing.Framework;

namespace Metalama.AspectWorkbench.Model
{
    internal class TemplateTest
    {
        public TestInput? Input { get; set; }

        public string? ExpectedTransformedCode { get; set; }

        public string? ExpectedProgramOutput { get; set; }
    }
}