// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Templating.Expressions;

internal class FieldOrPropertyExpression : UserExpression
{
    private readonly IFieldOrProperty _fieldOrProperty;
    private readonly UserExpression? _instance;

    public FieldOrPropertyExpression( IFieldOrProperty fieldOrProperty, UserExpression? instance )
    {
        this._fieldOrProperty = fieldOrProperty;
        this._instance = instance;
    }

    protected override ExpressionSyntax ToSyntax( SyntaxGenerationContext syntaxGenerationContext )
    {
        if ( this._fieldOrProperty.IsStatic )
        {
            return SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                syntaxGenerationContext.SyntaxGenerator.Type( this._fieldOrProperty.DeclaringType.GetSymbol() ),
                SyntaxFactory.IdentifierName( this._fieldOrProperty.Name ) );
        }
        else if ( this._instance != null )
        {
            return SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                this._instance.ToExpressionSyntax( syntaxGenerationContext ),
                SyntaxFactory.IdentifierName( this._fieldOrProperty.Name ) );
        }
        else
        {
            return SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.ThisExpression(),
                SyntaxFactory.IdentifierName( this._fieldOrProperty.Name ) );
        }
    }

    public override IType Type => this._fieldOrProperty.Type;
}