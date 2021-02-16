using System.Linq;
using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal partial class AdviceParameterList
    {
        private class ToArrayImpl : IDynamicMember
        {
            private readonly AdviceParameterList _parent;

            public ToArrayImpl( AdviceParameterList parent )
            {
                this._parent = parent;
            }

            public RuntimeExpression CreateExpression()
            {
                var syntaxGenerator = this._parent.Compilation.SyntaxGenerator;
                var array = (ExpressionSyntax) syntaxGenerator.ArrayCreationExpression(
                    syntaxGenerator.TypeExpression( SpecialType.System_Object ),
                    this._parent._parameters.Select(
                        p =>
                            p.IsOut()
                                ? this._parent.Compilation.SyntaxGenerator.DefaultExpression( p.ParameterType.GetSymbol() )
                                : SyntaxFactory.IdentifierName( p.Name ) ) );

                return new RuntimeExpression(
                    array,
                    this._parent.Compilation.Factory.GetTypeByReflectionType( typeof( object[] ) ),
                    false );

            }
        }
    }
}