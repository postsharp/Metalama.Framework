// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Pipeline;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Testing
{
    public class TestBase
    {
        private readonly Func<ServiceProvider, ServiceProvider> _addServices;

        protected TestBase( Func<ServiceProvider, ServiceProvider>? addServices = null )
        {
            this._addServices = addServices ?? new Func<ServiceProvider, ServiceProvider>( p => p );
        }

        private static CSharpCompilation CreateCSharpCompilation(
            IReadOnlyDictionary<string, string> code,
            string? dependentCode = null,
            bool ignoreErrors = false,
            IEnumerable<MetadataReference>? additionalReferences = null,
            string? name = null,
            bool addMetalamaReferences = true,
            IEnumerable<string>? preprocessorSymbols = null )
        {
            var additionalAssemblies = new[] { typeof(TestBase).Assembly };

            var parseOptions = new CSharpParseOptions( preprocessorSymbols: preprocessorSymbols );

            var mainRoslynCompilation = TestCompilationFactory
                .CreateEmptyCSharpCompilation( name, additionalAssemblies, addMetalamaReferences )
                .AddSyntaxTrees( code.Select( c => SyntaxFactory.ParseSyntaxTree( c.Value, path: c.Key, options: parseOptions ) ) );

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
        
        protected TestContext CreateTestContext( TestProjectOptions? projectOptions = null ) => this.CreateTestContext( null, projectOptions );

        protected TestContext CreateTestContext( Func<ServiceProvider, ServiceProvider>? addServices, TestProjectOptions? projectOptions = null )
            => new( this, projectOptions, addServices );

        protected class TestContext : IDisposable
        {
            public TestProjectOptions ProjectOptions { get; }

            public ServiceProvider ServiceProvider { get; }

            public TestContext( TestBase parent, TestProjectOptions? projectOptions, Func<ServiceProvider, ServiceProvider>? addServices )
            {
                this.ProjectOptions = projectOptions ?? new TestProjectOptions();

                this.ServiceProvider = ServiceProviderFactory.GetServiceProvider( this.ProjectOptions.PathOptions )
                    .WithService( this.ProjectOptions )
                    .WithProjectScopedServices( TestCompilationFactory.GetMetadataReferences() )
                    .WithMark( ServiceProviderMark.Test );

                this.ServiceProvider = parent._addServices( this.ServiceProvider );

                if ( addServices != null )
                {
                    this.ServiceProvider = addServices( this.ServiceProvider );
                }
            }

            public ICompilation CreateCompilation(
                string code,
                string? dependentCode = null,
                bool ignoreErrors = false,
                IEnumerable<MetadataReference>? additionalReferences = null,
                string? name = null )
                => this.CreateCompilationModel( code, dependentCode, ignoreErrors, additionalReferences, name );
            
            private CompilationModel CreateCompilationModel(
                string code,
                string? dependentCode = null,
                bool ignoreErrors = false,
                IEnumerable<MetadataReference>? additionalReferences = null,
                string? name = null,
                bool addMetalamaReferences = true )
                => this.CreateCompilationModel(
                    new Dictionary<string, string> { { "test.cs", code } },
                    dependentCode,
                    ignoreErrors,
                    additionalReferences,
                    name,
                    addMetalamaReferences );

            private CompilationModel CreateCompilationModel(
                IReadOnlyDictionary<string, string> code,
                string? dependentCode = null,
                bool ignoreErrors = false,
                IEnumerable<MetadataReference>? additionalReferences = null,
                string? name = null,
                bool addMetalamaReferences = true )
            {
                var roslynCompilation = CreateCSharpCompilation( code, dependentCode, ignoreErrors, additionalReferences, name, addMetalamaReferences );

                return CompilationModel.CreateInitialInstance(
                    new ProjectModel( roslynCompilation, this.ServiceProvider ),
                    roslynCompilation );
            }

            public void Dispose()
            {
                this.ProjectOptions.Dispose();
            }
        }
    }
}