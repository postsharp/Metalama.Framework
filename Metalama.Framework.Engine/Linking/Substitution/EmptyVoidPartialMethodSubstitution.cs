// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Linking.Substitution;

internal sealed class EmptyVoidPartialMethodSubstitution : SyntaxNodeSubstitution
{
    private readonly MethodDeclarationSyntax _rootNode;

    public EmptyVoidPartialMethodSubstitution( CompilationContext compilationContext, MethodDeclarationSyntax rootNode ) : base( compilationContext )
    {
        this._rootNode = rootNode;
    }

    public override SyntaxNode TargetNode => this._rootNode;

    public override SyntaxNode Substitute( SyntaxNode currentNode, SubstitutionContext substitutionContext )
    {
        switch ( currentNode )
        {
            case MethodDeclarationSyntax:
                return SyntaxFactoryEx.FormattedBlock().WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );

            default:
                throw new AssertionFailedException( $"{currentNode.Kind()} is not supported." );
        }
    }
}