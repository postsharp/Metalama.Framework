// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal partial class AdvisedParameterList
    {
        private class ToValueTupleImpl : IDynamicExpression
        {
            private readonly AdvisedParameterList _parent;

            public ToValueTupleImpl( AdvisedParameterList parent )
            {
                this._parent = parent;
            }

            public RuntimeExpression? CreateExpression( string? expressionText, Location? location = null )
            {
                ExpressionSyntax expression;

                if ( this._parent.Count == 0 )
                {
                    var valueType = this._parent.Compilation.Factory.GetTypeByReflectionType( typeof(ValueType) ).GetSymbol();
                    expression = SyntaxFactory.DefaultExpression( LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( valueType ) );
                }
                else
                {
                    expression = LanguageServiceFactory.CSharpSyntaxGenerator.TupleExpression(
                        this._parent._parameters.Select(
                            p =>
                                p.IsOut()
                                    ? (SyntaxNode) LanguageServiceFactory.CSharpSyntaxGenerator.DefaultExpression( p.ParameterType.GetSymbol() )
                                    : SyntaxFactory.IdentifierName( p.Name ) ) );
                }

                return new RuntimeExpression( expression );
            }
        }
    }
}