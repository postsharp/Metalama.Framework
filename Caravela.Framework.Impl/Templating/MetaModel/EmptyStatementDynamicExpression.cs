// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal class EmptyStatementDynamicExpression : IDynamicExpression
    {
        public EmptyStatementDynamicExpression( IType type )
        {
            this.ExpressionType = type;
        }

        public RuntimeExpression? CreateExpression( string? expressionText = null, Location? location = null ) => null;

        public IType ExpressionType { get; }
    }
}