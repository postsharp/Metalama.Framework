// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using Caravela.Framework.Code;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating.MetaModel
{

    internal class CurrentTypeOrInstanceDynamic : IDynamicMemberDifferentiated
    {
        private readonly bool _allowExpression;
        private readonly IType _type;

        public CurrentTypeOrInstanceDynamic( bool allowExpression, IType type )
        {
            this._allowExpression = allowExpression;
            this._type = type;
        }

        public RuntimeExpression CreateExpression()
        {
            if ( this._allowExpression )
            {
                return new( ThisExpression(), this._type );
            }

            // TODO: Diagnostic.
            throw new InvalidOperationException( "Cannot directly access 'this' on a static method." );
        }

        RuntimeExpression IDynamicMemberDifferentiated.CreateMemberAccessExpression( string member ) => new( IdentifierName( Identifier( member ) ) );
    }
}
