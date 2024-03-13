// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Observers;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Utilities.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MethodBase = Metalama.Framework.Engine.CodeModel.MethodBase;
using SpecialType = Metalama.Framework.Code.SpecialType;
using TypedConstant = Metalama.Framework.Code.TypedConstant;

#if DEBUG
using Metalama.Framework.Engine.Formatting;
#endif

namespace Metalama.Framework.Engine.Linking;

/// <summary>
/// Aspect linker injection steps. Adds introduced members from all transformation to the Roslyn compilation. This involves calling template expansion.
/// This results in the transformation registry and intermediate compilation, and also produces diagnostics.
/// </summary>
internal sealed partial class LinkerInjectionStep : AspectLinkerPipelineStep<AspectLinkerInput, LinkerInjectionStepOutput>
{
    private readonly ProjectServiceProvider _serviceProvider;
    private readonly CompilationContext _compilationContext;
    private readonly IConcurrentTaskRunner _concurrentTaskRunner;

    public LinkerInjectionStep( ProjectServiceProvider serviceProvider, CompilationContext compilationContext )
    {
        this._serviceProvider = serviceProvider;
        this._compilationContext = compilationContext;
        this._concurrentTaskRunner = serviceProvider.GetRequiredService<IConcurrentTaskRunner>();
    }

