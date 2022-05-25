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
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

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

            var nameProvider = new LinkerIntroductionNameProvider();
            var syntaxTransformationCollection = new SyntaxTransformationCollection();

            // TODO: this sorting can be optimized.
            var allTransformations =
                input.Transformations.OfType<ISyntaxTreeTransformation>()
                    .OrderBy( x => x.Advice.AspectLayerId, new AspectLayerIdComparer( input.OrderedAspectLayers ) )
                    .Cast<ITransformation>()
                    .ToList();

            ProcessReplaceTransformations( input, allTransformations, syntaxTransformationCollection, out var replacedTransformations );

            var lexicalScopeFactory = new LexicalScopeFactory( input.CompilationModel );

            ProcessOverrideTransformations(
                allTransformations,
                syntaxTransformationCollection,
                out var buildersWithSynthesizedSetters );

            this.ProcessIntroduceTransformations(
                input,
                allTransformations,
                diagnostics,
                lexicalScopeFactory,
                nameProvider,
                buildersWithSynthesizedSetters,
                syntaxTransformationCollection,
                replacedTransformations );

            this.ProcessInsertStatementTransformations(
                input,
                diagnostics,
                lexicalScopeFactory,
                allTransformations,
                out var syntaxNodeInsertStatements,
                out var memberIntroductionInsertStatements );

            // Group diagnostic suppressions by target.
            var suppressionsByTarget = input.DiagnosticSuppressions.ToMultiValueDictionary(
                s => s.Declaration,
                input.CompilationModel.InvariantComparer );

            Rewriter rewriter = new(
                syntaxTransformationCollection,
                suppressionsByTarget,
                input.CompilationModel,
                input.OrderedAspectLayers,
                syntaxNodeInsertStatements,
                memberIntroductionInsertStatements );

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

            intermediateCompilation = intermediateCompilation.Update(
                syntaxTreeMapping.Select( p => new SyntaxTreeModification( p.Value, p.Key ) ).ToList(),
                Array.Empty<SyntaxTree>() );

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

        private static void ProcessReplaceTransformations(
            AspectLinkerInput input,
            List<ITransformation> allTransformations,
            SyntaxTransformationCollection syntaxTransformationCollection,
            out HashSet<ISyntaxTreeTransformation> replacedTransformations )
        {
            var compilation = input.CompilationModel;
            replacedTransformations = new HashSet<ISyntaxTreeTransformation>();

            foreach ( var transformation in allTransformations.OfType<IReplaceMemberTransformation>() )
            {
                if ( transformation.ReplacedMember == null )
                {
                    continue;
                }

                // We want to get the replaced member as it is in the compilation of the transformation, i.e. with applied redirections up to that point.
                var replacedDeclaration = (IDeclaration) transformation.ReplacedMember.Value.GetTarget( compilation, false );

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

                    case ISyntaxTreeTransformation replacedTransformation:
                        replacedTransformations.Add( replacedTransformation );

                        break;

                    default:
                        throw new AssertionFailedException();
                }
            }
        }

        private void ProcessIntroduceTransformations(
            AspectLinkerInput input,
            List<ITransformation> allTransformations,
            UserDiagnosticSink diagnostics,
            LexicalScopeFactory lexicalScopeFactory,
            LinkerIntroductionNameProvider nameProvider,
            IReadOnlyCollection<PropertyBuilder> buildersWithSyntesizedSetters,
            SyntaxTransformationCollection syntaxTransformationCollection,
            HashSet<ISyntaxTreeTransformation> replacedTransformations )
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
                            memberIntroduction.TargetSyntaxTree,
                            positionInSyntaxTree );

                        // Call GetIntroducedMembers
                        var introductionContext = new MemberIntroductionContext(
                            diagnostics,
                            nameProvider,
                            lexicalScopeFactory,
                            syntaxGenerationContext,
                            this._serviceProvider );

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
                    if ( transformation is PropertyBuilder propertyBuilder && buildersWithSyntesizedSetters.Contains( propertyBuilder ) )
                    {
                        // This is a property which should have a synthesized setter added.
                        return
                            introducedMembers
                                .Select(
                                    im =>
                                    {
                                        switch ( im )
                                        {
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
        {
            var positionInSyntaxTree = 0;

            if ( insertPosition.SyntaxNode != null )
            {
                switch ( insertPosition.Relation )
                {
                    case InsertPositionRelation.After:
                        positionInSyntaxTree = insertPosition.SyntaxNode.Span.End + 1;

                        break;

                    case InsertPositionRelation.Within:
                        positionInSyntaxTree = ((BaseTypeDeclarationSyntax) insertPosition.SyntaxNode).CloseBraceToken.Span.Start
                                               - 1;

                        break;

                    default:
                        positionInSyntaxTree = 0;

                        break;
                }
            }

            return positionInSyntaxTree;
        }

        private static void ProcessOverrideTransformations(
            List<ITransformation> allTransformations,
            SyntaxTransformationCollection syntaxTransformationCollection,
            out IReadOnlyCollection<PropertyBuilder> buildersWithSynthesizedSetters )
        {
            buildersWithSynthesizedSetters = new HashSet<PropertyBuilder>();

            foreach ( var transformation in allTransformations.OfType<IOverriddenDeclaration>() )
            {
                if ( transformation.OverriddenDeclaration is IProperty
                        { IsAutoPropertyOrField: true, Writeability: Writeability.ConstructorOnly, SetMethod: { IsImplicit: true } } overriddenAutoProperty )
                {
                    switch ( overriddenAutoProperty )
                    {
                        case Property codeProperty:
                            syntaxTransformationCollection.AddAutoPropertyWithSynthetizedSetter(
                                (PropertyDeclarationSyntax) codeProperty.GetPrimaryDeclaration().AssertNotNull() );

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

        private void ProcessInsertStatementTransformations(
            AspectLinkerInput input,
            UserDiagnosticSink diagnostics,
            LexicalScopeFactory lexicalScopeFactory,
            List<ITransformation> allTransformations,
            out Dictionary<SyntaxNode, IReadOnlyList<LinkerInsertedStatement>> symbolInsertedStatements,
            out Dictionary<IIntroduceMemberTransformation, IReadOnlyList<LinkerInsertedStatement>> introductionInsertedStatements )
        {
            symbolInsertedStatements = new Dictionary<SyntaxNode, IReadOnlyList<LinkerInsertedStatement>>();
            introductionInsertedStatements = new Dictionary<IIntroduceMemberTransformation, IReadOnlyList<LinkerInsertedStatement>>();

            foreach ( var insertStatementTransformation in allTransformations.OfType<IInsertStatementTransformation>() )
            {
                // TODO: Supports only constructors without overrides.
                //       Needs to be generalized for anything else (take into account overrides).
                switch ( insertStatementTransformation.TargetDeclaration )
                {
                    case Constructor constructor:
                        {
                            var primaryDeclaration = constructor.GetPrimaryDeclaration().AssertNotNull();

                            var syntaxGenerationContext = SyntaxGenerationContext.Create(
                                this._serviceProvider,
                                input.InitialCompilation.Compilation,
                                primaryDeclaration );

                            var insertedStatement = GetInsertedStatement( syntaxGenerationContext );

                            if ( insertedStatement != null )
                            {
                                var syntaxNode = constructor.GetPrimaryDeclaration().AssertNotNull();

                                if ( !symbolInsertedStatements.TryGetValue( syntaxNode, out var list ) )
                                {
                                    symbolInsertedStatements[syntaxNode] = list = new List<LinkerInsertedStatement>();
                                }

                                ((List<LinkerInsertedStatement>) list).Add(
                                    new LinkerInsertedStatement(
                                        insertStatementTransformation,
                                        primaryDeclaration,
                                        insertedStatement.Value.Statement,
                                        insertedStatement.Value.ContextDeclaration ) );
                            }

                            break;
                        }

                    case BuiltConstructor:
                    case ConstructorBuilder:
                        {
                            var constructorBuilder = insertStatementTransformation.TargetDeclaration as ConstructorBuilder
                                                     ?? ((BuiltConstructor) insertStatementTransformation.TargetDeclaration).ConstructorBuilder;

                            var positionInSyntaxTree = GetSyntaxTreePosition( constructorBuilder.InsertPosition );

                            var syntaxGenerationContext = SyntaxGenerationContext.Create(
                                this._serviceProvider,
                                input.InitialCompilation.Compilation,
                                constructorBuilder.PrimarySyntaxTree.AssertNotNull(),
                                positionInSyntaxTree );

                            var insertedStatement = GetInsertedStatement( syntaxGenerationContext );

                            if ( insertedStatement != null )
                            {
                                if ( !introductionInsertedStatements.TryGetValue( constructorBuilder, out var list ) )
                                {
                                    introductionInsertedStatements[constructorBuilder] = list = new List<LinkerInsertedStatement>();
                                }

                                ((List<LinkerInsertedStatement>) list).Add(
                                    new LinkerInsertedStatement(
                                        insertStatementTransformation,
                                        constructorBuilder,
                                        insertedStatement.Value.Statement,
                                        insertedStatement.Value.ContextDeclaration ) );
                            }

                            break;
                        }

                    default:
                        throw new AssertionFailedException();
                }

                InsertedStatement? GetInsertedStatement( SyntaxGenerationContext syntaxGenerationContext )
                {
                    var context = new InsertStatementTransformationContext(
                        diagnostics,
                        lexicalScopeFactory,
                        syntaxGenerationContext,
                        this._serviceProvider );

                    return insertStatementTransformation.GetInsertedStatement( context );
                }
            }
        }
    }
}