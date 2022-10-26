﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Formatting;
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

namespace Metalama.Framework.Engine.Linking;

internal partial class LinkerIntroductionStep
{
    private partial class Rewriter : SafeSyntaxRewriter
    {
        private readonly CompilationModel _compilation;
        private readonly SemanticModelProvider _semanticModelProvider;
        private readonly SyntaxGenerationContextFactory _syntaxGenerationContextFactory;
        private readonly ImmutableDictionaryOfArray<IDeclaration, ScopedSuppression> _diagnosticSuppressions;
        private readonly SyntaxTransformationCollection _syntaxTransformationCollection;
        private readonly IReadOnlyDictionary<SyntaxNode, MemberLevelTransformations> _symbolMemberLevelTransformations;
        private readonly IReadOnlyDictionary<IIntroduceMemberTransformation, MemberLevelTransformations> _introductionMemberLevelTransformations;
        private readonly IReadOnlyCollectionWithContains<SyntaxNode> _nodesWithModifiedAttributes;
        private readonly SyntaxTree _syntaxTreeForGlobalAttributes;
        private readonly IReadOnlyDictionary<TypeDeclarationSyntax, TypeLevelTransformations> _typeLevelTransformations;

        // Maps a diagnostic id to the number of times it has been suppressed.
        private ImmutableHashSet<string> _activeSuppressions = ImmutableHashSet.Create<string>( StringComparer.OrdinalIgnoreCase );

        public Rewriter(
            IServiceProvider serviceProvider,
            SyntaxTransformationCollection introducedMemberCollection,
            ImmutableDictionaryOfArray<IDeclaration, ScopedSuppression> diagnosticSuppressions,
            CompilationModel compilation,
            IReadOnlyDictionary<SyntaxNode, MemberLevelTransformations> symbolMemberLevelTransformations,
            IReadOnlyDictionary<IIntroduceMemberTransformation, MemberLevelTransformations> introductionMemberLevelTransformations,
            IReadOnlyCollectionWithContains<SyntaxNode> nodesWithModifiedAttributes,
            SyntaxTree syntaxTreeForGlobalAttributes,
            IReadOnlyDictionary<TypeDeclarationSyntax, TypeLevelTransformations> typeLevelTransformations )
        {
            this._syntaxGenerationContextFactory = new SyntaxGenerationContextFactory( compilation.RoslynCompilation, serviceProvider );
            this._diagnosticSuppressions = diagnosticSuppressions;
            this._compilation = compilation;
            this._syntaxTransformationCollection = introducedMemberCollection;
            this._semanticModelProvider = compilation.RoslynCompilation.GetSemanticModelProvider();
            this._symbolMemberLevelTransformations = symbolMemberLevelTransformations;
            this._introductionMemberLevelTransformations = introductionMemberLevelTransformations;
            this._nodesWithModifiedAttributes = nodesWithModifiedAttributes;
            this._syntaxTreeForGlobalAttributes = syntaxTreeForGlobalAttributes;
            this._typeLevelTransformations = typeLevelTransformations;
        }

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
                FieldDeclarationSyntax field when field.Declaration.Variables.Count == 1
                    => FindSuppressionsCore( field.Declaration.Variables.First() ),

                // If we have a field declaration that declares many field, we merge all suppressions
                // and suppress all for all fields. This is significantly simpler than splitting the declaration.
                FieldDeclarationSyntax field when field.Declaration.Variables.Count > 1
                    => field.Declaration.Variables.Select( FindSuppressionsCore ).SelectMany( l => l ),

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
        /// <param name="node"></param>
        /// <param name="suppressionsOnThisElement"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
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

                var disable = Trivia(
                        PragmaWarningDirectiveTrivia( Token( SyntaxKind.DisableKeyword ), true )
                            .WithErrorCodes( errorCodes )
                            .NormalizeWhitespace()
                            .WithLeadingTrivia( ElasticLineFeed )
                            .WithTrailingTrivia( ElasticLineFeed ) )
                    .WithLinkerGeneratedFlags( LinkerGeneratedFlags.GeneratedSuppression );

