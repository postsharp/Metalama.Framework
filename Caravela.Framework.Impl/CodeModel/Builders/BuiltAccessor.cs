﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Code.DeclarationBuilders;
using Caravela.Framework.Code.Invokers;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.Invokers;
using Caravela.Framework.Impl.CodeModel.References;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Accessibility = Caravela.Framework.Code.Accessibility;
using MethodKind = Caravela.Framework.Code.MethodKind;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class BuiltAccessor : BuiltDeclaration, IMethodImpl, IMemberRef<IMethod>
    {
        private readonly BuiltMember _builtMember;

        public BuiltAccessor( BuiltMember builtMember, AccessorBuilder builder ) : base( builtMember.Compilation )
        {
            this._builtMember = builtMember;
            this.AccessorBuilder = builder;
        }

        public AccessorBuilder AccessorBuilder { get; }

        public override DeclarationBuilder Builder => this.AccessorBuilder;

        public Accessibility Accessibility => this.AccessorBuilder.Accessibility;

        public string Name => this.AccessorBuilder.Name;

        public bool IsAbstract => this.AccessorBuilder.IsAbstract;

        public bool IsStatic => this.AccessorBuilder.IsStatic;

        public bool IsVirtual => this.AccessorBuilder.IsVirtual;

        public bool IsSealed => this.AccessorBuilder.IsSealed;

        public bool IsReadOnly => this.AccessorBuilder.IsReadOnly;

        public bool IsOverride => this.AccessorBuilder.IsOverride;

        public bool IsExplicitInterfaceImplementation => this.ExplicitInterfaceImplementations.Count > 0;

        public bool IsNew => this.AccessorBuilder.IsNew;

        public bool IsAsync => this.AccessorBuilder.IsAsync;

        public IMethodList LocalFunctions => MethodList.Empty;

        [Memo]
        public IParameterList Parameters
            => new ParameterList(
                this,
                this.AccessorBuilder.Parameters.AsBuilderList.Select( DeclarationRef.FromBuilder<IParameter, IParameterBuilder> ) );

        public MethodKind MethodKind => this.AccessorBuilder.MethodKind;

        [Memo]
        public IParameter ReturnParameter => new BuiltParameter( this.AccessorBuilder.ReturnParameter, this.Compilation );

        [Memo]
        public IType ReturnType => this.Compilation.Factory.GetIType( this.AccessorBuilder.ReturnParameter.Type );

        [Memo]
        public IGenericParameterList TypeParameters => GenericParameterList.Empty;

        public IReadOnlyList<IType> TypeArguments => this.AccessorBuilder.TypeArguments;

        public bool IsOpenGeneric => this.AccessorBuilder.IsOpenGeneric;

        public bool IsGeneric => this.AccessorBuilder.IsGeneric;

        IGeneric IGenericInternal.ConstructGenericInstance( params IType[] typeArguments ) => this.AccessorBuilder.ConstructGenericInstance( typeArguments );

        [Memo]
        public IInvokerFactory<IMethodInvoker> Invokers
            => new InvokerFactory<IMethodInvoker>( ( order, invokerOperator ) => new MethodInvoker( this, order, invokerOperator ), false );

        public IMethod? OverriddenMethod => this.AccessorBuilder.OverriddenMethod;

        public INamedType DeclaringType => this._builtMember.DeclaringType;

        public object? Target => throw new NotImplementedException();

        IMethod IDeclarationRef<IMethod>.Resolve( CompilationModel compilation ) => (IMethod) this.GetForCompilation( compilation );

        ISymbol? IDeclarationRef<IMethod>.GetSymbol( Compilation compilation ) => this.GetSymbol();

        public IReadOnlyList<IMethod> ExplicitInterfaceImplementations => this.AccessorBuilder.ExplicitInterfaceImplementations;

        public MethodInfo ToMethodInfo() => this.AccessorBuilder.ToMethodInfo();

        IMemberWithAccessors? IMethod.DeclaringMember => (IMemberWithAccessors) this._builtMember;

        public System.Reflection.MethodBase ToMethodBase() => this.AccessorBuilder.ToMethodBase();

        public MemberInfo ToMemberInfo() => this.AccessorBuilder.ToMemberInfo();

        public IMember? OverriddenMember => this.Compilation.Factory.GetDeclaration( this.AccessorBuilder.OverriddenMember );
    }
}