// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.TestFramework;

namespace Metalama.AspectWorkbench.Model
{
    internal class TemplateTest
    {
        public TestInput? Input { get; set; }

        public string? ExpectedTransformedCode { get; set; }

        public string? ExpectedProgramOutput { get; set; }
    }
}