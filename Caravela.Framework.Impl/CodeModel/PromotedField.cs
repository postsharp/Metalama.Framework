// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Code.Invokers;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.Invokers;
using Caravela.Framework.Impl.CodeModel.References;
using Caravela.Framework.Impl.Transformations;
using Caravela.Framework.RunTime;
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
        public IInvokerFactory<IPropertyInvoker> Invokers => new InvokerFactory<IPropertyInvoker>( order => new PropertyInvoker( this, order ) );

        IInvokerFactory<IFieldOrPropertyInvoker> IFieldOrProperty.Invokers => this.Invokers;

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

        public PropertyInfo ToPropertyInfo() => throw new NotImplementedException();

        public FieldOrPropertyInfo ToFieldOrPropertyInfo() => throw new NotImplementedException();

        RefKind IProperty.RefKind => RefKind.None;

        public bool IsAutoPropertyOrField => true;

        public Writeability Writeability => this._symbol.IsReadOnly ? Writeability.ConstructorOnly : Writeability.All;

        IParameterList IHasParameters.Parameters => ParameterList.Empty;

        public override bool IsAsync => false;

        public IReadOnlyList<IProperty> ExplicitInterfaceImplementations => Array.Empty<IProperty>();

        public override MemberInfo ToMemberInfo() => this.ToFieldOrPropertyInfo();

        MemberRef<IMemberOrNamedType> IReplaceMemberTransformation.ReplacedMember => new( this._symbol );

        IDeclaration IObservableTransformation.ContainingDeclaration => this.ContainingDeclaration.AssertNotNull();
    }
}