// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using System;
using System.Text;

namespace Metalama.Framework.Aspects;

[CompileTimeOnly]
internal interface ISyntaxBuilderImpl
{
    ICompilation Compilation { get; }

    IExpression Expression( object? expression );

    IExpression BuildArray( ArrayBuilder arrayBuilder );

    IExpression BuildInterpolatedString( InterpolatedStringBuilder interpolatedStringBuilder );

    IExpression ParseExpression( string code );

    IStatement ParseStatement( string code );

    void AppendLiteral( object? value, StringBuilder stringBuilder, SpecialType specialType, bool stronglyTyped );

    void AppendTypeName( IType type, StringBuilder stringBuilder );

    void AppendTypeName( Type type, StringBuilder stringBuilder );

    void AppendExpression( IExpression expression, StringBuilder stringBuilder );

    void AppendDynamic( object? expression, StringBuilder stringBuilder );
}