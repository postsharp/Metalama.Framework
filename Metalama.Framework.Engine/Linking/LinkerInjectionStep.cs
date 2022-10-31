﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Metalama.Framework.Code.DeclarationBuilders;
#if DEBUG
using Metalama.Framework.Engine.Formatting;
#endif

namespace Metalama.Framework.Engine.Linking
{
    /// <summary>
    /// Aspect linker injection steps. Adds introduced members from all transformation to the Roslyn compilation. This involves calling template expansion.
    /// This results in the transformation registry and intermediate compilation, and also produces diagnostics.
    /// </summary>
    internal partial class LinkerInjectionStep : AspectLinkerPipelineStep<AspectLinkerInput, LinkerInjectionStepOutput>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ITaskScheduler _taskScheduler;

        public LinkerInjectionStep( IServiceProvider serviceProvider )
        {
            serviceProvider.GetRequiredService<ServiceProviderMark>().RequireProjectWide();

            this._serviceProvider = serviceProvider;
            this._taskScheduler = serviceProvider.GetRequiredService<ITaskScheduler>();
        }

        public override async Task<LinkerInjectionStepOutput> ExecuteAsync( AspectLinkerInput input, CancellationToken cancellationToken )
        {
            // We don't use a code fix filter because the linker is not supposed to suggest code fixes. If that changes, we need to pass a filter.
            var diagnostics = new UserDiagnosticSink( input.CompileTimeProject, null );

            var transformationComparer = TransformationLinkerOrderComparer.Instance;
            var nameProvider = new LinkerInjectionNameProvider( input.CompilationModel );
            var syntaxTransformationCollection = new SyntaxTransformationCollection( transformationComparer );
            var lexicalScopeFactory = new LexicalScopeFactory( input.CompilationModel );

            var supportsNullability = input.InitialCompilation.InitialCompilation.Options.NullableContextOptions != NullableContextOptions.Disable;

            var aspectReferenceSyntaxProvider = new LinkerAspectReferenceSyntaxProvider( supportsNullability );

            ConcurrentSet<IIntroduceDeclarationTransformation> replacedIntroduceDeclarationTransformations = new();
            ConcurrentSet<PropertyBuilder> buildersWithSynthesizedSetters = new();
            ConcurrentDictionary<IMemberBuilder, ConcurrentLinkedList<AspectLinkerDeclarationFlags>> buildersWithAdditionalDeclarationFlags = new();
            ConcurrentDictionary<SyntaxNode, MemberLevelTransformations> symbolMemberLevelTransformations = new();
            ConcurrentDictionary<IDeclarationBuilder, MemberLevelTransformations> introductionMemberLevelTransformations = new();
            ConcurrentDictionary<TypeDeclarationSyntax, TypeLevelTransformations> typeLevelTransformations = new();
            ConcurrentDictionary<IDeclarationBuilder, IIntroduceDeclarationTransformation> builderToTransformationMap = new();
            ConcurrentSet<SyntaxNode> nodesWithModifiedAttributes = new();

            void IndexTransformationsInSyntaxTree( IGrouping<SyntaxTree, ITransformation> transformationGroup )
            {
                // Transformations need to be sorted here because some transformations require a LexicalScope to get an unique name, and it
                // will give deterministic results only when called in a deterministic order.
                var sortedTransformations = transformationGroup.OrderBy( x => x, transformationComparer ).ToArray();

                // IntroduceDeclarationTransformation instances need to be indexed first.
                foreach ( var transformation in sortedTransformations )
                {
                    IndexIntroduceDeclarationTransformation( transformation, builderToTransformationMap );
                }

                // Replace transformations need to be indexed second.
                // NOTE: This is correct because replaced transformation is always in the same syntax tree as the replacing one.
                foreach ( var transformation in sortedTransformations )
                {
                    IndexReplaceTransformation( input, transformation, syntaxTransformationCollection, builderToTransformationMap, replacedIntroduceDeclarationTransformations );
                }

                foreach ( var transformation in sortedTransformations )
                {
                    IndexOverrideTransformation(
                        transformation,
                        syntaxTransformationCollection,
                        buildersWithSynthesizedSetters );

                    this.IndexIntroduceTransformation(
                        input,
                        transformation,
                        diagnostics,
                        lexicalScopeFactory,
                        nameProvider,
                        aspectReferenceSyntaxProvider,
                        buildersWithSynthesizedSetters,
                        buildersWithAdditionalDeclarationFlags,
                        syntaxTransformationCollection,
                        replacedIntroduceDeclarationTransformations );

                    this.IndexMemberLevelTransformation(
                        input,
                        diagnostics,
                        lexicalScopeFactory,
                        transformation,
                        symbolMemberLevelTransformations,
                        introductionMemberLevelTransformations );

                    IndexTypeLevelTransformation( transformation, symbolMemberLevelTransformations, typeLevelTransformations );

                    IndexNodesWithModifiedAttributes( transformation, nodesWithModifiedAttributes );
                }
            }

            var transformationsBySyntaxTree = input.Transformations.GroupBy( t => t.TransformedSyntaxTree );

            await this._taskScheduler.RunInParallelAsync( transformationsBySyntaxTree, IndexTransformationsInSyntaxTree, cancellationToken );

            await this._taskScheduler.RunInParallelAsync(
                introductionMemberLevelTransformations.Values,
                t => t.Sort( transformationComparer ),
                cancellationToken );

            await this._taskScheduler.RunInParallelAsync( symbolMemberLevelTransformations.Values, t => t.Sort( transformationComparer ), cancellationToken );

            var syntaxTreeForGlobalAttributes = input.CompilationModel.PartialCompilation.SyntaxTreeForCompilationLevelAttributes;

            // Group diagnostic suppressions by target.
            var suppressionsByTarget = input.DiagnosticSuppressions.ToMultiValueDictionary(
                s => s.Declaration,
                input.CompilationModel.InvariantComparer );

            // Rewrite syntax trees.
            Rewriter rewriter = new(
                this._serviceProvider,
                syntaxTransformationCollection,
                suppressionsByTarget,
                input.CompilationModel,
                symbolMemberLevelTransformations,
                introductionMemberLevelTransformations,
                nodesWithModifiedAttributes,
                syntaxTreeForGlobalAttributes,
                typeLevelTransformations );

            var syntaxTreeMapping = new ConcurrentDictionary<SyntaxTree, SyntaxTree>();

            var intermediateCompilation = input.InitialCompilation;

            void RewriteSyntaxTree( SyntaxTree initialSyntaxTree )
            {
                var oldRoot = initialSyntaxTree.GetRoot();
                var newRoot = rewriter.Visit( oldRoot ).AssertNotNull();

                if ( oldRoot != newRoot )
                {
                    var intermediateSyntaxTree = initialSyntaxTree.WithRootAndOptions( newRoot, initialSyntaxTree.Options );

                    if ( !syntaxTreeMapping.TryAdd( initialSyntaxTree, intermediateSyntaxTree ) )
                    {
                        throw new AssertionFailedException();
                    }
                }
            }

            await this._taskScheduler.RunInParallelAsync( input.InitialCompilation.SyntaxTrees.Values, RewriteSyntaxTree, cancellationToken );

            var helperSyntaxTree = aspectReferenceSyntaxProvider.GetLinkerHelperSyntaxTree( intermediateCompilation.LanguageOptions );
            var transformations = syntaxTreeMapping.Select( p => SyntaxTreeTransformation.ReplaceTree( p.Key, p.Value ) ).ToList();
            transformations.Add( SyntaxTreeTransformation.AddTree( helperSyntaxTree ) );

            intermediateCompilation = intermediateCompilation.Update( transformations );

            var injectionRegistry = new LinkerInjectionRegistry(
                transformationComparer,
                input.CompilationModel,
                intermediateCompilation.Compilation,
                syntaxTreeMapping,
                syntaxTransformationCollection.InjectedMembers );

            var projectOptions = this._serviceProvider.GetService<IProjectOptions>();

            return
                new LinkerInjectionStepOutput(
                    diagnostics,
                    input.CompilationModel,
                    intermediateCompilation,
                    injectionRegistry,
                    input.OrderedAspectLayers,
                    projectOptions );
        }

