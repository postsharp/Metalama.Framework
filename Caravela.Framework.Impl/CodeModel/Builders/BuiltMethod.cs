// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.References;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MethodKind = Caravela.Framework.Code.MethodKind;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class BuiltMethod : BuiltMember, IMethod, IMemberRef<IMethod>
    {
        public BuiltMethod( MethodBuilder builder, CompilationModel compilation ) : base( compilation )
        {
            this.MethodBuilder = builder;
        }

        public MethodBuilder MethodBuilder { get; }

        public override DeclarationBuilder Builder => this.MethodBuilder;

        public override MemberOrNamedTypeBuilder MemberOrNamedTypeBuilder => this.MethodBuilder;

        public IMethodList LocalFunctions => MethodList.Empty;

        [Memo]
        public IParameterList Parameters
            => new ParameterList(
                this,
                this.MethodBuilder.Parameters.AsBuilderList.Select( DeclarationRef.FromBuilder<IParameter, IParameterBuilder> ) );

        public MethodKind MethodKind => this.MethodBuilder.MethodKind;

        // TODO: When an interface is introduced, explicit implementation should appear here.
        public IReadOnlyList<IMethod> ExplicitInterfaceImplementations => Array.Empty<IMethod>();

        public MethodInfo ToMethodInfo() => throw new NotImplementedException();

        System.Reflection.MethodBase IMethodBase.ToMethodBase() => this.ToMethodInfo();

        public dynamic Invoke( dynamic? instance, params dynamic[] args ) => throw new NotImplementedException();

        [Memo]
        public IParameter ReturnParameter => new BuiltParameter( this.MethodBuilder.ReturnParameter, this.Compilation );

        [Memo]
        public IType ReturnType => this.Compilation.Factory.GetIType( this.MethodBuilder.ReturnParameter.ParameterType );

        [Memo]
        public IGenericParameterList GenericParameters
            => new GenericParameterList(
                this,
                this.MethodBuilder.GenericParameters.AsBuilderList.Select( DeclarationRef.FromBuilder<IGenericParameter, GenericParameterBuilder> ) );

        public IReadOnlyList<IType> GenericArguments => throw new NotImplementedException();

        public bool IsOpenGeneric => true;

        public IMethod WithGenericArguments( params IType[] genericArguments ) => throw new NotImplementedException();

        public bool HasBase => throw new NotImplementedException();

        public IMethodInvocation Base => throw new NotImplementedException();

        public IMethod? OverriddenMethod => throw new NotImplementedException();

        IMethod IDeclarationRef<IMethod>.Resolve( CompilationModel compilation ) => (IMethod) this.GetForCompilation( compilation );

        ISymbol IDeclarationRef<IMethod>.GetSymbol( Compilation compilation ) => throw new NotSupportedException();
    }
}