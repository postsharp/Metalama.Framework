// unset

using Caravela.Framework.Code;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal partial class AdviceParameterList
    {
        private class ToValueTupleImpl : IDynamicMember
        {
            private readonly AdviceParameterList _parent;

            public ToValueTupleImpl( AdviceParameterList parent )
            {
                this._parent = parent;
            }

            public RuntimeExpression CreateExpression()
            {
                ExpressionSyntax expression;
                if ( this._parent.Count == 0 )
                {
                    var valueType = this._parent.Compilation.Factory.GetTypeByReflectionType( typeof(ValueType) ).GetSymbol();
                    expression = SyntaxFactory.DefaultExpression( (TypeSyntax) this._parent.Compilation.SyntaxGenerator.TypeExpression( valueType ) );
                }
                else
                {
                     expression = (ExpressionSyntax) this._parent.Compilation.SyntaxGenerator.TupleExpression(
                        this._parent._parameters.Select(
                            p =>
                                p.IsOut()
                                    ? this._parent.Compilation.SyntaxGenerator.DefaultExpression( p.ParameterType.GetSymbol() )
                                    : (ExpressionSyntax) SyntaxFactory.IdentifierName( p.Name ) )
                    );
                    
                }
                
                return new RuntimeExpression( expression);
            }
        }
    }
}