        private static void IndexNodesWithModifiedAttributes(
            ITransformation transformation,
            ConcurrentSet<SyntaxNode> nodesWithModifiedAttributes )
        {
            // We only need to index transformations on syntax (i.e. on source code) because introductions on generated code
            // are taken from the compilation model.

            // Note: Compilation-level attributes will not be indexed because the containing declaration has no
            // syntax reference.

            if ( transformation is IntroduceAttributeTransformation introduceAttributeTransformation )
            {
                foreach ( var declaringSyntax in introduceAttributeTransformation.TargetDeclaration.GetDeclaringSyntaxReferences() )
                {
                    nodesWithModifiedAttributes.Add( declaringSyntax.GetSyntax() );
                }
            }
            else if ( transformation is RemoveAttributesTransformation removeAttributesTransformation )
            {
                foreach ( var declaringSyntax in removeAttributesTransformation.ContainingDeclaration.GetDeclaringSyntaxReferences() )
                {
                    nodesWithModifiedAttributes.Add( declaringSyntax.GetSyntax() );
                }
            }
        }

        private static void IndexIntroduceDeclarationTransformation(
            ITransformation transformation,
            ConcurrentDictionary<IDeclarationBuilder, IIntroduceDeclarationTransformation> builderToTransformationMap
            )
        {
            if ( transformation is IIntroduceDeclarationTransformation introduceDeclarationTransformation)
            {
                builderToTransformationMap.TryAdd( introduceDeclarationTransformation.DeclarationBuilder, introduceDeclarationTransformation );
            }
        }