                var restore =
                    Trivia(
                            PragmaWarningDirectiveTrivia( Token( SyntaxKind.RestoreKeyword ), true )
                                .WithErrorCodes( errorCodes )
                                .NormalizeWhitespace()
                                .WithLeadingTrivia( ElasticLineFeed )
                                .WithTrailingTrivia( ElasticLineFeed ) )
                        .WithLinkerGeneratedFlags( LinkerGeneratedFlags.GeneratedSuppression );

                transformedNode =
                    transformedNode
                        .WithLeadingTrivia( node.GetLeadingTrivia().InsertRange( 0, new[] { ElasticLineFeed, disable, ElasticLineFeed } ) )
                        .WithTrailingTrivia( transformedNode.GetTrailingTrivia().AddRange( new[] { ElasticLineFeed, restore, ElasticLineFeed } ) );
            }

            return transformedNode;
        }

        private (SyntaxList<AttributeListSyntax> Attributes, SyntaxTriviaList Trivia) RewriteDeclarationAttributeLists(
            SyntaxNode originalDeclaringNode,
            SyntaxList<AttributeListSyntax> attributeLists )
        {
            if ( !this._nodesWithModifiedAttributes.Contains( originalDeclaringNode ) )
            {
                return (attributeLists, default);
            }

            // Resolve the symbol.
            var semanticModel = this._semanticModelProvider.GetSemanticModel( originalDeclaringNode.SyntaxTree );
            var symbol = semanticModel.GetDeclaredSymbol( originalDeclaringNode );

            if ( symbol == null )
            {
                return (attributeLists, default);
            }

            var outputLists = new List<AttributeListSyntax>();
            var outputTrivias = new List<SyntaxTrivia>();
            SyntaxGenerationContext? syntaxGenerationContext = null;

            this.RewriteAttributeLists(
                Ref.FromSymbol( symbol, this._compilation.RoslynCompilation ),
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
                        Ref.ReturnParameter( method, this._compilation.RoslynCompilation ),
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
                return (default, TriviaList( outputTrivias ));
            }
            else
            {
                return (List( outputLists ), TriviaList( outputTrivias ));
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

            // Remove attributes from the list.
            foreach ( var list in inputAttributeLists )
            {
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
                        switch ( trivia.Kind() )
                        {
                            case SyntaxKind.MultiLineCommentTrivia or SyntaxKind.MultiLineDocumentationCommentTrivia:
                                outputTrivia.Add( trivia );

                                break;

                            case SyntaxKind.SingleLineCommentTrivia or SyntaxKind.SingleLineDocumentationCommentTrivia:
                                outputTrivia.Add( trivia );
                                outputTrivia.Add( ElasticLineFeed );

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
                    syntaxGenerationContext ??= this._syntaxGenerationContextFactory.GetSyntaxGenerationContext( originalDeclaringNode );

                    var newAttribute = syntaxGenerationContext.SyntaxGenerator.Attribute( attributeBuilder )
                        .AssertNotNull();

                    var newList = AttributeList( SingletonSeparatedList( newAttribute ) )
                        .WithTrailingTrivia( ElasticLineFeed )
                        .WithAdditionalAnnotations( attributeBuilder.ParentAdvice.Aspect.AspectClass.GeneratedCodeAnnotation );

                    if ( targetKind != SyntaxKind.None )
                    {
                        newList = newList.WithTarget( AttributeTargetSpecifier( Token( targetKind ) ) );
                    }

                    outputAttributeLists.Add( newList );
                }
            }
        }

        public override SyntaxNode? VisitClassDeclaration( ClassDeclarationSyntax node ) => this.VisitTypeDeclaration( node );

        public override SyntaxNode? VisitStructDeclaration( StructDeclarationSyntax node ) => this.VisitTypeDeclaration( node );

        public override SyntaxNode? VisitInterfaceDeclaration( InterfaceDeclarationSyntax node ) => this.VisitTypeDeclaration( node );

        public override SyntaxNode? VisitRecordDeclaration( RecordDeclarationSyntax node )
            => this.VisitTypeDeclaration(
                node,
                ( syntax, members ) =>
                {
                    // If the record has no braces, add them.
                    if ( syntax.OpenBraceToken.IsKind( SyntaxKind.None ) && members.Count > 0 )
                    {
                        // TODO: trivias.
                        syntax = syntax
                            .WithOpenBraceToken( Token( SyntaxKind.OpenBraceToken ).AddColoringAnnotation( TextSpanClassification.GeneratedCode ) )
                            .WithCloseBraceToken( Token( SyntaxKind.CloseBraceToken ).AddColoringAnnotation( TextSpanClassification.GeneratedCode ) )
                            .WithSemicolonToken( default );
                    }

                    return syntax.WithMembers( List( members ) );
                } );

        private SyntaxNode? VisitTypeDeclaration<T>( T node, Func<T, List<MemberDeclarationSyntax>, T>? withMembers = null )
            where T : TypeDeclarationSyntax
        {
            var originalNode = node;
            var members = new List<MemberDeclarationSyntax>( node.Members.Count );
            var additionalBaseList = this._syntaxTransformationCollection.GetIntroducedInterfacesForTypeDeclaration( node );
            var syntaxGenerationContext = this._syntaxGenerationContextFactory.GetSyntaxGenerationContext( node );

            if ( this._typeLevelTransformations.TryGetValue( node, out var typeLevelTransformations ) )
            {
                if ( typeLevelTransformations.AddExplicitDefaultConstructor )
                {
                    var constructorBody = SyntaxFactoryEx.FormattedBlock();

                    var constructor = ConstructorDeclaration( node.Identifier )
                        .WithModifiers( TokenList( Token( SyntaxKind.PublicKeyword ) ) )
                        .WithBody( constructorBody )
                        .NormalizeWhitespace()
                        .AddColoringAnnotation( TextSpanClassification.GeneratedCode );

                    members.Add( constructor );
                }
            }

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
                    AddIntroductionsOnPosition( new InsertPosition( InsertPositionRelation.After, member ) );
                }

                AddIntroductionsOnPosition( new InsertPosition( InsertPositionRelation.Within, node ) );

                node = this.AddSuppression( node, suppressionContext.NewSuppressions );

                if ( withMembers != null )
                {
                    node = withMembers( node, members );
                }
                else
                {
                    node = (T) node.WithMembers( List( members ) );
                }

                // Process the type bases.
                if ( additionalBaseList.Any() )
                {
                    if ( node.BaseList == null )
                    {
                        node = (T) node
                            .WithIdentifier( node.Identifier.WithTrailingTrivia() )
                            .WithBaseList(
                                BaseList( SeparatedList( additionalBaseList.Select( i => i.Syntax ) ) )
                                    .WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation ) )
                            .WithTrailingTrivia( node.Identifier.TrailingTrivia );
                    }
                    else
                    {
                        node = (T) node.WithBaseList(
                            BaseList(
                                node.BaseList.Types.AddRange(
                                    additionalBaseList.Select(
                                        i => i.Syntax.WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation ) ) ) ) );
                    }
                }

                // Rewrite attributes.
                var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );
                node = (T) node.WithAttributeLists( rewrittenAttributes.Attributes ).WithAdditionalLeadingTrivia( rewrittenAttributes.Trivia );

                return node;
            }

            // TODO: Try to avoid closure allocation.
            void AddIntroductionsOnPosition( InsertPosition position )
            {
                var membersAtPosition = this._syntaxTransformationCollection.GetIntroducedMembersOnPosition( position );

                foreach ( var introducedMember in membersAtPosition )
                {
                    // Allow for tracking of the node inserted.
                    // IMPORTANT: This need to be here and cannot be in introducedMember.Syntax, result of TrackNodes is not trackable!
                    var introducedNode = introducedMember.Syntax.TrackNodes( introducedMember.Syntax );

                    introducedNode = introducedNode
                        .WithLeadingTrivia( ElasticLineFeed, ElasticLineFeed )
                        .WithGeneratedCodeAnnotation( introducedMember.Introduction.ParentAdvice.Aspect.AspectClass.GeneratedCodeAnnotation );

                    // Insert inserted statements into 
                    switch ( introducedNode )
                    {
                        case ConstructorDeclarationSyntax constructorDeclaration:
                            if ( this._introductionMemberLevelTransformations.TryGetValue(
                                    introducedMember.Introduction,
                                    out var memberLevelTransformations ) )
                            {
                                introducedNode = this.ApplyMemberLevelTransformations(
                                    constructorDeclaration,
                                    memberLevelTransformations,
                                    syntaxGenerationContext );
                            }

                            break;
                    }

                    if ( introducedMember.Declaration != null )
                    {
                        using ( var suppressions = this.WithSuppressions( introducedMember.Declaration ) )
                        {
                            introducedNode = this.AddSuppression( introducedNode, suppressions.NewSuppressions );
                        }
                    }

                    members.Add( introducedNode );
                }
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
            constructorDeclaration = this.InsertStatements( constructorDeclaration, memberLevelTransformations.Statements );

            constructorDeclaration = constructorDeclaration.WithParameterList(
                AppendParameters( constructorDeclaration.ParameterList, memberLevelTransformations.Parameters, syntaxGenerationContext ) );

            constructorDeclaration = constructorDeclaration.WithInitializer(
                AppendInitializerArguments( constructorDeclaration.Initializer, memberLevelTransformations.Arguments ) );

            return constructorDeclaration;
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
                return existingParameters.WithParameters(
                    existingParameters.Parameters.AddRange(
                        newParameters.Select( x => x.ToSyntax( syntaxGenerationContext ).WithTrailingTrivia( ElasticSpace ) ) ) );
            }
        }

        private static ConstructorInitializerSyntax? AppendInitializerArguments(
            ConstructorInitializerSyntax? initializerSyntax,
            ImmutableArray<IntroduceConstructorInitializerArgumentTransformation> newArguments )
        {
            if ( newArguments.IsEmpty )
            {
                return initializerSyntax;
            }

            var newArgumentsSyntax = newArguments.Select( a => a.ToSyntax().WithTrailingTrivia( ElasticSpace ) );

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

        private ConstructorDeclarationSyntax InsertStatements(
            ConstructorDeclarationSyntax constructorDeclaration,
            ImmutableArray<LinkerInsertedStatement> insertedStatements )
        {
            if ( insertedStatements.IsEmpty )
            {
                return constructorDeclaration;
            }

            // TODO: The order here is correct for initialization, i.e. first aspects (transformation order) are initialized first.
            //       This would not be, however, correct for other uses, but we don't have those.

            var beginningStatements = Order( insertedStatements )
                .Select( s => s.Statement );

            switch ( constructorDeclaration )
            {
                case { ExpressionBody: { } expressionBody }:
                    return
                        constructorDeclaration
                            .WithExpressionBody( null )
                            .WithSemicolonToken( default )
                            .WithBody(
                                SyntaxFactoryEx.FormattedBlock(
                                    beginningStatements
                                        .Append( ExpressionStatement( expressionBody.Expression.WithSourceCodeAnnotationIfNotGenerated() ) ) ) );

                case { Body: { } body }:
                    return
                        constructorDeclaration
                            .WithBody(
                                SyntaxFactoryEx.FormattedBlock(
                                    beginningStatements
                                        .Append(
                                            body.WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock )
                                                .WithSourceCodeAnnotationIfNotGenerated() ) ) );
            }

            return constructorDeclaration;

            IEnumerable<LinkerInsertedStatement> Order( IEnumerable<LinkerInsertedStatement> statements )
            {
                // TODO: This sort is intended only for beginning statements.
                var memberStatements = new Dictionary<IMember, List<LinkerInsertedStatement>>( this._compilation.Comparer );
                var typeStatements = new List<LinkerInsertedStatement>();

                foreach ( var mark in statements )
                {
                    switch ( mark.ContextDeclaration )
                    {
                        case INamedType:
                            typeStatements.Add( mark );

                            break;

                        case IMember member:
                            if ( !memberStatements.TryGetValue( member, out var list ) )
                            {
                                memberStatements[member] = list = new List<LinkerInsertedStatement>();
                            }

                            list.Add( mark );

                            break;

                        default:
                            throw new AssertionFailedException();
                    }
                }

                // TODO: This sorting is suboptimal, but needed for stable order since we are using a dictionary.
                foreach ( var pair in memberStatements.OrderBy( p => p.Key.ToDisplayString() ) )
                {
                    foreach ( var mark in pair.Value )
                    {
                        yield return mark;
                    }
                }

                foreach ( var mark in typeStatements )
                {
                    yield return mark;
                }
            }
        }

        private IReadOnlyList<MemberDeclarationSyntax> VisitFieldDeclarationCore( FieldDeclarationSyntax node )
        {
            var originalNode = node;

            // Rewrite attributes.
            if ( originalNode.Declaration.Variables.Count > 1
                 && originalNode.Declaration.Variables.Any( v => this._nodesWithModifiedAttributes.Contains( v ) ) )
            {
                // TODO: This needs to use rewritten variable declaration or do removal in place.
                var members = new List<MemberDeclarationSyntax>( originalNode.Declaration.Variables.Count );

                // If we have changes in attributes and several members, we have to split them.
                foreach ( var variable in originalNode.Declaration.Variables )
                {
                    if ( this._syntaxTransformationCollection.IsRemovedSyntax( variable ) )
                    {
                        continue;
                    }

                    var finalVariable = variable;

                    if ( this._symbolMemberLevelTransformations.TryGetValue( variable, out var transformations )
                         && transformations.AddDefaultInitializer )
                    {
                        finalVariable =
                            finalVariable.WithInitializer(
                                EqualsValueClause(
                                    LiteralExpression(
                                        SyntaxKind.DefaultLiteralExpression,
                                        Token( SyntaxKind.DefaultKeyword ) ) ) );
                    }

                    var declaration = VariableDeclaration( node.Declaration.Type, SingletonSeparatedList( finalVariable ) );
                    var attributes = this.RewriteDeclarationAttributeLists( variable, originalNode.AttributeLists );

                    var fieldDeclaration = FieldDeclaration( attributes.Attributes, node.Modifiers, declaration, Token( SyntaxKind.SemicolonToken ) )
                        .WithTrailingTrivia( ElasticLineFeed )
                        .WithLeadingTrivia( attributes.Trivia );

                    members.Add( fieldDeclaration );
                }

                return members;
            }
            else
            {
                var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode.Declaration.Variables[0], originalNode.AttributeLists );
                node = node.WithAttributeLists( rewrittenAttributes.Attributes ).WithAdditionalLeadingTrivia( rewrittenAttributes.Trivia );

                var anyChangeToVariables = false;
                var rewrittenVariables = new List<VariableDeclaratorSyntax>();

                foreach ( var variable in originalNode.Declaration.Variables )
                {
                    if ( this._syntaxTransformationCollection.IsRemovedSyntax( variable ) )
                    {
                        anyChangeToVariables = true;

                        continue;
                    }

                    if ( this._symbolMemberLevelTransformations.TryGetValue( variable, out var transformations ) && transformations.AddDefaultInitializer )
                    {
                        anyChangeToVariables = true;

                        rewrittenVariables.Add(
                            variable.WithInitializer(
                                EqualsValueClause(
                                    LiteralExpression(
                                        SyntaxKind.DefaultLiteralExpression,
                                        Token( SyntaxKind.DefaultKeyword ) ) ) ) );
                    }
                    else
                    {
                        rewrittenVariables.Add( variable );
                    }
                }

                if ( anyChangeToVariables )
                {
                    if ( rewrittenVariables.Count > 0 )
                    {
                        return new[] { node.WithDeclaration( node.Declaration.WithVariables( SeparatedList( rewrittenVariables ) ) ) };
                    }
                    else
                    {
                        return Array.Empty<MemberDeclarationSyntax>();
                    }
                }
                else
                {
                    return new[] { node };
                }
            }
        }

        private ConstructorDeclarationSyntax VisitConstructorDeclarationCore( ConstructorDeclarationSyntax node )
        {
            var originalNode = node;

            if ( this._symbolMemberLevelTransformations.TryGetValue( node, out var memberLevelTransformations ) )
            {
                var syntaxGenerationContext = this._syntaxGenerationContextFactory.GetSyntaxGenerationContext( node );
                node = this.ApplyMemberLevelTransformations( node, memberLevelTransformations, syntaxGenerationContext );
            }

            // Rewrite attributes.
            var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );
            node = node.WithAttributeLists( rewrittenAttributes.Attributes ).WithAdditionalLeadingTrivia( rewrittenAttributes.Trivia );

            return (ConstructorDeclarationSyntax) this.VisitConstructorDeclaration( node )!;
        }

        private MethodDeclarationSyntax VisitMethodDeclarationCore( MethodDeclarationSyntax node )
        {
            var originalNode = node;
            node = (MethodDeclarationSyntax) this.VisitMethodDeclaration( node )!;

            // Rewrite attributes.
            var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );
            node = node.WithAttributeLists( rewrittenAttributes.Attributes ).WithAdditionalLeadingTrivia( rewrittenAttributes.Trivia );

            return node;
        }

        private OperatorDeclarationSyntax VisitOperatorDeclarationCore( OperatorDeclarationSyntax node )
        {
            var originalNode = node;
            node = (OperatorDeclarationSyntax) this.VisitOperatorDeclaration( node )!;

            // Rewrite attributes.
            var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );
            node = node.WithAttributeLists( rewrittenAttributes.Attributes ).WithAdditionalLeadingTrivia( rewrittenAttributes.Trivia );

            return node;
        }

        public override SyntaxNode? VisitParameter( ParameterSyntax node )
        {
            var originalNode = node;
            node = (ParameterSyntax) base.VisitParameter( node )!;

            // Rewrite attributes.
            var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );
            node = node.WithAttributeLists( rewrittenAttributes.Attributes ).WithAdditionalLeadingTrivia( rewrittenAttributes.Trivia );

            return node;
        }

        public override SyntaxNode? VisitTypeParameter( TypeParameterSyntax node )
        {
            var originalNode = node;
            node = (TypeParameterSyntax) base.VisitTypeParameter( node )!;

            // Rewrite attributes.
            var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );
            node = node.WithAttributeLists( rewrittenAttributes.Attributes ).WithAdditionalLeadingTrivia( rewrittenAttributes.Trivia );

            return node;
        }

        private PropertyDeclarationSyntax VisitPropertyDeclarationCore( PropertyDeclarationSyntax node )
        {
            var originalNode = node;

            node = (PropertyDeclarationSyntax) this.VisitPropertyDeclaration( node )!;

            if ( this._syntaxTransformationCollection.IsAutoPropertyWithSynthesizedSetter( originalNode ) )
            {
                node = node.WithSynthesizedSetter();
            }

            if ( this._symbolMemberLevelTransformations.TryGetValue( originalNode, out var transformations )
                 && transformations.AddDefaultInitializer )
            {
                node =
                    node.Update(
                        node.AttributeLists,
                        node.Modifiers,
                        node.Type,
                        node.ExplicitInterfaceSpecifier,
                        node.Identifier,
                        node.AccessorList,
                        node.ExpressionBody,
                        EqualsValueClause(
                            LiteralExpression(
                                SyntaxKind.DefaultLiteralExpression,
                                Token( SyntaxKind.DefaultKeyword ) ) ),
                        Token( SyntaxKind.SemicolonToken ) );
            }

            if ( this._syntaxTransformationCollection.GetAdditionalDeclarationFlags( originalNode ) is not AspectLinkerDeclarationFlags.None and var flags )
            {
                var existingFlags = node.GetLinkerDeclarationFlags();
                node = node.WithLinkerDeclarationFlags( existingFlags | flags );
            }

            // Rewrite attributes.
            var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );
            node = node.WithAttributeLists( rewrittenAttributes.Attributes ).WithAdditionalLeadingTrivia( rewrittenAttributes.Trivia );

            return node;
        }

        public override SyntaxNode? VisitAccessorDeclaration( AccessorDeclarationSyntax node )
        {
            var originalNode = node;
            node = (AccessorDeclarationSyntax) base.VisitAccessorDeclaration( node )!;

            // Rewrite attributes.
            var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );
            node = node.WithAttributeLists( rewrittenAttributes.Attributes ).WithAdditionalLeadingTrivia( rewrittenAttributes.Trivia );

            return node;
        }

        private EventDeclarationSyntax VisitEventDeclarationCore( EventDeclarationSyntax node )
        {
            var originalNode = node;
            node = (EventDeclarationSyntax) this.VisitEventDeclaration( node )!;

            // Represents the event field that cannot be otherwise expressed (explicit interface implementation).
            if ( node.GetLinkerDeclarationFlags().HasFlagFast( AspectLinkerDeclarationFlags.EventField ) )
            {
                if ( this._symbolMemberLevelTransformations.TryGetValue( originalNode, out var transformations )
                     && transformations.AddDefaultInitializer )
                {
                    node = node.WithLinkerDeclarationFlags(
                        AspectLinkerDeclarationFlags.EventField | AspectLinkerDeclarationFlags.HasDefaultInitializerExpression );
                }
            }

            // Rewrite attributes.
            var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode, originalNode.AttributeLists );
            node = node.WithAttributeLists( rewrittenAttributes.Attributes ).WithAdditionalLeadingTrivia( rewrittenAttributes.Trivia );

            return node;
        }

        private IReadOnlyList<MemberDeclarationSyntax> VisitEventFieldDeclarationCore( EventFieldDeclarationSyntax node )
        {
            var originalNode = node;

            // Rewrite attributes.
            if ( originalNode.Declaration.Variables.Count > 1
                 && originalNode.Declaration.Variables.Any( v => this._nodesWithModifiedAttributes.Contains( v ) ) )
            {
                var members = new List<MemberDeclarationSyntax>( originalNode.Declaration.Variables.Count );

                // If we have changes in attributes and several members, we have to split them.
                foreach ( var variable in originalNode.Declaration.Variables )
                {
                    var finalVariable = variable;

                    if ( this._symbolMemberLevelTransformations.TryGetValue( variable, out var transformations )
                         && transformations.AddDefaultInitializer )
                    {
                        finalVariable =
                            finalVariable.WithInitializer(
                                EqualsValueClause(
                                    LiteralExpression(
                                        SyntaxKind.DefaultLiteralExpression,
                                        Token( SyntaxKind.DefaultKeyword ) ) ) );
                    }

                    var declaration = VariableDeclaration( node.Declaration.Type, SingletonSeparatedList( finalVariable ) );

                    var attributes = this.RewriteDeclarationAttributeLists( variable, originalNode.AttributeLists );

                    var eventDeclaration = EventFieldDeclaration(
                            attributes.Attributes,
                            node.Modifiers,
                            Token( TriviaList(), SyntaxKind.EventKeyword, TriviaList( Space ) ),
                            declaration,
                            Token( SyntaxKind.SemicolonToken ) )
                        .WithTrailingTrivia( ElasticLineFeed )
                        .WithLeadingTrivia( attributes.Trivia );

                    members.Add( eventDeclaration );
                }

                return members;
            }
            else
            {
                var rewrittenAttributes = this.RewriteDeclarationAttributeLists( originalNode.Declaration.Variables[0], originalNode.AttributeLists );
                node = node.WithAttributeLists( rewrittenAttributes.Attributes ).WithAdditionalLeadingTrivia( rewrittenAttributes.Trivia );

                var anyChange = false;
                var rewrittenVariables = new List<VariableDeclaratorSyntax>();

                foreach ( var variable in originalNode.Declaration.Variables )
                {
                    if ( this._symbolMemberLevelTransformations.TryGetValue( variable, out var transformations ) && transformations.AddDefaultInitializer )
                    {
                        anyChange = true;

                        rewrittenVariables.Add(
                            variable.WithInitializer(
                                EqualsValueClause(
                                    LiteralExpression(
                                        SyntaxKind.DefaultLiteralExpression,
                                        Token( SyntaxKind.DefaultKeyword ) ) ) ) );
                    }
                    else
                    {
                        rewrittenVariables.Add( variable );
                    }
                }

                if ( anyChange )
                {
                    return new[] { node.WithDeclaration( node.Declaration.WithVariables( SeparatedList( rewrittenVariables ) ) ) };
                }
                else
                {
                    return new[] { node };
                }
            }
        }

        public override SyntaxNode? VisitCompilationUnit( CompilationUnitSyntax node )
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
                    _ => throw new AssertionFailedException()
                };
            }
        }

        private SuppressionContext WithSuppressions( SyntaxNode node ) => new( this, this.GetSuppressions( node ) );

        private SuppressionContext WithSuppressions( IDeclaration declaration ) => new( this, this.GetSuppressions( declaration ) );
    }
}