﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.Builders;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Code.Invokers;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.Invokers;
using Caravela.Framework.Impl.CodeModel.References;
using Caravela.Framework.RunTime;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RefKind = Caravela.Framework.Code.RefKind;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class BuiltProperty : BuiltMember, IProperty, IMemberRef<IProperty>
    {
        public BuiltProperty( PropertyBuilder builder, CompilationModel compilation ) : base( compilation )
        {
            this.PropertyBuilder = builder;
        }

        public PropertyBuilder PropertyBuilder { get; }

        public override MemberBuilder MemberBuilder => this.PropertyBuilder;

        public override MemberOrNamedTypeBuilder MemberOrNamedTypeBuilder => this.PropertyBuilder;

        [Memo]
        public IParameterList Parameters
            => new ParameterList(
                this,
                this.PropertyBuilder.Parameters.AsBuilderList.Select( DeclarationRef.FromBuilder<IParameter, IParameterBuilder> ) );

        public RefKind RefKind => this.PropertyBuilder.RefKind;

        public Writeability Writeability => this.PropertyBuilder.Writeability;

        public bool IsAutoPropertyOrField => this.PropertyBuilder.IsAutoPropertyOrField;

        public IType Type => this.PropertyBuilder.Type;

        [Memo]
        public IMethod? Getter => this.PropertyBuilder.Getter != null ? new BuiltAccessor( this, (AccessorBuilder) this.PropertyBuilder.Getter ) : null;

        [Memo]
        public IMethod? Setter => this.PropertyBuilder.Setter != null ? new BuiltAccessor( this, (AccessorBuilder) this.PropertyBuilder.Setter ) : null;

        IInvokerFactory<IFieldOrPropertyInvoker> IFieldOrProperty.Invokers => this.Invokers;

        [Memo]
        public IInvokerFactory<IPropertyInvoker> Invokers => new InvokerFactory<IPropertyInvoker>( order => new PropertyInvoker( this, order ), false );

        public IProperty? OverriddenProperty => this.PropertyBuilder.OverriddenProperty;

        // TODO: When an interface is introduced, explicit implementation should appear here.
        public IReadOnlyList<IProperty> ExplicitInterfaceImplementations => Array.Empty<IProperty>();

        [return: RunTimeOnly]
        public FieldOrPropertyInfo ToFieldOrPropertyInfo() => throw new NotImplementedException();

        [return: RunTimeOnly]
        public PropertyInfo ToPropertyInfo() => throw new NotImplementedException();

        IProperty IDeclarationRef<IProperty>.Resolve( CompilationModel compilation ) => (IProperty) this.GetForCompilation( compilation );

        ISymbol IDeclarationRef<IProperty>.GetSymbol( Compilation compilation ) => throw new NotSupportedException();
    }
}