// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Collections;
using Microsoft.CodeAnalysis;
using RefKind = Caravela.Framework.Code.RefKind;

namespace Caravela.Framework.Impl.CodeModel
{

    /// <summary>
    /// Represent a source-code field promoted to a property by an aspect.
    /// </summary>
    internal sealed class PromotedField : Member, IProperty
    {
        private readonly IFieldSymbol _symbol;
        
        public override CodeElementKind ElementKind => CodeElementKind.Field;

        public override ISymbol Symbol => this._symbol;

        public PromotedField( IFieldSymbol symbol, CompilationModel compilation ) : base( compilation )
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

        public object GetValue( object? instance ) => new PropertyInvocation( this ).GetValue( instance );

        public object SetValue( object? instance, object value ) => new PropertyInvocation( this ).SetValue( instance, value );

        public bool HasBase => true;

        IFieldOrPropertyInvocation IFieldOrProperty.Base => new PropertyInvocation( this ).Base;

        public IPropertyInvocation Base => this.Base;

        RefKind IProperty.RefKind => RefKind.None;

        bool IProperty.IsByRef => false;

        bool IProperty.IsRef => false;

        bool IProperty.IsRefReadonly => false;

        IParameterList IProperty.Parameters => ParameterList.Empty;

        public override bool IsReadOnly => this._symbol.IsReadOnly;

        public override bool IsAsync => false;

        dynamic IPropertyInvocation.GetIndexerValue( dynamic? instance, params dynamic[] args ) => throw new NotImplementedException();

        dynamic IPropertyInvocation.SetIndexerValue( dynamic? instance, dynamic value, params dynamic[] args ) => throw new NotImplementedException();
    }
}
