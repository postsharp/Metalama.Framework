// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.RunTime;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MethodKind = Metalama.Framework.Code.MethodKind;
using RefKind = Metalama.Framework.Code.RefKind;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal class BuiltProperty : BuiltMember, IPropertyImpl, IMemberRef<IProperty>
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
                this.PropertyBuilder.Parameters.AsBuilderList.Select( Ref.FromBuilder<IParameter, IParameterBuilder> ) );

        public RefKind RefKind => this.PropertyBuilder.RefKind;

        public Writeability Writeability => this.PropertyBuilder.Writeability;

        public bool IsAutoPropertyOrField => this.PropertyBuilder.IsAutoPropertyOrField;

        public IType Type => this.PropertyBuilder.Type;

        [Memo]
        public IMethod? GetMethod
            => this.PropertyBuilder.GetMethod != null ? new BuiltAccessor( this, (AccessorBuilder) this.PropertyBuilder.GetMethod ) : null;

        [Memo]
        public IMethod? SetMethod
            => this.PropertyBuilder.SetMethod != null ? new BuiltAccessor( this, (AccessorBuilder) this.PropertyBuilder.SetMethod ) : null;

        IInvokerFactory<IFieldOrPropertyInvoker> IFieldOrProperty.Invokers => this.Invokers;

        [Memo]
        public IInvokerFactory<IPropertyInvoker> Invokers
            => new InvokerFactory<IPropertyInvoker>( ( order, invokerOperator ) => new PropertyInvoker( this, order, invokerOperator ), false );

        public IProperty? OverriddenProperty => this.PropertyBuilder.OverriddenProperty;

        // TODO: When an interface is introduced, explicit implementation should appear here.
        public IReadOnlyList<IProperty> ExplicitInterfaceImplementations => this.PropertyBuilder.ExplicitInterfaceImplementations;

        public FieldOrPropertyInfo ToFieldOrPropertyInfo() => this.PropertyBuilder.ToFieldOrPropertyInfo();

        public PropertyInfo ToPropertyInfo() => this.PropertyBuilder.ToPropertyInfo();

        string? IRef<IProperty>.ToSerializableId() => null;

        IProperty IRef<IProperty>.GetTarget( ICompilation compilation ) => (IProperty) this.GetForCompilation( compilation );

        ISymbol ISdkRef<IProperty>.GetSymbol( Compilation compilation ) => throw new NotSupportedException();

        public IMethod? GetAccessor( MethodKind methodKind ) => this.GetAccessorImpl( methodKind );

        public IEnumerable<IMethod> Accessors => this.PropertyBuilder.Accessors;
    }
}