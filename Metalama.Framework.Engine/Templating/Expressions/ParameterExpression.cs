// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Templating.Expressions;

internal class ParameterExpression : UserExpression
{
    private readonly IParameter _parameter;

    public ParameterExpression( IParameter parameter )
    {
        this._parameter = parameter;
    }

    public override ExpressionSyntax ToSyntax( SyntaxGenerationContext syntaxGenerationContext ) => SyntaxFactory.IdentifierName( this._parameter.Name );

    public override IType Type => this._parameter.Type;
}