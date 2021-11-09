// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Code.Invokers;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.Invokers;
using Caravela.Framework.Impl.CodeModel.References;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.ReflectionMocks;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class Method : MethodBase, IMethodImpl
    {
        public Method( IMethodSymbol symbol, CompilationModel compilation ) : base( symbol, compilation )
        {
            if ( symbol.MethodKind == MethodKind.Constructor || symbol.MethodKind == MethodKind.StaticConstructor )
            {
                throw new ArgumentOutOfRangeException( nameof(symbol), "Cannot use the Method class with constructors." );
            }
        }

        [Memo]
        public IParameter ReturnParameter => new MethodReturnParameter( this );

        [Memo]
        public IType ReturnType => this.Compilation.Factory.GetIType( this.MethodSymbol.ReturnType );

        [Memo]
        public IGenericParameterList TypeParameters
            => new GenericParameterList(
                this,
                this.MethodSymbol.TypeParameters.Select( Ref.FromSymbol<ITypeParameter> ) );

        [Memo]
        public IReadOnlyList<IType> TypeArguments => this.MethodSymbol.TypeArguments.Select( t => this.Compilation.Factory.GetIType( t ) ).ToImmutableArray();

        public override DeclarationKind DeclarationKind => DeclarationKind.Method;

        public bool IsOpenGeneric => this.MethodSymbol.TypeArguments.Any( ga => ga is ITypeParameterSymbol ) || this.DeclaringType.IsOpenGeneric;

        public bool IsGeneric => this.MethodSymbol.TypeParameters.Length > 0;

        IGeneric IGenericInternal.ConstructGenericInstance( params IType[] typeArguments )
        {
            if ( this.DeclaringType.IsOpenGeneric )
            {
                throw new InvalidOperationException(
                    UserMessageFormatter.Format(
                        $"Cannot construct a generic instance of this method because the declaring type '{this.DeclaringType}' has unbound type parameters." ) );
            }

            var symbolWithGenericArguments = this.MethodSymbol.Construct( typeArguments.Select( a => a.GetSymbol() ).ToArray() );

            return new Method( symbolWithGenericArguments, this.Compilation );
        }

        [Memo]
        public IInvokerFactory<IMethodInvoker> Invokers
            => new InvokerFactory<IMethodInvoker>( ( order, invokerOperator ) => new MethodInvoker( this, order, invokerOperator ) );

        public bool IsReadOnly => this.MethodSymbol.IsReadOnly;

        public override bool IsExplicitInterfaceImplementation => !this.MethodSymbol.ExplicitInterfaceImplementations.IsEmpty;

        public override bool IsAsync => this.MethodSymbol.IsAsync;

        public IMethod? OverriddenMethod
        {
            get
            {
                var overriddenMethod = this.MethodSymbol.OverriddenMethod;

                if ( overriddenMethod != null )
                {
                    return this.Compilation.Factory.GetMethod( overriddenMethod );
                }
                else
                {
                    return null;
                }
            }
        }

        [Memo]
        public IReadOnlyList<IMethod> ExplicitInterfaceImplementations
            => ((IMethodSymbol) this.Symbol).ExplicitInterfaceImplementations.Select( m => this.Compilation.Factory.GetMethod( m ) ).ToList();

        public MethodInfo ToMethodInfo() => CompileTimeMethodInfo.Create( this );

        public IMemberWithAccessors? DeclaringMember
            => this.MethodSymbol.AssociatedSymbol != null
                ? this.Compilation.Factory.GetDeclaration( this.MethodSymbol.AssociatedSymbol ) as IMemberWithAccessors
                : null;

        public override System.Reflection.MethodBase ToMethodBase() => this.ToMethodInfo();

        public IMember? OverriddenMember => this.OverriddenMethod;
    }
}