﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.DesignTime.AspectExplorer;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Tests.UnitTests.DesignTime.Mocks;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

#pragma warning disable VSTHRD200

public sealed class AspectDatabaseTests( ITestOutputHelper testOutputHelper ) : DesignTimeTestBase( testOutputHelper )
{
    protected override void ConfigureServices( IAdditionalServiceCollection services )
    {
        base.ConfigureServices( services );
        services.AddGlobalService( provider => new TestWorkspaceProvider( provider ) );
    }

    private static void AssertAspectClasses( IEnumerable<string> expected, IEnumerable<string> actual ) => Assert.Equal( expected, actual.OrderBy( id => id ) );

    private static void AssertAspectInstances( IEnumerable<string> expected, IEnumerable<AspectDatabaseAspectInstance> actual )
        => Assert.Equal( expected, actual.Select( aspectInstance => aspectInstance.TargetDeclarationId ).OrderBy( s => s ) );

    private static void AssertAspectTransformations( IEnumerable<string> expected, IEnumerable<AspectDatabaseAspectInstance> actual )
        => Assert.Equal(
            expected,
            actual.SelectMany( i => i.Transformations ).Select( aspectTransformation => aspectTransformation.Description ).OrderBy( s => s ) );

    [Fact]
    public async Task BasicTest()
    {
        const string code =
            """
            using Metalama.Framework.Advising;
            using Metalama.Framework.Aspects; 
            using Metalama.Framework.Code;
            using Metalama.Framework.Fabrics;
            using System;
            using System.Linq;

            [Inheritable]
            class Aspect : TypeAspect { }

            class ParentAspect : TypeAspect
            {
                public override void BuildAspect(IAspectBuilder<INamedType> builder)
                {
                    builder.Outbound.AddAspect<Aspect>();
                }
            }

            class Fabric : ProjectFabric
            {
                public override void AmendProject(IProjectAmender amender)
                {
                    amender
                        .Select(compilation => compilation.Types.OfName(nameof(FabricTarget)).Single())
                        .AddAspect<Aspect>();
                }
            }

            [Aspect]
            class AttributeTarget;

            [ParentAspect]
            class ParentTarget;

            class FabricTarget;

            class DerivedClass : AttributeTarget;
            """;

        using var testContext = this.CreateTestContext();
        using TestDesignTimeAspectPipelineFactory factory = new( testContext );

        var workspaceProvider = factory.ServiceProvider.GetRequiredService<TestWorkspaceProvider>();
        var projectKey = workspaceProvider.AddOrUpdateProject( "project", new Dictionary<string, string> { ["code.cs"] = code } );

        var aspectDatabase = new AspectDatabase( factory.ServiceProvider );

        var aspectClasses = await aspectDatabase.GetAspectClassesAsync( projectKey, default );

        AssertAspectClasses(
            [
                "Y:global::Aspect",
                "Y:global::Fabric",
                "Y:global::Metalama.Framework.Validation.InternalImplementAttribute",
                "Y:global::ParentAspect"
            ],
            aspectClasses );

        var aspectInstances = await aspectDatabase.GetAspectInstancesAsync( projectKey, "project", new SerializableTypeId( "Y:global::Aspect" ), default );

        AssertAspectInstances(
            [
                "T:AttributeTarget",
                "T:DerivedClass",
                "T:DerivedClass",
                "T:FabricTarget",
                "T:ParentTarget"
            ],
            aspectInstances );
    }

    [Fact]
    public async Task TransformationsTest()
    {
        const string code = """
                            using Metalama.Framework.Advising;
                            using Metalama.Framework.Aspects; 
                            using Metalama.Framework.Code;
                            using Metalama.Framework.Code.DeclarationBuilders;
                            using System;
                            using System.Linq;

                            [assembly: CompilationAttribute]
                            [assembly: AspectOrder( AspectOrderDirection.RunTime, typeof(MethodAspect), typeof(Aspect))]

                            class MethodAspect : OverrideMethodAspect
                            {
                                public override dynamic? OverrideMethod()
                                {
                                    return meta.Proceed();
                                }
                            }

                            class Aspect : TypeAspect
                            {
                                public override void BuildAspect(IAspectBuilder<INamedType> builder)
                                {
                                    base.BuildAspect(builder);
                            
                                    if (builder.Target.Methods.OfName("M").FirstOrDefault() is { } method)
                                    {
                                        //builder.Advice.Override(method, nameof(Template));
                                        builder.With(method).Outbound.AddAspect<MethodAspect>();
                                    }
                            
                                    if (builder.Target.Constructors.OfExactSignature(Array.Empty<IType>()) is { } constructor)
                                    {
                                        builder.Advice.IntroduceParameter(constructor, "p", typeof(int), TypedConstant.Create(0));
                                    }
                                }
                            
                                [Introduce]
                                int i;
                            
                                [Template]
                                void Template() { }
                            }

                            class CompilationAttribute : CompilationAspect
                            {
                                public override void BuildAspect(IAspectBuilder<ICompilation> builder)
                                {
                                    base.BuildAspect(builder);
                            
                                    builder.IntroduceAttribute( AttributeConstruction.Create(typeof(MyAttribute)));
                                }
                            }

                            class MyAttribute : Attribute { }

                            [Aspect]
                            class Target
                            {
                                void M() { }
                            }
                            """;

        using var testContext = this.CreateTestContext();
        using TestDesignTimeAspectPipelineFactory factory = new( testContext );

        var workspaceProvider = factory.ServiceProvider.GetRequiredService<TestWorkspaceProvider>();
        var projectKey = workspaceProvider.AddOrUpdateProject( "project", new Dictionary<string, string> { ["code.cs"] = code } );

        var aspectDatabase = new AspectDatabase( factory.ServiceProvider );

        var aspectInstances = await aspectDatabase.GetAspectInstancesAsync( projectKey, "project", new SerializableTypeId( "Y:global::Aspect" ), default );

        AssertAspectTransformations(
            [
                "Add the 'MethodAspect' aspect to 'Target.M()'.",
                "Introduce constructor 'Target.Target()'.",
                "Introduce field 'Target.i'.",
                "Introduce parameter 'p' into 'Target.Target()'."
            ],
            aspectInstances );

        var compilationInstances = await aspectDatabase.GetAspectInstancesAsync(
            projectKey,
            "project",
            new SerializableTypeId( "Y:global::CompilationAttribute" ),
            default );

        AssertAspectTransformations(
            ["Introduce attribute of type 'MyAttribute' into 'project'"],
            compilationInstances );
    }

