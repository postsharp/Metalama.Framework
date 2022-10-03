// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using System;
using System.Text;

namespace Metalama.Framework.Aspects;

[CompileTime]
internal interface ISyntaxBuilderImpl
{
    ICompilation Compilation { get; }

    IExpression Capture( object? expression );

    IExpression BuildArray( ArrayBuilder arrayBuilder );

    IExpression BuildInterpolatedString( InterpolatedStringBuilder interpolatedStringBuilder );

    IExpression ParseExpression( string code );

    IStatement ParseStatement( string code );

    IStatement CreateExpressionStatement( IExpression expression );

    void AppendLiteral( object? value, StringBuilder stringBuilder, SpecialType specialType, bool stronglyTyped );

    IExpression Literal( object? value, SpecialType specialType, bool stronglyTyped );

    void AppendTypeName( IType type, StringBuilder stringBuilder );

    void AppendTypeName( Type type, StringBuilder stringBuilder );

    void AppendExpression( IExpression expression, StringBuilder stringBuilder );

    void AppendDynamic( object? expression, StringBuilder stringBuilder );

    IExpression Cast( IExpression expression, IType targetType );

    object TypedConstant( in TypedConstant typedConstant );

    IExpression ThisExpression( INamedType type );

    IExpression ToExpression( IFieldOrProperty fieldOrProperty, IExpression? instance );

    IExpression ToExpression( IParameter parameter );
}