// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal partial class AdviceParameterList
    {
        private class ToValueTupleImpl : IDynamicExpression
        {
            private readonly AdviceParameterList _parent;

            public ToValueTupleImpl( AdviceParameterList parent )
            {
                this._parent = parent;
            }

            public RuntimeExpression CreateExpression( string? expressionText, Location? location )
            {
                ExpressionSyntax expression;

                if ( this._parent.Count == 0 )
                {
                    var valueType = this._parent.Compilation.Factory.GetTypeByReflectionType( typeof( ValueType ) ).GetSymbol();
                    expression = SyntaxFactory.DefaultExpression( (TypeSyntax) this._parent.Compilation.SyntaxGenerator.TypeExpression( valueType ) );
                }
                else
                {
                    expression = (ExpressionSyntax) this._parent.Compilation.SyntaxGenerator.TupleExpression(
                        this._parent._parameters.Select(
                            p =>
                                p.IsOut()
                                    ? this._parent.Compilation.SyntaxGenerator.DefaultExpression( p.ParameterType.GetSymbol() )
                                    : SyntaxFactory.IdentifierName( p.Name ) ) );
                }

                return new RuntimeExpression( expression );
            }
        }
    }
}