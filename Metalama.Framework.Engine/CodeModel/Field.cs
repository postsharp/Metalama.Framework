// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.CodeModel.Pseudo;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.RunTime;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Reflection;
using Accessibility = Metalama.Framework.Code.Accessibility;
using MethodKind = Metalama.Framework.Code.MethodKind;
using RefKind = Metalama.Framework.Code.RefKind;

namespace Metalama.Framework.Engine.CodeModel
{
    internal sealed class Field : Member, IFieldImpl
    {
        private readonly IFieldSymbol _symbol;

        public override DeclarationKind DeclarationKind => DeclarationKind.Field;

        public override ISymbol Symbol => this._symbol;

        public Field( IFieldSymbol symbol, CompilationModel compilation ) : base( compilation, symbol )
        {
            this._symbol = symbol;
        }

        [Memo]
        public IInvokerFactory<IFieldOrPropertyInvoker> Invokers
            => new InvokerFactory<IFieldOrPropertyInvoker>( ( order, invokerOperator ) => new FieldOrPropertyInvoker( this, order, invokerOperator ) );

        [Memo]
        public IType Type => this.Compilation.Factory.GetIType( this._symbol.Type );

        public RefKind RefKind 
#if ROSLYN_4_4_0_OR_LATER        
        => this._symbol.RefKind.ToOurRefKind();
#else
        => RefKind.None;        
#endif

        [Memo]
        public IMethod? GetMethod => new PseudoGetter( this );

        [Memo]
        public IMethod? SetMethod
            => this.Writeability switch
            {
                Writeability.None => null,
                Writeability.ConstructorOnly => new PseudoSetter( this, Accessibility.Private ),
                Writeability.All => new PseudoSetter( this, null ),
                _ => throw new AssertionFailedException( $"Unexpected Writeability: {this.Writeability}." )
            };

        public Writeability Writeability
            => this._symbol switch
            {
                { IsConst: true } => Writeability.None,
                { IsReadOnly: true } => Writeability.ConstructorOnly,
                _ => Writeability.All
            };

        public bool? IsAutoPropertyOrField => true;

        public FieldOrPropertyInfo ToFieldOrPropertyInfo() => CompileTimeFieldOrPropertyInfo.Create( this );

        public FieldInfo ToFieldInfo() => CompileTimeFieldInfo.Create( this );

        public override bool IsExplicitInterfaceImplementation => false;

        public override bool IsAsync => false;

        public IMember? OverriddenMember => null;

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