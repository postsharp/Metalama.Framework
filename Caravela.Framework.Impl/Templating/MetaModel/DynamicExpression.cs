// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal class DynamicExpression : IDynamicExpression
    {
        private readonly ExpressionSyntax _expression;
        private readonly IType _type;
        private readonly bool _isReferenceable;

        public DynamicExpression( ExpressionSyntax expression, IType type, bool isReferenceable )
        {
            this._expression = expression;
            this._type = type;
            this._isReferenceable = isReferenceable;
        }

        public RuntimeExpression? CreateExpression( string? expressionText, Location? location = null )
            => new( this._expression, this._type, this._isReferenceable );
    }
}