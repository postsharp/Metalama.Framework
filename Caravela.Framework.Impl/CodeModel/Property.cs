// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Code.Invokers;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.Invokers;
using Caravela.Framework.Impl.CodeModel.References;
using Caravela.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
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

        IFieldOrPropertyInvoker? IFieldOrProperty.BaseInvoker => this.BaseInvoker;

        IFieldOrPropertyInvoker IFieldOrProperty.Invoker => this.Invoker;

        [Memo]
        public IPropertyInvoker Invoker => new PropertyInvoker( this, InvokerOrder.Default );

        [Memo]
        public IPropertyInvoker BaseInvoker => new PropertyInvoker( this, InvokerOrder.Base );

        public override ISymbol Symbol => this._symbol;

        public RefKind RefKind => this._symbol.RefKind.ToOurRefKind();

        public bool IsByRef => this.RefKind != RefKind.None;

        public bool IsRef => this.RefKind == RefKind.Ref;

        public bool IsRefReadonly => this.RefKind == RefKind.RefReadOnly;

        public bool IsExplicitInterfaceImplementation => !this._symbol.ExplicitInterfaceImplementations.IsEmpty;

        [Memo]
        public IType Type => this.Compilation.Factory.GetIType( this._symbol.Type );

        [Memo]
        public IParameterList Parameters
            => new ParameterList(
                this,
                this._symbol.Parameters.Select( p => new DeclarationRef<IParameter>( p ) ) );

        [Memo]
        public IMethod? Getter => this._symbol.GetMethod == null ? null : this.Compilation.Factory.GetMethod( this._symbol.GetMethod );

        [Memo]

        // TODO: get-only properties
        public IMethod? Setter => this._symbol.SetMethod == null ? null : this.Compilation.Factory.GetMethod( this._symbol.SetMethod );

        public override DeclarationKind DeclarationKind => DeclarationKind.Property;

        public object GetValue( object? instance ) => this.Invoker.GetValue( instance );

        public object SetValue( object? instance, object value ) => this.Invoker.SetValue( instance, value );

        public object GetIndexerValue( object? instance, params object[] args ) => this.Invoker.GetIndexerValue( instance, args );

        public object SetIndexerValue( object? instance, object value, params object[] args ) => this.Invoker.SetIndexerValue( instance, value, args );

        [Memo]
        public IReadOnlyList<IProperty> ExplicitInterfaceImplementations
            => this._symbol.ExplicitInterfaceImplementations.Select( p => this.Compilation.Factory.GetProperty( p ) ).ToList();

        public PropertyInfo ToPropertyInfo() => CompileTimePropertyInfo.Create( this );

        IPropertyInvoker? IProperty.BaseInvoker => throw new NotImplementedException();

        public FieldOrPropertyInfo ToFieldOrPropertyInfo() => CompileTimeFieldOrPropertyInfo.Create( this );

        public override string ToString() => this._symbol.ToString();

        public override bool IsReadOnly => this._symbol.IsReadOnly;

        public override bool IsAsync => false;

        public override MemberInfo ToMemberInfo() => this.ToFieldOrPropertyInfo();
    }
}