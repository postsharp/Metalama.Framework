// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Code.Invokers;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.Invokers;
using Caravela.Framework.Impl.CodeModel.References;
using Caravela.Framework.Impl.ReflectionMocks;
using Caravela.Framework.RunTime;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MethodKind = Caravela.Framework.Code.MethodKind;
using RefKind = Caravela.Framework.Code.RefKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal sealed class Property : Member, IPropertyInternal
    {
        private readonly IPropertySymbol _symbol;

        public Property( IPropertySymbol symbol, CompilationModel compilation ) : base( compilation )
        {
            this._symbol = symbol;
        }

        IInvokerFactory<IFieldOrPropertyInvoker> IFieldOrProperty.Invokers => this.Invokers;

        [Memo]
        public IInvokerFactory<IPropertyInvoker> Invokers
            => new InvokerFactory<IPropertyInvoker>( ( order, invokerOperator ) => new PropertyInvoker( this, order, invokerOperator ) );

        public override ISymbol Symbol => this._symbol;

        public RefKind RefKind => this._symbol.RefKind.ToOurRefKind();

        public override bool IsExplicitInterfaceImplementation => !this._symbol.ExplicitInterfaceImplementations.IsEmpty;

        [Memo]
        public IType Type => this.Compilation.Factory.GetIType( this._symbol.Type );

        [Memo]
        public IParameterList Parameters
            => new ParameterList(
                this,
                this._symbol.Parameters.Select( p => new DeclarationRef<IParameter>( p ) ) );

        [Memo]
        public IMethod? GetMethod => this._symbol.GetMethod == null ? null : this.Compilation.Factory.GetMethod( this._symbol.GetMethod );

        [Memo]

        // TODO: get-only properties
        public IMethod? SetMethod => this._symbol.SetMethod == null ? null : this.Compilation.Factory.GetMethod( this._symbol.SetMethod );

        public override DeclarationKind DeclarationKind => DeclarationKind.Property;

        public IProperty? OverriddenProperty
        {
            get
            {
                var overriddenProperty = this._symbol.OverriddenProperty;

                if ( overriddenProperty != null )
                {
                    return this.Compilation.Factory.GetProperty( overriddenProperty );
                }
                else
                {
                    return null;
                }
            }
        }

        [Memo]
        public IReadOnlyList<IProperty> ExplicitInterfaceImplementations
            => this._symbol.ExplicitInterfaceImplementations.Select( p => this.Compilation.Factory.GetProperty( p ) ).ToList();

        public PropertyInfo ToPropertyInfo() => CompileTimePropertyInfo.Create( this );

        public FieldOrPropertyInfo ToFieldOrPropertyInfo() => CompileTimeFieldOrPropertyInfo.Create( this );

        public override string ToString() => this._symbol.ToString();

        // TODO: Memo does not work here.
        // [Memo]
        public Writeability Writeability
            => this._symbol switch
            {
                { IsReadOnly: true } => Writeability.None,
                { SetMethod: { IsInitOnly: true } _ } => Writeability.InitOnly,
                _ => Writeability.All
            };

        // TODO: Memo does not work here.
        // [Memo]
        public bool IsAutoPropertyOrField => this._symbol.IsAutoProperty();

        public override bool IsAsync => false;

        public override MemberInfo ToMemberInfo() => this.ToFieldOrPropertyInfo();

        public IMethod? GetAccessor( MethodKind methodKind ) => this.GetAccessorImpl( methodKind );
    }
}