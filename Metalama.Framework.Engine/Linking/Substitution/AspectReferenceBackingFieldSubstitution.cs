// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking.Substitution
{
    /// <summary>
    /// Substitutes an aspect reference that points to the source semantic of auto property for an access to the generated backing field.
    /// </summary>
    internal class AspectReferenceBackingFieldSubstitution : SyntaxNodeSubstitution
    {
        private readonly ResolvedAspectReference _aspectReference;

        public override SyntaxNode TargetNode => this._aspectReference.RootNode;

        public AspectReferenceBackingFieldSubstitution( ResolvedAspectReference aspectReference )
        {
            this._aspectReference = aspectReference;
        }

        public override SyntaxNode? Substitute( SyntaxNode currentNode, SubstitutionContext substitutionContext )
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
                case MemberAccessExpressionSyntax { Name: { } name } memberAccess:
                    return
                        memberAccess.
                        WithName( IdentifierName( LinkerRewritingDriver.GetBackingFieldName( this._aspectReference.ResolvedSemantic.Symbol ) ) );

                default:
                    throw new AssertionFailedException( $"Unexpected syntax kind: {currentNode.Kind()}" );
            }
        }
    }
}