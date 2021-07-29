// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.Invokers;
using Caravela.Framework.Impl.CodeModel.Invokers;
using Caravela.Framework.Impl.CodeModel.References;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MethodKind = Caravela.Framework.Code.MethodKind;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class BuiltEvent : BuiltMember, IEventInternal, IMemberRef<IEvent>
    {
        public BuiltEvent( EventBuilder builder, CompilationModel compilation ) : base( compilation )
        {
            this.EventBuilder = builder;
        }

        public EventBuilder EventBuilder { get; }

        public override MemberBuilder MemberBuilder => this.EventBuilder;

        public override MemberOrNamedTypeBuilder MemberOrNamedTypeBuilder => this.EventBuilder;

        public INamedType EventType => this.EventBuilder.EventType;

        public IMethod Signature => this.EventType.Methods.OfName( "Invoke" ).Single();

        [Memo]
        public IMethod Adder => new BuiltAccessor( this, (AccessorBuilder) this.EventBuilder.Adder );

        [Memo]
        public IMethod Remover => new BuiltAccessor( this, (AccessorBuilder) this.EventBuilder.Remover );

        public IMethod? Raiser => null;

        [Memo]
        public IInvokerFactory<IEventInvoker> Invokers
            => new InvokerFactory<IEventInvoker>( ( order, invokerOperator ) => new EventInvoker( this, order, invokerOperator ), false );

        public IEvent? OverriddenEvent => this.EventBuilder.OverriddenEvent;

        // TODO: When an interface is introduced, explicit implementation should appear here.
        public IReadOnlyList<IEvent> ExplicitInterfaceImplementations => Array.Empty<IEvent>();

        [return: RunTimeOnly]
        public EventInfo ToEventInfo() => throw new NotImplementedException();

        IEvent IDeclarationRef<IEvent>.Resolve( CompilationModel compilation ) => (IEvent) this.GetForCompilation( compilation );

        ISymbol IDeclarationRef<IEvent>.GetSymbol( Compilation compilation ) => throw new NotSupportedException();

        public IMethod? GetAccessor( MethodKind methodKind ) => this.GetAccessorImpl( methodKind );
    }
}