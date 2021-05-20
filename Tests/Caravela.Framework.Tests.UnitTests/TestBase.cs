// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Caravela.Framework.Tests.UnitTests
{
    public class TestBase : IDisposable
    {
        /// <summary>
        /// A value indicating whether tests that test the serialization of reflection objects like <see cref="Type"/> should use "dotnet build" to see if the
        /// resulting syntax tree actually compiles and results in valid IL. This is slow but necessary during development, at least, since an incorrect syntax tree
        /// can easily be produced.
        /// </summary>
        private const bool _doCodeExecutionTests = false;

        private TestProjectOptions _projectOptions = new();

        protected ServiceProvider ServiceProvider { get; private set; }

        protected TestBase()
        {
            this.ServiceProvider = ServiceProviderFactory.GetServiceProvider( this._projectOptions );
        }

        public static CSharpCompilation CreateCSharpCompilation(
            string code,
            string? dependentCode = null,
            bool ignoreErrors = false,
            IEnumerable<MetadataReference>? additionalReferences = null,
            string? name = null )
            => CreateCSharpCompilation(
                new Dictionary<string, string> { { Guid.NewGuid() + ".cs", code } },
                dependentCode,
                ignoreErrors,
                additionalReferences,
                name );

        public static CSharpCompilation CreateCSharpCompilation(
            IReadOnlyDictionary<string, string> code,
            string? dependentCode = null,
            bool ignoreErrors = false,
            IEnumerable<MetadataReference>? additionalReferences = null,
            string? name = null )
        {
            CSharpCompilation CreateEmptyCompilation()
            {
                return CSharpCompilation.Create( name ?? "test_" + Guid.NewGuid() )
                    .WithOptions( new CSharpCompilationOptions( OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true ) )
                    .AddReferences(
                        new[] { "netstandard", "System.Runtime" }
                            .Select(
                                r => MetadataReference.CreateFromFile(
                                    Path.Combine( Path.GetDirectoryName( typeof(object).Assembly.Location )!, r + ".dll" ) ) ) )
                    .AddReferences(
                        MetadataReference.CreateFromFile( typeof(object).Assembly.Location ),
                        MetadataReference.CreateFromFile( typeof(DynamicAttribute).Assembly.Location ),
                        MetadataReference.CreateFromFile( typeof(TestBase).Assembly.Location ),
                        MetadataReference.CreateFromFile( typeof(CompileTimeAttribute).Assembly.Location ) );
            }

            var mainRoslynCompilation = CreateEmptyCompilation();

            if ( code != null )
            {
                mainRoslynCompilation = mainRoslynCompilation.AddSyntaxTrees( code.Select( c => SyntaxFactory.ParseSyntaxTree( c.Value, path: c.Key ) ) );
            }

            if ( dependentCode != null )
            {
                var dependentCompilation = CreateEmptyCompilation().AddSyntaxTrees( SyntaxFactory.ParseSyntaxTree( dependentCode ) );
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

        protected static void AssertNoError( CSharpCompilation mainRoslynCompilation )
        {
            var diagnostics = mainRoslynCompilation.GetDiagnostics();

            if ( diagnostics.Any( diag => diag.Severity >= DiagnosticSeverity.Error ) )
            {
                var lines = diagnostics.Select( diag => diag.ToString() ).Prepend( "The given code produced errors:" );

                throw new InvalidOperationException( string.Join( Environment.NewLine, lines ) );
            }
        }

        internal static CompilationModel CreateCompilationModel( string code, string? dependentCode = null )
        {
            var roslynCompilation = CreateCSharpCompilation( code, dependentCode );

            return CompilationModel.CreateInitialInstance( roslynCompilation );
        }

        public static object? ExecuteExpression( string context, string expression )
        {
            var expressionContainer = $@"
class Expression
{{
    public static object Execute() => {expression};
}}";

            var assemblyPath = CaravelaCompiler.CompileAssembly( context, expressionContainer );

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
        public static void TestExpression<T>( string context, string expression, Action<T> withResult )
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

        public void Dispose()
        {
            this._projectOptions.Dispose();
            this.ServiceProvider.Dispose();
        }

        protected IsolatedTest WithIsolatedTest() => new( this );

        protected class IsolatedTest : IDisposable
        {
            private readonly TestBase _parent;
            private readonly ServiceProvider _oldServiceProvider;
            private readonly TestProjectOptions _oldProjectOptions;

            public TestProjectOptions ProjectOptions { get; } = new();

            public ServiceProvider ServiceProvider { get; }

            public IsolatedTest( TestBase parent )
            {
                this._parent = parent;
                this._oldServiceProvider = parent.ServiceProvider;
                this._oldProjectOptions = parent._projectOptions;
                this.ServiceProvider = ServiceProviderFactory.GetServiceProvider( this.ProjectOptions );
            }

            public void Dispose()
            {
                this.ProjectOptions.Dispose();
                this.ServiceProvider.Dispose();
                this._parent.ServiceProvider = this._oldServiceProvider;
                this._parent._projectOptions = this._oldProjectOptions;
            }
        }
    }
}