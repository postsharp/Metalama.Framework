// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.RunTime;
using Metalama.Framework.Tests.UnitTests.DesignTime.Mocks;
using Metalama.Testing.UnitTesting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

#pragma warning disable IDE0079   // Remove unnecessary suppression.
#pragma warning disable CA1307    // Specify StringComparison for clarity
#pragma warning disable VSTHRD200 // Warning VSTHRD200 : Use "Async" suffix in names of methods that return an awaitable type.

namespace Metalama.Framework.Tests.UnitTests.DesignTime.Pipeline;

public sealed class DesignTimePipelineTests : UnitTestClass
{
    public DesignTimePipelineTests( ITestOutputHelper logger ) : base( logger ) { }

    private static CSharpCompilation CreateCSharpCompilation(
        IReadOnlyDictionary<string, string> code,
        string? assemblyName = null,
        bool acceptErrors = false,
        IEnumerable<MetadataReference>? additionalReferences = null )
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
                        .SelectAsImmutableArray(
                            r => (MetadataReference) MetadataReference.CreateFromFile(
                                Path.Combine( Path.GetDirectoryName( typeof(object).Assembly.Location )!, r + ".dll" ) ) ) )
                .AddReferences(
                    MetadataReference.CreateFromFile( typeof(object).Assembly.Location ),
                    MetadataReference.CreateFromFile( typeof(DynamicAttribute).Assembly.Location ),
                    MetadataReference.CreateFromFile( typeof(Enumerable).Assembly.Location ),
                    MetadataReference.CreateFromFile( typeof(CompileTimeAttribute).Assembly.Location ),
                    MetadataReference.CreateFromFile( typeof(FieldOrPropertyInfo).Assembly.Location ) )
                .AddReferences( additionalReferences ?? Enumerable.Empty<MetadataReference>() );
        }

        var compilation = CreateEmptyCompilation();

        compilation = compilation.AddSyntaxTrees(
            code.SelectAsArray(
                c => SyntaxFactory.ParseSyntaxTree(
                    c.Value,
                    path: c.Key,
                    options: SupportedCSharpVersions.DefaultParseOptions.WithPreprocessorSymbols( "METALAMA" ) ) ) );

        if ( !acceptErrors )
        {
            Assert.Empty( compilation.GetDiagnostics().Where( d => d.Severity == DiagnosticSeverity.Error ) );
        }

        return compilation;
    }

    private static void DumpSyntaxTreeResult( SyntaxTree? syntaxTree, SyntaxTreePipelineResult syntaxTreeResult, StringBuilder stringBuilder )
    {
        string GetTextUnderDiagnostic( Diagnostic diagnostic )
        {
            var syntaxTreeOfDiagnostic = diagnostic.Location.SourceTree ?? syntaxTree;

            return syntaxTreeOfDiagnostic?.GetText().GetSubText( diagnostic.Location.SourceSpan ).ToString() ?? "";
        }

        stringBuilder.AppendLine( syntaxTreeResult.SyntaxTreePath + ":" );

        // Diagnostics
        stringBuilder.AppendLineInvariant( $"{syntaxTreeResult.Diagnostics.Length} diagnostic(s):" );

        foreach ( var diagnostic in syntaxTreeResult.Diagnostics )
        {
            stringBuilder.AppendLineInvariant(
                $"   {diagnostic.Severity} {diagnostic.Id} on `{GetTextUnderDiagnostic( diagnostic )}`: `{diagnostic.GetMessage( CultureInfo.CurrentCulture )}`" );
        }

        // Suppressions
        stringBuilder.AppendLineInvariant( $"{syntaxTreeResult.Suppressions.Length} suppression(s):" );

        foreach ( var suppression in syntaxTreeResult.Suppressions )
        {
            stringBuilder.AppendLineInvariant( $"   {suppression.Definition.SuppressedDiagnosticId} on {suppression.DeclarationId}" );
        }

        // Introductions
        stringBuilder.AppendLineInvariant( $"{syntaxTreeResult.Introductions.Length} introductions(s):" );

        foreach ( var introduction in syntaxTreeResult.Introductions.OrderBy( i => i.Name ) )
        {
            stringBuilder.AppendLine( introduction.GeneratedSyntaxTree.ToString() );
        }
    }

    private static string DumpResults( AspectPipelineResultAndState results )
    {
        StringBuilder stringBuilder = new();

        var i = 0;

        foreach ( var syntaxTreeResult in results.Result.SyntaxTreeResults.Values.OrderBy( t => t.SyntaxTreePath ) )
        {
            if ( i > 0 )
            {
                stringBuilder.AppendLine( "----------------------------------------------------------" );
            }

            i++;

            var syntaxTree = syntaxTreeResult.SyntaxTreePath != null ? results.ProjectVersion.SyntaxTrees[syntaxTreeResult.SyntaxTreePath].SyntaxTree : null;

            DumpSyntaxTreeResult( syntaxTree, syntaxTreeResult, stringBuilder );
        }

        return stringBuilder.ToString().Trim();
    }

    [Fact]
    public void InitializationWithoutAspect()
    {
        using var testContext = this.CreateTestContext();

        var code = new Dictionary<string, string> { ["Class1.cs"] = "public class Class1 { }", ["Class2.cs"] = "public class Class2 { }" };

        var compilation = CreateCSharpCompilation( code );

        using var pipelineFactory = new TestDesignTimeAspectPipelineFactory( testContext );
        var pipeline = pipelineFactory.CreatePipeline( compilation );
        Assert.True( pipeline.TryExecute( compilation, default, out _ ) );
    }

    [Fact]
    public void InitializationWithAspect()
    {
        using var testContext = this.CreateTestContext();

        // Test that we can initialize the pipeline with a different compilation than the one with which we execute it.

        var code = new Dictionary<string, string>
        {
            ["Aspect.cs"] =
                "public class Aspect : Metalama.Framework.Aspects.OverrideMethodAspect { public override dynamic OverrideMethod() { return null; } }",
            ["Class1.cs"] = "public class Class1 { }",
            ["Class2.cs"] = "public class Class2 { [Aspect]  void Method() {} }"
        };

        var compilation = CreateCSharpCompilation( code );

        using var pipelineFactory = new TestDesignTimeAspectPipelineFactory( testContext );
        var pipeline = pipelineFactory.CreatePipeline( compilation );
        Assert.True( pipeline.TryExecute( compilation, default, out _ ) );
    }

    [Fact]
    public void NoCompileTimeCode()
    {
        using var testContext = this.CreateTestContext();

        var compilation = CreateCSharpCompilation( new Dictionary<string, string>() { { "F1.cs", "public class X {}" } } );
        using TestDesignTimeAspectPipelineFactory factory = new( testContext );
        var pipeline = factory.CreatePipeline( compilation );

        // First execution of the pipeline.
        Assert.True( factory.TryExecute( testContext.ProjectOptions, compilation, default, out var results ) );
        var dumpedResults = DumpResults( results );
        this.TestOutput.WriteLine( dumpedResults );

        const string expectedResult = @"
F1.cs:
0 diagnostic(s):
0 suppression(s):
0 introductions(s):
";

        Assert.Equal( expectedResult.Trim(), dumpedResults );

        Assert.Equal( 1, pipeline.PipelineExecutionCount );

        // Second execution. The result should be the same, and the number of executions should not change.
        Assert.True( factory.TryExecute( testContext.ProjectOptions, compilation, default, out var results2 ) );
        var dumpedResults2 = DumpResults( results2 );
        Assert.Equal( expectedResult.Trim(), dumpedResults2 );
        Assert.Equal( 1, pipeline.PipelineExecutionCount );
    }

    [Fact]
    public void ErrorInCompileTimeCode()
    {
        using var testContext = this.CreateTestContext();

        const string code = """
                            using Metalama.Framework.Aspects;

                            public class Aspect : OverrideMethodAspect
                            {
                                public override dynamic? OverrideMethod()
                                {
                                    dynamic[] x; // This should cause LAMA0227
                                    return null;
                                }
                            }

                            """;

        var compilation = CreateCSharpCompilation( new Dictionary<string, string>() { { "F1.cs", code } } );

        using TestDesignTimeAspectPipelineFactory factory = new( testContext );
        var pipeline = factory.CreatePipeline( compilation );

        // First execution of the pipeline.
        Assert.False( factory.TryExecute( testContext.ProjectOptions, compilation, default, out _, out var diagnostics ) );
        Assert.Equal( 1, pipeline.PipelineExecutionCount );
        Assert.Single( diagnostics.Where( d => d.Id == TemplatingDiagnosticDescriptors.InvalidDynamicTypeConstruction.Id ) );

        // Second execution. The result should be the same, and the number of executions should not change.
        Assert.False( factory.TryExecute( testContext.ProjectOptions, compilation, default, out _, out var diagnostics2 ) );
        Assert.Equal( 1, pipeline.PipelineExecutionCount );
        Assert.Single( diagnostics2.Where( d => d.Id == TemplatingDiagnosticDescriptors.InvalidDynamicTypeConstruction.Id ) );
    }

    [Fact]
    public async Task ChangeInAspectCode()
    {
        var assemblyName = "test_" + RandomIdGenerator.GenerateId();

        const string aspectCode = @"
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
aspectBuilder.Diagnostics.Report( _description.WithArguments( this.Version ) );
   }
}
";

        const string targetCode = @"
