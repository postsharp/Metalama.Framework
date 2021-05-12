// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using System;

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
        private FieldOrPropertyInvocation Invocation => new( this );

        [Memo]
        public IType Type => this.Compilation.Factory.GetIType( this._symbol.Type );

        [Memo]
        public IMethod? Getter => new PseudoAccessor( this, AccessorSemantic.Get );

        [Memo]
        public IMethod? Setter => new PseudoAccessor( this, AccessorSemantic.Set );

        public dynamic Value
        {
            get => this.Invocation.Value;
            set => throw new InvalidOperationException();
        }

        public dynamic GetValue( object? instance ) => this.Invocation.GetValue( instance );

        public dynamic SetValue( object? instance, object value ) => this.Invocation.SetValue( instance, value );

        public bool HasBase => true;

        public IFieldOrPropertyInvocation Base => this.Invocation.Base;

        public override bool IsReadOnly => this._symbol.IsReadOnly;

        public override bool IsAsync => false;
    }
}