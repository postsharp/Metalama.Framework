// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Pipeline.CompileTime;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Testing;
using Metalama.Framework.Engine.Utilities;
using Metalama.TestFramework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using StringExtensions = Metalama.TestFramework.Utilities.StringExtensions;

#pragma warning disable CA1307 // Specify StringComparison for clarity

namespace Metalama.Framework.Tests.UnitTests.DesignTime
{
    public class PipelineIntegrationTests
    {
        public PipelineIntegrationTests( ITestOutputHelper logger )
        {
            this.Logger = logger;
        }

        protected ITestOutputHelper Logger { get; }

        private static CSharpCompilation CreateCSharpCompilation(
            IReadOnlyDictionary<string, string> code,
            string? assemblyName = null,
            bool acceptErrors = false )
        {
            CSharpCompilation CreateEmptyCompilation()
            {
                return CSharpCompilation.Create( assemblyName ?? "test_" + RandomIdGenerator.GenerateId() )
                    .WithOptions(
                        new CSharpCompilationOptions(
                            OutputKind.DynamicallyLinkedLibrary,
                            allowUnsafe: true,
                            nullableContextOptions: NullableContextOptions.Enable ) )
                    .AddReferences(
                        new[] { "netstandard", "System.Runtime" }
                            .Select(
                                r => MetadataReference.CreateFromFile(
                                    Path.Combine( Path.GetDirectoryName( typeof(object).Assembly.Location )!, r + ".dll" ) ) ) )
                    .AddReferences(
                        MetadataReference.CreateFromFile( typeof(object).Assembly.Location ),
                        MetadataReference.CreateFromFile( typeof(DynamicAttribute).Assembly.Location ),
                        MetadataReference.CreateFromFile( typeof(CompileTimeAttribute).Assembly.Location ) );
            }

            var compilation = CreateEmptyCompilation();

            compilation = compilation.AddSyntaxTrees( code.Select( c => SyntaxFactory.ParseSyntaxTree( c.Value, path: c.Key ) ) );

            if ( !acceptErrors )
            {
                Assert.Empty( compilation.GetDiagnostics().Where( d => d.Severity == DiagnosticSeverity.Error ) );
            }

            return compilation;
        }

        private static void DumpSyntaxTreeResult( SyntaxTreePipelineResult syntaxTreeResult, StringBuilder stringBuilder )
        {
            string? GetTextUnderDiagnostic( Diagnostic diagnostic )
            {
                var syntaxTree = diagnostic.Location.SourceTree ?? syntaxTreeResult.SyntaxTree;

                return syntaxTree.GetText().GetSubText( diagnostic.Location.SourceSpan ).ToString();
            }

            stringBuilder.AppendLine( syntaxTreeResult.SyntaxTree.FilePath + ":" );

            // Diagnostics
            StringExtensions.AppendLineInvariant( stringBuilder, $"{syntaxTreeResult.Diagnostics.Length} diagnostic(s):" );

            foreach ( var diagnostic in syntaxTreeResult.Diagnostics )
            {
                StringExtensions.AppendLineInvariant(
                    stringBuilder,
                    $"   {diagnostic.Severity} {diagnostic.Id} on `{GetTextUnderDiagnostic( diagnostic )}`: `{diagnostic.GetMessage()}`" );
            }

            // Suppressions
            StringExtensions.AppendLineInvariant( stringBuilder, $"{syntaxTreeResult.Suppressions.Length} suppression(s):" );

            foreach ( var suppression in syntaxTreeResult.Suppressions )
            {
                StringExtensions.AppendLineInvariant( stringBuilder, $"   {suppression.Definition.SuppressedDiagnosticId} on {suppression.SymbolId}" );
            }

            // Introductions

            StringExtensions.AppendLineInvariant( stringBuilder, $"{syntaxTreeResult.Introductions.Length} introductions(s):" );

            foreach ( var introduction in syntaxTreeResult.Introductions )
            {
                stringBuilder.AppendLine( introduction.GeneratedSyntaxTree.ToString() );
            }
        }

