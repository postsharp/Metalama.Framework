﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Metalama.Framework.CompileTimeContracts;

public interface ITemplateSyntaxFactory
{
    void AddStatement( List<StatementOrTrivia> list, StatementSyntax statement );

    void AddStatement( List<StatementOrTrivia> list, IStatement statement );

    void AddStatement( List<StatementOrTrivia> list, IExpression expression );

    void AddStatement( List<StatementOrTrivia> list, string statement );

    void AddComments( List<StatementOrTrivia> list, params string?[]? comments );

    StatementSyntax? ToStatement( ExpressionSyntax expression );

    SyntaxList<StatementSyntax> ToStatementList( List<StatementOrTrivia> list );

    SyntaxKind Boolean( bool value );

    StatementSyntax ReturnStatement( ExpressionSyntax? returnExpression );

    StatementSyntax DynamicReturnStatement( IUserExpression returnExpression, bool awaitResult );

    StatementSyntax DynamicDiscardAssignment( IUserExpression? expression, bool awaitResult );

    StatementSyntax DynamicLocalDeclaration(
        TypeSyntax type,
        SyntaxToken identifier,
        IUserExpression? value,
        bool awaitResult );

    TypedExpressionSyntax? DynamicMemberAccessExpression( IUserExpression userExpression, string member );

    SyntaxToken GetUniqueIdentifier( string hint );

    ExpressionSyntax Serialize<T>( T o );

    T AddSimplifierAnnotations<T>( T node )
        where T : SyntaxNode;

    ExpressionSyntax RenderInterpolatedString( InterpolatedStringExpressionSyntax interpolatedString );

    ExpressionSyntax ConditionalExpression( ExpressionSyntax condition, ExpressionSyntax whenTrue, ExpressionSyntax whenFalse );

    IUserExpression? Proceed( string methodName );
    
    ExpressionSyntax? GetDynamicSyntax( object? expression );

    TypedExpressionSyntax RuntimeExpression( ExpressionSyntax syntax, string? type = null );

    ExpressionSyntax SuppressNullableWarningExpression( ExpressionSyntax operand );

    ExpressionSyntax StringLiteralExpression( string? value );

    TypeOfExpressionSyntax TypeOf( string typeId, Dictionary<string, TypeSyntax> substitutions );

    InterpolationSyntax FixInterpolationSyntax( InterpolationSyntax interpolation );

    ITemplateSyntaxFactory ForLocalFunction( string returnType, Dictionary<string, IType> genericArguments );
}