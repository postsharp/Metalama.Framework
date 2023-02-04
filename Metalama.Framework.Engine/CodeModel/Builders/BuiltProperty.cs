﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.RunTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal sealed class BuiltProperty : BuiltMember, IPropertyImpl
    {
        public BuiltProperty( PropertyBuilder builder, CompilationModel compilation ) : base( compilation, builder )
        {
            this.PropertyBuilder = builder;
        }

        public PropertyBuilder PropertyBuilder { get; }

        protected override MemberBuilder MemberBuilder => this.PropertyBuilder;

        protected override MemberOrNamedTypeBuilder MemberOrNamedTypeBuilder => this.PropertyBuilder;

        public RefKind RefKind => this.PropertyBuilder.RefKind;

        public Writeability Writeability => this.PropertyBuilder.Writeability;

        public bool? IsAutoPropertyOrField => this.PropertyBuilder.IsAutoPropertyOrField;

        [Memo]
        public IType Type => this.Compilation.Factory.GetIType( this.PropertyBuilder.Type );

        [Memo]
        public IMethod? GetMethod
            => this.PropertyBuilder.GetMethod != null ? new BuiltAccessor( this, (AccessorBuilder) this.PropertyBuilder.GetMethod ) : null;

        [Memo]
        public IMethod? SetMethod
            => this.PropertyBuilder.SetMethod != null ? new BuiltAccessor( this, (AccessorBuilder) this.PropertyBuilder.SetMethod ) : null;

        [Obsolete]
        public IInvokerFactory<IFieldOrPropertyInvoker> Invokers => throw new NotSupportedException();

        [Memo]
        public IProperty? OverriddenProperty => this.Compilation.Factory.GetDeclaration( this.PropertyBuilder.OverriddenProperty );

        // TODO: When an interface is introduced, explicit implementation should appear here.
        [Memo]
        public IReadOnlyList<IProperty> ExplicitInterfaceImplementations
            => this.PropertyBuilder.ExplicitInterfaceImplementations.SelectAsImmutableArray( i => this.Compilation.Factory.GetDeclaration( i ) );

        public FieldOrPropertyInfo ToFieldOrPropertyInfo() => this.PropertyBuilder.ToFieldOrPropertyInfo();

        public bool IsRequired => this.PropertyBuilder.IsRequired;

        public IExpression? InitializerExpression => this.PropertyBuilder.InitializerExpression;

        public object? GetValue( object? target ) => TemplateExpansionContext.CurrentInvocationApi.GetValue( this, target );

        public object? SetValue( object? target, object? value ) => TemplateExpansionContext.CurrentInvocationApi.GetValue( this, target );

        public PropertyInfo ToPropertyInfo() => this.PropertyBuilder.ToPropertyInfo();

        public IMethod? GetAccessor( MethodKind methodKind ) => this.GetAccessorImpl( methodKind );

        public IEnumerable<IMethod> Accessors => this.PropertyBuilder.Accessors.Select( a => this.Compilation.Factory.GetDeclaration( a ) );

        bool IExpression.IsAssignable => this.Writeability != Writeability.None;

        public ref object? Value => ref RefHelper.Wrap( new FieldOrPropertyExpression( this, null ) );
    }
}