        private static string DumpResults( CompilationResult results )
        {
            StringBuilder stringBuilder = new();

            var i = 0;

            foreach ( var result in results.PipelineResult.SyntaxTreeResults.Values.OrderBy( t => t.SyntaxTree.FilePath ) )
            {
                if ( i > 0 )
                {
                    stringBuilder.AppendLine( "----------------------------------------------------------" );
                }

                i++;

                DumpSyntaxTreeResult( result, stringBuilder );
            }

            return stringBuilder.ToString().Trim();
        }

        [Fact]
        public void NoCompileTimeCode()
        {
            using TestProjectOptions testProjectOptions = new();
            var compilation = CreateCSharpCompilation( new Dictionary<string, string>() { { "F1.cs", "public class X {}" } } );
            using TestDesignTimeAspectPipelineFactory factory = new( new UnloadableCompileTimeDomain(), testProjectOptions );
            var pipeline = factory.GetOrCreatePipeline( testProjectOptions, compilation, CancellationToken.None )!;

            // First execution of the pipeline.
            Assert.True( factory.TryExecute( testProjectOptions, compilation, CancellationToken.None, out var results ) );
            var dumpedResults = DumpResults( results! );
            this.Logger.WriteLine( dumpedResults );

            var expectedResult = @"
F1.cs:
0 diagnostic(s):
0 suppression(s):
0 introductions(s):
";

            Assert.Equal( expectedResult.Trim(), dumpedResults );

            Assert.Equal( 1, pipeline.PipelineExecutionCount );

            // Second execution. The result should be the same, and the number of executions should not change.
            Assert.True( factory.TryExecute( testProjectOptions, compilation, CancellationToken.None, out var results2 ) );
            var dumpedResults2 = DumpResults( results2! );
            Assert.Equal( expectedResult.Trim(), dumpedResults2 );
            Assert.Equal( 1, pipeline.PipelineExecutionCount );
        }

