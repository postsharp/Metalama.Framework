// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.References;
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

        [Memo]
        private PropertyInvocation Invocation => new( this );

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
        public IMethod? Getter => new PseudoAccessor( this, AccessorSemantic.Get );

        [Memo]
        public IMethod? Setter => new PseudoAccessor( this, AccessorSemantic.Set );

        public dynamic Value
        {
            get => new FieldOrPropertyInvocation( this ).Value;
            set => throw new InvalidOperationException();
        }

        public object GetValue( object? instance ) => this.Invocation.GetValue( instance );

        public object SetValue( object? instance, object value ) => this.Invocation.SetValue( instance, value );

        public bool HasBase => true;

        IFieldOrPropertyInvocation IFieldOrProperty.Base => new PropertyInvocation( this ).Base;

        public PropertyInfo ToPropertyInfo() => throw new NotImplementedException();

        public FieldOrPropertyInfo ToFieldOrPropertyInfo() => throw new NotImplementedException();

        public IPropertyInvocation Base => throw new NotImplementedException();

        RefKind IProperty.RefKind => RefKind.None;

        public bool IsAutoPropertyOrField => true;

        public Writeability Writeability => this._symbol.IsReadOnly ? Writeability.ConstructorOnly : Writeability.All;

        IParameterList IProperty.Parameters => ParameterList.Empty;

        public override bool IsAsync => false;

        public IReadOnlyList<IProperty> ExplicitInterfaceImplementations => Array.Empty<IProperty>();

        public override MemberInfo ToMemberInfo() => this.ToFieldOrPropertyInfo();

        dynamic IPropertyInvocation.GetIndexerValue( dynamic? instance, params dynamic[] args ) => this.Invocation.GetIndexerValue( instance, args );

        dynamic IPropertyInvocation.SetIndexerValue( dynamic? instance, dynamic value, params dynamic[] args )
            => this.Invocation.SetIndexerValue( instance, value, args );

        MemberRef<IMemberOrNamedType> IReplaceMemberTransformation.ReplacedMember => new( this._symbol );

        IDeclaration IObservableTransformation.ContainingDeclaration => this.ContainingDeclaration.AssertNotNull();
    }
}