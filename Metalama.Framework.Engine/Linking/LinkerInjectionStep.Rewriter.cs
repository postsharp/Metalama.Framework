﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using MethodKind = Metalama.Framework.Code.MethodKind;
using SpecialType = Metalama.Framework.Code.SpecialType;

namespace Metalama.Framework.Engine.Linking;

internal sealed partial class LinkerInjectionStep
{
    private sealed partial class Rewriter : SafeSyntaxRewriter
    {
        private readonly CompilationModel _compilation;
        private readonly SemanticModelProvider _semanticModelProvider;
        private readonly SyntaxGenerationContext _defaultSyntaxGenerationContext;

        private readonly LinkerInjectionStep _parent;
        private readonly ImmutableDictionaryOfArray<IDeclaration, ScopedSuppression> _diagnosticSuppressions;
        private readonly TransformationCollection _transformationCollection;
        private readonly SyntaxTree _syntaxTreeForGlobalAttributes;

        // Maps a diagnostic id to the number of times it has been suppressed.
        private ImmutableHashSet<string> _activeSuppressions = ImmutableHashSet.Create<string>( StringComparer.OrdinalIgnoreCase );

        public Rewriter(
            LinkerInjectionStep parent,
            TransformationCollection syntaxTransformationCollection,
            ImmutableDictionaryOfArray<IDeclaration, ScopedSuppression> diagnosticSuppressions,
            CompilationModel compilation,
            SyntaxTree syntaxTreeForGlobalAttributes )
        {
            this._parent = parent;
            this._diagnosticSuppressions = diagnosticSuppressions;
            this._compilation = compilation;
            this._transformationCollection = syntaxTransformationCollection;
            this._semanticModelProvider = compilation.RoslynCompilation.GetSemanticModelProvider();
            this._syntaxTreeForGlobalAttributes = syntaxTreeForGlobalAttributes;
            this._defaultSyntaxGenerationContext = this.CompilationContext.GetSyntaxGenerationContext( this.SyntaxGenerationOptions );
        }

        private CompilationContext CompilationContext => this._parent._compilationContext;

        private bool PreserveTrivia => this._parent._syntaxGenerationOptions.PreserveTrivia;

        private bool NormalizeWhitespace => this._parent._syntaxGenerationOptions.NormalizeWhitespace;

        private SyntaxGenerationOptions SyntaxGenerationOptions => this._parent._syntaxGenerationOptions;

        public override bool VisitIntoStructuredTrivia => true;

        /// <summary>
        /// Gets the list of suppressions for a given syntax node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private IEnumerable<string> GetSuppressions( SyntaxNode node )
        {
            return node switch
            {
                FieldDeclarationSyntax { Declaration.Variables.Count: 1 } field => FindSuppressionsCore( field.Declaration.Variables.First() ),

                // If we have a field declaration that declares many field, we merge all suppressions
                // and suppress all for all fields. This is significantly simpler than splitting the declaration.
                FieldDeclarationSyntax { Declaration.Variables.Count: > 1 } field => field.Declaration.Variables.SelectAsReadOnlyList( FindSuppressionsCore )
                    .SelectMany( l => l ),

                _ => FindSuppressionsCore( node )
            };

            IEnumerable<string> FindSuppressionsCore( SyntaxNode identifierNode )
            {
                var declaredSymbol = this._semanticModelProvider.GetSemanticModel( node.SyntaxTree ).GetDeclaredSymbol( identifierNode );

                if ( declaredSymbol != null )
                {
                    var declaration = this._compilation.Factory.GetDeclaration( declaredSymbol );

                    return this.GetSuppressions( declaration );
                }
                else
                {
                    return ImmutableArray<string>.Empty;
                }
            }
        }

        private IEnumerable<string> GetSuppressions( IDeclaration declaration )
            => this._diagnosticSuppressions[declaration].Select( s => s.Definition.SuppressedDiagnosticId );

        /// <summary>
        /// Adds suppression to a node. This is done both by adding <c>#pragma warning</c> trivia
        /// around the node and by updating (or even suppressing) the <c>#pragma warning</c>
        /// inside the node.
        /// </summary>
        private T AddSuppression<T>( T node, IReadOnlyList<string> suppressionsOnThisElement )
            where T : SyntaxNode
        {
            var transformedNode = node;

            if ( !this._activeSuppressions.IsEmpty && node is not BaseTypeDeclarationSyntax )
            {
                // TODO: We are probably processing classes incorrectly.

                // Since we're adding suppressions, we need to visit each `#pragma warning` of the added node to update them.
                transformedNode = (T) this.Visit( transformedNode ).AssertNotNull();
            }

            if ( suppressionsOnThisElement.Any() )
            {
                // Add `#pragma warning` trivia around the node.
                var errorCodes = SeparatedList<ExpressionSyntax>( suppressionsOnThisElement.Distinct().OrderBy( e => e ).Select( IdentifierName ) );

                var disable = Trivia( SyntaxFactoryEx.PragmaWarningDirectiveTrivia( SyntaxKind.DisableKeyword, errorCodes ) )
                    .WithLinkerGeneratedFlags( LinkerGeneratedFlags.GeneratedSuppression );

                var restore = Trivia( SyntaxFactoryEx.PragmaWarningDirectiveTrivia( SyntaxKind.RestoreKeyword, errorCodes ) )
                    .WithLinkerGeneratedFlags( LinkerGeneratedFlags.GeneratedSuppression );

#pragma warning disable LAMA0832 // Avoid WithLeadingTrivia and WithTrailingTrivia calls.
                transformedNode = transformedNode
                    .WithLeadingTrivia( transformedNode.GetLeadingTrivia().InsertRange( 0, new[] { ElasticLineFeed, disable, ElasticLineFeed } ) )
                    .WithTrailingTrivia( transformedNode.GetTrailingTrivia().AddRange( new[] { ElasticLineFeed, restore, ElasticLineFeed } ) );
#pragma warning restore LAMA0832
            }

            return transformedNode;
        }