        [Fact]
        public async Task ChangeInAspectCodeAsync()
        {
            var assemblyName = "test_" + RandomIdGenerator.GenerateId();

            var aspectCode = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Eligibility;

class MyAspect : MethodAspect
{
   private static readonly DiagnosticDefinition<int> _description = new(""MY001"", Severity.Warning, ""AspectVersion=$version$,TargetVersion={0}"" );
   public int Version;

   public override void BuildAspect( IAspectBuilder<IMethod> aspectBuilder )
   {
aspectBuilder.Diagnostics.Report(         _description.WithArguments( this.Version ) );
   }
}
";

            var targetCode = @"
class C
{
   [MyAspect(Version=$version$)]
   void M() {}
}
";

            var expectedResult = @"
Aspect.cs:
0 diagnostic(s):
0 suppression(s):
0 introductions(s):
----------------------------------------------------------
Target.cs:
1 diagnostic(s):
   Warning MY001 on `M`: `AspectVersion=$AspectVersion$,TargetVersion=$TargetVersion$`
0 suppression(s):
0 introductions(s):
";

            using TestProjectOptions projectOptions = new();

            var compilation = CreateCSharpCompilation(
                new Dictionary<string, string>()
                {
                    { "Aspect.cs", aspectCode.Replace( "$version$", "1" ) }, { "Target.cs", targetCode.Replace( "$version$", "1" ) }
                },
                assemblyName );

            using TestDesignTimeAspectPipelineFactory factory = new( new UnloadableCompileTimeDomain(), projectOptions );
            var pipeline = factory.GetOrCreatePipeline( projectOptions, compilation, CancellationToken.None )!;

            // First execution of the pipeline.
            Assert.True( factory.TryExecute( projectOptions, compilation, CancellationToken.None, out var results ) );
            var dumpedResults = DumpResults( results! );

            Assert.Equal( expectedResult.Replace( "$AspectVersion$", "1" ).Replace( "$TargetVersion$", "1" ).Trim(), dumpedResults );
            Assert.Equal( 1, pipeline.PipelineExecutionCount );

            // Second execution with the same compilation. The result should be the same, and the number of executions should not change because the result is cached.
            Assert.True( factory.TryExecute( projectOptions, compilation, CancellationToken.None, out var results2 ) );
            var dumpedResults2 = DumpResults( results2! );
            Assert.Equal( expectedResult.Replace( "$AspectVersion$", "1" ).Replace( "$TargetVersion$", "1" ).Trim(), dumpedResults2 );
            Assert.Equal( 1, pipeline.PipelineExecutionCount );

            // Third execution, this time with modified target but same aspect code.
            var compilation3 = CreateCSharpCompilation(
                new Dictionary<string, string>()
                {
                    { "Aspect.cs", aspectCode.Replace( "$version$", "1" ) }, { "Target.cs", targetCode.Replace( "$version$", "2" ) }
                },
                assemblyName );

            Assert.True( factory.TryExecute( projectOptions, compilation3, CancellationToken.None, out var results3 ) );
            var dumpedResults3 = DumpResults( results3! );

            this.Logger.WriteLine( dumpedResults3 );

            Assert.Equal( 2, pipeline.PipelineExecutionCount );
            Assert.Equal( 1, pipeline.PipelineInitializationCount );
            Assert.Equal( expectedResult.Replace( "$AspectVersion$", "1" ).Replace( "$TargetVersion$", "2" ).Trim(), dumpedResults3 );

            // Forth execution, with modified aspect but not target code. We don't trigger a build, so we should get the old result.
            var compilation4 = CreateCSharpCompilation(
                new Dictionary<string, string>()
                {
                    { "Aspect.cs", aspectCode.Replace( "$version$", "2" ) }, { "Target.cs", targetCode.Replace( "$version$", "2" ) }
                },
                assemblyName );

            var aspect4 = compilation4.SyntaxTrees.Single( t => t.FilePath == "Aspect.cs" );

            Assert.True( factory.TryExecute( projectOptions, compilation4, CancellationToken.None, out var results4 ) );

            Assert.Equal( DesignTimeAspectPipelineStatus.Paused, pipeline.Status );
            Assert.True( pipeline.IsCompileTimeSyntaxTreeOutdated( "Aspect.cs" ) );

            var dumpedResults4 = DumpResults( results4! );

            Assert.Equal( expectedResult.Replace( "$AspectVersion$", "1" ).Replace( "$TargetVersion$", "2" ).Trim(), dumpedResults4 );
            Assert.Equal( 2, pipeline.PipelineExecutionCount );
            Assert.Equal( 1, pipeline.PipelineInitializationCount );

            // There must be an error on the aspect.
            List<Diagnostic> diagnostics4 = new();

            pipeline.ValidateTemplatingCode( compilation4.GetSemanticModel( aspect4 ), diagnostics4.Add );

            Assert.Contains(
                diagnostics4,
                d => d.Severity == DiagnosticSeverity.Error && d.Id == TemplatingDiagnosticDescriptors.CompileTimeTypeNeedsRebuild.Id );

            // Fifth execution, the same scenario as before.
            var compilation5 = CreateCSharpCompilation(
                new Dictionary<string, string>()
                {
                    { "Aspect.cs", aspectCode.Replace( "$version$", "3" ) }, { "Target.cs", targetCode.Replace( "$version$", "2" ) }
                },
                assemblyName );

            var aspect5 = compilation5.SyntaxTrees.Single( t => t.FilePath == "Aspect.cs" );

            Assert.Equal( DesignTimeAspectPipelineStatus.Paused, pipeline.Status );

            Assert.True( factory.TryExecute( projectOptions, compilation5, CancellationToken.None, out var results5 ) );
            var dumpedResults5 = DumpResults( results5! );

            Assert.Equal( expectedResult.Replace( "$AspectVersion$", "1" ).Replace( "$TargetVersion$", "2" ).Trim(), dumpedResults5 );
            Assert.Equal( 2, pipeline.PipelineExecutionCount );
            Assert.Equal( 1, pipeline.PipelineInitializationCount );

            List<Diagnostic> diagnostics5 = new();

            pipeline.ValidateTemplatingCode( compilation5.GetSemanticModel( aspect5 ), diagnostics5.Add );

            Assert.Contains(
                diagnostics5,
                d => d.Severity == DiagnosticSeverity.Error && d.Id == TemplatingDiagnosticDescriptors.CompileTimeTypeNeedsRebuild.Id );

            // Build the project from the compile-time pipeline.
            using UnloadableCompileTimeDomain domain = new();

            var serviceProvider = ServiceProviderFactory.GetServiceProvider( projectOptions.PathOptions )
                .WithService( projectOptions )
                .WithProjectScopedServices( compilation );

            var compileTimeAspectPipeline = new CompileTimeAspectPipeline( serviceProvider, true, domain );
            DiagnosticList compileDiagnostics = new();
            var pipelineResult = await compileTimeAspectPipeline.ExecuteAsync( compileDiagnostics, compilation5, default, CancellationToken.None );

            Assert.NotNull( pipelineResult );

            // Simulate an external build event. This is normally triggered by the build touch file or by a UI signal.
            pipeline.Resume( false );

            // A new evaluation of the design-time pipeline should now give the new results.
            Assert.True( factory.TryExecute( projectOptions, compilation5, CancellationToken.None, out var results6 ) );
            var dumpedResults6 = DumpResults( results6! );

            Assert.Equal( expectedResult.Replace( "$AspectVersion$", "3" ).Replace( "$TargetVersion$", "2" ).Trim(), dumpedResults6 );
            Assert.Equal( 3, pipeline.PipelineExecutionCount );
            Assert.Equal( 2, pipeline.PipelineInitializationCount );
            Assert.False( pipeline.IsCompileTimeSyntaxTreeOutdated( "Aspect.cs" ) );

            List<Diagnostic> diagnostics6 = new();

            pipeline.ValidateTemplatingCode( compilation5.GetSemanticModel( aspect5 ), diagnostics6.Add );

            Assert.DoesNotContain(
                diagnostics6,
                d => d.Severity == DiagnosticSeverity.Error && d.Id == TemplatingDiagnosticDescriptors.CompileTimeTypeNeedsRebuild.Id );
        }

