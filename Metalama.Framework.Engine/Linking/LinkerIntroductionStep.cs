// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
#if DEBUG
using Metalama.Framework.Engine.Formatting;
#endif

namespace Metalama.Framework.Engine.Linking
{
    /// <summary>
    /// Aspect linker introduction steps. Adds introduced members from all transformation to the Roslyn compilation. This involves calling template expansion.
    /// This results in the transformation registry and intermediate compilation, and also produces diagnostics.
    /// </summary>
    internal partial class LinkerIntroductionStep : AspectLinkerPipelineStep<AspectLinkerInput, LinkerIntroductionStepOutput>
    {
        private readonly IServiceProvider _serviceProvider;

        public LinkerIntroductionStep( IServiceProvider serviceProvider )
        {
            serviceProvider.GetRequiredService<ServiceProviderMark>().RequireProjectWide();

            this._serviceProvider = serviceProvider;
        }

        public override LinkerIntroductionStepOutput Execute( AspectLinkerInput input )
        {
            // We don't use a code fix filter because the linker is not supposed to suggest code fixes. If that changes, we need to pass a filter.
            var diagnostics = new UserDiagnosticSink( input.CompileTimeProject, null );

            var nameProvider = new LinkerIntroductionNameProvider( input.CompilationModel );
            var syntaxTransformationCollection = new SyntaxTransformationCollection();
            var lexicalScopeFactory = new LexicalScopeFactory( input.CompilationModel );

            var supportsNullability = input.InitialCompilation.InitialCompilation.Options.NullableContextOptions != NullableContextOptions.Disable;

            var aspectReferenceSyntaxProvider = new LinkerAspectReferenceSyntaxProvider( supportsNullability );

            // TODO: this sorting can be optimized.
            var allTransformations =
                input.Transformations
                    .OrderBy( x => x.ParentAdvice.AspectLayerId, new AspectLayerIdComparer( input.OrderedAspectLayers ) )
                    .ToList();

            // TODO: this series of calls to Index* methods can be optimized to avoid repeated type checks of the transformations.
            IndexReplaceTransformations( input, allTransformations, syntaxTransformationCollection, out var replacedTransformations );

            IndexOverrideTransformations(
                allTransformations,
                syntaxTransformationCollection,
                out var buildersWithSynthesizedSetters );

            this.IndexIntroduceTransformations(
                input,
                allTransformations,
                diagnostics,
                lexicalScopeFactory,
                nameProvider,
                aspectReferenceSyntaxProvider,
                buildersWithSynthesizedSetters,
                syntaxTransformationCollection,
                replacedTransformations );

            this.IndexMemberLevelTransformations(
                input,
                diagnostics,
                lexicalScopeFactory,
                allTransformations,
                out var syntaxMemberLevelTransformations,
                out var introductionMemberLevelTransformations );

            IndexTypeLevelTransformations( allTransformations, out var typeLevelTransformations );

            IndexNodesWithModifiedAttributes( allTransformations, out var nodesWithModifiedAttributes );

            FindPrimarySyntaxTreeForGlobalAttributes( input.CompilationModel, out var syntaxTreeForGlobalAttributes );

            // Group diagnostic suppressions by target.
            var suppressionsByTarget = input.DiagnosticSuppressions.ToMultiValueDictionary(
                s => s.Declaration,
                input.CompilationModel.InvariantComparer );

            Rewriter rewriter = new(
                this._serviceProvider,
                syntaxTransformationCollection,
                suppressionsByTarget,
                input.CompilationModel,
                input.OrderedAspectLayers,
                syntaxMemberLevelTransformations,
                introductionMemberLevelTransformations,
                nodesWithModifiedAttributes,
                syntaxTreeForGlobalAttributes,
                typeLevelTransformations );

            var syntaxTreeMapping = new Dictionary<SyntaxTree, SyntaxTree>();

            // Process syntax trees one by one.
            var intermediateCompilation = input.InitialCompilation;

            foreach ( var initialSyntaxTree in input.InitialCompilation.SyntaxTrees.Values )
            {
                var oldRoot = initialSyntaxTree.GetRoot();
                var newRoot = rewriter.Visit( oldRoot ).AssertNotNull();

                if ( oldRoot != newRoot )
                {
                    var intermediateSyntaxTree = initialSyntaxTree.WithRootAndOptions( newRoot, initialSyntaxTree.Options );

                    syntaxTreeMapping.Add( initialSyntaxTree, intermediateSyntaxTree );
                }
            }

            var helperSyntaxTree = aspectReferenceSyntaxProvider.GetLinkerHelperSyntaxTree( intermediateCompilation.LanguageOptions );

            intermediateCompilation =
                helperSyntaxTree != null
                    ? intermediateCompilation.Update(
                        syntaxTreeMapping.Select( p => new SyntaxTreeModification( p.Value, p.Key ) ).ToList(),
                        new[] { helperSyntaxTree } )
                    : intermediateCompilation;

            var introductionRegistry = new LinkerIntroductionRegistry(
                input.CompilationModel,
                intermediateCompilation.Compilation,
                syntaxTreeMapping,
                syntaxTransformationCollection.IntroducedMembers );

            var projectOptions = this._serviceProvider.GetService<IProjectOptions>();

            return new LinkerIntroductionStepOutput(
                diagnostics,
                input.CompilationModel,
                intermediateCompilation,
                introductionRegistry,
                input.OrderedAspectLayers,
                projectOptions );
        }

