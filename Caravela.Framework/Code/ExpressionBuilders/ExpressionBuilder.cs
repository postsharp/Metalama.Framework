// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using System;
using System.Text;

namespace Caravela.Framework.Code.ExpressionBuilders
{
    [CompileTimeOnly]
    public sealed class ExpressionBuilder : IExpressionBuilder
    {
        private readonly IMetaCodeBuilder _impl;

        public StringBuilder StringBuilder { get; }

        public ExpressionBuilder()
        {
            this._impl = meta.CurrentContext.CodeBuilder;
            this.StringBuilder = new StringBuilder();
        }

        public ExpressionBuilder( ExpressionBuilder prototype )
        {
            this._impl = prototype._impl;
            this.StringBuilder = new StringBuilder( prototype.StringBuilder.ToString() );
        }

        public void AppendVerbatim( string rawCode ) => this.StringBuilder.Append( rawCode );

        public void AppendLiteral( int value ) => this._impl.AppendLiteral( value, this.StringBuilder, SpecialType.Int32, false );

        public void AppendLiteral( uint value, bool stronglyTyped = false )
            => this._impl.AppendLiteral( value, this.StringBuilder, SpecialType.UInt32, stronglyTyped );

        public void AppendLiteral( short value, bool stronglyTyped = false )
            => this._impl.AppendLiteral( value, this.StringBuilder, SpecialType.Int16, stronglyTyped );

        public void AppendLiteral( ushort value, bool stronglyTyped = false )
            => this._impl.AppendLiteral( value, this.StringBuilder, SpecialType.UInt16, stronglyTyped );

        public void AppendLiteral( long value, bool stronglyTyped = false )
            => this._impl.AppendLiteral( value, this.StringBuilder, SpecialType.Int64, stronglyTyped );

        public void AppendLiteral( ulong value, bool stronglyTyped = false )
            => this._impl.AppendLiteral( value, this.StringBuilder, SpecialType.UInt64, stronglyTyped );

        public void AppendLiteral( byte value, bool stronglyTyped = false )
            => this._impl.AppendLiteral( value, this.StringBuilder, SpecialType.Byte, stronglyTyped );

        public void AppendLiteral( sbyte value, bool stronglyTyped = false )
            => this._impl.AppendLiteral( value, this.StringBuilder, SpecialType.SByte, stronglyTyped );

        public void AppendLiteral( double value, bool stronglyTyped = false )
            => this._impl.AppendLiteral( value, this.StringBuilder, SpecialType.Double, stronglyTyped );

        public void AppendLiteral( float value, bool stronglyTyped = false )
            => this._impl.AppendLiteral( value, this.StringBuilder, SpecialType.Single, stronglyTyped );

        public void AppendLiteral( decimal value, bool stronglyTyped = false )
            => this._impl.AppendLiteral( value, this.StringBuilder, SpecialType.Decimal, stronglyTyped );

        public void AppendLiteral( string? value, bool stronglyTyped = false )
            => this._impl.AppendLiteral( value, this.StringBuilder, SpecialType.String, stronglyTyped );

        public void AppendTypeName( IType type ) => this._impl.AppendTypeName( type, this.StringBuilder );

        public void AppendTypeName( Type type ) => this._impl.AppendTypeName( type, this.StringBuilder );

        public void AppendExpression( IExpression expression ) => this._impl.AppendExpression( expression, this.StringBuilder );

        public void AppendExpression( IExpressionBuilder expression ) => this._impl.AppendExpression( expression.ToExpression(), this.StringBuilder );

        public void AppendExpression( dynamic? expression ) => this._impl.AppendDynamic( expression, this.StringBuilder );

        public IExpression ToExpression() => meta.ParseExpression( this.StringBuilder.ToString() );

        public ExpressionBuilder Clone() => new( this );
    }
}