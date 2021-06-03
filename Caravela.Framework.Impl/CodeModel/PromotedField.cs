// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Code.Invokers;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.Invokers;
using Caravela.Framework.Impl.CodeModel.References;
using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Reflection;
using RefKind = Caravela.Framework.Code.RefKind;

namespace Caravela.Framework.Impl.CodeModel
{
    /// <summary>
    /// Represent a source-code field promoted to a property by an aspect.
    /// </summary>
    internal sealed class PromotedField : Member, IProperty, IReplaceMemberTransformation
    {
        private readonly IFieldSymbol _symbol;

        IFieldOrPropertyInvoker? IFieldOrProperty.BaseInvoker => this.BaseInvoker;

        IFieldOrPropertyInvoker IFieldOrProperty.Invoker => this.Invoker;

        [Memo]
        public IPropertyInvoker Invoker => new PropertyInvoker( this, InvokerOrder.Default );

        [Memo]
        public IPropertyInvoker BaseInvoker => new PropertyInvoker( this, InvokerOrder.Base );

        public override DeclarationKind DeclarationKind => DeclarationKind.Field;

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

        public object GetValue( object? instance ) => this.Invoker.GetValue( instance );

        public object SetValue( object? instance, object value ) => this.Invoker.SetValue( instance, value );

        public PropertyInfo ToPropertyInfo() => throw new NotImplementedException();

        public FieldOrPropertyInfo ToFieldOrPropertyInfo() => throw new NotImplementedException();

        RefKind IProperty.RefKind => RefKind.None;

        bool IProperty.IsByRef => false;

        bool IProperty.IsRef => false;

        bool IProperty.IsRefReadonly => false;

        IParameterList IHasParameters.Parameters => ParameterList.Empty;

        public override bool IsReadOnly => this._symbol.IsReadOnly;

        public override bool IsAsync => false;

        public IReadOnlyList<IProperty> ExplicitInterfaceImplementations => Array.Empty<IProperty>();

        public override MemberInfo ToMemberInfo() => this.ToFieldOrPropertyInfo();

        MemberRef<IMemberOrNamedType> IReplaceMemberTransformation.ReplacedMember => new( this._symbol );

        IDeclaration IObservableTransformation.ContainingDeclaration => this.ContainingDeclaration.AssertNotNull();
    }
}