    public override async Task<LinkerInjectionStepOutput> ExecuteAsync( AspectLinkerInput input, CancellationToken cancellationToken )
    {
        // TODO: Consider parallelizing based on containing type and not syntax tree. This would remove non-determinism in name selection.

        // We don't use a code fix filter because the linker is not supposed to suggest code fixes. If that changes, we need to pass a filter.
        var diagnostics = new UserDiagnosticSink( input.CompileTimeProject, null );

        var supportsNullability = input.CompilationModel.RoslynCompilation.Options.NullableContextOptions != NullableContextOptions.Disable;

        var transformationComparer = TransformationLinkerOrderComparer.Instance;
        var injectionHelperProvider = new LinkerInjectionHelperProvider( input.CompilationModel, supportsNullability );
        var injectionNameProvider = new LinkerInjectionNameProvider( input.CompilationModel, injectionHelperProvider, OurSyntaxGenerator.Default );
        var transformationCollection = new TransformationCollection( input.CompilationModel, transformationComparer );
        var lexicalScopeFactory = new LexicalScopeFactory( input.CompilationModel );
        var aspectReferenceSyntaxProvider = new LinkerAspectReferenceSyntaxProvider();

        HashSet<IIntroduceDeclarationTransformation> replacedIntroduceDeclarationTransformations = new();
        HashSet<PropertyBuilder> buildersWithSynthesizedSetters = new();
        ConcurrentDictionary<IMember, InsertStatementTransformationContextImpl> pendingInsertStatementContexts = new( input.CompilationModel.Comparers.Default );
        ConcurrentDictionary<IMember, AuxiliaryMemberTransformations> auxiliaryMemberTransformations = new( input.CompilationModel.Comparers.Default );

        void IndexTransformationsInSyntaxTree( IGrouping<SyntaxTree, ITransformation> transformationGroup )
        {
            // Transformations need to be sorted here because some transformations require a LexicalScope to get an unique name, and it
            // will give deterministic results only when called in a deterministic order.
            var sortedTransformations = transformationGroup.OrderBy( x => x, transformationComparer ).ToArray();

            // IntroduceDeclarationTransformation instances need to be indexed first.
            foreach ( var transformation in sortedTransformations )
            {
                IndexIntroduceDeclarationTransformation( transformation, transformationCollection );
            }

            // Replace transformations need to be indexed second.
            // NOTE: This is correct because replaced transformation is always in the same syntax tree as the replacing one.
            foreach ( var transformation in sortedTransformations )
            {
                IndexReplaceTransformation(
                    input,
                    transformation,
                    transformationCollection,
                    replacedIntroduceDeclarationTransformations );
            }

            foreach ( var transformation in sortedTransformations )
            {
                IndexOverrideTransformation(
                    transformation,
                    transformationCollection,
                    auxiliaryMemberTransformations,
                    buildersWithSynthesizedSetters,
                    pendingInsertStatementContexts );

                this.IndexInjectTransformation(
                    input,
                    transformation,
                    diagnostics,
                    lexicalScopeFactory,
                    injectionNameProvider,
                    aspectReferenceSyntaxProvider,
                    buildersWithSynthesizedSetters,
                    transformationCollection,
                    replacedIntroduceDeclarationTransformations );

                IndexMemberLevelTransformation(
                    transformation,
                    transformationCollection );

                this.IndexInsertStatementTransformation(
                    input,
                    diagnostics,
                    lexicalScopeFactory,
                    transformation,
                    transformationCollection,
                    auxiliaryMemberTransformations,
                    pendingInsertStatementContexts );

                IndexNodesWithModifiedAttributes( transformation, transformationCollection );
            }
        }

        // It's imperative that order of transformations is preserved while grouping by syntax tree.
        // The syntax tree we group by must be the main syntax tree of the enclosing type. We should never run transformations
        // of a partial type in parallel.
        var transformationsByCanonicalSyntaxTree = input.Transformations.GroupBy( GetCanonicalSyntaxTree );

        static SyntaxTree GetCanonicalSyntaxTree( ITransformation transformation )
        {
            return GetCanonicalTargetDeclaration( transformation.TargetDeclaration ) switch
            {
                INamedType namedType => namedType.GetPrimarySyntaxTree().AssertNotNull(),
                ICompilation compilation => transformation.TransformedSyntaxTree,
                var t => throw new AssertionFailedException( $"Unsupported: {t.DeclarationKind}" ),
            };

            static IDeclaration GetCanonicalTargetDeclaration( IDeclaration declaration )
                => declaration switch
                {
                    IMember member => member.DeclaringType,
                    INamedType type => type,
                    IParameter parameter => GetCanonicalTargetDeclaration( parameter.ContainingDeclaration.AssertNotNull() ),
                    ICompilation compilation => compilation,
                    var t => throw new AssertionFailedException( $"Unsupported: {t.DeclarationKind}" ),
                };
        }

        await this._concurrentTaskRunner.RunInParallelAsync( transformationsByCanonicalSyntaxTree, IndexTransformationsInSyntaxTree, cancellationToken );

        // Finalize non-auxiliary transformations (sorting).
        await transformationCollection.FinalizeAsync(
            this._concurrentTaskRunner,
            cancellationToken );

        void FlushPendingInsertStatementContext(KeyValuePair<IMember, InsertStatementTransformationContextImpl> pair)
        {
            if ( RequiresAuxiliaryContractMember(pair.Key, pair.Value) )
            {
                pair.Value.Complete();

                transformationCollection.AddTransformationCausingAuxiliaryOverride( pair.Value.OriginTransformation );

                // This may be the only "override" present, so make sure all other effects of overrides are present.
                AddSynthesizedSetterForPropertyIfRequired( pair.Key, transformationCollection, buildersWithSynthesizedSetters );

                auxiliaryMemberTransformations
                    .GetOrAdd( pair.Key, _ => new() )
                    .InjectAuxiliaryContractMember(
                        pair.Value.OriginTransformation,
                        pair.Value.ReturnValueVariableName );
            }
        }

        await this._concurrentTaskRunner.RunInParallelAsync( pendingInsertStatementContexts, FlushPendingInsertStatementContext, cancellationToken );

        // Process any auxiliary member transformations in parallel.
        void ProcessAuxiliaryMemberTransformations( KeyValuePair<IMember, AuxiliaryMemberTransformations> transformationPair)
        {
            var member = transformationPair.Key;
            var transformations = transformationPair.Value;

            this.IndexAuxiliaryMemberTransformations(
                input.CompilationModel,
                transformationCollection,
                lexicalScopeFactory,
                aspectReferenceSyntaxProvider,
                injectionNameProvider,
                injectionHelperProvider,
                member,
                transformations );
        }

        await this._concurrentTaskRunner.RunInParallelAsync( auxiliaryMemberTransformations, ProcessAuxiliaryMemberTransformations, cancellationToken );

        var syntaxTreeForGlobalAttributes = input.CompilationModel.PartialCompilation.SyntaxTreeForCompilationLevelAttributes;

        // Group diagnostic suppressions by target.
        var suppressionsByTarget = input.DiagnosticSuppressions.ToMultiValueDictionary(
            s => s.Declaration,
            input.CompilationModel.Comparers.Default );

        // Replace wildcard AssemblyVersionAttribute with actual version.
        var attributes = input.CompilationModel.GetAttributeCollection( input.CompilationModel.ToRef() );
        var assemblyVersionAttributeType = (INamedType) input.CompilationModel.Factory.GetTypeByReflectionType( typeof(AssemblyVersionAttribute) );
        var assemblyVersionAttribute = input.CompilationModel.Attributes.OfAttributeType( assemblyVersionAttributeType ).FirstOrDefault();

#pragma warning disable CA1307 // Specify StringComparison for clarity
        if ( assemblyVersionAttribute?.ConstructorArguments.FirstOrDefault() is { Value: string version }
             && version.Contains( '*' ) )
        {
            attributes.Remove( assemblyVersionAttributeType );

            // It's hacky to add an AttributeBuilder with null Advice, but it seems to work fine.
            // We avoid to use user APIs that require a user code execution context.
            var assemblyVersionAttributeConstructor =
                assemblyVersionAttributeType.Constructors.Single( x => x.Parameters is [{ Type.SpecialType: SpecialType.String }] );

            var newAssemblyVersionAttribute =
                new StandaloneAttributeData( assemblyVersionAttributeConstructor )
                {
                    ConstructorArguments = ImmutableArray.Create(
                        TypedConstant.Create(
                            input.CompilationModel.RoslynCompilation.Assembly.Identity.Version.ToString(),
                            assemblyVersionAttributeConstructor.Parameters[0].Type ) )
                };

            attributes.Add(
                new AttributeBuilder(
                    null!,
                    input.CompilationModel.DeclaringAssembly,
                    newAssemblyVersionAttribute ) );
        }
#pragma warning restore CA1307

        // Rewrite syntax trees.
        var intermediateCompilation = input.CompilationModel.PartialCompilation;
        var transformations = new ConcurrentBag<SyntaxTreeTransformation>();

        async Task RewriteSyntaxTreeAsync( SyntaxTree initialSyntaxTree )
        {
            Rewriter rewriter = new(
                this._compilationContext,
                transformationCollection,
                suppressionsByTarget,
                input.CompilationModel,
                syntaxTreeForGlobalAttributes );

            var oldRoot = await initialSyntaxTree.GetRootAsync( cancellationToken );
            var newRoot = rewriter.Visit( oldRoot ).AssertNotNull();

            if ( oldRoot != newRoot )
            {
                var intermediateSyntaxTree = initialSyntaxTree.WithRootAndOptions( newRoot, initialSyntaxTree.Options );

                transformations.Add( SyntaxTreeTransformation.ReplaceTree( initialSyntaxTree, intermediateSyntaxTree ) );
            }
        }

        await this._concurrentTaskRunner.RunInParallelAsync( intermediateCompilation.SyntaxTrees.Values, RewriteSyntaxTreeAsync, cancellationToken );

        var helperSyntaxTree = injectionHelperProvider.GetLinkerHelperSyntaxTree( intermediateCompilation.LanguageOptions );
        transformations.Add( SyntaxTreeTransformation.AddTree( helperSyntaxTree ) );

        intermediateCompilation = intermediateCompilation.Update( transformations );

        // Report the linker intermediate compilation to tooling/tests.
        this._serviceProvider.GetService<ILinkerObserver>()
            ?.OnIntermediateCompilationCreated( intermediateCompilation );

        var injectionRegistry = new LinkerInjectionRegistry(
            transformationComparer,
            input.CompilationModel,
            intermediateCompilation,
            transformations,
            transformationCollection.InjectedMembers,
            transformationCollection.BuilderToTransformationMap,
            transformationCollection.IntroducedParametersByTargetDeclaration,
            transformationCollection.TransformationsCausingAuxiliaryOverrides,
            this._concurrentTaskRunner,
            cancellationToken );

        var lateTransformationRegistry = 
            new LinkerLateTransformationRegistry(
                intermediateCompilation,
                transformationCollection.LateTypeLevelTransformations );

        var projectOptions = this._serviceProvider.GetService<IProjectOptions>();

        return
            new LinkerInjectionStepOutput(
                diagnostics,
                input.CompilationModel,
                intermediateCompilation,
                injectionRegistry,
                lateTransformationRegistry,
                input.OrderedAspectLayers,
                projectOptions );
    }

