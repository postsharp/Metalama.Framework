// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Linking.Substitution;

internal sealed class EmptyVoidPartialMethodSubstitution : SyntaxNodeSubstitution
{
    private readonly MethodDeclarationSyntax _rootNode;

    public EmptyVoidPartialMethodSubstitution( CompilationContext compilationContext, MethodDeclarationSyntax rootNode ) : base( compilationContext )
    {
        this._rootNode = rootNode;
    }

    public override SyntaxNode ReplacedNode => this._rootNode;

    public override SyntaxNode Substitute( SyntaxNode currentNode, SubstitutionContext substitutionContext )
    {
        switch ( currentNode )
        {
            case MethodDeclarationSyntax:
                return substitutionContext.SyntaxGenerationContext.SyntaxGenerator.FormattedBlock()
                    .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );

            default:
                throw new AssertionFailedException( $"Unsupported syntax: {currentNode}" );
        }
    }
}