        private static void IndexReplaceTransformation(
            AspectLinkerInput input,
            ITransformation transformation,
            SyntaxTransformationCollection syntaxTransformationCollection,
            ConcurrentDictionary<IDeclarationBuilder, IIntroduceDeclarationTransformation> builderToTransformationMap,
            ConcurrentSet<IIntroduceDeclarationTransformation> replacedIntroduceDeclarationTransformations )
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
                        var fieldSyntaxReference = replacedField.Symbol.GetPrimarySyntaxReference();

                        if ( fieldSyntaxReference == null )
                        {
                            throw new AssertionFailedException();
                        }

                        var removedFieldSyntax = fieldSyntaxReference.GetSyntax();
                        syntaxTransformationCollection.AddRemovedSyntax( removedFieldSyntax );

                        break;

                    case Constructor replacedConstructor:
                        Invariant.Assert( replacedConstructor.Symbol.GetPrimarySyntaxReference() == null );

                        break;

                    // This needs to point to an interface
                    case IDeclarationBuilder replacedBuilder:
                        if (!builderToTransformationMap.TryGetValue( replacedBuilder, out var introduceDeclarationTransformation))
                        {
                            throw new AssertionFailedException();
                        }

                        replacedIntroduceDeclarationTransformations.Add( introduceDeclarationTransformation );

                        break;