    private static void IndexNodesWithModifiedAttributes(
        ITransformation transformation,
        TransformationCollection transformationCollection )
    {
        // We only need to index transformations on syntax (i.e. on source code) because introductions on generated code
        // are taken from the compilation model.

        // Note: Compilation-level attributes will not be indexed because the containing declaration has no
        // syntax reference.

        if ( transformation is IntroduceAttributeTransformation introduceAttributeTransformation )
        {
            foreach ( var declaringSyntax in introduceAttributeTransformation.TargetDeclaration.GetDeclaringSyntaxReferences() )
            {
                transformationCollection.AddNodeWithModifiedAttributes( declaringSyntax.GetSyntax() );
            }
        }
        else if ( transformation is RemoveAttributesTransformation removeAttributesTransformation )
        {
            foreach ( var declaringSyntax in removeAttributesTransformation.ContainingDeclaration.GetDeclaringSyntaxReferences() )
            {
                transformationCollection.AddNodeWithModifiedAttributes( declaringSyntax.GetSyntax() );
            }
        }
    }

    private static void IndexIntroduceDeclarationTransformation( ITransformation transformation, TransformationCollection transformationCollection )
    {
        if ( transformation is IIntroduceDeclarationTransformation introduceDeclarationTransformation )
        {
            transformationCollection.AddIntroduceTransformation(
                introduceDeclarationTransformation.DeclarationBuilder,
                introduceDeclarationTransformation );
        }
    }

