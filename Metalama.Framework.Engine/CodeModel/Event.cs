﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.CodeModel.Pseudo;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MethodKind = Metalama.Framework.Code.MethodKind;

namespace Metalama.Framework.Engine.CodeModel
{
    internal class Event : Member, IEventImpl
    {
        private readonly IEventSymbol _symbol;

        public override ISymbol Symbol => this._symbol;

        public Event( IEventSymbol symbol, CompilationModel compilation ) : base( compilation )
        {
            this._symbol = symbol;
        }

        [Memo]
        public IInvokerFactory<IEventInvoker> Invokers
            => new InvokerFactory<IEventInvoker>( ( order, invokerOperator ) => new EventInvoker( this, order, invokerOperator ) );

        [Memo]
        public INamedType Type => (INamedType) this.Compilation.Factory.GetIType( this._symbol.Type );

        public IMethod Signature => this.Type.Methods.OfName( "Invoke" ).Single();

        [Memo]
        public IMethod AddMethod => this.Compilation.Factory.GetMethod( this._symbol.AddMethod! );

        [Memo]
        public IMethod RemoveMethod => this.Compilation.Factory.GetMethod( this._symbol.RemoveMethod! );

        // TODO: pseudo-accessor
        [Memo]
        public IMethod? RaiseMethod
            => this._symbol.RaiseMethod == null
                ? new PseudoRaiser( this )
                : this.Compilation.Factory.GetMethod( this._symbol.RaiseMethod );

        public IEvent? OverriddenEvent
        {
            get
            {
                var overriddenEvent = this._symbol.OverriddenEvent;

                if ( overriddenEvent != null )
                {
                    return this.Compilation.Factory.GetEvent( overriddenEvent );
                }
                else
                {
                    return null;
                }
            }
        }

        [Memo]
        public IReadOnlyList<IEvent> ExplicitInterfaceImplementations
            => this._symbol.ExplicitInterfaceImplementations.Select( e => this.Compilation.Factory.GetEvent( e ) ).ToList();

        public EventInfo ToEventInfo() => new CompileTimeEventInfo( this );

        public override DeclarationKind DeclarationKind => DeclarationKind.Event;

        public override bool IsExplicitInterfaceImplementation => !this._symbol.ExplicitInterfaceImplementations.IsEmpty;

        public override bool IsImplicit => throw new NotImplementedException();

        public IMethod? GetAccessor( MethodKind methodKind ) => this.GetAccessorImpl( methodKind );

        public IEnumerable<IMethod> Accessors
        {
            get
            {
                yield return this.AddMethod;
                yield return this.RemoveMethod;

                if ( this.RaiseMethod != null )
                {
                    yield return this.RaiseMethod;
                }
            }
        }

        public override bool IsAsync => false;

        public override MemberInfo ToMemberInfo() => this.ToEventInfo();

        IType IHasType.Type => this.Type;

        public IMember? OverriddenMember => this.OverriddenEvent;
    }
}