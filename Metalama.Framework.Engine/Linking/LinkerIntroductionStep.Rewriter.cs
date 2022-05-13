// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking
{
    internal partial class LinkerIntroductionStep
    {
        private partial class Rewriter : CSharpSyntaxRewriter
        {
            private readonly CompilationModel _compilation;
            private readonly ImmutableDictionary<AspectLayerId, OrderedAspectLayer> _orderedAspectLayers;
            private readonly ImmutableDictionaryOfArray<IDeclaration, ScopedSuppression> _diagnosticSuppressions;
            private readonly SyntaxTransformationCollection _introducedMemberCollection;
            private readonly IReadOnlyDictionary<SyntaxNode, IReadOnlyList<LinkerInsertedStatement>> _symbolInsertedStatements;
            private readonly IReadOnlyDictionary<IIntroduceMemberTransformation, IReadOnlyList<LinkerInsertedStatement>> _introductionInsertedStatements;

            // Maps a diagnostic id to the number of times it has been suppressed.
            private ImmutableHashSet<string> _activeSuppressions = ImmutableHashSet.Create<string>( StringComparer.OrdinalIgnoreCase );

            public Rewriter(
                SyntaxTransformationCollection introducedMemberCollection,
                ImmutableDictionaryOfArray<IDeclaration, ScopedSuppression> diagnosticSuppressions,
                CompilationModel compilation,
                IReadOnlyList<OrderedAspectLayer> inputOrderedAspectLayers,
                IReadOnlyDictionary<SyntaxNode, IReadOnlyList<LinkerInsertedStatement>> symbolInsertedStatements,
                IReadOnlyDictionary<IIntroduceMemberTransformation, IReadOnlyList<LinkerInsertedStatement>> introductionInsertedStatements )
            {
                this._diagnosticSuppressions = diagnosticSuppressions;
                this._compilation = compilation;
                this._orderedAspectLayers = inputOrderedAspectLayers.ToImmutableDictionary( e => e.AspectLayerId, e => e );
                this._introducedMemberCollection = introducedMemberCollection;
                this._symbolInsertedStatements = symbolInsertedStatements;
                this._introductionInsertedStatements = introductionInsertedStatements;
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
                    var declaredSymbol = this._compilation.RoslynCompilation.GetSemanticModel( node.SyntaxTree ).GetDeclaredSymbol( identifierNode );

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
                            .NormalizeWhitespace() );

                    var restore =
                        Trivia(
                            PragmaWarningDirectiveTrivia( Token( SyntaxKind.RestoreKeyword ), true )
                                .WithErrorCodes( errorCodes )
                                .NormalizeWhitespace() );

                    transformedNode = transformedNode.WithLeadingTrivia( node.GetLeadingTrivia().Insert( 0, disable ) ).WithTrailingTrivia( LineFeed, restore );
                }

                return transformedNode;
            }

            public override SyntaxNode? VisitClassDeclaration( ClassDeclarationSyntax node )
            {
                var members = new List<MemberDeclarationSyntax>( node.Members.Count );
                var additionalBaseList = this._introducedMemberCollection.GetIntroducedInterfacesForTypeDecl( node );

                using ( var classSuppressions = this.WithSuppressions( node ) )
                {
                    foreach ( var member in node.Members )
                    {
                        var visitedMember = (MemberDeclarationSyntax?) this.Visit( member );

                        if ( visitedMember != null )
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

                    node = this.AddSuppression( node, classSuppressions.NewSuppressions ).WithMembers( List( members ) );

                    if ( additionalBaseList.Any() )
                    {
                        if ( node.BaseList == null )
                        {
                            node = node
                                .WithIdentifier( node.Identifier.WithTrailingTrivia() )
                                .WithBaseList( BaseList( SeparatedList( additionalBaseList ) ).WithGeneratedCodeAnnotation() )
                                .WithTrailingTrivia( node.Identifier.TrailingTrivia );
                        }
                        else
                        {
                            node = node.WithBaseList(
                                BaseList( node.BaseList.Types.AddRange( additionalBaseList.Select( i => i.WithGeneratedCodeAnnotation() ) ) ) );
                        }
                    }

                    return node;
                }

                // TODO: Try to avoid closure allocation.
                void AddIntroductionsOnPosition( InsertPosition position )
                {
                    var comparer = new LinkerIntroducedMemberComparer( this._orderedAspectLayers );

                    var membersAtPosition = this._introducedMemberCollection.GetIntroducedMembersOnPosition( position )
                        .ToList();

                    membersAtPosition.Sort( comparer );

                    foreach ( var introducedMember in membersAtPosition )
                    {
                        // Allow for tracking of the node inserted.
                        // IMPORTANT: This need to be here and cannot be in introducedMember.Syntax, result of TrackNodes is not trackable!
                        var introducedNode = introducedMember.Syntax.TrackNodes( introducedMember.Syntax );

                        introducedNode = introducedNode.NormalizeWhitespace()
                            .WithLeadingTrivia( ElasticLineFeed, ElasticLineFeed )
                            .WithGeneratedCodeAnnotation();

                        // Insert inserted statements into 
                        switch ( introducedNode )
                        {
                            case ConstructorDeclarationSyntax constructorDeclaration:
                                if ( this._introductionInsertedStatements.TryGetValue( introducedMember.Introduction, out var insertedStatements ) )
                                {
                                    introducedNode = this.WithInsertedStatements( constructorDeclaration, insertedStatements );
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

            private ConstructorDeclarationSyntax WithInsertedStatements(
                ConstructorDeclarationSyntax constructorDeclaration,
                IReadOnlyList<LinkerInsertedStatement>? insertedStatements )
            {
                if ( insertedStatements == null )
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
                                    Block(
                                            beginningStatements
                                                .Append( ExpressionStatement( expressionBody.Expression.WithSourceCodeAnnotationIfNotGenerated() ) ) )
                                        .WithGeneratedCodeAnnotation() );

                    case { Body: { } body }:
                        return
                            constructorDeclaration
                                .WithBody(
                                    Block(
                                        beginningStatements
                                            .Append(
                                                body.WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock )
                                                    .WithSourceCodeAnnotationIfNotGenerated() ) ) );
                }

                return constructorDeclaration;

                IEnumerable<LinkerInsertedStatement> Order( IEnumerable<LinkerInsertedStatement> statements )
                {
                    // TODO: This sort is intended only for beginning statements.
                    var memberStatements = new Dictionary<IMember, List<LinkerInsertedStatement>>( this._compilation.InvariantComparer );
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

            public override SyntaxNode? VisitVariableDeclarator( VariableDeclaratorSyntax node )
            {
                if ( this._introducedMemberCollection.IsRemovedSyntax( node ) )
                {
                    return null;
                }

                return base.VisitVariableDeclarator( node );
            }

            public override SyntaxNode? VisitVariableDeclaration( VariableDeclarationSyntax node )
            {
                var remainingVariables = new List<VariableDeclaratorSyntax>();

                foreach ( var variable in node.Variables )
                {
                    var rewrittenVariable = (VariableDeclaratorSyntax?) this.Visit( variable );

                    if ( rewrittenVariable != null )
                    {
                        remainingVariables.Add( rewrittenVariable );
                    }
                }

                if ( node.Variables.SequenceEqual( remainingVariables ) )
                {
                    return base.VisitVariableDeclaration( node );
                }
                else if ( remainingVariables.Count > 0 )
                {
                    return node
                        .WithType( (TypeSyntax) this.Visit( node.Type ).AssertNotNull() )
                        .WithVariables( SeparatedList( remainingVariables ) )
                        .WithLeadingTrivia( this.VisitTriviaList( node.GetLeadingTrivia() ) )
                        .WithTrailingTrivia( this.VisitTriviaList( node.GetTrailingTrivia() ) );
                }
                else
                {
                    return null;
                }
            }

            public override SyntaxNode? VisitFieldDeclaration( FieldDeclarationSyntax node )
            {
                var rewrittenDeclaration = (VariableDeclarationSyntax?) this.Visit( node.Declaration );

                if ( rewrittenDeclaration == null )
                {
                    return null;
                }

                return node.WithDeclaration( rewrittenDeclaration );
            }

            public override SyntaxNode? VisitConstructorDeclaration( ConstructorDeclarationSyntax node )
            {
                if ( this._symbolInsertedStatements.TryGetValue( node, out var insertedStatements ) )
                {
                    node = this.WithInsertedStatements( node, insertedStatements );
                }

                return base.VisitConstructorDeclaration( node );
            }

            public override SyntaxNode? VisitEventFieldDeclaration( EventFieldDeclarationSyntax node )
            {
                var rewrittenDeclaration = (VariableDeclarationSyntax?) this.Visit( node.Declaration );

                if ( rewrittenDeclaration == null )
                {
                    // We are not supporting removal of event fields during introduction step.
                    throw new AssertionFailedException();
                }

                return node.WithDeclaration( rewrittenDeclaration );
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

            // The following methods remove the #if code and replaces with its content, but it's not sure that this is the right
            // approach in the scenario where we have to produce code that can become source code ("divorce" feature).
            // When this scenario is supported, more tests will need to be added to specifically support #if.

            public override SyntaxNode? VisitIfDirectiveTrivia( IfDirectiveTriviaSyntax node ) => null;

            public override SyntaxNode? VisitEndIfDirectiveTrivia( EndIfDirectiveTriviaSyntax node ) => null;

            public override SyntaxNode? VisitElseDirectiveTrivia( ElseDirectiveTriviaSyntax node ) => null;

            private SuppressionContext WithSuppressions( SyntaxNode node ) => new( this, this.GetSuppressions( node ) );

            private SuppressionContext WithSuppressions( IDeclaration declaration ) => new( this, this.GetSuppressions( declaration ) );
        }
    }
}