    private static void IndexReplaceTransformation(
        AspectLinkerInput input,
        ITransformation transformation,
        TransformationCollection transformationCollection,
        HashSet<IIntroduceDeclarationTransformation> replacedIntroduceDeclarationTransformations )
    {
        var compilation = input.CompilationModel;

        if ( transformation is not IReplaceMemberTransformation replaceMemberTransformation )
        {
            return;
        }

        {
            if ( replaceMemberTransformation.ReplacedMember.IsDefault )
            {
                return;
            }

            // We want to get the replaced member as it is in the compilation of the transformation, i.e. with applied redirections up to that point.
            // TODO: the target may have been removed from the
            var replacedDeclaration = (IDeclaration) replaceMemberTransformation.ReplacedMember.GetTarget(
                compilation,
                ReferenceResolutionOptions.DoNotFollowRedirections );

            replacedDeclaration = replacedDeclaration switch
            {
                BuiltDeclaration declaration => declaration.Builder,
                _ => replacedDeclaration
            };

            switch ( replacedDeclaration )
            {
                case Field replacedField:
                    var fieldSyntaxReference =
                        replacedField.Symbol.GetPrimarySyntaxReference()
                        ?? throw new AssertionFailedException( $"The field '{replacedField.Symbol}' does not have syntax." );

                    var removedFieldSyntax = fieldSyntaxReference.GetSyntax();
                    transformationCollection.AddRemovedSyntax( removedFieldSyntax );

                    break;

                case Constructor replacedConstructor:
                    Invariant.Assert( replacedConstructor.Symbol.GetPrimarySyntaxReference() == null );

                    break;

                // This needs to point to an interface
                case IDeclarationBuilder replacedBuilder:
                    if ( !transformationCollection.TryGetIntroduceDeclarationTransformation( replacedBuilder, out var introduceDeclarationTransformation ) )
                    {
                        throw new AssertionFailedException( $"Builder {replacedBuilder} is missing registered transformation." );
                    }

                    lock ( replacedIntroduceDeclarationTransformations )
                    {
                        replacedIntroduceDeclarationTransformations.Add( introduceDeclarationTransformation );
                    }

                    break;

                default:
                    throw new AssertionFailedException( $"Unexpected replace declaration: '{replacedDeclaration}'." );
            }
        }
    }

    private void IndexInjectTransformation(
        AspectLinkerInput input,
        ITransformation transformation,
        UserDiagnosticSink diagnostics,
        LexicalScopeFactory lexicalScopeFactory,
        LinkerInjectionNameProvider nameProvider,
        LinkerAspectReferenceSyntaxProvider aspectReferenceSyntaxProvider,
        IReadOnlyCollection<PropertyBuilder> buildersWithSynthesizedSetters,
        TransformationCollection transformationCollection,
        HashSet<IIntroduceDeclarationTransformation> replacedIntroduceDeclarationTransformations )
    {
        {
            if ( transformation is IIntroduceDeclarationTransformation introduceDeclarationTransformation )
            {
                lock ( replacedIntroduceDeclarationTransformations )
                {
                    if ( replacedIntroduceDeclarationTransformations.Contains( introduceDeclarationTransformation ) )
                    {
                        return;
                    }
                }
            }

            switch ( transformation )
            {
                case IInjectMemberTransformation injectMemberTransformation:
                    // Transformed syntax tree must match insert position.
                    Invariant.Assert( injectMemberTransformation.TransformedSyntaxTree == injectMemberTransformation.InsertPosition.SyntaxNode.SyntaxTree );

                    // Create the SyntaxGenerationContext for the insertion point.
                    var positionInSyntaxTree = GetSyntaxTreePosition( injectMemberTransformation.InsertPosition );

                    var syntaxGenerationContext = this._compilationContext.GetSyntaxGenerationContext(
                        injectMemberTransformation.TransformedSyntaxTree,
                        positionInSyntaxTree );

                    // TODO: It smells that we pass original compilation here. Should be the compilation for the transformation.
                    //       For introduction, this should be a compilation that INCLUDES the builder.
                    //       But, if we pass the mutable compilation, it will get changed before the template is expanded.
                    //       The expanded template should not see declarations added after it runs.

                    // Call GetInjectedMembers.
                    var injectionContext = new MemberInjectionContext(
                        this._serviceProvider,
                        diagnostics,
                        nameProvider,
                        aspectReferenceSyntaxProvider,
                        lexicalScopeFactory,
                        syntaxGenerationContext,
                        input.CompilationModel );

                    var injectedMembers = injectMemberTransformation.GetInjectedMembers( injectionContext );

                    injectedMembers = PostProcessInjectedMembers( injectedMembers );

                    transformationCollection.AddInjectedMembers( injectMemberTransformation, injectedMembers );

                    break;

                case IInjectInterfaceTransformation injectInterfaceTransformation:
                    var introducedInterface = injectInterfaceTransformation.GetSyntax();
                    transformationCollection.AddInjectedInterface( injectInterfaceTransformation, introducedInterface );

                    break;
            }

            IEnumerable<InjectedMember> PostProcessInjectedMembers( IEnumerable<InjectedMember> injectedMembers )
            {
                if ( transformation is IntroducePropertyTransformation introducePropertyTransformation )
                {
                    bool hasSynthesizedSetter;

                    lock ( buildersWithSynthesizedSetters )
                    {
                        hasSynthesizedSetter = buildersWithSynthesizedSetters.Contains( introducePropertyTransformation.IntroducedDeclaration );
                    }

                    if ( hasSynthesizedSetter )
                    {
                        // This is a property which should have a synthesized setter added.
                        injectedMembers =
                            injectedMembers
                                .Select(
                                    im =>
                                    {
                                        switch ( im )
                                        {
                                            // ReSharper disable once MissingIndent
                                            case
                                            {
                                                Semantic: InjectedMemberSemantic.Introduction, Kind: DeclarationKind.Property,
                                                Syntax: PropertyDeclarationSyntax propertyDeclaration
                                            }:
                                                return im.WithSyntax(
                                                    propertyDeclaration.WithSynthesizedSetter( this._compilationContext.DefaultSyntaxGenerationContext ) );

                                            case { Semantic: InjectedMemberSemantic.InitializerMethod }:
                                                return im;

                                            default:
                                                throw new AssertionFailedException( $"Unexpected semantic for '{im.Declaration}'." );
                                        }
                                    } );
                    }
                }

                return injectedMembers;
            }
        }
    }

