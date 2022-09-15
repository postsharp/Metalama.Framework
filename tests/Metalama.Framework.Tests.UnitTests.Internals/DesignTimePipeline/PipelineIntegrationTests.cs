// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Tests.UnitTests.DesignTime;
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

#pragma warning disable IDE0079   // Remove unnecessary suppression.
#pragma warning disable CA1307    // Specify StringComparison for clarity
#pragma warning disable VSTHRD200 // Warning VSTHRD200 : Use "Async" suffix in names of methods that return an awaitable type.

namespace Metalama.Framework.Tests.UnitTests.DesignTimePipeline
{
    public class PipelineIntegrationTests : TestBase
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

            compilation = compilation.AddSyntaxTrees(
                code.Select(
                    c => SyntaxFactory.ParseSyntaxTree( c.Value, path: c.Key, options: CSharpParseOptions.Default.WithPreprocessorSymbols( "METALAMA" ) ) ) );

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

            foreach ( var result in results.TransformationResult.SyntaxTreeResults.Values.OrderBy( t => t.SyntaxTree.FilePath ) )
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
            using var testContext = this.CreateTestContext();

            var compilation = CreateCSharpCompilation( new Dictionary<string, string>() { { "F1.cs", "public class X {}" } } );
            using TestDesignTimeAspectPipelineFactory factory = new( testContext );
            var pipeline = factory.CreatePipeline( compilation );

