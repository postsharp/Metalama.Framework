// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime;
using Metalama.Framework.DesignTime.Contracts.EntryPoint;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.DesignTime.VersionNeutral;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Metalama.Framework.Tests.UnitTests.DesignTime.Mocks;
using Metalama.Testing.UnitTesting;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

#pragma warning disable VSTHRD200

public sealed class SideBySideVersionTests : DesignTimeTestBase
{
    private async Task<(DesignTimeAspectPipelineFactory PipelineFactory, Compilation DependentCompilation, SyntaxTree DependentCodeTree)> PreparePipeline(
        TestContext testContext,
        string masterCode,
        string dependentCode )
    {
        var workspaceProvider = new TestWorkspaceProvider( testContext.ServiceProvider );

        var entryPointManager = new DesignTimeEntryPointManager();
        var consumer = entryPointManager.GetConsumer( CurrentContractVersions.All );

        var serviceProvider = testContext.ServiceProvider.Global.Underlying.WithUntypedService( typeof( IDesignTimeEntryPointConsumer ), consumer )
            .WithService( workspaceProvider );

        // Initialize dependencies simulating the current version.
        var currentVersionPipelineFactory = new TestDesignTimeAspectPipelineFactory( testContext, serviceProvider );
        var currentVersionServiceProvider = new CompilerServiceProvider( version: TestMetalamaProjectClassifier.CurrentMetalamaVersion );
        currentVersionServiceProvider.Initialize( serviceProvider.WithService( currentVersionPipelineFactory ) );
        entryPointManager.RegisterServiceProvider( currentVersionServiceProvider );

        // Initialize dependencies simulating the _other_ version.
        var otherVersionPipelineFactory = new TestDesignTimeAspectPipelineFactory( testContext, serviceProvider );
        var otherVersionServiceProvider = new CompilerServiceProvider( version: TestMetalamaProjectClassifier.OtherMetalamaVersion );
        otherVersionServiceProvider.Initialize( serviceProvider.WithService( otherVersionPipelineFactory ) );
        entryPointManager.RegisterServiceProvider( otherVersionServiceProvider );

        workspaceProvider.AddOrUpdateProject(
            "master",
            new Dictionary<string, string>() { ["master.cs"] = masterCode },
            preprocessorSymbols: ["METALAMA", TestMetalamaProjectClassifier.OtherMetalamaVersionPreprocessorSymbol] );

        var dependentProjectKey = workspaceProvider.AddOrUpdateProject(
            "dependent",
            new Dictionary<string, string>() { ["dependent.cs"] = dependentCode },
            projectReferences: ["master"],
            preprocessorSymbols: ["METALAMA"] );

        var dependentCompilation = await workspaceProvider.GetCompilationAsync( dependentProjectKey );
        var dependentCodeSyntaxTree = await workspaceProvider.GetDocument( "dependent", "dependent.cs" ).GetSyntaxTreeAsync();

        return (currentVersionPipelineFactory, dependentCompilation, dependentCodeSyntaxTree);
    }

    private async Task<FallibleResultWithDiagnostics<AspectPipelineResultAndState>> RunPipeline( string masterCode, string dependentCode )
    {
        using var testContext = this.CreateTestContext();
        var (pipelineFactory, dependentCompilation, _) = await this.PreparePipeline( testContext, masterCode, dependentCode );

        return await pipelineFactory.ExecuteAsync( dependentCompilation!, AsyncExecutionContext.Get() );
    }

    private async Task<List<Diagnostic>> RunAnalyzer( string masterCode, string dependentCode )
    {
        using var testContext = this.CreateTestContext();
        var (pipelineFactory, dependentCompilation, dependentSyntaxTree) = await this.PreparePipeline( testContext, masterCode, dependentCode );

        var semanticModel = dependentCompilation!.GetSemanticModel( dependentSyntaxTree! );

        var analyzer = new TheDiagnosticAnalyzer( pipelineFactory.ServiceProvider );
        var analysisContext = new TestSemanticModelAnalysisContext( semanticModel, testContext.ProjectOptions );

        analyzer.AnalyzeSemanticModel( analysisContext );

        return analysisContext.ReportedDiagnostics;
    }

    [Fact]
    public async Task Inheritance()
    {
        const string masterCode =
            """
            using System;
            using Metalama.Framework.Advising;
            using Metalama.Framework.Advising;
            using Metalama.Framework.Aspects;

            [Inheritable]
            public class TheAspect : TypeAspect
            {
                [Introduce( WhenExists = OverrideStrategy.New )]
                public void IntroducedMethod() {}
            }

            [TheAspect]
            public interface TheInterface;
            """;

        const string dependentCode = "public class TheClass : TheInterface;";

        var result = await this.RunPipeline( masterCode, dependentCode );

        Assert.Single( result.Value.Result.SyntaxTreeResults.Single().Value.AspectInstances );
    }

    [Fact]
    public async Task Inheritance_CompileTimeType()
    {
        const string masterCode = """
                                  using System;
                                  using Metalama.Framework.Advising;
                                  using Metalama.Framework.Aspects;
                                  using Metalama.Framework.Code;

                                  [Inheritable]
                                  public class TheAspect : TypeAspect
                                  {
                                    private Type _compileTimeType;
                                  
                                    public TheAspect()
                                    {
                                        this._compileTimeType = typeof(TestType<int>);
                                    }
                                  
                                    public override void BuildAspect(IAspectBuilder<INamedType> builder)
                                    {
                                        builder.IntroduceMethod(nameof(IntroducedMethod), args: new { type = this._compileTimeType });
                                    }
                                  
                                    [Template]
                                    public Type IntroducedMethod([CompileTime] Type type)
                                    {
                                        return type;
                                    }
                                  }

                                  public class TestType<T>;

                                  [TheAspect]
                                  public interface TheInterface
                                  {
                                  }

                                  """;

        const string dependentCode = """
                                     public class TheClass : TheInterface
                                     {
                                     }

                                     """;

        var result = await this.RunPipeline( masterCode, dependentCode );

        Assert.Single( result.Value.Result.SyntaxTreeResults.Single().Value.AspectInstances );
    }

    [Fact]
    public async Task TransitiveValidator()
    {
        const string masterCode = """
                                  using System;
                                  using Metalama.Framework.Fabrics;
                                  using Metalama.Framework.Code;
                                  using Metalama.Framework.Validation;
                                  using Metalama.Framework.Diagnostics;

                                  public class Fabric : ProjectFabric
                                  {
                                      static DiagnosticDefinition<IDeclaration> _warning = new("MY001", Severity.Warning, "Reference to {0}");
                                      public override void AmendProject(IProjectAmender amender)
                                      {
                                          amender
                                              .SelectMany(p => p.Types)
                                              .ValidateInboundReferences(
                                                  ValidateReference,
                                                  ReferenceGranularity.Member,
                                                  ReferenceKinds.All,
                                                  ReferenceValidationOptions.IncludeDerivedTypes);
                                      }
                                  
                                      private static void ValidateReference(ReferenceValidationContext context)
                                      {
                                          context.Diagnostics.Report(_warning.WithArguments(context.GetReferenceEnd(ReferenceEndRole.Origin).Declaration));
                                      }
                                  }

                                  public class A {}
                                  """;

        const string dependentCode = """
                                     class B
                                     {
                                         void M()
                                         {
                                             A a;
                                         }
                                     }
                                     """;

        var diagnostics = await this.RunAnalyzer( masterCode, dependentCode );

        Assert.Single( diagnostics );
    }
}