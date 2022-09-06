// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.RunTime;
using System.Collections.Generic;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal class BuiltProperty : BuiltMember, IPropertyImpl
    {
        public BuiltProperty( PropertyBuilder builder, CompilationModel compilation ) : base( compilation, builder )
        {
            this.PropertyBuilder = builder;
        }

        public PropertyBuilder PropertyBuilder { get; }

        public override MemberBuilder MemberBuilder => this.PropertyBuilder;

        public override MemberOrNamedTypeBuilder MemberOrNamedTypeBuilder => this.PropertyBuilder;

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

        [Memo]
        public IInvokerFactory<IFieldOrPropertyInvoker> Invokers
            => new InvokerFactory<IFieldOrPropertyInvoker>( ( order, invokerOperator ) => new FieldOrPropertyInvoker( this, order, invokerOperator ), false );

        public IProperty? OverriddenProperty => this.PropertyBuilder.OverriddenProperty;

        // TODO: When an interface is introduced, explicit implementation should appear here.
        public IReadOnlyList<IProperty> ExplicitInterfaceImplementations => this.PropertyBuilder.ExplicitInterfaceImplementations;

        public FieldOrPropertyInfo ToFieldOrPropertyInfo() => this.PropertyBuilder.ToFieldOrPropertyInfo();

        public PropertyInfo ToPropertyInfo() => this.PropertyBuilder.ToPropertyInfo();

        public IMethod? GetAccessor( MethodKind methodKind ) => this.GetAccessorImpl( methodKind );

        public IEnumerable<IMethod> Accessors => this.PropertyBuilder.Accessors;
    }
}