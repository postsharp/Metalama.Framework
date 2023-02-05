﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MethodKind = Metalama.Framework.Code.MethodKind;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal sealed class BuiltEvent : BuiltMember, IEventImpl
    {
        public BuiltEvent( EventBuilder builder, CompilationModel compilation ) : base( compilation, builder )
        {
            this.EventBuilder = builder;
        }

        public EventBuilder EventBuilder { get; }

        protected override MemberBuilder MemberBuilder => this.EventBuilder;

        protected override MemberOrNamedTypeBuilder MemberOrNamedTypeBuilder => this.EventBuilder;

        [Memo]
        public INamedType Type => this.Compilation.Factory.GetDeclaration( this.EventBuilder.Type );

        public IMethod Signature => this.Type.Methods.OfName( "Invoke" ).Single();

        [Memo]
        public IMethod AddMethod => new BuiltAccessor( this, (AccessorBuilder) this.EventBuilder.AddMethod );

        [Memo]
        public IMethod RemoveMethod => new BuiltAccessor( this, (AccessorBuilder) this.EventBuilder.RemoveMethod );

        public IMethod? RaiseMethod => null;

        [Obsolete]
        IInvokerFactory<IEventInvoker> IEvent.Invokers => throw new NotSupportedException();

        [Memo]
        public IEvent? OverriddenEvent => this.Compilation.Factory.GetDeclaration( this.EventBuilder.OverriddenEvent );

        // TODO: When an interface is introduced, explicit implementation should appear here.
        [Memo]
        public IReadOnlyList<IEvent> ExplicitInterfaceImplementations
            => this.EventBuilder.ExplicitInterfaceImplementations.SelectAsImmutableArray( i => this.Compilation.Factory.GetDeclaration( i ) );

        public EventInfo ToEventInfo() => this.EventBuilder.ToEventInfo();

        public IEventInvoker With( InvokerOptions options ) => this.EventBuilder.With( options );

        public IEventInvoker With( object target, InvokerOptions options = default ) => this.EventBuilder.With( target, options );

        public object Add( object? handler ) => this.EventBuilder.Add( handler );

        public object Remove( object? handler ) => this.EventBuilder.Remove( handler );

        public object? Raise( params object?[] args ) => this.EventBuilder.Raise( args );

        public IMethod? GetAccessor( MethodKind methodKind ) => this.GetAccessorImpl( methodKind );

        public IEnumerable<IMethod> Accessors => this.EventBuilder.Accessors.Select( x => this.Compilation.Factory.GetDeclaration( x ) );

        IType IHasType.Type => this.Type;

        RefKind IHasType.RefKind => RefKind.None;
    }
}