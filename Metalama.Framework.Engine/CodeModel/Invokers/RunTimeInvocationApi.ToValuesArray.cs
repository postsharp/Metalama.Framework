﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.Templating.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using SpecialType = Microsoft.CodeAnalysis.SpecialType;

namespace Metalama.Framework.Engine.CodeModel.Invokers;

internal sealed class ValueArrayExpression : UserExpression
{
    private readonly ParameterList _parent;

    public ValueArrayExpression( IParameterList parent )
    {
        this._parent = (ParameterList) parent;
    }

    public override ExpressionSyntax ToSyntax( SyntaxGenerationContext syntaxGenerationContext )
    {
        var syntaxGenerator = syntaxGenerationContext.SyntaxGenerator;

        return syntaxGenerator.ArrayCreationExpression(
            syntaxGenerator.Type( SpecialType.System_Object ),
            this._parent.SelectAsEnumerable(
                p =>
                    RefKindExtensions.IsReadable( p.RefKind )
                        ? SyntaxFactory.IdentifierName( p.Name )
                        : (SyntaxNode) syntaxGenerator.DefaultExpression( p.Type.GetSymbol() ) ) );
    }

    public override IType Type => this._parent.Compilation.Factory.GetTypeByReflectionType( typeof(object[]) );
}