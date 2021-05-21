﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis;
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
        public INamedType EventType => this.Compilation.Factory.GetNamedType( (INamedTypeSymbol) this._symbol.Type );

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

        public EventInfo ToEventInfo() => new CompileTimeEventInfo( this._symbol, this.DeclaringType );

        public override DeclarationKind DeclarationKind => DeclarationKind.Event;

        public override bool IsReadOnly => false;

        public override bool IsAsync => false;

        public override MemberInfo ToMemberInfo() => this.ToEventInfo();
    }
}