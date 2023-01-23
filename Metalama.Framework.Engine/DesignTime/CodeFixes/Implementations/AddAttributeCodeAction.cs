// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.DesignTime.CodeFixes.Implementations;

internal sealed class AddAttributeCodeAction : ICodeAction
{
    private readonly AttributeConstruction _attribute;

    public IDeclaration TargetDeclaration { get; }
    
    public AddAttributeCodeAction( IDeclaration targetDeclaration, AttributeConstruction attribute )
    {
        this.TargetDeclaration = targetDeclaration;
        this._attribute = attribute;
    }

    public async Task ExecuteAsync( CodeActionContext context )
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        var compilation = context.Compilation.Compilation;

        var targetSymbol = this.TargetDeclaration.ToRef().GetSymbol( compilation );

        if ( targetSymbol == null )
        {
            throw new ArgumentOutOfRangeException( nameof(this.TargetDeclaration), "The declaration is not declared in source." );
        }

        var originalNode = this.TargetDeclaration.GetPrimaryDeclarationSyntax().AssertNotNull();

        if ( originalNode is VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax variableDeclaration } )
        {
            originalNode = variableDeclaration.Parent!;
        }

        var originalTree = originalNode.SyntaxTree;
        var originalRoot = await originalTree.GetRootAsync( context.CancellationToken );

        var generationContext = SyntaxGenerationContext.Create( context.CompilationContext, originalNode );
        var transformedNode = generationContext.SyntaxGenerator.AddAttribute( originalNode, this._attribute );

        var transformedRoot = originalRoot.ReplaceNode( originalNode, transformedNode );

        context.UpdateTree( transformedRoot, originalTree );
    }
}