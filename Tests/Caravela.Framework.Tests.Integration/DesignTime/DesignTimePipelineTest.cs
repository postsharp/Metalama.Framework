// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.DesignTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Tests.Integration.DesignTime
{
    public class DesignTimePipelineTest
    {
        public DesignTimePipelineTest( ITestOutputHelper logger )
        {
            this.Logger = logger;
        }

        protected ITestOutputHelper Logger { get; }

        private static CSharpCompilation CreateCSharpCompilation( IReadOnlyDictionary<string, string> code, string? assemblyName = null )
        {
            CSharpCompilation CreateEmptyCompilation()
            {
                return CSharpCompilation.Create( assemblyName ?? "test_" + Guid.NewGuid() )
                    .WithOptions( new CSharpCompilationOptions( OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true ) )
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

            Assert.Empty( compilation.GetDiagnostics().Where( d => d.Severity == DiagnosticSeverity.Error ) );

            return compilation;
        }

        private static void DumpSyntaxTreeResult( DesignTimeSyntaxTreeResult syntaxTreeResult, StringBuilder stringBuilder )
        {
            string? GetTextUnderDiagnostic( Diagnostic diagnostic )
            {
                var syntaxTree = diagnostic.Location!.SourceTree ?? syntaxTreeResult.SyntaxTree;

                return syntaxTree.GetText().GetSubText( diagnostic.Location.SourceSpan ).ToString();
            }

            stringBuilder.AppendLine( syntaxTreeResult.SyntaxTree.FilePath + ":" );

            // Diagnostics
            stringBuilder.AppendLine( $"{syntaxTreeResult.Diagnostics.Length} diagnostic(s):" );

            foreach ( var diagnostic in syntaxTreeResult.Diagnostics )
            {
                stringBuilder.AppendLine(
                    $"   {diagnostic.Severity} {diagnostic.Id} on `{GetTextUnderDiagnostic( diagnostic )}`: `{diagnostic.GetMessage()}`" );
            }

            // Suppressions
            stringBuilder.AppendLine( $"{syntaxTreeResult.Suppressions.Length} suppression(s):" );

            foreach ( var suppression in syntaxTreeResult.Suppressions )
            {
                stringBuilder.AppendLine( $"   {suppression.Definition.SuppressedDiagnosticId} on {suppression.SymbolId}" );
            }

            // Introductions

            stringBuilder.AppendLine( $"{syntaxTreeResult.Introductions.Length} introductions(s):" );

            foreach ( var introduction in syntaxTreeResult.Introductions )
            {
                stringBuilder.AppendLine( introduction.GeneratedSyntaxTree.ToString() );
            }
        }

        private static string DumpResults( DesignTimeResults results )
        {
            StringBuilder stringBuilder = new();

            for ( var i = 0; i < results.SyntaxTreeResults.Length; i++ )
            {
                if ( i > 0 )
                {
                    stringBuilder.AppendLine( "----------------------------------------------------------" );
                }

                var result = results.SyntaxTreeResults[i];
                DumpSyntaxTreeResult( result, stringBuilder );
            }

            return stringBuilder.ToString().Trim();
        }

        [Fact]
        public void NoCompileTimeCode()
        {
            using TestProjectOptions testProjectOptions = new();
            var compilation = CreateCSharpCompilation( new Dictionary<string, string>() { { "F1.cs", "public class X {}" } } );
            using DesignTimeAspectPipelineCache cache = new( new UnloadableCompileTimeDomain() );

            // First execution of the pipeline.
            var results = cache.GetDesignTimeResults( compilation, testProjectOptions );
            var dumpedResults = DumpResults( results );
            this.Logger.WriteLine( dumpedResults );

            var expectedResult = @"
F1.cs:
0 diagnostic(s):
0 suppression(s):
0 introductions(s):
";

            Assert.Equal( expectedResult.Trim(), dumpedResults );

            Assert.Equal( 1, cache.PipelineExecutionCount );

            // Second execution. The result should be the same, and the number of executions should not change.
            var results2 = cache.GetDesignTimeResults( compilation, testProjectOptions );
            var dumpedResults2 = DumpResults( results2 );
            Assert.Equal( expectedResult.Trim(), dumpedResults2 );
            Assert.Equal( 1, cache.PipelineExecutionCount );
        }

        [Fact]
        public void Caching()
        {
            var assemblyName = "test_" + Guid.NewGuid();

            var aspectCode = @"
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;

class MyAspect : System.Attribute, IAspect<IMethod>
{
   private static readonly DiagnosticDefinition<int> _description = new(""MY001"", Severity.Warning, ""My Message $version$,{0}"" );
   public int Version;

   public void Initialize( IAspectBuilder<IMethod> aspectBuilder )
   {
        aspectBuilder.Diagnostics.Report( _description, this.Version );
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
   Warning MY001 on `M`: `My Message $AspectVersion$,$TargetVersion$`
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

            using DesignTimeAspectPipelineCache cache = new( new UnloadableCompileTimeDomain() );
            var pipeline = cache.GetOrCreatePipeline( projectOptions );

            // First execution of the pipeline.
            var results = cache.GetDesignTimeResults( compilation, projectOptions );
            var dumpedResults = DumpResults( results );

            Assert.Equal( expectedResult.Replace( "$AspectVersion$", "1" ).Replace( "$TargetVersion$", "1" ).Trim(), dumpedResults );
            Assert.Equal( 1, cache.PipelineExecutionCount );

            // Second execution. The result should be the same, and the number of executions should not change.
            var results2 = cache.GetDesignTimeResults( compilation, projectOptions );
            var dumpedResults2 = DumpResults( results2 );
            Assert.Equal( expectedResult.Replace( "$AspectVersion$", "1" ).Replace( "$TargetVersion$", "1" ).Trim(), dumpedResults2 );
            Assert.Equal( 1, cache.PipelineExecutionCount );

            // Third execution, this time with modified target.
            var compilation3 = CreateCSharpCompilation(
                new Dictionary<string, string>()
                {
                    { "Aspect.cs", aspectCode.Replace( "$version$", "1" ) }, { "Target.cs", targetCode.Replace( "$version$", "2" ) }
                },
                assemblyName );

            var results3 = cache.GetDesignTimeResults( compilation3, projectOptions );
            var dumpedResults3 = DumpResults( results3 );

            this.Logger.WriteLine( dumpedResults3 );

            Assert.Equal( expectedResult.Replace( "$AspectVersion$", "1" ).Replace( "$TargetVersion$", "2" ).Trim(), dumpedResults3 );
            Assert.Equal( 2, cache.PipelineExecutionCount );

            Assert.Equal( 1, pipeline.PipelineInitializationCount );

            // Forth execution, with modified aspect but not target code. We don't trigger a build, so we should get the old result.
            var compilation4 = CreateCSharpCompilation(
                new Dictionary<string, string>()
                {
                    { "Aspect.cs", aspectCode.Replace( "$version$", "2" ) }, { "Target.cs", targetCode.Replace( "$version$", "2" ) }
                },
                assemblyName );

            var aspect4 = compilation4.SyntaxTrees.Single( t => t.FilePath == "Aspect.cs" );

            var results4 = cache.GetDesignTimeResults( compilation4, projectOptions );

            Assert.Equal( DesignTimeAspectPipelineStatus.NeedsExternalBuild, pipeline.Status );
            Assert.True( pipeline.IsCompileTimeSyntaxTreeOutdated( "Aspect.cs" ) );

            var dumpedResults4 = DumpResults( results4 );

            Assert.Equal( expectedResult.Replace( "$AspectVersion$", "1" ).Replace( "$TargetVersion$", "2" ).Trim(), dumpedResults4 );
            Assert.Equal( 2, cache.PipelineExecutionCount );
            Assert.Equal( 1, pipeline.PipelineInitializationCount );

            // There must be an error on the aspect.
            List<Diagnostic> diagnostics4 = new();

            new DesignTimeAnalyzerAdditionalVisitor( compilation4.GetSemanticModel( aspect4 ), diagnostics4.Add, pipeline, CancellationToken.None ).Visit(
                aspect4.GetRoot() );

            Assert.Contains(
                diagnostics4,
                d => d.Severity == DiagnosticSeverity.Error && d.Id == DesignTimeDiagnosticDescriptors.CompileTimeTypeNeedsRebuild.Id );

            // Fifth execution, the same scenario as before.
            var compilation5 = CreateCSharpCompilation(
                new Dictionary<string, string>()
                {
                    { "Aspect.cs", aspectCode.Replace( "$version$", "3" ) }, { "Target.cs", targetCode.Replace( "$version$", "2" ) }
                },
                assemblyName );

            var aspect5 = compilation5.SyntaxTrees.Single( t => t.FilePath == "Aspect.cs" );

            Assert.Equal( DesignTimeAspectPipelineStatus.NeedsExternalBuild, pipeline.Status );

            var results5 = cache.GetDesignTimeResults( compilation5, projectOptions );
            var dumpedResults5 = DumpResults( results5 );

            Assert.Equal( expectedResult.Replace( "$AspectVersion$", "1" ).Replace( "$TargetVersion$", "2" ).Trim(), dumpedResults5 );
            Assert.Equal( 2, cache.PipelineExecutionCount );
            Assert.Equal( 1, pipeline.PipelineInitializationCount );

            List<Diagnostic> diagnostics5 = new();

            new DesignTimeAnalyzerAdditionalVisitor( compilation5.GetSemanticModel( aspect5 ), diagnostics5.Add, pipeline, CancellationToken.None ).Visit(
                aspect5.GetRoot() );

            Assert.Contains(
                diagnostics5,
                d => d.Severity == DiagnosticSeverity.Error && d.Id == DesignTimeDiagnosticDescriptors.CompileTimeTypeNeedsRebuild.Id );

            // Build the project from the compile-time pipeline.
            using UnloadableCompileTimeDomain domain = new();
            var compileTimeAspectPipeline = new CompileTimeAspectPipeline( projectOptions, domain );
            DiagnosticList compileDiagnostics = new();
            Assert.True( compileTimeAspectPipeline.TryExecute( compileDiagnostics, compilation5, CancellationToken.None, out _, out _ ) );

            // Simulate an external build event. This is normally triggered by the build touch file.
            pipeline.OnExternalBuildStarted();

            // A new evaluation of the design-time pipeline should now give the new results.
            var results6 = cache.GetDesignTimeResults( compilation5, projectOptions );
            var dumpedResults6 = DumpResults( results6 );

            Assert.Equal( expectedResult.Replace( "$AspectVersion$", "3" ).Replace( "$TargetVersion$", "2" ).Trim(), dumpedResults6 );
            Assert.Equal( 3, cache.PipelineExecutionCount );
            Assert.Equal( 2, pipeline.PipelineInitializationCount );
            Assert.False( pipeline.IsCompileTimeSyntaxTreeOutdated( "Aspect.cs" ) );

            List<Diagnostic> diagnostics6 = new();

            new DesignTimeAnalyzerAdditionalVisitor( compilation5.GetSemanticModel( aspect5 ), diagnostics6.Add, pipeline, CancellationToken.None ).Visit(
                aspect5.GetRoot() );

            Assert.DoesNotContain(
                diagnostics6,
                d => d.Severity == DiagnosticSeverity.Error && d.Id == DesignTimeDiagnosticDescriptors.CompileTimeTypeNeedsRebuild.Id );
        }
    }
}