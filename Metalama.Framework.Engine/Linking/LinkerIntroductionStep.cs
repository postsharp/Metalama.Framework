// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.AspectOrdering;
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

            // TODO: Merge observable and non-observable transformations so that the order is preserved.
            //       Maybe have all transformations already together in the input?
            var allTransformations =
                MergeOrderedTransformations(
                        input.OrderedAspectLayers,
                        input.CompilationModel.GetAllObservableTransformations( false ).Select( x => x.Transformations.OfType<ISyntaxTreeTransformation>() ),
                        input.NonObservableTransformations.OfType<ISyntaxTreeTransformation>() )
                    .ToList();

            ProcessReplaceTransformations( input, allTransformations, syntaxTransformationCollection, out var replacedTransformations );

            var lexicalScopeFactory = new LexicalScopeFactory( input.CompilationModel );

            this.ProcessIntroduceTransformations(
                input,
                allTransformations,
                diagnostics,
                lexicalScopeFactory,
                nameProvider,
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
            replacedTransformations = new HashSet<ISyntaxTreeTransformation>();

            foreach ( var transformation in allTransformations.OfType<IReplaceMember>() )
            {
                if ( transformation.ReplacedMember == null )
                {
                    continue;
                }

                var replacedMember = transformation.ReplacedMember.Value.GetTarget( input.CompilationModel );

                IDeclaration canonicalReplacedMember = replacedMember switch
                {
                    BuiltDeclaration declaration => declaration.Builder,
                    _ => replacedMember
                };

                switch ( canonicalReplacedMember )
                {
                    case Field replacedField:
                        var syntaxReference = replacedField.Symbol.GetPrimarySyntaxReference();

                        if ( syntaxReference == null )
                        {
                            throw new AssertionFailedException();
                        }

                        var removedSyntax = syntaxReference.GetSyntax();

                        syntaxTransformationCollection.AddRemovedSyntax( removedSyntax );

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
                    case IMemberIntroduction memberIntroduction:
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

                        syntaxTransformationCollection.Add( memberIntroduction, introducedMembers );

                        break;

                    case IIntroducedInterface interfaceIntroduction:
                        var introducedInterface = interfaceIntroduction.GetSyntax();
                        syntaxTransformationCollection.Add( interfaceIntroduction, introducedInterface );

                        break;
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

        private void ProcessInsertStatementTransformations(
            AspectLinkerInput input,
            UserDiagnosticSink diagnostics, 
            LexicalScopeFactory lexicalScopeFactory,
            List<ITransformation> allTransformations,
            out Dictionary<SyntaxNode, IReadOnlyList<LinkerInsertedStatement>> symbolInsertedStatements,
            out Dictionary<IMemberIntroduction, IReadOnlyList<LinkerInsertedStatement>> introductionInsertedStatements )
        {
            symbolInsertedStatements = new Dictionary<SyntaxNode, IReadOnlyList<LinkerInsertedStatement>>();
            introductionInsertedStatements = new Dictionary<IMemberIntroduction, IReadOnlyList<LinkerInsertedStatement>>();

            foreach ( var insertStatementTransformation in allTransformations.OfType<IInsertStatementTransformation>() )
            {
                // TODO: Supports only constructors without overrides.
                //       Needs to be generalized for anything else (take into account overrides).
                switch (insertStatementTransformation.TargetDeclaration)
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
                                        insertedStatement.Value.Position, 
                                        insertedStatement.Value.Statement,
                                        insertedStatement.Value.ContextDeclaration ) );
                            }

                            break;
                        }

                    case ConstructorBuilder constructorBuilder:
                        {
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
                                        insertedStatement.Value.Position, 
                                        insertedStatement.Value.Statement,
                                        insertedStatement.Value.ContextDeclaration ) );
                            }

                            break;
                        }

                    default:
                        throw new AssertionFailedException();
                }

                InsertedStatement? GetInsertedStatement(SyntaxGenerationContext syntaxGenerationContext)
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

        private static IEnumerable<ITransformation> MergeOrderedTransformations(
            IReadOnlyList<OrderedAspectLayer> orderedLayers,
            IEnumerable<IEnumerable<ITransformation>> observableTransformationLists,
            IEnumerable<ITransformation> nonObservableTransformations )
        {
            var enumerators = new LinkedList<IEnumerator<ITransformation>>();

            foreach ( var observableTransformations in observableTransformationLists )
            {
                enumerators.AddLast( observableTransformations.GetEnumerator() );
            }

            enumerators.AddLast( nonObservableTransformations.GetEnumerator() );

            // Initialize enumerators and remove empty ones.
            var currentEnumerator = enumerators.First;

            while ( currentEnumerator != null )
            {
                if ( !currentEnumerator.Value.MoveNext() )
                {
                    enumerators.Remove( currentEnumerator );
                }

                currentEnumerator = currentEnumerator.Next;
            }

            // Go through ordered layers and yield all transformations for these layers.
            // Presumes all input enumerable are ordered according to ordered layers.
            foreach ( var orderedLayer in orderedLayers )
            {
                currentEnumerator = enumerators.First;

                if ( currentEnumerator == null )
                {
                    break;
                }

                do
                {
                    var current = currentEnumerator.Value.Current.AssertNotNull();

                    while ( current.Advice.AspectLayerId == orderedLayer.AspectLayerId )
                    {
                        yield return current;

                        if ( !currentEnumerator.Value.MoveNext() )
                        {
                            var toRemove = currentEnumerator;
                            currentEnumerator = currentEnumerator.Next;
                            enumerators.Remove( toRemove );

                            goto next;
                        }

                        current = currentEnumerator.Value.Current;
                    }

                    currentEnumerator = currentEnumerator.Next;

                next:

                    // Comment to make the formatter happy.

                    ;
                }
                while ( currentEnumerator != null );
            }
        }
    }
}