class C
{
   [MyAspect(Version=$version$)]
   void M() {}
}
";

        const string expectedResult = @"
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

        var compilation = TestCompilationFactory.CreateCSharpCompilation(
            new Dictionary<string, string>()
            {
                { "Aspect.cs", aspectCode.Replace( "$version$", "1" ) }, { "Target.cs", targetCode.Replace( "$version$", "1" ) }
            },
            name: assemblyName );

        using TestDesignTimeAspectPipelineFactory factory = new( testContext );
        var pipeline = factory.CreatePipeline( compilation );

        // First execution of the pipeline.
        Assert.True( factory.TryExecute( testContext.ProjectOptions, compilation, default, out var results ) );
        var dumpedResults = DumpResults( results );

        Assert.Equal( expectedResult.Replace( "$AspectVersion$", "1" ).Replace( "$TargetVersion$", "1" ).Trim(), dumpedResults );
        Assert.Equal( 1, pipeline.PipelineExecutionCount );
        Assert.Equal( 1, pipeline.PipelineInitializationCount );

        // Second execution with the same compilation. The result should be the same, and the number of executions should not change because the result is cached.
        Assert.True( factory.TryExecute( testContext.ProjectOptions, compilation, default, out var results2 ) );
        var dumpedResults2 = DumpResults( results2 );
        Assert.Equal( expectedResult.Replace( "$AspectVersion$", "1" ).Replace( "$TargetVersion$", "1" ).Trim(), dumpedResults2 );
        Assert.Equal( 1, pipeline.PipelineExecutionCount );
        Assert.Equal( 1, pipeline.PipelineInitializationCount );

        // Third execution, this time with modified target but same aspect code.
        var compilation3 = TestCompilationFactory.CreateCSharpCompilation(
            new Dictionary<string, string>()
            {
                { "Aspect.cs", aspectCode.Replace( "$version$", "1" ) }, { "Target.cs", targetCode.Replace( "$version$", "2" ) }
            },
            name: assemblyName );

        Assert.True( factory.TryExecute( testContext.ProjectOptions, compilation3, default, out var results3 ) );
        var dumpedResults3 = DumpResults( results3 );

        this.TestOutput.WriteLine( dumpedResults3 );

        Assert.Equal( 2, pipeline.PipelineExecutionCount );
        Assert.Equal( 1, pipeline.PipelineInitializationCount );
        Assert.Equal( expectedResult.Replace( "$AspectVersion$", "1" ).Replace( "$TargetVersion$", "2" ).Trim(), dumpedResults3 );

        // Forth execution, with modified aspect but not target code. This should pause the pipeline. We don't resume the pipeline, so we should get the old result.
        var compilation4 = TestCompilationFactory.CreateCSharpCompilation(
            new Dictionary<string, string>()
            {
                { "Aspect.cs", aspectCode.Replace( "$version$", "2" ) }, { "Target.cs", targetCode.Replace( "$version$", "2" ) }
            },
            name: assemblyName );

        var aspect4 = compilation4.SyntaxTrees.Single( t => t.FilePath == "Aspect.cs" );

        Assert.True( factory.TryExecute( testContext.ProjectOptions, compilation4, default, out var results4 ) );

        Assert.Equal( DesignTimeAspectPipelineStatus.Paused, pipeline.Status );
        Assert.True( factory.EventHub.IsEditingCompileTimeCode );
        Assert.True( pipeline.IsCompileTimeSyntaxTreeOutdated( "Aspect.cs" ) );

        var dumpedResults4 = DumpResults( results4 );

        Assert.Equal( expectedResult.Replace( "$AspectVersion$", "1" ).Replace( "$TargetVersion$", "2" ).Trim(), dumpedResults4 );
        Assert.Equal( 2, pipeline.PipelineExecutionCount );
        Assert.Equal( 1, pipeline.PipelineInitializationCount );

        // There must be an error on the aspect.
        List<Diagnostic> diagnostics4 = new();

        pipeline.ValidateTemplatingCode( compilation4.GetSemanticModel( aspect4 ), diagnostics4.Add );

        Assert.Contains(
            diagnostics4,
            d => d.Id == TemplatingDiagnosticDescriptors.CompileTimeTypeNeedsRebuild.Id );

        // Fifth execution, the same scenario as before.
        var compilation5 = TestCompilationFactory.CreateCSharpCompilation(
            new Dictionary<string, string>()
            {
                { "Aspect.cs", aspectCode.Replace( "$version$", "3" ) }, { "Target.cs", targetCode.Replace( "$version$", "2" ) }
            },
            name: assemblyName );

        var aspect5 = compilation5.SyntaxTrees.Single( t => t.FilePath == "Aspect.cs" );

        Assert.Equal( DesignTimeAspectPipelineStatus.Paused, pipeline.Status );

        Assert.True( factory.TryExecute( testContext.ProjectOptions, compilation5, default, out var results5 ) );
        var dumpedResults5 = DumpResults( results5 );

        Assert.Equal( expectedResult.Replace( "$AspectVersion$", "1" ).Replace( "$TargetVersion$", "2" ).Trim(), dumpedResults5 );
        Assert.Equal( 2, pipeline.PipelineExecutionCount );
        Assert.Equal( 1, pipeline.PipelineInitializationCount );

        List<Diagnostic> diagnostics5 = new();

        pipeline.ValidateTemplatingCode( compilation5.GetSemanticModel( aspect5 ), diagnostics5.Add );

        Assert.Contains(
            diagnostics5,
            d => d.Id == TemplatingDiagnosticDescriptors.CompileTimeTypeNeedsRebuild.Id );

        // Simulate an external build event. This is normally triggered by the build touch file or by a UI signal.
        await pipeline.ResumeAsync( AsyncExecutionContext.Get(), false );
        Assert.False( factory.EventHub.IsEditingCompileTimeCode );

        // A new evaluation of the design-time pipeline should now give the new results.
        Assert.True( factory.TryExecute( testContext.ProjectOptions, compilation5, default, out var results6 ) );
        var dumpedResults6 = DumpResults( results6 );

        Assert.Equal( expectedResult.Replace( "$AspectVersion$", "3" ).Replace( "$TargetVersion$", "2" ).Trim(), dumpedResults6 );
        Assert.Equal( 3, pipeline.PipelineExecutionCount );
        Assert.Equal( 2, pipeline.PipelineInitializationCount );
        Assert.False( pipeline.IsCompileTimeSyntaxTreeOutdated( "Aspect.cs" ) );

        List<Diagnostic> diagnostics6 = new();

        pipeline.ValidateTemplatingCode( compilation5.GetSemanticModel( aspect5 ), diagnostics6.Add );

        Assert.DoesNotContain(
            diagnostics6,
            d => d.Id == TemplatingDiagnosticDescriptors.CompileTimeTypeNeedsRebuild.Id );
    }

    [Fact]
    public async Task ChangeInAspectCodeSeparateProject()
    {
        var aspectAssemblyName = "aspect_" + RandomIdGenerator.GenerateId();
        var targetAssemblyName = "target_" + RandomIdGenerator.GenerateId();

        const string aspectCode = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Eligibility;

public class MyAspect : MethodAspect
{
   private static readonly DiagnosticDefinition<int> _description = new(""MY001"", Severity.Warning, ""AspectVersion=$version$,TargetVersion={0}"" );
   public int Version;

   public override void BuildAspect( IAspectBuilder<IMethod> aspectBuilder )
   {
       aspectBuilder.Diagnostics.Report( _description.WithArguments( this.Version ) );
   }
}
";

        const string targetCode = @"
class C
{
   [MyAspect(Version=$version$)]
   void M() {}
}
";

        const string expectedResult = @"
Target.cs:
1 diagnostic(s):
   Warning MY001 on `M`: `AspectVersion=$AspectVersion$,TargetVersion=$TargetVersion$`
0 suppression(s):
0 introductions(s):
";

        using var testContext = this.CreateTestContext();

        var aspectCompilation = TestCompilationFactory.CreateCSharpCompilation(
            new Dictionary<string, string>() { { "Aspect.cs", aspectCode.Replace( "$version$", "1" ) } },
            name: aspectAssemblyName );

        var targetCompilation = TestCompilationFactory.CreateCSharpCompilation(
            new Dictionary<string, string>() { { "Target.cs", targetCode.Replace( "$version$", "1" ) } },
            name: targetAssemblyName,
            additionalReferences: new[] { aspectCompilation.ToMetadataReference() } );

        using TestDesignTimeAspectPipelineFactory factory = new( testContext );
        var aspectProjectPipeline = factory.CreatePipeline( aspectCompilation );
        var targetProjectPipeline = factory.CreatePipeline( targetCompilation );

        // First execution of the pipeline.
        Assert.True( factory.TryExecute( testContext.ProjectOptions, targetCompilation, default, out var results ) );
        var dumpedResults = DumpResults( results );

        Assert.Equal( expectedResult.Replace( "$AspectVersion$", "1" ).Replace( "$TargetVersion$", "1" ).Trim(), dumpedResults );
        Assert.Equal( 1, targetProjectPipeline.PipelineExecutionCount );

        // Second execution with the same compilation. The result should be the same, and the number of executions should not change because the result is cached.
        Assert.True( factory.TryExecute( testContext.ProjectOptions, targetCompilation, default, out var results2 ) );
        var dumpedResults2 = DumpResults( results2 );
        Assert.Equal( expectedResult.Replace( "$AspectVersion$", "1" ).Replace( "$TargetVersion$", "1" ).Trim(), dumpedResults2 );
        Assert.Equal( 1, targetProjectPipeline.PipelineExecutionCount );

        // Third execution, with modified aspect but not target code. This should pause the pipeline. We don't resume the pipeline, so we should get the old result.
        var aspectCompilation3 = TestCompilationFactory.CreateCSharpCompilation(
            new Dictionary<string, string>() { { "Aspect.cs", aspectCode.Replace( "$version$", "2" ) } },
            name: aspectAssemblyName );

        var targetCompilation3 = TestCompilationFactory.CreateCSharpCompilation(
            new Dictionary<string, string>() { { "Target.cs", targetCode.Replace( "$version$", "1" ) } },
            name: targetAssemblyName,
            additionalReferences: new[] { aspectCompilation3.ToMetadataReference() } );

        Assert.True( factory.TryExecute( testContext.ProjectOptions, targetCompilation3, default, out var results3 ) );

        await targetProjectPipeline.ProcessJobQueueWhenLockAvailableAsync();
        await aspectProjectPipeline.ProcessJobQueueWhenLockAvailableAsync();
        Assert.Equal( DesignTimeAspectPipelineStatus.Paused, targetProjectPipeline.Status );
        Assert.Equal( DesignTimeAspectPipelineStatus.Paused, aspectProjectPipeline.Status );
        Assert.True( factory.EventHub.IsEditingCompileTimeCode );
        Assert.True( aspectProjectPipeline.IsCompileTimeSyntaxTreeOutdated( "Aspect.cs" ) );

        var dumpedResults3 = DumpResults( results3 );

        Assert.Equal( expectedResult.Replace( "$AspectVersion$", "1" ).Replace( "$TargetVersion$", "1" ).Trim(), dumpedResults3 );
        Assert.Equal( 1, targetProjectPipeline.PipelineExecutionCount );
        Assert.Equal( 1, targetProjectPipeline.PipelineInitializationCount );

        // Simulate an external build event. This is normally triggered by the build touch file or by a UI signal.
        await aspectProjectPipeline.ResumeAsync( AsyncExecutionContext.Get(), false );
        await aspectProjectPipeline.ProcessJobQueueWhenLockAvailableAsync();
        await targetProjectPipeline.ProcessJobQueueWhenLockAvailableAsync();
        Assert.Equal( DesignTimeAspectPipelineStatus.Default, targetProjectPipeline.Status );
        Assert.Equal( DesignTimeAspectPipelineStatus.Default, aspectProjectPipeline.Status );
        Assert.False( factory.EventHub.IsEditingCompileTimeCode );

        // A new evaluation of the design-time pipeline should now give the new results.
        Assert.True( factory.TryExecute( testContext.ProjectOptions, targetCompilation3, default, out var results6 ) );
        var dumpedResults6 = DumpResults( results6 );

        Assert.Equal( expectedResult.Replace( "$AspectVersion$", "2" ).Replace( "$TargetVersion$", "1" ).Trim(), dumpedResults6 );
        await targetProjectPipeline.ProcessJobQueueWhenLockAvailableAsync();
        await aspectProjectPipeline.ProcessJobQueueWhenLockAvailableAsync();
        Assert.Equal( 2, targetProjectPipeline.PipelineExecutionCount );
        Assert.Equal( 2, targetProjectPipeline.PipelineInitializationCount );
        Assert.False( targetProjectPipeline.IsCompileTimeSyntaxTreeOutdated( "Aspect.cs" ) );
    }

    [Fact]
    public void ChangeInTargetCode()
    {
        var assemblyName = "test_" + RandomIdGenerator.GenerateId();

        const string aspectCode = @"
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

        const string expectedResult = @"
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

            Assert.True( factory.TryExecute( testContext.ProjectOptions, compilation1, default, out var results ) );

            var dumpedResults = DumpResults( results );

            this.TestOutput.WriteLine( "-----------------" );
            this.TestOutput.WriteLine( dumpedResults );

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
        using var context = this.CreateTestContext();

        using var pipelineFactory = new TestDesignTimeAspectPipelineFactory( context );

        // The dependency cannot have a reference to Metalama.
        // It needs to define a system type that is considered as compile-time.
        var dependency = TestCompilationFactory.CreateCSharpCompilation( "namespace System; struct Index {}", addMetalamaReferences: false );

        // The main compilation must have a compile-time syntax tree.
        var compilation = context.CreateCompilationModel(
            "using Metalama.Framework.Aspects; class A : TypeAspect {}",
            additionalReferences: new[] { dependency.ToMetadataReference() } );

        Assert.True( pipelineFactory.TryExecute( context.ProjectOptions, compilation.RoslynCompilation, default, out _ ) );
    }

    [Fact]
    public void ChangeInDependency_CacheInvalidation()
    {
        var observer = new TestDesignTimePipelineObserver();
        var mocks = new AdditionalServiceCollection( observer );
        using var testContext = this.CreateTestContext( mocks );

        using TestDesignTimeAspectPipelineFactory factory = new( testContext );

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

        var masterCompilation1 = TestCompilationFactory.CreateCSharpCompilation( masterCode1, name: "Master" );

        var dependentCompilation1 = TestCompilationFactory.CreateCSharpCompilation(
            dependentCode,
            name: "Dependent",
            additionalReferences: new[] { masterCompilation1.ToMetadataReference() } );

        Assert.True( factory.TryExecute( testContext.ProjectOptions, dependentCompilation1, default, out var results1 ) );

        Assert.Equal( 2, observer.InitializePipelineEvents.Count );
        Assert.Contains( "Master", observer.InitializePipelineEvents );
        Assert.Contains( "Dependent", observer.InitializePipelineEvents );
        observer.InitializePipelineEvents.Clear();

        Assert.Contains(
            "Fields='Field1'",
            results1.Result.SyntaxTreeResults.Single().Value.Diagnostics.Single().GetMessage( CultureInfo.InvariantCulture ) );

        // Second compilation with a different master compilation.
        var masterCode2 = new Dictionary<string, string>() { ["master.cs"] = @"public partial class BaseClass { public int Field2; }" };

        var masterCompilation2 = TestCompilationFactory.CreateCSharpCompilation( masterCode2, name: "Master" );

        var dependentCompilation2 = TestCompilationFactory.CreateCSharpCompilation(
            dependentCode,
            name: "Dependent",
            additionalReferences: new[] { masterCompilation2.ToMetadataReference() } );

        Assert.True( factory.TryExecute( testContext.ProjectOptions, dependentCompilation2, default, out var results2 ) );

        Assert.Empty( observer.InitializePipelineEvents );

        Assert.Contains(
            "Fields='Field2'",
            results2.Result.SyntaxTreeResults.Single().Value.Diagnostics.Single().GetMessage( CultureInfo.InvariantCulture ) );

        // Third compilation. Add a syntax tree with a partial type.
        var masterCode3 = new Dictionary<string, string>()
        {
            ["master.cs"] = @"public partial class BaseClass { public int Field2; }", ["partial.cs"] = "partial class BaseClass { public int Field3; }"
        };

        var masterCompilation3 = TestCompilationFactory.CreateCSharpCompilation( masterCode3, name: "Master" );

        var dependentCompilation3 = TestCompilationFactory.CreateCSharpCompilation(
            dependentCode,
            name: "Dependent",
            additionalReferences: new[] { masterCompilation3.ToMetadataReference() } );

        observer.InitializePipelineEvents.Clear();

        Assert.True( factory.TryExecute( testContext.ProjectOptions, dependentCompilation3, default, out var results3 ) );

        Assert.Empty( observer.InitializePipelineEvents );

        Assert.Contains(
            "Fields='Field2,Field3'",
            results3.Result.SyntaxTreeResults.Single().Value.Diagnostics.Single().GetMessage( CultureInfo.InvariantCulture ) );
    }

    [Fact]
    public async Task FixingTemplateErrorAsync()
    {
        using var testContext = this.CreateTestContext();

        using TestDesignTimeAspectPipelineFactory factory = new( testContext );

        const string code1 = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

class MyAspect : TypeAspect
{
   SomeError;
}

";

        var compilation1 = TestCompilationFactory.CreateCSharpCompilation( code1, name: "project", ignoreErrors: true );

        var result1 = await factory.ExecuteAsync( compilation1, AsyncExecutionContext.Get() );
        Assert.False( result1.IsSuccessful );

        const string code2 = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

class MyAspect : TypeAspect
{
   
}

";

        var compilation2 = TestCompilationFactory.CreateCSharpCompilation( code2, name: "project" );

        var result2 = await factory.ExecuteAsync( compilation2, AsyncExecutionContext.Get() );
        Assert.True( result2.IsSuccessful );
    }

    [Fact]
    public void CompilationMissingAnyReferenceDuringInitialization()
    {
        using var testContext = this.CreateTestContext();
        var observer = new TestDesignTimePipelineObserver();

        using TestDesignTimeAspectPipelineFactory factory = new( testContext, testContext.ServiceProvider.WithService( observer ) );

        var code = new Dictionary<string, string>()
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

class C 
{
   [MyAspect]
   void M() {}
}
"
        };

        // First compilation without any reference.

        var compilation1 = TestCompilationFactory.CreateCSharpCompilation( code, name: "Project" ).WithReferences( Enumerable.Empty<MetadataReference>() );

        Assert.False( factory.TryExecute( testContext.ProjectOptions, compilation1, default, out _ ) );

        // Second compilation with proper references.
        var compilation2 = TestCompilationFactory.CreateCSharpCompilation( code, name: "Project" );

        Assert.True( factory.TryExecute( testContext.ProjectOptions, compilation2, default, out _ ) );
    }

    [Fact]
    public async Task PipelineConfigurationDoesNotKeepReferenceToCompilation()
    {
        var output = await this.PipelineConfigurationDoesNotKeepReferenceToCompilationCore();

        for ( var i = 0; i < 10; i++ )
        {
            var hasDanglingRef = false;

            if ( output.DependentCompilationRef.IsAlive )
            {
                hasDanglingRef = true;
                this.TestOutput.WriteLine( "Reference to the dependent compilation." );
            }

            if ( output.MasterCompilationRef.IsAlive )
            {
                hasDanglingRef = true;
                this.TestOutput.WriteLine( "Reference to the master compilation." );
            }

            if ( output.SyntaxTreeRefs.Any( r => r.IsAlive ) )
            {
                hasDanglingRef = true;
                this.TestOutput.WriteLine( "Reference to a syntax tree." );
            }

            if ( !hasDanglingRef )
            {
                this.TestOutput.WriteLine( "No more dangling reference." );

                return;
            }

            this.TestOutput.WriteLine( "GC.Collect()" );
#if NET6_0_OR_GREATER
            this.TestOutput.WriteLine( $"Finalizing queue: {GC.GetGCMemoryInfo().FinalizationPendingCount}" );
#endif
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        MemoryLeakHelper.CaptureMiniDumpOnce();
        MemoryLeakHelper.CaptureDotMemoryDumpAndThrow();

        GC.KeepAlive( output.Configuration );
    }

    private async Task<(
            WeakReference MasterCompilationRef,
            WeakReference DependentCompilationRef,
            List<WeakReference> SyntaxTreeRefs,
            AspectPipelineConfiguration Configuration,
            DesignTimeAspectPipeline Pipeline)>
        PipelineConfigurationDoesNotKeepReferenceToCompilationCore()
    {
        using var testContext = this.CreateTestContext();

        using TestDesignTimeAspectPipelineFactory factory = new( testContext, testContext.ServiceProvider );

        var (masterCompilation, dependentCompilation) = CreateCompilations( 1 );
        var syntaxTreeRefs = new List<WeakReference>();
        syntaxTreeRefs.AddRange( masterCompilation.SyntaxTrees.Select( x => new WeakReference( x ) ) );
        syntaxTreeRefs.AddRange( dependentCompilation.SyntaxTrees.Select( x => new WeakReference( x ) ) );

        var pipeline = factory.GetOrCreatePipeline( testContext.ProjectOptions, dependentCompilation )!;

        var configuration = await pipeline.GetConfigurationAsync(
            PartialCompilation.CreateComplete( dependentCompilation ),
            true,
            AsyncExecutionContext.Get(),
            default );

        var (_, dependentCompilation2) = CreateCompilations( 2 );

        // This is to make sure that the first compilation is not the last one, because it's ok to hold a reference to the last-seen compilation.
        await pipeline.ExecuteAsync( dependentCompilation2, true, AsyncExecutionContext.Get() );

        Assert.Same( pipeline.LastProjectVersion!.Compilation, dependentCompilation2 );

        return (new WeakReference( masterCompilation ), new WeakReference( dependentCompilation ), syntaxTreeRefs, configuration.Value, pipeline);
    }

    private static ( CSharpCompilation Master, CSharpCompilation Dependent ) CreateCompilations( int version )
    {
        var masterCode = new Dictionary<string, string>()
        {
            ["aspect.cs"] = $@"
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Eligibility;
using System.Linq;
using System;

public class MyAspect{version} : OverrideMethodAspect
{{
   public override dynamic? OverrideMethod() 
   {{
      return meta.Proceed();
    }}
}}
",
            ["usage.cs"] = $@"

class C{version} 
{{
   [MyAspect{version}]
   void M() {{}}
}}
"
        };

        var dependentCode = new Dictionary<string, string>()
        {
            ["dependent.cs"] = $@"
class D{version} 
{{
   [MyAspect{version}]
   void M() {{}}
}}

"
        };

        var masterCompilation = TestCompilationFactory.CreateCSharpCompilation(
            masterCode,
            name: "Master" );

        var dependentCompilation = TestCompilationFactory.CreateCSharpCompilation(
            dependentCode,
            name: "Dependent",
            additionalReferences: new[] { masterCompilation.ToMetadataReference() } );

        return (masterCompilation, dependentCompilation);
    }

#if NET6_0_OR_GREATER
    [SkippableFact]
    public void OverrideMethodWithMultipleTargetFrameworks()
    {
        const string code = """
                            using Metalama.Framework.Aspects;

                            class Aspect : OverrideMethodAspect
                            {
                                public override dynamic? OverrideMethod() => null;
                            }

                            class Target
                            {
                                [Aspect]
                                void M() {}
                            }
                            """;

        using var testContext = this.CreateTestContext();

        using TestDesignTimeAspectPipelineFactory factory = new( testContext );

        var netCompilation = TestCompilationFactory.CreateCSharpCompilation( code, name: "Project" );

        var coreReferences = netCompilation.References
            .Where( reference => reference.Display?.EndsWith( "Metalama.Framework.dll", StringComparison.Ordinal ) != true );

        var netMetalamaFrameworkPath = typeof(CompileTimeAttribute).Assembly.Location;
        var baseDirectoryPath = new FileInfo( netMetalamaFrameworkPath ).Directory!.Parent!.FullName;
        var metalamaFrameworkAssemblyName = Path.GetFileName( netMetalamaFrameworkPath );

        var netFrameworkMetalamaFrameworkPath = Path.Combine( baseDirectoryPath, "netframework4.8", metalamaFrameworkAssemblyName );

        // It may be possible that only the .Net 6.0 TFM of this project has been built. In that case, this test cannot proceed.
        Skip.If( !File.Exists( netFrameworkMetalamaFrameworkPath ) );

        var netFrameworkCompilation = TestCompilationFactory.CreateCSharpCompilation( "", name: "Project" )
            .WithReferences( coreReferences.Append( MetadataReference.CreateFromFile( netFrameworkMetalamaFrameworkPath ) ) );

        Assert.True( factory.TryExecute( testContext.ProjectOptions, netFrameworkCompilation, default, out _ ) );
        Assert.True( factory.TryExecute( testContext.ProjectOptions, netCompilation, default, out var result ) );

        foreach ( var (_, treeResult) in result.Result.SyntaxTreeResults )
        {
            Assert.Empty( treeResult.Diagnostics );
        }
    }
#endif

    [Fact]
    public async Task ResumeWithErrorAsync()
    {
        static CSharpCompilation CreateCompilation( string statement )
        {
            var code = new Dictionary<string, string>
            {
                ["Aspect.cs"] =
                    $$"""
                      using Metalama.Framework.Aspects;
                      using System;

                      public class Aspect : OverrideMethodAspect
                      {
                          public override dynamic OverrideMethod()
                          {
                              {{statement}}
                              return null;
                          }
                      }
                      """
            };

            return CreateCSharpCompilation( code, acceptErrors: true );
        }

        using var testContext = this.CreateTestContext();
        using var pipelineFactory = new TestDesignTimeAspectPipelineFactory( testContext );

        var compilation1 = CreateCompilation( "" );
        var pipeline = pipelineFactory.CreatePipeline( compilation1 );

        Assert.Equal( DesignTimeAspectPipelineStatus.Default, pipeline.Status );

        // Execute with the initial valid code.
        Assert.True( pipeline.TryExecute( compilation1, default, out _ ) );

        Assert.Equal( DesignTimeAspectPipelineStatus.Ready, pipeline.Status );

        // Execute with incomplete/invalid statement.
        var compilation2 = CreateCompilation( "Console" );
        Assert.True( pipeline.TryExecute( compilation2, default, out var compilationResult ) );

        // Note that LAMA0118 is no longer reported by the pipeline but by the analyzer.
        Assert.Empty( compilationResult.GetAllDiagnostics( "Aspect.cs" ) );

        Assert.Equal( DesignTimeAspectPipelineStatus.Paused, pipeline.Status );

        // Resume while the code is still invalid.
        await pipeline.ResumeAsync( AsyncExecutionContext.Get(), false );

        Assert.Equal( DesignTimeAspectPipelineStatus.Default, pipeline.Status );

        // Executing with the same code fails at this point.
        var executionResult = await pipeline.ExecuteAsync( compilation2, AsyncExecutionContext.Get() );
        Assert.False( executionResult.IsSuccessful );

        Assert.Equal( DesignTimeAspectPipelineStatus.Default, pipeline.Status );

        // Executing with new invalid code fails and causes pausing.
        var compilation3 = CreateCompilation( "Console.Write" );
        executionResult = await pipeline.ExecuteAsync( compilation3, AsyncExecutionContext.Get() );
        Assert.False( executionResult.IsSuccessful );

        // Note that LAMA0118 is no longer reported by the pipeline but by the analyzer.
        Assert.Empty( executionResult.Diagnostics );

        Assert.Equal( DesignTimeAspectPipelineStatus.Paused, pipeline.Status );
    }

    [Fact]
    public void GenericTargetIntroductions()
    {
        using var testContext = this.CreateTestContext();

        const string code = """
                            using Metalama.Framework.Aspects;
                            using Metalama.Framework.Code;

                            public class RepositoryAspect : TypeAspect
                            {
                                [Introduce]
                                public int Id { get; set; }
                            }

                            [RepositoryAspect]
                            public partial class Repository<T1, T2>
                            {
                            }

                            [RepositoryAspect]
                            public partial interface IVariantRepository<in T>
                            {
                            }

                            public partial class Outer<T1>
                            {
                                [RepositoryAspect]
                                public partial class Inner<T2>
                                {
                                }
                            }
                            """;

        var compilation = CreateCSharpCompilation( new Dictionary<string, string>() { { "F1.cs", code } } );
        using TestDesignTimeAspectPipelineFactory factory = new( testContext );

        Assert.True( factory.TryExecute( testContext.ProjectOptions, compilation, default, out var results ) );
        var dumpedResults = DumpResults( results );
        this.TestOutput.WriteLine( dumpedResults );

        const string expectedResult = """
                                      F1.cs:
                                      0 diagnostic(s):
                                      0 suppression(s):
                                      3 introductions(s):
                                      /// <generated>
                                      /// Generated by Metalama to support the code editing experience. This is NOT the code that gets executed.
                                      /// </generated>
                                      partial interface IVariantRepository<in T>
                                      {
                                          public global::System.Int32 Id { get; set; }
                                      }
                                      /// <generated>
                                      /// Generated by Metalama to support the code editing experience. This is NOT the code that gets executed.
                                      /// </generated>
                                      partial class Outer<T1>
                                      {
                                          partial class Inner<T2>
                                          {
                                              public global::System.Int32 Id { get; set; }
                                          }
                                      }
                                      /// <generated>
                                      /// Generated by Metalama to support the code editing experience. This is NOT the code that gets executed.
                                      /// </generated>
                                      partial class Repository<T1, T2>
                                      {
                                          public global::System.Int32 Id { get; set; }
                                      }
                                      """;

        Assert.Equal( expectedResult.Replace( "\r\n", "\n" ), dumpedResults.Replace( "\r\n", "\n" ) );
    }

    [Fact]
    public void OptionsTest()
    {
        using var testContext = this.CreateTestContext();

        var code = new Dictionary<string, string>
        {
            ["options.cs"] = OptionsTestHelper.OptionsCode,
            ["aspect.cs"] = OptionsTestHelper.ReportWarningFromOptionAspectCode,
            ["fabric.cs"] = """
                            using Metalama.Framework.Fabrics;
                            class Fabric : ProjectFabric
                            {
                                public override void AmendProject( IProjectAmender amender )
                                {
                                    amender.Outbound.SetOptions<MyOptions>( o => new MyOptions { Value = "THE_VALUE" } );
                                }
                            }
                            """,
            ["target.cs"] =
                """
                class C
                {
                    [ReportWarningFromOptionsAspect]
                    void M(){}
                }
                """
        };

        using TestDesignTimeAspectPipelineFactory factory = new( testContext );

        var compilation = CreateCSharpCompilation( code );
        Assert.True( factory.TryExecute( testContext.ProjectOptions, compilation, default, out var results ) );
        Assert.Contains( results.GetAllDiagnostics(), d => d.GetMessage( CultureInfo.InvariantCulture ).Contains( "Option='THE_VALUE'" ) );
    }

    [Fact]
    public async Task OptionsCrossProjectTest()
    {
        using var testContext = this.CreateTestContext();

        const string fabricCode = """
                                  using Metalama.Framework.Fabrics;
                                  class Fabric : ProjectFabric
                                  {
                                      public override void AmendProject( IProjectAmender amender )
                                      {
                                          amender.Outbound.SetOptions<MyOptions>( o => new MyOptions { Value = "THE_VALUE" } );
                                      }
                                  }
                                  """;

        var dependencyCode = new Dictionary<string, string>()
        {
            ["options.cs"] = OptionsTestHelper.OptionsCode,
            ["fabric.cs"] = fabricCode,
            ["code.cs"] =
                """
                public class C {}
                """
        };

        var code = new Dictionary<string, string>
        {
            ["aspect.cs"] = OptionsTestHelper.ReportWarningFromOptionAspectCode,
            ["target.cs"] =
                """
                class D : C
                {
                    [ReportWarningFromOptionsAspect]
                    void M(){}
                }
                """
        };

        using TestDesignTimeAspectPipelineFactory factory = new( testContext );

        var dependencyCompilation = CreateCSharpCompilation( dependencyCode, assemblyName: "dependency" );
        var compilation = CreateCSharpCompilation( code, assemblyName: "main", additionalReferences: new[] { dependencyCompilation.ToMetadataReference() } );
        Assert.True( factory.TryExecute( testContext.ProjectOptions, compilation, default, out var results ) );
        Assert.Contains( results.GetAllDiagnostics(), d => d.GetMessage( CultureInfo.InvariantCulture ).Contains( "Option='THE_VALUE'" ) );

        // Try an update.
        dependencyCode["fabric.cs"] = fabricCode.Replace( "THE_VALUE", "THE_UPDATED_VALUE" );
        var updatedDependencyCompilation = CreateCSharpCompilation( dependencyCode, assemblyName: "dependency" );

        var updatedCompilation = CreateCSharpCompilation(
            code,
            assemblyName: "main",
            additionalReferences: new[] { updatedDependencyCompilation.ToMetadataReference() } );

        // The next pipeline run will pause the pipeline.
        Assert.True( factory.TryExecute( testContext.ProjectOptions, updatedCompilation, default, out _ ) );

        // Resume the pipeline.
        await factory.ResumePipelinesAsync( AsyncExecutionContext.Get(), false, default );

        // Run the pipeline again.
        Assert.True( factory.TryExecute( testContext.ProjectOptions, updatedCompilation, default, out var updatedResults ) );
        Assert.Contains( updatedResults.GetAllDiagnostics(), d => d.GetMessage( CultureInfo.InvariantCulture ).Contains( "Option='THE_UPDATED_VALUE'" ) );
    }

    [Fact]
    public void AnnotationsCrossProjectTest()
    {
        using var testContext = this.CreateTestContext();

        var dependencyCode = new Dictionary<string, string>()
        {
            ["annotation.cs"] =
                """
                using Metalama.Framework.Code;
                public class TheAnnotation : IAnnotation<INamedType>
                {
                    public string Value;
                    
                    public TheAnnotation( string value )
                    {
                        this.Value = value;
                    }
                }
                """,
            ["aspect.cs"] =
                """
                using Metalama.Framework.Aspects;
                using Metalama.Framework.Code;
                public class AddAnnotation : TypeAspect
                {
                    public string Value;
                   
                   public AddAnnotation( string value )
                   {
                       this.Value = value;
                   }
                    
                    public override void BuildAspect( IAspectBuilder<INamedType> builder )
                    {
                        builder.Advice.AddAnnotation( builder.Target, new TheAnnotation(this.Value), true );
                    }
                }
                """,
            ["code.cs"] =
                """
                [AddAnnotation("THE_VALUE")]
                public class C {}
                """
        };

        var code = new Dictionary<string, string>
        {
            ["aspect.cs"] =
                """
                using Metalama.Framework.Aspects;
                using Metalama.Framework.Code;
                using Metalama.Framework.Diagnostics;
                using Metalama.Framework.Eligibility;
                using System.Linq;
                using System;

                class ReportWarningFromAnnotationAspect : TypeAspect
                {
                   private static readonly DiagnosticDefinition<string> _description = new("MY001", Severity.Warning, "Option='{0}'" );
                   
                   public override void BuildAspect( IAspectBuilder<INamedType> aspectBuilder )
                   {
                        var annotation = aspectBuilder.Target.BaseType.Enhancements().GetAnnotations<TheAnnotation>().SingleOrDefault();
                        aspectBuilder.Diagnostics.Report( _description.WithArguments( annotation?.Value ?? "<undefined>" ) );
                   }
                }
                """,
            ["target.cs"] =
                """
                [ReportWarningFromAnnotationAspect]
                class D : C
                {
                    
                }
                """
        };

        using TestDesignTimeAspectPipelineFactory factory = new( testContext );

        var dependencyCompilation = CreateCSharpCompilation( dependencyCode, assemblyName: "dependency" );
        var compilation = CreateCSharpCompilation( code, assemblyName: "main", additionalReferences: new[] { dependencyCompilation.ToMetadataReference() } );
        Assert.True( factory.TryExecute( testContext.ProjectOptions, compilation, default, out var results ) );
        Assert.Contains( results.GetAllDiagnostics(), d => d.GetMessage( CultureInfo.InvariantCulture ).Contains( "Option='THE_VALUE'" ) );

        // Try an update.
        dependencyCode["code.cs"] = dependencyCode["code.cs"].Replace( "THE_VALUE", "THE_UPDATED_VALUE" );
        var updatedDependencyCompilation = CreateCSharpCompilation( dependencyCode, assemblyName: "dependency" );

        var updatedCompilation = CreateCSharpCompilation(
            code,
            assemblyName: "main",
            additionalReferences: new[] { updatedDependencyCompilation.ToMetadataReference() } );

        // Run the pipeline again.
        Assert.True( factory.TryExecute( testContext.ProjectOptions, updatedCompilation, default, out var updatedResults ) );
        Assert.Contains( updatedResults.GetAllDiagnostics(), d => d.GetMessage( CultureInfo.InvariantCulture ).Contains( "Option='THE_UPDATED_VALUE'" ) );
    }
}