            // First execution of the pipeline.
            Assert.True( factory.TryExecute( testContext.ProjectOptions, compilation, CancellationToken.None, out var results ) );
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
            Assert.True( factory.TryExecute( testContext.ProjectOptions, compilation, CancellationToken.None, out var results2 ) );
            var dumpedResults2 = DumpResults( results2! );
            Assert.Equal( expectedResult.Trim(), dumpedResults2 );
            Assert.Equal( 1, pipeline.PipelineExecutionCount );
        }

        [Fact]
        public async Task ChangeInAspectCode()
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

            using var testContext = this.CreateTestContext();

            var compilation = CreateCSharpCompilation(
                new Dictionary<string, string>()
                {
                    { "Aspect.cs", aspectCode.Replace( "$version$", "1" ) }, { "Target.cs", targetCode.Replace( "$version$", "1" ) }
                },
                assemblyName );

            using TestDesignTimeAspectPipelineFactory factory = new( testContext );
            var pipeline = factory.CreatePipeline( compilation );

            // First execution of the pipeline.
            Assert.True( factory.TryExecute( testContext.ProjectOptions, compilation, CancellationToken.None, out var results ) );
            var dumpedResults = DumpResults( results! );

            Assert.Equal( expectedResult.Replace( "$AspectVersion$", "1" ).Replace( "$TargetVersion$", "1" ).Trim(), dumpedResults );
            Assert.Equal( 1, pipeline.PipelineExecutionCount );

            // Second execution with the same compilation. The result should be the same, and the number of executions should not change because the result is cached.
            Assert.True( factory.TryExecute( testContext.ProjectOptions, compilation, CancellationToken.None, out var results2 ) );
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

            Assert.True( factory.TryExecute( testContext.ProjectOptions, compilation3, CancellationToken.None, out var results3 ) );
            var dumpedResults3 = DumpResults( results3! );

            this.Logger.WriteLine( dumpedResults3 );

            Assert.Equal( 2, pipeline.PipelineExecutionCount );
            Assert.Equal( 1, pipeline.PipelineInitializationCount );
            Assert.Equal( expectedResult.Replace( "$AspectVersion$", "1" ).Replace( "$TargetVersion$", "2" ).Trim(), dumpedResults3 );

            // Forth execution, with modified aspect but not target code. This should pause the pipeline. We don't resume the pipeline, so we should get the old result.
            var compilation4 = CreateCSharpCompilation(
                new Dictionary<string, string>()
                {
                    { "Aspect.cs", aspectCode.Replace( "$version$", "2" ) }, { "Target.cs", targetCode.Replace( "$version$", "2" ) }
                },
                assemblyName );

            var aspect4 = compilation4.SyntaxTrees.Single( t => t.FilePath == "Aspect.cs" );

            Assert.True( factory.TryExecute( testContext.ProjectOptions, compilation4, CancellationToken.None, out var results4 ) );

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

            Assert.True( factory.TryExecute( testContext.ProjectOptions, compilation5, CancellationToken.None, out var results5 ) );
            var dumpedResults5 = DumpResults( results5! );

            Assert.Equal( expectedResult.Replace( "$AspectVersion$", "1" ).Replace( "$TargetVersion$", "2" ).Trim(), dumpedResults5 );
            Assert.Equal( 2, pipeline.PipelineExecutionCount );
            Assert.Equal( 1, pipeline.PipelineInitializationCount );

            List<Diagnostic> diagnostics5 = new();

            pipeline.ValidateTemplatingCode( compilation5.GetSemanticModel( aspect5 ), diagnostics5.Add );

            Assert.Contains(
                diagnostics5,
                d => d.Severity == DiagnosticSeverity.Error && d.Id == TemplatingDiagnosticDescriptors.CompileTimeTypeNeedsRebuild.Id );

            // Simulate an external build event. This is normally triggered by the build touch file or by a UI signal.
            await pipeline.ResumeAsync( false, CancellationToken.None );

            // A new evaluation of the design-time pipeline should now give the new results.
            Assert.True( factory.TryExecute( testContext.ProjectOptions, compilation5, CancellationToken.None, out var results6 ) );
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

            using var testContext = this.CreateTestContext();

            using TestDesignTimeAspectPipelineFactory factory = new( testContext );

            void TestWithTargetCode( string targetCode )
            {
                var compilation1 = CreateCSharpCompilation(
                    new Dictionary<string, string>() { { "Aspect.cs", aspectCode }, { "Target.cs", targetCode } },
                    assemblyName,
                    true );

                Assert.True( factory.TryExecute( testContext.ProjectOptions, compilation1, CancellationToken.None, out var results ) );

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

        [Fact]
        public void ProjectDependencyWithNoMetalamaReferenceButSystemCompileTimeType()
        {
            var context = this.CreateTestContext();

            using var pipelineFactory = new TestDesignTimeAspectPipelineFactory( context );

            // The dependency cannot have a reference to Metalama.
            // It needs to define a system type that is considered as compile-time.
            var dependency = CreateCSharpCompilation( "namespace System; struct Index {}", addMetalamaReferences: false );

            // The main compilation must have a compile-time syntax tree.
            var compilation = context.CreateCompilationModel(
                "using Metalama.Framework.Aspects; class A : TypeAspect {}",
                additionalReferences: new[] { dependency.ToMetadataReference() } );

            Assert.True( pipelineFactory.TryExecute( context.ProjectOptions, compilation.RoslynCompilation, CancellationToken.None, out _ ) );
        }

        [Fact]
        public void ChangeInDependency()
        {
            using var testContext = this.CreateTestContext();
            var observer = new TestDesignTimePipelineObserver();

            using TestDesignTimeAspectPipelineFactory factory = new( testContext, testContext.ServiceProvider.WithService( observer ) );

            var dependentCode = new Dictionary<string, string>()
            {
                ["dependent.cs"] = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Eligibility;
using System.Linq;
using System;

class MyAspect : MethodAspect
{
   private static readonly DiagnosticDefinition<string> _description = new(""MY001"", Severity.Warning, ""Fields='{0}'"" );
   
   public override void BuildAspect( IAspectBuilder<IMethod> aspectBuilder )
   {
        var allFields = string.Join( "","",  aspectBuilder.Target.DeclaringType.AllFields.Select( f => f.Name ) );
        aspectBuilder.Diagnostics.Report( _description.WithArguments( allFields ) );
   }
}

class C : BaseClass
{
   [MyAspect]
   void M() {}
}
"
            };

            // First compilation.
            var masterCode1 = new Dictionary<string, string>() { ["master.cs"] = @"public class BaseClass { public int Field1; }" };

            var masterCompilation1 = CreateCSharpCompilation( masterCode1, name: "Master" );

            var dependentCompilation1 = CreateCSharpCompilation(
                dependentCode,
                name: "Dependent",
                additionalReferences: new[] { masterCompilation1.ToMetadataReference() } );

            Assert.True( factory.TryExecute( testContext.ProjectOptions, dependentCompilation1, CancellationToken.None, out var results1 ) );

            Assert.Equal( 2, observer.InitializePipelineEvents.Count );
            Assert.Contains( "Master", observer.InitializePipelineEvents );
            Assert.Contains( "Dependent", observer.InitializePipelineEvents );
            observer.InitializePipelineEvents.Clear();

            Assert.Contains( "Fields='Field1'", results1!.TransformationResult.SyntaxTreeResults.Single().Value.Diagnostics.Single().GetMessage() );

            // Second compilation with a different master compilation.
            var masterCode2 = new Dictionary<string, string>() { ["master.cs"] = @"public partial class BaseClass { public int Field2; }" };

            var masterCompilation2 = CreateCSharpCompilation( masterCode2, name: "Master" );

            var dependentCompilation2 = CreateCSharpCompilation(
                dependentCode,
                name: "Dependent",
                additionalReferences: new[] { masterCompilation2.ToMetadataReference() } );

            Assert.True( factory.TryExecute( testContext.ProjectOptions, dependentCompilation2, CancellationToken.None, out var results2 ) );

            Assert.Empty( observer.InitializePipelineEvents );

            Assert.Contains( "Fields='Field2'", results2!.TransformationResult.SyntaxTreeResults.Single().Value.Diagnostics.Single().GetMessage() );

            // Third compilation. Add a syntax tree with a partial type.
            var masterCode3 = new Dictionary<string, string>()
            {
                ["master.cs"] = @"public partial class BaseClass { public int Field2; }", ["partial.cs"] = "partial class BaseClass { public int Field3; }"
            };

            var masterCompilation3 = CreateCSharpCompilation( masterCode3, name: "Master" );

            var dependentCompilation3 = CreateCSharpCompilation(
                dependentCode,
                name: "Dependent",
                additionalReferences: new[] { masterCompilation3.ToMetadataReference() } );

            observer.InitializePipelineEvents.Clear();

            Assert.True( factory.TryExecute( testContext.ProjectOptions, dependentCompilation3, CancellationToken.None, out var results3 ) );

            Assert.Empty( observer.InitializePipelineEvents );

            Assert.Contains( "Fields='Field2,Field3'", results3!.TransformationResult.SyntaxTreeResults.Single().Value.Diagnostics.Single().GetMessage() );
        }
    }
}