                    default:
                        throw new AssertionFailedException();
                }
            }
        }

        private void IndexIntroduceTransformation(
            AspectLinkerInput input,
            ITransformation transformation,
            UserDiagnosticSink diagnostics,
            LexicalScopeFactory lexicalScopeFactory,
            LinkerInjectionNameProvider nameProvider,
            LinkerAspectReferenceSyntaxProvider aspectReferenceSyntaxProvider,
            IReadOnlyCollection<PropertyBuilder> buildersWithSynthesizedSetters,
            ConcurrentDictionary<IMemberBuilder, ConcurrentLinkedList<AspectLinkerDeclarationFlags>> buildersWithAdditionalDeclarationFlags,
            SyntaxTransformationCollection syntaxTransformationCollection,
            ConcurrentSet<IIntroduceDeclarationTransformation> replacedIntroduceDeclarationTransformations )
        {
            {
                if ( transformation is IIntroduceDeclarationTransformation introduceDeclarationTransformation
                     && replacedIntroduceDeclarationTransformations.Contains( introduceDeclarationTransformation ) )
                {
                    return;
                }

                switch ( transformation )
                {
                    case IInjectMemberTransformation injectMemberTransformation:
                        // Create the SyntaxGenerationContext for the insertion point.
                        var positionInSyntaxTree = GetSyntaxTreePosition( injectMemberTransformation.InsertPosition );

                        var syntaxGenerationContext = SyntaxGenerationContext.Create(
                            this._serviceProvider,
                            input.InitialCompilation.Compilation,
                            injectMemberTransformation.TransformedSyntaxTree,
                            positionInSyntaxTree );

                        // Call GetInjectedMembers
                        var injectionContext = new MemberInjectionContext(
                            diagnostics,
                            nameProvider,
                            aspectReferenceSyntaxProvider,
                            lexicalScopeFactory,
                            syntaxGenerationContext,
                            this._serviceProvider,
                            input.CompilationModel );

                        var injectedMembers = injectMemberTransformation.GetInjectedMembers( injectionContext );

                        injectedMembers = PostProcessInjectedMembers( injectedMembers );

                        syntaxTransformationCollection.Add( injectMemberTransformation, injectedMembers );

                        break;

                    case IInjectInterfaceTransformation injectInterfaceTransformation:
                        var introducedInterface = injectInterfaceTransformation.GetSyntax();
                        syntaxTransformationCollection.Add( injectInterfaceTransformation, introducedInterface );

                        break;
                }

                IEnumerable<InjectedMember> PostProcessInjectedMembers( IEnumerable<InjectedMember> injectedMembers )
                {
                    if ( transformation is IntroducePropertyTransformation introducePropertyTransformation
                         && buildersWithSynthesizedSetters.Contains( introducePropertyTransformation.IntroducedDeclaration ) )
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
                                                return im.WithSyntax( propertyDeclaration.WithSynthesizedSetter() );

                                            case { Semantic: InjectedMemberSemantic.InitializerMethod }:
                                                return im;

                                            default:
                                                throw new AssertionFailedException();
                                        }
                                    } );
                    }

                    if ( transformation is IIntroduceDeclarationTransformation introduceMemberTransformation
                         && buildersWithAdditionalDeclarationFlags.TryGetValue(
                             (IMemberBuilder) introduceMemberTransformation.DeclarationBuilder,
                             out var additionalFlagsList ) )
                    {
                        // This is a member builder that should have linker declaration flags added.
                        injectedMembers =
                            injectedMembers
                                .Select(
                                    im =>
                                    {
                                        var flags = im.Syntax.GetLinkerDeclarationFlags();

                                        foreach ( var additionalFlags in additionalFlagsList )
                                        {
                                            flags &= additionalFlags;
                                        }

                                        return
                                            im.WithSyntax( im.Syntax.WithLinkerDeclarationFlags( flags ) );
                                    } );
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
            SyntaxTransformationCollection syntaxTransformationCollection,
            ConcurrentSet<PropertyBuilder> buildersWithSynthesizedSetters )
        {
            if ( transformation is not IOverrideDeclarationTransformation overriddenDeclaration )
            {
                return;
            }

            // If this is an auto-property that does not override a base property, we can add synthesized init-only setter.
            // If this is overridden property we need to:
            //  1) Block inlining of the first override (force the trampoline).
            //  2) Substitute all sets of the property (can be only in constructors) to use the first override instead.
#pragma warning disable SA1513
            if ( overriddenDeclaration.OverriddenDeclaration is IProperty
                {
                    IsAutoPropertyOrField: true, Writeability: Writeability.ConstructorOnly, SetMethod: { IsImplicitlyDeclared: true },
                    OverriddenProperty: null or { SetMethod: not null }
                } overriddenAutoProperty )
#pragma warning restore SA1513
            {
                switch ( overriddenAutoProperty )
                {
                    case Property codeProperty:
                        syntaxTransformationCollection.AddAutoPropertyWithSynthesizedSetter(
                            (PropertyDeclarationSyntax) codeProperty.GetPrimaryDeclarationSyntax().AssertNotNull() );

                        break;

                    case BuiltProperty { PropertyBuilder: var builder }:
                        buildersWithSynthesizedSetters.Add( builder.AssertNotNull() );

                        break;

                    case PropertyBuilder builder:
                        buildersWithSynthesizedSetters.Add( builder.AssertNotNull() );

                        break;

                    default:
                        throw new AssertionFailedException();
                }
            }
        }

        private static void IndexTypeLevelTransformation(
            ITransformation transformation,
            ConcurrentDictionary<SyntaxNode, MemberLevelTransformations> symbolMemberLevelTransformations,
            ConcurrentDictionary<TypeDeclarationSyntax, TypeLevelTransformations> typeLevelTransformations )
        {
            if ( transformation is not ITypeLevelTransformation typeLevelTransformation )
            {
                return;
            }

            var declarationSyntax = (TypeDeclarationSyntax?) typeLevelTransformation.TargetType.GetPrimaryDeclarationSyntax();

            if ( declarationSyntax == null )
            {
                return;
            }

            var thisTypeLevelTransformations = typeLevelTransformations.GetOrAddNew( declarationSyntax );

            switch ( transformation )
            {
                case AddExplicitDefaultConstructorTransformation:
                    thisTypeLevelTransformations.AddExplicitDefaultConstructor = true;

                    foreach ( var syntaxReference in typeLevelTransformation.TargetType.GetSymbol().DeclaringSyntaxReferences )
                    {
                        foreach ( var member in ((TypeDeclarationSyntax) syntaxReference.GetSyntax()).Members )
                        {
                            switch ( member )
                            {
                                case PropertyDeclarationSyntax propertyDeclaration:
                                    var propertyTransformations = symbolMemberLevelTransformations.GetOrAdd(
                                        propertyDeclaration,
                                        _ => new MemberLevelTransformations() );

                                    propertyTransformations.AddDefaultInitializer = true;

                                    break;

                                case FieldDeclarationSyntax fieldDeclaration:

                                    foreach ( var variable in fieldDeclaration.Declaration.Variables )
                                    {
                                        var fieldTransformations = symbolMemberLevelTransformations.GetOrAddNew( variable );

                                        fieldTransformations.AddDefaultInitializer = true;
                                    }

                                    break;

                                case EventDeclarationSyntax eventDeclaration
                                    when eventDeclaration.GetLinkerDeclarationFlags().HasFlagFast( AspectLinkerDeclarationFlags.EventField )
                                         && !eventDeclaration.GetLinkerDeclarationFlags()
                                             .HasFlagFast( AspectLinkerDeclarationFlags.HasHiddenInitializerExpression ):
                                    var eventTransformations = symbolMemberLevelTransformations.GetOrAddNew( eventDeclaration );
                                    eventTransformations.AddDefaultInitializer = true;

                                    break;

                                case EventFieldDeclarationSyntax eventFieldDeclaration:

                                    foreach ( var variable in eventFieldDeclaration.Declaration.Variables )
                                    {
                                        var eventFieldTransformations = symbolMemberLevelTransformations.GetOrAddNew( variable );
                                        eventFieldTransformations.AddDefaultInitializer = true;
                                    }

                                    break;
                            }
                        }
                    }

                    break;

                default:
                    throw new AssertionFailedException();
            }
        }

        private void IndexMemberLevelTransformation(
            AspectLinkerInput input,
            UserDiagnosticSink diagnostics,
            LexicalScopeFactory lexicalScopeFactory,
            ITransformation transformation,
            ConcurrentDictionary<SyntaxNode, MemberLevelTransformations> symbolMemberLevelTransformations,
            ConcurrentDictionary<IDeclarationBuilder, MemberLevelTransformations> introductionMemberLevelTransformations )
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
                memberLevelTransformations = symbolMemberLevelTransformations.GetOrAddNew( declarationSyntax );
            }
            else
            {
                var parentDeclarationBuilder = (memberLevelTransformation.TargetMember as DeclarationBuilder
                                                ?? (memberLevelTransformation.TargetMember as BuiltDeclaration)?.Builder)
                    .AssertNotNull();

                memberLevelTransformations = introductionMemberLevelTransformations.GetOrAddNew( parentDeclarationBuilder );
            }

            switch (transformation, memberLevelTransformation.TargetMember)
            {
                case (IInsertStatementTransformation insertStatementTransformation, Constructor constructor):
                    {
                        var primaryDeclaration = constructor.GetPrimaryDeclarationSyntax().AssertNotNull();

                        var syntaxGenerationContext = SyntaxGenerationContext.Create(
                            this._serviceProvider,
                            input.InitialCompilation.Compilation,
                            primaryDeclaration );

                        foreach ( var insertedStatement in GetInsertedStatements( insertStatementTransformation, syntaxGenerationContext ) )
                        {
                            memberLevelTransformations.Add(
                                new LinkerInsertedStatement(
                                    transformation,
                                    primaryDeclaration,
                                    insertedStatement.Statement,
                                    insertedStatement.ContextDeclaration ) );
                        }

                        break;
                    }

                case (IInsertStatementTransformation insertStatementTransformation, BuiltConstructor or ConstructorBuilder):
                    {
                        var constructorBuilder = memberLevelTransformation.TargetMember as ConstructorBuilder
                                                 ?? ((BuiltConstructor) memberLevelTransformation.TargetMember).ConstructorBuilder;

                        var positionInSyntaxTree = GetSyntaxTreePosition( constructorBuilder.ToInsertPosition() );

                        var syntaxGenerationContext = SyntaxGenerationContext.Create(
                            this._serviceProvider,
                            input.InitialCompilation.Compilation,
                            constructorBuilder.PrimarySyntaxTree.AssertNotNull(),
                            positionInSyntaxTree );

                        foreach ( var insertedStatement in GetInsertedStatements( insertStatementTransformation, syntaxGenerationContext ) )
                        {
                            memberLevelTransformations.Add(
                                new LinkerInsertedStatement(
                                    transformation,
                                    constructorBuilder,
                                    insertedStatement.Statement,
                                    insertedStatement.ContextDeclaration ) );
                        }

                        break;
                    }

                case (IntroduceParameterTransformation appendParameterTransformation, _):
                    memberLevelTransformations.Add( appendParameterTransformation );

                    break;

                case (IntroduceConstructorInitializerArgumentTransformation appendArgumentTransformation, _):
                    memberLevelTransformations.Add( appendArgumentTransformation );

                    break;

                default:
                    throw new AssertionFailedException();
            }

            IEnumerable<InsertedStatement> GetInsertedStatements(
                IInsertStatementTransformation insertStatementTransformation,
                SyntaxGenerationContext syntaxGenerationContext )
            {
                var context = new InsertStatementTransformationContext(
                    diagnostics,
                    lexicalScopeFactory,
                    syntaxGenerationContext,
                    this._serviceProvider,
                    input.CompilationModel );

                var statements = insertStatementTransformation.GetInsertedStatements( context );
#if DEBUG
                statements = statements.ToList();

                foreach ( var statement in statements )
                {
                    if ( statement.Statement is BlockSyntax block )
                    {
                        if ( !block.Statements.All( s => s.HasAnnotations( FormattingAnnotations.GeneratedCodeAnnotationKind ) ) )
                        {
                            throw new AssertionFailedException();
                        }
                    }
                    else
                    {
                        if ( !statement.Statement.HasAnnotations( FormattingAnnotations.GeneratedCodeAnnotationKind ) )
                        {
                            throw new AssertionFailedException();
                        }
                    }
                }
#endif

                return statements;
            }
        }
    }
}