        private (SyntaxList<AttributeListSyntax> Attributes, List<SyntaxTrivia> Trivia)? RewriteDeclarationAttributeLists(
            SyntaxNode originalDeclaringNode,
            SyntaxList<AttributeListSyntax> attributeLists,
            SyntaxNode? originalNodeForTrivia = null )
        {
            if ( !this._transformationCollection.IsNodeWithModifiedAttributes( originalDeclaringNode ) )
            {
                return null;
            }

            // Resolve the symbol.
            var semanticModel = this._semanticModelProvider.GetSemanticModel( originalDeclaringNode.SyntaxTree );
            var symbol = semanticModel.GetDeclaredSymbol( originalDeclaringNode );

            if ( symbol == null )
            {
                return null;
            }

            // Find trivia that's directly on the declaration (and not on its attributes).
            originalNodeForTrivia ??= originalDeclaringNode;

            if ( attributeLists.Any() )
            {
                originalNodeForTrivia = originalNodeForTrivia.RemoveNodes( attributeLists, SyntaxRemoveOptions.KeepNoTrivia )!;
            }

            var originalDeclarationTrivia = originalNodeForTrivia.GetLeadingTrivia();

            var outputLists = new List<AttributeListSyntax>();
            var outputTrivias = new List<SyntaxTrivia>( originalDeclarationTrivia );
            SyntaxGenerationContext? syntaxGenerationContext = null;

            this.RewriteAttributeLists(
                Ref.FromSymbol( symbol, this._compilation.CompilationContext ),
                SyntaxKind.None,
                originalDeclaringNode,
                attributeLists,
                ( a, n ) => a.ContainingDeclaration.GetPrimaryDeclarationSyntax() == n,
                outputLists,
                outputTrivias,
                ref syntaxGenerationContext );

            switch ( symbol )
            {
                case IMethodSymbol method:
                    this.RewriteAttributeLists(
                        Ref.ReturnParameter( method, this._compilation.CompilationContext ),
                        SyntaxKind.ReturnKeyword,
                        originalDeclaringNode,
                        attributeLists,
                        ( a, n ) => a.ContainingDeclaration.ContainingDeclaration!.GetPrimaryDeclarationSyntax() == n,
                        outputLists,
                        outputTrivias,
                        ref syntaxGenerationContext );

                    break;
            }

            if ( outputLists.Count == 0 )
            {
                return (default, outputTrivias);
            }
            else
            {
                return (List( outputLists ), outputTrivias);
            }
        }

        private void RewriteAttributeLists(
            Ref<IDeclaration> target,
            SyntaxKind targetKind,
            SyntaxNode originalDeclaringNode,
            SyntaxList<AttributeListSyntax> inputAttributeLists,
            Func<AttributeBuilder, SyntaxNode, bool> isPrimaryNode,
            List<AttributeListSyntax> outputAttributeLists,
            List<SyntaxTrivia> outputTrivia,
            ref SyntaxGenerationContext? syntaxGenerationContext )
        {
            // Get the final list of attributes.
            var finalModelAttributes = this._compilation.GetAttributeCollection( target );

            List<SyntaxTrivia>? firstListLeadingTrivia = null;
            var isFirstList = true;

            // Remove attributes from the list.
            foreach ( var list in inputAttributeLists )
            {
                var wasFirstList = isFirstList;
                isFirstList = false;

                // Skip the different target kinds.
                if ( list.Target == null )
                {
                    if ( targetKind != SyntaxKind.None )
                    {
                        continue;
                    }
                }
                else
                {
                    if ( !list.Target.Identifier.IsKind( targetKind ) )
                    {
                        continue;
                    }
                }

                var modifiedList = list;

                foreach ( var attribute in list.Attributes )
                {
                    if ( !finalModelAttributes.Any( a => a.IsSyntax( attribute ) ) )
                    {
                        modifiedList = modifiedList.RemoveNode( attribute, SyntaxRemoveOptions.KeepDirectives )!;
                    }
                }

                if ( modifiedList.Attributes.Count > 0 )
                {
                    outputAttributeLists.Add( modifiedList );
                }
                else
                {
                    // If we are removing a custom attribute, keep its trivia.
                    foreach ( var trivia in list.GetLeadingTrivia() )
                    {
                        if ( trivia.Kind() is SyntaxKind.SingleLineCommentTrivia
                            or SyntaxKind.MultiLineCommentTrivia
                            or SyntaxKind.SingleLineDocumentationCommentTrivia
                            or SyntaxKind.MultiLineDocumentationCommentTrivia )
                        {
                            List<SyntaxTrivia> targetList;

                            if ( wasFirstList )
                            {
                                // Trivia preceding the first attribute list needs to before the first final attribute list.
                                targetList = firstListLeadingTrivia ??= new List<SyntaxTrivia>();
                            }
                            else
                            {
                                targetList = outputTrivia;
                            }

                            targetList.Add( trivia );

                            if ( trivia.Kind() is SyntaxKind.SingleLineCommentTrivia or SyntaxKind.SingleLineDocumentationCommentTrivia )
                            {
                                targetList.Add( ElasticLineFeed );
                            }

                            break;
                        }
                    }
                }
            }

            // Add new attributes.
            foreach ( var attribute in finalModelAttributes )
            {
                if ( attribute.Target is AttributeBuilder attributeBuilder && isPrimaryNode( attributeBuilder, originalDeclaringNode ) )
                {
                    syntaxGenerationContext ??= this.CompilationContext.GetSyntaxGenerationContext( this.SyntaxGenerationOptions, originalDeclaringNode );

                    var newAttribute = syntaxGenerationContext.SyntaxGenerator.Attribute( attributeBuilder )
                        .AssertNotNull();

                    var newList = AttributeList( SingletonSeparatedList( newAttribute ) )
                        .WithTrailingTriviaIfNecessary( ElasticLineFeed, syntaxGenerationContext.NormalizeWhitespace )
                        .WithAdditionalAnnotations(
                            attributeBuilder.ParentAdvice?.Aspect.AspectClass.GeneratedCodeAnnotation ?? FormattingAnnotations.SystemGeneratedCodeAnnotation );

                    if ( targetKind != SyntaxKind.None )
                    {
                        newList = newList.WithTarget( AttributeTargetSpecifier( Token( targetKind ) ) );
                    }

                    if ( outputTrivia.Any() && !outputAttributeLists.Any() )
                    {
                        newList = newList.WithLeadingTriviaIfNecessary(
                            newList.GetLeadingTrivia().InsertRange( 0, outputTrivia ),
                            syntaxGenerationContext.PreserveTrivia );

                        outputTrivia.Clear();
                    }

                    outputAttributeLists.Add( newList );
                }
            }

            if ( firstListLeadingTrivia != null )
            {
                if ( outputAttributeLists.Count > 0 )
                {
                    syntaxGenerationContext ??= this._defaultSyntaxGenerationContext;

                    outputAttributeLists[0] =
                        outputAttributeLists[0]
                            .WithLeadingTriviaIfNecessary(
                                outputAttributeLists[0].GetLeadingTrivia().AddRange( firstListLeadingTrivia ),
                                syntaxGenerationContext.PreserveTrivia );
                }
                else
                {
                    outputTrivia.InsertRange( 0, firstListLeadingTrivia );
                }
            }
        }