    [Fact]
    public async Task EventsTest()
    {
        const string aspect =
            """
            using Metalama.Framework.Advising;
            using Metalama.Framework.Aspects; 
            using Metalama.Framework.Code;
            using Metalama.Framework.Fabrics;
            using System;
            using System.Linq;

            class Aspect : TypeAspect { }

            [Aspect]
            class Target;
            """;

        const string anotherAspect =
            """
            using Metalama.Framework.Advising;
            using Metalama.Framework.Aspects; 

            class AnotherAspect : TypeAspect { }
            """;

        const string newTarget =
            """
            [Aspect]
            class NewTarget;
            """;

        using var testContext = this.CreateTestContext();
        using TestDesignTimeAspectPipelineFactory factory = new( testContext );

        var workspaceProvider = factory.ServiceProvider.GetRequiredService<TestWorkspaceProvider>();

        Dictionary<string, string> code = new() { ["aspect.cs"] = aspect };

        var projectKey = workspaceProvider.AddOrUpdateProject( "project", code );

        var aspectDatabase = new AspectDatabase( factory.ServiceProvider );

        var aspectClasses = await aspectDatabase.GetAspectClassesAsync( projectKey, default );

        AssertAspectClasses(
            [
                "Y:global::Aspect",
                "Y:global::Metalama.Framework.Validation.InternalImplementAttribute"
            ],
            aspectClasses );

        var aspectClassesChanges = 0;
        aspectDatabase.AspectClassesChanged += _ => aspectClassesChanges++;

        code.Add( "another.cs", anotherAspect );

        workspaceProvider.AddOrUpdateProject( "project", code );

        Assert.True( factory.ServiceProvider.GetRequiredService<DesignTimeAspectPipelineFactory>().TryGetPipeline( projectKey, out var pipeline ) );

        // Have to recompute configuration to get the event raised.
        await pipeline.GetConfigurationAsync(
            PartialCompilation.CreateComplete( await workspaceProvider.GetCompilationAsync( projectKey ) ),
            ignoreStatus: true,
            AsyncExecutionContext.Get(),
            default );

        Assert.Equal( 1, aspectClassesChanges );

        aspectClasses = await aspectDatabase.GetAspectClassesAsync( projectKey, default );

        AssertAspectClasses(
            [
                "Y:global::AnotherAspect",
                "Y:global::Aspect",
                "Y:global::Metalama.Framework.Validation.InternalImplementAttribute"
            ],
            aspectClasses );

        var aspectClass = new SerializableTypeId( "Y:global::Aspect" );

        var aspectInstances = await aspectDatabase.GetAspectInstancesAsync( projectKey, "project", aspectClass, default );

        AssertAspectInstances( ["T:Target"], aspectInstances );

        var aspectInstancesChanges = 0;
        aspectDatabase.AspectInstancesChanged += _ => aspectInstancesChanges++;

        code.Add( "new.cs", newTarget );

        workspaceProvider.AddOrUpdateProject( "project", code );

        // Have to re-execute to get the event raised.
        await pipeline.ExecuteAsync( await workspaceProvider.GetCompilationAsync( projectKey ), AsyncExecutionContext.Get() );

        Assert.Equal( 1, aspectClassesChanges );
        Assert.Equal( 1, aspectInstancesChanges );

        aspectInstances = await aspectDatabase.GetAspectInstancesAsync( projectKey, "project", aspectClass, default );

        AssertAspectInstances( ["T:NewTarget", "T:Target"], aspectInstances );
    }