        private static void FindPrimarySyntaxTreeForGlobalAttributes( CompilationModel compilation, out SyntaxTree globalAttributeSyntaxTree )
        {
            globalAttributeSyntaxTree =
                compilation.Attributes.SelectMany( a => a.GetDeclaringSyntaxReferences() )
                    .Select( x => x.SyntaxTree )
                    .OrderByDescending( t => t.FilePath.Length )
                    .FirstOrDefault()
                ?? compilation.PartialCompilation.SyntaxTrees.OrderByDescending( t => t.Key.Length )
                    .FirstOrDefault()
                    .Value;
        }

        private static void IndexNodesWithModifiedAttributes(
            List<ITransformation> allTransformations,
            out HashSet<SyntaxNode> nodesWithModifiedAttributes )
        {
            // We only need to index transformations on syntax (i.e. on source code) because introductions on generated code
            // are taken from the compilation model.

            // Note: Compilation-level attributes will not be indexed because the containing declaration has no
            // syntax reference.

            nodesWithModifiedAttributes = new HashSet<SyntaxNode>();

            foreach ( var transformation in allTransformations )
            {
                if ( transformation is AttributeBuilder attributeBuilder )
                {
                    foreach ( var declaringSyntax in attributeBuilder.ContainingDeclaration.GetDeclaringSyntaxReferences() )
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
        }

        private static void IndexReplaceTransformations(
            AspectLinkerInput input,
            List<ITransformation> allTransformations,
            SyntaxTransformationCollection syntaxTransformationCollection,
            out HashSet<ITransformation> replacedTransformations )
        {
            var compilation = input.CompilationModel;
            replacedTransformations = new HashSet<ITransformation>();

            foreach ( var transformation in allTransformations.OfType<IReplaceMemberTransformation>() )
            {
                if ( transformation.ReplacedMember.IsDefault )
                {
                    continue;
                }

                // We want to get the replaced member as it is in the compilation of the transformation, i.e. with applied redirections up to that point.
                // TODO: the target may have been removed from the
                var replacedDeclaration = (IDeclaration) transformation.ReplacedMember.GetTarget(
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

                    case ITransformation replacedTransformation:
                        replacedTransformations.Add( replacedTransformation );

                        break;

                    default:
                        throw new AssertionFailedException();
                }
            }
        }

        private void IndexIntroduceTransformations(
            AspectLinkerInput input,
            List<ITransformation> allTransformations,
            UserDiagnosticSink diagnostics,
            LexicalScopeFactory lexicalScopeFactory,
            LinkerIntroductionNameProvider nameProvider,
            LinkerAspectReferenceSyntaxProvider aspectReferenceSyntaxProvider,
            IReadOnlyCollection<PropertyBuilder> buildersWithSynthesizedSetters,
            SyntaxTransformationCollection syntaxTransformationCollection,
            HashSet<ITransformation> replacedTransformations )
        {
            // Visit all transformations, respect aspect part ordering.
            foreach ( var transformation in allTransformations )
            {
                if ( replacedTransformations.Contains( transformation ) )
                {
                    continue;
                }

                switch ( transformation )
                {
                    case IIntroduceMemberTransformation memberIntroduction:
                        // Create the SyntaxGenerationContext for the insertion point.
                        var positionInSyntaxTree = GetSyntaxTreePosition( memberIntroduction.InsertPosition );

                        var syntaxGenerationContext = SyntaxGenerationContext.Create(
                            this._serviceProvider,
                            input.InitialCompilation.Compilation,
                            memberIntroduction.TransformedSyntaxTree,
                            positionInSyntaxTree );

                        // Call GetIntroducedMembers
                        var introductionContext = new MemberIntroductionContext(
                            diagnostics,
                            nameProvider,
                            aspectReferenceSyntaxProvider,
                            lexicalScopeFactory,
                            syntaxGenerationContext,
                            this._serviceProvider,
                            input.CompilationModel );

                        var introducedMembers = memberIntroduction.GetIntroducedMembers( introductionContext );

                        introducedMembers = PostProcessIntroducedMembers( introducedMembers );

                        syntaxTransformationCollection.Add( memberIntroduction, introducedMembers );

                        break;

                    case IIntroduceInterfaceTransformation interfaceIntroduction:
                        var introducedInterface = interfaceIntroduction.GetSyntax();
                        syntaxTransformationCollection.Add( interfaceIntroduction, introducedInterface );

                        break;
                }

                IEnumerable<IntroducedMember> PostProcessIntroducedMembers( IEnumerable<IntroducedMember> introducedMembers )
                {
                    if ( transformation is PropertyBuilder propertyBuilder && buildersWithSynthesizedSetters.Contains( propertyBuilder ) )
                    {
                        // This is a property which should have a synthesized setter added.
                        return
                            introducedMembers
                                .Select(
                                    im =>
                                    {
                                        switch ( im )
                                        {
                                            // ReSharper disable once MissingIndent
                                            case
                                            {
                                                Semantic: IntroducedMemberSemantic.Introduction, Kind: DeclarationKind.Property,
                                                Syntax: PropertyDeclarationSyntax propertyDeclaration
                                            }:
                                                return im.WithSyntax( propertyDeclaration.WithSynthesizedSetter() );

                                            case { Semantic: IntroducedMemberSemantic.InitializerMethod }:
                                                return im;

                                            default:
                                                throw new AssertionFailedException();
                                        }
                                    } );
                    }

                    return introducedMembers;
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

        private static void IndexOverrideTransformations(
            List<ITransformation> allTransformations,
            SyntaxTransformationCollection syntaxTransformationCollection,
            out IReadOnlyCollection<PropertyBuilder> buildersWithSynthesizedSetters )
        {
            buildersWithSynthesizedSetters = new HashSet<PropertyBuilder>();

            foreach ( var transformation in allTransformations.OfType<IOverriddenDeclaration>() )
            {
#pragma warning disable SA1513
                if ( transformation.OverriddenDeclaration is IProperty
                    {
                        IsAutoPropertyOrField: true, Writeability: Writeability.ConstructorOnly, SetMethod: { IsImplicitlyDeclared: true }
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
                            ((HashSet<PropertyBuilder>) buildersWithSynthesizedSetters).Add( builder.AssertNotNull() );

                            break;

                        case PropertyBuilder builder:
                            ((HashSet<PropertyBuilder>) buildersWithSynthesizedSetters).Add( builder.AssertNotNull() );

                            break;

                        default:
                            throw new AssertionFailedException();
                    }
                }
            }
        }

        private static void IndexTypeLevelTransformations(
            List<ITransformation> allTransformations,
            out Dictionary<TypeDeclarationSyntax, TypeLevelTransformations> typeLevelTransformations )
        {
            typeLevelTransformations = new Dictionary<TypeDeclarationSyntax, TypeLevelTransformations>();

            foreach ( var transformation in allTransformations.OfType<ITypeLevelTransformation>() )
            {
                TypeLevelTransformations? theseTypeLevelTransformations;
                var declarationSyntax = (TypeDeclarationSyntax?) transformation.TargetType.GetPrimaryDeclarationSyntax();

                if ( declarationSyntax != null )
                {
                    if ( !typeLevelTransformations.TryGetValue( declarationSyntax, out theseTypeLevelTransformations ) )
                    {
                        typeLevelTransformations[declarationSyntax] = theseTypeLevelTransformations = new TypeLevelTransformations();
                    }
                }
                else
                {
                    continue;
                }

                switch ( transformation )
                {
                    case AddExplicitDefaultConstructorTransformation:
                        theseTypeLevelTransformations.AddExplicitDefaultConstructor = true;

                        break;

                    default:
                        throw new AssertionFailedException();
                }
            }
        }

        private void IndexMemberLevelTransformations(
            AspectLinkerInput input,
            UserDiagnosticSink diagnostics,
            LexicalScopeFactory lexicalScopeFactory,
            List<ITransformation> allTransformations,
            out Dictionary<SyntaxNode, MemberLevelTransformations> symbolMemberLevelTransformations,
            out Dictionary<IIntroduceMemberTransformation, MemberLevelTransformations> introductionMemberLevelTransformations )
        {
            symbolMemberLevelTransformations = new Dictionary<SyntaxNode, MemberLevelTransformations>();
            introductionMemberLevelTransformations = new Dictionary<IIntroduceMemberTransformation, MemberLevelTransformations>();

            // Insert statements must be executed in inverse order (because we need the forward execution order and not the override order)
            // except within an aspect, where the order needs to be preserved.
            var allMemberLevelTransformations = allTransformations.OfType<IMemberLevelTransformation>()
                .GroupBy( x => x.ParentAdvice.Aspect )
                .Reverse()
                .SelectMany( x => x );

            foreach ( var transformation in allMemberLevelTransformations )
            {
                // TODO: Supports only constructors without overrides.
                //       Needs to be generalized for anything else (take into account overrides).

                MemberLevelTransformations? memberLevelTransformations;
                var declarationSyntax = transformation.TargetMember.GetPrimaryDeclarationSyntax();

                if ( declarationSyntax != null )
                {
                    if ( !symbolMemberLevelTransformations.TryGetValue( declarationSyntax, out memberLevelTransformations ) )
                    {
                        symbolMemberLevelTransformations[declarationSyntax] = memberLevelTransformations = new MemberLevelTransformations();
                    }
                }
                else
                {
                    var parentTransformation = (transformation.TargetMember as IIntroduceMemberTransformation
                                                ?? (transformation.TargetMember as BuiltDeclaration)?.Builder as IIntroduceMemberTransformation)
                        .AssertNotNull();

                    if ( !introductionMemberLevelTransformations.TryGetValue( parentTransformation, out memberLevelTransformations ) )
                    {
                        introductionMemberLevelTransformations[parentTransformation] = memberLevelTransformations = new MemberLevelTransformations();
                    }
                }

                switch (transformation, transformation.TargetMember)
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
                            var constructorBuilder = transformation.TargetMember as ConstructorBuilder
                                                     ?? ((BuiltConstructor) transformation.TargetMember).ConstructorBuilder;

                            var positionInSyntaxTree = GetSyntaxTreePosition( constructorBuilder.InsertPosition );

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

                    case (CallDefaultConstructorTransformation, _):
                        memberLevelTransformations.HasCallDefaultConstructorTransformation = true;

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
}