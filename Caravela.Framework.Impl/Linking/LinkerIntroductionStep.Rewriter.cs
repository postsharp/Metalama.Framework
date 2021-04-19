// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Linking
{
    internal partial class LinkerIntroductionStep
    {
        private partial class Rewriter : CSharpSyntaxRewriter
        {
            private readonly CompilationModel _compilation;
            private readonly ImmutableMultiValueDictionary<ICodeElement, ScopedSuppression> _diagnosticSuppressions;
            private readonly IntroducedMemberCollection _introducedMemberCollection;

            // Maps a diagnostic id to the number of times it has been suppressed.
            private ImmutableHashSet<string> _activeSuppressions = ImmutableHashSet.Create<string>( StringComparer.OrdinalIgnoreCase );

            public Rewriter(
                IntroducedMemberCollection introducedMemberCollection,
                ImmutableMultiValueDictionary<ICodeElement, ScopedSuppression> diagnosticSuppressions,
                CompilationModel compilation )
            {
                this._diagnosticSuppressions = diagnosticSuppressions;
                this._compilation = compilation;
                this._introducedMemberCollection = introducedMemberCollection;
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
                        var codeElement = this._compilation.Factory.GetCodeElement( declaredSymbol );

                        return this.GetSuppressions( codeElement );
                    }
                    else
                    {
                        return ImmutableArray<string>.Empty;
                    }
                }
            }

            private IEnumerable<string> GetSuppressions( ICodeElement codeElement ) => this._diagnosticSuppressions[codeElement].Select( s => s.Id );

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

                if ( !this._activeSuppressions.IsEmpty )
                {
                    // Since we're adding suppressions, we need to visit each `#pragma warning` of the added node to update them.
                    transformedNode = (T) this.Visit( transformedNode )!;
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

                using ( var classSuppressions = this.WithSuppressions( node ) )
                {
                    foreach ( var member in node.Members )
                    {
                        using ( var memberSuppressions = this.WithSuppressions( member ) )
                        {
                            var memberWithSuppressions = this.AddSuppression( member, memberSuppressions.NewSuppressions );
                            members.Add( memberWithSuppressions );
                        }

                        // We have to call AddIntroductionsOnPosition outside of the previous suppression scope, otherwise we don't get new suppressions.
                        AddIntroductionsOnPosition( member );
                    }

                    AddIntroductionsOnPosition( node );

                    return this.AddSuppression( node, classSuppressions.NewSuppressions ).WithMembers( List( members ) );
                }

                void AddIntroductionsOnPosition( MemberDeclarationSyntax position )
                {
                    foreach ( var introducedMember in this._introducedMemberCollection.GetIntroducedMembersOnPosition( position ) )
                    {
                        // Allow for tracking of the node inserted.
                        // IMPORTANT: This need to be here and cannot be in introducedMember.Syntax, result of TrackNodes is not trackable!
                        var introducedNode = introducedMember.Syntax.TrackNodes( introducedMember.Syntax );

                        introducedNode = introducedNode.NormalizeWhitespace();

                        if ( introducedMember.CodeElement != null )
                        {
                            using ( var suppressions = this.WithSuppressions( introducedMember.CodeElement ) )
                            {
                                introducedNode = this.AddSuppression( introducedNode, suppressions.NewSuppressions );
                            }
                        }

                        members.Add( introducedNode );
                    }
                }
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
                    => expression switch
                    {
                        IdentifierNameSyntax identifier => identifier.Identifier.Text,
                        LiteralExpressionSyntax literal => string.Format( "CS{0:0000}", literal.Token.Value ),
                        _ => throw new AssertionFailedException()
                    };
            }

            // The following methods remove the #if code and replaces with its content, but it's not sure that this is the right
            // approach in the scenario where we have to produce code that can become source code ("divorce" feature).
            // When this scenario is supported, more tests will need to be added to specifically support #if.

            public override SyntaxNode? VisitIfDirectiveTrivia( IfDirectiveTriviaSyntax node ) => null;

            public override SyntaxNode? VisitEndIfDirectiveTrivia( EndIfDirectiveTriviaSyntax node ) => null;

            public override SyntaxNode? VisitElseDirectiveTrivia( ElseDirectiveTriviaSyntax node ) => null;

            private SuppressionContext WithSuppressions( SyntaxNode node ) => new( this, this.GetSuppressions( node ) );

            private SuppressionContext WithSuppressions( ICodeElement codeElement ) => new( this, this.GetSuppressions( codeElement ) );
        }
    }
}