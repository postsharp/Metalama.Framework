// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.TestFramework;
using Microsoft.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Tests.Integration.Aspects
{
    public class DiagnosticTests : UnitTestBase
    {
        public DiagnosticTests( ITestOutputHelper logger ) : base( logger ) { }

        [Fact]
        public async Task ReportFromInitialize()
        {
            var testResult = await this.GetTestResultAsync( @"Aspects\Diagnostics\ReportFromInitialize.cs" );
            Assert.False( testResult.Success );
            Assert.Contains( testResult.Diagnostics.Where( d => d.Severity != DiagnosticSeverity.Hidden ), d => d.Id == "MY001" );
        }

        [Fact]
        public async Task SkipWithoutError()
        {
            var testResult = await this.GetTestResultAsync( @"Aspects\Diagnostics\SkipWithoutError.cs" );
            Assert.True( testResult.Success );
            Assert.DoesNotContain( "This code should not be emitted.", testResult.TransformedTargetSourceText?.ToString() );
        }

        [Fact]
        public async Task ReportFromTemplate()
        {
            var testResult = await this.GetTestResultAsync( @"Aspects\Diagnostics\ReportFromTemplate.cs" );
            Assert.False( testResult.Success );
            Assert.Contains( testResult.Diagnostics.Where( d => d.Severity != DiagnosticSeverity.Hidden ), d => d.Id == "MY001" );
        }

        [Fact]
        public async Task InvalidCompileTimeUserCode()
        {
            await this.AssertTransformedSourceEqualAsync( @"Aspects\Diagnostics\InvalidCompileTimeUserCode.cs" );
        }

        protected override TestRunnerBase CreateTestRunner() => new AspectTestRunner( this.ProjectDirectory );
    }
}