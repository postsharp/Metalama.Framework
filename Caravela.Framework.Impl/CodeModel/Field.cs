// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel
{
    internal sealed class Field : Member, IField
    {
        private readonly IFieldSymbol _symbol;

        public override CodeElementKind ElementKind => CodeElementKind.Field;

        public override ISymbol Symbol => this._symbol;

        public Field( IFieldSymbol symbol, CompilationModel compilation ) : base( compilation )
        {
            this._symbol = symbol;
        }

        [Memo]
        public IType Type => this.Compilation.Factory.GetIType( this._symbol.Type );

        // TODO: pseudo-accessors
        [Memo]
        public IMethod? Getter => null;

        [Memo]
        public IMethod? Setter => null;

        public dynamic Value
        {
            get => new FieldOrPropertyInvocation( this ).Value;
            set => throw new InvalidOperationException();
        }

        public object GetValue( object? instance ) => new FieldOrPropertyInvocation( this ).GetValue( instance );

        public object SetValue( object? instance, object value ) => new FieldOrPropertyInvocation( this ).SetValue( instance, value );

        public bool HasBase => true;

        public IFieldOrPropertyInvocation Base => new FieldOrPropertyInvocation( this ).Base;

        public override bool IsReadOnly => this._symbol.IsReadOnly;

        public override bool IsAsync => false;
    }
}
