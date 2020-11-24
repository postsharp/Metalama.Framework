using System;
using System.Linq;
using System.Reflection;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.UnitTests
{
    public class TestBase
    {
        public static bool DoCodeExecutionTests = true;
            
        public static CSharpCompilation CreateRoslynCompilation( string code, bool ignoreErrors = false )
        {
            var roslynCompilation = CSharpCompilation.Create( null! )
                .WithOptions( new CSharpCompilationOptions( OutputKind.DynamicallyLinkedLibrary ) )
                .AddSyntaxTrees( SyntaxFactory.ParseSyntaxTree( code ) )
                .AddReferences( MetadataReference.CreateFromFile( typeof( object ).Assembly.Location ) );

            if ( !ignoreErrors )
            {
                var diagostics = roslynCompilation.GetDiagnostics();
                if ( diagostics.Any( diag => diag.Severity >= DiagnosticSeverity.Error ) )
                {
                    var lines = diagostics.Select( diag => diag.ToString() ).Prepend( "The given code produced errors:" );

                    throw new InvalidOperationException( string.Join( Environment.NewLine, lines ) );
                }
            }

            return roslynCompilation;
        }

        public static ICompilation CreateCompilation( string code )
        {
            var roslynCompilation = CreateRoslynCompilation( code );

            return CompilationFactory.CreateCompilation( roslynCompilation );
        }

        public static object? ExecuteExpression(string context, string expression)
        {
            string expressionContainer = $@"
class Expression
{{
    public static object Execute() => {expression};
}}";

            var assemblyPath = CaravelaCompiler.CompileAssembly( context, expressionContainer );

            var assembly = Assembly.LoadFile( assemblyPath );

            return assembly.GetType( "Expression" )!.GetMethod( "Execute" )!.Invoke( null, null );
        }
        
        public static void TestExpression<T>(string context, string expression, Action<T> withResult)
        {
            if ( DoCodeExecutionTests )
            {
                T t = (T) ExecuteExpression( context, expression )!;
                withResult( t );
            }
        }
    }
}
