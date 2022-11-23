// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.Testing
{
    public class TestBase
    {
        static TestBase()
        {
            TestingServices.Initialize();
        }

        protected virtual void ConfigureServices( MocksFactory services ) { }

        protected static CSharpCompilation CreateCSharpCompilation(
            string code,
            string? dependentCode = null,
            bool ignoreErrors = false,
            IEnumerable<MetadataReference>? additionalReferences = null,
            string? name = null,
            bool addMetalamaReferences = true,
            IEnumerable<string>? preprocessorSymbols = null,
            OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary )
            => CreateCSharpCompilation(
                new Dictionary<string, string> { { RandomIdGenerator.GenerateId() + ".cs", code } },
                dependentCode,
                ignoreErrors,
                additionalReferences,
                name,
                addMetalamaReferences,
                preprocessorSymbols,
                outputKind );

        internal static CSharpCompilation CreateCSharpCompilation(
            IReadOnlyDictionary<string, string> code,
            string? dependentCode = null,
            bool ignoreErrors = false,
            IEnumerable<MetadataReference>? additionalReferences = null,
            string? name = null,
            bool addMetalamaReferences = true,
            IEnumerable<string>? preprocessorSymbols = null,
            OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary )
        {
            var additionalAssemblies = new[] { typeof(TestBase).Assembly };

            var parseOptions =
                SupportedCSharpVersions.DefaultParseOptions.WithPreprocessorSymbols( preprocessorSymbols: preprocessorSymbols ?? new[] { "METALAMA" } );

            var mainRoslynCompilation = TestCompilationFactory
                .CreateEmptyCSharpCompilation( name, additionalAssemblies, addMetalamaReferences, outputKind: outputKind )
                .AddSyntaxTrees( code.SelectEnumerable( c => SyntaxFactory.ParseSyntaxTree( c.Value, path: c.Key, options: parseOptions ) ) );

            if ( dependentCode != null )
            {
                var dependentCompilation = TestCompilationFactory
                    .CreateEmptyCSharpCompilation( name == null ? null : null + ".Dependency", additionalAssemblies )
                    .AddSyntaxTrees( SyntaxFactory.ParseSyntaxTree( dependentCode, parseOptions ) );

                mainRoslynCompilation = mainRoslynCompilation.AddReferences( dependentCompilation.ToMetadataReference() );
            }

            if ( additionalReferences != null )
            {
                mainRoslynCompilation = mainRoslynCompilation.AddReferences( additionalReferences );
            }

            if ( !ignoreErrors )
            {
                AssertNoError( mainRoslynCompilation );
            }

            return mainRoslynCompilation;
        }

        private static void AssertNoError( CSharpCompilation mainRoslynCompilation )
        {
            var diagnostics = mainRoslynCompilation.GetDiagnostics();

            if ( diagnostics.Any( diag => diag.Severity >= DiagnosticSeverity.Error ) )
            {
                var lines = diagnostics.Select( diag => diag.ToString() ).Prepend( "The given code produced errors:" );

                throw new InvalidOperationException( string.Join( Environment.NewLine, lines ) );
            }
        }

        protected TestContext CreateTestContext( MocksFactory serviceFactory ) => this.CreateTestContext( null, serviceFactory );

        protected TestContext CreateTestContext( TestProjectOptions? projectOptions = null, MocksFactory? mockFactory = null )
            => new(
                projectOptions ?? new TestProjectOptions( additionalAssemblies: ImmutableArray.Create( this.GetType().Assembly ) ),
                null,
                this.GetMockServices(mockFactory) );

        private MocksFactory GetMockServices(MocksFactory? arg)
        {
            var services = arg ?? new MocksFactory();
            this.ConfigureServices( services );

            return services;
        }
    }
}