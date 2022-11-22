﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking.Substitution
{
    /// <summary>
    /// Substitution of the constructor body that adds initial forced initialization statements to avoid warnings/errors.
    /// </summary>
    internal class ForcedInitializationSubstitution : SyntaxNodeSubstitution
    {
        private readonly SyntaxNode _rootNode;
        private readonly ISymbol[] _symbolsToInitialize;

        public ForcedInitializationSubstitution( SyntaxNode rootNode, ISymbol[] symbolsToInitialize )
        {
            this._rootNode = rootNode;
            this._symbolsToInitialize = symbolsToInitialize;
        }

        public override SyntaxNode TargetNode => this._rootNode;

        public override SyntaxNode? Substitute( SyntaxNode currentNode, SubstitutionContext substitutionContext )
        {
            // We currently need to support all name syntaxes that may reference a property of the current object.

            switch ( currentNode )
            {
                case BlockSyntax block:
                    return
                        block.WithStatements( block.Statements.InsertRange( 0, GetInitializationStatements() ) );

                case ArrowExpressionClauseSyntax arrowExpressionClause:
                    return
                        Block( List( GetInitializationStatements().Append( ExpressionStatement( arrowExpressionClause.Expression ) ) ) );

                default:
                    throw new AssertionFailedException( $"{currentNode.Kind()} is not supported." );
            }

            IEnumerable<StatementSyntax> GetInitializationStatements()
            {
                foreach ( var symbol in this._symbolsToInitialize )
                {
                    yield return
                        ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    ThisExpression(),
                                    IdentifierName( LinkerRewritingDriver.GetBackingFieldName( symbol ) ) ),
                                LiteralExpression(
                                    SyntaxKind.DefaultLiteralExpression,
                                    Token( SyntaxKind.DefaultKeyword ) ) ) );
                }
            }
        }
    }
}