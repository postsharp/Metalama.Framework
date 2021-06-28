// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Code.Invokers;
using Caravela.Framework.Impl.CodeModel.Invokers;
using Caravela.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class Event : Member, IEvent
    {
        private readonly IEventSymbol _symbol;

        public override ISymbol Symbol => this._symbol;

        public Event( IEventSymbol symbol, CompilationModel compilation ) : base( compilation )
        {
            this._symbol = symbol;
        }

        [Memo]
        public IInvokerFactory<IEventInvoker> Invokers => new InvokerFactory<IEventInvoker>( order => new EventInvoker( this, order ) );

        [Memo]
        public INamedType EventType => (INamedType) this.Compilation.Factory.GetIType( this._symbol.Type );

        public IMethod Signature => this.EventType.Methods.OfName( "Invoke" ).Single();

        [Memo]
        public IMethod Adder => this.Compilation.Factory.GetMethod( this._symbol.AddMethod! );

        [Memo]
        public IMethod Remover => this.Compilation.Factory.GetMethod( this._symbol.RemoveMethod! );

        // TODO: pseudo-accessor
        [Memo]
        public IMethod? Raiser
            => this._symbol.RaiseMethod == null
                ? new PseudoAccessor( this, AccessorSemantic.Raise )
                : this.Compilation.Factory.GetMethod( this._symbol.RaiseMethod );

        [Memo]
        public IReadOnlyList<IEvent> ExplicitInterfaceImplementations
            => this._symbol.ExplicitInterfaceImplementations.Select( e => this.Compilation.Factory.GetEvent( e ) ).ToList();

        public EventInfo ToEventInfo() => new CompileTimeEventInfo( this );

        public override DeclarationKind DeclarationKind => DeclarationKind.Event;

        public override bool IsExplicitInterfaceImplementation => !this._symbol.ExplicitInterfaceImplementations.IsEmpty;

        public override bool IsAsync => false;

        public override MemberInfo ToMemberInfo() => this.ToEventInfo();
    }
}