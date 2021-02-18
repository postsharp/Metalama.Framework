using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Caravela.Framework.Impl.CodeModel.Symbolic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.UnitTests
{
    public class TestBase
    {
        /// <summary>
        /// Flag that determines whether tests that test the serialization of reflection objects like <see cref="Type"/> should use "dotnet build" to see if the
        /// resulting syntax tree actually compiles and results in valid IL. This is slow but neccessary during development, at least, since an incorrect syntax tree
        /// can easily be produced.
        /// </summary>
        private const bool _doCodeExecutionTests = true;

        public static CSharpCompilation CreateRoslynCompilation( string? code, bool ignoreErrors = false )
        {
            var roslynCompilation = CSharpCompilation.Create( null! )
                .WithOptions( new CSharpCompilationOptions( OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true ) )
                .AddReferences(
                    new[] { "netstandard", "System.Runtime" }
                        .Select( r => MetadataReference.CreateFromFile(
                             Path.Combine( Path.GetDirectoryName( typeof( object ).Assembly.Location )!, r + ".dll" ) ) ) )
                .AddReferences(
                    MetadataReference.CreateFromFile( typeof( object ).Assembly.Location ),
                    MetadataReference.CreateFromFile( typeof( DynamicAttribute ).Assembly.Location ),
                    MetadataReference.CreateFromFile( typeof( Project.CompileTimeAttribute ).Assembly.Location ) );

            if ( code != null )
            {
                roslynCompilation = roslynCompilation.AddSyntaxTrees( SyntaxFactory.ParseSyntaxTree( code ) );
            }

            if ( !ignoreErrors )
            {
                var diagnostics = roslynCompilation.GetDiagnostics();
                if ( diagnostics.Any( diag => diag.Severity >= DiagnosticSeverity.Error ) )
                {
                    var lines = diagnostics.Select( diag => diag.ToString() ).Prepend( "The given code produced errors:" );

                    throw new InvalidOperationException( string.Join( Environment.NewLine, lines ) );
                }
            }

            return roslynCompilation;
        }

        internal static CompilationModel CreateCompilation( string? code )
        {
            var roslynCompilation = CreateRoslynCompilation( code );

            return new CompilationModel( roslynCompilation );
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