    private static int GetSyntaxTreePosition( InsertPosition insertPosition )
        => insertPosition.Relation switch
        {
            InsertPositionRelation.After => insertPosition.SyntaxNode.Span.End + 1,
            InsertPositionRelation.Within => ((BaseTypeDeclarationSyntax) insertPosition.SyntaxNode).CloseBraceToken.Span.Start - 1,
            _ => 0
        };

    private static void IndexOverrideTransformation(
        ITransformation transformation,
        TransformationCollection transformationCollection,
        ConcurrentDictionary<IMember, AuxiliaryMemberTransformations> auxiliaryMemberTransformations,
        HashSet<PropertyBuilder> buildersWithSynthesizedSetters,
        ConcurrentDictionary<IMember, InsertStatementTransformationContextImpl> pendingInsertStatementContexts )
    {
        if ( transformation is not IOverrideDeclarationTransformation overrideDeclarationTransformation )
        {
            return;
        }

        AddSynthesizedSetterForPropertyIfRequired(
            overrideDeclarationTransformation.OverriddenDeclaration,
            transformationCollection,
            buildersWithSynthesizedSetters );

        if (overrideDeclarationTransformation.OverriddenDeclaration is IConstructor { IsPrimary: true } overriddenConstructor)
        {
            auxiliaryMemberTransformations.GetOrAdd( overriddenConstructor, _ => new() ).InjectAuxiliarySourceMember();
            transformationCollection.GetOrAddLateTypeLevelTransformations( overriddenConstructor.DeclaringType ).RemovePrimaryConstructor();
        }

        if ( overrideDeclarationTransformation.OverriddenDeclaration is IMember overriddenMember
             && pendingInsertStatementContexts.TryGetValue( overriddenMember, out var insertStatementContext ) )
        {
            // Remove context for the insert statement (there should be no race since we parallelize based on containing type).
            pendingInsertStatementContexts.TryRemove( overriddenMember, out _ );

            // Auxiliary member is needed for output contracts
            if ( RequiresAuxiliaryContractMember( overriddenMember, insertStatementContext ) )
            {
                insertStatementContext.Complete();

                transformationCollection.AddTransformationCausingAuxiliaryOverride( insertStatementContext.OriginTransformation );

                auxiliaryMemberTransformations
                    .GetOrAdd( overriddenMember, _ => new() )
                    .InjectAuxiliaryContractMember(
                        insertStatementContext.OriginTransformation,
                        insertStatementContext.ReturnValueVariableName );
            }
        }
    }

