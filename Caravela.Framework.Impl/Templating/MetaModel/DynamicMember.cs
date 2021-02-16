using Caravela.Framework.Code;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal class DynamicMember : IDynamicMember
    {
        private readonly ExpressionSyntax _expression;
        private readonly IType _type;
        private readonly bool _isReferenceable;

        public DynamicMember( ExpressionSyntax expression, IType type, bool isReferenceable )
        {
            this._expression = expression;
            this._type = type;
            this._isReferenceable = isReferenceable;
        }

        public RuntimeExpression CreateExpression() => new( this._expression, this._type, this._isReferenceable );
    }
}