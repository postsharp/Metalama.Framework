// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Caravela.Framework.Tests.UnitTests
{
    public class TestBase
    {
        /// <summary>
        /// A value indicating whether tests that test the serialization of reflection objects like <see cref="Type"/> should use "dotnet build" to see if the
        /// resulting syntax tree actually compiles and results in valid IL. This is slow but necessary during development, at least, since an incorrect syntax tree
        /// can easily be produced.
        /// </summary>
        private const bool _doCodeExecutionTests = true;

        public static CSharpCompilation CreateRoslynCompilation( string? code, string? dependentCode = null, bool ignoreErrors = false )
        {
            static CSharpCompilation CreateEmptyCompilation()
            {
                return CSharpCompilation.Create( "test_" + Guid.NewGuid() )
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
                mainRoslynCompilation = mainRoslynCompilation.AddSyntaxTrees( SyntaxFactory.ParseSyntaxTree( code ) );
            }

            if ( dependentCode != null )
            {
                var dependentCompilation = CreateEmptyCompilation().AddSyntaxTrees( SyntaxFactory.ParseSyntaxTree( dependentCode ) );
                mainRoslynCompilation = mainRoslynCompilation.AddReferences( dependentCompilation.ToMetadataReference() );
            }

            if ( !ignoreErrors )
            {
                CheckRoslynDiagnostics( mainRoslynCompilation );
            }

            return mainRoslynCompilation;
        }

        public static void CheckRoslynDiagnostics( CSharpCompilation mainRoslynCompilation )
        {
            var diagnostics = mainRoslynCompilation.GetDiagnostics();

            if ( diagnostics.Any( diag => diag.Severity >= DiagnosticSeverity.Error ) )
            {
                var lines = diagnostics.Select( diag => diag.ToString() ).Prepend( "The given code produced errors:" );

                throw new InvalidOperationException( string.Join( Environment.NewLine, lines ) );
            }
        }

        internal static CompilationModel CreateCompilation( string? code, string? dependentCode = null )
        {
            var roslynCompilation = CreateRoslynCompilation( code, dependentCode );

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
            if ( _doCodeExecutionTests )
            {
                var t = (T) ExecuteExpression( context, expression )!;
                withResult( t );
            }
        }
    }
}