    private static void AddSynthesizedSetterForPropertyIfRequired(
        IDeclaration overriddenDeclaration, 
        TransformationCollection transformationCollection, 
        HashSet<PropertyBuilder> buildersWithSynthesizedSetters )
    {         
        // If this is an auto-property that does not override a base property, we can add synthesized init-only setter.
        // If this is overridden property we need to:
        //  1) Block inlining of the first override (force the trampoline).
        //  2) Substitute all sets of the property (can be only in constructors) to use the first override instead.
        if ( overriddenDeclaration is IProperty
            {
                IsAutoPropertyOrField: true, Writeability: Writeability.ConstructorOnly, SetMethod.IsImplicitlyDeclared: true,
                OverriddenProperty: null or { SetMethod: not null }
            } overriddenAutoProperty )
        {
            switch ( overriddenAutoProperty )
            {
                case Property codeProperty:
                    transformationCollection.AddAutoPropertyWithSynthesizedSetter(
                        (PropertyDeclarationSyntax) codeProperty.GetPrimaryDeclarationSyntax().AssertNotNull() );

                    break;

                case BuiltProperty { PropertyBuilder: var builder }:
                    lock ( buildersWithSynthesizedSetters )
                    {
                        buildersWithSynthesizedSetters.Add( builder.AssertNotNull() );
                    }

                    break;

                case PropertyBuilder builder:
                    lock ( buildersWithSynthesizedSetters )
                    {
                        buildersWithSynthesizedSetters.Add( builder.AssertNotNull() );
                    }

                    break;

                default:
                    throw new AssertionFailedException( $"Unexpected declaration: '{overriddenAutoProperty}'." );
            }
        }
    }

