﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;

namespace Metalama.Framework.Engine.CodeModel
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
            => new TypeParameterList(
                this,
                this.MethodSymbol.TypeParameters.Select( x => Ref.FromSymbol<ITypeParameter>( x, this.Compilation.RoslynCompilation ) ).ToList() );

        [Memo]
        public IReadOnlyList<IType> TypeArguments => this.MethodSymbol.TypeArguments.Select( t => this.Compilation.Factory.GetIType( t ) ).ToImmutableArray();

        public override DeclarationKind DeclarationKind => DeclarationKind.Method;

        public override bool IsImplicit => this.Symbol.IsImplicitlyDeclared;

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

        [Memo]
        public override bool IsAsync
            => this.MethodSymbol.MetadataToken == 0
                ? this.MethodSymbol.IsAsync
                : this.MethodSymbol.GetAttributes().Any( a => a.AttributeConstructor?.ContainingType.Name == nameof(AsyncStateMachineAttribute) );

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