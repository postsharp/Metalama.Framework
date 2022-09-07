// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Diagnostics;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.Diagnostics
{
    public class DiagnosticListTests
    {
        [Fact]
        public void Add()
        {
            DiagnosticList list = new();

            var diagnostic = Diagnostic.Create(
                "id",
                "category",
                new NonLocalizedString( "message" ),
                DiagnosticSeverity.Error,
                DiagnosticSeverity.Error,
                true,
                0 );

            list.Report( diagnostic );
            Assert.Single( list, diagnostic );
            Assert.Same( diagnostic, list[0] );
        }
    }
}