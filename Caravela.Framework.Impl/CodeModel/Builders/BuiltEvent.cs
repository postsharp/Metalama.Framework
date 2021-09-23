// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Invokers;
using Caravela.Framework.Impl.CodeModel.Invokers;
using Caravela.Framework.Impl.CodeModel.References;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MethodKind = Caravela.Framework.Code.MethodKind;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class BuiltEvent : BuiltMember, IEventImpl, IMemberRef<IEvent>
    {
        public BuiltEvent( EventBuilder builder, CompilationModel compilation ) : base( compilation )
        {
            this.EventBuilder = builder;
        }

        public EventBuilder EventBuilder { get; }

        public override MemberBuilder MemberBuilder => this.EventBuilder;

        public override MemberOrNamedTypeBuilder MemberOrNamedTypeBuilder => this.EventBuilder;

        public INamedType Type => this.EventBuilder.Type;

        public IMethod Signature => this.Type.Methods.OfName( "Invoke" ).Single();

        [Memo]
        public IMethod AddMethod => new BuiltAccessor( this, (AccessorBuilder) this.EventBuilder.AddMethod );

        [Memo]
        public IMethod RemoveMethod => new BuiltAccessor( this, (AccessorBuilder) this.EventBuilder.RemoveMethod );

        public IMethod? RaiseMethod => null;

        [Memo]
        public IInvokerFactory<IEventInvoker> Invokers
            => new InvokerFactory<IEventInvoker>( ( order, invokerOperator ) => new EventInvoker( this, order, invokerOperator ), false );

        public IEvent? OverriddenEvent => this.EventBuilder.OverriddenEvent;

        // TODO: When an interface is introduced, explicit implementation should appear here.
        public IReadOnlyList<IEvent> ExplicitInterfaceImplementations => this.EventBuilder.ExplicitInterfaceImplementations;

        public EventInfo ToEventInfo() => this.EventBuilder.ToEventInfo();

        IEvent IDeclarationRef<IEvent>.Resolve( CompilationModel compilation ) => (IEvent) this.GetForCompilation( compilation );

        ISymbol IDeclarationRef<IEvent>.GetSymbol( Compilation compilation ) => throw new NotSupportedException();

        public IMethod? GetAccessor( MethodKind methodKind ) => this.GetAccessorImpl( methodKind );

        public IEnumerable<IMethod> Accessors => this.EventBuilder.Accessors;

        IType IHasType.Type => this.Type;
    }
}