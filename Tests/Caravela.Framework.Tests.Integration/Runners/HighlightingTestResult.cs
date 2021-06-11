// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.TestFramework;

// ReSharper disable StringLiteralTypo

namespace Caravela.Framework.Tests.Integration.Runners
{
    internal class HighlightingTestResult : TestResult
    {
        public string? OutputHtml { get; set; }
    }
}