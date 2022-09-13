// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking.Substitution
{
    internal class EmptyVoidPartialMethodSubstitution : SyntaxNodeSubstitution
    {
        private readonly MethodDeclarationSyntax _rootNode;

        public EmptyVoidPartialMethodSubstitution( MethodDeclarationSyntax rootNode )
        {
            this._rootNode = rootNode;
        }

        public override SyntaxNode TargetNode => this._rootNode;

        public override SyntaxNode? Substitute( SyntaxNode currentNode, SubstitutionContext substitutionContext )
        {
            switch ( currentNode )
            {
                case MethodDeclarationSyntax declaration:
                    return Block();

                default:
                    throw new AssertionFailedException();
            }
        }
    }
}