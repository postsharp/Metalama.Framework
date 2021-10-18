// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Invokers;
using Caravela.Framework.Impl.CodeModel.Invokers;
using Caravela.Framework.Impl.CodeModel.Pseudo;
using Caravela.Framework.Impl.ReflectionMocks;
using Caravela.Framework.Impl.Utilities;
using Caravela.Framework.RunTime;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Reflection;
using MethodKind = Caravela.Framework.Code.MethodKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal sealed class Field : Member, IFieldImpl
    {
        private readonly IFieldSymbol _symbol;

        public override DeclarationKind DeclarationKind => DeclarationKind.Field;

        public override ISymbol Symbol => this._symbol;

        public Field( IFieldSymbol symbol, CompilationModel compilation ) : base( compilation )
        {
            this._symbol = symbol;
        }

        [Memo]
        public IInvokerFactory<IFieldOrPropertyInvoker> Invokers
            => new InvokerFactory<IFieldOrPropertyInvoker>( ( order, invokerOperator ) => new FieldOrPropertyInvoker( this, order, invokerOperator ) );

        [Memo]
        public IType Type => this.Compilation.Factory.GetIType( this._symbol.Type );

        [Memo]
        public IMethod? GetMethod => new PseudoGetter( this );

        [Memo]
        public IMethod? SetMethod => this.Writeability != Writeability.None ? new PseudoSetter( this ) : null;

        public Writeability Writeability
            => this._symbol switch
            {
                { IsConst: true } => Writeability.None,
                { IsReadOnly: true } => Writeability.ConstructorOnly,
                _ => Writeability.All
            };

        public bool IsAutoPropertyOrField => true;

        public FieldOrPropertyInfo ToFieldOrPropertyInfo() => CompileTimeFieldOrPropertyInfo.Create( this );

        public override bool IsExplicitInterfaceImplementation => false;

        public override bool IsAsync => false;

        public override MemberInfo ToMemberInfo() => this.ToFieldOrPropertyInfo();

        public IMethod? GetAccessor( MethodKind methodKind )
            => methodKind switch
            {
                MethodKind.PropertyGet => this.GetMethod,
                MethodKind.PropertySet => this.SetMethod,
                _ => throw new ArgumentOutOfRangeException()
            };

        public IEnumerable<IMethod> Accessors
        {
            get
            {
                if ( this.GetMethod != null )
                {
                    yield return this.GetMethod;
                }

                if ( this.SetMethod != null )
                {
                    yield return this.SetMethod;
                }
            }
        }
    }
}