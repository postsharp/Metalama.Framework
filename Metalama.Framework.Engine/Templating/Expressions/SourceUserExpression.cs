﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using TypedConstant = Metalama.Framework.Code.TypedConstant;
using TypeKind = Microsoft.CodeAnalysis.TypeKind;

namespace Metalama.Framework.Engine.Templating.Expressions;

internal sealed class SourceUserExpression : SyntaxUserExpression, ISourceExpression
{
    public SourceUserExpression( ExpressionSyntax expression, IType type, bool isReferenceable = false, bool isAssignable = false ) : base(
        expression,
        type,
        isReferenceable,
        isAssignable ) { }

    public object AsSyntaxNode => this.Expression;

    [Memo]
    public string AsString => this.Expression.ToString();

    [Memo]
    public string AsFullString => this.Expression.ToFullString();

    [Memo]
    public TypedConstant? AsTypedConstant => this.GetTypeConstant( this.Expression );

    private TypedConstant? GetTypeConstant( ExpressionSyntax expression )
    {
        switch ( expression )
        {
            case PostfixUnaryExpressionSyntax postFix when postFix.OperatorToken.Kind() == SyntaxKind.ExclamationToken:
                return this.GetTypeConstant( postFix.Operand );

            case LiteralExpressionSyntax literal:
                var value = literal.Token.Value;

                if ( value != null )
                {
                    return TypedConstant.Create( value, this.Type.GetCompilationModel().Factory.GetTypeByReflectionType( value.GetType() ) );
                }
                else
                {
                    return TypedConstant.Default( this.Type );
                }

            case MemberAccessExpressionSyntax memberAccess:
                var semanticModel = this.Type.GetCompilationModel().CompilationContext.SemanticModelProvider.GetSemanticModel( this.Expression.SyntaxTree );
                var member = semanticModel.GetSymbolInfo( memberAccess ).Symbol;

                if ( member is IFieldSymbol field && field.ContainingType.TypeKind == TypeKind.Enum )
                {
                    return TypedConstant.Create( field.ConstantValue, this.Type );
                }
                else
                {
                    return null;
                }

            default:
                return null;
        }
    }
}