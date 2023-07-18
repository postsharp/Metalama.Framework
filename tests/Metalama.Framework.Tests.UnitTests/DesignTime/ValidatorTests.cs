// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Tests.UnitTests.DesignTime.Mocks;
using Metalama.Testing.UnitTesting;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime
{
    public sealed class ValidatorTests : UnitTestClass
    {
        [Fact]
        public void ReferenceValidatorsMakeItToCompilationResult()
        {
            using var testContext = this.CreateTestContext();

            // Initial compilation.
            const string code1 = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Validation;
using Metalama.Framework.Code;

public class Aspect : TypeAspect 
{
   public override void BuildAspect( IAspectBuilder<INamedType> builder )
   {
        builder.Outbound.ValidateReferences( this.Validate, ReferenceKinds.All );
   }

    private void Validate( in ReferenceValidationContext context ) {}
}

[Aspect]
public class C {}
";

            var compilation1 = testContext.CreateCompilationModel( code1 );

            using var pipelineFactory = new TestDesignTimeAspectPipelineFactory( testContext );
            var pipeline = pipelineFactory.CreatePipeline( compilation1.RoslynCompilation );

            Assert.True( pipeline.TryExecute( compilation1.RoslynCompilation, default, out var compilationResult1 ) );

            Assert.False( compilationResult1.TransformationResult.Validators.IsEmpty );
            Assert.Single( compilationResult1.TransformationResult.Validators.GetValidatorsForSymbol( compilation1.Types.OfName( "C" ).Single().GetSymbol() ) );
        }

        [Fact]
        public void IncrementalCompilationWorks()
        {
            using var testContext = this.CreateTestContext();

            const string aspectCode = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Validation;
using Metalama.Framework.Code;

public class Aspect1 : TypeAspect 
{
   public override void BuildAspect( IAspectBuilder<INamedType> builder )
   {
        builder.Outbound.ValidateReferences( this.Validate, ReferenceKinds.All );
   }

    private void Validate( in ReferenceValidationContext context ) {}
}

public class Aspect2 : TypeAspect 
{
   public override void BuildAspect( IAspectBuilder<INamedType> builder )
   {
        builder.Outbound.ValidateReferences( this.Validate, ReferenceKinds.All );
   }

    private void Validate( in ReferenceValidationContext context ) {}
}

";

            // Initial compilation.
            const string targetCode1 = "[Aspect1] class C {}";

            var compilation1 = testContext.CreateCompilationModel( new Dictionary<string, string> { ["aspect.cs"] = aspectCode, ["target.cs"] = targetCode1 } );
            var classC = compilation1.Types.OfName( "C" ).Single().GetSymbol().AssertNotNull();

            var targetTree1 = compilation1.RoslynCompilation.SyntaxTrees.Single( t => t.FilePath == "target.cs" );

            using var pipelineFactory = new TestDesignTimeAspectPipelineFactory( testContext );
            var pipeline = pipelineFactory.CreatePipeline( compilation1.RoslynCompilation );

            Assert.True( pipeline.TryExecute( compilation1.RoslynCompilation, default, out var compilationResult1 ) );

            Assert.False( compilationResult1.TransformationResult.Validators.IsEmpty );

            Assert.Equal(
                new[] { "Aspect1" },
                compilationResult1.TransformationResult.Validators.GetValidatorsForSymbol( classC )
                    .SelectAsImmutableArray( v => v.Implementation.Implementation.GetType().Name ) );

            // Add a constraint.
            var targetTree2 = CSharpSyntaxTree.ParseText(
                "[Aspect1, Aspect2] class C {}",
                path: "target.cs",
                options: SupportedCSharpVersions.DefaultParseOptions );

            var compilation2 = testContext.CreateCompilationModel( compilation1.RoslynCompilation.ReplaceSyntaxTree( targetTree1, targetTree2 ) );
            Assert.True( pipeline.TryExecute( compilation2.RoslynCompilation, default, out var compilationResult2 ) );
            Assert.False( compilationResult2.TransformationResult.Validators.IsEmpty );

            Assert.Equal(
                new[] { "Aspect1", "Aspect2" },
                compilationResult2.TransformationResult.Validators.GetValidatorsForSymbol( classC )
                    .SelectAsEnumerable( v => v.Implementation.Implementation.GetType().Name )
                    .OrderBy( n => n )
                    .ToArray() );

            // Remove a constraint
            var targetTree3 = CSharpSyntaxTree.ParseText( "[Aspect2] class C {}", path: "target.cs", options: SupportedCSharpVersions.DefaultParseOptions );
            var compilation3 = testContext.CreateCompilationModel( compilation2.RoslynCompilation.ReplaceSyntaxTree( targetTree2, targetTree3 ) );
            Assert.True( pipeline.TryExecute( compilation3.RoslynCompilation, default, out var compilationResult3 ) );
            Assert.False( compilationResult3.TransformationResult.Validators.IsEmpty );

            Assert.Equal(
                new[] { "Aspect2" },
                compilationResult3.TransformationResult.Validators.GetValidatorsForSymbol( classC )
                    .SelectAsImmutableArray( v => v.Implementation.Implementation.GetType().Name ) );
        }

        /*
#if NET5_0_OR_GREATER
        [Fact( Skip = "CLR internal error when unloading the domain" )]
#else
        [Fact]
#endif
        public void CrossProjectIntegration()
        {
            using var domain = testContext.CreateDomain();
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

            Assert.True( factory.TryExecute( testContext1.ProjectOptions, compilation1, default) );
            Assert.True( factory.TryExecute( testContext2.ProjectOptions, compilation2, default, out var compilationResult2 ) );

            Assert.Single( compilationResult2!.IntroducedSyntaxTrees );
        }
        */
    }
}