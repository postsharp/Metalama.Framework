﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking.Substitution
{
    /// <summary>
    /// Substitutes an aspect reference that points to the source semantic of auto property for an access to the generated backing field.
    /// </summary>
    internal sealed class AspectReferenceBackingFieldSubstitution : SyntaxNodeSubstitution
    {
        private readonly ResolvedAspectReference _aspectReference;

        public override SyntaxNode TargetNode => this._aspectReference.RootNode;

        public AspectReferenceBackingFieldSubstitution( CompilationContext compilationContext, ResolvedAspectReference aspectReference ) : base(
            compilationContext )
        {
            this._aspectReference = aspectReference;
        }

        public override SyntaxNode Substitute( SyntaxNode currentNode, SubstitutionContext substitutionContext )
        {
            if ( this._aspectReference.RootNode != this._aspectReference.SymbolSourceNode )
            {
                // Root node is different that symbol source node - this is introduction in form:
                // <helper_type>.<helper_member>(<symbol_source_node>);
                // We need to get to symbol source node.

                currentNode = this._aspectReference.RootNode switch
                {
                    InvocationExpressionSyntax { ArgumentList: { Arguments.Count: 1 } argumentList } =>
                        argumentList.Arguments[0].Expression,
                    _ => throw new AssertionFailedException( $"{this._aspectReference.RootNode.Kind()} is not in a supported form." )
                };
            }

            switch ( currentNode )
            {
                case MemberAccessExpressionSyntax { Name: { } } memberAccessExpression:
                    var backingFieldName = LinkerRewritingDriver.GetBackingFieldName( this._aspectReference.ResolvedSemantic.Symbol );

                    if ( this._aspectReference.OriginalSymbol.IsInterfaceMemberImplementation() )
                    {
                        return memberAccessExpression.PartialUpdate( expression: ThisExpression(), name: IdentifierName( backingFieldName ) );
                    }
                    else
                    {
                        return memberAccessExpression.WithName( IdentifierName( backingFieldName ) );
                    }

                default:
                    throw new AssertionFailedException( $"Unexpected syntax kind: {currentNode.Kind()}" );
            }
        }
    }
}