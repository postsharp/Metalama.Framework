// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.Templating
{
    public class SemanticModelAnalyzerTests : TestBase
    {
        [Fact]
        public void RuntimeCodeCallingCompileTimeOnlyMethod()
        {
            using var testContext = this.CreateTestContext();

            var compilation = CreateCSharpCompilation(
                @"
using Metalama.Framework.Code;

class X { void M() { IMethod m; } }

" );

            List<Diagnostic> diagnostics = new();
            var syntaxTree = compilation.SyntaxTrees[0];
            var semanticModel = compilation.GetSemanticModel( syntaxTree );

            TemplatingCodeValidator.Validate(
                testContext.ServiceProvider,
                semanticModel,
                diagnostics.Add,
                false,
                false,
                CancellationToken.None );

            Assert.Contains( diagnostics, d => d.Id == TemplatingDiagnosticDescriptors.CannotReferenceCompileTimeOnly.Id );
        }

        [Fact]
        public void MustImportNamespace()
        {
            using var testContext = this.CreateTestContext();

            var compilation = CreateCSharpCompilation(
                @"
class X : Metalama.Framework.Aspects.OverrideMethodAspect {  public override dynamic? OverrideMethod() { return null; } }

" );

            List<Diagnostic> diagnostics = new();
            var syntaxTree = compilation.SyntaxTrees[0];
            var semanticModel = compilation.GetSemanticModel( syntaxTree );

            TemplatingCodeValidator.Validate(
                testContext.ServiceProvider,
                semanticModel,
                diagnostics.Add,
                false,
                false,
                CancellationToken.None );

            Assert.Contains( diagnostics, d => d.Id == TemplatingDiagnosticDescriptors.CompileTimeCodeNeedsNamespaceImport.Id );
        }
    }
}