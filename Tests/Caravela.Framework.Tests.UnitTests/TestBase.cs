﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Tests.UnitTests.Utilities;
using Caravela.TestFramework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Caravela.Framework.Tests.UnitTests
{
    public class TestBase
    {
        private readonly Func<ServiceProvider, ServiceProvider> _addServices;

        /// <summary>
        /// A value indicating whether tests that test the serialization of reflection objects like <see cref="Type"/> should use "dotnet build" to see if the
        /// resulting syntax tree actually compiles and results in valid IL. This is slow but necessary during development, at least, since an incorrect syntax tree
        /// can easily be produced.
        /// </summary>
        private const bool _doCodeExecutionTests = false;

        /// <summary>
        /// Gets the base options, which are being cloned by <see cref="CreateTestContext()"/>.
        /// </summary>
        private TestProjectOptions BaseProjectOptions { get; }

        /// <summary>
        /// Gets the base service provider, without project-scoped services. A test can get a <see cref="ServiceProvider"/> that has
        /// project-scoped services using <see cref="CreateTestContext()"/>.
        /// </summary>
        private ServiceProvider BaseServiceProvider { get; }

        protected TestBase( Func<ServiceProvider, ServiceProvider>? addServices = null )
        {
            this._addServices = addServices ?? new Func<ServiceProvider, ServiceProvider>( p => p );
            this.BaseServiceProvider = ServiceProviderFactory.GetServiceProvider( this.BaseProjectOptions ).WithMark( ServiceProviderMark.Test );

            if ( addServices != null )
            {
                this.BaseServiceProvider = addServices( this.BaseServiceProvider );
            }
        }

        protected static CSharpCompilation CreateCSharpCompilation(
            string code,
            string? dependentCode = null,
            bool ignoreErrors = false,
            IEnumerable<MetadataReference>? additionalReferences = null,
            string? name = null,
            bool addCaravelaReferences = true )
            => CreateCSharpCompilation(
                new Dictionary<string, string> { { Guid.NewGuid() + ".cs", code } },
                dependentCode,
                ignoreErrors,
                additionalReferences,
                name,
                addCaravelaReferences );

        protected static CSharpCompilation CreateCSharpCompilation(
            IReadOnlyDictionary<string, string> code,
            string? dependentCode = null,
            bool ignoreErrors = false,
            IEnumerable<MetadataReference>? additionalReferences = null,
            string? name = null,
            bool addCaravelaReferences = true )
        {
            var additionalAssemblies = new[] { typeof(TestBase).Assembly };

            var mainRoslynCompilation = TestCompilationFactory
                .CreateEmptyCSharpCompilation( name, additionalAssemblies, addCaravelaReferences )
                .AddSyntaxTrees( code.Select( c => SyntaxFactory.ParseSyntaxTree( c.Value, path: c.Key ) ) );

            if ( dependentCode != null )
            {
                var dependentCompilation = TestCompilationFactory
                    .CreateEmptyCSharpCompilation( name == null ? null : null + ".Dependency", additionalAssemblies )
                    .AddSyntaxTrees( SyntaxFactory.ParseSyntaxTree( dependentCode ) );

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

        protected static object? ExecuteExpression( string context, string expression )
        {
            var expressionContainer = $@"
class Expression
{{
    public static object Execute() => {expression};
}}";

            var assemblyPath = CaravelaCompilerUtility.CompileAssembly( context, expressionContainer );

            var assembly = Assembly.LoadFile( assemblyPath );

            return assembly.GetType( "Expression" )!.GetMethod( "Execute" )!.Invoke( null, null );
        }

        /// <summary>
        /// Executes the C# <paramref name="expression"/> alongside the code <paramref name="context"/> and passes the value of the expression
        /// as the argument to the callback <paramref name="withResult"/>. Does all of this only conditionally: it does nothing if <see cref="_doCodeExecutionTests"/>
        /// is false.
        /// </summary>
        /// <param name="context">Additional C# code.</param>
        /// <param name="expression">A C# expression of type <typeparamref name="T"/>.</param>
        /// <param name="withResult">Code to run on the result of the expression.</param>
        protected static void TestExpression<T>( string context, string expression, Action<T> withResult )
        {
#pragma warning disable CS0162 // Unreachable code detected

            // ReSharper disable HeuristicUnreachableCode

            if ( _doCodeExecutionTests )
            {
                var t = (T) ExecuteExpression( context, expression )!;
                withResult( t );
            }
#pragma warning restore CS0162 // Unreachable code detected
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

                this.ServiceProvider = ServiceProviderFactory.GetServiceProvider( this.ProjectOptions )
                    .WithProjectScopedServices()
                    .WithMark( ServiceProviderMark.Test );

                this.ServiceProvider = parent._addServices( this.ServiceProvider );

                if ( addServices != null )
                {
                    this.ServiceProvider = addServices( this.ServiceProvider );
                }
            }

            internal CompilationModel CreateCompilationModel(
                string code,
                string? dependentCode = null,
                bool ignoreErrors = false,
                IEnumerable<MetadataReference>? additionalReferences = null,
                string? name = null,
                bool addCaravelaReferences = true )
            {
                var roslynCompilation = CreateCSharpCompilation( code, dependentCode, ignoreErrors, additionalReferences, name, addCaravelaReferences );

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