    private void IndexInsertStatementTransformation(
        AspectLinkerInput input,
        UserDiagnosticSink diagnostics,
        LexicalScopeFactory lexicalScopeFactory,
        ITransformation transformation,
        TransformationCollection transformationCollection,
        ConcurrentDictionary<IMember, AuxiliaryMemberTransformations> auxiliaryMemberTransformations,
        ConcurrentDictionary<IMember, InsertStatementTransformationContextImpl> pendingInsertStatementContexts )
    {
        if ( transformation is not IInsertStatementTransformation insertStatementTransformation )
        {
            return;
        }

        switch ( insertStatementTransformation.TargetMember )
        {
            case IPropertyOrIndexer propertyOrIndexer:
                {
                    SyntaxNode primaryDeclaration;
                    SyntaxGenerationContext syntaxGenerationContext;

                    switch ( propertyOrIndexer)
                    {
                        case PropertyOrIndexer sourcePropertyOrIndexer:
                            primaryDeclaration = sourcePropertyOrIndexer.GetPrimaryDeclarationSyntax().AssertNotNull();
                            syntaxGenerationContext = this._compilationContext.GetSyntaxGenerationContext( primaryDeclaration );
                            break;

                        default:
                            var propertyOrIndexerBuilder = propertyOrIndexer as PropertyOrIndexerBuilder
                                ?? (PropertyOrIndexerBuilder) ((BuiltPropertyOrIndexer) insertStatementTransformation.TargetMember).Builder;

                            var positionInSyntaxTree = GetSyntaxTreePosition( propertyOrIndexerBuilder.ToInsertPosition() );

                            syntaxGenerationContext = this._compilationContext.GetSyntaxGenerationContext(
                                propertyOrIndexerBuilder.PrimarySyntaxTree.AssertNotNull(),
                                positionInSyntaxTree );

                            propertyOrIndexer = propertyOrIndexerBuilder;

                            break;
                    }

                    var insertedStatements = GetInsertedStatements( insertStatementTransformation, syntaxGenerationContext );

                    if ( propertyOrIndexer.GetMethod != null )
                    {
                        transformationCollection.AddInsertedStatements(
                            propertyOrIndexer.GetMethod,
                            insertedStatements
                            .Where( s =>
                                s.ContextDeclaration.IsContainedIn( propertyOrIndexer.GetMethod )
                                || (propertyOrIndexer is IIndexer indexer && s.ContextDeclaration is IParameter parameter && parameter.ContainingDeclaration == indexer) )
                            .ToList() );
                    }

                    if ( propertyOrIndexer.SetMethod != null )
                    {
                        transformationCollection.AddInsertedStatements(
                            propertyOrIndexer.SetMethod,
                            insertedStatements
                            .Where( s =>
                                s.ContextDeclaration.IsContainedIn( propertyOrIndexer.SetMethod )
                                || (propertyOrIndexer is IIndexer indexer && s.ContextDeclaration is IParameter parameter && parameter.ContainingDeclaration == indexer) )
                            .ToList() );
                    }

                    break;
                }

            case IMethodBase methodBase:
                {
                    SyntaxNode primaryDeclaration;
                    SyntaxGenerationContext syntaxGenerationContext;

                    switch ( methodBase )
                    {
                        case MethodBase sourceMethodBase:
                            primaryDeclaration = sourceMethodBase.GetPrimaryDeclarationSyntax().AssertNotNull();
                            syntaxGenerationContext = this._compilationContext.GetSyntaxGenerationContext( primaryDeclaration );
                            break;

                        default:
                            var methodBaseBuilder = methodBase as MethodBaseBuilder
                                ?? (MethodBaseBuilder) ((BuiltMethodBase) insertStatementTransformation.TargetMember).Builder;

                            var positionInSyntaxTree = GetSyntaxTreePosition( methodBaseBuilder.ToInsertPosition() );

                            syntaxGenerationContext = this._compilationContext.GetSyntaxGenerationContext(
                                methodBaseBuilder.PrimarySyntaxTree.AssertNotNull(),
                                positionInSyntaxTree );

                            methodBase = methodBaseBuilder;

                            break;
                    }

                    var insertedStatements = GetInsertedStatements( insertStatementTransformation, syntaxGenerationContext );

                    transformationCollection.AddInsertedStatements( methodBase, insertedStatements );

                    break;
                }

            default:
                throw new AssertionFailedException( $"Unexpected target: {insertStatementTransformation.TargetMember}." );
        }

        if ( insertStatementTransformation.TargetMember is IConstructor { IsPrimary: true } overriddenConstructor )
        {
            auxiliaryMemberTransformations.GetOrAdd( overriddenConstructor, _ => new() ).InjectAuxiliarySourceMember();
            transformationCollection.GetOrAddLateTypeLevelTransformations( overriddenConstructor.DeclaringType ).RemovePrimaryConstructor();
        }

        IReadOnlyList<InsertedStatement> GetInsertedStatements(
            IInsertStatementTransformation insertStatementTransformation,
            SyntaxGenerationContext syntaxGenerationContext )
        {
            // Contexts for inserting statements are reused until the next override of the target declaration.
            var context =
                pendingInsertStatementContexts.GetOrAdd(
                    insertStatementTransformation.TargetMember,
                    m => new InsertStatementTransformationContextImpl(
                        this._serviceProvider,
                        diagnostics,
                        syntaxGenerationContext,
                        input.CompilationModel,
                        lexicalScopeFactory,
                        insertStatementTransformation,
                        m ) );

            var statements = insertStatementTransformation.GetInsertedStatements( context );

            var markedForInputContracts = false;
            var markedForOutputContracts = false;

            foreach ( var statement in statements )
            {
                if ( !markedForInputContracts && statement.Kind == InsertedStatementKind.InputContract)
                {
                    markedForInputContracts = true;
                    context.MarkAsUsedForInputContracts();
                }

                if ( !markedForOutputContracts && statement.Kind == InsertedStatementKind.OutputContract )
                {
                    markedForOutputContracts = true;
                    context.MarkAsUsedForOutputContracts();

                    if ( insertStatementTransformation.TargetMember is IProperty or IIndexer
                        || (insertStatementTransformation.TargetMember is IMethod method && method.GetAsyncInfo().ResultType.SpecialType != SpecialType.Void) )
                    {
                        // Force the return variable name to be allocated if the return type is not void.
                        // If there are output contracts that don't use the return value, the return value is still required.
                        // If not done here, the return value would be allocated later, leading to unintuitive variable name ordinals.
                        _ = context.GetReturnValueVariableName();
                    }
                }

                if (markedForInputContracts && markedForOutputContracts)
                {
                    break;
                }
            }

#if DEBUG
            foreach ( var statement in statements )
            {
                if ( statement.Statement is BlockSyntax block )
                {
                    if ( !block.Statements.All( s => s.HasAnnotations( FormattingAnnotations.GeneratedCodeAnnotationKind ) ) )
                    {
                        throw new AssertionFailedException( "GeneratedCodeAnnotationKind annotation missing." );
                    }
                }
                else
                {
                    if ( !statement.Statement.HasAnnotations( FormattingAnnotations.GeneratedCodeAnnotationKind ) )
                    {
                        throw new AssertionFailedException( "GeneratedCodeAnnotationKind annotation missing." );
                    }
                }
#endif
            }

            return statements;
        }
    }