        private T ReplaceAttributes<T>( T node, (SyntaxList<AttributeListSyntax> Attributes, List<SyntaxTrivia> Trivia)? attributesTuple )
            where T : MemberDeclarationSyntax
        {
            if ( attributesTuple is var (attributes, trivia) )
            {
                if ( trivia.ShouldBePreserved( this.PreserveTrivia ) )
                {
#pragma warning disable LAMA0832 // Avoid WithLeadingTrivia and WithTrailingTrivia calls.
                    return (T) node.WithAttributeLists( default ).WithLeadingTrivia( trivia ).WithAttributeLists( attributes );
#pragma warning restore LAMA0832
                }
                else
                {
                    return (T) node.WithAttributeLists( attributes );
                }
            }
            else
            {
                return node;
            }
        }

        public override SyntaxNode VisitClassDeclaration( ClassDeclarationSyntax node ) => this.VisitTypeDeclaration( node );

        public override SyntaxNode VisitStructDeclaration( StructDeclarationSyntax node ) => this.VisitTypeDeclaration( node );

        public override SyntaxNode VisitInterfaceDeclaration( InterfaceDeclarationSyntax node ) => this.VisitTypeDeclaration( node );

        public override SyntaxNode VisitRecordDeclaration( RecordDeclarationSyntax node ) => this.VisitTypeDeclaration( node );

        public override SyntaxNode VisitEnumDeclaration( EnumDeclarationSyntax node )
        {
            var originalNode = node;
            var members = new List<EnumMemberDeclarationSyntax>( node.Members.Count );

            using ( var suppressionContext = this.WithSuppressions( node ) )
            {
                // Process the type members.
                foreach ( var member in node.Members )
                {
                    var visitedMember = this.VisitEnumMemberDeclarationCore( member );

                    using ( var memberSuppressions = this.WithSuppressions( member ) )
                    {
                        var memberWithSuppressions = this.AddSuppression( visitedMember, memberSuppressions.NewSuppressions );
                        members.Add( memberWithSuppressions );
                    }
                }

                node = this.AddSuppression( node, suppressionContext.NewSuppressions );
            }

            node = node.WithMembers( SeparatedList( members ) );

            // Rewrite attributes.
            var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );
            node = this.ReplaceAttributes( node, rewrittenAttributes );

            return node;
        }

        public override SyntaxNode VisitDelegateDeclaration( DelegateDeclarationSyntax node )
        {
            var originalNode = node;

            using ( var suppressionContext = this.WithSuppressions( node ) )
            {
                node = this.AddSuppression( node, suppressionContext.NewSuppressions );
            }

            // Rewrite attributes.
            var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );
            node = this.ReplaceAttributes( node, rewrittenAttributes );

