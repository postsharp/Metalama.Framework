// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.Links;
using Caravela.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Reflection;
using RefKind = Caravela.Framework.Code.RefKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal sealed class Property : Member, IProperty
    {
        private readonly IPropertySymbol _symbol;

        public Property( IPropertySymbol symbol, CompilationModel compilation ) : base( compilation )
        {
            this._symbol = symbol;
        }

        [Memo]
        private PropertyInvocation Invocation => new( this );

        public override ISymbol Symbol => this._symbol;

        public RefKind RefKind => this._symbol.RefKind.ToOurRefKind();

        public bool IsByRef => this.RefKind != RefKind.None;

        public bool IsRef => this.RefKind == RefKind.Ref;

        public bool IsRefReadonly => this.RefKind == RefKind.RefReadOnly;

        [Memo]
        public IType Type => this.Compilation.Factory.GetIType( this._symbol.Type );

        [Memo]
        public IParameterList Parameters
            => new ParameterList(
                this,
                this._symbol.Parameters.Select( p => new CodeElementLink<IParameter>( p ) ) );

        [Memo]
        public IMethod? Getter => this._symbol.GetMethod == null ? null : this.Compilation.Factory.GetMethod( this._symbol.GetMethod );

        [Memo]

        // TODO: get-only properties
        public IMethod? Setter => this._symbol.SetMethod == null ? null : this.Compilation.Factory.GetMethod( this._symbol.SetMethod );

        public override CodeElementKind ElementKind => CodeElementKind.Property;

        public object Value
        {
            get => new PropertyInvocation( this ).Value;
            set => throw new InvalidOperationException();
        }

        public object GetValue( object? instance ) => this.Invocation.GetValue( instance );

        public object SetValue( object? instance, object value ) => this.Invocation.SetValue( instance, value );

        public object GetIndexerValue( object? instance, params object[] args ) => this.Invocation.GetIndexerValue( instance, args );

        public object SetIndexerValue( object? instance, object value, params object[] args ) => this.Invocation.SetIndexerValue( instance, value, args );

        public bool HasBase => true;

        IFieldOrPropertyInvocation IFieldOrProperty.Base => this.Base;

        public FieldOrPropertyInfo ToFieldOrPropertyInfo() => new CompileTimeFieldOrPropertyInfo( this );

        public IPropertyInvocation Base => this.Invocation.Base;

        public override string ToString() => this._symbol.ToString();

        public override bool IsReadOnly => this._symbol.IsReadOnly;

        public override bool IsAsync => false;

        public override MemberInfo ToMemberInfo() => this.ToFieldOrPropertyInfo();
    }
}