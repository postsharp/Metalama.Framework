// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.DesignTime.Pipeline;
using Metalama.Framework.Engine.Testing;
using Metalama.TestFramework;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime
{
    public class InheritableAspectTests : TestBase
    {
        [Fact]
        public void InheritableAspectsMakeItToCompilationResult()
        {
            using var testContext = this.CreateTestContext();
            using var domain = new UnloadableCompileTimeDomain();

            // Initial compilation.
            var code1 = @"
using Metalama.Framework.Aspects;

public class Aspect : TypeAspect, IInheritedAspect {}

[Aspect]
public interface I {}
";

            var compilation1 = testContext.CreateCompilationModel( code1 );

            var pipeline = new DesignTimeAspectPipeline( testContext.ServiceProvider, domain, compilation1.RoslynCompilation.References, true );

            Assert.True( pipeline.TryExecute( compilation1.RoslynCompilation, CancellationToken.None, out var compilationResult1 ) );

            Assert.Equal( new[] { "Aspect" }, compilationResult1!.InheritableAspectTypes.ToArray() );
            Assert.Equal( new[] { "T:I" }, compilationResult1.GetInheritedAspects( "Aspect" ).Select( i=>i.TargetDeclaration.ToSerializableId() ).ToArray() );
        }

        [Fact]
        public void IncrementalCompilationWorks()
        {
            using var testContext = this.CreateTestContext();
            using var domain = new UnloadableCompileTimeDomain();

            var aspectCode = @"
using Metalama.Framework.Aspects;

public class Aspect : TypeAspect, IInheritedAspect
{
}
";

            // Initial compilation.
            var targetCode1 = "[Aspect] interface I {}";

            var compilation1 = testContext.CreateCompilationModel( new Dictionary<string, string> { ["aspect.cs"] = aspectCode, ["target.cs"] = targetCode1 } );

            var targetTree1 = compilation1.RoslynCompilation.SyntaxTrees.Single( t => t.FilePath == "target.cs" );

            var pipeline = new DesignTimeAspectPipeline( testContext.ServiceProvider, domain, compilation1.RoslynCompilation.References, true );
            Assert.True( pipeline.TryExecute( compilation1.RoslynCompilation, CancellationToken.None, out var compilationResult1 ) );
            Assert.Equal( new[] { "T:I" }, compilationResult1!.GetInheritedAspects( "Aspect" ).Select( i=>i.TargetDeclaration.ToSerializableId() ).ToArray() );

            // Add a target class.
            var targetTree2 = CSharpSyntaxTree.ParseText( "[Aspect] interface I {} [Aspect] class C {}", path: "target.cs" );

            var compilation2 = testContext.CreateCompilationModel( compilation1.RoslynCompilation.ReplaceSyntaxTree( targetTree1, targetTree2 ) );
            Assert.True( pipeline.TryExecute( compilation2.RoslynCompilation, CancellationToken.None, out var compilationResult2 ) );
            Assert.Equal( new[] { "T:C", "T:I" }, compilationResult2!.GetInheritedAspects( "Aspect" ).Select( i=>i.TargetDeclaration.ToSerializableId() ).OrderBy( a => a ).ToArray() );

            // Remove a target
            var targetTree3 = CSharpSyntaxTree.ParseText( "[Aspect] class C {}", path: "target.cs"  );
            var compilation3 = testContext.CreateCompilationModel( compilation2.RoslynCompilation.ReplaceSyntaxTree( targetTree2, targetTree3 ) );
            Assert.True( pipeline.TryExecute( compilation3.RoslynCompilation, CancellationToken.None, out var compilationResult3 ) );
            Assert.Equal( new[] { "T:C" }, compilationResult3!.GetInheritedAspects( "Aspect" ).Select( i=>i.TargetDeclaration.ToSerializableId() ).OrderBy( a => a ).ToArray() );
        }

#if NET5_0_OR_GREATER
        [Fact( Skip = "CLR internal error when unloading the domain" )]
#else
        [Fact]
#endif
        public void CrossProjectIntegration()
        {
            using var domain = new UnloadableCompileTimeDomain();
            using var options = new TestProjectOptions();
            using var factory = new TestDesignTimeAspectPipelineFactory( domain, options );

            var code1 = @"
using Metalama.Framework.Aspects;

public class Aspect : TypeAspect, IInheritedAspect
{
    [Introduce]
    public void IntroducedMethod() {}
}

[Aspect]
public interface I {}
";

            var code2 = @"partial class C : I {}";

            using var testContext1 = this.CreateTestContext();

            var compilation1 = CreateCSharpCompilation( code1 );

            using var testContext2 = this.CreateTestContext();

            var compilation2 = CreateCSharpCompilation( code2, additionalReferences: new[] { compilation1.ToMetadataReference() } );

            // We have to execute the pipeline on compilation1 first and explicitly because implicit running is not currently possible
            // because of missing project options.

            Assert.True( factory.TryExecute( testContext1.ProjectOptions, compilation1, CancellationToken.None, out _ ) );
            Assert.True( factory.TryExecute( testContext2.ProjectOptions, compilation2, CancellationToken.None, out var compilationResult2 ) );

            Assert.Single( compilationResult2!.IntroducedSyntaxTrees );
        }
    }
}