            return node;
        }

        private SyntaxNode VisitTypeDeclaration<T>( T node )
            where T : TypeDeclarationSyntax
        {
            var originalNode = node;
            var members = new List<MemberDeclarationSyntax>( node.Members.Count );
            var additionalBaseList = this._transformationCollection.GetIntroducedInterfacesForTypeDeclaration( node );
            var syntaxGenerationContext = this.CompilationContext.GetSyntaxGenerationContext( this.SyntaxGenerationOptions, node );

            var baseList = node.BaseList;
            var parameterList = node.GetParameterList();

            this.ApplyMemberLevelTransformationsToPrimaryConstructor(
                node,
                syntaxGenerationContext,
                ref baseList,
                ref parameterList );

            using ( var suppressionContext = this.WithSuppressions( node ) )
            {
                // Process the type members.
                foreach ( var member in node.Members )
                {
                    foreach ( var visitedMember in this.VisitMember( member ) )
                    {
                        using ( var memberSuppressions = this.WithSuppressions( member ) )
                        {
                            var memberWithSuppressions = this.AddSuppression( visitedMember, memberSuppressions.NewSuppressions );
                            members.Add( memberWithSuppressions );
                        }
                    }

                    // We have to call AddIntroductionsOnPosition outside of the previous suppression scope, otherwise we don't get new suppressions.
                    AddInjectionsOnPosition( new InsertPosition( InsertPositionRelation.After, member ) );
                }

                AddInjectionsOnPosition( new InsertPosition( InsertPositionRelation.Within, node ) );

                node = this.AddSuppression( node, suppressionContext.NewSuppressions );

                // If the type has no braces, add them.
                if ( node.OpenBraceToken.IsKind( SyntaxKind.None ) && members.Count > 0 )
                {
                    // TODO: trivias.
                    node = (T) node
                        .WithOpenBraceToken( Token( SyntaxKind.OpenBraceToken ).AddColoringAnnotation( TextSpanClassification.GeneratedCode ) )
                        .WithCloseBraceToken( Token( SyntaxKind.CloseBraceToken ).AddColoringAnnotation( TextSpanClassification.GeneratedCode ) )
                        .WithSemicolonToken( default );
                }

                node = (T) node.WithMembers( List( members ) );

                // Process the type bases.
                if ( additionalBaseList.Any() )
                {
                    if ( baseList == null )
                    {
                        node = (T) node
                            .WithIdentifier(
                                node.Identifier.WithTrailingTriviaIfNecessary(
                                    default,
                                    syntaxGenerationContext.PreserveTrivia || node.Identifier.ContainsDirectives ) )
                            .WithBaseList(
                                BaseList( SeparatedList( additionalBaseList.SelectAsReadOnlyList( i => i.Syntax ) ) )
                                    .WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation ) )
                            .WithTrailingTriviaIfNecessary( node.Identifier.TrailingTrivia, syntaxGenerationContext.PreserveTrivia );
                    }
                    else
                    {
                        node = (T) node.WithBaseList(
                            BaseList(
                                baseList.Types.AddRange(
                                    additionalBaseList.SelectAsReadOnlyList(
                                        i => i.Syntax.WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation ) ) ) ) );
                    }
                }
                else if ( baseList != null )
                {
                    node = (T) node.WithBaseList( baseList );
                }

                node = (T) node.WithParameterList( parameterList );

                // Rewrite attributes.
                var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );
                node = this.ReplaceAttributes( node, rewrittenAttributes );

                return node;
            }

            // TODO: Try to avoid closure allocation.
            void AddInjectionsOnPosition( InsertPosition position )
            {
                var injectedMembersAtPosition = this._transformationCollection.GetInjectedMembersOnPosition( position );

                foreach ( var injectedMember in injectedMembersAtPosition )
                {
                    // We should inject into a correct syntax tree.
                    Invariant.Assert( injectedMember.TargetSyntaxTree == originalNode.SyntaxTree );

                    // Allow for tracking of the node inserted.
                    // IMPORTANT: This need to be here and cannot be in injectedMember.Syntax, result of TrackNodes is not trackable!
                    var injectedNode = injectedMember.Syntax.TrackNodes( injectedMember.Syntax );

                    switch ( injectedMember.Declaration )
                    {
                        case IMethod or IConstructor:
                            // TODO: AssertNotNull is needed due to some weird bug in Roslyn.
                            var entryStatements = this._transformationCollection.GetInjectedEntryStatements( injectedMember );
                            var exitStatements = this._transformationCollection.GetInjectedExitStatements( injectedMember );

                            injectedNode = InjectStatementsIntoMemberDeclaration( injectedMember.Declaration, entryStatements, exitStatements, injectedNode );

                            break;

                        case IPropertyOrIndexer propertyOrIndexer:
                            if ( propertyOrIndexer.GetMethod != null )
                            {
                                var getEntryStatements = this._transformationCollection.GetInjectedEntryStatements(
                                    propertyOrIndexer.GetMethod,
                                    injectedMember );

                                var getExitStatements = this._transformationCollection.GetInjectedExitStatements( propertyOrIndexer.GetMethod, injectedMember );

                                injectedNode = InjectStatementsIntoMemberDeclaration(
                                    propertyOrIndexer.GetMethod,
                                    getEntryStatements,
                                    getExitStatements,
                                    injectedNode );
                            }

                            if ( propertyOrIndexer.SetMethod != null )
                            {
                                var setEntryStatements = this._transformationCollection.GetInjectedEntryStatements(
                                    propertyOrIndexer.SetMethod,
                                    injectedMember );

                                var setExitStatements = this._transformationCollection.GetInjectedExitStatements( propertyOrIndexer.SetMethod, injectedMember );

                                injectedNode = InjectStatementsIntoMemberDeclaration(
                                    propertyOrIndexer.SetMethod,
                                    setEntryStatements,
                                    setExitStatements,
                                    injectedNode );
                            }

                            break;
                    }

                    injectedNode = injectedNode
                        .WithLeadingTriviaIfNecessary( new SyntaxTriviaList( ElasticLineFeed, ElasticLineFeed ), syntaxGenerationContext.NormalizeWhitespace )
                        .WithGeneratedCodeAnnotation(
                            injectedMember.Transformation?.ParentAdvice.Aspect.AspectClass.GeneratedCodeAnnotation
                            ?? FormattingAnnotations.SystemGeneratedCodeAnnotation );

                    switch ( injectedNode )
                    {
                        case ConstructorDeclarationSyntax constructorDeclaration:
                            {
                                if ( injectedMember.DeclarationBuilder != null &&
                                     this._transformationCollection.TryGetMemberLevelTransformations(
                                         injectedMember.DeclarationBuilder.AssertNotNull(),
                                         out var memberLevelTransformations ) )
                                {
                                    injectedNode = this.ApplyMemberLevelTransformations(
                                        constructorDeclaration,
                                        memberLevelTransformations,
                                        syntaxGenerationContext );
                                }

                                break;
                            }
                    }

                    using ( var suppressions = this.WithSuppressions( injectedMember.Declaration ) )
                    {
                        injectedNode = this.AddSuppression( injectedNode, suppressions.NewSuppressions );
                    }

                    members.Add( injectedNode );
                }
            }
        }

        private static MemberDeclarationSyntax InjectStatementsIntoMemberDeclaration(
            IMemberOrNamedType contextDeclaration,
            IReadOnlyList<StatementSyntax> entryStatements,
            IReadOnlyList<StatementSyntax> exitStatements,
            MemberDeclarationSyntax currentNode )
        {
            if ( entryStatements.Count == 0 && exitStatements.Count == 0 )
            {
                return currentNode;
            }

            switch ( currentNode )
            {
                case ConstructorDeclarationSyntax { Body: { } body } constructor:
                    return constructor.WithBody( ReplaceBlock( contextDeclaration, entryStatements, exitStatements, body ) );

                case ConstructorDeclarationSyntax { ExpressionBody: { } expressionBody } constructor:
                    return
                        constructor.PartialUpdate(
                            expressionBody: null,
                            semicolonToken: default(SyntaxToken),
                            body: ReplaceExpression( entryStatements, exitStatements, expressionBody.Expression, true ) );

                // Static constructor overrides also go here.
                case MethodDeclarationSyntax { Body: { } body } method:
                    return method.WithBody( ReplaceBlock( contextDeclaration, entryStatements, exitStatements, body ) );

                case MethodDeclarationSyntax { ExpressionBody: { } expressionBody } method:
                    var returnsVoid =
                        contextDeclaration switch
                        {
                            IConstructor => true,
                            IMethod { IsAsync: false } contextMethod => contextMethod.ReturnType.Is( SpecialType.Void ),
                            IMethod { IsAsync: true } contextMethod => contextMethod.GetAsyncInfo().ResultType.Is( SpecialType.Void ),
                            _ => throw new InvalidOperationException( $"Not supported: {contextDeclaration}" )
                        };

                    return
                        method.PartialUpdate(
                            expressionBody: null,
                            semicolonToken: default(SyntaxToken),
                            body: ReplaceExpression( entryStatements, exitStatements, expressionBody.Expression, returnsVoid ) );

                case MethodDeclarationSyntax { Body: null, ExpressionBody: null } method:
                    Invariant.Assert( method.Modifiers.All( m => !m.IsKind( SyntaxKind.AbstractKeyword ) && !m.IsKind( SyntaxKind.ExternKeyword ) ) );

                    return method.PartialUpdate(
                        body: Block( entryStatements.Concat( exitStatements ) ),
                        semicolonToken: default(SyntaxToken) );

                case OperatorDeclarationSyntax { Body: { } body } @operator:
                    return @operator.WithBody( ReplaceBlock( contextDeclaration, entryStatements, exitStatements, body ) );

                case OperatorDeclarationSyntax { ExpressionBody: { } expressionBody } @operator:
                    return
                        @operator.PartialUpdate(
                            expressionBody: null,
                            semicolonToken: default(SyntaxToken),
                            body: ReplaceExpression( entryStatements, exitStatements, expressionBody.Expression, false ) );

                case PropertyDeclarationSyntax { ExpressionBody: { } expressionBody } property:
                    Invariant.Assert( contextDeclaration is IMethod { MethodKind: MethodKind.PropertyGet } );

                    return
                        property.PartialUpdate(
                            expressionBody: null,
                            semicolonToken: default(SyntaxToken),
                            accessorList: AccessorList(
                                SingletonList(
                                    AccessorDeclaration(
                                        SyntaxKind.GetAccessorDeclaration,
                                        ReplaceExpression( entryStatements, exitStatements, expressionBody.Expression, false ) ) ) ) );

                case IndexerDeclarationSyntax { ExpressionBody: { } expressionBody } indexer:
                    Invariant.Assert( contextDeclaration is IMethod { MethodKind: MethodKind.PropertyGet } );

                    return
                        indexer.PartialUpdate(
                            expressionBody: null,
                            semicolonToken: default(SyntaxToken),
                            accessorList: AccessorList(
                                SingletonList(
                                    AccessorDeclaration(
                                        SyntaxKind.GetAccessorDeclaration,
                                        ReplaceExpression( entryStatements, exitStatements, expressionBody.Expression, false ) ) ) ) );

                case BasePropertyDeclarationSyntax { AccessorList: { } accessorList } propertyOrIndexer:
                    Invariant.Assert( contextDeclaration is IMethod { MethodKind: MethodKind.PropertyGet or MethodKind.PropertySet } );

                    return
                        propertyOrIndexer.WithAccessorList(
                            accessorList.WithAccessors(
                                List(
                                    accessorList.Accessors.SelectAsArray(
                                        a =>
                                            IsMatchingAccessor( a, contextDeclaration )
                                                ? a switch
                                                {
                                                    { Body: { } body } => a.WithBody(
                                                        ReplaceBlock( contextDeclaration, entryStatements, exitStatements, body ) ),
                                                    { ExpressionBody: { } expressionBody } =>
                                                        a.PartialUpdate(
                                                            expressionBody: null,
                                                            semicolonToken: default(SyntaxToken),
                                                            body: ReplaceExpression(
                                                                entryStatements,
                                                                exitStatements,
                                                                expressionBody.Expression,
                                                                a.Kind() is not SyntaxKind.GetAccessorDeclaration ) ),
                                                    _ => throw new AssertionFailedException( $"Not supported: {a.Kind()}" )
                                                }
                                                : a ) ) ) );

                    static bool IsMatchingAccessor( AccessorDeclarationSyntax accessorDeclaration, IDeclaration contextDeclaration )
                    {
                        return (accessorDeclaration.Kind(), contextDeclaration) switch
                        {
                            (SyntaxKind.GetAccessorDeclaration, IMethod { MethodKind: MethodKind.PropertyGet }) => true,
                            (SyntaxKind.SetAccessorDeclaration, IMethod { MethodKind: MethodKind.PropertySet }) => true,
                            (SyntaxKind.InitAccessorDeclaration, IMethod { MethodKind: MethodKind.PropertySet }) => true,
                            _ => false
                        };
                    }

                default:
                    throw new AssertionFailedException( $"Not supported: {currentNode.Kind()}" );
            }

            static BlockSyntax ReplaceBlock(
                IDeclaration declaration,
                IReadOnlyList<StatementSyntax> entryStatements,
                IReadOnlyList<StatementSyntax> exitStatements,
                BlockSyntax targetBlock )
            {
                if ( exitStatements.Count > 0 )
                {
                    // Patterns recognizing bodies generated by AuxiliaryMemberFactory.
                    switch ( targetBlock )
                    {
                        case { Statements: [ExpressionStatementSyntax expressionStatement] }:
                            return
                                Block(
                                    Block( List( entryStatements ) )
                                        .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock ),
                                    expressionStatement,
                                    Block( List( exitStatements ) )
                                        .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock ) );

                        case { Statements: [LocalDeclarationStatementSyntax localDeclarationStatement, ReturnStatementSyntax returnStatement] }:
                            return
                                Block(
                                    Block( List( entryStatements ) )
                                        .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock ),
                                    localDeclarationStatement,
                                    Block( List( exitStatements ) )
                                        .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock ),
                                    returnStatement );

                        case { Statements: [LocalDeclarationStatementSyntax localDeclarationStatement, ForEachStatementSyntax foreachStatement] }:
                            return
                                Block(
                                    Block( List( entryStatements ) )
                                        .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock ),
                                    localDeclarationStatement,
                                    Block( List( exitStatements ) ).WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock ),
                                    foreachStatement );

                        case
                        {
                            Statements:
                            [
                                LocalDeclarationStatementSyntax bufferedEnumerableLocal, LocalDeclarationStatementSyntax returnValueLocal,
                                WhileStatementSyntax whileStatement
                            ]
                        }:
                            return
                                Block(
                                    Block( List( entryStatements ) )
                                        .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock ),
                                    bufferedEnumerableLocal,
                                    returnValueLocal,
                                    Block(
                                            List(
                                                exitStatements.Select(
                                                    ( s, i ) =>
                                                    {
                                                        if ( i == 0 )
                                                        {
                                                            return s;
                                                        }
                                                        else
                                                        {
                                                            var declarator = returnValueLocal.Declaration.Variables.Single();

                                                            return
                                                                Block(
                                                                        ExpressionStatement(
                                                                            AssignmentExpression(
                                                                                SyntaxKind.SimpleAssignmentExpression,
                                                                                IdentifierName( declarator.Identifier.ValueText ),
                                                                                declarator.Initializer.AssertNotNull().Value ) ),
                                                                        s )
                                                                    .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
                                                        }
                                                    } ) ) )
                                        .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock ),
                                    whileStatement );

                        default:
                            throw new AssertionFailedException( $"Unsupported form of body with exit statements for: {declaration}" );
                    }
                }
                else
                {
                    return
                        Block(
                            Block( List( entryStatements ) )
                                .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock ),
                            targetBlock
                                .WithSourceCodeAnnotationIfNotGenerated()
                                .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock ) );
                }
            }

            static BlockSyntax ReplaceExpression(
                IReadOnlyList<StatementSyntax> entryStatements,
                IReadOnlyList<StatementSyntax> exitStatements,
                ExpressionSyntax targetExpression,
                bool returnsVoid )
            {
                // Auxiliary bodies that may receive exit statements are never expression bodies.
                Invariant.Assert( exitStatements.Count == 0 );

                return
                    Block(
                        Block( List( entryStatements ) )
                            .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock ),
                        returnsVoid
                            ? ExpressionStatement( targetExpression.WithSourceCodeAnnotationIfNotGenerated() )
                            : ReturnStatement(
                                Token( default, SyntaxKind.ReturnKeyword, TriviaList( ElasticSpace ) ),
                                targetExpression.WithSourceCodeAnnotationIfNotGenerated(),
                                Token( SyntaxKind.SemicolonToken ) ) );
            }
        }

        private IReadOnlyList<MemberDeclarationSyntax> VisitMember( MemberDeclarationSyntax member )
        {
            static MemberDeclarationSyntax[] Singleton( MemberDeclarationSyntax m )
            {
                return new[] { m };
            }

            return member switch
            {
                ConstructorDeclarationSyntax constructor => Singleton( this.VisitConstructorDeclarationCore( constructor ) ),
                MethodDeclarationSyntax method => Singleton( this.VisitMethodDeclarationCore( method ) ),
                PropertyDeclarationSyntax property => Singleton( this.VisitPropertyDeclarationCore( property ) ),
                IndexerDeclarationSyntax indexer => Singleton( this.VisitIndexerDeclarationCore( indexer ) ),
                OperatorDeclarationSyntax @operator => Singleton( this.VisitOperatorDeclarationCore( @operator ) ),
                EventDeclarationSyntax @event => Singleton( this.VisitEventDeclarationCore( @event ) ),
                FieldDeclarationSyntax field => this.VisitFieldDeclarationCore( field ),
                EventFieldDeclarationSyntax @eventField => this.VisitEventFieldDeclarationCore( @eventField ),
                _ => Singleton( (MemberDeclarationSyntax) this.Visit( member )! )
            };
        }

        private ConstructorDeclarationSyntax ApplyMemberLevelTransformations(
            ConstructorDeclarationSyntax constructorDeclaration,
            MemberLevelTransformations memberLevelTransformations,
            SyntaxGenerationContext syntaxGenerationContext )
        {
            constructorDeclaration = constructorDeclaration.WithParameterList(
                AppendParameters( constructorDeclaration.ParameterList, memberLevelTransformations.Parameters, syntaxGenerationContext ) );

            constructorDeclaration = constructorDeclaration.WithInitializer(
                this.AppendInitializerArguments( constructorDeclaration.Initializer, memberLevelTransformations.Arguments ) );

            return constructorDeclaration;
        }

        private void ApplyMemberLevelTransformationsToPrimaryConstructor(
            TypeDeclarationSyntax typeDeclaration,
            SyntaxGenerationContext syntaxGenerationContext,
            ref BaseListSyntax? baseList,
            ref ParameterListSyntax? parameterList )
        {
            if ( !this._transformationCollection.TryGetMemberLevelTransformations( typeDeclaration, out var memberLevelTransformations ) )
            {
                return;
            }

            Invariant.AssertNot( typeDeclaration.BaseList == null && memberLevelTransformations.Arguments.Length > 0 );
            Invariant.AssertNot( typeDeclaration.GetParameterList() == null );

            parameterList = AppendParameters( typeDeclaration.GetParameterList()!, memberLevelTransformations.Parameters, syntaxGenerationContext );
            baseList = typeDeclaration.BaseList;

            if ( memberLevelTransformations.Arguments.Length > 0 )
            {
                var baseTypeSyntax = typeDeclaration.BaseList.AssertNotNull().Types[0];

                BaseTypeSyntax newBaseTypeSyntax;

                switch ( baseTypeSyntax )
                {
                    case SimpleBaseTypeSyntax simpleBaseType:
                        newBaseTypeSyntax =
                            PrimaryConstructorBaseType(
                                simpleBaseType.Type,
                                ArgumentList( SeparatedList( memberLevelTransformations.Arguments.Select( x => x.ToSyntax() ) ) ) );

                        break;

                    case PrimaryConstructorBaseTypeSyntax primaryCtorBaseType:
                        newBaseTypeSyntax =
                            primaryCtorBaseType
                                .WithArgumentList(
                                    primaryCtorBaseType.ArgumentList.AddArguments( memberLevelTransformations.Arguments.SelectAsArray( x => x.ToSyntax() ) ) );

                        break;

                    default:
                        throw new AssertionFailedException( $"Unexpected base type: {baseTypeSyntax.Kind()}" );
                }

                // TODO: This may be slower than replacing specific index.
                baseList = typeDeclaration.BaseList.ReplaceNode( baseTypeSyntax, newBaseTypeSyntax );
            }
        }

        private static ParameterListSyntax AppendParameters(
            ParameterListSyntax existingParameters,
            ImmutableArray<IntroduceParameterTransformation> newParameters,
            SyntaxGenerationContext syntaxGenerationContext )
        {
            if ( newParameters.IsEmpty )
            {
                return existingParameters;
            }
            else
            {
                if ( existingParameters.Parameters.Any( p => p.Modifiers.Any( m => m.IsKind( SyntaxKind.ParamsKeyword ) ) ) )
                {
                    return existingParameters.WithParameters(
                        existingParameters.Parameters.InsertRange(
                            existingParameters.Parameters.Count - 1,
                            newParameters.Select(
                                x => x.ToSyntax( syntaxGenerationContext )
                                    .WithTrailingTriviaIfNecessary( ElasticSpace, syntaxGenerationContext.NormalizeWhitespace ) ) ) );
                }
                else
                {
                    return existingParameters.WithParameters(
                        existingParameters.Parameters.AddRange(
                            newParameters.Select(
                                x => x.ToSyntax( syntaxGenerationContext )
                                    .WithTrailingTriviaIfNecessary( ElasticSpace, syntaxGenerationContext.NormalizeWhitespace ) ) ) );
                }
            }
        }

        private ConstructorInitializerSyntax? AppendInitializerArguments(
            ConstructorInitializerSyntax? initializerSyntax,
            ImmutableArray<IntroduceConstructorInitializerArgumentTransformation> newArguments )
        {
            if ( newArguments.IsEmpty )
            {
                return initializerSyntax;
            }

            var newArgumentsSyntax = newArguments.Select( a => a.ToSyntax().WithTrailingTriviaIfNecessary( ElasticSpace, this.NormalizeWhitespace ) );

            if ( initializerSyntax == null )
            {
                return ConstructorInitializer(
                    SyntaxKind.BaseConstructorInitializer,
                    ArgumentList( SeparatedList( newArgumentsSyntax ) ) );
            }
            else
            {
                return initializerSyntax.WithArgumentList( initializerSyntax.ArgumentList.AddArguments( newArgumentsSyntax.ToArray() ) );
            }
        }

        private IReadOnlyList<FieldDeclarationSyntax> VisitFieldDeclarationCore( FieldDeclarationSyntax node )
        {
            var originalNode = node;

            // Rewrite attributes.
            if ( originalNode.Declaration.Variables.Count > 1
                 && originalNode.Declaration.Variables.Any( this._transformationCollection.IsNodeWithModifiedAttributes ) )
            {
                // TODO: This needs to use rewritten variable declaration or do removal in place.
                var members = new List<FieldDeclarationSyntax>( originalNode.Declaration.Variables.Count );

                // If we have changes in attributes and several members, we have to split them.
                foreach ( var variable in originalNode.Declaration.Variables )
                {
                    if ( this._transformationCollection.IsRemovedSyntax( variable ) )
                    {
                        continue;
                    }

                    var declaration = VariableDeclaration( node.Declaration.Type, SingletonSeparatedList( variable ) );
                    var attributes = this.RewriteDeclarationAttributeLists( variable, originalNode.AttributeLists, originalNode );

                    var fieldDeclaration = FieldDeclaration(
                        default,
                        node.Modifiers,
                        declaration,
                        Token( default, SyntaxKind.SemicolonToken, new SyntaxTriviaList( ElasticLineFeed ) ) );

                    fieldDeclaration = this.ReplaceAttributes( fieldDeclaration, attributes );

                    members.Add( fieldDeclaration );
                }

                return members;
            }
            else
            {
                var rewrittenAttributes = this.RewriteDeclarationAttributeLists(
                    originalNode.Declaration.Variables[0],
                    originalNode.AttributeLists,
                    originalNode );

                node = this.ReplaceAttributes( node, rewrittenAttributes );

                var anyChangeToVariables = false;
                var rewrittenVariables = new List<VariableDeclaratorSyntax>();

                foreach ( var variable in originalNode.Declaration.Variables )
                {
                    if ( this._transformationCollection.IsRemovedSyntax( variable ) )
                    {
                        anyChangeToVariables = true;

                        continue;
                    }

                    rewrittenVariables.Add( variable );
                }

                if ( anyChangeToVariables )
                {
                    if ( rewrittenVariables.Count > 0 )
                    {
                        return new[] { node.WithDeclaration( node.Declaration.WithVariables( SeparatedList( rewrittenVariables ) ) ) };
                    }
                    else
                    {
                        return Array.Empty<FieldDeclarationSyntax>();
                    }
                }
                else
                {
                    return new[] { node };
                }
            }
        }

        private EnumMemberDeclarationSyntax VisitEnumMemberDeclarationCore( EnumMemberDeclarationSyntax node )
        {
            var originalNode = node;

            // Rewrite attributes.
            var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );
            node = this.ReplaceAttributes( node, rewrittenAttributes );

            return node;
        }

        private ConstructorDeclarationSyntax VisitConstructorDeclarationCore( ConstructorDeclarationSyntax node )
        {
            var originalNode = node;

            node = (ConstructorDeclarationSyntax) this.VisitConstructorDeclaration( node )!;

            if ( this._transformationCollection.TryGetMemberLevelTransformations( node, out var memberLevelTransformations ) )
            {
                var syntaxGenerationContext = this.CompilationContext.GetSyntaxGenerationContext( this.SyntaxGenerationOptions, node );
                node = this.ApplyMemberLevelTransformations( node, memberLevelTransformations, syntaxGenerationContext );
            }

            var semanticModel = this._semanticModelProvider.GetSemanticModel( originalNode.SyntaxTree );
            var symbol = semanticModel.GetDeclaredSymbol( originalNode );

            if ( symbol != null )
            {
                var constructor = (IConstructor) this._compilation.GetDeclaration( symbol );
                var entryStatements = this._transformationCollection.GetInjectedEntryStatements( constructor );

                node = (ConstructorDeclarationSyntax) InjectStatementsIntoMemberDeclaration(
                    constructor,
                    entryStatements,
                    Array.Empty<StatementSyntax>(),
                    node );
            }

            // Rewrite attributes.
            var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );
            node = this.ReplaceAttributes( node, rewrittenAttributes );

            return node;
        }

        private MethodDeclarationSyntax VisitMethodDeclarationCore( MethodDeclarationSyntax node )
        {
            var originalNode = node;

            node = (MethodDeclarationSyntax) this.VisitMethodDeclaration( node )!;

            var semanticModel = this._semanticModelProvider.GetSemanticModel( originalNode.SyntaxTree );
            var symbol = semanticModel.GetDeclaredSymbol( originalNode );

            if ( symbol != null && symbol is not { PartialImplementationPart: not null } )
            {
                var method = (IMethod) this._compilation.GetDeclaration( symbol );
                var entryStatements = this._transformationCollection.GetInjectedEntryStatements( method );

                node = (MethodDeclarationSyntax) InjectStatementsIntoMemberDeclaration( method, entryStatements, Array.Empty<StatementSyntax>(), node );
            }

            // Rewrite attributes.
            var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );
            node = this.ReplaceAttributes( node, rewrittenAttributes );

            return node;
        }

        private OperatorDeclarationSyntax VisitOperatorDeclarationCore( OperatorDeclarationSyntax node )
        {
            var originalNode = node;

            node = (OperatorDeclarationSyntax) this.VisitOperatorDeclaration( node )!;

            var semanticModel = this._semanticModelProvider.GetSemanticModel( originalNode.SyntaxTree );
            var symbol = semanticModel.GetDeclaredSymbol( originalNode );

            if ( symbol != null )
            {
                var method = (IMethod) this._compilation.GetDeclaration( symbol );
                var entryStatements = this._transformationCollection.GetInjectedEntryStatements( method );

                node = (OperatorDeclarationSyntax) InjectStatementsIntoMemberDeclaration( method, entryStatements, Array.Empty<StatementSyntax>(), node );
            }

            // Rewrite attributes.
            var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );
            node = this.ReplaceAttributes( node, rewrittenAttributes );

            return node;
        }

        public override SyntaxNode VisitParameter( ParameterSyntax node )
        {
            var originalNode = node;
            node = (ParameterSyntax) base.VisitParameter( node )!;

            // Rewrite attributes.
            var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );

            if ( rewrittenAttributes is var (attributes, trivia) )
            {
                if ( trivia.ShouldBePreserved( this.PreserveTrivia ) )
                {
#pragma warning disable LAMA0832 // Avoid WithLeadingTrivia and WithTrailingTrivia calls.
                    node = node.WithAttributeLists( default ).WithLeadingTrivia( trivia ).WithAttributeLists( attributes );
#pragma warning restore LAMA0832
                }
                else
                {
                    node = node.WithAttributeLists( attributes );
                }
            }

            return node;
        }

        public override SyntaxNode VisitTypeParameter( TypeParameterSyntax node )
        {
            var originalNode = node;
            node = (TypeParameterSyntax) base.VisitTypeParameter( node )!;

            // Rewrite attributes.
            var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );

            if ( rewrittenAttributes is var (attributes, trivia) )
            {
                if ( trivia.ShouldBePreserved( this.PreserveTrivia ) )
                {
#pragma warning disable LAMA0832 // Avoid WithLeadingTrivia and WithTrailingTrivia calls.
                    node = node.WithAttributeLists( default ).WithLeadingTrivia( trivia ).WithAttributeLists( attributes );
#pragma warning restore LAMA0832
                }
                else
                {
                    node = node.WithAttributeLists( attributes );
                }
            }

            return node;
        }

        private PropertyDeclarationSyntax VisitPropertyDeclarationCore( PropertyDeclarationSyntax node )
        {
            var originalNode = node;

            node = (PropertyDeclarationSyntax) this.VisitPropertyDeclaration( node )!;

            var semanticModel = this._semanticModelProvider.GetSemanticModel( originalNode.SyntaxTree );
            var symbol = semanticModel.GetDeclaredSymbol( originalNode );

            if ( symbol is { SetMethod: not null } )
            {
                var declaration = (IProperty) this._compilation.GetDeclaration( symbol );
                var entryStatements = this._transformationCollection.GetInjectedEntryStatements( declaration.SetMethod.AssertNotNull() );

                node = (PropertyDeclarationSyntax) InjectStatementsIntoMemberDeclaration(
                    declaration.SetMethod,
                    entryStatements,
                    Array.Empty<StatementSyntax>(),
                    node );
            }

            if ( this._transformationCollection.IsAutoPropertyWithSynthesizedSetter( originalNode ) )
            {
                node = node.WithSynthesizedSetter( this._defaultSyntaxGenerationContext );
            }

            if ( this._transformationCollection.GetAdditionalDeclarationFlags( originalNode ) is not AspectLinkerDeclarationFlags.None and var flags )
            {
                var existingFlags = node.GetLinkerDeclarationFlags();
                node = node.WithLinkerDeclarationFlags( existingFlags | flags );
            }

            // Rewrite attributes.
            var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );
            node = this.ReplaceAttributes( node, rewrittenAttributes );

            return node;
        }

        private IndexerDeclarationSyntax VisitIndexerDeclarationCore( IndexerDeclarationSyntax node )
        {
            var originalNode = node;

            node = (IndexerDeclarationSyntax) this.VisitIndexerDeclaration( node )!;

            var semanticModel = this._semanticModelProvider.GetSemanticModel( originalNode.SyntaxTree );
            var symbol = semanticModel.GetDeclaredSymbol( originalNode );

            if ( symbol != null )
            {
                var declaration = (IPropertyOrIndexer) this._compilation.GetDeclaration( symbol );

                if ( symbol.GetMethod != null )
                {
                    var entryStatements = this._transformationCollection.GetInjectedEntryStatements( declaration.GetMethod.AssertNotNull() );

                    node = (IndexerDeclarationSyntax) InjectStatementsIntoMemberDeclaration(
                        declaration.GetMethod,
                        entryStatements,
                        Array.Empty<StatementSyntax>(),
                        node );
                }

                if ( symbol.SetMethod != null )
                {
                    var entryStatements = this._transformationCollection.GetInjectedEntryStatements( declaration.SetMethod.AssertNotNull() );

                    node = (IndexerDeclarationSyntax) InjectStatementsIntoMemberDeclaration(
                        declaration.SetMethod,
                        entryStatements,
                        Array.Empty<StatementSyntax>(),
                        node );
                }
            }

            // Rewrite attributes.
            var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );
            node = this.ReplaceAttributes( node, rewrittenAttributes );

            return node;
        }

        public override SyntaxNode VisitAccessorDeclaration( AccessorDeclarationSyntax node )
        {
            var originalNode = node;
            node = (AccessorDeclarationSyntax) base.VisitAccessorDeclaration( node )!;

            // Rewrite attributes.
            var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );

            if ( rewrittenAttributes is var (attributes, trivia) )
            {
                if ( trivia.ShouldBePreserved( this.PreserveTrivia ) )
                {
#pragma warning disable LAMA0832 // Avoid WithLeadingTrivia and WithTrailingTrivia calls.
                    node = node.WithAttributeLists( default ).WithLeadingTrivia( trivia ).WithAttributeLists( attributes );
#pragma warning restore LAMA0832
                }
                else
                {
                    node = node.WithAttributeLists( attributes );
                }
            }

            return node;
        }

        private EventDeclarationSyntax VisitEventDeclarationCore( EventDeclarationSyntax node )
        {
            var originalNode = node;
            node = (EventDeclarationSyntax) this.VisitEventDeclaration( node )!;

            // Rewrite attributes.
            var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );
            node = this.ReplaceAttributes( node, rewrittenAttributes );

            return node;
        }

        private IReadOnlyList<MemberDeclarationSyntax> VisitEventFieldDeclarationCore( EventFieldDeclarationSyntax node )
        {
            var originalNode = node;

            // Rewrite attributes.
            if ( originalNode.Declaration.Variables.Count > 1
                 && originalNode.Declaration.Variables.Any( this._transformationCollection.IsNodeWithModifiedAttributes ) )
            {
                var members = new List<MemberDeclarationSyntax>( originalNode.Declaration.Variables.Count );

                // If we have changes in attributes and several members, we have to split them.
                foreach ( var variable in originalNode.Declaration.Variables )
                {
                    var declaration = VariableDeclaration( node.Declaration.Type, SingletonSeparatedList( variable ) );

                    var attributes = this.RewriteDeclarationAttributeLists( variable, originalNode.AttributeLists, node );

                    var eventDeclaration = EventFieldDeclaration(
                        default,
                        node.Modifiers,
                        Token( TriviaList(), SyntaxKind.EventKeyword, TriviaList( Space ) ),
                        declaration,
                        Token( default, SyntaxKind.SemicolonToken, new SyntaxTriviaList( ElasticLineFeed ) ) );

                    eventDeclaration = this.ReplaceAttributes( eventDeclaration, attributes );

                    members.Add( eventDeclaration );
                }

                return members;
            }
            else
            {
                var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode.Declaration.Variables[0], originalNode.AttributeLists, node );
                node = this.ReplaceAttributes( node, rewrittenAttributes );

                return new[] { node };
            }
        }

        public override SyntaxNode VisitCompilationUnit( CompilationUnitSyntax node )
        {
            SyntaxGenerationContext? syntaxGenerationContext = null;
            List<AttributeListSyntax> outputLists = new();
            List<SyntaxTrivia> outputTrivias = new();

            this.RewriteAttributeLists(
                this._compilation.ToTypedRef<IDeclaration>(),
                SyntaxKind.AssemblyKeyword,
                node,
                node.AttributeLists,
                ( _, n ) => n.SyntaxTree == this._syntaxTreeForGlobalAttributes,
                outputLists,
                outputTrivias,
                ref syntaxGenerationContext );

            return ((CompilationUnitSyntax) base.VisitCompilationUnit( node )!).WithAttributeLists( List( outputLists ) );
        }

        public override SyntaxNode? VisitPragmaWarningDirectiveTrivia( PragmaWarningDirectiveTriviaSyntax node )
        {
            // Don't disable or restore warnings that have been suppressed in a parent scope.

            var remainingErrorCodes = node
                .ErrorCodes
                .Where( c => !this._activeSuppressions.Contains( GetErrorCode( c ) ) )
                .ToImmutableArray();

            if ( remainingErrorCodes.IsEmpty )
            {
                return null;
            }
            else
            {
                return node.WithErrorCodes( SeparatedList( remainingErrorCodes ) );
            }

            static string GetErrorCode( ExpressionSyntax expression )
            {
                return expression switch
                {
                    IdentifierNameSyntax identifier => identifier.Identifier.Text,
                    LiteralExpressionSyntax literal => $"CS{literal.Token.Value:0000}",
                    _ => throw new AssertionFailedException( $"Unexpected expression '{expression.Kind()}' at '{expression.GetLocation()}'." )
                };
            }
        }

        private SuppressionContext WithSuppressions( SyntaxNode node ) => new( this, this.GetSuppressions( node ) );

        private SuppressionContext WithSuppressions( IDeclaration declaration ) => new( this, this.GetSuppressions( declaration ) );
    }
}