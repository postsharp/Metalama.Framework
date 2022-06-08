// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MethodKind = Metalama.Framework.Code.MethodKind;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal class BuiltMethod : BuiltMember, IMethodImpl, IMemberRef<IMethod>
    {
        public BuiltMethod( MethodBuilder builder, CompilationModel compilation ) : base( compilation )
        {
            this.MethodBuilder = builder;
        }

        public MethodBuilder MethodBuilder { get; }

        public override MemberBuilder MemberBuilder => this.MethodBuilder;

        public override MemberOrNamedTypeBuilder MemberOrNamedTypeBuilder => this.MethodBuilder;

        [Memo]
        public IParameterList Parameters
            => new ParameterList(
                this,
                this.GetCompilationModel().GetParameterCollection( this.MethodBuilder.ToTypedRef<IHasParameters>() ) );

        public MethodKind MethodKind => this.MethodBuilder.MethodKind;

        public bool IsReadOnly => this.MethodBuilder.IsReadOnly;

        // TODO: When an interface is introduced, explicit implementation should appear here.
        public IReadOnlyList<IMethod> ExplicitInterfaceImplementations => this.MethodBuilder.ExplicitInterfaceImplementations;

        public MethodInfo ToMethodInfo() => throw new NotImplementedException();

        IMemberWithAccessors? IMethod.DeclaringMember => null;

        System.Reflection.MethodBase IMethodBase.ToMethodBase() => this.ToMethodInfo();

        [Memo]
        public IParameter ReturnParameter => new BuiltParameter( this.MethodBuilder.ReturnParameter, this.Compilation );

        [Memo]
        public IType ReturnType => this.Compilation.Factory.GetIType( this.MethodBuilder.ReturnParameter.Type );

        [Memo]
        public IGenericParameterList TypeParameters
            => new TypeParameterList(
                this,
                this.MethodBuilder.TypeParameters.AsBuilderList.Select( Ref.FromBuilder<ITypeParameter, TypeParameterBuilder> ).ToList() );

        public IReadOnlyList<IType> TypeArguments => throw new NotImplementedException();

        public bool IsOpenGeneric => this.MethodBuilder.IsOpenGeneric;

        public bool IsGeneric => this.MethodBuilder.IsGeneric;

        IGeneric IGenericInternal.ConstructGenericInstance( params IType[] typeArguments ) => throw new NotImplementedException();

        [Memo]
        public IInvokerFactory<IMethodInvoker> Invokers
            => new InvokerFactory<IMethodInvoker>(
                ( order, invokerOperator ) => new MethodInvoker( this, order, invokerOperator ),
                this.OverriddenMethod != null );

        public IMethod? OverriddenMethod => this.Compilation.Factory.GetDeclaration( this.MethodBuilder.OverriddenMethod );

        DeclarationSerializableId IRef<IMethod>.ToSerializableId() => throw new NotImplementedException();

        IMethod IRef<IMethod>.GetTarget( ICompilation compilation ) => (IMethod) this.GetForCompilation( compilation );

        ISymbol? ISdkRef<IMethod>.GetSymbol( Compilation compilation, bool ignoreAssemblyKey ) => throw new NotSupportedException();
    }
}