        [Fact]
        public void ChangeInTargetCode()
        {
            var assemblyName = "test_" + RandomIdGenerator.GenerateId();

            var aspectCode = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Eligibility;

class MyAspect : TypeAspect
{
   [Introduce]
   public void NewProperty() { }
}
";

            var expectedResult = @"
Aspect.cs:
0 diagnostic(s):
0 suppression(s):
0 introductions(s):
----------------------------------------------------------
Target.cs:
0 diagnostic(s):
0 suppression(s):
1 introductions(s):
/// <generated>
/// Generated by Metalama to support the code editing experience. This is NOT the code that gets executed.
/// </generated>
partial class C
{
    public void NewProperty()
    {
    }
}
";

            using TestProjectOptions projectOptions = new();

            using TestDesignTimeAspectPipelineFactory factory = new( new UnloadableCompileTimeDomain(), projectOptions );

            void TestWithTargetCode( string targetCode )
            {
                var compilation1 = CreateCSharpCompilation(
                    new Dictionary<string, string>() { { "Aspect.cs", aspectCode }, { "Target.cs", targetCode } },
                    assemblyName,
                    true );

                Assert.True( factory.TryExecute( projectOptions, compilation1, CancellationToken.None, out var results ) );

                var dumpedResults = DumpResults( results! );

                this.Logger.WriteLine( "-----------------" );
                this.Logger.WriteLine( dumpedResults );

                Assert.Equal( expectedResult.Trim().Replace( "\r\n", "\n" ), dumpedResults.Trim().Replace( "\r\n", "\n" ) );
            }

            TestWithTargetCode( "[MyAspect] partial class C { }" );
            TestWithTargetCode( "[MyAspect] partial class C { void }" );
            TestWithTargetCode( "[MyAspect] partial class C { void NewMethod() {} }" );
            TestWithTargetCode( "[MyAspect] partial class C { void NewMethod() { ; } }" );
        }
    }
}