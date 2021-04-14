// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.Links;
using Microsoft.CodeAnalysis;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal partial class Method : MethodBase, IMethod
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
        public IGenericParameterList GenericParameters =>
            new GenericParameterList(
                this.MethodSymbol.TypeParameters.Select( tp => CodeElementLink.FromSymbol<IGenericParameter>( tp ) ),
                this.Compilation );

        public override CodeElementKind ElementKind => CodeElementKind.Method;

        [Memo]
        public IReadOnlyList<IType> GenericArguments =>
            this.MethodSymbol.TypeArguments.Select( this.Compilation.Factory.GetIType ).ToImmutableList();

        public bool IsOpenGeneric => this.GenericArguments.Any( ga => ga is IGenericParameter ) || this.DeclaringType.IsOpenGeneric;

        public object Invoke( object? instance, params object[] args ) => new MethodInvocation( this ).Invoke( instance, args );

        public bool HasBase => true;

        public IMethodInvocation Base => new MethodInvocation( this ).Base;

        public IMethod WithGenericArguments( params IType[] genericArguments )
        {
            var symbolWithGenericArguments = this.MethodSymbol.Construct( genericArguments.Select( a => a.GetSymbol() ).ToArray() );

            return new Method( symbolWithGenericArguments, this.Compilation );
        }

        public override bool IsReadOnly => this.MethodSymbol.IsReadOnly;

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
    }
}
