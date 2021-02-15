// unset

using Caravela.Framework.Code;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

            public RuntimeExpression CreateExpression() =>
                new RuntimeExpression( (ExpressionSyntax) this._parent.Compilation.SyntaxGenerator.TupleExpression(
                        this._parent._parameters.Select(
                            p =>
                                p.IsOut()
                                    ? this._parent.Compilation.SyntaxGenerator.DefaultExpression( CodeModelExtensions.GetSymbol( (IType) p.ParameterType ) )
                                    : (ExpressionSyntax) SyntaxFactory.IdentifierName( p.Name ) )
                    ),
                    null,
                    false );
        }
    }
}