    [Fact]
    public async Task MultipleAspectsSameNameTest()
    {
        static string GetCode( int i )
            => $$"""
                 using Metalama.Framework.Advising;
                 using Metalama.Framework.Aspects; 
                 using Metalama.Framework.Code;

                 class Aspect : OverrideMethodAspect
                 {
                     public override dynamic? OverrideMethod()
                     {
                         return meta.Proceed();
                     }
                 }

                 class Target{{i}}
                 {
                     [Aspect]
                     void M() { }
                 }
                 """;

        using var testContext = this.CreateTestContext();
        using TestDesignTimeAspectPipelineFactory factory = new( testContext );

        var workspaceProvider = factory.ServiceProvider.GetRequiredService<TestWorkspaceProvider>();

        var projectKey1 = workspaceProvider.AddOrUpdateProject( "project1", new Dictionary<string, string> { ["code1.cs"] = GetCode( 1 ) } );
        var projectKey2 = workspaceProvider.AddOrUpdateProject( "project2", new Dictionary<string, string> { ["code2.cs"] = GetCode( 2 ) } );

        var aspectDatabase = new AspectDatabase( factory.ServiceProvider );

        var aspectClasses1 = await aspectDatabase.GetAspectClassesAsync( projectKey1, default );
        AssertAspectClasses( ["Y:global::Aspect", "Y:global::Metalama.Framework.Validation.InternalImplementAttribute"], aspectClasses1 );

        var aspectClasses2 = await aspectDatabase.GetAspectClassesAsync( projectKey2, default );
        AssertAspectClasses( ["Y:global::Aspect", "Y:global::Metalama.Framework.Validation.InternalImplementAttribute"], aspectClasses2 );

        var aspectInstances1 = await aspectDatabase.GetAspectInstancesAsync( projectKey1, "project1", new SerializableTypeId( "Y:global::Aspect" ), default );
        AssertAspectInstances( ["M:Target1.M"], aspectInstances1 );

        var aspectInstances2 = await aspectDatabase.GetAspectInstancesAsync( projectKey2, "project2", new SerializableTypeId( "Y:global::Aspect" ), default );
        AssertAspectInstances( ["M:Target2.M"], aspectInstances2 );
    }

    [Fact]
    public async Task IntroduceParameterTransformation()
    {
        const string code =
            """
            using Metalama.Framework.Advising;
            using Metalama.Framework.Aspects; 
            using Metalama.Framework.Code;
            using System.Linq;

            class MyAspect : TypeAspect
            {
                public override void BuildAspect(IAspectBuilder<INamedType> builder)
                {
                    base.BuildAspect(builder);
            
                    builder.Advice.IntroduceParameter(builder.Target.Constructors.Single(), "p", typeof(int), TypedConstant.Create(0));
                }
            }

            [MyAspect]
            class WithoutCtor
            {
            }

            [MyAspect]
            class WithCtor
            {
                public WithCtor()
                {
                }
            }
            """;

        using var testContext = this.CreateTestContext();
        using TestDesignTimeAspectPipelineFactory factory = new( testContext );

        var workspaceProvider = factory.ServiceProvider.GetRequiredService<TestWorkspaceProvider>();
        var projectKey = workspaceProvider.AddOrUpdateProject( "project", new Dictionary<string, string> { ["code.cs"] = code } );

        var aspectDatabase = new AspectDatabase( factory.ServiceProvider );

        var aspectInstances = await aspectDatabase.GetAspectInstancesAsync( projectKey, "project", new SerializableTypeId( "Y:global::MyAspect" ), default );

#pragma warning disable IDE0053 // Use expression body for lambda expression
        Assert.Collection(
            aspectInstances.OrderBy( i => i.TargetDeclarationId ),
            aspectInstance =>
            {
                Assert.Equal( "T:WithCtor", aspectInstance.TargetDeclarationId );

                Assert.Collection(
                    aspectInstance.Transformations,

                    // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
                    transformation =>
                    {
                        Assert.Equal( "Introduce parameter 'p' into 'WithCtor.WithCtor()'.", transformation.Description );
                        Assert.Equal( "M:WithCtor.#ctor", transformation.TargetDeclarationId );
                    } );
            },
            aspectInstance =>
            {
                Assert.Equal( "T:WithoutCtor", aspectInstance.TargetDeclarationId );

                Assert.Collection(
                    aspectInstance.Transformations.OrderBy( i => i.TargetDeclarationId ),

                    // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
                    transformation =>
                    {
                        Assert.Equal( "Introduce parameter 'p' into 'WithoutCtor.WithoutCtor()'.", transformation.Description );
                        Assert.Equal( "M:WithoutCtor.#ctor", transformation.TargetDeclarationId );
                    },

                    // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
                    transformation =>
                    {
                        Assert.Equal( "Introduce constructor 'WithoutCtor.WithoutCtor()'.", transformation.Description );
                    } );
            } );
#pragma warning restore IDE0053
    }
}