    private static void IndexMemberLevelTransformation(
        ITransformation transformation,
        TransformationCollection transformationCollection )
    {
        if ( transformation is not IMemberLevelTransformation memberLevelTransformation )
        {
            return;
        }

        // TODO: Supports only constructors without overrides.
        //       Needs to be generalized for anything else (take into account overrides).

        MemberLevelTransformations? memberLevelTransformations;
        var declarationSyntax = memberLevelTransformation.TargetMember.GetPrimaryDeclarationSyntax();

        if ( declarationSyntax != null )
        {
            memberLevelTransformations = transformationCollection.GetOrAddMemberLevelTransformations( declarationSyntax );
        }
        else
        {
            var parentDeclarationBuilder = (memberLevelTransformation.TargetMember as DeclarationBuilder
                                            ?? (memberLevelTransformation.TargetMember as BuiltDeclaration)?.Builder)
                .AssertNotNull();

            memberLevelTransformations = transformationCollection.GetOrAddMemberLevelTransformations( parentDeclarationBuilder );
        }

        switch (transformation, memberLevelTransformation.TargetMember)
        {
            case (IntroduceParameterTransformation introduceParameterTransformation, _):
                memberLevelTransformations.Add( introduceParameterTransformation );
                transformationCollection.AddIntroducedParameter( introduceParameterTransformation );

                break;

            case (IntroduceConstructorInitializerArgumentTransformation appendArgumentTransformation, _):
                memberLevelTransformations.Add( appendArgumentTransformation );

                break;

            default:
                throw new AssertionFailedException( $"Unexpected combination: ('{transformation}', '{memberLevelTransformation.TargetMember}')." );
        }
    }

    private void IndexAuxiliaryMemberTransformations( 
        CompilationModel finalCompilationModel,
        TransformationCollection transformationCollection,
        LexicalScopeFactory lexicalScopeFactory,
        AspectReferenceSyntaxProvider aspectReferenceSyntaxProvider,
        LinkerInjectionNameProvider injectionNameProvider,
        LinkerInjectionHelperProvider injectionHelperProvider,
        IMember member,
        AuxiliaryMemberTransformations transformations )
    {
        var auxiliaryMemberFactory =
            new AuxiliaryMemberFactory(
                this._compilationContext,
                finalCompilationModel,
                lexicalScopeFactory,
                aspectReferenceSyntaxProvider,
                injectionNameProvider,
                injectionHelperProvider,
                transformationCollection );

        if ( transformations.ShouldInjectAuxiliarySourceMember )
        {
            switch ( member )
            {
                case IConstructor { IsPrimary: true } primaryConstructor:
                    transformationCollection.AddInjectedMember(
                        new InjectedMember(
                            null,
                            DeclarationKind.Constructor,
                            auxiliaryMemberFactory.GetAuxiliarySourceConstructor(primaryConstructor),
                            null,
                            InjectedMemberSemantic.AuxiliaryBody,
                            primaryConstructor ) );

                    break;
                default:
                    throw new AssertionFailedException( $"Unsupported: {member}" );
            }
        }

        foreach ( var (originTransformation, returnVariableName) in transformations.AuxiliaryContractMembers )
        {
            // Having a record in this list means that there is an output contract, which requires a trivial body that will act as a receiver of contracts.
            // Usually this means that there is an output contract, otherwise all input contracts are injected into the preceding override or source.
            // Example:
            // <input_contracts> 
            // var returnValue = meta.Proceed();
            // <output_contracts>
            // return returnValue;

            var advice = originTransformation.ParentAdvice;
            var compilationModel = (CompilationModel) originTransformation.ParentAdvice.SourceCompilation;

            var rootMember =
                member switch
                {
                    IMethod { ContainingDeclaration: IProperty property } => property,
                    IMethod { ContainingDeclaration: IIndexer indexer } => indexer,
                    _ => member,
                };

            // TODO: Ideally, entry + exit statements should be injected here, but it complicates the transformation collection and rewriter.
            //       This now generates "well-known" structure, which is recognized by the rewriter, which is quite ugly.
            //       TransformationCollection is not finalized at this point and now selects statements based on InjectedMember, which we are creating here.

            transformationCollection.AddInjectedMember(
                new InjectedMember(
                    originTransformation,
                    member.DeclarationKind,
                    auxiliaryMemberFactory.GetAuxiliaryContractMember( rootMember, compilationModel, advice, returnVariableName ),
                    advice.AspectLayerId,
                    InjectedMemberSemantic.AuxiliaryBody,
                    rootMember ) );
        }
    }

    private static bool RequiresAuxiliaryContractMember( IMember member, InsertStatementTransformationContextImpl insertStatementContext )
    {
        // TODO: This is not optimal for cases with no output contracts, because we need this only to have "an override" to force other transformations.
        //       But for these declarations, the auxiliary member is created always, even when there are no input contracts.
        return
            insertStatementContext.WasUsedForOutputContracts
            || (member is IFieldOrProperty { IsAutoPropertyOrField: true } 
                       or IMethod { ContainingDeclaration: IFieldOrProperty { IsAutoPropertyOrField: true } }
                       or IMethod { IsPartial: true, HasImplementation: false }
                && (insertStatementContext.WasUsedForInputContracts || insertStatementContext.WasUsedForOutputContracts));
    }
}