﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.References;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class BuiltEvent : BuiltMember, IEvent, IMemberRef<IEvent>
    {
        public BuiltEvent( EventBuilder builder, CompilationModel compilation ) : base( compilation )
        {
            this.EventBuilder = builder;
        }

        public EventBuilder EventBuilder { get; }

        public override DeclarationBuilder Builder => this.EventBuilder;

        public override MemberOrNamedTypeBuilder MemberOrNamedTypeBuilder => this.EventBuilder;

        public IEventInvocation Base => throw new NotImplementedException();

        public IType EventType => this.EventBuilder.EventType;

        [Memo]
        public IMethod Adder => new BuiltAccessor( this, (AccessorBuilder) this.EventBuilder.Adder );

        [Memo]
        public IMethod Remover => new BuiltAccessor( this, (AccessorBuilder) this.EventBuilder.Remover );

        public IMethod? Raiser => null;

        public bool HasBase => throw new NotImplementedException();

        IEventInvocation IEvent.Base => throw new NotImplementedException();

        // TODO: When an interface is introduced, explicit implementation should appear here.
        public IReadOnlyList<IEvent> ExplicitInterfaceImplementations => Array.Empty<IEvent>();

        [return: RunTimeOnly]
        public EventInfo ToEventInfo()
        {
            throw new NotImplementedException();
        }

        IEvent IDeclarationRef<IEvent>.Resolve( CompilationModel compilation ) => (IEvent) this.GetForCompilation( compilation );

        ISymbol IDeclarationRef<IEvent>.GetSymbol( Compilation compilation ) => throw new NotSupportedException();
    }
}